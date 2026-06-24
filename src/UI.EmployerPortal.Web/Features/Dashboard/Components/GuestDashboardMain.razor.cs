using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.Dashboard.Components;


/// <summary>
/// 
/// </summary>
public partial class GuestDashboardMain
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _isListView = true;
    private void SetView(bool isListView)
    {
        _isListView = isListView;
    }

    private async Task NavigateHere(string linkClicked)
    {
        NavigationManager.NavigateTo(linkClicked, true);
    }
}

