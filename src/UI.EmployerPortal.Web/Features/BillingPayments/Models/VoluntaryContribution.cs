using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;



namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;
/// <summary>
/// 
/// </summary>
public class VoluntaryContribution
{
    /// <summary>
    /// AmountToPay
    /// </summary>
    [PaymentValidation]
    public decimal EstimatedTaxablePayroll { get; set; }
    /// <summary>
    /// AmountToPay
    /// </summary>
    [PaymentValidation]
    public decimal PaymentAmount { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal NetTaxSavings { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal ReserveFundBalance { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal ReserveFundPercentage { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal TaxRateForYear { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal TaxSavingsBasedOnEstimatedPayroll { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal TaxablePayRoll { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal VcRequired { get; set; }
    /// <summary>
    /// SelectedPaymentMethod
    /// </summary>
    public string? SelectedPaymentMethod { get; set; }
    /// <summary>
    ///  Description Text from recalculate method
    /// </summary>
    public string? DisclaimerText { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public decimal Lowerrate { get; set; }
    /// <summary>
    /// Any rule violations returned from the service.
    /// </summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
    /// <summary>
    /// check
    /// </summary>
    /// 
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
