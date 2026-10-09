using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Pages;


/// <summary>
/// Home page for File a Taxt and Wage Report Adjustments to select the Adjustment Type
/// </summary>
public partial class SelectAdjustmentType
{
    private int _selectedIndex = 0;
    private bool _showValidationError;
    private bool _isLoading = true;
    private PendingAdjustmentReportModel? _pendingAdjustment;
    private ElementReference _headingRef;
    private bool _hasFocused = false;
    private string? SelectedValue => _adjustmentOptions[_selectedIndex];

    private readonly List<string> _adjustmentOptions = new()
    {
        "Tax Report Adjustment",
        "Wage Adjustment by Employee for Multiple Quarters",
        "Wage Adjustment by Quarter",
        "Add New Employees to Existing Wage Report",
    };

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IWageAdjustmentService WageAdjustmentService { get; set; } = default!;

    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isLoading = true;
        await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
        var response = await WageAdjustmentService.GetPendingAdjustmentsAsync();
        if (response is not null)
        {
            _pendingAdjustment = new PendingAdjustmentReportModel
            {
                WageAdjustmentByQuarter = response.WageAdjustmentsByQuarter?.FirstOrDefault(),
                WageAdjustmentByEmployee = response.WageAdjustmentsByEmployee?.FirstOrDefault(),
                WageAdjustmentAppended = response.WageAdjustmentsAppended?.FirstOrDefault(),
            };

            if (!_pendingAdjustment.HasPending)
            {
                _pendingAdjustment = null;
            }
        }
        _isLoading = false;
    }

    /// <summary>
    /// /
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_isLoading && !_hasFocused)
        {
            _hasFocused = true;
            await _headingRef.FocusAsync();
        }
    }

    private static string GetReportName(PendingAdjustmentReportModel pending)
    {
        return pending switch
        {
            { WageAdjustmentByQuarter: not null } => "Wage Adjustment by Quarter",
            { WageAdjustmentByEmployee: not null } => "Wage Adjustment by Employee for Multiple Quarters",
            { WageAdjustmentAppended: not null } => "Add New Employees to Existing Wage Report",
            _ => string.Empty
        };
    }

    private static string GetNavigationUrl(PendingAdjustmentReportModel pending)
    {
        return pending switch
        {
            { WageAdjustmentByQuarter: not null } => "tax-wage-report-adjustments/wage-adjustment-by-quarter",
            { WageAdjustmentByEmployee: not null } => "tax-wage-report-adjustments/wage-adjustment-multiple-quarter",
            { WageAdjustmentAppended: not null } => "tax-wage-report-adjustments/add-new-employees",
            _ => string.Empty
        };
    }

    private async Task HandleContinuePendingAdjustment()
    {
        if (_pendingAdjustment is null)
        {
            return;
        }

        _isLoading = true;
        StateHasChanged();

        await QuarterlyReportOrchestrator.SavePendingAdjustmentToSessionAsync(_pendingAdjustment);

        var url = GetNavigationUrl(_pendingAdjustment);
        _isLoading = false;
        if (!string.IsNullOrEmpty(url))
        {
            NavigationManager.NavigateTo(url, true);
        }
    }

    private void HandleCardSelected(int index)
    {
        _selectedIndex = index;
        _showValidationError = false;
    }

    private async Task HandleNextCard()
    {
        if (_adjustmentOptions.Count == 0)
        {
            return;
        }
        // Move down or wrap to beginning
        HandleCardSelected((_selectedIndex + 1) % _adjustmentOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task HandlePreviousCard()
    {
        if (_adjustmentOptions.Count == 0)
        {
            return;
        }
        // Move up or wrap to end
        HandleCardSelected((_selectedIndex - 1 + _adjustmentOptions.Count) % _adjustmentOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task FocusOnCard(int index)
    {
        await JSRuntime.InvokeVoidAsync("focusElement", $"reporting-card-{index}");
    }

    private void HandleContinue()
    {
        if (string.IsNullOrEmpty(SelectedValue))
        {
            _showValidationError = true;
            return;
        }

        var destination = SelectedValue switch
        {
            "Tax Report Adjustment" => "tax-wage-report-adjustments/tax-report-adjustment",
            "Wage Adjustment by Employee for Multiple Quarters" => "tax-wage-report-adjustments/wage-adjustment-multiple-quarter",
            "Wage Adjustment by Quarter" => "tax-wage-report-adjustments/wage-adjustment-by-quarter",
            "Add New Employees to Existing Wage Report" => "tax-wage-report-adjustments/add-new-employees",
            _ => null
        };

        if (destination is not null)
        {
            NavigationManager.NavigateTo(destination, true);
        }
    }
}
