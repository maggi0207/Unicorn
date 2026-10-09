using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Pages;
/// <summary>
/// Code-behind for Add New Employee To Wage Report page.
/// </summary>
public partial class AddNewEmployeeToWageReport
{
    #region Injected Services
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;
    [Inject] private IWageAdjustmentService WageAdjustmentService { get; set; } = default!;
    [Inject] private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;
    [Inject] private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    #endregion
    #region Fields
    private int _savedWageAdjustmentSK = 0;
    private int _wageReportSK;
    private bool _step2Attempted = false;
    private int _currentStep = 1;
    private readonly EditContext _editContext = new(new object());
    private readonly ValidationMessageStore _validationMessageStore = default!;
    private readonly Dictionary<string, string> _fieldIds = [];
    private readonly Dictionary<string, FieldIdentifier> _rowFields = [];
    private readonly Dictionary<string, string> _step1FieldIds = [];
    private bool _showErrors = false;
    private bool _showStep1Errors = false;
    private bool _isSubmitting = false;
    private string? _confirmationNumber;
    private PageState _pageState = PageState.Wizard;
    private bool _showBackModal = false;
    private bool _showSaveAndQuitModal = false;
    private bool _showCancelModal = false;
    private List<string> _filedWageQuarters = new();
    #endregion
    #region Enums
    /// <summary>Page state enum</summary>
    public enum PageState
    {
        /// <summary>Wizard</summary>
        Wizard,
        /// <summary>Confirmation</summary>
        Confirmation
    }
    #endregion
    #region Wizard Configuration
    private readonly List<WizardStep> _wizardSteps = new()
    {
        new WizardStep { StepNumber = 1, Title = "Select Quarter & Year", ActionButtonText = "Continue"},
        new WizardStep { StepNumber = 2, Title = "Add Employees", ActionButtonText = "Continue" },
        new WizardStep { StepNumber = 3, Title = "Verify Information", ActionButtonText = "SUBMIT ADJUSTMENT" }
    };
    private readonly List<string> _availableReasons = new()
    {
        "Adding New Employee(s)",
        "Adding Wages for Service Localized in Wisconsin",
        "Adding Wages Originally Reported to Wrong Account",
        "Adding Wages Originally Reported to Wrong Quarter"
    };
    private readonly AddNewEmployeeModel _model = new()
    {
        EmployeeEntryModels = new List<EmployeeEntryModel> { new() }
    };
    #endregion
    #region Lifecycle

    /// <summary>
    /// constructor
    /// </summary>
    public AddNewEmployeeToWageReport()
    {
        _model = new AddNewEmployeeModel
        {
            EmployeeEntryModels = new List<EmployeeEntryModel>()
        };
        _editContext = new EditContext(_model);
        _validationMessageStore = new ValidationMessageStore(_editContext);
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
            .Where(report => report.HasWageReport && report.Quarter > 0 && report.Year >= minYear)
            .OrderByDescending(report => report.Year)
            .ThenByDescending(report => report.Quarter)
            .Select(report => $"Q{report.Quarter} {report.Year}")
            .Distinct()
            .ToList();

        if (_filedWageQuarters.Count == 0)
        {
            _validationMessageStore.Add(
                new FieldIdentifier(_model, nameof(_model.SelectedQuarter)),
                "There are no previously filed wage reports available to adjust.");
            _showStep1Errors = true;
            _editContext.NotifyValidationStateChanged();
        }
    }

    /// <summary>Initialize edit context</summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadFiledWageQuarters();
    }

    /// <summary>Load data on first render</summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            var response = await QuarterlyReportOrchestrator
                .GetPendingAdjustmentReportFromSessionAsync();
            var pending = response?.WageAdjustmentAppended;

            if (pending != null)
            {
                _savedWageAdjustmentSK = pending.WageAdjustmentSK ?? 0;
                _model.WageAdjustmentSK = pending.WageAdjustmentSK;
                _model.SelectedQuarter = $"Q{pending.ReportQuarter} {pending.ReportYear}";

                var reasonCode = pending.WageAdjustmentDetails?.FirstOrDefault()?.AdjustmentReasons?.FirstOrDefault()?.CodeSK;
                _model.SelectedReason = GetReasonText(reasonCode);

                await LoadWageReportSK();

                if (pending.WageAdjustmentDetails != null)
                {
                    _model.EmployeeEntryModels = pending.WageAdjustmentDetails
                        .Select(d =>
                        {
                            return new EmployeeEntryModel
                            {
                                FirstName = d.EmployeeFirstName,
                                LastName = d.EmployeeLastName,
                                SSN = SsnHelper.FormatSSN(d.EmployeeSSN),
                                QuarterlyWages = d.WageAmount
                            };
                        })
                        .ToList();
                }

                _currentStep = 2;
                await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
            }

            _validationMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            StateHasChanged();
        }
    }
    #endregion
    #region Validation
    private void EvaluateStep1Banner()
    {
        _step1FieldIds.Clear();
        _validationMessageStore.Clear();
        var valid = _editContext.Validate();
        _step1FieldIds[nameof(_model.SelectedQuarter)] = "quarter-year-select";
        _showStep1Errors = !valid;
    }
    private void EvaluateStep2Banner()
    {
        if (!_step2Attempted)
        {
            _showErrors = false;
            return;
        }
        _validationMessageStore.Clear();

        var employees = _model.EmployeeEntryModels ?? new List<EmployeeEntryModel>();

        if (string.IsNullOrWhiteSpace(_model.SelectedReason))
        {
            AddError(nameof(_model.SelectedReason),
                "Please select a reason before continuing.");
        }

        var hasAnyData = employees.Any(emp =>
        {
            return !string.IsNullOrWhiteSpace(emp.LastName) ||
                        !string.IsNullOrWhiteSpace(emp.FirstName) ||
                        !string.IsNullOrWhiteSpace(emp.SSN) ||
                        emp.QuarterlyWages > 0;
        });

        if (!hasAnyData)
        {
            AddError(_editContext.Field(string.Empty),
                "Please add at least one employee before continuing.");
        }

        for (var i = 0; i < employees.Count; i++)
        {
            var emp = employees[i];
            if (emp.AddedAfterValidation)
            {
                continue;
            }

            var context = new ValidationContext(emp);
            var results = new List<ValidationResult>();
            Validator.TryValidateObject(emp, context, results, validateAllProperties: true);
            foreach (var result in results)
            {
                var memberName = result.MemberNames.FirstOrDefault() ?? string.Empty;
                var message = result.ErrorMessage ?? string.Empty;
                // Against the ROW, which is the identifier the inputs and DataAnnotationsValidator
                // already use. Writing to a synthetic key on the root model instead was what made
                // framework-produced messages unattributable by the summary.
                _validationMessageStore.Add(new FieldIdentifier(emp, memberName), message);
            }
        }

        var hasDuplicateSSN = TryFindDuplicateSSN(employees, out var dupSsnIndex);
        if (hasDuplicateSSN)
        {
            AddError("DuplicateSSN",
                "Duplicate SSN found. Each employee must have a unique Social Security Number.");
        }

        var hasDuplicateName = TryFindDuplicateName(employees, out var dupNameIndex);
        if (hasDuplicateName)
        {
            AddError("DuplicateName",
                "Duplicate employee name found. Please verify each employee entry is unique.");
        }

        UpdateFieldIds(employees.ToList(), dupSsnIndex, dupNameIndex);
        _editContext.NotifyValidationStateChanged();
        _showErrors = _editContext.GetValidationMessages().Any();
    }
    private void AddError(string fieldName, string message)
    {
        AddError(new FieldIdentifier(_model, fieldName), message);
    }
    private void AddError(FieldIdentifier field, string message)
    {
        _validationMessageStore.Add(field, message);
        _editContext.NotifyValidationStateChanged();
        _showErrors = true;
    }
    private void UpdateFieldIds(List<EmployeeEntryModel> employees, int dupSsnIndex = -1, int dupNameIndex = -1)
    {
        _fieldIds.Clear();
        _rowFields.Clear();

        for (var i = 0; i < employees.Count; i++)
        {
            _fieldIds[$"LastName_{i}"] = $"last-name-{i}";
            _fieldIds[$"FirstName_{i}"] = $"first-name-{i}";
            _fieldIds[$"SSN_{i}"] = $"ssn-{i}";
            _fieldIds[$"QuarterlyWages_{i}"] = $"wages-{i}";

            // Every per-row message - the loop's own, DataAnnotationsValidator's, and
            // InputNumber's ParsingErrorMessage - is filed against the row. ResolveField maps the
            // summary's key onto it so all three are linkable.
            var employee = employees[i];
            _rowFields[$"LastName_{i}"] = new FieldIdentifier(employee, nameof(EmployeeEntryModel.LastName));
            _rowFields[$"FirstName_{i}"] = new FieldIdentifier(employee, nameof(EmployeeEntryModel.FirstName));
            _rowFields[$"SSN_{i}"] = new FieldIdentifier(employee, nameof(EmployeeEntryModel.SSN));
            _rowFields[$"QuarterlyWages_{i}"] = new FieldIdentifier(employee, nameof(EmployeeEntryModel.QuarterlyWages));
        }

        _fieldIds["SelectedReason"] = "reason-select";
        _fieldIds[nameof(_model.EmployeeEntryModels)] = employees.Any() ? "last-name-0" : string.Empty;

        if (dupSsnIndex >= 0)
        {
            _fieldIds["DuplicateSSN"] = $"ssn-{dupSsnIndex}";
        }

        if (dupNameIndex >= 0)
        {
            _fieldIds["DuplicateName"] = $"last-name-{dupNameIndex}";
        }
    }

    /// <summary>
    /// Maps a <c>_fieldIds</c> key onto the field that actually owns its messages: the employee row
    /// for per-row keys, the root model for everything else (the reason dropdown, list-level errors).
    /// </summary>
    private FieldIdentifier ResolveField(string key) =>
        _rowFields.TryGetValue(key, out var rowField) ? rowField : _editContext.Field(key);

    private void HandleReasonChanged(string? value)
    {
        _model.SelectedReason = value;
        if (_step2Attempted)
        {
            EvaluateStep2Banner();
            StateHasChanged();
        }
    }
    private void HandleEmployeeChanged()
    {
        if (_step2Attempted)
        {
            EvaluateStep2Banner();
            StateHasChanged();
        }
    }
    private static bool TryFindDuplicateSSN(List<EmployeeEntryModel> employees, out int index)
    {
        var seen = new Dictionary<string, int>();
        for (var i = 0; i < employees.Count; i++)
        {
            var digits = new string((employees[i].SSN ?? "").Where(char.IsDigit).ToArray());

            // All-zero SSN is the deliberate "unknown SSN" convention — the backend
            // generates a unique M00 number for each one, so it must never be
            // flagged as a duplicate, no matter how many employees use it.
            if (string.IsNullOrEmpty(digits) || digits == "000000000")
            {
                continue;
            }

            if (seen.TryGetValue(digits, out var firstIndex))
            {
                index = firstIndex;
                return true;
            }

            seen[digits] = i;
        }

        index = -1;
        return false;
    }
    private static bool TryFindDuplicateName(List<EmployeeEntryModel> employees, out int index)
    {
        var seen = new Dictionary<(string?, string?, string), int>();
        for (var i = 0; i < employees.Count; i++)
        {
            var key = (
                First: employees[i].FirstName?.Trim().ToLower(),
                Last: employees[i].LastName?.Trim().ToLower(),
                SSN: new string((employees[i].SSN ?? "").Where(char.IsDigit).ToArray())
            );

            if (seen.TryGetValue(key, out var firstIndex))
            {
                index = firstIndex;
                return true;
            }

            seen[key] = i;
        }

        index = -1;
        return false;
    }
    #endregion
    #region Event Handlers
    private async Task HandleActionClick()
    {
        if (_currentStep == 1)
        {
            if (_filedWageQuarters.Count == 0)
            {
                _validationMessageStore.Clear();
                _validationMessageStore.Add(
                    new FieldIdentifier(_model, nameof(_model.SelectedQuarter)),
                    "There are no previously filed wage reports available to adjust.");
                _showStep1Errors = true;
                _editContext.NotifyValidationStateChanged();
                StateHasChanged();
                return;
            }

            EvaluateStep1Banner();

            if (_showStep1Errors)
            {
                StateHasChanged();
                return;
            }

            _isSubmitting = true;
            StateHasChanged();
            var loaded = await LoadWageReportSK();
            _isSubmitting = false;
            if (!loaded)
            {
                StateHasChanged();
                return;
            }
            _validationMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            _currentStep = 2;
            _step2Attempted = false;
            _showErrors = false;
            _showStep1Errors = false;
            _fieldIds.Clear();
            _fieldIds["EmployeeEntryModels"] = "last-name-0";
            _fieldIds["SelectedReason"] = "reason-select";
            StateHasChanged();
            return;
        }
        if (_currentStep == 2)
        {
            _step2Attempted = true;
            foreach (var emp in _model.EmployeeEntryModels)
            {
                emp.AddedAfterValidation = false;
            }

            _validationMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();

            EvaluateStep2Banner();

            if (_showErrors)
            {
                StateHasChanged();
                return;
            }

            _showErrors = false;
            _validationMessageStore.Clear();
            _currentStep = 3;
            StateHasChanged();
            return;
        }

        if (_currentStep == 3)
        {
            await HandleValidSubmit();
        }
    }
    private Task HandleBackClick()
    {
        if (_currentStep == 1)
        {
            NavigationManager.NavigateTo("quarterly-tax/missing-reports");
            return Task.CompletedTask;
        }
        if (_currentStep == 2)
        {
            var hasAnyData = !string.IsNullOrWhiteSpace(_model.SelectedReason) ||
                     _model.EmployeeEntryModels.Any(emp =>
                     {
                         return !string.IsNullOrWhiteSpace(emp.LastName) ||
                 !string.IsNullOrWhiteSpace(emp.FirstName) ||
                 !string.IsNullOrWhiteSpace(emp.SSN) ||
                 emp.QuarterlyWages > 0;
                     });

            if (hasAnyData || _savedWageAdjustmentSK > 0)
            {
                _showBackModal = true;
            }
            else
            {
                _currentStep = 1;
            }
            StateHasChanged();
            return Task.CompletedTask;
        }

        if (_currentStep == 3)
        {
            _step2Attempted = false;
            _showErrors = false;
            _validationMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            foreach (var emp in _model.EmployeeEntryModels)
            {
                emp.LastNameTouched = false;
                emp.FirstNameTouched = false;
                emp.SSNTouched = false;
                emp.WagesTouched = false;
            }
        }

        _currentStep = _currentStep == 3 ? 2 : 1;
        StateHasChanged();
        return Task.CompletedTask;
    }
    private Task HandleCancelClick()
    {
        if (_currentStep == 1)
        {
            NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
            return Task.CompletedTask;
        }

        _showCancelModal = true;
        return Task.CompletedTask;
    }
    private void HandleCancelModalStay()
    {
        _showCancelModal = false;
    }
    private async Task HandleCancelModalConfirm()
    {
        _showCancelModal = false;
        _isSubmitting = true;
        StateHasChanged();

        if (_savedWageAdjustmentSK > 0)
        {
            try
            {
                await WageAdjustmentService
                    .DeletePendingWageReportAdjustmentFormAdditionalEmployeesAsync(
                        _savedWageAdjustmentSK);
            }
            catch (Exception)
            {
                // Silent fail — navigate away
            }
        }
        _isSubmitting = false;
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
    }
    private Task HandleSaveAndQuitClick()
    {
        _step2Attempted = true;
        foreach (var emp in _model.EmployeeEntryModels)
        {
            emp.AddedAfterValidation = false;
        }
        _validationMessageStore.Clear();
        _editContext.NotifyValidationStateChanged();
        EvaluateStep2Banner();
        if (_showErrors)
        {
            StateHasChanged();
            return Task.CompletedTask;
        }
        _showErrors = false;
        _validationMessageStore.Clear();
        _showSaveAndQuitModal = true;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleSaveAndQuitClose()
    {
        _showSaveAndQuitModal = false;
        StateHasChanged();
        return Task.CompletedTask;
    }
    private async Task HandleSaveAndQuitConfirm(EmployeeWageEntryData _)
    {
        _showSaveAndQuitModal = false;
        _isSubmitting = true;
        StateHasChanged();

        if (_currentStep < 2 || string.IsNullOrWhiteSpace(_model.SelectedQuarter))
        {
            _isSubmitting = false;
            NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
            return;
        }

        var parts = _model.SelectedQuarter!.Split(' ');
        var quarterNumber = int.Parse(parts[0].Replace("Q", ""));
        var year = int.Parse(parts[1]);

        // Only include employees that have at least some valid data
        var employeesToSave = (_model.EmployeeEntryModels ?? new List<EmployeeEntryModel>())
            .Where(emp =>
            {
                return !string.IsNullOrWhiteSpace(emp.FirstName) ||
                                !string.IsNullOrWhiteSpace(emp.LastName) ||
                                !string.IsNullOrWhiteSpace(emp.SSN) ||
                                emp.QuarterlyWages > 0;
            })
            .Select(emp =>
            {
                return new WageAdjustmentRequestNewEmployeeDetail
                {
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    SSN = emp.SSN?.Replace("-", ""),
                    GrossWages = emp.QuarterlyWages ?? 0
                };
            })
            .ToArray();

        var request = new WageAdjustmentAdditionEmployeesRequest
        {
            Quarter = quarterNumber,
            Year = year,
            WageAdjustmentReasonCodeSK = GetReasonCode(_model.SelectedReason) ?? 0,
            NewEmployeeDetails = employeesToSave,
            WageReportSK = _wageReportSK,
            WageAdjustmentSK = _model.WageAdjustmentSK,
            EmailAddress = "test@test.com"
        };

        try
        {
            var response = await WageAdjustmentService
                .SavePendingWageAdjustmentAdditionalEmployeesAsync(request);

            if (response?.RuleViolations != null && response.RuleViolations.Any())
            {
                var rawMessage = response.RuleViolations.First().RuleViolation ?? string.Empty;
                ShowStep2Error(GetErrorMessage(rawMessage));
                _isSubmitting = false;
                //_currentStep = 2;
                StateHasChanged();
                return;
            }
        }
        catch (Exception)
        {
            // Swallow error — navigate away regardless
        }

        _isSubmitting = false;
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments", forceLoad: true);
    }
    private void HandleQuarterChanged(string? value)
    {
        _model.SelectedQuarter = value;

        if (!string.IsNullOrWhiteSpace(value))
        {
            // Clear errors as soon as a valid value is selected
            _validationMessageStore.Clear();
            _editContext.NotifyValidationStateChanged();
            _showStep1Errors = false;
            _step1FieldIds.Clear();
            StateHasChanged();
        }
    }
    private Task HandleKeepEmployeeData(bool _)
    {
        _showBackModal = false;
        _currentStep = 2;
        StateHasChanged();
        return Task.CompletedTask;
    }
    private async Task HandleRemoveEmployeeData()
    {
        _showBackModal = false;
        _isSubmitting = true;
        StateHasChanged();
        if (_savedWageAdjustmentSK > 0)
        {
            try
            {
                await WageAdjustmentService
                    .DeletePendingWageReportAdjustmentFormAdditionalEmployeesAsync(
                        _savedWageAdjustmentSK);
            }
            catch (Exception)
            {
                // Silent fail — continue clearing local state
            }

            _savedWageAdjustmentSK = 0;
        }

        _model.EmployeeEntryModels = new List<EmployeeEntryModel>();
        _model.SelectedReason = null;
        _step2Attempted = false;
        _showErrors = false;
        _validationMessageStore.Clear();
        _editContext.NotifyValidationStateChanged();
        _isSubmitting = false;
        _currentStep = 1;
        StateHasChanged();
    }

    #endregion

    #region Submission

    private async Task HandleValidSubmit()
    {
        _isSubmitting = true;
        StateHasChanged();

        var parts = _model.SelectedQuarter!.Split(' ');
        var quarterNumber = int.Parse(parts[0].Replace("Q", ""));
        var year = int.Parse(parts[1]);

        var request = new WageAdjustmentAdditionEmployeesRequest
        {
            Quarter = quarterNumber,
            Year = year,
            WageAdjustmentReasonCodeSK = GetReasonCode(_model.SelectedReason) ?? 0,
            NewEmployeeDetails = _model.EmployeeEntryModels.Select(emp =>
            {
                return new WageAdjustmentRequestNewEmployeeDetail
                {
                    FirstName = emp.FirstName,
                    LastName = emp.LastName,
                    SSN = emp.SSN?.Replace("-", "").ToString(),
                    GrossWages = emp.QuarterlyWages ?? 0
                };
            }).ToArray(),
            WageReportSK = _wageReportSK,
            WageAdjustmentSK = _model.WageAdjustmentSK,
            EmailAddress = "test@test.com"
        };

        SaveWageAdjustmentResponse? response;
        try
        {
            response = await WageAdjustmentService
     .SubmitAdditionalEmployeesAsync(request);
        }
        catch (Exception)
        {
            _isSubmitting = false;
            ShowStep2Error("Submission failed. Please try again or contact support.");
            return;
        }
        finally
        {
            _isSubmitting = false;
            StateHasChanged();
        }

        if (response == null)
        {
            ShowStep2Error("Submission failed. No response from server. Please try again.");
            return;
        }

        if (!string.IsNullOrWhiteSpace(response.ConfirmationNumber))
        {
            _confirmationNumber = response.ConfirmationNumber;
            _pageState = PageState.Confirmation;
            StateHasChanged();
            return;
        }

        if (response.RuleViolations != null && response.RuleViolations.Any())
        {
            var rawMessage = response.RuleViolations.First().RuleViolation ?? string.Empty;
            ShowStep2Error(GetErrorMessage(rawMessage));
            _currentStep = 2;
            StateHasChanged();
            return;
        }

        var msg = Configuration["Messages:TechnicalDifficulties"]
                     ?? "We are currently experiencing technical difficulties. Please try again later.";
        ShowStep2Error(msg);
        _currentStep = 2;
        StateHasChanged();
        return;
    }
    private void ShowStep2Error(string message)
    {
        _validationMessageStore.Clear();
        _validationMessageStore.Add(_editContext.Field(string.Empty), message);
        _editContext.NotifyValidationStateChanged();
        _showErrors = true;
        StateHasChanged();
    }

    #endregion

    #region Helpers

    private void JumpToStep(int step)
    {
        _currentStep = step;
        StateHasChanged();
    }

    private async Task<bool> LoadWageReportSK()
    {
        var parts = _model.SelectedQuarter!.Split(' ');
        var quarterNumber = int.Parse(parts[0].Replace("Q", ""));
        var year = int.Parse(parts[1]);

        var wageReport = await TaxAndWageEntryService
            .GetWageReportByQuarterYear(year, quarterNumber);

        if (wageReport == null)
        {
            //_step1BannerMessage = "No wage report was found to adjust.";
            _validationMessageStore.Clear();
            _validationMessageStore.Add(new FieldIdentifier(_model, "WageReportNotFound"),
                "No wage report was found to adjust.");
            _editContext.NotifyValidationStateChanged();
            _showStep1Errors = true;
            StateHasChanged();
            return false;
        }

        // _step1BannerMessage = string.Empty;
        _wageReportSK = (int) (wageReport.WageReportSK ?? 0);
        return true;
    }

    private static string GetErrorMessage(string rawMessage)
    {
        return rawMessage.Contains("Key in dictionary") || rawMessage.Contains("already been added")
            ? "One or more employees have already been added to this wage report."
            : rawMessage.Contains("SSN has been invalidated") || rawMessage.Contains("rule violations that prevent it from saving")
            ? "One or more SSNs entered are invalid. Please correct them before continue."
            : rawMessage;
    }
    private static int? GetReasonCode(string? reason)
    {
        return reason switch
        {
            "Adding New Employee(s)" => 1,
            "Adding Wages for Service Localized in Wisconsin" => 2,
            "Adding Wages Originally Reported to Wrong Account" => 3,
            "Adding Wages Originally Reported to Wrong Quarter" => 4,
            _ => null
        };
    }
    private static string? GetReasonText(int? reasonCode)
    {
        return reasonCode switch
        {
            1 => "Adding New Employee(s)",
            2 => "Adding Wages for Service Localized in Wisconsin",
            3 => "Adding Wages Originally Reported to Wrong Account",
            4 => "Adding Wages Originally Reported to Wrong Quarter",
            _ => null
        };
    }
    #endregion
}
