using System.ServiceModel;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>Loads all data needed for the Payment Details page.</summary>
public interface IPaymentDetailService
{
    /// <summary>
    /// Returns full payment detail including payment info, bank info, contact info, and activity history.
    /// Returns null on service failure or when the payment is not found.
    /// </summary>
    Task<PaymentDetailModel?> GetPaymentDetailAsync(int eftPaymentSk);
}

internal sealed class PaymentDetailService : IPaymentDetailService
{
    private readonly IEftPaymentService _eftPaymentService;
    private readonly IBankAccountOrchestrator _bankAccountOrchestrator;
    private readonly IContactInformationService _contactInformationService;
    private readonly IUserAccountService _userAccountService;
    private readonly ISessionManager _sessionManager;

    /// <summary>PaymentDetailService</summary>
    public PaymentDetailService(
        IEftPaymentService eftPaymentService,
        IBankAccountOrchestrator bankAccountOrchestrator,
        IContactInformationService contactInformationService,
        IUserAccountService userAccountService,
        ISessionManager sessionManager)
    {
        _eftPaymentService = eftPaymentService;
        _bankAccountOrchestrator = bankAccountOrchestrator;
        _contactInformationService = contactInformationService;
        _userAccountService = userAccountService;
        _sessionManager = sessionManager;
    }

    /// <inheritdoc />
    public async Task<PaymentDetailModel?> GetPaymentDetailAsync(int eftPaymentSk)
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var employerSk = selected?.EmployerAccount?.Id;
        if (employerSk is null)
        {
            return null;
        }

        var secureUserSk = _userAccountService.GetUserSKClaim();

        try
        {
            var paymentTask = _eftPaymentService.LoadEftPaymentAsync(eftPaymentSk, secureUserSk, employerSk.Value);
            var historyTask = _eftPaymentService.LoadEftPaymentHistoryAsync(eftPaymentSk, secureUserSk, employerSk.Value);
            var bankAccountsTask = _bankAccountOrchestrator.GetExistingAccountsAsync();
            var contactTask = _contactInformationService.GetEmployerWebContact(secureUserSk, employerSk.Value, 4);

            await Task.WhenAll(paymentTask, historyTask, bankAccountsTask, contactTask);
            var paymentResponse = paymentTask.Result;
            if (paymentResponse?.RuleViolations is { Length: > 0 })
            {
                return null;
            }
            var payment = paymentResponse?.EFTPayment;
            if (payment is null)
            {
                return null;
            }

            var bankAccount = bankAccountsTask.Result
                .FirstOrDefault(a =>
                {
                    return a.BankAccountSk == payment.BankAccountSK;
                });

            var activityHistory = historyTask.Result?.EFTPayments is { } activities
                ? (IReadOnlyList<PaymentActivityItem>) activities
                    .Select(a =>
                    {
                        return new PaymentActivityItem
                        {
                            Date = a.Date,
                            Action = a.Action ?? string.Empty,
                            Description = a.Description ?? string.Empty
                        };
                    })
                    .OrderByDescending(a =>
                    {
                        return a.Date;
                    })
                    .ToList()
                : [];

            return new PaymentDetailModel
            {
                ConfirmationNumber = payment.ConfirmationID ?? string.Empty,
                TransactionDateTime = payment.LastSubmitDate,
                Amount = payment.Amount,
                SettlementDate = payment.SettlementDate.HasValue
                    ? DateOnly.FromDateTime(payment.SettlementDate.Value)
                    : null,
                Status = payment.EFTPaymentStatusCodeDescription ?? string.Empty,
                CancellationDate = payment.CancellationDate,
                BankAccount = bankAccount,
                ContactInfo = contactTask.Result,
                ActivityHistory = activityHistory
            };
        }
        catch (CommunicationException)
        {
            return null;
        }
    }
}
