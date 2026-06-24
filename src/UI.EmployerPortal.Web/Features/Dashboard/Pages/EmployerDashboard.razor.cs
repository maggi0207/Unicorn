using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

namespace UI.EmployerPortal.Web.Features.Dashboard.Pages;


/// <summary>
/// Landing page component that displays the employer account.
/// Serves as the main entry point for users to select an employer account. 
/// </summary>
public partial class EmployerDashboard
{

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;


    private EmployerAccount? _employerAccount;

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _employerAccount = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
            StateHasChanged();
        }
    }
}
