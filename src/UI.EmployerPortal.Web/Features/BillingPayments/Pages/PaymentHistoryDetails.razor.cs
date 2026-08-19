using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.BillingPayments.Services;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Pages;

/// <summary>
/// Code-behind for the Payment History Details page.
/// Displays read-only payment information, bank info, contact info, and activity history
/// for a single EFT payment selected from Payment History.
/// </summary>
public partial class PaymentHistoryDetails
{
    /// <summary>Surrogate key of the EFT payment to display, supplied via the route.</summary>
    [Parameter] public int EftPaymentSk { get; set; }

    [Inject] private IPaymentDetailService PaymentDetailService { get; set; } = default!;
    [Inject] private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private PaymentDetailModel? _payment;
    private bool _isLoading;
    private string? _loadError;

    private bool _paymentInfoExpanded = true;
    private bool _bankInfoExpanded = true;
    private bool _contactInfoExpanded = true;
    private bool _activityExpanded = true;

    private string _activitySortColumn = "date";
    private bool _activitySortAscending = false;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        if (!await PageAuthorizationService.AuthorizeAsync(AuthorizationPolicies.RequiresAnyPaymentsPermission))
        {
            return;
        }

        await LoadPaymentAsync();
    }

    private async Task LoadPaymentAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            _payment = await PaymentDetailService.GetPaymentDetailAsync(EftPaymentSk);

            if (_payment is null)
            {
                _loadError = "Unable to load payment details. Please try again.";
            }
        }
        finally
        {
            _isLoading = false;
        }
    }

    private IEnumerable<PaymentActivityItem> GetSortedActivity()
    {
        return _payment is null
            ? []
            : _activitySortColumn switch
            {
                "action" => SortActivityBy(a =>
                {
                    return a.Action;
                }),
                "description" => SortActivityBy(a =>
                {
                    return a.Description;
                }),
                _ => SortActivityBy(a =>
                {
                    return a.Date;
                }),
            };
    }

    private IOrderedEnumerable<PaymentActivityItem> SortActivityBy<TKey>(Func<PaymentActivityItem, TKey> keySelector)
    {
        return _activitySortAscending
            ? _payment!.ActivityHistory.OrderBy(keySelector)
            : _payment!.ActivityHistory.OrderByDescending(keySelector);
    }

    private void SortActivity(string column)
    {
        if (_activitySortColumn == column)
        {
            _activitySortAscending = !_activitySortAscending;
        }
        else
        {
            _activitySortColumn = column;
            _activitySortAscending = true;
        }
    }

    private MarkupString GetActivitySortIcon(string column)
    {
        string path;
        string altText;

        if (_activitySortColumn == column)
        {
            path = _activitySortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _activitySortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetActivityAriaSort(string column)
    {
        return _activitySortColumn != column ? null : _activitySortAscending ? "ascending" : "descending";
    }

    private void HandleActivityHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            SortActivity(column);
        }
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("billing-payments/payment-history");
    }
}
