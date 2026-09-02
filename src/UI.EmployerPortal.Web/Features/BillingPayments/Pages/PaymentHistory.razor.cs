using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.BillingPayments.Services;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Pages;

/// <summary>
/// Code-behind for the Payment History page.
/// Loads EFT payments for the selected employer and presents them in a sortable table.
/// </summary>
public partial class PaymentHistory
{
    [Inject] private IPaymentHistoryService PaymentHistoryService { get; set; } = default!;
    [Inject] private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<PaymentHistoryItem> _payments = [];
    private bool _isLoading = true;
    private string? _loadError;
    private string _sortColumn = "settlementDate";
    private bool _sortAscending = false;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (!await PageAuthorizationService.AuthorizeAsync(AuthorizationPolicies.RequiresAnyPaymentsPermission))
        {
            return;
        }

        await LoadPaymentsAsync();
    }

    private async Task LoadPaymentsAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            var result = await PaymentHistoryService.GetPaymentHistoryAsync();
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

    private IEnumerable<PaymentHistoryItem> GetSortedPayments()
    {
        return _sortColumn switch
        {
            "paymentType" => SortBy(p =>
            {
                return p.PaymentType;
            }),
            "settlementDate" => SortBy(p =>
            {
                return p.SettlementDate;
            }),
            "amount" => SortBy(p =>
            {
                return p.Amount;
            }),
            "confirmationId" => SortBy(p =>
            {
                return p.ConfirmationId;
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

    private IOrderedEnumerable<PaymentHistoryItem> SortBy<TKey>(Func<PaymentHistoryItem, TKey> keySelector)
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

    private void NavigateToPaymentDetails(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"billing-payments/payment-history-details/{eftPaymentSk}");
    }

    /// <summary>
    /// Sends the user to the ACH Edit Payment step for the given payment,
    /// the same step reached from the "EDIT PAYMENT" button on the EFT Payment Confirmation page.
    /// </summary>
    private void NavigateToEditPayment(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"billing-payments/bank-account-payment-ach?eftPaymentSk={eftPaymentSk}&action=edit");
    }

    /// <summary>
    /// Sends the user to the ACH Verify &amp; Cancel step for the given payment,
    /// the same step reached from the "CANCEL PAYMENT" button on the EFT Payment Confirmation page.
    /// </summary>
    private void NavigateToCancelPayment(int eftPaymentSk)
    {
        NavigationManager.NavigateTo($"billing-payments/bank-account-payment-ach?eftPaymentSk={eftPaymentSk}&action=cancel");
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("billing-payments/make-ach-payment");
    }
}
