using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
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
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IGenerateAccessKeyFlow Flow { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;
    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private string? SelectedValue => _keyTypeOptions[_selectedIndex];
    private int _selectedIndex;
    private readonly List<string> _keyTypeOptions = new()
    {
            "worker",
            "manager"
    };

    private bool _isLoading = true;
    private bool _hasError = false;
    private bool _isCurrentUserManager = false;
    private bool _isWorking = false;
    private string? _errorMessage = null;
    private bool _isEspUser;

    private int _employerSK = 0;
    private int _managerWebUserSecuritySK = 0;
    private int _managerRoleCode = 0;
    private int _workerRoleCode = 0;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        Flow.Reset();
        try
        {
            var isEspUser = await UserAccountService.IsEspUserAsync();
            _isEspUser = isEspUser;

            if (!isEspUser)
            {
                var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
                _employerSK = selected?.EmployerAccount?.Id ?? 0;
            }

            // Only managers may generate access keys. The check runs on arrival - not on submit -
            // so switching to an account the user only has worker access for immediately replaces
            // the key type cards with the access denied banner.
            _isCurrentUserManager = isEspUser
                ? await UserAccountService.IsEspUserManagerAsync()
                : await IsEmployerManagerAsync();
            if (!_isCurrentUserManager)
            {
                return;
            }

            if (!isEspUser)
            {
                var users = await AccountUserService.GetAccountUsersAsync(_employerSK);
                var currentUserSK = AccountUserService.GetCurrentUserSk();
                _managerWebUserSecuritySK = users
                    .FirstOrDefault(u =>
                    {
                        return u.SecureUserSK == currentUserSK;
                    })?.WebUserSecuritySK ?? 0;
            }
            (_managerRoleCode, _workerRoleCode) = await AccountUserService.GetRoleCodesAsync();
        }
        catch
        {
            _hasError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task<bool> IsEmployerManagerAsync()
    {
        var user = UserAccountService.GetCurrentUser();
        if (user is null)
        {
            return false;
        }

        var result = await AuthorizationService.AuthorizeAsync(user, null, AuthorizationPolicies.RequiresManagerRole);
        return result.Succeeded;
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-users");
    }


    private void HandleCardSelected(int index)
    {
        _selectedIndex = index;
        _errorMessage = null;
    }


    private async Task HandleNextCard()
    {
        if (_keyTypeOptions.Count == 0)
        {
            return;
        }
        // Move down or wrap to beginning
        HandleCardSelected((_selectedIndex + 1) % _keyTypeOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task HandlePreviousCard()
    {
        if (_keyTypeOptions.Count == 0)
        {
            return;
        }
        // Move up or wrap to end
        HandleCardSelected((_selectedIndex - 1 + _keyTypeOptions.Count) % _keyTypeOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task FocusOnCard(int index)
    {
        await JSRuntime.InvokeVoidAsync("focusElement", $"reporting-card-{index}");
    }

    private async Task HandleContinueAsync()
    {
        if (string.IsNullOrEmpty(SelectedValue))
        {
            _errorMessage = "You must select a key type to generate.";
            return;
        }

        Flow.SelectedKeyType = SelectedValue;
        var espUserSk = await UserAccountService.GetEspUserSKAsync();
        var espRoleCode = _managerRoleCode;
        if (SelectedValue == "worker")
        {
            if (espUserSk == null)
            {
                Navigation.NavigateTo("manage-account/account-users/generate-key/permissions");
                return;
            }
            (_, espRoleCode) = await AccountUserService.GetRoleCodesAsync();
        }

        // Manager: generate the key immediately and navigate to the result page
        _isWorking = true;
        _errorMessage = null;

        var (success, keyOrError) = espUserSk is null ? await AccountUserService.GenerateAccessKeyAsync(
            _employerSK,
            _managerWebUserSecuritySK,
            _managerRoleCode,
            null) : await AccountUserService.GenerateESPAccessKeyAsync(
    espUserSk ?? 0,
    espRoleCode);

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
