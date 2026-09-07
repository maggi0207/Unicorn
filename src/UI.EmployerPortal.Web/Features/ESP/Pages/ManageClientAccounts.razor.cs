using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Manage Client Accounts — lists the current ESP's client relationships and allows
/// removing inactive clients. Mirrors the pattern of the account-users page.
/// </summary>
public partial class ManageClientAccounts
{
    [Inject]
    private IEspAccountService EspAccountService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    private List<EspClientModel> _allClients = [];
    private bool _isLoading = true;
    private bool _hasError = false;
    private string _resultsText = string.Empty;

    private string? _flashTitle = null;
    private string? _flashMessage = null;
    private int _flashKey = 0;

    private bool _showRemoveModal = false;
    private bool _isRemoving = false;
    private string? _removeError = null;
    private EspClientModel? _clientToRemove = null;

    private string _searchText = string.Empty;

    private int _currentPage = 1;
    private int _pageSize = 10;
    private int PaginationStart => FilteredClients.Count > 0 ? ((_currentPage - 1) * _pageSize) + 1 : 0;
    private int PaginationEnd => Math.Min(_currentPage * _pageSize, FilteredClients.Count);
    private int TotalPages => (int) Math.Ceiling((double) FilteredClients.Count / _pageSize);

    private string _sortColumn = "accountName";
    private bool _sortAscending = true;

    private List<EspClientModel> FilteredClients
    {
        get
        {
            var query = _allClients.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(_searchText))
            {
                var term = _searchText.Trim();
                query = query.Where(c =>
                {
                    var accountName = c.AccountName ?? string.Empty;
                    var uiAccountNumber = c.UIAccountNumber ?? string.Empty;
                    return accountName.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                           uiAccountNumber.Contains(term, StringComparison.OrdinalIgnoreCase);
                });
            }

            return [.. query];
        }
    }

    private IEnumerable<EspClientModel> PagedClients
    {
        get
        {
            var filtered = FilteredClients;
            var sorted = _sortColumn switch
            {
                "accountName" => SortBy(filtered, c =>
                {
                    return c.AccountName;
                }),
                "uiAccountNumber" => SortBy(filtered, c =>
                {
                    return c.UIAccountNumber;
                }),
                _ => SortBy(filtered, c =>
                {
                    return c.AccountName;
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
        await LoadClientsAsync();
    }

    private async Task LoadClientsAsync()
    {
        try
        {
            _allClients = await EspAccountService.GetClientsAsync();
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

    private IOrderedEnumerable<EspClientModel> SortBy<TKey>(
        List<EspClientModel> source, Func<EspClientModel, TKey> keySelector)
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
        var total = FilteredClients.Count;
        _resultsText = total == 0
            ? "0 Clients"
            : $"{Math.Min(_pageSize, total)} of {total} Clients";
    }

    private void HandleDeleteClick(EspClientModel client)
    {
        _clientToRemove = client;
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
        _clientToRemove = null;
        _removeError = null;
    }

    private async Task ConfirmRemoveAsync()
    {
        if (_clientToRemove == null)
        {
            return;
        }

        _isRemoving = true;
        _removeError = null;

        var (success, message) = await EspAccountService.RemoveClientAsync(_clientToRemove.CommonClientSK);

        _isRemoving = false;

        if (success)
        {
            _flashTitle = "Client Removed";
            _flashMessage = $"You've successfully removed {_clientToRemove.AccountName} from your client accounts.";
            _flashKey++;
            _showRemoveModal = false;
            _clientToRemove = null;
            await LoadClientsAsync();
            await ScrollToTopAsync();
        }
        else
        {
            _removeError = string.IsNullOrWhiteSpace(message) ? "Unable to remove client." : message;
        }
    }

    private async Task ScrollToTopAsync()
    {
        await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
    }

    /// <summary>
    /// Formats a 10-digit UI account number for display as 999999-999-9. Any value that
    /// isn't exactly 10 digits (after stripping non-digits) is returned unchanged.
    /// </summary>
    private static string FormatUiAccountNumber(string? uiAccountNumber)
    {
        var digits = new string((uiAccountNumber ?? string.Empty).Where(char.IsDigit).ToArray());
        return digits.Length == 10
            ? $"{digits[..6]}-{digits.Substring(6, 3)}-{digits.Substring(9, 1)}"
            : uiAccountNumber ?? string.Empty;
    }

    private bool IsEspUserManager()
    {
        return UserAccountService.IsEspUserManager();
    }
}
