using System.Linq.Expressions;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
namespace UI.EmployerPortal.Web.Features.OtherFunctions.Models;
/// <summary>
/// Week Date Range Model
/// </summary>
public class DownloadTransactionModel
{
    /// <summary>
    /// </summary>
    public List<SelectOption> QuarterOptions { get; set; } = new();
    /// <summary>
    /// </summary>
    public BenefitWeekDateRangeModel BenefitDateRangeModel { get; set; } = new();
    /// <summary>
    /// </summary>
    public string? SelectedDateRangeOption { get; set; }
    /// <summary>
    /// </summary>
    public string? FileTypeSelected { get; set; }
    /// <summary>
    /// </summary>
    public Expression<Func<string>>? ForFileType { get; set; }
    /// <summary>
    /// </summary>
    public DownloadReportRequestModel DownloadReportRequest { get; set; } = new();
    /// <summary>
    /// </summary>
    public bool IsSSNSelected { get; set; }
    /// <summary>
    /// </summary>
    public string SSN { get; set; } = string.Empty;
    /// <summary>
    /// File Type validation message
    /// </summary>
    public string? FileTypeError { get; set; }
    /// <summary>
    /// Date Range Option validation message
    /// </summary>
    public string? DateRangeError { get; set; }
    /// <summary>
    /// From date required/invalid validation message
    /// </summary>
    public string? FromDateError { get; set; }
    /// <summary>
    /// To date required/invalid validation message
    /// </summary>
    public string? ToDateError { get; set; }
    /// <summary>
    /// Cross-field custom date range (From/To ordering, bounds) validation message
    /// </summary>
    public string? CustomDateRangeError { get; set; }
    /// <summary>
    /// SSN validation message
    /// </summary>
    public string? SSNError { get; set; }
    /// <summary>
    /// Message shown when the search/download returns no records
    /// </summary>
    public string? NoRecordsFoundMessage { get; set; }

}
/// <summary>
/// Week Date Range Model
/// </summary>
public class DownloadReportRequestModel
{
    /// <summary>
    /// </summary>
    public string EmployerSK { get; set; } = string.Empty;
    /// <summary>
    /// </summary>
    public string? FromDate { get; set; }
    /// <summary>
    /// </summary>
    public string? ToDate { get; set; }
    /// <summary>
    /// </summary>
    public string HighSSN { get; set; } = string.Empty;
    /// <summary>
    /// </summary>
    public string LowSSN { get; set; } = string.Empty;
    /// <summary>
    /// </summary>
    public int SecureUserSK { get; set; }
    /// <summary>
    /// </summary>
    public string? FileExtension { get; set; }
}
