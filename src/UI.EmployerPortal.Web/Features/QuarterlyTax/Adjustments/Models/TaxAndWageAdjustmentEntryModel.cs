using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;

/// <summary>
/// Entry model for Tax and Wage Adjustment with live validation support.
/// </summary>
public class TaxAndWageAdjustmentEntryModel : BaseQuarterlyTaxReportModel, IValidatableObject
{
    /// <summary>
    /// Year
    /// </summary>
    public int? Year { get; set; }

    /// <summary>
    /// Employee Count 1
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Employee counts cannot be negative")]
    public int? EmployeeCountQtr1 { get; set; } = 0;

    /// <summary>
    /// Employee Count 2
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Employee counts cannot be negative")]
    public int? EmployeeCountQtr2 { get; set; } = 0;

    /// <summary>
    /// Employee Count 3
    /// </summary>
    [Range(0, int.MaxValue, ErrorMessage = "Employee counts cannot be negative")]
    public int? EmployeeCountQtr3 { get; set; } = 0;

    /// <summary>
    /// Month 1
    /// </summary>
    public string? Month1 { get; set; }

    /// <summary>
    /// Month 2
    /// </summary>
    public string? Month2 { get; set; }

    /// <summary>
    /// Month 3
    /// </summary>
    public string? Month3 { get; set; }

    /// <summary>
    /// Total Wages
    /// </summary>
    [Range(0, 999_999_999.99, ErrorMessage = "Quarterly wages must be less than $1,000,000,000.")]
    public decimal? TotalGrossCoveredWages { get; set; } = 0;

    /// <summary>
    /// Less Exclusions
    /// </summary>
    [Range(0, 999_999_999.99, ErrorMessage = "Quarterly wages must be less than $1,000,000,000.")]
    public decimal? LessExclusionWages { get; set; } = 0;

    /// <summary>
    /// Defined Taxable Income
    /// </summary>

    public decimal? DefinedTaxableIncome { get; set; } = 0;

    /// <summary>
    /// Current Tax Rate
    /// </summary>
    public decimal CurrentTaxRate { get; set; } = 0;

    /// <summary>
    /// Tax Assessed
    /// </summary>
    public decimal? TaxAssessed { get; set; } = 0;

    /// <summary>
    /// Wage Change Text
    /// </summary>
    public string? WageChangeText { get; set; }

    /// <summary>
    /// Effective Date
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// CodeSK
    /// </summary>
    public int? CodeSK { get; set; }

    /// <summary>
    /// Reason
    /// </summary>
    public string? ReasonText { get; set; }

    /// <summary>
    /// Has Payroll Adjustment
    /// </summary>
    public bool HasPayrollAdjustment { get; set; }

    /// <summary>
    /// Contact Email Address
    /// </summary>
    public string? ContactEmailAddress { get; set; }

    /// <summary>
    /// model to hold previous values
    /// </summary>
    public PreviousTaxWageAdjustmentModel? PreviousTaxWageValues { get; set; }

    /// <summary>
    /// Custom Validation
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Normalize nulls
        var emp1 = EmployeeCountQtr1 ?? 0;
        var emp2 = EmployeeCountQtr2 ?? 0;
        var emp3 = EmployeeCountQtr3 ?? 0;
        var gross = TotalGrossCoveredWages ?? 0m;
        var exclusion = LessExclusionWages ?? 0m;

        var employeeTotalCount = emp1 + emp2 + emp3;

        var isSame = PreviousTaxWageValues!.PreviousEmployeeCountQtr1 == EmployeeCountQtr1 &&
                             PreviousTaxWageValues.PreviousEmployeeCountQtr2 == EmployeeCountQtr2 &&
                             PreviousTaxWageValues.PreviousEmployeeCountQtr3 == EmployeeCountQtr3 &&
                             PreviousTaxWageValues.PreviousTotalGrossCoveredWages == TotalGrossCoveredWages &&
                             PreviousTaxWageValues.PreviousLessExclusionWages == LessExclusionWages &&
                             PreviousTaxWageValues.PreviousDefinedTaxableIncome == DefinedTaxableIncome;

        if (isSame)
        {
            yield return new ValidationResult(
               "No changes have been made. Make changes as necessary or click cancel."
           );
        }

        if (gross > 0 && (gross <= (employeeTotalCount)))
        {
            yield return new ValidationResult(
              "The sum of the employee counts is equal to or greater than the gross wages.");
        }

        if (gross == 0 && employeeTotalCount > 0)
        {
            yield return new ValidationResult(
                "If Gross Wages are equal to zero, all employee counts must equal to zero.",
                new[] { nameof(TotalGrossCoveredWages) });
        }

        if (gross > 0 && emp1 == 0 && emp2 == 0 && emp3 == 0)
        {
            yield return new ValidationResult(
                "When gross wages, item 2, are greater than zero you must enter an employee count in at least one month in the quarter. Do not enter wage amounts; you must enter the Number of Employees."

            );
        }

        if (exclusion > gross)
        {
            yield return new ValidationResult(
                "Total Covered Wage, item 2, must be greater than or equal to Exclusions, item 3.",
                new[] { nameof(TotalGrossCoveredWages) }
            );
        }

        if (gross == 0 && string.IsNullOrEmpty(WageChangeText))
        {
            yield return new ValidationResult(
               "Wage Change Explanation is required",
               new[] { nameof(WageChangeText) }
           );
        }

        if (CodeSK is null)
        {
            yield return new ValidationResult(
               "Reason is required",
               new[] { nameof(CodeSK) }
           );
        }
    }
}
