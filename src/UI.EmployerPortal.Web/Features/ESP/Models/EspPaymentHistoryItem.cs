namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// View model for a single row in the ESP Payment History table.
/// </summary>
public sealed record EspPaymentHistoryItem
{
    /// <summary>
    /// Surrogate key identifying the EFT payment.
    /// </summary>
    public int EftPaymentSk { get; init; }

    /// <summary>
    /// Date the payment is scheduled to settle.
    /// </summary>
    public DateOnly? SettlementDate { get; init; }

    /// <summary>
    /// Payment amount.
    /// </summary>
    public decimal Amount { get; init; }

    /// <summary>
    /// Unique confirmation identifier for the payment.
    /// </summary>
    public string ConfirmationId { get; init; } = string.Empty;

    /// <summary>
    /// Current status description from the service (e.g. "Pending", "Cancelled").
    /// </summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>
    /// Whether the Edit/Cancel action icons should be shown for this row.
    /// Requires: Pending status and a future settlement date.
    /// </summary>
    public bool IsEligibleForAction { get; init; }
}
