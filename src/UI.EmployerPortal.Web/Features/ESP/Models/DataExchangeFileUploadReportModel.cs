using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Root model for the Data Exchange File Upload wizard.
/// </summary>
public class DataExchangeFileUploadReportModel
{
    /// <summary>
    /// contact information
    /// </summary>
    public ContactModel ContactData { get; set; } = new();
    /// <summary>
    /// tax file data
    /// </summary>
    public DataExchangeFileUploadModel DataExchangeFileData { get; set; } = new();
}

/// <summary>
/// Holds state for the tax file being uploaded.
/// </summary>
public class DataExchangeFileUploadModel
{
    /// <summary>
    /// server side path to stage the files
    /// </summary>
    public string? UploadedFilePath { get; set; }
    /// <summary>
    /// name of the file
    /// </summary>
    public string? FileName { get; set; }
    /// <summary>
    /// whether the file being uploaded or not
    /// </summary>
    public bool IsUploaded { get; set; }
    /// <summary>
    /// error violations
    /// </summary>
    public bool HasBlockingErrors { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool HasWarnings { get; set; }

    /// <summary>
    /// Get number of records in file.
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// contains warnings
    /// </summary>
    public List<FileValidationMessage> ValidationMessages { get; set; } = [];
}
