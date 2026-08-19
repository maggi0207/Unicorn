namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Keep Payment State in Session using this class
/// </summary>
public class PaymentState
{
    /// <summary>
    /// AmountToPay
    /// </summary>
    public decimal AmountToPay { get; set; }
    /// <summary>
    /// TotalPayments
    /// </summary>
    public decimal TotalPayments { get; set; }
    /// <summary>
    /// BalanceDue
    /// </summary>
    public decimal BalanceDue { get; set; }
    /// <summary>
    /// SelectedPaymentMethod
    /// </summary>
    public string? SelectedPaymentMethod { get; set; }
}
