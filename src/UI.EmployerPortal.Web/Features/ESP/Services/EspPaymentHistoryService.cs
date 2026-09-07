using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

internal sealed class EspPaymentHistoryService : IEspPaymentHistoryService
{
    private readonly IESPService _espService;
    private readonly IUserAccountService _userAccountService;

    public EspPaymentHistoryService(IESPService espService, IUserAccountService userAccountService)
    {
        _espService = espService;
        _userAccountService = userAccountService;
    }

    /// <inheritdoc />
    public async Task<IReadOnlyList<EspPaymentHistoryItem>?> GetPaymentHistoryAsync()
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();
        var today = DateOnly.FromDateTime(DateTime.Today);

        try
        {
            var response = await _espService.LoadTaxFileEFTPaymentHistoryAsync(secureUserSk);

            return response?.Payments is null
                ? []
                : (IReadOnlyList<EspPaymentHistoryItem>) response.Payments
                .Select(p =>
                {
                    var settlementDate = p.SettlementDate.HasValue
                        ? DateOnly.FromDateTime(p.SettlementDate.Value)
                        : (DateOnly?) null;

                    var isPending = string.Equals(p.EFTPaymentStatusCodeDescription, "Pending", StringComparison.OrdinalIgnoreCase);
                    var isFutureSettlement = settlementDate.HasValue && settlementDate.Value > today;

                    return new EspPaymentHistoryItem
                    {
                        EftPaymentSk = p.EFTPaymentSK ?? 0,
                        SettlementDate = settlementDate,
                        Amount = p.Amount ?? 0m,
                        ConfirmationId = p.ConfirmationID ?? string.Empty,
                        Status = p.EFTPaymentStatusCodeDescription ?? string.Empty,
                        IsEligibleForAction = isPending && isFutureSettlement
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
