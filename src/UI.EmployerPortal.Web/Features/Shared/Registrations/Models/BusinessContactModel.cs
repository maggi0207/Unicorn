using System.ComponentModel.DataAnnotations;
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
    public string? FirstName { get; set; }

    /// <summary>
    /// Contact's last name.
    /// </summary>
    [Required(ErrorMessage = "Last Name is required")]
    public string? LastName { get; set; }

    /// <summary>
    /// Contact's job title (optional).
    /// </summary>
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
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

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
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CNTC_ADR_DIFF, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsDifferentAddress.Value) });
        }
        return responses;
    }

}
