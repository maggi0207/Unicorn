namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents a section of the employer registration wizard that can produce
/// survey responses for submission to the back-end service.
/// </summary>
public interface IEmployerRegistrationModelSection
{
    /// <summary>
    /// Returns the list of survey responses collected by this registration section.
    /// </summary>
    List<SurveyResponse> GetSurveyResponses();
}
