using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Code-behind for the ESP Make ACH Payment page.
/// Lists tax report file uploads pending EFT payment initiation, so the ESP user can
/// view a file to make its payment or cancel processing.
/// </summary>
public partial class MakeAchPayment
{
    [Inject] private IEspPaymentService EspPaymentService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private IReadOnlyList<PendingTaxFilePayment> _files = [];
    private bool _isLoading;
    private string? _loadError;
    private string _sortColumn = "uploadDate";
    private bool _sortAscending;

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadFilesAsync();
    }

    private async Task LoadFilesAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            var result = await EspPaymentService.GetPendingTaxFilePaymentsAsync();
            _loadError = result.ErrorMessage;
            _files = result.Payments;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private IEnumerable<PendingTaxFilePayment> GetSortedFiles()
    {
        return _sortColumn switch
        {
            "fileName" => SortBy(f =>
            {
                return f.FileName;
            }),
            "confirmationId" => SortBy(f =>
            {
                return f.ConfirmationId;
            }),
            "reportsWithoutErrors" => SortBy(f =>
            {
                return f.ReportsWithoutErrors;
            }),
            "paymentAmount" => SortBy(f =>
            {
                return f.PaymentAmount;
            }),
            _ => SortBy(f =>
            {
                return f.UploadDate;
            }),
        };
    }

    private IOrderedEnumerable<PendingTaxFilePayment> SortBy<TKey>(Func<PendingTaxFilePayment, TKey> keySelector)
    {
        return _sortAscending
            ? _files.OrderBy(keySelector)
            : _files.OrderByDescending(keySelector);
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

    private void HandleViewFile(PendingTaxFilePayment file)
    {
        NavigationManager.NavigateTo($"quarterly-tax/tax-upload-details?fileUploadSK={file.FileUploadDetailSk}&returnUrl={Uri.EscapeDataString("esp/make-ach-payment")}");
    }
}
