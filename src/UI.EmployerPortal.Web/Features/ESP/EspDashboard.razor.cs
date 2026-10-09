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
    private bool _isEspUser;
    private bool _isEspUserManager;
    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        _isEspUser = await UserAccountService.IsEspUserAsync();
        _isEspUserManager = await UserAccountService.IsEspUserManagerAsync();
    }
}
