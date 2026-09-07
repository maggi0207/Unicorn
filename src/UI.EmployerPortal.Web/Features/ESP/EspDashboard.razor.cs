using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

namespace UI.EmployerPortal.Web.Features.ESP;

/// <summary>
/// 
/// </summary>
public partial class EspDashboard
{
    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    private bool IsEspUser()
    {
        return UserAccountService.IsEspUser();
    }

    private bool IsEspUserManager()
    {
        return UserAccountService.IsEspUserManager();
    }
}
