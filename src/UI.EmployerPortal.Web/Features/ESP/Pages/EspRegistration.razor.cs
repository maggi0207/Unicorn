using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ESP.Components;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Employer Service Provider registration page. Walks the user through a data-entry
/// step (page 1) and a read-only review step (page 2) before submitting.
/// </summary>
public partial class EspRegistration
{
    [Inject] private IEspAccountService EspAccountService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>The steps of the registration flow.</summary>
    private enum PageState
    {
        /// <summary>Data-entry form (page 1).</summary>
        Entry,
        /// <summary>Read-only review (page 2).</summary>
        Review,
        /// <summary>Successful submission confirmation.</summary>
        Submitted
    }

    private readonly EspRegistrationModel _model = new();
    private readonly List<string> _ruleViolationMessages = new();
    private PageState _pageState = PageState.Entry;
    private bool _isLoading;
    private EspRegistrationEntry? _entryRef;

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

    private async Task HandleSubmit()
    {
        ClearRuleViolations();
        _isLoading = true;
        StateHasChanged();
        await Task.Yield();

        try
        {
            var response = await EspAccountService.RegisterEspAsync(_model);

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
