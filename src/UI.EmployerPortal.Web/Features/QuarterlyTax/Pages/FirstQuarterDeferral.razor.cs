
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// FirstQuarterDeferral
/// </summary>
public partial class FirstQuarterDeferral
{
    /// <summary>
    /// Represents the current page state of the wizard.
    /// </summary>
    public enum PageState
    {
        /// <summary>
        /// The deferral form view.
        /// </summary>
        Wizard,

        /// <summary>
        /// The confirmation view after successful submission.
        /// </summary>
        Confirmation
    }

    /// <summary>
    /// The current page state (wizard or confirmation).
    /// </summary>
    private PageState _pageState = PageState.Wizard;

    /// <summary>
    /// Whether the user has agreed to the terms.
    /// </summary>
    private bool _hasAgreed;

    /// <summary>
    /// Whether the page is loading eligibility data.
    /// </summary>
    private bool _isLoading = true;

    /// <summary>
    /// Whether the employer is eligible for deferral.
    /// </summary>
    private bool _isEligible;

    /// <summary>
    /// The confirmation number returned after successful submission.
    /// </summary>
    private string _confirmationNumber = string.Empty;

    /// <summary>
    /// The edit context for form validation.
    /// </summary>
    private EditContext _editContext = new(new object());

    /// <summary>
    /// Shared message store for validation errors. Single instance to prevent nesting
    /// </summary>
    private ValidationMessageStore _messageStore = default!;

    /// <summary>
    /// Controls whether the validation error summary is displayed.
    /// </summary>
    private bool _showValidationSummary;

    /// <summary>
    /// Controls whether the notification summary is displayed.
    /// </summary>
    private bool _showNotificationSummary = false;

    private readonly string _notificationMessage = "Your account does not meet the eligibility criteria for first quarter deferral.";

    /// <summary>
    /// Maps field names to HTML element IDs for validation summary navigation.
    /// </summary>
    private readonly Dictionary<string, string> _validationFieldIds = new()
    {
        { "HasAgreed", "agreeTerms" }
    };

    /// <summary>
    /// Blazor navigation manager for page redirects.
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Orchestrator for quarterly report session and missing report data.
    /// </summary>
    [Inject]
    private IQuarterlyReportOrchestrator Orchestrator { get; set; } = default!;

    /// <summary>
    /// Service for tax and wage entry operations.
    /// </summary>
    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    /// <summary>
    /// JavaScript runtime for interop calls (e.g., scrollToTop).
    /// </summary>
    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// DashboardOrchestrator
    /// </summary>
    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    /// <summary>
    /// Configuration
    /// </summary>
    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _editContext = new EditContext(new object());
        _messageStore = new ValidationMessageStore(_editContext);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await CheckEligibilityAsync();
            _isLoading = false;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Checks deferral eligibility and populates validation errors if ineligible.
    /// </summary>
    private async Task CheckEligibilityAsync()
    {
        try
        {
            var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
            var employerSK = account!.Id;
            var deferralYear = DateTime.Now.Year;
            var eligibility = await TaxAndWageEntryService.CheckDeferralEligibilityAsync(employerSK, deferralYear);
            _isEligible = eligibility.IsEligible;

            if (!_isEligible)
            {
                _messageStore.Clear();

                if (eligibility.RuleViolations.Count > 0)
                {
                    foreach (var violation in eligibility.RuleViolations)
                    {
                        _messageStore.Add(_editContext.Field(string.Empty), violation.RuleViolation);
                    }
                }
                else
                {
                    _showNotificationSummary = true;
                    await JS.InvokeVoidAsync("scrollToTop");
                    return;
                }
                _showValidationSummary = true;
                _editContext.NotifyValidationStateChanged();
                await JS.InvokeVoidAsync("scrollToTop");
            }
        }

        catch (Exception)
        {
            _isEligible = false;
            _messageStore.Clear();
            _messageStore.Add(
                _editContext.Field(string.Empty),
                Configuration["Messages:TechnicalDifficulties"] ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            await JS.InvokeVoidAsync("scrollToTop");
        }
    }

    /// <summary>
    /// Navigates away from the deferral page when Cancel is clicked.
    /// </summary>
    private void HandleCancel()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    /// <summary>
    /// Handles the submit button click. Validates the checkbox, then calls the backend.
    /// </summary>
    private async Task HandleSubmit()
    {
        _showValidationSummary = false;

        // Clear any previous validation messages        
        _messageStore.Clear();
        _editContext.NotifyValidationStateChanged();

        // Validate checkbox
        if (!_hasAgreed)
        {
            _messageStore.Add(_editContext.Field("HasAgreed"),
                "You must agree to the terms before submitting.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            await JS.InvokeVoidAsync("scrollToTop");
            return;
        }
        _isLoading = true;

        try
        {
            var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
            var commonClientSK = account!.Id;
            var deferralYear = DateTime.Now.Year;
            var electionDate = DateTime.Now;

            var response = await TaxAndWageEntryService.ElectFirstQuarterDeferralAsync(commonClientSK, deferralYear, electionDate);

            if (response.RuleViolations.Count == 0 && !string.IsNullOrEmpty(response.ElectionConfirmationNumber))
            {
                _confirmationNumber = response.ElectionConfirmationNumber;
                await JS.InvokeVoidAsync("scrollToTop");
                _pageState = PageState.Confirmation;
            }

            else
            {
                foreach (var violation in response.RuleViolations)
                {
                    _messageStore.Add(_editContext.Field(string.Empty), violation.RuleViolation);
                }

                if (response.RuleViolations.Count == 0)
                {
                    _messageStore.Add(
                        _editContext.Field(string.Empty),
                        Configuration["Messages:TechnicalDifficulties"] ?? "We are currently experiencing technical difficulties. Please try again later.");
                }

                _showValidationSummary = true;
                _editContext.NotifyValidationStateChanged();
                await JS.InvokeVoidAsync("scrollToTop");
            }
        }
        catch (Exception)
        {
            _messageStore.Add(
                _editContext.Field(string.Empty),
                Configuration["Messages:TechnicalDifficulties"] ?? "We are currently experiencing technical difficulties. Please try again later.");

            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            await JS.InvokeVoidAsync("scrollToTop");
        }
        finally
        {
            _isLoading = false;
        }
    }

    /// <summary>
    /// Navigates to the employer dashboard when "File Another Report" is clicked.
    /// </summary>
    private void HandleFileAnotherReport()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }
}
