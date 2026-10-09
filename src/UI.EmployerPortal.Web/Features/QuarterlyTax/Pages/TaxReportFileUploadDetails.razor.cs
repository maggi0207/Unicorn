using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ESP;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Represents the Tax Report File Upload Details Page
/// </summary>
public partial class TaxReportFileUploadDetails
{
    /// <summary>
    /// Gets or sets the navigation manager
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the tax report file upload detail service.
    /// </summary>
    [Inject]
    private ITaxFileUploadDetailService TaxFileUploadDetailService { get; set; } = default!;

    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;


    /// <summary>
    /// Gets the upload detail model
    /// </summary>
    [Inject]
    protected IESPOrchestrator ESPOrchestrator { get; set; } = default!;

    /// <summary>
    /// Gets or sets the ESP payment service, used to cancel processing of this file upload.
    /// </summary>
    [Inject]
    private IEspPaymentService EspPaymentService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the JS runtime, used to scroll the page back to the top after cancelling.
    /// </summary>
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Gets the upload detail model
    /// </summary>
    protected TaxReportFileUploadDetailModel? Model { get; set; } = new();

    /// <summary>
    /// Gets whether the cancel processing confirmation modal is open.
    /// </summary>
    protected bool ShowCancelModal { get; private set; }

    /// <summary>
    /// Gets whether the cancel processing request is currently in progress.
    /// </summary>
    protected bool IsCancelling { get; private set; }

    /// <summary>
    /// Gets whether the "Processing Cancelled" confirmation banner should be shown.
    /// </summary>
    protected bool ShowCancelledBanner { get; private set; }

    /// <summary>
    /// Gets the error message to show in the page-level banner after a failed cancel processing attempt, if any.
    /// </summary>
    protected string? CancelFailedMessage { get; private set; }

    ///// <summary>
    ///// Indicates whether the request originated from the test environment
    ///// </summary>
    //private bool _isSourceTestEnv;
    /// <summary>
    /// Currently selected tab
    /// </summary>
    protected UploadDetailTab SelectedTab { get; private set; } = UploadDetailTab.FileSummary;


    private string _sortColumn = "recordNumber";

    private bool _sortAscending = true;

    private bool _isSourceTestEnv;

    /// <summary>
    /// Whether the file is still awaiting review, so per-report results do not exist yet.
    /// </summary>
    private bool _awaitingReview;
    /// <summary>
    /// fileUploadSK
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "fileUploadSK")]
    public int FileUploadSK { get; set; }


    /// <summary>
    /// Initializes the page and loads upload detail information.
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);

        Model = await TaxFileUploadDetailService.LoadTaxReportFileUploadDetailAsync(FileUploadSK);

        _awaitingReview = TaxFileStatus.IsAwaitingReview(Model?.TaxFileStatusCode ?? TaxFileStatusCode.NotProcessed);
        Sort(_sortColumn);
    }

    private static string FormatUIAccountNo(string accountNo)
    {
        var digits = accountNo.Replace("-", "");
        return digits.Length == 10 ? $"{digits[..6]}-{digits[6..9]}-{digits[9..]}" : accountNo;
    }

    /// <summary>
    /// Displays the File Summary tab
    /// </summary>
    private void ShowFileSummary()
    {
        SelectedTab = UploadDetailTab.FileSummary;
    }


    /// <summary>
    /// Displays the Error Detail Report tab
    /// </summary>
    private void ShowErrorDetail()
    {
        SelectedTab = UploadDetailTab.ErrorDetail;
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo(TestEnvironmentSource.Preserve("quarterly-tax/tax-file-upload-summary", _isSourceTestEnv));
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }
    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private void Sort(string column)
    {
        if (Model?.FileSummary == null)
        {
            return;
        }
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
        IEnumerable<TaxReportFileSummaryModel> sortedData = column switch
        {
            "recordNumber" => _sortAscending ? Model.FileSummary.OrderBy(x =>
            {
                return x.RecordNumber;
            }) :
            Model.FileSummary.OrderBy(x =>
            {
                return x.RecordNumber;
            }),

            "errorDescription" => _sortAscending ? Model.FileSummary.OrderBy(x =>
            {
                return x.ErrorDescription;
            }) :
            Model.FileSummary.OrderBy(x =>
            {
                return x.ErrorDescription;
            }),
            _ => Model.FileSummary
        };
        Model.FileSummary = sortedData.ToList();
    }

    private void HandleCancelProcessing()
    {
        ShowCancelModal = true;
    }

    private void HandleCancelModalClose()
    {
        if (IsCancelling)
        {
            return;
        }

        ShowCancelModal = false;
    }

    private async Task HandleCancelModalConfirm()
    {
        if (Model == null)
        {
            return;
        }

        IsCancelling = true;

        var (success, errorMessage) = await EspPaymentService.CancelTaxFileUploadAsync((int) Model.FileUploadDetailSK);

        IsCancelling = false;
        ShowCancelModal = false;

        if (success)
        {
            ShowCancelledBanner = true;
            CancelFailedMessage = null;
            Model = await TaxFileUploadDetailService.LoadTaxReportFileUploadDetailAsync(FileUploadSK);
            Sort(_sortColumn);
        }
        else
        {
            ShowCancelledBanner = false;
            CancelFailedMessage = errorMessage ?? "Unable to cancel processing for this file.";
        }

        await ScrollToTopAsync();
    }

    private async Task ScrollToTopAsync()
    {
        await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
    }

    private async Task HandleMakeAchPayment()
    {
        var amount = Model?.PaymentAmountWithoutErrors ?? 0;
        await ESPOrchestrator.SavePaymentToSessionAsync(amount.ToString());
        var fileUploadSk = Model?.FileUploadDetailSK ?? 0;
        await ESPOrchestrator.SaveFileUploadDetailSkToSessionAsync(fileUploadSk);
        var fileConfirmationNumber = Model?.ConfirmationNumber ?? string.Empty;
        await ESPOrchestrator.SaveFileConfirmationToSessionAsync(fileConfirmationNumber);
        NavigationManager.NavigateTo("esp/esp-make-payment-ach-information");

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
        return GetImageAndAltText(path, altText);
    }
    private MarkupString GetImageAndAltText(string path, string altText)
    {
        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }
}
