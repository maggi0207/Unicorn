using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.BillingPayments.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.BillingPayments;

/// <summary>
/// Orchestrates bank account operations including routing number lookup,
/// duplicate validation, and save via the EFT payment service.
/// </summary>
public interface IBankAccountOrchestrator
{
    /// <summary>
    /// Looks up the bank name for the given 9-digit routing number.
    /// Returns null if the routing number is invalid or the service is unavailable.
    /// </summary>
    Task<string?> LookupBankNameAsync(string routingNumber);

    /// <summary>
    /// Validates duplicate rules and saves the bank account via the EFT payment service.
    /// </summary>
    Task<SaveBankAccountResult> AddBankAccountAsync(BankAccountModel model);

    /// <summary>
    /// Validates duplicate rules (excluding the account being edited) and updates the bank account via the EFT payment service.
    /// </summary>
    Task<SaveBankAccountResult> EditBankAccountAsync(BankAccountModel model);

    /// <summary>
    /// Loads a single bank account by SK and returns a pre-populated <see cref="BankAccountModel"/> for the edit form.
    /// Returns null if the account is not found or the service is unavailable.
    /// </summary>
    Task<BankAccountModel?> GetBankAccountForEditAsync(int bankAccountSk);

    /// <summary>
    /// Returns all pending EFT payments for the given bank account.
    /// Returns an empty list if none exist or the service is unavailable.
    /// </summary>
    Task<IReadOnlyList<PendingPayment>> GetPendingPaymentsAsync(int bankAccountSk);

    /// <summary>
    /// Inactivates the selected bank account
    /// </summary>
    Task<SaveBankAccountResult> InactivateBankAccountAsync(int bankAccountSk);

    /// <summary>
    /// Returns all existing bank accounts for the currently selected employer.
    /// </summary>
    Task<IReadOnlyList<SavedBankAccount>> GetExistingAccountsAsync();
    /// <summary>
    /// GetPendingReimbursePaymentToSessionAsync
    /// </summary>
    /// <returns></returns>
    Task<String?> GetPaymentToSessionAsync();
    /// <summary>
    /// GetPaymentDescriptionToSessionAsync
    /// </summary>
    /// <returns></returns>
    Task<String?> GetPaymentDescriptionToSessionAsync();
    /// <summary>
    /// Stores the selected payment in session storage.
    /// </summary>
    Task SavePaymentToSessionAsync(string amount, string desc);

    /// <summary>
    /// GetPendingReimbursePaymentToSessionAsync
    /// </summary>
    /// <returns></returns>
    Task<String?> GetVCPaymentToSessionAsync();


    /// <summary>
    /// Stores the selected payment in session storage.
    /// </summary>
    Task SaveVCPaymentToSessionAsync(string amount, string desc);

    /// <summary>
    /// Returns the list of countries from the EFT payment service.
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

    /// <summary>
    /// GetPendingReimbursePaymentToSessionAsync
    /// </summary>
    /// <returns></returns>
    Task<PaymentState?> GetPaymentStateFromSessionAsync();

    /// <summary>
    /// Stores the selected payment in session storage.
    /// </summary>
    Task SavePaymentStateToSessionAsync(PaymentState model);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<(String?, bool)> GetPaymentToSessionWithStatusAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task UpdatePaymentSessionAsync();
}

/// <summary>
/// Default implementation of <see cref="IBankAccountOrchestrator"/>.
/// </summary>
internal class BankAccountOrchestrator : IBankAccountOrchestrator
{
    private readonly IBankAccountService _bankAccountService;
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initializes a new instance of <see cref="BankAccountOrchestrator"/>.
    /// </summary>
    public BankAccountOrchestrator(IBankAccountService bankAccountService, ISessionManager sessionManager)
    {
        _bankAccountService = bankAccountService;
        _sessionManager = sessionManager;
    }

    /// <inheritdoc/>
    public async Task<string?> LookupBankNameAsync(string routingNumber)
    {
        return string.IsNullOrWhiteSpace(routingNumber) || routingNumber.Length != 9
            ? null
            : await _bankAccountService.CheckRoutingNumberAsync(routingNumber);
    }

    /// <inheritdoc/>
    public async Task<SaveBankAccountResult> AddBankAccountAsync(BankAccountModel model)
    {
        var employerSk = await GetEmployerSkAsync();
        if (employerSk is null)
        {
            return new SaveBankAccountResult(false, "No employer account selected");
        }

        var existing = await _bankAccountService.GetExistingAccountsAsync(employerSk.Value);

        return existing.Any(a =>
        { return string.Equals(a.Nickname, model.Nickname, StringComparison.OrdinalIgnoreCase); })
            ? new SaveBankAccountResult(false, "An account with this nickname already exists")
            : existing.Any(a =>
        {
            return a.RoutingNumber == model.RoutingNumber &&
                                   a.MaskedAccountNumber.EndsWith(model.AccountNumber?[^4..] ?? string.Empty);
        })
            ? new SaveBankAccountResult(false, "An account with this account number already exists")
            : await _bankAccountService.SaveBankAccountAsync(model, employerSk.Value);
    }

    /// <inheritdoc/>
    public async Task<SaveBankAccountResult> EditBankAccountAsync(BankAccountModel model)
    {
        var employerSk = await GetEmployerSkAsync();
        if (employerSk is null)
        {
            return new SaveBankAccountResult(false, "No employer account selected");
        }

        var existing = await _bankAccountService.GetExistingAccountsAsync(employerSk.Value);

        var duplicateNickname = existing.Any(a =>
        {
            return a.BankAccountSk != model.BankAccountSk &&
                   string.Equals(a.Nickname, model.Nickname, StringComparison.OrdinalIgnoreCase);
        });

        if (duplicateNickname)
        {
            return new SaveBankAccountResult(false, "An account with this nickname already exists");
        }

        var last4 = model.AccountNumber?.Length >= 4 ? model.AccountNumber[^4..] : model.AccountNumber ?? string.Empty;
        var duplicateAccount = existing.Any(a =>
        {
            return a.BankAccountSk != model.BankAccountSk &&
                   a.RoutingNumber == model.RoutingNumber &&
                   a.MaskedAccountNumber.EndsWith(last4);
        });

        return duplicateAccount
            ? new SaveBankAccountResult(false, "An account with this account number already exists")
            : await _bankAccountService.SaveBankAccountAsync(model, employerSk.Value);
    }

    /// <inheritdoc/>
    public async Task<BankAccountModel?> GetBankAccountForEditAsync(int bankAccountSk)
    {
        var employerSk = await GetEmployerSkAsync();
        return employerSk is null ? null : await _bankAccountService.GetBankAccountForEditAsync(bankAccountSk, employerSk.Value);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<PendingPayment>> GetPendingPaymentsAsync(int bankAccountSk)
    {
        var employerSk = await GetEmployerSkAsync();
        return employerSk is null ? [] : await _bankAccountService.GetPendingPaymentsAsync(bankAccountSk, employerSk.Value);
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SavedBankAccount>> GetExistingAccountsAsync()
    {
        var employerSk = await GetEmployerSkAsync();
        return employerSk is null ? [] : await _bankAccountService.GetExistingAccountsAsync(employerSk.Value);
    }

    private async Task<int?> GetEmployerSkAsync()
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selected?.EmployerAccount?.Id;
    }

    /// <inheritdoc/>
    public async Task<SaveBankAccountResult> InactivateBankAccountAsync(int bankAccountSk)
    {
        var employerSk = await GetEmployerSkAsync();

        return employerSk is null ? new SaveBankAccountResult(false, "No employer account selected") : await _bankAccountService.InactivateBankAccountAsync(bankAccountSk, employerSk.Value);
    }

    public async Task SavePaymentToSessionAsync(string amount, string desc)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectPayment = amount;
            selectedEmployer.PaymentDescription = desc;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    public async Task SaveVCPaymentToSessionAsync(string amount, string desc)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectVCPayment = amount;
            selectedEmployer.PaymentDescription = desc;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    public async Task<String?> GetPaymentToSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer == null)
        {
            return null;
        }

        if (selectedEmployer.SelectPayment != null)
        {
            return selectedEmployer?.SelectPayment;
        }

        if (selectedEmployer.EmployerAccount != null)
        {
            var defaultAmount = selectedEmployer.EmployerAccount.BalanceDue > 0 ? selectedEmployer.EmployerAccount.BalanceDue : 0m;
            selectedEmployer?.SelectPayment = Convert.ToString(defaultAmount);
            return selectedEmployer?.SelectPayment;
        }
        return null;
    }

    public async Task<String?> GetVCPaymentToSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer switch
        {
            null => null,
            _ => selectedEmployer.EmployerAccount != null ? (selectedEmployer?.SelectVCPayment) : null,
        };

    }

    public async Task<(String?, bool)> GetPaymentToSessionWithStatusAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var isAmountCalculatedForSession = selectedEmployer?.IsAmountCalculatedForSession ?? false;

        if (selectedEmployer == null)
        {
            return (null, isAmountCalculatedForSession);
        }

        if (selectedEmployer.SelectPayment != null)
        {
            return (selectedEmployer.SelectPayment, isAmountCalculatedForSession);
        }

        if (selectedEmployer.EmployerAccount != null)
        {
            var defaultAmount = selectedEmployer.EmployerAccount.BalanceDue > 0 ? selectedEmployer.EmployerAccount.BalanceDue : 0m;
            selectedEmployer.SelectPayment = Convert.ToString(defaultAmount);
            return (selectedEmployer.SelectPayment, isAmountCalculatedForSession);
        }
        return (null, isAmountCalculatedForSession);
    }
    //GetPaymentDescriptionToSessionAsync
    public async Task<String?> GetPaymentDescriptionToSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer == null ? null : selectedEmployer.PaymentDescription != null ? (selectedEmployer?.PaymentDescription) : null;
    }



    /// <inheritdoc/>
    public async Task<IReadOnlyList<BankCountryOption>> GetCountryCodesAsync()
    {
        return await _bankAccountService.GetCountryCodesAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SelectOption>> GetUSStateCodesAsync()
    {
        return await _bankAccountService.GetUSStateCodesAsync();
    }

    /// <inheritdoc/>
    public async Task<IReadOnlyList<SelectOption>> GetCanadianStateCodesAsync()
    {
        return await _bankAccountService.GetCanadianStateCodesAsync();
    }

    public async Task SavePaymentStateToSessionAsync(PaymentState model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectedPaymentDetail = model;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    public async Task UpdatePaymentSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.IsAmountCalculatedForSession = true;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }
    public async Task<PaymentState?> GetPaymentStateFromSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer?.SelectedPaymentDetail;
    }

}
