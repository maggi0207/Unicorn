namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;

/// <summary>
///
/// </summary>
public class AdjustedTaxWageModel
{
    /// <summary>
    /// Adjusted EmployeeCountQtr1
    /// </summary>
    public int? AdjustedEmployeeCountQtr1 { get; set; } = 0;

    /// <summary>
    /// Adjusted EmployeeCountQtr2
    /// </summary>
    public int? AdjustedEmployeeCountQtr2 { get; set; } = 0;

    /// <summary>
    /// Adjusted EmployeeCountQtr3
    /// </summary>
    public int? AdjustedEmployeeCountQtr3 { get; set; } = 0;

    /// <summary>
    /// Adjusted TotalGrossCoveredWages
    /// </summary>
    public decimal? AdjustedTotalGrossCoveredWages { get; set; } = 0;

    /// <summary>
    /// Adjusted LessExclusionWages
    /// </summary>
    public decimal? AdjustedLessExclusionWages { get; set; } = 0;

    /// <summary>
    /// Adjusted Defined TaxableIncome
    /// </summary>
    public decimal? AdjustedDefinedTaxableIncome { get; set; } = 0;

    /// <summary>
    /// Adjusted TaxAssessed
    /// </summary>
    public decimal? AdjustedTaxAssessed { get; set; }

    /// <summary>
    /// Adjusted ReportQuarter
    /// </summary>
    public int AdjustedReportQuarter { get; set; }

    /// <summary>
    /// Adjusted Year
    /// </summary>
    public int AdjustedYear { get; set; }
}
