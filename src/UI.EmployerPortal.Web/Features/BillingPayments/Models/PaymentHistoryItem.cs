namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// View model for a single row in the Payment History table.
/// </summary>
public sealed record PaymentHistoryItem
{
    /// <summary>
    /// Surrogate key identifying the EFT payment.
    /// </summary>
    public int EftPaymentSk { get; init; }

    /// <summary>
    /// Display label for the payment type (e.g. "ACH Debit", "Credit/Debit Card").
    /// </summary>
    public string PaymentType { get; init; } = string.Empty;

    /// <summary>
    /// Date the payment is scheduled to settle.
    /// </summary>
    public DateOnly SettlementDate { get; init; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Service fee amount. Null when not applicable (e.g. ACH payments).
    /// </summary>
    public decimal? ServiceFee { get; init; }

    /// <summary>
    /// Unique confirmation identifier for the payment.
    /// </summary>
    public string ConfirmationId { get; init; } = string.Empty;

    /// <summary>
    /// Current status description from the service (e.g. "Pending", "Processed").
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// SK of the user who created this payment.
    /// </summary>
    public int SecureUserSk { get; init; }

    /// <summary>
    /// True when this payment was initiated by the currently logged-in user.
    /// Determines whether the Confirmation # is shown as a link and whether actions are shown.
    /// </summary>
    public bool IsCurrentUser { get; init; }

    /// <summary>
    /// True when the payment is eligible for edit and cancel actions.
    /// Requires: ACH type, Pending status, future settlement date, and created by current user.
    /// </summary>
    public bool IsEligibleForAction { get; init; }
}
