using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Services;


/// <summary>
/// DownloadTransactionService
/// </summary>
internal interface IDownloadTransactionService
{
    Task<BenefitWeekDateRangesResponse> GetBenefitWeekDateRangesAsync();
    int GetSecureUserSK();
    Task<int> GetEmployerSK();
    Task<BenefitChargesAndAdjustmentsFileResponse> DownloadBenefitChargesAndAdjustmentsAsync(DownloadReportRequestModel request);
    Task<OtherRFBTransactionsFileResponse> DownloadOtherRFBTransactionsAsync(DownloadReportRequestModel request);
}
