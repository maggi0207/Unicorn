using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ESP.Components;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Manage ESP Profile page. Loads the current Employer Service Provider's registration
/// details, presents them in the same read-only review used by the registration flow,
/// and lets the user edit and update them. Reuses the registration entry and review
/// components; the only differences are the "Manage ESP Profile" title and the "UPDATE"
/// action in place of "SUBMIT".
/// </summary>
public partial class EspProfile
{
    [Inject] private IEspAccountService EspAccountService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>The steps of the profile flow.</summary>
    private enum PageState
    {
        /// <summary>Data-entry form.</summary>
        Entry,
        /// <summary>Read-only review (the landing view).</summary>
        Review,
        /// <summary>Successful update confirmation.</summary>
        Submitted
    }

    private EspRegistrationModel _model = new();
    private readonly List<string> _ruleViolationMessages = new();
    private PageState _pageState = PageState.Review;
    private bool _isLoading = true;
    private string _loadingMessage = "Loading...";
    private EspRegistrationEntry? _entryRef;

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        var details = await EspAccountService.ObtainEspDetailsAsync();
        if (details is not null)
        {
            _model = details;
        }

        _isLoading = false;
    }

    private async Task HandleContinue()
    {
        ClearRuleViolations();

        if (_entryRef is not null && !_entryRef.IsValid())
        {
            return;
        }

        _pageState = PageState.Review;
        await JS.InvokeVoidAsync("scrollToTop");
    }

    private async Task HandleEdit()
    {
        ClearRuleViolations();
        _pageState = PageState.Entry;
        await JS.InvokeVoidAsync("scrollToTop");
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("esp-dashboard");
    }

    private async Task HandleUpdate()
    {
        ClearRuleViolations();
        _isLoading = true;
        _loadingMessage = "Updating...";
        StateHasChanged();
        await Task.Yield();

        try
        {
            var response = await EspAccountService.UpdateEspDetailsAsync(_model);

            if (response.RuleViolations is { Length: > 0 })
            {
                _ruleViolationMessages.AddRange(response.RuleViolations
                    .Select(v =>
                    {
                        return v.RuleViolation ?? string.Empty;
                    })
                    .Where(message =>
                    {
                        return !string.IsNullOrWhiteSpace(message);
                    }));

                _pageState = PageState.Review;
            }
            else
            {
                _pageState = PageState.Submitted;
            }
        }
        finally
        {
            _isLoading = false;
        }

        await JS.InvokeVoidAsync("scrollToTop");
    }

    private void ClearRuleViolations()
    {
        _ruleViolationMessages.Clear();
    }
}
