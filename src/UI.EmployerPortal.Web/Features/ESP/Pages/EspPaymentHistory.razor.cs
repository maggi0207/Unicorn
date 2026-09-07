using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Code-behind for the ESP Payment History page.
/// Loads tax file EFT payments for the current ESP user and presents them in a sortable table.
/// </summary>
public partial class EspPaymentHistory
{
    [Inject] private IEspPaymentHistoryService EspPaymentHistoryService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<EspPaymentHistoryItem> _payments = [];
    private bool _isLoading;
    private string? _loadError;
    private string _sortColumn = "settlementDate";
    private bool _sortAscending = true;

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadPaymentsAsync();
    }

    private async Task LoadPaymentsAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            var result = await EspPaymentHistoryService.GetPaymentHistoryAsync();
            if (result is null)
            {
                _loadError = "Unable to load payment history. Please try again.";
            }
            else
            {
                _payments = result;
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private IEnumerable<EspPaymentHistoryItem> GetSortedPayments()
    {
        return _sortColumn switch
        {
            "amount" => SortBy(p =>
            {
                return p.Amount;
            }),
            "status" => SortBy(p =>
            {
                return p.Status;
            }),
            _ => SortBy(p =>
            {
                return p.SettlementDate;
            }),
        };
    }

    private IOrderedEnumerable<EspPaymentHistoryItem> SortBy<TKey>(Func<EspPaymentHistoryItem, TKey> keySelector)
    {
        return _sortAscending
            ? _payments.OrderBy(keySelector)
            : _payments.OrderByDescending(keySelector);
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

    private void NavigateToPaymentDetails(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"esp/payment-history-details/{eftPaymentSk}");
    }

    private void NavigateToEditPayment(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"esp/esp-make-payment-ach-information?eftPaymentSk={eftPaymentSk}&action=edit");
    }

    private void NavigateToCancelPayment(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"esp/esp-make-payment-ach-information?eftPaymentSk={eftPaymentSk}&action=cancel");
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("esp/make-ach-payment");
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("esp/make-ach-payment");
    }
}
