namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

/// <summary>
/// Represents the result of a file upload WCF service call,
/// containing identifiers, status messages, and any rule violations.
/// </summary>
public class FileUploadService
{
    /// <summary>
    /// The rule identifier returned by the upload service validation.
    /// </summary>
    public string RuleId { get; set; } = string.Empty;

    /// <summary>
    /// Description of the rule violation, if any.
    /// </summary>
    public string RuleViolation { get; set; } = string.Empty;

    /// <summary>
    /// The employer surrogate key assigned by the service.
    /// </summary>
    public string EmployerSK { get; init; } = string.Empty;

    /// <summary>
    /// The web user file upload surrogate key assigned by the service.
    /// </summary>
    public string WebUserFileUploadSK { get; init; } = string.Empty;

    /// <summary>
    /// General status message returned by the service.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// The survey number associated with this upload.
    /// </summary>
    public string SurveyNumber { get; set; } = string.Empty;

    /// <summary>
    /// The survey response surrogate key associated with this upload.
    /// </summary>
    public string SurveyResponseSK { get; set; } = string.Empty;
}
