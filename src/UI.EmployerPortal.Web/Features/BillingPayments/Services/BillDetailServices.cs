using UI.EmployerPortal.Generated.ServiceClients.BillDetailService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using EmployerRequest = UI.EmployerPortal.Generated.ServiceClients.BillDetailService.EmployerRequest;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;
/// <summary>
/// 
/// </summary>
public interface IBillDetailServices
{
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<ReimbursableBillingDetail?> GetReimburseBillingDetail(EmployerRequest request);

}
internal class BillDetailServices : IBillDetailServices
{
    private readonly IAsyncRetryPolicy<BillDetailServices> _retryPolicy;
    private readonly IBillDetailService _billDetailService;

    private readonly IUserAccountService _userAccountService;
    public BillDetailServices(
         IAsyncRetryPolicy<BillDetailServices> retryPolicy,
        IBillDetailService billDetailService,
        IUserAccountService userAccountService)

    {
        _retryPolicy = retryPolicy;
        _billDetailService = billDetailService;
        _userAccountService = userAccountService;

    }

    public async Task<ReimbursableBillingDetail?> GetReimburseBillingDetail(EmployerRequest request)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _billDetailService.ReimbursableBillingDetailAsync(request);


        });
        return response?.RuleViolations == null ? (ReimbursableBillingDetail?) null : MapreimburseDetailtoModel(response);
        ;
    }
    private static ReimbursableBillingDetail MapreimburseDetailtoModel(BillDetailSummaryReimbursableResponse response)
    {
        return new ReimbursableBillingDetail
        {
            AmountToPay = response.AmountToPay,
            TotalEFTPayments = response.TotalEFTPayments,
            TotalOutstandingBalance = response.TotalOutstandingBalance
        };
    }

}


