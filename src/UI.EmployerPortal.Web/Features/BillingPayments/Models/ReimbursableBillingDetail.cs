using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;
/// <summary>
/// 
/// </summary>
public class ReimbursableBillingDetail
{
    /// <summary>
    /// SessionKey
    /// </summary>
    public const string SessionKey = "ReimbursableBillingDetail";
    /// <summary>
    /// AmountToPay
    /// </summary>
    [PaymentValidation]
    public decimal AmountToPay { get; set; }
    /// <summary>
    /// TotalEFTPayments
    /// </summary>
    public decimal TotalEFTPayments { get; set; }
    /// <summary>
    /// TotalOutstandingBalance
    /// </summary>
    public decimal TotalOutstandingBalance { get; set; }
    /// <summary>
    /// SelectedPaymentMethod
    /// </summary>
    public string? SelectedPaymentMethod { get; set; }

    /// <summary>
    /// check
    /// </summary>
    public class PaymentValidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// Validates the specified value with respect to the current validation context.
        /// </summary>
        /// <param name="value">The object to validate.</param>
        /// <param name="validationContext">The context information about the validation operation.</param>
        /// <returns>
        /// A <see cref="ValidationResult"/> that indicates whether the specified value is valid. Any other validations can be added here as needed, such as ensuring that the sum of employee counts is greater than zero or that wage values are consistent with each other.
        /// </returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            return value is decimal amount && amount <= 0 ? new ValidationResult("Amount To Pay cannot be negative.") : ValidationResult.Success;
        }
    }
}
