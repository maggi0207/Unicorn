using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Business Activity form model for the employer registration workflow.
/// </summary>
public class BusinessActivityModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Gets or sets the date the business started or was acquired.
    /// </summary>
    [Required(ErrorMessage = "Date business started or acquired is required.")]
    public DateTime? DateBusinessStarted { get; set; }

    /// <summary>
    /// Gets or sets the date the employer first had paid employees working in Wisconsin.
    /// </summary>
    [Required(ErrorMessage = "Date you first had paid employees working in WI is required.")]
    public DateTime? DateFirstPaidEmployeesInWI { get; set; }

    /// <summary>
    /// Gets or sets the date the employer first paid wages for work performed in Wisconsin.
    /// </summary>
    [Required(ErrorMessage = "Date first paid wages for work performed in WI is required.")]
    public DateTime? DateFirstPaidWagesInWI { get; set; }

    /// <summary>
    /// Gets or sets the principal business activity type from the legacy system catalog.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Principal Business Activity is required.")]
    public PrincipalBusinessActivityType PrincipalBusinessActivity { get; set; } = PrincipalBusinessActivityType.None;

    /// <summary>
    /// Gets or sets the description of the employer's primary business activity.
    /// </summary>
    [Required(ErrorMessage = "Primary business activity description is required.")]
    public string? PrimaryBusinessActivityDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Wisconsin-specific business activity is the same as the primary business activity.
    /// </summary>
    public bool SameAsPrimaryBusinessActivity { get; set; }

    /// <summary>
    /// Gets or sets the description of the employer's Wisconsin-specific business activity.
    /// </summary>
    public string? WisconsinSpecificBusinessActivity { get; set; }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        return new();
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

        if (DateBusinessStarted.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.BUS_ACQ_DT, _response = DateBusinessStarted.Value.ToString("MM/dd/yyyy") });
        }
        if (DateFirstPaidEmployeesInWI.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FST_EMPL_DT, _response = DateFirstPaidEmployeesInWI.Value.ToString("MM/dd/yyyy") });
        }
        if (DateFirstPaidWagesInWI.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FST_PYRL_DT, _response = DateFirstPaidWagesInWI.Value.ToString("MM/dd/yyyy") });
        }

        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.RGST_PRIN_ACTV_CD_SK, _response = $"{(int) PrincipalBusinessActivity}" });
        if (!string.IsNullOrWhiteSpace(PrimaryBusinessActivityDescription))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRIN_SALE_DSC, _response = PrimaryBusinessActivityDescription });
        }

        if (!string.IsNullOrWhiteSpace(WisconsinSpecificBusinessActivity))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRIN_OTHR_ACTV_DSC, _response = WisconsinSpecificBusinessActivity });
        }

        return responses;

    }
}
