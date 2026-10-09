namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Data model for the wage upload report
/// </summary>
public class WageUploadReportModel
{
    /// <summary>
    /// The type of the wage report
    /// </summary>
    public string WageReportType { get; set; } = "Original";

    /// <summary>
    /// Set when user confirms that they intend to uupload a replacement or appended wage report. 
    /// </summary>
    public bool ReportTypeAcknowledged { get; set; }

    /// <summary>
    /// Wage file upload data
    /// </summary>
    public WageFileUploadModel WageFileData { get; set; } = new();

    /// <summary>
    /// Contact information
    /// </summary>
    public ContactModel ContactData { get; set; } = new();
}
