using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;

/// <summary>
/// Confirmation of the Wage Report Adjustment wizard.
/// Displayed after successful submission.
/// </summary>
public partial class AdjustmentConfirmation
{
    /// <summary>
    /// Navigation
    /// </summary>
    [Inject] private NavigationManager Nav { get; set; } = default!;

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
    /// Gets or sets the confirmation number from the submission response.
    /// </summary>
    [Parameter]
    public string ConfirmationNumber { get; set; } = string.Empty;
    /// <summary>
    /// _hasOutstandingBalance
    /// </summary>
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

    private async Task HandleSavePageClicked()
    {
        await JS.InvokeVoidAsync("printElement", ".confirmation-page");
    }

    private async Task HandleReviewTaxReportClicked()
    {
        Nav.NavigateTo("quarterly-tax/previously-filed", forceLoad: true);
    }

    private async Task HandleAdjustAnotherReportClicked()
    {
        Nav.NavigateTo("tax-wage-report-adjustments/adjustments", forceLoad: true);
    }

    private void HandlePaymentOptionsClicked()
    {
        Nav.NavigateTo("billing-payments/payment-options");
    }
}
