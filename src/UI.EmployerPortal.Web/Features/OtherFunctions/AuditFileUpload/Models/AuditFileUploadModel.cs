namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

/// <summary>
/// Page-level model backing the Audit File Upload flow. Holds the contact information,
/// the file that was selected/uploaded, and the confirmation result once submitted.
/// </summary>
public class AuditFileUploadModel
{
    /// <summary>
    /// Contact information for the business and file upload contact.
    /// </summary>
    public AuditContactModel Contact { get; set; } = new();

    /// <summary>
    /// Server-side path of the uploaded file (GUID-appended for uniqueness).
    /// </summary>
    public string? UploadedFilePath { get; set; }

    /// <summary>
    /// Original file name as chosen by the user (for display on the confirmation page).
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// True once a file has been successfully attached.
    /// </summary>
    public bool IsFileUploaded { get; set; }
}
