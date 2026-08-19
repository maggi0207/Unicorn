namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Represents employer contact information used in the ACH payment flow.
/// </summary>
public sealed record AchContactInfo
{
    /// <summary>Gets the contact name.</summary>
    public string ContactName { get; init; } = string.Empty;
    /// <summary>Gets the phone number associated with the entity.</summary>
    public string PhoneNumber { get; init; } = string.Empty;
    /// <summary>Gets the email address associated with the entity.</summary>
    public string EmailAddress { get; init; } = string.Empty;
}
