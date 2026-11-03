using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Models;

/// <summary>
/// Email Subscription Model
/// </summary>
public class EmailSubscription
{
    /// <summary>
    /// Email Address
    /// </summary>
    [Required(ErrorMessage = "Email address is required")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address")]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Confirm email address
    /// </summary>
    [ConfirmEmailvalidation]
    public string ConfirmEmailAddress { get; set; } = string.Empty;

    /// <summary>
    /// Time to File UI Tax and Wage Report
    /// </summary>
    public bool TimeToFileUiTaxAndWageReport { get; set; }

    /// <summary>
    /// Unemployment Insurance News
    /// </summary>
    public bool UnemploymentInsuranceNews { get; set; }

    /// <summary>
    /// check
    /// </summary>
    public class ConfirmEmailvalidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (EmailSubscription) validationContext.ObjectInstance;
            var confirmEmail = value as string;
            if (string.IsNullOrWhiteSpace(confirmEmail))
            {
                return new ValidationResult("Confirm Email Address is required", new[] { nameof(EmailSubscription.ConfirmEmailAddress) });
            }
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");

            return !regex.IsMatch(confirmEmail)
                ? new ValidationResult("Invalid email format", new[] { nameof(EmailSubscription.ConfirmEmailAddress) })
                : !string.Equals(confirmEmail?.Trim(), model.EmailAddress?.Trim(), StringComparison.OrdinalIgnoreCase)
                ? new ValidationResult("Email addresses do not match", new[] { nameof(EmailSubscription.ConfirmEmailAddress) })
                : ValidationResult.Success;
        }
    }

}
