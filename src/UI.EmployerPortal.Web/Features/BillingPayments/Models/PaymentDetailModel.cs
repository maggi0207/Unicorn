namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>View model for the Payment Details page.</summary>
public sealed record PaymentDetailModel
{
    /// <summary>Unique confirmation identifier for the payment.</summary>
    public string ConfirmationNumber { get; init; } = string.Empty;

    /// <summary>Date and time the payment was last submitted.</summary>
    public DateTime? TransactionDateTime { get; init; }

    /// <summary>Payment amount.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Date the payment is scheduled to settle.</summary>
    public DateOnly? SettlementDate { get; init; }

    /// <summary>Current status description from the service (e.g. "Pending", "Completed").</summary>
    public string Status { get; init; } = string.Empty;

    /// <summary>Date and time the payment was cancelled. Null when not cancelled.</summary>
    public DateTime? CancellationDate { get; init; }

    /// <summary>Bank account used for the payment. Null when not resolvable from current accounts.</summary>
    public SavedBankAccount? BankAccount { get; init; }

    /// <summary>Contact information associated with the payment.</summary>
    public ACHContactModel? ContactInfo { get; init; }

    /// <summary>Audit/activity history entries for the payment, ordered most-recent first.</summary>
    public IReadOnlyList<PaymentActivityItem> ActivityHistory { get; init; } = [];
}

/// <summary>A single row in the Payment Activity history table.</summary>
public sealed record PaymentActivityItem
{
    /// <summary>Date and time the activity occurred.</summary>
    public DateTime Date { get; init; }

    /// <summary>Action label (e.g. "Informational", "Processed", "Created").</summary>
    public string Action { get; init; } = string.Empty;

    /// <summary>Full description text of the activity.</summary>
    public string Description { get; init; } = string.Empty;
}
