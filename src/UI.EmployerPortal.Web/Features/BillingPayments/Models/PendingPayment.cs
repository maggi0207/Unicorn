namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// A pending EFT payment associated with a bank account, used on the edit bank account form.
/// </summary>
public sealed record PendingPayment
{
    /// <summary>
    /// Scheduled settlement date for this payment.
    /// </summary>
    public DateTime SettlementDate { get; init; }

    /// <summary>
    /// Unique confirmation identifier returned by the EFT payment service.
    /// </summary>
    public string ConfirmationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Dollar amount of the payment.
    /// </summary>
    public decimal Amount { get; init; }
}
