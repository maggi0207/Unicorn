using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// Wraps EFT payment WCF service calls for bank account operations.
/// </summary>
internal interface IBankAccountService
{
    /// <summary>
    /// Calls the routing number lookup and returns the bank name.
    /// Returns null if the number is invalid or the service is unavailable.
    /// </summary>
    Task<string?> CheckRoutingNumberAsync(string routingNumber);

    /// <summary>
    /// Submits the bank account to the EFT payment service.
    /// Returns a result indicating success or the first rule violation message.
    /// </summary>
    Task<SaveBankAccountResult> SaveBankAccountAsync(BankAccountModel model, int employerAccountSk);

    /// <summary>
    /// Returns all active bank accounts on record for the given employer.
    /// Returns an empty list if none exist or the service is unavailable.
    /// </summary>
    Task<IReadOnlyList<SavedBankAccount>> GetExistingAccountsAsync(int employerAccountSk);

    /// <summary>
    /// Inactivates the specified bank account for the currently selected employer
    /// Returns a result indicating success or the first rule violation message.
    /// </summary>
    Task<SaveBankAccountResult> InactivateBankAccountAsync(int bankAccountSk, int employerAccountSk);

    /// <summary>
    /// Loads a single bank account by SK and maps it to a <see cref="BankAccountModel"/> for pre-populating the edit form.
    /// Returns null if the account is not found or the service is unavailable.
    /// </summary>
    Task<BankAccountModel?> GetBankAccountForEditAsync(int bankAccountSk, int employerAccountSk);

    /// <summary>
    /// Returns all pending EFT payments associated with the given bank account.
    /// Returns an empty list if none exist or the service is unavailable.
    /// </summary>
    Task<IReadOnlyList<PendingPayment>> GetPendingPaymentsAsync(int bankAccountSk, int employerAccountSk);

    /// <summary>
    /// Returns the list of countries from the EFT payment service,
    /// including the short ISO code needed to identify USA and Canada.
    /// </summary>
    Task<IReadOnlyList<BankCountryOption>> GetCountryCodesAsync();

    /// <summary>
    /// Returns the list of US states from the EFT payment service.
    /// </summary>
    Task<IReadOnlyList<SelectOption>> GetUSStateCodesAsync();

    /// <summary>
    /// Returns the list of Canadian provinces from the EFT payment service.
    /// </summary>
    Task<IReadOnlyList<SelectOption>> GetCanadianStateCodesAsync();
}

/// <summary>
/// Represents the outcome of a save bank account operation.
/// </summary>
/// <param name="Success">True if the account was saved without rule violations.</param>
/// <param name="ErrorMessage">The first rule violation message when <paramref name="Success"/> is false.</param>
public sealed record SaveBankAccountResult(bool Success, string? ErrorMessage = null);

/// <summary>
/// A country entry returned from the EFT payment service code table.
/// Carries the short ISO code so the form can reliably identify USA and Canada.
/// </summary>
/// <param name="Value">The CodeSK as a string — used as the dropdown option value.</param>
/// <param name="Text">The long description — displayed in the dropdown.</param>
/// <param name="ShortCode">The short ISO code, e.g. "US" or "CA".</param>
public sealed record BankCountryOption(string Value, string Text, string ShortCode);
