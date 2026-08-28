using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;



/// <summary>
/// Model for Step 3 (Business Information) of the employer registration wizard.
/// Contains business details, mailing address and physical location(s).
/// </summary>
public class BusinessInformationModel : IEmployerRegistrationModelSection
{
    #region Business Details


    /// <summary>
    /// Legal business name as registered with the state.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required")]
    [MaxLength(64, ErrorMessage = "Legal Name cannot exceed 64 characters")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Trade name or DBA (optional).
    /// </summary>
    [MaxLength(64, ErrorMessage = "Trade Name cannot exceed 64 characters")]
    public string? TradeName { get; set; }

    /// <summary>
    /// Business contact email address.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required")]
    [EmailAddress(ErrorMessage = "Email Address is not in a valid format")]
    [MaxLength(255, ErrorMessage = "Email Address cannot exceed 255 characters")]
    public string? Email { get; set; }

    #endregion

    #region Mailing Address

    /// <summary>
    /// Business mailing address.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    #endregion

    #region Physical Locations

    /// <summary>
    /// Indicates whether the physical location address differs from the business mailing address.
    /// Maps to QuestionSetItemSK 3170 (PHYS_LOC_ADR_DIFF).
    /// Null means the user has not yet made a selection.
    /// </summary>
    public bool? IsPhysicalLocationDifferent { get; set; }

    /// <summary>
    /// Physical business locations. At least one is required; maximum of three allowed.
    /// </summary>
    public List<AddressModel> PhysicalLocations { get; set; } = new()
    {
        new AddressModel()
    };
    #endregion

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts) { }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        var addresses = new List<Tuple<RegistrationAddressCode, AddressModel>>();

        if (!string.IsNullOrWhiteSpace(MailingAddress.AddressLine1))
        {
            addresses.Add(Tuple.Create(RegistrationAddressCode.Main_Business_Mailing, MailingAddress));
        }

        addresses.AddRange(PhysicalLocations.Where(pl =>
        {
            return !string.IsNullOrWhiteSpace(pl.AddressLine1);
        }).Select(pl =>
        {
            return Tuple.Create(RegistrationAddressCode.Physical_Location, pl);
        }));

        return addresses;
    }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses)
    {
        if (IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.Main_Business_Mailing, out var mainBusinessMailing))
        {
            // Save phone fields populated by LoadSurveyResponses since ConvertAddressResponseToModel creates a new AddressModel
            var tempPhone = MailingAddress.PhoneNumber;
            var tempCountryCode = MailingAddress.PhoneCountryCode;
            var tempExtension = MailingAddress.PhoneExtension;

            MailingAddress = IEmployerRegistrationModelSection.ConvertAddressResponseToModel(mainBusinessMailing);

            MailingAddress.PhoneNumber = tempPhone;
            MailingAddress.PhoneCountryCode = tempCountryCode;
            MailingAddress.PhoneExtension = tempExtension;
        }

        if (IEmployerRegistrationModelSection.FindAddressesHelper(addresses, RegistrationAddressCode.Physical_Location, out var physicalLocations))
        {
            PhysicalLocations = physicalLocations.Select(IEmployerRegistrationModelSection.ConvertAddressResponseToModel).ToList();
        }
    }

    /// <inheritdoc/>
    public void PutAddressSKs(RegistrationAddressProxy[] addresses)
    {
        if (MailingAddress != null
            && IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.Main_Business_Mailing, out var mainBusinessMailing))
        {
            MailingAddress.RegistrationAddressSk = mainBusinessMailing.EmployerRegistrationAddressSK;
        }

        if (PhysicalLocations != null
            && IEmployerRegistrationModelSection.FindAddressesHelper(addresses, RegistrationAddressCode.Physical_Location, out var physicalLocations))
        {
            foreach (var location in PhysicalLocations)
            {
                var match = physicalLocations.FirstOrDefault(l =>
                {
                    return string.Equals(l.LineTwoAddress, location.AddressLine1, StringComparison.OrdinalIgnoreCase);
                });

                if (match != null)
                {
                    location.RegistrationAddressSk = match.EmployerRegistrationAddressSK;
                }
            }
        }
    }

    /// <inheritdoc/>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (!string.IsNullOrWhiteSpace(LegalName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.BUS_LGL_NAM, _response = LegalName });
        }

        if (!string.IsNullOrWhiteSpace(TradeName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.TRD_NAM, _response = TradeName });
        }

        // responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EMAIL_NOTIFY, _response = "Yes" });

        if (!string.IsNullOrWhiteSpace(Email))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_EMAIL_ADR, _response = Email });
        }
        // ── Phone number fields (same 3/7 split as AccountDetailsService) ─────
        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneNumber))
        {
            var phoneDigits = new string(MailingAddress.PhoneNumber.Where(char.IsDigit).ToArray());
            if (phoneDigits.Length == 10)
            {
                var areaCode = phoneDigits[..3];
                var localNumber = phoneDigits[3..];
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_AREA_CD, _response = areaCode });
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_NUM, _response = localNumber });
            }
            else
            {
                // International or non-standard length — send the raw digits as the number
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_NUM, _response = phoneDigits });
            }
        }

        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneCountryCode))
        {
            responses.Add(new SurveyResponse() 
            { 
                _surveyResponseItemSk = (int) SurveyResponseItem.ER_INT_PHN_CD, 
                _response = MailingAddress.PhoneCountryCode.TrimStart('+'),
                _responseDisplay = MailingAddress.PhoneCountryCode 
            });
        }

        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneExtension))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_EXTN_NUM, _response = MailingAddress.PhoneExtension });
        }

        if (IsPhysicalLocationDifferent.HasValue)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.PHYS_LOC_ADR_DIFF,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsPhysicalLocationDifferent.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsPhysicalLocationDifferent.Value)
            });
        }

        return responses;
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BUS_LGL_NAM, out var businessLegalName))
        {
            LegalName = businessLegalName.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.TRD_NAM, out var tradeName))
        {
            TradeName = tradeName.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_EMAIL_ADR, out var emailAddress))
        {
            Email = emailAddress.ReplyText;
        }
        // ── Restore phone number fields ──────────────────────────────────────
        string? loadedAreaCode = null;
        string? loadedNumber = null;

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_AREA_CD, out var phoneAreaCode))
        {
            loadedAreaCode = phoneAreaCode.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_NUM, out var phoneNumber))
        {
            loadedNumber = phoneNumber.ReplyText;
        }

        if (!string.IsNullOrWhiteSpace(loadedAreaCode) && !string.IsNullOrWhiteSpace(loadedNumber))
        {
            // Reconstruct formatted phone: 999-999-9999
            var fullDigits = loadedAreaCode + loadedNumber;
            MailingAddress.PhoneNumber = fullDigits.Length == 10
                ? $"{fullDigits[..3]}-{fullDigits[3..6]}-{fullDigits[6..]}"
                : fullDigits;
        }
        else if (!string.IsNullOrWhiteSpace(loadedNumber))
        {
            MailingAddress.PhoneNumber = loadedNumber;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_INT_PHN_CD, out var intPhoneCode))
        {
            MailingAddress.PhoneCountryCode = $"+{intPhoneCode.ReplyText.TrimStart('+')}";
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_EXTN_NUM, out var phoneExtension))
        {
            MailingAddress.PhoneExtension = phoneExtension.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PHYS_LOC_ADR_DIFF, out var physLocDiffers))
        {
            IsPhysicalLocationDifferent = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(physLocDiffers.ReplyText);
        }
    }
}
