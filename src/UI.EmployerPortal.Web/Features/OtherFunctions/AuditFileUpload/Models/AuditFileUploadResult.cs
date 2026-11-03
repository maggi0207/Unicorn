namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

/// <summary>
/// Outcome of submitting an audit file upload.
/// </summary>
public class AuditFileUploadResult
{
    /// <summary>
    /// Confirmation number returned when the upload succeeds.
    /// </summary>
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Original (display) name of the uploaded file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Date and time the file was received.
    /// </summary>
    public DateTime UploadDateTime { get; set; }

    /// <summary>
    /// Rule violations / error messages returned by the service when the upload could not be completed.
    /// </summary>
    public IReadOnlyList<string> RuleViolations { get; set; } = Array.Empty<string>();

    /// <summary>
    /// True when a confirmation number was returned.
    /// </summary>
    public bool IsSuccess => !string.IsNullOrEmpty(ConfirmationNumber);
}
