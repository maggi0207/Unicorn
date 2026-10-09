using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Mode lfro the wage file upload step
/// </summary>
public class WageFileUploadModel
{
    /// <summary>
    /// The uploaded file path on the server.
    /// </summary>
    public string? UploadedFilePath { get; set; }

    /// <summary>
    /// The display name of the uploaded file.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Contents of an uploaded file that has NOT been written to disk yet.
    /// </summary>
    public byte[]? PendingBytes { get; set; }

    /// <summary>
    /// The wage report type label 
    /// </summary>
    public string WageReportType { get; set; } = "Original Wage Report";

    /// <summary>
    /// Whether the file was uploaded successfully.
    /// </summary>
    public bool IsUploaded { get; set; }

    /// <summary>
    /// Wage file status code
    /// </summary>
    public WageFileStatusCode WageFileStatusCode { get; set; }

    /// <summary>
    /// Whether the file has errors that must be fixed before proceeding. 
    /// </summary>
    public bool HasBlockingErrors { get; set; }

    /// <summary>
    /// Whether the file has warnings that the user can proceed past.
    /// </summary>
    public bool HasWarnings { get; set; }

    /// <summary>
    /// File type code sk returned by validation endpoint 
    /// </summary>
    public int FileUploadFormatCodeSK { get; set; }

    /// <summary>
    /// number of records in file
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// List of file processing errors/warnings
    /// </summary>
    public List<FileValidationMessage> ValidationMessages { get; set; } = [];
}
