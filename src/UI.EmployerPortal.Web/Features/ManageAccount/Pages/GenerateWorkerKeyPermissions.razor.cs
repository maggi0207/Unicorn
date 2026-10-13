using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ManageAccount.Components;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// GenerateWorkerKeyPermissions
/// </summary>
public partial class GenerateWorkerKeyPermissions
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;

    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private IGenerateAccessKeyFlow Flow { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private Permissions? _tree;
    private List<PermissionGroup> _groups = [];
    private int _employerSK = 0;
    private int _managerWebUserSecuritySK = 0;
    private int _workerRoleCode = 0;

    private bool _isLoading = true;
    private bool _loadError = false;
    private bool _isCurrentUserManager = false;
    private bool _isWorking = false;
    private bool _showPermissionValidationError = false;
    private string? _errorMessage = null;
    private List<string> _validationErrors = [];

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        if (Flow.SelectedKeyType != "worker")
        {
            Navigation.NavigateTo("manage-account/account-users/generate-key");
            return;
        }

        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;
            if (_employerSK == 0)
            {
                _loadError = true;
                return;
            }

            // Only managers may generate access keys - re-checked here so the step cannot be
            // reached directly, or left open after switching to a worker-access account.
            _isCurrentUserManager = await IsEmployerManagerAsync();
            if (!_isCurrentUserManager)
            {
                return;
            }

            var users = await AccountUserService.GetAccountUsersAsync(_employerSK);
            var currentUserSK = AccountUserService.GetCurrentUserSk();
            _managerWebUserSecuritySK = users
                .FirstOrDefault(u =>
                {
                    return u.SecureUserSK == currentUserSK;
                })?.WebUserSecuritySK ?? 0;

            (_, _workerRoleCode) = await AccountUserService.GetRoleCodesAsync();
            _groups = await AccountUserService.GetAllPermissionsAsync();
        }
        catch
        {
            _loadError = true;
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

    private void HandleSelectionChanged()
    {
        _showPermissionValidationError = false;
        _errorMessage = null;
        _validationErrors = [];
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-users/generate-key");
    }

    private async Task HandleGenerateKeyAsync()
    {
        if (_tree == null)
        {
            return;
        }

        if (!_tree.HasAnySelection())
        {
            _showPermissionValidationError = true;
            _errorMessage = "You must select at least one permission before generating a key.";
            await ScrollToTopAsync();
            return;
        }

        // Dependency-rule validation (see Permissions.Validate)
        _validationErrors = _tree.Validate();
        if (_validationErrors.Count > 0)
        {
            await ScrollToTopAsync();
            return;
        }

        _isWorking = true;
        _errorMessage = null;
        _showPermissionValidationError = false;

        var selectedSKs = _tree.GetSelectedWebControlSKs();
        var (success, keyOrError) = await AccountUserService.GenerateAccessKeyAsync(
            _employerSK,
            _managerWebUserSecuritySK,
            _workerRoleCode,
            selectedSKs);

        _isWorking = false;

        if (success)
        {
            Flow.SelectedWebControlSKs = selectedSKs;
            Flow.GeneratedKey = keyOrError;
            Navigation.NavigateTo("manage-account/account-users/generate-key/result");
        }
        else
        {
            _errorMessage = string.IsNullOrWhiteSpace(keyOrError) ? "Unable to generate access key." : keyOrError;
            await ScrollToTopAsync();
        }
    }

    private async Task ScrollToTopAsync()
    {
        await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
    }
}
