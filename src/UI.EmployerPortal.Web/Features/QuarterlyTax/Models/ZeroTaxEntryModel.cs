namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents tax entry data used across reporting workflows.
/// </summary>
public class ZeroTaxEntryModel : BaseQuarterlyTaxReportModel
{
    /// <summary>
    /// Month 1
    /// </summary>
    public string MonthName1 { get; set; } = string.Empty;

    /// <summary>
    /// Month 2
    /// </summary>
    public string MonthName2 { get; set; } = string.Empty;

    /// <summary>
    /// Month 3
    /// </summary>
    public string MonthName3 { get; set; } = string.Empty;

    /// <summary>
    /// Month
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// Employee count for first month of the quarter.
    /// </summary>
    public int EmployeeCountMonth1 { get; set; }

    /// <summary>
    /// Employee count for second month of the quarter.
    /// </summary>
    public int EmployeeCountMonth2 { get; set; }

    /// <summary>
    /// Employee count for third month of the quarter.
    /// </summary>
    public int EmployeeCountMonth3 { get; set; }

    /// <summary>
    /// Total gross covered wages for the quarter.
    /// </summary>
    public decimal TotalGrossCoveredWages { get; set; }

    /// <summary>
    /// Previously Reported Wages for the quarter
    /// </summary>
    public decimal? PreviouslyReportedGrossWages { get; set; }

    /// <summary>
    /// Exclusion amount for wages over the threshold.
    /// </summary>
    public decimal ExclusionAmount { get; set; }

    /// <summary>
    /// Reason for exclusion amount modification.
    /// </summary>
    public string? ExclusionReason { get; set; }

    /// <summary>
    /// Calculated defined (taxable) payroll.
    /// </summary>
    public decimal DefinedTaxablePayroll { get; set; }

    /// <summary>
    /// Applicable tax rate.
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Total tax assessed.
    /// </summary>
    public decimal TaxAssessed { get; set; }
}
