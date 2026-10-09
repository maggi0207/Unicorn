using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents the details of a tax report frile upload.
/// </summary>
public class TaxReportFileUploadDetailModel
{
    /// <summary>
    /// Gets or sets the file upload surrogate key.
    /// </summary>
    public long FileUploadDetailSK { get; set; }

    /// <summary>
    /// Gets or sets the uploaded file name.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the upload date and time
    /// </summary>
    public DateTime? UploadDate { get; set; }

    /// <summary>
    /// Gets or sets the confirmation number
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the contact name.
    /// </summary>
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file size in KB.
    /// </summary>
    public long FileSizeKB { get; set; }

    /// <summary>
    /// Gets or sets the file format type.
    /// </summary>
    public string FileFormatType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of reports contained in the file.
    /// </summary>
    public int ReportsInFile { get; set; }

    /// <summary>
    /// Gets or sets the number of reports without errors
    /// </summary>
    public int ReportsWithoutErrors { get; set; }

    /// <summary>
    /// Gets or sets the number of reports with errors
    /// </summary>
    public int ReportsWithErrors { get; set; }

    /// <summary>
    /// Gets or sets the payment amount for reports without errors.
    /// </summary>
    public decimal PaymentAmountInFile { get; set; }

    /// <summary>
    /// Gets or sets the payment amount for reports without errors.
    /// </summary>
    public decimal PaymentAmountWithoutErrors { get; set; }

    /// <summary>
    /// Gets or sets the payment amount for reports with errors.
    /// </summary>
    public decimal PaymentAmountWithErrors { get; set; }

    /// <summary>
    /// Gets or sets the current file status.
    /// </summary>
    public string FileStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the status date and time.
    /// </summary>
    public DateTime? StatusDate { get; set; }

    /// <summary>
    /// Gets or sets the file status code.
    /// </summary>
    public TaxFileStatusCode TaxFileStatusCode { get; set; }

    /// <summary>
    /// Gets or sets the upload type(HTTP/FTP).
    /// </summary>
    public string UploadType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the collection of file summary errors.
    /// </summary>
    public List<TaxReportFileSummaryModel> FileSummary { get; set; } = [];

    /// <summary>
    /// Gets or sets the collection of detailed error records.
    /// </summary>
    public List<TaxReportErrorDetailModel> ErrorDetails { get; set; } = [];



}


