namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;

/// <summary>
/// 
/// </summary>
public class PreviousTaxWageAdjustmentModel
{
    /// <summary>
    /// Previous QuarterYear
    /// </summary>
    public string? PreviousQuarterYear { get; set; }
    /// <summary>
    /// Previous Quarter
    /// </summary>
    public int? PreviousQuarter { get; set; }
    /// <summary>
    /// Previous Year
    /// </summary>
    public int? PreviousYear { get; set; }
    /// <summary>
    /// Previous Month1
    /// </summary>
    public string? PreviousMonth1 { get; set; }
    /// <summary>
    /// Previous Month2
    /// </summary>
    public string? PreviousMonth2 { get; set; }
    /// <summary>
    /// Previous Month3
    /// </summary>
    public string? PreviousMonth3 { get; set; }
    /// <summary>
    /// Previous EmployeeCountQtr1
    /// </summary>
    public int? PreviousEmployeeCountQtr1 { get; set; } = 0;
    /// <summary>
    /// Previous EmployeeCountQtr2
    /// </summary>
    public int? PreviousEmployeeCountQtr2 { get; set; } = 0;
    /// <summary>
    /// Previous EmployeeCountQtr3
    /// </summary>
    public int? PreviousEmployeeCountQtr3 { get; set; } = 0;
    /// <summary>
    /// Previous TotalGrossCoveredWages
    /// </summary>
    public decimal? PreviousTotalGrossCoveredWages { get; set; } = 0;
    /// <summary>
    /// PreviousLess ExclusionWages
    /// </summary>
    public decimal? PreviousLessExclusionWages { get; set; } = 0;
    /// <summary>
    /// Previous Defined TaxableIncome
    /// </summary>
    public decimal? PreviousDefinedTaxableIncome { get; set; } = 0;
    /// <summary>
    /// Previous Current TaxRate
    /// </summary>
    public decimal PreviousCurrentTaxRate { get; set; } = 0;
    /// <summary>
    /// Previous Effective Date
    /// </summary>
    public DateTime PreviousEffectiveDate { get; set; }
    /// <summary>
    ///Previous Due Date
    /// </summary>
    public DateTime PreviousDueDate { get; set; }
    /// <summary>
    /// Previous Tax Assessed
    /// </summary>
    public decimal? PreviousTaxAssessed { get; set; }

}
