using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// GenerateUserKey
/// </summary>
public partial class GenerateUserKey
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;

    [Inject]
    private IGenerateAccessKeyFlow Flow { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private string _selectedKeyType = string.Empty;
    private bool _isWorking = false;
    private string? _errorMessage = null;

    private int _employerSK = 0;
    private int _managerWebUserSecuritySK = 0;
    private int _managerRoleCode = 0;
    private int _workerRoleCode = 0;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        Flow.Reset();

        var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
        _employerSK = selected?.EmployerAccount?.Id ?? 0;

        var users = await AccountUserService.GetAccountUsersAsync(_employerSK);
        var currentUserSK = AccountUserService.GetCurrentUserSk();
        _managerWebUserSecuritySK = users
            .FirstOrDefault(u =>
            {
                return u.SecureUserSK == currentUserSK;
            })?.WebUserSecuritySK ?? 0;

        (_managerRoleCode, _workerRoleCode) = await AccountUserService.GetRoleCodesAsync();
    }

    private void SelectKeyType(string keyType)
    {
        _selectedKeyType = keyType;
        _errorMessage = null;
    }

    private void HandleCardKeyDown(KeyboardEventArgs e, string keyType)
    {
        if (e.Key is "Enter" or " ")
        {
            SelectKeyType(keyType);
        }
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-users");
    }

    private async Task HandleContinueAsync()
    {
        if (string.IsNullOrEmpty(_selectedKeyType))
        {
            _errorMessage = "You must select a key type to generate.";
            return;
        }

        Flow.SelectedKeyType = _selectedKeyType;

        if (_selectedKeyType == "worker")
        {
            Navigation.NavigateTo("manage-account/account-users/generate-key/permissions");
            return;
        }

        // Manager: generate the key immediately and navigate to the result page
        _isWorking = true;
        _errorMessage = null;

        var (success, keyOrError) = await AccountUserService.GenerateAccessKeyAsync(
            _employerSK,
            _managerWebUserSecuritySK,
            _managerRoleCode,
            null);

        _isWorking = false;

        if (success)
        {
            Flow.GeneratedKey = keyOrError;
            Navigation.NavigateTo("manage-account/account-users/generate-key/result");
        }
        else
        {
            _errorMessage = string.IsNullOrWhiteSpace(keyOrError) ? "Unable to generate access key." : keyOrError;
        }
    }
}
