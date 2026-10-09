using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents tax entry data used across reporting workflows.
/// </summary>
public class TaxEntryModel : BaseQuarterlyTaxReportModel, IValidatableObject
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
    [Required(ErrorMessage = "Employee count month1 is required.")]
    [Range(0, EmployeeFieldRules.MaxEmployeeCount, ErrorMessage = EmployeeFieldRules.EmployeeCountOutOfRangeMessage)]
    public int EmployeeCountMonth1 { get; set; }

    /// <summary>
    /// Employee count for second month of the quarter.
    /// </summary>
    [Required(ErrorMessage = "Employee count month2 is required.")]
    [Range(0, EmployeeFieldRules.MaxEmployeeCount, ErrorMessage = EmployeeFieldRules.EmployeeCountOutOfRangeMessage)]
    public int EmployeeCountMonth2 { get; set; }

    /// <summary>
    /// Employee count for third month of the quarter.
    /// </summary>
    [Required(ErrorMessage = "Employee count month3 is required.")]
    [Range(0, EmployeeFieldRules.MaxEmployeeCount, ErrorMessage = EmployeeFieldRules.EmployeeCountOutOfRangeMessage)]
    public int EmployeeCountMonth3 { get; set; }

    /// <summary>
    /// Total gross covered wages for the quarter.
    /// </summary>
    [Required(ErrorMessage = "Total Gross Covered Wages is required.")]
    [Range(0, 999_999_999.99, ErrorMessage = "Quarterly wages must be less than $1,000,000,000.")]
    public decimal TotalGrossCoveredWages { get; set; }

    /// <summary>
    /// Previously Reported Wages for the quarter
    /// </summary>
    public decimal? PreviouslyReportedGrossWages { get; set; }

    /// <summary>
    /// Explanation for why gross wages differ from previously reported wages.
    /// </summary>
    public string? GrossWageDiscrepancyExplanation { get; set; }

    /// <summary>
    /// Exclusion override state: calculated amount, override amount, reason, and whether override is active.
    /// </summary>
    public ExclusionOverrideModel ExclusionOverride { get; set; } = new();

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

    /// <summary>
    /// Report Name
    /// </summary>
    public string? ReportName { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// When true, user enters exclusion directly without the popup - no reason required. 
    /// </summary>
    public bool AllowDirectExclusionEntry { get; set; }

    /// <summary>
    /// Gets or sets WageTaxFilingSK
    /// </summary>
    public int? WageTaxFilingSK { get; set; }

    /// <summary>
    /// IsAuditSource
    /// </summary>
    public bool IsAuditSource { get; set; }

    /// <summary>
    /// Validate
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (TotalGrossCoveredWages == 0 && (EmployeeCountMonth1 + EmployeeCountMonth2 + EmployeeCountMonth3) > 0)
        {
            yield return new ValidationResult(
                "If Gross Wages are equal to zero, all employee counts must equal to zero.",
                new[] { nameof(TotalGrossCoveredWages) });
        }

        var totalEmployeeCount = EmployeeCountMonth1 + EmployeeCountMonth2 + EmployeeCountMonth3;
        if (TotalGrossCoveredWages > 0 && totalEmployeeCount > 0 && TotalGrossCoveredWages < (totalEmployeeCount / 0.80m))
        {
            yield return new ValidationResult(
                $"Employee count total must not exceed 80% of total gross covered wages.",
                new[] { nameof(TotalGrossCoveredWages) });
        }

        if (TotalGrossCoveredWages < ExclusionOverride.EffectiveAmount)
        {
            yield return new ValidationResult(
                "Ensure the Exclusion amount, item 3, is less than or equal to Total Covered Wages, item 2.",
                new[] { nameof(ExclusionOverride) });
        }

        if (TotalGrossCoveredWages > 0 && EmployeeCountMonth1 == 0 && EmployeeCountMonth2 == 0 && EmployeeCountMonth3 == 0)
        {
            yield return new ValidationResult(
                "When gross wages, item2, are greater than zero you must enter an employee count in at least one month in the quarter. Do not enter wage amounts; you must enter the Number of Employees.",
                new[] { nameof(EmployeeCountMonth1) }
                );
        }

        if (ExclusionOverride.IsOverride &&
            !AllowDirectExclusionEntry &&
            string.IsNullOrWhiteSpace(ExclusionOverride.OverrideReason))
        {
            yield return new ValidationResult(
                "Reason for exclusion change is required.",
                new[] { nameof(ExclusionOverride) });
        }

        if (!IsAuditSource &&
            PreviouslyReportedGrossWages.HasValue &&
            TotalGrossCoveredWages != PreviouslyReportedGrossWages.Value &&
            string.IsNullOrWhiteSpace(GrossWageDiscrepancyExplanation))
        {
            yield return new ValidationResult(
                "Please explain why the gross covered wages do not match the previously reported wage report.",
                new[] { nameof(GrossWageDiscrepancyExplanation) });
        }

        if (TotalGrossCoveredWages > MaxGrossWagesAmount)
        {
            yield return new ValidationResult(
                $"Total Covered Gross Wages Cannot Exceed {MaxGrossWagesAmount:C}.",
                new[] { nameof(TotalGrossCoveredWages) });
        }
    }
}
