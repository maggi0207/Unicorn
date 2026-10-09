using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// UserAccounts
/// </summary>
public partial class UserAccounts
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private List<AccountUserModel> _allUsers = [];
    private bool _isLoading = true;
    private bool _hasError = false;
    private string _resultsText = string.Empty;

    private int _managerWebUserSecuritySK = 0;
    private bool _isCurrentUserManager = false;

    private int _employerSK = 0;

    private string? _flashTitle = null;
    private string? _flashMessage = null;
    private int _flashKey = 0;

    private bool _showRemoveModal = false;
    private bool _isRemoving = false;
    private string? _removeError = null;
    private AccountUserModel? _userToRemove = null;

    private string _searchText = string.Empty;
    private bool _roleDropdownOpen = false;
    private readonly HashSet<string> _selectedRoles = [];
    private static readonly List<string> AvailableRoles = ["Manager", "Worker"];

    private string RoleDisplayText => _selectedRoles.Count == 0 ? "No Filter Applied" : string.Join(", ", _selectedRoles);

    private int _currentPage = 1;
    private int _pageSize = 10;
    private int PaginationStart => FilteredUsers.Count > 0 ? ((_currentPage - 1) * _pageSize) + 1 : 0;
    private int PaginationEnd => Math.Min(_currentPage * _pageSize, FilteredUsers.Count);
    private int TotalPages => (int) Math.Ceiling((double) FilteredUsers.Count / _pageSize);

    private string _sortColumn = "lastName";
    private bool _sortAscending = true;

    private List<AccountUserModel> FilteredUsers
    {
        get
        {
            var query = _allUsers.AsEnumerable();

            if (_selectedRoles.Count > 0)
            {
                query = query.Where(u =>
                {
                    return _selectedRoles.Contains(u.Role);
                });
            }

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var term = _searchText.Trim();
                query = query.Where(u =>
                {
                    var firstName = u.FirstName ?? string.Empty;
                    var lastName = u.LastName ?? string.Empty;
                    return firstName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                                        lastName.Contains(term, StringComparison.OrdinalIgnoreCase);
                });
            }

            return [.. query];
        }
    }

    private IEnumerable<AccountUserModel> PagedUsers
    {
        get
        {
            var filtered = FilteredUsers;
            var sorted = _sortColumn switch
            {
                "lastName" => SortBy(filtered, u =>
                {
                    return u.LastName;
                }),
                "firstName" => SortBy(filtered, u =>
                {
                    return u.FirstName;
                }),
                "role" => SortBy(filtered, u =>
                {
                    return u.Role;
                }),
                "createdBy" => SortBy(filtered, u =>
                {
                    return u.CreatedBy;
                }),
                "accesskey" => SortBy(filtered, u =>
                {
                    return u.AccessKeyMasked;
                }),
                "accessGranted" => SortBy(filtered, u =>
                {
                    return u.AccessGranted;
                }),
                _ => SortBy(filtered, u =>
                {
                    return u.LastName;
                })
            };

            return sorted
                .Skip((_currentPage - 1) * _pageSize)
                .Take(_pageSize);
        }
    }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        ReadFlashFromUrl();
        await LoadUsersAsync();
    }

    private async Task LoadUsersAsync()
    {
        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;
            if (_employerSK == 0)
            {
                _hasError = true;
                return;
            }
            _allUsers = await AccountUserService.GetAccountUsersAsync(_employerSK);
            var currentUserSK = AccountUserService.GetCurrentUserSk();
            var currentUser = _allUsers.FirstOrDefault(u =>
            {
                return u.SecureUserSK == currentUserSK;
            });
            _managerWebUserSecuritySK = currentUser?.WebUserSecuritySK ?? 0;
            _isCurrentUserManager = currentUser?.Role == "Manager";
            UpdateResultsText();
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

    private void ReadFlashFromUrl()
    {
        var uri = Navigation.ToAbsoluteUri(Navigation.Uri);
        var query = QueryHelpers.ParseQuery(uri.Query);
        if (!query.TryGetValue("flash", out var flash))
        {
            return;
        }

        query.TryGetValue("name", out var name);
        var displayName = name.ToString();

        switch (flash.ToString())
        {
            case "permissions-updated":
                _flashTitle = "Permissions Updated";
                _flashMessage = $"The {displayName}'s permissions have been updated and will take effect immediately.";
                _flashKey++;
                break;
            case "user-removed":
                _flashTitle = "User Removed";
                _flashMessage = $"You've successfully removed {displayName} from this UI account. If you wish to reinstate this user's access you will need to generate a new access key.";
                _flashKey++;
                break;
        }
    }

    private IOrderedEnumerable<AccountUserModel> SortBy<TKey>(
        List<AccountUserModel> source, Func<AccountUserModel, TKey> keySelector)
    {
        return _sortAscending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);
    }

    private void Sort(string column)
    {
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        _currentPage = 1;
    }

    private MarkupString GetSortIcon(string column)
    {
        string path;
        string altText;

        if (_sortColumn == column)
        {
            path = _sortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _sortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
        UpdateResultsText();
    }

    private void ToggleRoleDropdown()
    {
        _roleDropdownOpen = !_roleDropdownOpen;
    }

    private void CloseRoleDropdown()
    {
        _roleDropdownOpen = false;
    }

    private void HandleRoleChange(string role, bool isChecked)
    {
        if (isChecked)
        {
            _selectedRoles.Add(role);
        }
        else
        {
            _selectedRoles.Remove(role);
        }
        _currentPage = 1;
        UpdateResultsText();
    }

    private void ResetRoleFilter()
    {
        _selectedRoles.Clear();
        _roleDropdownOpen = false;
        _currentPage = 1;
        UpdateResultsText();
    }

    private void HandlePageSizeChanged(ChangeEventArgs e)
    {
        _pageSize = int.Parse(e.Value?.ToString() ?? "10");
        _currentPage = 1;
        UpdateResultsText();
    }

    private void FirstPage()
    {
        if (_currentPage > 1)
        {
            _currentPage = 1;
        }
    }

    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
        }
    }

    private void NextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
        }
    }

    private void LastPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage = TotalPages;
        }
    }

    private void UpdateResultsText()
    {
        var total = FilteredUsers.Count;
        _resultsText = total == 0
            ? "0 Users"
            : $"{Math.Min(_pageSize, total)} of {total} Users";
    }

    private void RemoveRoleFilter(string role)
    {
        _selectedRoles.Remove(role);
        _currentPage = 1;
        UpdateResultsText();
    }

    private void NavigateToGenerateKey()
    {
        Navigation.NavigateTo("manage-account/account-users/generate-key");
    }


    private void HandleEditClick(AccountUserModel user)
    {
        Navigation.NavigateTo($"manage-account/account-users/{user.WebUserSecuritySK}/edit");
    }

    private void HandleDeleteClick(AccountUserModel user)
    {
        _userToRemove = user;
        _removeError = null;
        _isRemoving = false;
        _showRemoveModal = true;
    }

    private void CloseRemoveModal()
    {
        if (_isRemoving)
        {
            return;
        }
        _showRemoveModal = false;
        _userToRemove = null;
        _removeError = null;
    }

    private async Task ConfirmRemoveAsync()
    {
        if (_userToRemove == null)
        {
            return;
        }

        _isRemoving = true;
        _removeError = null;

        var (success, message) = await AccountUserService.DeleteUserAsync(_employerSK, _userToRemove.WebUserSecuritySK);

        _isRemoving = false;

        if (success)
        {
            _flashTitle = "User Removed";
            _flashMessage = $"You've successfully removed {_userToRemove.FirstName} {_userToRemove.LastName} from this UI account. If you wish to reinstate this user's access you will need to generate a new access key.";
            _flashKey++;
            _showRemoveModal = false;
            _userToRemove = null;
            await LoadUsersAsync();
            await ScrollToTopAsync();
        }
        else
        {
            _removeError = string.IsNullOrWhiteSpace(message) ? "Unable to remove user." : message;
        }
    }

    private async Task ScrollToTopAsync()
    {
        await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
    }
}





