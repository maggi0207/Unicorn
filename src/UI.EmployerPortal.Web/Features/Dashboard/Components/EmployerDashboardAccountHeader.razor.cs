using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

namespace UI.EmployerPortal.Web.Features.Dashboard.Components;

/// <summary>
/// 
/// </summary>
public partial class EmployerDashboardAccountHeader
{
    /// <summary>
    /// Gets or sets the EmployerAccount displayed in the filter bar.
    /// </summary>
    [Parameter]
    public EmployerAccount? EmployerAccount { get; set; }
}
