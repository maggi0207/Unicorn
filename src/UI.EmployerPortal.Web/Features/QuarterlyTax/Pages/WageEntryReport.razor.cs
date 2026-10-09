using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.EmployeeWageEntry;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using WageTaxFilingEmployeeProxy = UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService.WageTaxFilingEmployeeProxy;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code behind for Wage Entry Report wizard.
/// Displays a paginated, sortable table for employer accounts.
/// Users in-memory sorting/pagination for fast client-side interactions.
/// </summary>

public partial class WageEntryReport
{
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
    private IJSRuntime JS { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    private TaxAndWageEntryReportModel _reportModel = new();
    private EditContext _editContext = default!;
    private int _currentStep = 1;
    private bool _showValidationSummary;
    private EmployeeWageEntry? _wageEntryRef;
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
        new WizardStep {StepNumber = 1, Title = "Enter Wages", ActionButtonText="Continue" },
        new WizardStep {StepNumber = 2, Title = "Review Taxes", ActionButtonText="Continue" },
        new WizardStep {StepNumber = 3, Title = "Verify Information", ActionButtonText="Submit" },
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

            if (_missingReportModel is not null)
            {
                var filingMethod = await TaxAndWageEntryService.GetAvailableFilingMethod(_missingReportModel!, TaxWageFilingType.WageEntry);
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

                _reportModel.TaxEntry = await TaxAndWageEntryService.GetTaxReportByYearAndQuarterAsync(_missingReportModel.Year, _missingReportModel.Quarter);
                _reportModel.TaxEntry.Quarter = _missingReportModel.Quarter;
                _reportModel.TaxEntry.TaxYear = _missingReportModel.Year;
                _reportModel.TaxEntry.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
                _reportModel.TaxEntry.DueDate = _missingReportModel.DueDate ?? DateTime.Today;
                _reportModel.TaxEntry.ReportName = _missingReportModel.ReportName;
                _reportModel.TaxEntry.Description = _missingReportModel.Description;

                _reportModel.Quarter = _missingReportModel.Quarter;
                _reportModel.TaxYear = _missingReportModel.Year;
                _reportModel.ReportingQuarter = _missingReportModel.FormattedQuarterYear;
                _reportModel.DueDate = _missingReportModel.DueDate ?? DateTime.Today;
                _reportModel.FilingType = TaxWageFilingType.WageEntry;
            }
            else
            {
                NavigationManager.NavigateTo("quarterly-tax/missing-reports");
                return;
            }

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

        _showValidationSummary = false;
        _isLoading = true;
        StateHasChanged();
        await Task.Yield();
        await OnNextAsync();
        _isLoading = false;
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
            FilingTypeCodeSK = filingMethod.CodeSK,
            EmployerSK = account != null ? account.Id : 0,
            Quarter = _missingReportModel?.Quarter,
            Year = _missingReportModel?.Year,
            GrossWages = _reportModel.WageEntry.TotalGrossWages,
            EffectiveDate = DateTime.Now,
            WageTaxFilingSK = _reportModel.WageTaxFilingSK,
            SourceText = _sourceText,
            ReportingSelectionCodeSK = (int) TaxWageFilingType.WageEntry,
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
                return true;

            case 2:
                BuildWageDetailsData();
                return true;

            case 3:
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
                    ReportingSelectionCodeSK = (int) TaxWageFilingType.WageEntry,
                    EffectiveDate = DateTime.Now,
                    WageTaxFilingSK = _reportModel.WageTaxFilingSK,
                    SourceText = _sourceText,
                    Employees = _reportModel.WageEntry.Employees.Select(employee =>
                    {
                        return new WageTaxFilingEmployeeProxy
                        {
                            FirstName = employee.FirstName,
                            LastName = employee.LastName,
                            SaveFlag = true,
                            SSN = employee.SSN,
                            WageAmount = employee.QuarterlyWages,
                            WageTaxFilingSK = (long?) employee.WageTaxFilingSK,
                            WageTaxFilingEmployeeSK = employee.Id == 0 ? null : employee.Id,
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

                await JS.InvokeVoidAsync("scrollToTop");
                await QuarterlyReportOrchestrator.ClearMissingReportFromSessionAsync();
                _missingReportModel = null;
                _taxFilingResponse = result;
                _showValidationSummary = false;
                _showNavButtons = false;
                _pageState = PageState.Confirmation;

                return true;
        }

        return true;
    }
    private void BuildWageDetailsData()
    {
        var taxReportGrossWages = _reportModel.TaxEntry.TotalGrossCoveredWages;

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
            }).ToList(),
            TaxReportGrossWages = taxReportGrossWages
        };

        _reportModel.WageDetails.IsOutOfBalance =
            _reportModel.WageDetails.TotalGrossCoveredWages != taxReportGrossWages;
    }
}
