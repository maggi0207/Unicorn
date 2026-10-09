using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Data model for the Employee wage entry component.
/// </summary>
public class EmployeeWageEntryModel : BaseQuarterlyTaxReportModel, IValidatableObject
{
    /// <summary>
    /// List of employees to display in the wage entry table
    /// </summary>
    [MinLength(1, ErrorMessage = "At least one employee is required.")]
    public List<EmployeeWageEntryData> Employees { get; set; } = [];

    /// <summary>
    /// Gets or sets WageTaxFilingSK
    /// </summary>
    public int? WageTaxFilingSK { get; set; }

    /// <summary>
    /// Gets or sets TotalGrossWages
    /// </summary>
    public decimal TotalGrossWages => Employees.Sum(e =>
    {
        // Null means "not entered yet" and contributes nothing to the running total. Submission
        // cannot happen with any wage still null - [Required] blocks it - so this only affects the
        // live total shown while the user is still typing.
        return e.QuarterlyWages ?? 0m;
    });

    /// <summary>
    /// Custom validation for the employee wage entry step.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var totalWages = Employees.Sum(e =>
        {
            return e.QuarterlyWages;
        });
        if (totalWages <= 0)
        {
            yield return new ValidationResult(
                "Total Gross Covered Wages must be greater than zero. If there are no reportable wages, you must Cancel this report and select \"Zero Payroll this Quarter Report\".",
                new[] { string.Empty });
        }

        if (totalWages > MaxGrossWagesAmount)
        {
            yield return new ValidationResult(
               $"Total Covered Gross Wages Cannot Exceed {MaxGrossWagesAmount:C}.",
                new[] { nameof(Employees) });
        }
    }
}
