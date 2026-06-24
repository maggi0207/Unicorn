using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.Dashboard.Components;

/// <summary>
/// GuestDashboardHeader
/// </summary>
public partial class GuestDashboardHeader
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    private bool _showUserInformation = false;
    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var sessionAllEmployerAccounts = await SessionManager.GetAsync<SessionAllEmployerAccounts>();
        if (sessionAllEmployerAccounts == null || sessionAllEmployerAccounts?.EmployerAccounts?.Count == 0)
        {
            var path = GetUrlPath();
            if (path.Equals("guest-dashboard") || path.Equals("employer-dashboard"))
            {
                _showUserInformation = true;
            }
        }
    }
    private string GetAuthenticatedUserInformation()
    {
        return $"{UserAccountService.GetFirstName()} {UserAccountService.GetLastName()}";
    }

    private string GetUrlPath()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();

        if (relativePath.Contains("#"))
        {
            relativePath = relativePath.Split('#')[0];
        }

        return relativePath.Split('?')[0].TrimEnd('/');
    }
}
