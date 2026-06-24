using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Model Used for storing the employer registration continue values in session.
/// </summary>
public class ContinueEmployerRegistrationModel : ISessionModel
{
    /// <summary>
    /// The Survey Number returned by the WCF service on save and exit.
    /// </summary>
    public string SurveyNumber { get; set; } = string.Empty;

    /// <summary>
    /// The FEIN of the employer.
    /// </summary>
    public string FEIN { get; set; } = string.Empty;
}
