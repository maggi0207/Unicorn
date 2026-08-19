namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Represents a bank account used in the ACH payment flow.
/// </summary>
public sealed record AchBankAccount
{
    /// <summary>Gets the unique identifier for the bank account.</summary>
    public string AccountNickname { get; init; } = string.Empty;
    /// <summary>Gets the name of the account holder.</summary>
    public string AccountHolderName { get; init; } = string.Empty;
    /// <summary>Gets the name of the bank.</summary>
    public string BankName { get; init; } = string.Empty;
    /// <summary>Gets the routing number of the bank.</summary>
    public string RoutingNumber { get; init; } = string.Empty;
    /// <summary>Gets the masked account number, showing only the last four digits.</summary>
    public string MaskedAccountNumber { get; init; } = string.Empty;
    /// <summary>Gets the type of the bank account (e.g., Checking, Savings).</summary>
    public string AccountType { get; init; } = string.Empty;
}
