using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Requires a complete 10-digit North American phone number when a value is present.
/// Blank values are left to <see cref="RequiredAttribute"/> so optional fields stay optional.
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public sealed class TenDigitPhoneAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is not string phone || string.IsNullOrWhiteSpace(phone))
        {
            return ValidationResult.Success;
        }

        if (phone.Count(char.IsDigit) == 10)
        {
            return ValidationResult.Success;
        }

        // The member name must be supplied: on whole-form validation Blazor attaches a
        // result with no member names to the model-level field instead of this property,
        // which stops ValidationErrorSummary from rendering a link to the input.
        return new ValidationResult(
            ErrorMessage ?? "Enter a valid 10-digit phone number.",
            validationContext.MemberName is { } memberName ? [memberName] : null);
    }
}
