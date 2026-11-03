using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// Home page for Taxable Reserve Fund Transactions to select any of the transactions
/// </summary>
public partial class BenefitChargesSelectOptions
{
    private string? SelectedValue => _benfitChargesOptions[_selectedIndex];
    private int _selectedIndex;
    private bool _showValidationError;
    private bool _isLoading = true;
    private bool _calendarDrawerOpen;

    private void OpenCalendarDrawer()
    {
        _calendarDrawerOpen = true;
    }

    private void CloseCalendarDrawer()
    {
        _calendarDrawerOpen = false;
    }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private bool _isTaxable;

    private readonly List<string> _benfitChargesOptions = new()
    {
            "Reserve Fund Balance",
            "Download Benefit Charges",
            "Download Other Transactions",
            "Weekly Reserve Fund"
    };

    private void HandleCardSelected(int index)
    {
        _selectedIndex = index;
    }


    private async Task HandleNextCard()
    {
        if (_benfitChargesOptions.Count == 0)
        {
            return;
        }
        // Move down or wrap to beginning
        HandleCardSelected((_selectedIndex + 1) % _benfitChargesOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task HandlePreviousCard()
    {
        if (_benfitChargesOptions.Count == 0)
        {
            return;
        }
        // Move up or wrap to end
        HandleCardSelected((_selectedIndex - 1 + _benfitChargesOptions.Count) % _benfitChargesOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task FocusOnCard(int index)
    {
        await JSRuntime.InvokeVoidAsync("focusElement", $"reporting-card-{index}");
    }

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        _isLoading = true;
        var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();
        _isTaxable = selectedEmployer?.EmployerAccount?.IsTaxable ?? false;
        _isLoading = false;
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
            "Reserve Fund Balance" => _isTaxable
            ? "other-functions/reserve-balance" : "other-functions/reserve-balance-form",
            "Download Benefit Charges" => "other-functions/download-benefit-charges",
            "Download Other Transactions" => "other-functions/download-transactions",
            "Weekly Reserve Fund" => "ui-documents",
            _ => null
        };

        if (destination is not null)
        {
            NavigationManager.NavigateTo(destination);
        }
    }



}
