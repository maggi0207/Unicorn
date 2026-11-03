using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// Home page for Other Functions to select any of the other functions
/// </summary>
public partial class SelectOtherFunctions
{
    private string? SelectedValue => _otherFunctionOptions[_selectedIndex];
    private int _selectedIndex = 0;
    private bool _showValidationError;
    private bool _isLoading = true;


    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private bool _isTaxable;

    private readonly List<string> _otherFunctionOptions = new()
    {
            "Download Benefit Charges",
            "Request 940C Recertification",
            "Manage Email Subscriptions",
            "Upload Audit Files"
    };

    private void HandleCardSelected(int index)
    {
        _selectedIndex = index;
    }


    private async Task HandleNextCard()
    {
        if (_otherFunctionOptions.Count == 0)
        {
            return;
        }
        // Move down or wrap to beginning
        HandleCardSelected((_selectedIndex + 1) % _otherFunctionOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task HandlePreviousCard()
    {
        if (_otherFunctionOptions.Count == 0)
        {
            return;
        }
        // Move up or wrap to end
        HandleCardSelected((_selectedIndex - 1 + _otherFunctionOptions.Count) % _otherFunctionOptions.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task FocusOnCard(int index)
    {
        await JSRuntime.InvokeVoidAsync("focusElement", $"reporting-card-{index}");
    }

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
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
            "Download Benefit Charges" => "other-functions/benefit-charges",
            "Request 940C Recertification" => "other-functions/940c",
            "Manage Email Subscriptions" => "other-functions/email-subscriptions",
            "Upload Audit Files" => "other-functions/upload-audit",
            _ => null
        };

        if (destination is not null)
        {
            NavigationManager.NavigateTo(destination);
        }
    }

    /// <summary>
    /// For Other Function Options
    /// </summary>
    public class OtherFunctionOption
    {
        /// <summary>
        /// For Title
        /// </summary>
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// For Description
        /// </summary>
        public MarkupString Description { get; set; }
    }
}
