using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;



/// <summary>
/// Model for Step 3 (Business Information) of the employer registration wizard.
/// Contains business details, mailing address, and physical location(s).
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

    /// <summary>
    /// MailingPhoneNum
    /// </summary>
    public string? MailingPhoneNum { get; set; }
    /// <summary>
    /// MailingPhoneNumExt
    /// </summary>
    public string? MailingPhoneNumExt { get; set; }
    /// <summary>
    /// MailingPhoneNumAreaCode
    /// </summary>
    public string? MailingPhoneNumAreaCode { get; set; }
    /// <summary>
    /// MailingPhoneNumIntCode
    /// </summary>
    public string? MailingPhoneNumIntCode { get; set; }
    #endregion

    #region Mailing Address

    /// <summary>
    /// Business mailing address.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    #endregion

    #region Physical Locations

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
            MailingAddress = IEmployerRegistrationModelSection.ConvertAddressResponseToModel(mainBusinessMailing);
            MailingAddress.PhoneNumber = MailingPhoneNumIntCode ?? MailingPhoneNum;
            MailingAddress.PhoneExtension = MailingPhoneNumExt;
            MailingAddress.PhoneCountryCode = MailingPhoneNumAreaCode;


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
        //Added Phone Number
        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_NUM, _response = MailingAddress.PhoneNumber });
        }
        //Added Phone Number Extension
        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneExtension))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_EXTN_NUM, _response = MailingAddress.PhoneExtension });
        }
        else
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_EXTN_NUM, _response = string.Empty });
        }

        //Added Phone Number areacode
        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneCountryCode))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_PHN_AREA_CD, _response = MailingAddress.PhoneCountryCode });

        }
        //Added Phone Number areacode
        if (!string.IsNullOrWhiteSpace(MailingAddress.PhoneCountryCode) && !string.IsNullOrWhiteSpace(MailingAddress.PhoneNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_INT_PHN_CD, _response = MailingAddress.PhoneNumber });
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
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_NUM, out var phonenum))
        {
            MailingPhoneNum = phonenum.ReplyText;
        }
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_EXTN_NUM, out var phonenumext))
        {
            MailingPhoneNumExt = phonenumext.ReplyText;
        }
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_PHN_AREA_CD, out var phonenumareacode))
        {
            MailingPhoneNumAreaCode = phonenumareacode.ReplyText;
        }
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_INT_PHN_CD, out var internationalphone))
        {
            MailingPhoneNumIntCode = internationalphone.ReplyText;
        }
    }
}
