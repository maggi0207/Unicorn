using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;
/// <summary>
/// 
/// </summary>
public class VoluntaryContributionIneligibe
{
    /// <summary>
    /// Whether the employer is eligible for first quarter deferral election.
    /// </summary>
    public bool? Eligible { get; set; } = false;
    /// <summary>
    /// Show Voluntary Information Based Dates
    /// </summary>
    public bool? ShowVCInfo { get; set; } = false;

    /// <summary>
    /// Any rule violations returned from the service.
    /// </summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
}


