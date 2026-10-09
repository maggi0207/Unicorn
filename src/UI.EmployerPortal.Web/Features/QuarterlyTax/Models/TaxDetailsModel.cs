namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Data model for Tax Details component
/// </summary>
public class TaxDetailsModel : BaseQuarterlyTaxReportModel
{
    /// <summary>
    /// Employee count for the first month
    /// </summary>
    public int? EmployeeCount1stMonth { get; set; } = 0;

    /// <summary>
    /// Name of the first month
    /// </summary>
    public string FirstMonthName { get; set; } = string.Empty;

    /// <summary>
    /// Employee count for the second month
    /// </summary>
    public int? EmployeeCount2ndMonth { get; set; } = 0;

    /// <summary>
    /// Name of the second month
    /// </summary>
    public string SecondMonthName { get; set; } = string.Empty;

    /// <summary>
    /// Employee count for the third month
    /// </summary>
    public int? EmployeeCount3rdMonth { get; set; } = 0;

    /// <summary>
    /// Name of the third month
    /// </summary>
    public string ThirdMonthName { get; set; } = string.Empty;

    /// <summary>
    /// Total gross covered wages
    /// </summary>
    public decimal TotalGrossCoveredWages { get; set; }

    /// <summary>
    /// Exclusions for wages over the threshold
    /// </summary>
    public decimal LessExclusions { get; set; }

    /// <summary>
    /// Wage threshold amount for exclusions
    /// </summary>
    public decimal ExclusionThreshold { get; set; }

    /// <summary>
    /// Defined (Taxable) Payroll after exclusions
    /// </summary>
    public decimal DefinedPayroll { get; set; }

    /// <summary>
    /// Tax rate for the year
    /// </summary>
    public decimal TaxRate { get; set; }

    /// <summary>
    /// Total tax assessed
    /// </summary>
    public decimal TaxAssessed { get; set; }

    /// <summary>
    /// Reason for the overriding the exclusion amount. Null if not overridden.
    /// </summary>
    public string? ExclusionOverrideReason { get; set; }

    /// <summary>
    /// Previously reported gross wages from the wage report. Null if no wage report exists
    /// </summary>
    public decimal? PreviouslyReportedGrossWages { get; set; }

    /// <summary>
    /// Explanation for why wages differ from the wage report.
    /// </summary>
    public string? GrossWageDiscrepancyExplanation { get; set; }

    /// <summary>
    /// IsAuditSource
    /// </summary>
    public bool IsAuditSource { get; set; }
}
