using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
/// <summary>
/// Unified confirmation page for all report types:
///   Zero Tax Report:      ShowUploadWageFile=false, ShowEmailSubscriptionLink=false
///   Wage and Tax Entry:   ShowUploadWageFile=false, ShowEmailSubscriptionLink=true (if filed late)
///   Tax Report (Full):    ShowUploadWageFile=true,  ShowEmailSubscriptionLink=true (if filed late)
/// When HasGrossWageMismatch=true, the page switches to the mismatch view
/// showing only the mismatch message, File Adjustment button, and Payment Options.
/// </summary>
public partial class ReportConfirmation : ComponentBase
{
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    /// <summary>
    /// Gets or sets Reporting Quarter to Display in Confirmation page
    /// </summary>
    [Parameter] public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Due Date to Display in Confirmation page
    /// </summary>
    [Parameter] public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets Effective Date in Confirmation page
    /// </summary>
    [Parameter] public DateTime? EffectiveDate { get; set; }


    // --- Confirmation ---
    /// <summary>
    /// Gets or sets Confirmation Number received from submit response
    /// </summary>
    [Parameter] public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// The quarter number (1-4) of the report that was just filed.
    /// </summary>
    [Parameter] public int FiledQuarter { get; set; }

    /// <summary>
    /// The tax year of the reprot that was just filed.
    /// </summary>
    [Parameter] public int FiledYear { get; set; }

    /// <summary>
    /// The filing type of the reprot that was just filed.
    /// </summary>
    [Parameter] public TaxWageFilingType FiledFilingType { get; set; }

    /// <summary>
    /// RequiresWageAdjustment?
    /// </summary>
    [Parameter] public bool RequiresWageAdjustment { get; set; }

    [Inject]
    private IEmployerAccountService EmployerAccountService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    private bool _isLoading = true;

    /// <summary>
    /// Whether there are missing quarterly reports. Populated automatically via service call.
    /// </summary>
    private bool _hasMissingQuarterlyReports;

    /// <summary>
    /// Whether a wage report is missing for the same quarter/year that was just filed.
    /// </summary>
    private bool _hasMatchingMissingWageReport;

    /// <summary>
    /// Whether a gross wage mismatch was detected for the filled quarter/year. Populated via service call. 
    /// </summary>
    private bool _hasGrossWageMismatch;

    /// <summary>
    /// The formatted quarter identifier for the adjusment report.
    /// </summary>
    private string _adjustmentQuarter = string.Empty;
    private bool _hasOutstandingBalance;

    /// <summary>
    /// Whether to show the email subscription link based on the filed report type.
    /// </summary>
    private bool ShowEmailSubscriptionLink => FiledFilingType is
    TaxWageFilingType.TaxAndWageEntry or
    TaxWageFilingType.TaxEntry or TaxWageFilingType.TaxAndWageUpload;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        var balanceTask = account != null
        ? EmployerAccountService.GetRemainingBalance(account.Id)
        : Task.FromResult(0m);
        var missingReportsTask = TaxAndWageEntryService.GetMissingWageAndTaxReports();
        var summaryReportsTask = TaxAndWageEntryService.GetOutOfBalanceSummaryReportsAsync();

        await Task.WhenAll(balanceTask, missingReportsTask, summaryReportsTask);

        var remainingBalance = await balanceTask;
        _hasOutstandingBalance = remainingBalance > 0m;

        if (account != null && account.BalanceDue != remainingBalance)
        {
            var updatedAccount = account with { BalanceDue = remainingBalance };
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            if (selected is not null)
            {
                await SessionManager.SetAsync(selected with { EmployerAccount = updatedAccount });
            }
        }

        var missingReportsData = await missingReportsTask;
        var missingReports = missingReportsData.MissingReports ?? new();

        _hasMissingQuarterlyReports = missingReports.Count > 0;

        _hasMatchingMissingWageReport = missingReports.Any(r =>
        {
            return r.Quarter == FiledQuarter &&
                   r.Year == FiledYear &&
                   string.Equals(r.ReportName, "Wage Report", StringComparison.OrdinalIgnoreCase);
        });

        var summaryReports = await summaryReportsTask;
        var matchingSummary = summaryReports.FirstOrDefault(r =>
        {
            return r.Quarter == FiledQuarter &&
                   r.Year == FiledYear &&
                   r.OutOfBalanceIndicator == true &&
                   r.TaxReportMissingFlag == false &&
                   r.WageReportMissingFlag == false;
        });

        _hasGrossWageMismatch = RequiresWageAdjustment || matchingSummary is not null;
        _adjustmentQuarter = _hasGrossWageMismatch ? $"Q{FiledQuarter} {FiledYear}" : string.Empty;
        _isLoading = false;
    }

    private void HandleFileAdjustmentClicked()
    {
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
    }

    private void HandleEmailSubscriptionClicked()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    private void HandlePaymentOptionsClicked()
    {
        NavigationManager.NavigateTo("billing-payments/payment-options");
    }

    private void HandleUploadWageFileClicked()
    {
        NavigationManager.NavigateTo("quarterly-tax/wage-upload");
    }

    private void HandleFileAnotherReportClicked()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    private void HandleFileWageReportClicked()
    {
        NavigationManager.NavigateTo("quarterly-tax/wage-entry-report");
    }
    private async Task HandleSavePageClicked()
    {
        await JS.InvokeVoidAsync("printElement", ".confirmation-page");
    }

}
