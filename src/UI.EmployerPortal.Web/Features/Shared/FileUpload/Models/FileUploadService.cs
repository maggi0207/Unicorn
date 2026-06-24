namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
/// <summary>
/// Call Wcf Service
/// </summary>
public class FileUploadService
{
    /// <summary>
    /// RuleId
    /// </summary>
    public string RuleId { get; set; } = string.Empty;
    /// <summary>
    /// RuleViolation
    /// </summary>
    public string RuleViolation { get; set; } = string.Empty;
    /// <summary>
    /// EmployerSK
    /// </summary>
    public string EmployerSK { get; init; } = string.Empty;
    /// <summary>
    /// WebUserFileUploadSK
    /// </summary>
    public string WebUserFileUploadSK { get; init; } = string.Empty;
    /// <summary>
    /// Message
    /// </summary>
    public string Message { get; set; } = string.Empty;
    /// <summary>
    /// SurveyNumber
    /// </summary>
    public string SurveyNumber { get; set; } = string.Empty;
    /// <summary>
    /// SurveyResponseSK
    /// </summary>
    public string SurveyResponseSK { get; set; } = string.Empty;
}

