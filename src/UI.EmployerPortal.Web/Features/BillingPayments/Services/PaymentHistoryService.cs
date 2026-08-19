using System.ServiceModel;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// Loads and maps EFT payment history for the Payment History page.
/// </summary>
public interface IPaymentHistoryService
{
    /// <summary>
    /// Returns all EFT payments for the currently selected employer,
    /// with per-row current-user and eligibility flags resolved.
    /// Returns null on service failure.
    /// </summary>
    Task<IReadOnlyList<PaymentHistoryItem>?> GetPaymentHistoryAsync();
}

internal sealed class PaymentHistoryService : IPaymentHistoryService
{
    private readonly IEftPaymentService _eftPaymentService;
    private readonly IUserAccountService _userAccountService;
    private readonly ISessionManager _sessionManager;

    public PaymentHistoryService(
        IEftPaymentService eftPaymentService,
        IUserAccountService userAccountService,
        ISessionManager sessionManager)
    {
        _eftPaymentService = eftPaymentService;
        _userAccountService = userAccountService;
        _sessionManager = sessionManager;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<PaymentHistoryItem>?> GetPaymentHistoryAsync()
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var employerSk = selected?.EmployerAccount?.Id;
        if (employerSk is null)
        {
            return null;
        }

        var currentUserSk = _userAccountService.GetUserSKClaim();
        var today = DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var response = await _eftPaymentService.LoadCardEftPaymentHistoryAsync(employerSk.Value, currentUserSk);

            return response?.RuleViolations is { Length: > 0 }
                ? null
                : response?.Payments is null
                ? []
                : (IReadOnlyList<PaymentHistoryItem>) response.Payments
                .Select(p =>
                {
                    var settlementDate = DateOnly.FromDateTime(p.SettlementDate);

                    var isCurrentUser = p.SecureUserSK == currentUserSk;
                    var isAchDebit = string.Equals(p.PaymentType, "ACH Debit", StringComparison.OrdinalIgnoreCase);
                    var isPending = string.Equals(p.Status, "Pending", StringComparison.OrdinalIgnoreCase);
                    var isFutureSettlement = settlementDate > today;

                    return new PaymentHistoryItem
                    {
                        EftPaymentSk = p.PaymentSK,
                        PaymentType = p.PaymentType ?? string.Empty,
                        SettlementDate = settlementDate,
                        Amount = p.Amount,
                        ServiceFee = p.ServiceFeeAmount,
                        ConfirmationId = p.ConfirmationID ?? string.Empty,
                        Status = p.Status ?? string.Empty,
                        SecureUserSk = p.SecureUserSK,
                        IsCurrentUser = isCurrentUser,
                        IsEligibleForAction = isCurrentUser && isAchDebit && isPending && isFutureSettlement
                    };
                })
                .ToList();
        }
        catch (CommunicationException)
        {
            return null;
        }
    }
}
