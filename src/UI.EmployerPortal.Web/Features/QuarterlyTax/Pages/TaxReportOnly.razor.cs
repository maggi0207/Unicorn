using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.TaxEntry;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Helpers;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code-behind for the TaxReportOnly wizard page.
/// </summary>
public partial class TaxReportOnly
{
    [Inject]
    private ITaxExclusionThresholdService TaxExclusionThresholdService { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    public enum PageState
    {
        /// <summary>
        /// wizard page
        /// </summary>
        Wizard,
        /// <summary>
        /// confimation
        /// </summary>
        Confirmation
    }

    [SupplyParameterFromQuery(Name = "source")]
    private string? Source { get; set; }
    private bool IsTaxWageUploadFlow => string.Equals(Source, "tax-wage-upload", StringComparison.OrdinalIgnoreCase);
    private PageState _pageState = PageState.Wizard;
    private string? _confirmationNumber;
    private EditContext _editContext = default!;
    private bool _showValidationSummary;
    private readonly Dictionary<string, string> _validationFieldIds = new();
    private MissingReportModel? _missingReportModel;
    private int _currentStep = 1;
    private TaxEntry? _taxEntryRef;
    private bool _showLeaveModal;
    private bool _showSaveAndQuitModal;
    private bool _showUnsavedChangesModal;
    private bool _isLoading = true;
    private readonly string _sourceText = "Internet - Nelnet";
    private WageTaxFilingPayrollQuestionsResponse? _subjectivityResponse;
    private Dictionary<int, string> _subjectivityAnswers = new();
    private SubjectivityQuestionsStep? _subjectivityStepRef;
    private bool _hasSubjectivityStep;

    private readonly List<WizardStep> _wizardSteps = new()
    {
        new() { StepNumber = 1, Title = "Enter Taxes", ActionButtonText = "Continue"      },
        new() { StepNumber = 2, Title = "Verify Information", ActionButtonText = "Submit Report" }
    };

    private readonly TaxReportOnlyModel _reportData = new();

    /// <summary>
    /// 
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        //This is required for razor template rdners, it will be replaced by real data in OnAfterRenderAsync.
        _editContext = new EditContext(_reportData);
    }

    /// <inheritdoc />
       /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _missingReportModel = await QuarterlyReportOrchestrator.GetMissingReportFromSessionAsync();
            if (_missingReportModel is null)
            {
                NavigationManager.NavigateTo("quarterly-tax/missing-reports");
                return;
            }

            var filingMethod = await TaxAndWageEntryService.GetAvailableFilingMethod(_missingReportModel!, TaxWageFilingType.TaxEntry);
            if (filingMethod is null || !filingMethod.IsEligible)
            {
                NavigationManager.NavigateTo("quarterly-tax/select-report");
                return;
            }

            _reportData.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
            _reportData.DueDate = _missingReportModel.DueDate ?? DateTime.Today;
            _reportData.TaxYear = _missingReportModel.Year;
            _reportData.Quarter = _missingReportModel.Quarter;
            _reportData.FilingType = IsTaxWageUploadFlow ? TaxWageFilingType.TaxAndWageUpload : TaxWageFilingType.TaxEntry;

            _reportData.TaxEntryData.ReportingQuarter = _reportData.ReportingQuarter;
            _reportData.TaxEntryData.DueDate = _reportData.DueDate;
            _reportData.TaxEntryData.TaxYear = _missingReportModel.Year;
            _reportData.TaxEntryData.Quarter = _missingReportModel.Quarter;

            var taxRate = await TaxAndWageEntryService.GetTaxRateForYear(_missingReportModel.Year);
            _reportData.TaxEntryData.TaxRate = taxRate.TotalRate;

            // 1. Always load previously reported wages so AllowDirectExclusionEntry evaluates correctly
            List<CalculateWageTaxFilingEmployeeProxy> employeesfromWageReport = [];
            var wageReport = await TaxAndWageEntryService.GetWageReportByQuarterYear(_missingReportModel.Year, _missingReportModel.Quarter);
            if (wageReport is not null)
            {
                _reportData.TaxEntryData.PreviouslyReportedGrossWages = wageReport.GrossWages;
                _reportData.TaxEntryData.IsAuditSource = wageReport.WageDetails?.Any(d => d.AuditSourceCode) == true;
                if (!_missingReportModel.PendingWageTaxFilingSK.HasValue)
                {
                    _reportData.TaxEntryData.TotalGrossCoveredWages = wageReport.GrossWages;
                }

                // Fill up employees from wage report
                foreach (var item in wageReport.WageDetails!)
                {
                    employeesfromWageReport.Add(new CalculateWageTaxFilingEmployeeProxy()
                    {
                        SSN = item.SSN,
                        WageAmount = item.GrossWages ?? item.PreviousWage ?? 0m
                    });
                }
            }

            if (employeesfromWageReport.Count > 0 && !_missingReportModel.PendingWageTaxFilingSK.HasValue)
            {
                var calculatedExclusion = await TaxAndWageEntryService.GetCalculatedExclusionAmountAsync(_missingReportModel, employeesfromWageReport);
                _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount = calculatedExclusion;
            }

            // 2. If resuming a pending report, overlay the saved user entries on top of the baseline
            if (_missingReportModel.PendingWageTaxFilingSK.HasValue)
            {
                var pendingReport = await TaxAndWageEntryService.GetPendingTaxReportByWageTaxFilingSK(_missingReportModel.PendingWageTaxFilingSK.Value);
                if (pendingReport is not null)
                {
                    _reportData.WageTaxFilingSK = pendingReport.WageTaxFilingSK;
                    _reportData.TaxEntryData.Quarter = pendingReport.Quarter ?? _missingReportModel!.Quarter;
                    _reportData.TaxEntryData.TaxYear = pendingReport.Year ?? _missingReportModel!.Year;
                    _reportData.TaxEntryData.EmployeeCountMonth1 = pendingReport.Month1EmployeeCount ?? 0;
                    _reportData.TaxEntryData.EmployeeCountMonth2 = pendingReport.Month2EmployeeCount ?? 0;
                    _reportData.TaxEntryData.EmployeeCountMonth3 = pendingReport.Month3EmployeeCount ?? 0;
                    _reportData.TaxEntryData.TotalGrossCoveredWages = pendingReport.GrossWages ?? 0m;
                    _reportData.TaxEntryData.GrossWageDiscrepancyExplanation = pendingReport.GrossWageOutOfBalanceExplanation;
                    _reportData.TaxEntryData.ExclusionOverride.OverrideAmount = pendingReport.Exclusions ?? 0m;
                    _reportData.TaxEntryData.ExclusionOverride.OverrideReason = pendingReport.ExclusionAmountReason;
                    _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount = pendingReport.CalculatedExclusionAmount ?? _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount;
                    _reportData.TaxEntryData.ExclusionOverride.IsOverride =
                    !string.IsNullOrEmpty(pendingReport.ExclusionAmountReason)
                    || ((pendingReport.Exclusions ?? 0m) != _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount);
                    _reportData.TaxEntryData.DefinedTaxablePayroll = pendingReport.TaxablePayrollAmount ?? 0;
                    _reportData.TaxEntryData.WageTaxFilingSK = pendingReport.WageTaxFilingSK;
                }
            }

            _reportData.TaxEntryData.AllowDirectExclusionEntry = !_reportData.TaxEntryData.PreviouslyReportedGrossWages.HasValue;

            _editContext = new EditContext(_reportData);

            await TaxWageExclusionThresholdHelper.TrySetExclusionThresholdAsync(
            TaxExclusionThresholdService,
            threshold =>
            {
                _reportData.ExclusionThreshold = threshold;
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
    }


    // Builds TaxDetailsData for step 2 from the live _reportData.TaxEntryData so that any
    // edits made in step 1 are reflected in the verification view.
    private TaxDetailsModel BuildTaxDetailsData()
    {
        var (m1, m2, m3) = GetMonthNamesFromQuarter(_missingReportModel!.Quarter);

        return new TaxDetailsModel
        {
            FirstMonthName = m1,
            SecondMonthName = m2,
            ThirdMonthName = m3,
            EmployeeCount1stMonth = _reportData.TaxEntryData.EmployeeCountMonth1,
            EmployeeCount2ndMonth = _reportData.TaxEntryData.EmployeeCountMonth2,
            EmployeeCount3rdMonth = _reportData.TaxEntryData.EmployeeCountMonth3,
            TotalGrossCoveredWages = _reportData.TaxEntryData.TotalGrossCoveredWages,
            LessExclusions = _reportData.TaxEntryData.ExclusionOverride.EffectiveAmount,
            ExclusionThreshold = _reportData.ExclusionThreshold,
            DefinedPayroll = _reportData.TaxEntryData.DefinedTaxablePayroll,
            TaxRate = _reportData.TaxEntryData.TaxRate,
            TaxYear = _missingReportModel.Year,
            TaxAssessed = _reportData.TaxEntryData.TaxAssessed,
            ExclusionOverrideReason =
                _reportData.TaxEntryData.ExclusionOverride.IsOverride
                ? _reportData.TaxEntryData.ExclusionOverride.OverrideReason
                : null,
            PreviouslyReportedGrossWages = _reportData.TaxEntryData.PreviouslyReportedGrossWages,
            GrossWageDiscrepancyExplanation = _reportData.TaxEntryData.GrossWageDiscrepancyExplanation,
            IsAuditSource = _reportData.TaxEntryData.IsAuditSource
        };
    }

    // Derives the three month names for a quarter string such as "Q2 2022".
    // Mirrors the logic in TaxEntry so both components stay in sync.
    private static (string m1, string m2, string m3) GetMonthNamesFromQuarter(int quarter)
    {
        var startMonth = ((quarter - 1) * 3) + 1;
        return (
            new DateTime(2000, startMonth, 1).ToString("MMMM"),
            new DateTime(2000, startMonth + 1, 1).ToString("MMMM"),
            new DateTime(2000, startMonth + 2, 1).ToString("MMMM")
        );
    }

    // Returns the user to step 1 (TaxEntry) when Edit is clicked in TaxDetails.
    private void HandleTaxDetailsEdit()
    {
        _currentStep = 1;
    }

    private Task HandleCancel()
    {
        _showUnsavedChangesModal = true;
        return Task.CompletedTask;
    }

    private void HandleUnsavedChangesStayOnReport()
    {
        _showUnsavedChangesModal = false;
    }

    private async Task HandleUnsavedChangesCancelReport()
    {
        _showUnsavedChangesModal = false;
        if (_missingReportModel?.PendingWageTaxFilingSK.HasValue == true)
        {
            _isLoading = true;
            StateHasChanged();
            var response = await TaxAndWageEntryService.DeletePendingTaxReportByWageTaxFilingSK(_missingReportModel!.PendingWageTaxFilingSK!.Value);

            if (response.RuleViolations.Length > 0)
            {
                _isLoading = false;
                _editContext = new EditContext(_reportData);
                var messageStore = new ValidationMessageStore(_editContext);
                foreach (var error in response.RuleViolations)
                {
                    messageStore.Add(_editContext.Field(string.Empty), $"{error.RuleID} {error.RuleViolation}");
                }
                _showValidationSummary = true;
                _editContext.NotifyValidationStateChanged();
                return;
            }
        }

        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    /// <summary>
    /// enables in step1 and when clicked navigate to missing reports
    /// </summary>    
    private void HandleBackClick()
    {
        if (_currentStep == 1)
        {
            _showLeaveModal = true;
        }
    }

    private void HandleLeaveModalClose()
    {
        _showLeaveModal = false;
    }

    private void HandleLeaveModalConfirm()
    {
        _showLeaveModal = false;
        var destination = _missingReportModel?.PendingWageTaxFilingSK.HasValue == true
           ? "quarterly-tax/missing-reports"
           : "quarterly-tax/select-report";
        NavigationManager.NavigateTo(destination);
    }

    private Task HandleSaveAndQuitClick()
    {
        _showSaveAndQuitModal = true;
        return Task.CompletedTask;
    }

    private void HandleSaveAndQuitModalClose()
    {
        _showSaveAndQuitModal = false;
    }

    private async Task HandleSaveAndQuitModalConfirm()
    {
        _showSaveAndQuitModal = false;
        _isLoading = true;
        StateHasChanged();

        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

        var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod(_reportData.FilingType);
        if (filingMethod == null)
        {
            _isLoading = false;
            _editContext = new EditContext(_reportData);
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty), "Cannot find Filing Method.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return;
        }

        var filing = new WageTaxFilingProxy
        {
            FilingTypeCodeSK = filingMethod!.CodeSK,
            EmployerSK = account != null ? account.Id : 0,
            Quarter = _missingReportModel?.Quarter,
            Year = _missingReportModel?.Year,
            Month1EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth1,
            Month2EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth2,
            Month3EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth3,
            GrossWages = _reportData.TaxEntryData.TotalGrossCoveredWages,
            Exclusions = _reportData.TaxEntryData.ExclusionOverride.EffectiveAmount,
            ExclusionAmountReason = _reportData.TaxEntryData.ExclusionOverride.OverrideReason,
            CalculatedExclusionAmount = _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount,
            GrossWageOutOfBalanceExplanation = _reportData.TaxEntryData.GrossWageDiscrepancyExplanation,
            TaxablePayrollAmount = _reportData.TaxEntryData.DefinedTaxablePayroll,
            WageTaxFilingSK = _reportData.WageTaxFilingSK,
            EffectiveDate = DateTime.Now,
            Employees = [],
            SourceText = _sourceText,
            ReportingSelectionCodeSK = (int) _reportData.FilingType,
        };

        var response = await TaxAndWageEntryService.SaveTaxReportAsync(filing);

        if (response.RuleViolations.Length > 0)
        {
            _isLoading = false;
            _editContext = new EditContext(_reportData);
            var messageStore = new ValidationMessageStore(_editContext);
            foreach (var error in response.RuleViolations)
            {
                messageStore.Add(_editContext.Field(string.Empty), $"{error.RuleID} {error.RuleViolation}");
            }
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return;
        }

        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }

    private async Task HandleActionClick()
    {
        if (_currentStep == 1)
        {
            if (_taxEntryRef != null && !await _taxEntryRef.IsValid())
            {
                return;
            }
        }

        _showValidationSummary = false;
        _isLoading = true;
        StateHasChanged();
        await Task.Yield();
        await OnNextAsync();
        _isLoading = false;
    }

    private async Task OnNextAsync()
    {
        var isSuccess = await ProcessCurrentStepAsync();

        if (isSuccess && _currentStep < _wizardSteps.Count)
        {
            _currentStep++;
        }
    }

    private async Task<bool> ProcessCurrentStepAsync()
    {
        switch (_currentStep)
        {
            case 1:
                _reportData.TaxDetailsData = BuildTaxDetailsData();
                // Call WCF Service to check subjectivity questions
                _subjectivityResponse = await TaxAndWageEntryService.GetSubjectivityQuestionsAsync(
                    _reportData.TaxEntryData.TotalGrossCoveredWages,
                    _reportData.Quarter,
                    _reportData.TaxYear);

                if (HasActualQuestions(_subjectivityResponse))
                {
                    if (!_hasSubjectivityStep)
                    {
                        _hasSubjectivityStep = true;
                        _wizardSteps.Insert(1, new WizardStep { StepNumber = 2, Title = "Subjectivity Questions", ActionButtonText = "Continue" });
                        for (var i = 0; i < _wizardSteps.Count; i++)
                        {
                            _wizardSteps[i].StepNumber = i + 1;
                        }
                    }
                }
                else
                {
                    // If subjectivity questions no longer apply, remove the step
                    if (_hasSubjectivityStep)
                    {
                        _hasSubjectivityStep = false;
                        _subjectivityAnswers.Clear();
                        _wizardSteps.RemoveAll(s => s.Title == "Subjectivity Questions");
                        for (var i = 0; i < _wizardSteps.Count; i++)
                        {
                            _wizardSteps[i].StepNumber = i + 1;
                        }
                    }
                }
                return true;

            case 2:
                if (_hasSubjectivityStep)
                {
                    if (_subjectivityStepRef != null && !_subjectivityStepRef.IsValid())
                    {
                        _showValidationSummary = true;
                        return false;
                    }
                    return true;
                }
                return await SubmitFilingReportAsync();
            case 3:
                return await SubmitFilingReportAsync();
        }
        return true;
    }
    private async Task<bool> SubmitFilingReportAsync()
    {
        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        if (account == null)
        {
            return false;
        }

        var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod(_reportData.FilingType);
        if (filingMethod == null)
        {
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty), "Cannot find Filing Method.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            _isLoading = false;
            return false;
        }

        var filing = new WageTaxFilingProxy
        {
            EmployerSK = account != null ? account.Id : 0,
            FilingTypeCodeSK = filingMethod!.CodeSK,
            FilingTypeDescription = filingMethod!.ShortDescription,
            Quarter = _missingReportModel?.Quarter,
            Year = _missingReportModel?.Year,
            Month1EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth1,
            Month2EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth2,
            Month3EmployeeCount = _reportData.TaxEntryData.EmployeeCountMonth3,
            GrossWages = _reportData.TaxEntryData.TotalGrossCoveredWages,
            Exclusions = _reportData.TaxEntryData.ExclusionOverride.EffectiveAmount,
            ExclusionAmountReason = _reportData.TaxEntryData.ExclusionOverride.OverrideReason,
            CalculatedExclusionAmount = _reportData.TaxEntryData.ExclusionOverride.CalculatedExclusionAmount,
            GrossWageOutOfBalanceExplanation = _reportData.TaxEntryData.GrossWageDiscrepancyExplanation ?? string.Empty,
            TaxablePayrollAmount = _reportData.TaxEntryData.DefinedTaxablePayroll,
            WageTaxFilingSK = _reportData.WageTaxFilingSK,
            EffectiveDate = DateTime.Now,
            SourceText = _sourceText,
            ReportingSelectionCodeSK = (int) _reportData.FilingType,
            // Attach Subjectivity Question Responses
            PayrollQuestionResponses = _subjectivityAnswers
                .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
                .Select(kvp => new PayrollQuestionResponseProxy
                {
                    EmployerPortalQuestionSK = kvp.Key,
                    QuestionResponseText = kvp.Value
                }).ToArray()
        };

        var result = await TaxAndWageEntryService.SubmitTaxReportAsync(filing);

        if (result.RuleViolations != null && result.RuleViolations.Any())
        {
            var messageStore = new ValidationMessageStore(_editContext);
            foreach (var error in result.RuleViolations)
            {
                messageStore.Add(_editContext.Field(string.Empty), error.RuleViolation);
            }
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return false;
        }
        else if (string.IsNullOrWhiteSpace(result.ConfirmationNumber))
        {
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty),
                Configuration["Messages:TechnicalDifficulties"]
                ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return false;
        }

        await JS.InvokeVoidAsync("scrollToTop");
        await QuarterlyReportOrchestrator.ClearMissingReportFromSessionAsync();
        _missingReportModel = null;
        _confirmationNumber = result.ConfirmationNumber;
        _pageState = PageState.Confirmation;
        return true;
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

    private void HandleSubjectivityEdit()
    {
        _currentStep = 2;
    }
}
