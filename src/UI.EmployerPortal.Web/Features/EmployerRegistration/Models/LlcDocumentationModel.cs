namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Data for LLC Corporation documentation section
/// </summary>
public class LlcDocumentationModel
{
    /// <summary>
    /// Does the registrant have the required documentation available to upload?
    /// </summary>
    public bool? HasRequiredDocumentation { get; set; }

    /// <summary>
    /// Uploaded file name (for display/tracking purposes)
    /// </summary>
    public string? UploadedFileName { get; set; }

    /// <summary>
    /// Uploaded file content (base64 or byte reference, depending on backend integration)
    /// </summary>
    public byte[]? UploadedFileContent { get; set; }

    /// <summary>
    /// Uploaded file content type
    /// </summary>
    public string? UploadedFileContentType { get; set; }

    /// <summary>
    /// UploadedFileName
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// NoDocumentReason
    /// </summary>
    public NoDocReason? NoDocumentationReason { get; set; }

    /// <summary>
    /// When do you plan to submit your application to the IRS?
    /// </summary>
    public DateOnly? PlannedSubmissionDate { get; set; }

    /// <summary>
    /// What date was the application submitted to the IRS?
    /// </summary>
    public DateOnly? ApplicationSubmittedDate { get; set; }

    /// <summary>
    /// Acknowledge Submit Documentation
    /// </summary>
    public bool AcknowledgeSubmitDocumentation { get; set; }

    /// <summary>
    /// User has indicated they will supply the required documentation at a later date
    /// </summary>
    public bool WillSupplyDocumentationLater { get; set; }
}
