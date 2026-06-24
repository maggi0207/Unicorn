using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
namespace UI.EmployerPortal.Web.Features.Dashboard.Components;

/// <summary>
/// EmployerDashboardMain
/// </summary>
public partial class EmployerDashboardMain
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isListView = true;
    private void SetView(bool isListView)
    {
        _isListView = isListView;
    }
    /// <summary>
    /// Gets or sets the title displayed in the filter bar.
    /// </summary>
    [Parameter]
    public EmployerAccount? EmployerAccount { get; set; }

    private async Task NavigateHere(string linkClicked)
    {
        NavigationManager.NavigateTo(linkClicked, true);
    }
}
