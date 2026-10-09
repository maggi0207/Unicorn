namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents a row displayed in the File Summary grid.
/// </summary>
public class TaxReportFileSummaryModel
{
    /// <summary>
    /// Gets or sets the record number.
    /// </summary>
    public int RecordNumber { get; set; }

    /// <summary>
    /// Gets or sets the error description
    /// </summary>
    public string ErrorDescription { get; set; } = string.Empty;
}
