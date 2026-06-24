using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ManageAccount.Components;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// EditUserPermissions
/// </summary>
public partial class EditUserPermissions
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// WebUserSecuritySK
    /// </summary>
    [Parameter]
    public int WebUserSecuritySK { get; set; }

    /// <summary>
    /// Mode — "remove" to show the remove-user confirmation view; otherwise edit view
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "mode")]
    public string? Mode { get; set; }

    private bool IsRemoveMode => Mode == "remove" || _user?.Role == "Manager";

    private Permissions? _tree;
    private AccountUserModel? _user;
    private List<PermissionGroup> _groups = [];
    private HashSet<int> _initialSelection = [];
    private int _employerSK = 0;

    private bool _isLoading = true;
    private bool _loadError = false;
    private bool _isSaving = false;
    private string? _saveError = null;
    private List<string> _validationErrors = [];

    private bool _showNoPermModal = false;
    private bool _showRemoveModal = false;
    private bool _isRemoving = false;
    private string? _removeError = null;

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;
            if (_employerSK == 0)
            {
                _loadError = true;
                return;
            }

            var users = await AccountUserService.GetAccountUsersAsync(_employerSK);
            _user = users.FirstOrDefault(u =>
            {
                return u.WebUserSecuritySK == WebUserSecuritySK;
            });

            if (_user == null)
            {
                _loadError = true;
                return;
            }

            // Skip catalog load when showing the remove-user confirmation view
            if (IsRemoveMode)
            {
                return;
            }

            // Full catalog of all available permissions (so manager can ADD permissions)
            _groups = await AccountUserService.GetAllPermissionsAsync();

            // The worker's currently-assigned permissions — flat list of SKs (so we can pre-check them)
            var assignedSKs = await AccountUserService.GetWorkerAssignedControlSKsAsync(_employerSK, _user.SecureUserSK);
            _initialSelection = [.. assignedSKs];

            // Pre-check items / groups in the catalog based on the worker's existing assignment
            foreach (var group in _groups)
            {
                foreach (var item in group.Items)
                {
                    item.IsSelected = _initialSelection.Contains(item.WebControlSK);
                }
                group.IsSelected = group.Items.Count > 0 && group.Items.All(i =>
                {
                    return i.IsSelected;
                });
            }
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

    private void HandleSelectionChanged()
    {
        _saveError = null;
        _validationErrors = [];
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-users");
    }

    private async Task HandleSaveAsync()
    {
        if (_tree == null || _user == null)
        {
            return;
        }

        var currentSelection = new HashSet<int>(_tree.GetSelectedWebControlSKs());

        // Show the "No Permissions Selected" modal only when the user has cleared everything.
        // Otherwise save the current state, even if nothing was changed.
        if (currentSelection.Count == 0)
        {
            _showNoPermModal = true;
            return;
        }

        // Dependency-rule validation (see Permissions.Validate)
        _validationErrors = _tree.Validate();
        if (_validationErrors.Count > 0)
        {
            await ScrollToTopAsync();
            return;
        }

        _isSaving = true;
        _saveError = null;

        var (success, message) = await AccountUserService.UpdateUserPermissionsAsync(
            _employerSK,
            _user.WebUserSecuritySK,
            [.. currentSelection]);

        _isSaving = false;

        if (success)
        {
            var name = Uri.EscapeDataString($"{_user.FirstName} {_user.LastName}");
            Navigation.NavigateTo($"manage-account/account-users?flash=permissions-updated&name={name}");
        }
        else
        {
            _saveError = string.IsNullOrWhiteSpace(message) ? "Unable to update permissions." : message;
            await ScrollToTopAsync();
        }
    }

    private async Task ScrollToTopAsync()
    {
        await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
    }

    private void CloseNoPermModal()
    {
        _showNoPermModal = false;
    }

    private void ContinueFromNoPermModal()
    {
        _showNoPermModal = false;
        Navigation.NavigateTo("manage-account/account-users");
    }

    private void OpenRemoveModal()
    {
        _removeError = null;
        _showRemoveModal = true;
    }

    private void CloseRemoveModal()
    {
        if (_isRemoving)
        {
            return;
        }
        _showRemoveModal = false;
        _removeError = null;
    }

    private async Task ConfirmRemoveAsync()
    {
        if (_user == null)
        {
            return;
        }

        _isRemoving = true;
        _removeError = null;

        var (success, message) = await AccountUserService.DeleteUserAsync(_employerSK, _user.WebUserSecuritySK);

        _isRemoving = false;

        if (success)
        {
            var name = Uri.EscapeDataString($"{_user.FirstName} {_user.LastName}");
            Navigation.NavigateTo($"/manage-account/account-users?flash=user-removed&name={name}");
        }
        else
        {
            _removeError = string.IsNullOrWhiteSpace(message) ? "Unable to remove user." : message;
        }
    }
}
