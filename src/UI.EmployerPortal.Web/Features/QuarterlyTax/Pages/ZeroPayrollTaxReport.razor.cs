using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Helpers;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code-behind for the ZeroPayrollTaxReport page.
/// Single-step page that displays zero-prepopulated tax details and submits a zero payroll report.
/// </summary>
public partial class ZeroPayrollTaxReport
{
    [Inject]
    private ITaxExclusionThresholdService TaxExclusionThresholdService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    /// <summary>
    /// Page display states.
    /// </summary>
    public enum PageState
    {
        /// <summary>Main report page</summary>
        Wizard,
        /// <summary>Confirmation after successful submission</summary>
        Confirmation
    }

    private PageState _pageState = PageState.Wizard;
    private EditContext _editContext = default!;
    private bool _showValidationSummary;
    private readonly Dictionary<string, string> _validationFieldIds = new();
    private MissingReportModel? _missingReportModel;
    private bool _showLeaveModal;
    private bool _isLoading = true;

    private string _reportingQuarter = string.Empty;
    private DateTime _dueDate;
    private int _taxYear;
    private int _quarter;
    private TaxWageFilingType _filingType;
    private string? _confirmationNumber;

    private readonly TaxDetailsModel _taxDetailsData = new();

    // Subjectivity state fields
    private WageTaxFilingPayrollQuestionsResponse? _subjectivityResponse;
    private readonly Dictionary<int, string> _subjectivityAnswers = new();
    private bool _hasSubjectivityStep;
    private SubjectivityQuestionsStep? _subjectivityStepRef;

    private int _currentStep = 1;

    private readonly List<WizardStep> _wizardSteps =
    [
        new() { StepNumber = 1, Title = "Subjectivity Questions", ActionButtonText = "Continue" },
        new() { StepNumber = 2, Title = "Verify Information", ActionButtonText = "Submit Report" }
    ];

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _editContext = new EditContext(_taxDetailsData);
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _missingReportModel = await QuarterlyReportOrchestrator.GetMissingReportFromSessionAsync();
        if (_missingReportModel is null)
        {
            NavigationManager.NavigateTo("quarterly-tax/missing-reports");
            return;
        }

        var filingMethod = await TaxAndWageEntryService.GetAvailableFilingMethod(_missingReportModel, TaxWageFilingType.ZeroPayroll);
        if (filingMethod is null || !filingMethod.IsEligible)
        {
            NavigationManager.NavigateTo("quarterly-tax/select-report");
            return;
        }

        _reportingQuarter = _missingReportModel.FormattedQuarterYear;
        _dueDate = _missingReportModel.DueDate ?? DateTime.Today;
        _taxYear = _missingReportModel.Year;
        _quarter = _missingReportModel.Quarter;
        _filingType = TaxWageFilingType.ZeroPayroll;

        var taxRate = await TaxAndWageEntryService.GetTaxRateForYear(_missingReportModel.Year);

        var (m1, m2, m3) = GetMonthNamesFromQuarter(_missingReportModel.Quarter);
        _taxDetailsData.FirstMonthName = m1;
        _taxDetailsData.SecondMonthName = m2;
        _taxDetailsData.ThirdMonthName = m3;
        _taxDetailsData.TaxYear = _missingReportModel.Year;
        _taxDetailsData.TaxRate = taxRate.TotalRate;

        // Check WCF Subjectivity questions for $0.00 payroll
        _subjectivityResponse = await TaxAndWageEntryService.GetSubjectivityQuestionsAsync(0m, _quarter, _taxYear);
        if (HasActualQuestions(_subjectivityResponse))
        {
            _hasSubjectivityStep = true;
            _currentStep = 1;
        }

        await TaxWageExclusionThresholdHelper.TrySetExclusionThresholdAsync(
            TaxExclusionThresholdService,
            threshold =>
            {
                _taxDetailsData.ExclusionThreshold = threshold;
            },
            _editContext,
            Configuration,
            () =>
            {
                _showValidationSummary = true;
            });

        _isLoading = false;
        StateHasChanged();
    }

    private static bool HasActualQuestions(WageTaxFilingPayrollQuestionsResponse? response)
    {
        return response != null
 && response.NumberOfPayrollQuestions > 0
 && response.EmployerPortalPayrollQuestionCategories != null
 && response.EmployerPortalPayrollQuestionCategories.Any(c =>
                c.TaxThresholds != null && c.TaxThresholds.Any(t =>
                    t.EmployerPortalPayrollQuestions != null && t.EmployerPortalPayrollQuestions.Length > 0));
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    private void HandleLeaveModalClose()
    {
        _showLeaveModal = false;
    }

    private void HandleLeaveModalConfirm()
    {
        _showLeaveModal = false;
        NavigationManager.NavigateTo("quarterly-tax/select-report", true);
    }

    private async Task HandleSubmitClick()
    {
        _showValidationSummary = false;
        _isLoading = true;
        StateHasChanged();
        await Task.Yield();
        await SubmitAsync();
        _isLoading = false;
    }

    private async Task SubmitAsync()
    {
        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        if (account == null)
        {
            return;
        }

        var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod(_filingType);
        if (filingMethod == null)
        {
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty), "Cannot find Filing Method.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            _isLoading = false;
            return;
        }

        var questionResponses = _subjectivityAnswers
            .Where(kv => !string.IsNullOrWhiteSpace(kv.Value))
            .Select(kv => new PayrollQuestionResponseProxy
            {
                EmployerPortalQuestionSK = kv.Key,
                QuestionResponseText = kv.Value
            })
            .ToArray();

        var filing = new WageTaxFilingProxy
        {
            EffectiveDate = DateTime.Now,
            EmployerSK = account.Id,
            Exclusions = 0,
            FilingTypeCodeSK = filingMethod.CodeSK,
            FilingTypeDescription = filingMethod.ShortDescription,
            GrossWages = 0,
            Month1EmployeeCount = 0,
            Month2EmployeeCount = 0,
            Month3EmployeeCount = 0,
            Quarter = _missingReportModel?.Quarter,
            Year = _missingReportModel?.Year,
            SourceText = "Internet - Nelnet",
            ReportingSelectionCodeSK = (int) TaxWageFilingType.ZeroPayroll,
            PayrollQuestionResponses = questionResponses
        };

        var result = await TaxAndWageEntryService.SubmitTaxReportAsync(filing);

        if (result.RuleViolations != null && result.RuleViolations.Any())
        {
            var messageStore = new ValidationMessageStore(_editContext);
            foreach (var error in result.RuleViolations)
            {
                messageStore.Add(_editContext.Field(string.Empty), $"{error.RuleID} - {error.RuleViolation}");
            }
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return;
        }
        else if (string.IsNullOrWhiteSpace(result.ConfirmationNumber))
        {
            _editContext = new EditContext(_taxDetailsData);
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty),
                Configuration["Messages:TechnicalDifficulties"]
                ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return;
        }

        await JS.InvokeVoidAsync("scrollToTop");
        _confirmationNumber = result.ConfirmationNumber;
        _pageState = PageState.Confirmation;
    }

    private static (string m1, string m2, string m3) GetMonthNamesFromQuarter(int quarter)
    {
        if (quarter is < 1 or > 4)
        {
            return ("", "", "");
        }
        var startMonth = ((quarter - 1) * 3) + 1;
        return (
            new DateTime(2000, startMonth, 1).ToString("MMMM"),
            new DateTime(2000, startMonth + 1, 1).ToString("MMMM"),
            new DateTime(2000, startMonth + 2, 1).ToString("MMMM")
        );
    }

    private async Task HandleActionClick()
    {
        if (_currentStep == 1)
        {
            if (_subjectivityStepRef != null && !_subjectivityStepRef.IsValid())
            {
                _showValidationSummary = true;

                return;
            }

            _showValidationSummary = false;
            _currentStep = 2;
            StateHasChanged();
        }
        else if (_currentStep == 2)
        {
            await HandleSubmitClick();
        }
    }

    private void HandleSubjectivityEdit()
    {
        _currentStep = 1;
        _showValidationSummary = false;
        StateHasChanged();
    }

    private void HandleBackClick()
    {
        if (!_hasSubjectivityStep || _currentStep == 1)
        {
            _showLeaveModal = true;
        }
    }
}

