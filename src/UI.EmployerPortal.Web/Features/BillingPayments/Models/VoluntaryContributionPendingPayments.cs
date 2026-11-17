using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;
/// <summary>
/// 
/// </summary>
public class VoluntaryContributionPendingPayments
{
    /// <summary>
    /// Pending voluntary contribution payment  for an employer
    /// </summary>
    public decimal PendingAmount { get; set; }

    /// <summary>
    /// Any rule violations returned from the service.
    /// </summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
}


