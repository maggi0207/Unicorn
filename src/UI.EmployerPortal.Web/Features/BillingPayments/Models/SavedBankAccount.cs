namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Read model representing a bank account that has been saved to the EFT payment service.
/// Passed from the form to the confirmation screen after a successful save.
/// </summary>
public sealed record SavedBankAccount
{
    /// <summary>
    /// User-defined label for the account.
    /// </summary>
    public string Nickname { get; init; } = string.Empty;

    /// <summary>
    /// ABA routing number.
    /// </summary>
    public string RoutingNumber { get; init; } = string.Empty;

    /// <summary>
    /// Account number with all but the last four digits masked (e.g. *******9177).
    /// </summary>
    public string MaskedAccountNumber { get; init; } = string.Empty;

    /// <summary>
    /// Name of the financial institution, populated from the federal bank name lookup.
    /// </summary>
    public string BankName { get; init; } = string.Empty;

    /// <summary>
    /// Account type — Checking or Savings.
    /// </summary>
    public string AccountType { get; init; } = string.Empty;

    /// <summary>
    /// Surrogate key identifying the bank account
    /// </summary>
    public int BankAccountSk { get; init; }
}
