using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Services;

internal class DownloadTransactionService : IDownloadTransactionService
{
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<DownloadTransactionService> _retryPolicy;
    private readonly ISessionManager _sessionManager;

    public DownloadTransactionService(
        IPortalUtilityService portalUtilityService,
        IUserAccountService userAccountService,
        ISessionManager sessionManager,
        IAsyncRetryPolicy<DownloadTransactionService> retryPolicy)
    {
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
        _sessionManager = sessionManager;
        _retryPolicy = retryPolicy;
    }

    public async Task<BenefitWeekDateRangesResponse> GetBenefitWeekDateRangesAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.GetMostCurrentDateRangesAsync();
        });

        return response;
    }

    public int GetSecureUserSK()
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();
        return secureUserSK;
    }

    public async Task<int> GetEmployerSK()
    {
        var selectedAccount = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var employerSk = selectedAccount?.EmployerAccount?.Id ?? 0;
        return employerSk;
    }

    public async Task<BenefitChargesAndAdjustmentsFileResponse> DownloadBenefitChargesAndAdjustmentsAsync(DownloadReportRequestModel request)
    {
        var req = new DownloadBenefitChargesAndAdjustmentsRequest()
        {
            EmployerSK = request.EmployerSK,
            FileExtension = request.FileExtension,
            FromDate = request.FromDate,
            HighSSN = request.HighSSN,
            LowSSN = request.LowSSN,
            SecureUserSK = request.SecureUserSK,
            ToDate = request.ToDate
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.DownloadBenefitChargesAndAdjustmentsAsync(req);
        });

        return response;
    }


    public async Task<OtherRFBTransactionsFileResponse> DownloadOtherRFBTransactionsAsync(DownloadReportRequestModel request)
    {
        var req = new DownloadOtherRFBTransactionsRequest
        {
            EmployerSK = request.EmployerSK,
            FileExtension = request.FileExtension,
            FromDate = request.FromDate,
            SecureUserSK = request.SecureUserSK,
            ToDate = request.ToDate
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.DownloadOtherRFBTransactionsAsync(req);
        });

        return response;
    }
}
