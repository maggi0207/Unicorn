using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.Dashboard.Pages;


/// <summary>
/// EmployerDashboardMain
/// </summary>
public partial class GuestAccessDashboard
{
    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await DashboardOrchestrator.RemoveEmployerAccountFromSession();
            StateHasChanged();
        }
    }
}
