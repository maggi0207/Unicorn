using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using WcfEmployerRequest = UI.EmployerPortal.Generated.ServiceClients.BillDetailService.EmployerRequest;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// Provides billing detail data for the Taxable Billing Detail page.
/// </summary>
public interface IBillingDetailService
{
    /// <summary>
    /// Returns the taxable billing detail view model for the given employer.
    /// </summary>
    Task<BillingDetailViewModel?> GetTaxableBillingDetail(WcfEmployerRequest request);

    /// <summary>
    /// Returns the Credit Rows for the given employer.
    /// and filtering rows where BillTypeDescription is "Credit"
    /// </summary>
    Task<List<BillLineItem>> GetCreditRowsAsync(int employerSK, int secureUserSK);
}
