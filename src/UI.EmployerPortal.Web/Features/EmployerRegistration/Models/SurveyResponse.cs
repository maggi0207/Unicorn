namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents a single survey response item to be submitted during employer registration.
/// </summary>
public class SurveyResponse
{
    /// <summary>
    /// The survey response item surrogate key, corresponding to a <see cref="SurveyResponseItem"/> value.
    /// </summary>
    public int _surveyResponseItemSk { get; set; }

    /// <summary>
    /// The string value of the response.
    /// </summary>
    public string? _response { get; set; }
}
