using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Tracks whether the user has overridden the system-calculated exclusion amount and stores both the calculated and override values.
/// </summary>
public class ExclusionOverrideModel : IValidatableObject
{
    /// <summary>
    /// The system-calculated exclusion amount.
    /// </summary>
    public decimal CalculatedExclusionAmount { get; set; }

    /// <summary>
    /// The user-entered override amount.
    /// </summary>
    public decimal OverrideAmount { get; set; }

    /// <summary>
    /// The reason the user gave for overriding the exclusion amount. 
    /// </summary>
    public string? OverrideReason { get; set; }

    /// <summary>
    /// Whether the user chose to override the calculated amount. 
    /// </summary>
    public bool IsOverride { get; set; }

    /// <summary>
    /// Returns the effective exclusion amount: the override if active, otherwise calculated value.
    /// </summary>
    public decimal EffectiveAmount => IsOverride ? OverrideAmount : CalculatedExclusionAmount;

    /// <summary>
    /// Validate
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (IsOverride && string.IsNullOrEmpty(OverrideReason))
        {
            yield return new ValidationResult(
                "Reason for change is required.",
                new[] { nameof(OverrideReason) });
        }
    }

    /// <summary>
    /// Creates a shallow copy of this model.
    /// </summary>
    /// <returns></returns>
    public ExclusionOverrideModel Clone()
    {
        return new()
        {
            CalculatedExclusionAmount = CalculatedExclusionAmount,
            OverrideAmount = OverrideAmount,
            OverrideReason = OverrideReason,
            IsOverride = IsOverride
        };
    }

    /// <summary>
    /// Copies values from another model into this one
    /// </summary>
    /// <param name="source"></param>
    public void CopyFrom(ExclusionOverrideModel source)
    {
        CalculatedExclusionAmount = source.CalculatedExclusionAmount;
        OverrideAmount = source.OverrideAmount;
        OverrideReason = source.OverrideReason;
        IsOverride = source.IsOverride;
    }
}
