using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;

/// <summary>
///
/// </summary>
public partial class ReportConfirmationAdjustments : ComponentBase
{
    /// <summary>
    /// Navigation
    /// </summary>
    [Inject]
    private NavigationManager Nav { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IEmployerAccountService EmployerAccountService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

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
    /// The quarter for the adjustment report .
    /// </summary>
    [Parameter] public string AdjustmentQuarter { get; set; } = string.Empty;

    /// <summary>
    /// File Name
    /// </summary>
    [Parameter] public string FileName { get; set; } = string.Empty;
    /// <summary>
    /// When true, shows a warning that no Wage Report exists for the adjusted quarter.
    /// </summary>
    [Parameter] public bool ShowNoWageReportWarning { get; set; }

    private bool _hasOutstandingBalance;

    /// <summary>
    /// OnInitializedAsync
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        if (account is not null)
        {
            var remainingBalance = await EmployerAccountService.GetRemainingBalance(account.Id);
            _hasOutstandingBalance = remainingBalance > 0m;

            if (account.BalanceDue != remainingBalance)
            {
                var updatedAccount = account with { BalanceDue = remainingBalance };
                var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
                if (selected is not null)
                {
                    await SessionManager.SetAsync(selected with { EmployerAccount = updatedAccount });
                }
            }
        }
    }

    // --- Events ---

    /// <summary>
    /// Callback invoked when OnSavePageClicked
    /// </summary>
    public async Task OnSavePageClicked()
    {
        if (!string.IsNullOrEmpty(FileName))
        {
            var safeDateTime = DateTime.Now.ToString("MM-dd-yyyy_hh_mm_tt");

            var finalName = FileName + " " + ReportingQuarter + " " + safeDateTime;

            await JS.InvokeVoidAsync("printDivWithFileName", ".pfr-details-page", finalName);
        }
    }

    /// <summary>
    /// Handles Adjust Another Report Click.
    /// </summary>
    public async Task HandleOnAdjustAnotherReport()
    {
        Nav.NavigateTo("tax-wage-report-adjustments/adjustments", forceLoad: true);
    }

    /// <summary>
    /// Handles On Wage Report Click.
    /// </summary>
    public async Task HandleOnWageReport()
    {
        Nav.NavigateTo("quarterly-tax/previously-filed", forceLoad: true);
    }

    /// <summary>
    /// Handles On Payment Click.
    /// </summary>
    public async Task HandleOnPaymentOptions()
    {
        Nav.NavigateTo("billing-payments/payment-options");
    }
}
