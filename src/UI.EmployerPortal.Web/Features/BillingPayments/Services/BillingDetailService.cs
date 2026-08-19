using UI.EmployerPortal.Generated.ServiceClients.BillDetailService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using WcfEmployerRequest = UI.EmployerPortal.Generated.ServiceClients.BillDetailService.EmployerRequest;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;
/// <summary>
/// Retrieves taxable billing detail data from the BillDetailService WCF endpoint.
/// </summary>
internal class BillingDetailService : IBillingDetailService
{
    private readonly IAsyncRetryPolicy<BillingDetailService> _retryPolicy;
    private readonly IBillDetailService _billDetailService;
    /// <summary>
    /// Initializes a new instance of <see cref="BillingDetailService"/>.
    /// </summary>
    public BillingDetailService(
        IAsyncRetryPolicy<BillingDetailService> retryPolicy,
        IBillDetailService billDetailService)
    {
        _retryPolicy = retryPolicy;
        _billDetailService = billDetailService;
    }
    /// <inheritdoc/>
    public async Task<BillingDetailViewModel?> GetTaxableBillingDetail(WcfEmployerRequest request)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _billDetailService.TaxableBillingDetailAsync(request);
        });
        return response?.RuleViolations == null ? (BillingDetailViewModel?) null : MapToViewModel(response);
    }

    private static BillingDetailViewModel MapToViewModel(BillDetailSummaryResponse response)
    {
        return new BillingDetailViewModel
        {
            TaxDue = response.TaxDue,
            InterestDue = response.InterestDue,
            PenaltyDue = response.PenaltyDue,
            CollectionCostsDue = response.CollectionCostsDue,
            NsfFeesDue = response.NSFFeesDue,
            AssessmentDue = response.AssessmentDue,
            TotalCredits = response.TotalCredits,
            TotalEftPayments = response.TotalEFTPayments,
            TotalOutstandingBalance = response.TotalOutstandingBalance,
            AmountToPay = response.AmountToPay,
            Bills = (response.Bills ?? []).Select(b =>
            {
                return MapBillItem(b);
            }).ToList(),
            Credits = (response.Credits ?? []).Select(c =>
            {
                return MapBillItem(c);
            }).ToList(),
            RuleViolations = []
        };
    }

    /// <inheritdoc/>
    public async Task<List<BillLineItem>> GetCreditRowsAsync(int employerSK, int secureUserSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _billDetailService.LoadBillsWithBalanceAsync(employerSK, secureUserSK);
        });

        return response?.RuleViolations == null || response.Bills == null
            ? []
            : response.Bills
            .Where(b =>
            {
                return string.Equals(b.BillTypeDescription, "Credit", StringComparison.OrdinalIgnoreCase);
            })
            .Select(b =>
            {
                return MapCreditItem(b);
            })
            .ToList();
    }

    private static BillLineItem MapBillItem(BillDetailSummaryProxy proxy)
    {
        return new BillLineItem
        {
            Year = proxy.Year,
            Quarter = proxy.Quarter,
            BillTypeDescription = proxy.BillTypeDescription ?? string.Empty,
            DueDate = proxy.DueDate,
            Balance = proxy.Balance,
            BalanceWithoutInterest = proxy.BalanceWithoutInterest,
            InterestDueAmount = proxy.InterestDueAmount
        };
    }
    private static BillLineItem MapCreditItem(BillVeneerProxy proxy)
    {
        return new BillLineItem
        {
            Year = proxy.Year,
            Quarter = proxy.Quarter,
            BillTypeDescription = proxy.BillTypeDescription ?? string.Empty,
            DueDate = proxy.DueDate,
            Balance = proxy.Balance,
            BalanceWithoutInterest = proxy.Balance
        };
    }
}
