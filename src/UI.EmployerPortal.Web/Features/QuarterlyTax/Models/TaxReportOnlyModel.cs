using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Unified data model for the TaxReportOnly page.
/// Acts as the single source of truth shared across step 1 (TaxEntry) and step 2 (TaxDetails).
/// </summary>
public class TaxReportOnlyModel : BaseQuarterlyTaxReportModel
{
    /// <summary>Wage threshold amount for exclusions.</summary>
    public decimal ExclusionThreshold { get; set; }

    /// <summary>
    /// Data model for step 1 (tax entry)
    /// </summary>
    public TaxEntryModel TaxEntryData { get; set; } = new();

    /// <summary>
    /// Data model for the setp 2 tax details verification.
    /// </summary>
    public TaxDetailsModel TaxDetailsData { get; set; } = new();

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
