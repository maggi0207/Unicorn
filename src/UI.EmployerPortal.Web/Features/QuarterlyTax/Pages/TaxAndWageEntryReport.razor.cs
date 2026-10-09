using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.EmployeeWageEntry;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.TaxEntry;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Helpers;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using WageTaxFilingEmployeeProxy = UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService.WageTaxFilingEmployeeProxy;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code behind for Tax and Wage Entry Report wizard.
/// Displays a paginated, sortable table for employer accounts.
/// Users in-memory sorting/pagination for fast client-side interactions.
/// </summary>

public partial class TaxAndWageEntryReport
{
    [Inject]
    private ITaxExclusionThresholdService TaxExclusionThresholdService { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private WageTaxFilingPayrollQuestionsResponse? _subjectivityResponse;
    private Dictionary<int, string> _subjectivityAnswers = new();
    private SubjectivityQuestionsStep? _subjectivityStepRef;
    private bool _hasSubjectivityStep;
    private TaxAndWageEntryReportModel _reportModel = new();
    private EditContext _editContext = default!;
    private int _currentStep = 1;
    private bool _showValidationSummary;
    private EmployeeWageEntry? _wageEntryRef;
    private TaxEntry? _taxEntryRef;
    private MissingReportModel? _missingReportModel = new();
    private readonly Dictionary<string, string> _validationFieldIds = [];
    private WageTaxFilingResponse? _taxFilingResponse;
    private bool _showNavButtons = true;
    private bool _showLeaveModal;
    private bool _showSaveAndQuitModal;
    private bool _showUnsavedChangesModal;
    private bool _isLoading = true;
    private readonly string _sourceText = "Internet - Nelnet";

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

    private PageState _pageState = PageState.Wizard;

    private readonly List<WizardStep> _wizardSteps =
    [
        new WizardStep {StepNumber = 1, Title="Emplyee Wage", ActionButtonText="Continue" },
        new WizardStep {StepNumber = 2, Title="Enter Taxes", ActionButtonText="Continue" },
        new WizardStep {StepNumber = 3, Title="Verify Information", ActionButtonText="Submit" },
    ];

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        //This is required for razor template rdners, it will be replaced by real data in OnAfterRenderAsync.
        _editContext = new EditContext(_reportModel);
    }

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

            var filingMethod = await TaxAndWageEntryService.GetAvailableFilingMethod(_missingReportModel!, TaxWageFilingType.TaxAndWageEntry);
            if (filingMethod is null || !filingMethod.IsEligible)
            {
                NavigationManager.NavigateTo("quarterly-tax/select-report");
                return;
            }

            _reportModel = await TaxAndWageEntryService.GetTaxAndWageEntryReportDataAsync(_missingReportModel.PendingWageTaxFilingSK, _missingReportModel);
            _editContext = new EditContext(_reportModel);

            _reportModel.WageEntry.Quarter = _missingReportModel.Quarter;
            _reportModel.WageEntry.TaxYear = _missingReportModel.Year;
            _reportModel.WageEntry.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
            _reportModel.WageEntry.DueDate = _missingReportModel.DueDate ?? DateTime.Today;

            _reportModel.TaxEntry.Quarter = _missingReportModel.Quarter;
            _reportModel.TaxEntry.TaxYear = _missingReportModel.Year;
            _reportModel.TaxEntry.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
            _reportModel.TaxEntry.DueDate = _missingReportModel.DueDate ?? DateTime.Today;

            _reportModel.TaxEntry.ReportName = _missingReportModel.ReportName;
            _reportModel.TaxEntry.Description = _missingReportModel.Description;

            var taxRate = await TaxAndWageEntryService.GetTaxRateForYear(_reportModel.TaxEntry.TaxYear);
            _reportModel.TaxEntry.TaxRate = taxRate.TotalRate;

            _reportModel.Quarter = _missingReportModel.Quarter;
            _reportModel.TaxYear = _missingReportModel.Year;
            _reportModel.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
            _reportModel.DueDate = _missingReportModel.DueDate ?? DateTime.Today;
            _reportModel.FilingType = TaxWageFilingType.TaxAndWageEntry;

            await TaxWageExclusionThresholdHelper.TrySetExclusionThresholdAsync(
            TaxExclusionThresholdService,
            threshold =>
            {
                _reportModel.TaxDetails.ExclusionThreshold = threshold;
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

    private async Task HandleActionClick()
    {
        if (_currentStep == 1)
        {
            if (_wageEntryRef != null && !await _wageEntryRef.IsValid())
            {
                return;
            }
        }
        else if (_currentStep == 2)
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

    private void HandleTaxDetailsEdit()
    {
        _currentStep = 2;
    }

    private void HandleWageDetailsEdit()
    {
        _currentStep = 1;
    }

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

    private Task HandleCancelClick()
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
                _editContext = new EditContext(_reportModel);
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

        NavigationManager.NavigateTo("quarterly-tax/missing-reports", true);
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

        var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod(_reportModel.FilingType);
        if (filingMethod == null)
        {
            _isLoading = false;
            _editContext = new EditContext(_reportModel);
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
            Month1EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth1,
            Month2EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth2,
            Month3EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth3,
            GrossWages = _reportModel.TaxEntry.TotalGrossCoveredWages,
            Exclusions = _reportModel.TaxEntry.ExclusionOverride.EffectiveAmount,
            ExclusionAmountReason = _reportModel.TaxEntry.ExclusionOverride.OverrideReason,
            CalculatedExclusionAmount = _reportModel.TaxEntry.ExclusionOverride.CalculatedExclusionAmount,
            GrossWageOutOfBalanceExplanation = _reportModel.TaxEntry.GrossWageDiscrepancyExplanation,
            TaxablePayrollAmount = _reportModel.TaxEntry.DefinedTaxablePayroll,
            WageTaxFilingSK = _reportModel.WageTaxFilingSK,
            EffectiveDate = DateTime.Now,
            SourceText = _sourceText,
            ReportingSelectionCodeSK = (int) TaxWageFilingType.TaxAndWageEntry,
            Employees = [.. _reportModel.WageEntry.Employees.Select(e =>
            {
                return new WageTaxFilingEmployeeProxy
                {
                    FirstName = e.FirstName,
                    LastName = e.LastName,
                    SaveFlag = e.SaveForNextQuarter,
                    SSN = e.SSN,
                    WageAmount = e.QuarterlyWages,
                    WageTaxFilingSK = (long?)e.WageTaxFilingSK,
                    WageTaxFilingEmployeeSK = e.Id == 0 ? null : e.Id
                };
            })]
        };

        var response = await TaxAndWageEntryService.SaveTaxReportAsync(filing);

        if (response.RuleViolations.Length > 0)
        {
            _isLoading = false;
            _editContext = new EditContext(_reportModel);
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

    private Task HandleExclusionsChanged(decimal exclusionAmount)
    {
        return Task.CompletedTask;
    }

    private async Task HandleValidSubmit()
    {
        await Task.CompletedTask;
    }

    private async Task OnNextAsync()
    {
        var ok = await ProcessCurrentStepAsync();

        if (ok)
        {
            if (_currentStep < _wizardSteps.Count)
            {
                _currentStep++;
            }
            StateHasChanged();
        }
    }

    private async Task<bool> ProcessCurrentStepAsync()
    {
        switch (_currentStep)
        {
            case 1:
                var totalWages = _reportModel.WageEntry.Employees.Sum(e =>
                {
                    // Null means "not entered yet"; it contributes nothing to the carried-over total.
                    return e.QuarterlyWages ?? 0m;
                });
                _reportModel.TaxEntry.TotalGrossCoveredWages = totalWages;
                _reportModel.TaxEntry.DefinedTaxablePayroll = totalWages;

                var employeesForExclustion = _reportModel.WageEntry.Employees.Select(e =>
                {
                    return new CalculateWageTaxFilingEmployeeProxy
                    {
                        SSN = e.SSN,
                        WageAmount = e.QuarterlyWages ?? 0m
                    };
                }).ToList();

                var calculatedExclusion = await TaxAndWageEntryService.GetCalculatedExclusionAmountAsync(_missingReportModel!, employeesForExclustion);
                _reportModel.TaxEntry.ExclusionOverride.CalculatedExclusionAmount = calculatedExclusion;
                return true;

            case 2:
                BuildWageDetailsData();
                BuildTaxDetailsData();

                // Call WCF Service to check subjectivity questions
                _subjectivityResponse = await TaxAndWageEntryService.GetSubjectivityQuestionsAsync(
                    _reportModel.TaxEntry.TotalGrossCoveredWages,
                    _reportModel.Quarter,
                    _reportModel.TaxYear);

                if (HasActualQuestions(_subjectivityResponse))
                {
                    if (!_hasSubjectivityStep)
                    {
                        _hasSubjectivityStep = true;
                        _wizardSteps.Insert(2, new WizardStep { StepNumber = 3, Title = "Subjectivity Questions", ActionButtonText = "Continue" });
                        for (var i = 3; i < _wizardSteps.Count; i++)
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

            case 3:
                if (_hasSubjectivityStep)
                {
                    if (_subjectivityStepRef != null && !_subjectivityStepRef.IsValid())
                    {
                        _showValidationSummary = true;
                        return false;
                    }
                    return true;
                }
                // If no subjectivity step, fall through to submit
                return await SubmitFilingReportAsync();
            case 4:
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

        var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod(_reportModel.FilingType);
        if (filingMethod == null)
        {
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty), "Cannot find Filing Method.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return false;
        }

        var filing = new WageTaxFilingProxy
        {
            EmployerSK = account.Id,
            Quarter = _missingReportModel!.Quarter,
            Year = _missingReportModel!.Year,
            Month1EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth1,
            Month2EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth2,
            Month3EmployeeCount = _reportModel.TaxEntry.EmployeeCountMonth3,
            GrossWages = _reportModel.TaxEntry.TotalGrossCoveredWages,
            Exclusions = _reportModel.TaxEntry.ExclusionOverride.EffectiveAmount,
            ExclusionAmountReason = _reportModel.TaxEntry.ExclusionOverride.OverrideReason,
            CalculatedExclusionAmount = _reportModel.TaxEntry.ExclusionOverride.CalculatedExclusionAmount,
            GrossWageOutOfBalanceExplanation = _reportModel.TaxEntry.GrossWageDiscrepancyExplanation ?? string.Empty,
            TaxablePayrollAmount = _reportModel.TaxEntry.DefinedTaxablePayroll,
            FilingTypeCodeSK = filingMethod!.CodeSK,
            FilingTypeDescription = filingMethod!.ShortDescription,
            ReportingSelectionCodeSK = (int) TaxWageFilingType.TaxAndWageEntry,
            EffectiveDate = DateTime.Now,
            WageTaxFilingSK = _reportModel.WageTaxFilingSK,
            SourceText = _sourceText,

            // Pass Subjectivity Question Responses
            PayrollQuestionResponses = _subjectivityAnswers
        .Where(kvp => !string.IsNullOrWhiteSpace(kvp.Value))
        .Select(kvp => new PayrollQuestionResponseProxy
        {
            EmployerPortalQuestionSK = kvp.Key,
            QuestionResponseText = kvp.Value
        }).ToArray(),

            Employees = _reportModel.WageEntry.Employees.Select(employee =>
            {
                return new WageTaxFilingEmployeeProxy
                {
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    SaveFlag = employee.SaveForNextQuarter,
                    SSN = employee.SSN,
                    WageAmount = employee.QuarterlyWages,
                    WageTaxFilingSK = (long?) employee.WageTaxFilingSK,
                    WageTaxFilingEmployeeSK = employee.Id == 0 ? null : employee.Id
                };
            }).ToArray(),
        };

        var result = await TaxAndWageEntryService.SubmitTaxReportAsync(filing);

        if (result.RuleViolations.Any())
        {
            _editContext = new EditContext(result);

            var messageStore = new ValidationMessageStore(_editContext);
            foreach (var error in result.RuleViolations)
            {
                messageStore.Add(_editContext.Field(string.Empty), error.RuleID + " " + error.RuleViolation);
            }
            _editContext.NotifyValidationStateChanged();

            _showValidationSummary = true;

            return false;
        }
        else if (string.IsNullOrWhiteSpace(result.ConfirmationNumber))
        {
            _editContext = new EditContext(_reportModel);
            var messageStore = new ValidationMessageStore(_editContext);
            messageStore.Add(_editContext.Field(string.Empty),
                Configuration["Messages:TechnicalDifficulties"]
                ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
            return false;
        }
        else
        {
            await JS.InvokeVoidAsync("scrollToTop");
            await QuarterlyReportOrchestrator.ClearMissingReportFromSessionAsync();
            _missingReportModel = null;
            _taxFilingResponse = result;
            _showValidationSummary = false;
            _showNavButtons = false;
            _pageState = PageState.Confirmation;
        }

        return true;
    }

    private void BuildWageDetailsData()
    {
        _reportModel.WageDetails = new WageDetailsModel
        {
            Employees = _reportModel.WageEntry.Employees.Select(e =>
            {
                return new EmployeeWageData
                {
                    LastName = e.LastName,
                    FirstName = e.FirstName,
                    SSN = e.SSN,
                    QuarterlyWages = e.QuarterlyWages ?? 0m,
                    SaveForNextQuarter = e.SaveForNextQuarter ? "Yes" : "No"
                };
            }).ToList()
        };
    }
    private void BuildTaxDetailsData()
    {
        _reportModel.TaxDetails.ReportingQuarter = _reportModel.ReportingQuarter;
        _reportModel.TaxDetails.DueDate = _reportModel.DueDate;
        _reportModel.TaxDetails.TaxYear = _reportModel.TaxYear;
        _reportModel.TaxDetails.Quarter = _reportModel.Quarter;

        _reportModel.TaxDetails.TaxAssessed = _reportModel.TaxEntry.TaxAssessed;
        _reportModel.TaxDetails.EmployeeCount1stMonth = _reportModel.TaxEntry.EmployeeCountMonth1;
        _reportModel.TaxDetails.EmployeeCount2ndMonth = _reportModel.TaxEntry.EmployeeCountMonth2;
        _reportModel.TaxDetails.EmployeeCount3rdMonth = _reportModel.TaxEntry.EmployeeCountMonth3;
        _reportModel.TaxDetails.FirstMonthName = _reportModel.TaxEntry.MonthName1;
        _reportModel.TaxDetails.SecondMonthName = _reportModel.TaxEntry.MonthName2;
        _reportModel.TaxDetails.ThirdMonthName = _reportModel.TaxEntry.MonthName3;
        _reportModel.TaxDetails.TaxRate = _reportModel.TaxEntry.TaxRate;
        _reportModel.TaxDetails.LessExclusions = _reportModel.TaxEntry.ExclusionOverride.EffectiveAmount;
        _reportModel.TaxDetails.DefinedPayroll = _reportModel.TaxEntry.DefinedTaxablePayroll;
        _reportModel.TaxDetails.TotalGrossCoveredWages = _reportModel.WageDetails.Employees.Sum(e =>
        {
            return e.QuarterlyWages;
        });
        _reportModel.TaxDetails.ExclusionOverrideReason =
            _reportModel.TaxEntry.ExclusionOverride.IsOverride
            ? _reportModel.TaxEntry.ExclusionOverride.OverrideReason
            : null;
        _reportModel.TaxDetails.PreviouslyReportedGrossWages = _reportModel.TaxEntry.PreviouslyReportedGrossWages;
        _reportModel.TaxDetails.GrossWageDiscrepancyExplanation = _reportModel.TaxEntry.GrossWageDiscrepancyExplanation;
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
        _currentStep = 3;
    }
}
