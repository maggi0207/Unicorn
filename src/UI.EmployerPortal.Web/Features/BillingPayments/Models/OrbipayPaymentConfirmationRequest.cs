namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>Request payload for payment confirmation.</summary>
public sealed record OrbipayPaymentConfirmationRequest
{
    /// <summary>Amount</summary>
    public decimal Amount { get; init; }
    /// <summary>ContactName</summary>
    public string ContactName { get; init; } = string.Empty;
    /// <summary>Email</summary>
    public string Email { get; init; } = string.Empty;
    /// <summary>AddressLine1</summary>
    public string AddressLine1 { get; init; } = string.Empty;
    /// <summary>City</summary>
    public string City { get; init; } = string.Empty;
    /// <summary>State</summary>
    public string? State { get; init; }
    /// <summary>Zip</summary>
    public string Zip { get; init; } = string.Empty;
    /// <summary>Country</summary>
    public string Country { get; init; } = string.Empty;
    /// <summary>EmployerSk</summary>
    public int EmployerSk { get; init; }
    /// <summary>RegistrationSk</summary>
    public int RegistrationSk { get; init; }
    /// <summary>EmployerLegalName</summary>
    public string EmployerLegalName { get; init; } = string.Empty;
    /// <summary>EmployerAccountNumber</summary>
    public string EmployerAccountNumber { get; init; } = string.Empty;
    /// <summary>UIAccountNumber</summary>
    public string UIAccountNumber { get; init; } = string.Empty;
    /// <summary>IsVoluntary</summary>
    public bool IsVoluntary { get; init; }
}
