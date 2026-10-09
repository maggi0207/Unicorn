using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Base model for Quarterly Reports.
/// </summary>
public class BaseQuarterlyTaxReportModel
{
    /// <summary>
    /// MaxGrossWagesAmount
    /// </summary>
    public const decimal MaxGrossWagesAmount = 999_999_999.99m;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>
    /// Due date fo the quarterly report submission
    /// </summary>
    public DateTime DueDate { get; set; }

    /// <summary>
    /// Tax year for the report
    /// </summary>
    public int TaxYear { get; set; }

    /// <summary>
    /// The quarter number (1-4)
    /// </summary>
    public int Quarter { get; set; }

    /// <summary>
    /// The filing type for this report.
    /// </summary>
    public TaxWageFilingType FilingType { get; set; }
}
