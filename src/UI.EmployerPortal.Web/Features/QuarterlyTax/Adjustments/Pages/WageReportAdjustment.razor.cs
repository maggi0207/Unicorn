using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Pages;
/// <summary>
/// Page for Wage Report Adjustments By Quarter
/// </summary>
public partial class WageReportAdjustment
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private IWageAdjustmentService WageAdjustmentService { get; set; } = default!;
    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;
    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    private PageState _pageState = PageState.Wizard;
    private bool _isSubmitting = false;
    private bool _isLoading = false;
    private int _currentStep = 1;
    private bool _showBackModel = false;
    private bool _showSaveAndQuitModal = false;
    private ValidationMessageStore _validationMessageStore = default!;
    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(WageAdjustmentModel.SelectedQuarterKey)] = "quarter-year-select",
    };
    private EditContext _editContext = new(new object());

    private bool _showValidationSummary;
    private List<string> _filedWageQuarters = new();
    private readonly WageAdjustmentModel _reportModel = new();
    private EmployeeWageAdjustments _employeeWageAdjustments = default!;
    private bool _showCancelModal = false;
    private readonly List<WizardStep> _wizardSteps = new()
    {
        new WizardStep { StepNumber = 1, Title = "Select Quarter & Year", ActionButtonText = "CONTINUE" },
        new WizardStep { StepNumber = 2,  Title = "Employees & Wage Adjustments", ActionButtonText = "CONTINUE"},
        new WizardStep { StepNumber = 3, Title = "Verify Information", ActionButtonText = "SUBMIT ADJUSTMENT"}
    };

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        _editContext = new EditContext(_reportModel);
        _validationMessageStore = new ValidationMessageStore(_editContext);
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isLoading = true;
            StateHasChanged();
            await LoadAdjustmentReasons();
            await LoadFiledWageQuarters();
            var response = await QuarterlyReportOrchestrator.GetPendingAdjustmentReportFromSessionAsync();
            var pending = response?.WageAdjustmentByQuarter;
            if (pending != null)
            {
                var reportingQuarter = pending?.ReportQuarter ?? 0;
                var reportingYear = pending?.ReportYear ?? 0;
                _reportModel.SelectedQuarterKey = $"Q{reportingQuarter} {reportingYear}";
                await LoadEmployeesForSelectedQuarter();

                _reportModel.WageAdjustmentSK = pending?.WageAdjustmentSK;
                var reportProxy = await WageAdjustmentService.LoadWageReportAsync(reportingQuarter, reportingYear);
                if (reportProxy is null)
                {
                    _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SelectedQuarterKey)), "No wage report was found to adjust");
                    _showValidationSummary = true;
                    _isLoading = false;
                    return;
                }
                _reportModel.WageReportSK = (int) (reportProxy.WageReportSK ?? 0);

                var employees = pending?.WageAdjustmentDetails?.Select(d =>
                {
                    return new WageAdjustmentEmployeeData
                    {
                        OriginalLastName = d.OriginalLastName ?? string.Empty,
                        OriginalFirstName = d.OriginalFirstName ?? string.Empty,
                        OriginalSSN = d.OriginalSSN ?? string.Empty,
                        OriginalQuarterlyWages = d.OriginalWageAmount ?? 0m,
                        CorrectedLastName = string.IsNullOrEmpty(d.EmployeeLastName) ? null : d.EmployeeLastName,
                        CorrectedFirstName = string.IsNullOrEmpty(d.EmployeeFirstName) ? null : d.EmployeeFirstName,
                        CorrectedSSN = string.IsNullOrEmpty(d.EmployeeSSN) ? null : d.EmployeeSSN,
                        AdjustedQuarterlyWages = d.WageAmount,
                        WageAdjustmentDetailSKField = d.WageAdjustmentDetailSK,
                        WageAdjustmentSKField = d.WageAdjustmentSK,
                        AdjustmentReasonCodeSK = d.AdjustmentReasons?.FirstOrDefault()?.CodeSK,
                        Order = d.WageReportDetailOrder,
                    };
                }).ToList() ?? [];
                _reportModel.Employees.AdjustmentEmployees = employees;
                _currentStep = 2;

            }
            _isLoading = false;
            StateHasChanged();
        }
    }

    private async Task LoadAdjustmentReasons()
    {
        _reportModel.Employees.AdjustmentReasons = await WageAdjustmentService.GetAdjustmentReasonsAsync();
        await Task.CompletedTask;
    }

    /// <summary>
    /// Builds the Quarter &amp; Year options from the employer's filed wage reports. Only
    /// quarters with a submitted wage report can be adjusted, so the dropdown must offer
    /// those rather than a rolling calendar range.
    /// </summary>
    private async Task LoadFiledWageQuarters()
    {
        var filedReports = await TaxAndWageEntryService.GetPreviouslyFiledReportsAsync();
        var minYear = DateTime.Now.Year - 4;
        _filedWageQuarters = filedReports
            .Where(report =>
            {
                return report.HasWageReport && report.Quarter > 0 && report.Year >= minYear;
            })
            .OrderByDescending(report =>
            {
                return report.Year;
            })
            .ThenByDescending(report =>
            {
                return report.Quarter;
            })
            .Select(report =>
            {
                return $"Q{report.Quarter} {report.Year}";
            })
            .Distinct()
            .ToList();

        if (_filedWageQuarters.Count == 0)
        {
            _validationMessageStore.Add(
                new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SelectedQuarterKey)),
                "There are no previously filed wage reports available to adjust.");
            _showValidationSummary = true;
            _editContext.NotifyValidationStateChanged();
        }
    }

    private async Task HandleActionClick()
    {
        if (_currentStep == 1)
        {
            if (_filedWageQuarters.Count == 0)
            {
                _editContext = new EditContext(_reportModel);
                _validationMessageStore = new ValidationMessageStore(_editContext);

                _validationMessageStore.Add(
                    new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SelectedQuarterKey)),
                    "There are no previously filed wage reports available to adjust.");
                _showValidationSummary = true;
                _editContext.NotifyValidationStateChanged();
                return;
            }

            _validationMessageStore.Clear();
            _showValidationSummary = false;
            if (!_editContext.Validate())
            {
                _showValidationSummary = true;
                return;
            }
            var selectedKey = _reportModel.SelectedQuarterKey;
            if (string.IsNullOrEmpty(selectedKey))
            {
                return;
            }
            _isLoading = true;
            StateHasChanged();
            var parts = selectedKey.Split(' ');
            var quarter = int.Parse(parts[0].TrimStart('Q'));
            var year = int.Parse(parts[1]);
            var reportProxy = await WageAdjustmentService.LoadWageReportAsync(quarter, year);
            if (reportProxy is null)
            {
                _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SelectedQuarterKey)), "No wage report was found to adjust");
                _showValidationSummary = true;
                _isLoading = false;
                return;
            }
            _reportModel.WageReportSK = (int) (reportProxy.WageReportSK ?? 0);
            await LoadEmployeesForSelectedQuarter();
            _isLoading = false;
            _currentStep = 2;
        }
        else if (_currentStep == 2)
        {
            if (!await _employeeWageAdjustments.IsValid())
            {
                return;
            }
            _reportModel.Employees.AdjustmentEmployees = _reportModel.Employees.AdjustmentEmployees
            .Where(HasEdits).ToList();
            _currentStep = 3;
        }
        else if (_currentStep == 3)
        {
            await HandleValidSubmit();
        }
    }

    private void HandleCancelClick()
    {
        if (_currentStep == 1)
        {
            NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
            return;
        }
        _showCancelModal = true;
    }

    private void HandleCancelModalStay()
    {
        _showCancelModal = false;
    }

    private async Task HandleCancelModalConfirm()
    {
        _showCancelModal = false;
        _isLoading = true;
        StateHasChanged();
        if (_reportModel.WageAdjustmentSK != null)
        {
            _isLoading = true;
            var result = await WageAdjustmentService.RemovePendingWageAdjustmentByQuarterAsync(_reportModel);
            _isLoading = false;
            StateHasChanged();
            var isSuccess = ValidateBooleanResponse(result);
            if (!isSuccess)
            {
                _isLoading = false;
                StateHasChanged();
                return;
            }
            await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
            _reportModel.WageAdjustmentSK = null;
            _reportModel.Employees = new WageAdjustmentEmployeesModel();
        }
        _isLoading = false;
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
    }

    private async Task HandleSaveAndQuitClick()
    {
        if (!await _employeeWageAdjustments.IsValid())
        {
            return;
        }
        _showSaveAndQuitModal = true;
    }
    private void HandleSaveAndQuitModalClose()
    {
        _showSaveAndQuitModal = false;
    }

    private async Task HandleSaveAndQuitModalConfirm()
    {
        _showSaveAndQuitModal = false;
        _isSubmitting = true;
        var result = await WageAdjustmentService.SavePendingWageAdjustmentByQuarterAsync(_reportModel);
        _isSubmitting = false;
        StateHasChanged();
        var isSuccess = ValidateBooleanResponse(result);
        if (!isSuccess)
        {
            return;
        }
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
    }

    private bool ValidateBooleanResponse(Generated.ServiceClients.TaxWageAdjustmentService.BoolResponse result)
    {
        if (result.RuleViolations.Length != 0)
        {
            _validationMessageStore.Clear();
            foreach (var error in result.RuleViolations)
            {
                _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SubmitError)), error.RuleID + " " + error.RuleViolation);
            }
            _showValidationSummary = true;
            StateHasChanged();
            return false;
        }
        if (!result.Value)
        {
            _validationMessageStore.Clear();
            _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SubmitError)), Configuration["Messages:TechnicalDifficulties"]
                        ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            StateHasChanged();
            return false;
        }

        return true;
    }

    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        StateHasChanged();
        var result = await WageAdjustmentService.SubmitWageAdjustmentAsync(_reportModel);
        _isSubmitting = false;

        if (!string.IsNullOrEmpty(result.ConfirmationNumber))
        {
            await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
            _reportModel.ConfirmationNumber = result.ConfirmationNumber;
            _pageState = PageState.Confirmation;
            StateHasChanged();
            return;
        }

        if (result.RuleViolations.Length != 0)
        {
            _validationMessageStore.Clear();

            foreach (var error in result.RuleViolations)
            {
                _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SubmitError)), error.RuleID + " " + error.RuleViolation);
            }

            _showValidationSummary = true;
            StateHasChanged();
            return;
        }
        if (string.IsNullOrEmpty(result.ConfirmationNumber))
        {
            _validationMessageStore.Clear();
            _validationMessageStore.Add(new FieldIdentifier(_reportModel, nameof(WageAdjustmentModel.SubmitError)), Configuration["Messages:TechnicalDifficulties"]
                        ?? "We are currently experiencing technical difficulties. Please try again later.");
            _showValidationSummary = true;
            StateHasChanged();
            return;
        }
    }

    private static bool HasEdits(WageAdjustmentEmployeeData employee)
    {
        return !string.IsNullOrWhiteSpace(employee.CorrectedLastName)
        || !string.IsNullOrWhiteSpace(employee.CorrectedFirstName)
        || !string.IsNullOrWhiteSpace(employee.CorrectedSSN)
        || employee.AdjustedQuarterlyWages.HasValue;
    }

    // ---- Add Employees Modal ----
    private async Task LoadEmployeesForSelectedQuarter()
    {
        var selectedKey = _reportModel.SelectedQuarterKey;
        if (string.IsNullOrEmpty(selectedKey))
        {
            return;
        }
        var parts = selectedKey.Split(' ');
        var quarter = int.Parse(parts[0].TrimStart('Q'));
        var year = int.Parse(parts[1]);
        _reportModel.Employees.PreviouslyReportedEmployees = await WageAdjustmentService.GetPreviouslyReportedEmployeesAsync(quarter, year);
    }
    private void HandleEditQuarter(string newValue)
    {
        _reportModel.SelectedQuarterKey = newValue;
        _showValidationSummary = false;
        _validationMessageStore.Clear();
        StateHasChanged();
    }
    private void HandleBackClick()
    {
        _showValidationSummary = false;
        _validationMessageStore.Clear();
        if (_currentStep == 2 && _reportModel.Employees.AdjustmentEmployees.Count > 0)
        {
            _showBackModel = true;
            return;
        }
        _currentStep -= 1;
    }
    private void CloseBackModel()
    {
        _showBackModel = false;
    }
    private async Task ContinueBack()
    {
        _showBackModel = false;
        _isLoading = true;
        StateHasChanged();
        if (_reportModel.WageAdjustmentSK != null)
        {
            var result = await WageAdjustmentService.RemovePendingWageAdjustmentByQuarterAsync(_reportModel);
            if (!ValidateBooleanResponse(result))
            {
                _isLoading = false;
                return;
            }
        }
        await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
        _reportModel.Employees = new WageAdjustmentEmployeesModel();
        _reportModel.WageAdjustmentSK = null;
        _currentStep -= 1;
        _isLoading = false;
        StateHasChanged();
    }
    // ---- Verification Edit Handlers ----
    private void HandleEditEmployees()
    {
        _showValidationSummary = false;
        _validationMessageStore.Clear();
        _currentStep = 2;
    }

    private enum PageState
    {
        Wizard,
        Confirmation
    }
}
