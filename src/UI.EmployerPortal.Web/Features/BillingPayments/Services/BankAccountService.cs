using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.EFTPaymentService;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

internal class BankAccountService : IBankAccountService
{
    private const int AccountTypeChecking = 1;
    private const int AccountTypeSavings = 2;

    private readonly IEFTPaymentService _eftPaymentService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<BankAccountService> _retryPolicy;

    public BankAccountService(
        IEFTPaymentService eftPaymentService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<BankAccountService> retryPolicy)
    {
        _eftPaymentService = eftPaymentService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    public async Task<string?> CheckRoutingNumberAsync(string routingNumber)
    {
        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.CheckBankRoutingNumberAsync(routingNumber);
            });

            return response?.BankName;
        }
        catch (CommunicationException)
        {
            return null;
        }
    }


    /// <inheritdoc/>
    public async Task<SaveBankAccountResult> InactivateBankAccountAsync(int bankAccountSk, int employerAccountSk)
    {
        try
        {
            var secureUserSk = _userAccountService.GetUserSKClaim();

            var request = new BankAccountInactivateRequest
            {
                BankAccountSK = bankAccountSk,
                EmployerSK = employerAccountSk,
                SecureUserSK = secureUserSk
            };

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.InactivateBankAccountAsync(request);
            });

            if (response is null)
            {
                return new SaveBankAccountResult(false, "Unable to remove bank account. Please try again .");
            }
            if (!response.Success)
            {
                var message = response.RuleViolations is not null && response.RuleViolations.Length > 0 ? response.RuleViolations[0].RuleViolation ?? "Unable to remove Bank Account. Please try again." : "Unable to remove Bank Account. Please try again.";

                return new SaveBankAccountResult(false, message);
            }
            return new SaveBankAccountResult(true, string.Empty);
        }
        catch (Exception)
        {
            return new SaveBankAccountResult(false, "Unable to remove bank account. Please try again.");
        }
    }

    public async Task<SaveBankAccountResult> SaveBankAccountAsync(BankAccountModel model, int employerAccountSk)
    {
        try
        {
            var accountNumberUnchanged = !string.IsNullOrWhiteSpace(model.OriginalMaskedAccountNumber)
                && model.AccountNumber == model.OriginalMaskedAccountNumber;

            var request = new BankAccountRequest
            {
                SecureUserSK = _userAccountService.GetUserSKClaim(),
                EmployerSK = employerAccountSk,
                NickName = model.Nickname,
                RoutingNumber = model.RoutingNumber,
                AccountNumber = accountNumberUnchanged ? null : model.AccountNumber,
                AccountType = MapAccountType(model.AccountType),
                OffshoreFlag = model.IsInternational,
                IATStreetAddress = model.IatStreetAddress,
                IATCity = model.IatCity,
                IATZip = model.IatPostalCode,
                IATCountryCode = model.IatCountryCode,
                IATState = model.IatCountryIsUsa ? model.IatStateCode : model.IatProvinceCode,
                BankAccountSK = model.BankAccountSk > 0 ? model.BankAccountSk : null
            };

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.SaveBankAccountAsync(request);
            });

            if (response?.RuleViolations != null && response.RuleViolations.Length > 0)
            {
                var firstViolation = response.RuleViolations[0].RuleViolation ?? "Save failed. Please try again.";
                return new SaveBankAccountResult(false, firstViolation);
            }

            return new SaveBankAccountResult(true);
        }
        catch (CommunicationException)
        {
            return new SaveBankAccountResult(false, "Service is temporarily unavailable. Please try again.");
        }
        catch (Exception)
        {
            return new SaveBankAccountResult(false, "An unexpected error occurred. Please try again.");
        }
    }

    public async Task<IReadOnlyList<SavedBankAccount>> GetExistingAccountsAsync(int employerAccountSk)
    {
        try
        {
            var secureUserSk = _userAccountService.GetUserSKClaim();

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.LoadActiveBankAccountAsync(employerAccountSk, secureUserSk);
            });

            if (response?.BankAccounts == null || response.BankAccounts.Length == 0)
            {
                return [];
            }

            var accounts = new List<SavedBankAccount>();

            foreach (var account in response.BankAccounts)
            {
                accounts.Add(new SavedBankAccount
                {
                    Nickname = account.Nickname ?? string.Empty,
                    RoutingNumber = account.RoutingNumber ?? string.Empty,
                    MaskedAccountNumber = account.AccountNumberMasked ?? string.Empty,
                    BankName = account.FederalBankName ?? string.Empty,
                    AccountType = MapAccountTypeToString(account.AccountType),
                    BankAccountSk = account.BankAccountSK
                });
            }

            return accounts;
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    public async Task<BankAccountModel?> GetBankAccountForEditAsync(int bankAccountSk, int employerAccountSk)
    {
        try
        {
            var secureUserSk = _userAccountService.GetUserSKClaim();

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.ObtainEFTBankAccountAsync(employerAccountSk, bankAccountSk, secureUserSk);
            });

            var account = response?.BankAccounts?.FirstOrDefault();

            if (account is null)
            {
                return null;
            }

            var masked = account.AccountNumberMasked ?? string.Empty;

            return new BankAccountModel
            {
                BankAccountSk = account.BankAccountSK,
                Nickname = account.Nickname,
                RoutingNumber = account.RoutingNumber,
                AccountNumber = masked,
                ConfirmAccountNumber = masked,
                OriginalMaskedAccountNumber = masked,
                BankName = account.FederalBankName,
                AccountType = MapAccountTypeToString(account.AccountType),
                IsInternational = account.OffshoreFlag,
                IatCountryCode = account.IATCountryCode,
                IatStreetAddress = account.IATStreetAddress,
                IatCity = account.IATCity,
                IatPostalCode = account.IATZip,
                IatStateCode = account.IATState,
                IatProvinceCode = account.IATState
            };
        }
        catch (CommunicationException)
        {
            return null;
        }
        catch (Exception)
        {
            return null;
        }
    }

    public async Task<IReadOnlyList<PendingPayment>> GetPendingPaymentsAsync(int bankAccountSk, int employerAccountSk)
    {
        try
        {
            var secureUserSk = _userAccountService.GetUserSKClaim();

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.ObtainPendingPaymentsAsync(employerAccountSk, bankAccountSk, secureUserSk);
            });

            return response?.PendingPayments == null || response.PendingPayments.Length == 0
                ? []
                : (IReadOnlyList<PendingPayment>) response.PendingPayments
                .Select(p =>
                {
                    return new PendingPayment
                    {
                        SettlementDate = p.SettlementDate ?? DateTime.MinValue,
                        ConfirmationNumber = p.ConfirmationID ?? string.Empty,
                        Amount = p.Amount ?? 0m
                    };
                })
                .ToList();
        }
        catch (CommunicationException)
        {
            return [];
        }
        catch (Exception)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<BankCountryOption>> GetCountryCodesAsync()
    {
        try
        {
            var result = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.GetCountryCodesAsync();
            });

            return result?.Select(c =>
            {
                return new BankCountryOption(
                                    Value: c.CodeSK.ToString(),
                                    Text: c.LongDescription,
                                    ShortCode: c.ShortDescription ?? string.Empty);
            })
                .ToList() ?? [];
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SelectOption>> GetUSStateCodesAsync()
    {
        try
        {
            var result = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.GetUSStateCodesAsync();
            });

            return result?.Select(s =>
            {
                return new SelectOption { Value = s.CodeSK.ToString(), Text = s.LongDescription };
            }).ToList()
                ?? [];
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    public async Task<IReadOnlyList<SelectOption>> GetCanadianStateCodesAsync()
    {
        try
        {
            var result = await _retryPolicy.ExecuteAsync(() =>
            {
                return _eftPaymentService.GetCanadianStateCodesAsync();
            });

            return result?.Select(p =>
            {
                return new SelectOption { Value = p.CodeSK.ToString(), Text = p.LongDescription };
            }).ToList()
                ?? [];
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    private static int MapAccountType(string? accountType)
    {
        return string.Equals(accountType, "Savings", StringComparison.OrdinalIgnoreCase) ? AccountTypeSavings : AccountTypeChecking;
    }

    private static string MapAccountTypeToString(int accountType)
    {
        return accountType == AccountTypeSavings ? "Savings" : "Checking";
    }
}
