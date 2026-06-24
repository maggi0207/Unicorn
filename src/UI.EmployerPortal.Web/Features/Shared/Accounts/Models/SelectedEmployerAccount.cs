
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

/// <summary>
/// Session Model for storing the currently selected employer account.
/// Used to persist the selected account across page navigations.
/// </summary>
public sealed record SelectedEmployerAccount : ISessionModel
{
    /// <summary>
    /// Gets or sets the selected employer account. 
    /// </summary>
    public EmployerAccount? EmployerAccount { get; init; }

    /// <summary>
    /// Gets or sets the selected missing report.
    /// Cleared automatically when the account is changed.
    /// </summary>
    public MissingReportModel? SelectedMissingReport { get; set; }

    /// <summary>
    /// Gets or sets the pending adjustment report selected for continuation.
    /// </summary>
    public PendingAdjustmentReportModel? SelectedPendingAdjustment { get; set; }
    /// <summary>
    /// Pay AMount
    /// </summary>
    public ReimbursableBillingDetail? SelectPayment { get; set; }
}
