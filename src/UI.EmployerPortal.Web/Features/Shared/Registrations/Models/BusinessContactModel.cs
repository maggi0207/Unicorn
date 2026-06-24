using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

/// <summary>
/// Model for the Business Contact step of the employer registration wizard.
/// </summary>
public class BusinessContactModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Contact's first name.
    /// </summary>
    [Required(ErrorMessage = "First Name is required")]
    [MaxLength(64, ErrorMessage = "First Name cannot exceed 64 characters")]
    public string? FirstName { get; set; }

    /// <summary>
    /// Contact's last name.
    /// </summary>
    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(64, ErrorMessage = "Last Name cannot exceed 64 characters")]
    public string? LastName { get; set; }

    /// <summary>
    /// Contact's job title (optional).
    /// </summary>
    [MaxLength(64, ErrorMessage = "Title cannot exceed 64 characters")]
    public string? Title { get; set; }

    /// <summary>
    /// Indicates whether the business contact address differs from the mailing address.
    /// Null means the user has not yet made a selection.
    /// </summary>
    [Required(ErrorMessage = "Please select an option")]
    public bool? IsDifferentAddress { get; set; }
    /// <summary>
    /// Employer has Selected the option or not
    /// </summary>

    /// <summary>
    /// Business contact address, collected only when <see cref="IsDifferentAddress"/> is true.
    /// </summary>
    public AddressModel ContactAddress { get; set; } = new();

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        var addresses = new List<Tuple<RegistrationAddressCode, AddressModel>>();

        if (IsDifferentAddress ?? false)
        {
            addresses.Add(Tuple.Create(RegistrationAddressCode.Contact, ContactAddress));
        }

        return addresses;
    }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses)
    {
        if (IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.Contact, out var contactAddress))
        {
            ContactAddress = IEmployerRegistrationModelSection.ConvertAddressResponseToModel(contactAddress);
        }
    }

    /// <inheritdoc/>
    public void PutAddressSKs(RegistrationAddressProxy[] addresses)
    {
        if (ContactAddress != null
            && IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.Contact, out var contactAddress))
        {
            ContactAddress.RegistrationAddressSk = contactAddress.EmployerRegistrationAddressSK;
        }
    }

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts) { }

    /// <summary>
    /// GetSurveyResponses
    /// </summary>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (!string.IsNullOrWhiteSpace(FirstName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CNTC_FST_NAM, _response = FirstName });
        }
        if (!string.IsNullOrWhiteSpace(LastName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CNTC_LAST_NAM, _response = LastName });
        }
        if (!string.IsNullOrWhiteSpace(Title))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CNTC_TTL, _response = Title });
        }
        if (IsDifferentAddress.HasValue)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.CNTC_ADR_DIFF,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsDifferentAddress.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsDifferentAddress.Value)
            });
        }
        return responses;
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CNTC_FST_NAM, out var contactFirstName))
        {
            FirstName = contactFirstName.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CNTC_LAST_NAM, out var contactLastName))
        {
            LastName = contactLastName.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CNTC_TTL, out var contactTitle))
        {
            Title = contactTitle.ReplyText;
        }

        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CNTC_ADR_DIFF, out var addressDiffers))
        {
            IsDifferentAddress = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(addressDiffers.ReplyText);
        }
    }
}
