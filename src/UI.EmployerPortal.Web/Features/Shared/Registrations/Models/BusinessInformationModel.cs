using System.ComponentModel.DataAnnotations;
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
    /// Federal Employer Identification Number in format 99-9999999.
    /// </summary>
    [Required(ErrorMessage = "FEIN is required.")]
    [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "FEIN must be in format 99-9999999.")]
    public string? FEIN { get; set; }

    /// <summary>
    /// Legal business name as registered with the state.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Trade name or DBA (optional).
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Business contact email address.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
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
    /// Physical business locations. At least one is required; maximum of three allowed.
    /// </summary>
    public List<AddressModel> PhysicalLocations { get; set; } = new()
    {
        new AddressModel()
    };

    #endregion

    /// <summary>
    /// Returns survey responses for all populated business information fields.
    /// </summary>
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

        if (!string.IsNullOrWhiteSpace(Email))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_EMAIL_ADR, _response = Email });
        }

        if (!string.IsNullOrWhiteSpace(FEIN))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FEIN_NUM, _response = FEIN });
        }

        return responses;
    }
}
