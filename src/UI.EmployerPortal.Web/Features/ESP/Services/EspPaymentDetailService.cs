using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <inheritdoc/>
internal sealed class EspPaymentDetailService : IEspPaymentDetailService
{
    private const int EspContactTypeCodeSk = 4;

    private readonly IESPService _espService;
    private readonly IESPBankAccountService _espBankAccountService;
    private readonly IESPPaymentACHContactService _espContactService;
    private readonly IUserAccountService _userAccountService;

    /// <summary>Initializes a new instance of <see cref="EspPaymentDetailService"/>.</summary>
    public EspPaymentDetailService(
        IESPService espService,
        IESPBankAccountService espBankAccountService,
        IESPPaymentACHContactService espContactService,
        IUserAccountService userAccountService)
    {
        _espService = espService;
        _espBankAccountService = espBankAccountService;
        _espContactService = espContactService;
        _userAccountService = userAccountService;
    }

    /// <inheritdoc/>
    public async Task<EspPaymentDetailModel?> GetPaymentDetailAsync(int eftPaymentSk)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        try
        {
            var paymentTask = _espService.LoadESPPaymentAsync(new ObtainEESPPaymentRequest
            {
                EFTPaymentSK = eftPaymentSk,
                SecureUserSK = secureUserSk
            });
            var historyTask = _espService.LoadESPPaymentHistoryAsync(new ObtainEESPPaymentHistoryRequest
            {
                EFTPaymentSK = eftPaymentSk,
                SecureUserSK = secureUserSk
            });

            var payment = (await paymentTask)?.EFTPayment;
            if (payment is null)
            {
                return null;
            }

            // Bank accounts and web contact belong to the ESP itself (keyed by secureUserSk), not to the
            // individual client employer the payment was made for - the same services ESPMakePaymentAchInformation
            // uses to build the payment in the first place. Looking these up through Billing's employer-scoped
            // services with the client's CommonClientSK trips the backend's "WebUserAccess" rule violation,
            // since the ESP user has no direct permission grant on that client employer, which sends the page to
            // Access Denied.
            var bankAccountsTask = _espBankAccountService.GetExistingAccountsAsync();
            var contactTask = _espContactService.GetESPWebContact(secureUserSk, EspContactTypeCodeSk);

            await Task.WhenAll(historyTask, bankAccountsTask, contactTask);

            var bankAccount = bankAccountsTask.Result.FirstOrDefault(a =>
            {
                return a.BankAccountSk == payment.BankAccountSK;
            });

            var activityHistory = historyTask.Result?.EFTPayments is { } activities
                ? (IReadOnlyList<EspPaymentActivityItem>) activities
                    .Select(a =>
                    {
                        return new EspPaymentActivityItem
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

            return new EspPaymentDetailModel
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
