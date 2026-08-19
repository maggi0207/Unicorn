namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>Result from confirming payment via Orbipay API.</summary>
public sealed record OrbipayConfirmationResult
{
    /// <summary>True if payment was successfully confirmed.</summary>
    public bool Success { get; init; }

    /// <summary>Confirmation number from Orbipay.</summary>
    public string? ConfirmationNumber { get; init; }

    /// <summary>Payment amount processed.</summary>
    public decimal? Amount { get; init; }

    /// <summary>Payment method (e.g., VISA, MASTERCARD).</summary>
    public string? PaymentMethod { get; init; }

    /// <summary>Last 4 digits of card.</summary>
    public string? LastFourDigits { get; init; }

    /// <summary>Convenience fee applied (2% per business rules).</summary>
    public decimal? ConvenienceFee { get; init; }

    /// <summary>Payment date/time.</summary>
    public DateTime? PaymentDate { get; init; }

    /// <summary>Error description if confirmation failed.</summary>
    public string? ErrorDescription { get; init; }

    /// <summary>Error field from Orbipay if applicable.</summary>
    public string? ErrorField { get; init; }

    /// <summary>Error code from Orbipay if applicable.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>PhoneNumber.</summary>
    public string? PhoneNumber { get; init; }

    /// <summary>DisplayError.</summary>
    public string? DisplayError { get; init; }
}
