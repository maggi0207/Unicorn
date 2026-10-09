using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents a quarterly tax and wage entry report containing all step data.
/// </summary>
public class TaxAndWageEntryReportModel : BaseQuarterlyTaxReportModel, IValidatableObject
{
    /// <summary>
    /// Data model for the step 1 (Employee wage entry).
    /// </summary>
    public EmployeeWageEntryModel WageEntry { get; set; } = new();

    /// <summary>
    /// Data model for step 2 (tax entry)
    /// </summary>
    public TaxEntryModel TaxEntry { get; set; } = new();

    /// <summary>
    /// Data model for the setp 3 tax details verification.
    /// </summary>
    public TaxDetailsModel TaxDetails { get; set; } = new();

    /// <summary>
    /// Data model for step 3 wage details verification.
    /// </summary>
    public WageDetailsModel WageDetails { get; set; } = new();

    /// <summary>
    /// Gets or sets WageTaxFilingSK
    /// </summary>
    public int? WageTaxFilingSK { get; set; }

    /// <summary>
    /// Custom Validation.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
