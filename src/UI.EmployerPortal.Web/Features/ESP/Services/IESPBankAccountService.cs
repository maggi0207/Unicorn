using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.ESP.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// Wraps EFT payment WCF service calls for bank account operations.
/// </summary>
internal interface IESPBankAccountService
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
    Task<SaveBankAccountResult> SaveBankAccountAsync(BankAccountModel model);

    /// <summary>
    /// Returns all active bank accounts on record for the given employer.
    /// Returns an empty list if none exist or the service is unavailable.
    /// </summary>
    Task<IReadOnlyList<SavedBankAccount>> GetExistingAccountsAsync();

    /// <summary>
    /// Inactivates the specified bank account for the currently selected employer
    /// Returns a result indicating success or the first rule violation message.
    /// </summary>
    Task<SaveBankAccountResult> InactivateBankAccountAsync(int bankAccountSk, int employerAccountSk);

    /// <summary>
    /// Loads a single bank account by SK and maps it to a <see cref="BankAccountModel"/> for pre-populating the edit form.
    /// Returns null if the account is not found or the service is unavailable.
    /// </summary>
    Task<BankAccountModel?> GetBankAccountForEditAsync(int bankAccountSk);

    /// <summary>
    /// Returns all pending EFT payments associated with the given bank account.
    /// Returns an empty list if none exist or the service is unavailable.
    /// </summary>
    Task<IReadOnlyList<PendingPayment>> GetPendingPaymentsAsync(int bankAccountSk);

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
