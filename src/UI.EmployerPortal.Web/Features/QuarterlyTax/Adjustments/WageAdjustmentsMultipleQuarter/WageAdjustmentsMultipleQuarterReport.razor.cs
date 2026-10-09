using System.ComponentModel.DataAnnotations;
using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.WageAdjustmentsMultipleQuarter.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.WageAdjustmentsMultipleQuarter;

/// <summary>
/// Code-behind for the WageAdjustmentsMultipleQuarterReport page.
/// </summary>

public partial class WageAdjustmentsMultipleQuarterReport
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;
    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IWageAdjustmentReasonsService WageAdjustmentReasonService { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private IConfiguration Configuration { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    /// <summary>
    /// EmployeeIdentifierModelData
    /// </summary>
    [Parameter]
    public EmployeeIdentifierModel EmployeeIdentifierModelData { get; set; } = new();

    /// <summary>
    /// WageAdjustmentData
    /// </summary>
    [Parameter]
    public List<WageAdjustmentModelOnly> WageAdjustmentData { get; set; } = new();

    /// <summary>
    /// DropDownItems for Reasons
    /// </summary>
    [Parameter]
    public List<DropDownItem> DropDownItems { get; set; } = new();

    /// <summary>
    /// ExistingSSNs
    /// </summary>
    [Parameter]
    public List<string> ExistingSSNs { get; set; } = new();

    private readonly List<WizardStep> _wageAdjustmentMultipleQuarterWizard = new()
    {
        new() { StepNumber = 1, Title = "Select Report to Adjust", ActionButtonText = "Continue" },
        new() { StepNumber = 2, Title = "Enter Adjustments", ActionButtonText = "Continue" },
        new() { StepNumber = 3, Title = "Verify Information", ActionButtonText = "SUBMIT ADJUSTMENT" },
    };

    /// <summary>
    /// WageAdjustmentsMultipleQuarter
    /// </summary>
    private WageAdjustmentsMultipleQuarterModelOnly WageAdjustmentsMultipleQuarter { get; set; } = default!;

    private int _currentStep = 1;
    private string? _confirmationNumber;
    private bool _ssnFocused;
    private bool _showSSN;
    private bool _showErrors;
    private bool _showStep2Errors;
    private bool _showBackModal = false;
    private bool _showSaveAndQuitModal;
    private readonly Dictionary<string, string> _fieldIdsStep2 = new();
    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(SSN)] = "wage-adj-ssn",
    };

    /// <summary>
    /// Gets or sets the employee's social security number, or the M00 number assigned to an
    /// employee reported without one.
    /// </summary>
    [Required(ErrorMessage = "SSN is required.")]
    [RegularExpression(@"^(\d{3}|M\d{2})-\d{2}-\d{4}$", ErrorMessage = SsnFormatErrorMessage)]

    public string SSN { get; set; } = string.Empty;

    private const string SsnFormatErrorMessage =
        "SSN must be in the format ###-##-####, or M##-##-#### for an M00 number.";

    /// <summary>
    /// Shown when an adjusted wage matches the wage already reported for that quarter.
    /// </summary>
    internal const string UnchangedWageErrorMessage =
        "Adjusted quarterly wages must be different from the quarterly wages already reported.";

    /// <summary>
    /// Builds the id shared by a row's adjusted wage input and its validation summary
    /// entry, so the summary link focuses the right quarter.
    /// </summary>
    internal static string AdjustedWageFieldKey(WageAdjustmentModelOnly wage)
    {
        return $"adjusted-wage-{wage.WageReportSK}-{wage.WageReportDetailOrder}";
    }

    internal static string AdjustmentReasonFieldKey(WageAdjustmentModelOnly wage)
    {
        return $"adjustment-reason-{wage.WageReportSK}";
    }

    private PageState _pageState = PageState.Wizard;

    private EditContext _editContext = default!;
    private ValidationMessageStore? _step1MessageStore;

    private EditContext _step2EditContext = default!;
    private ValidationMessageStore? _step2MessageStore;

    private WageDetailResponse _wageDetailsResponse = default!;

    private long? _wageReportSK;
    private int _savedWageAdjustmentSK = 0;

    private bool _isLoading = false;
    private bool _showUnsavedChangesModal;

    private WageAdjustmentBySSNDetailRequest BuildWageAdjustmentRequest(int employerSK,
        WageAdjustmentRequestBySSN[] wageAdjustmentDetails)
    {
        var originalEmployee = WageAdjustmentsMultipleQuarter.EmployeeIdentifierViewModel;
        var editedEmployee = WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel;
        return new WageAdjustmentBySSNDetailRequest
        {
            WageAdjustmentSK = _savedWageAdjustmentSK > 0 ? _savedWageAdjustmentSK : null,
            OriginalFirstName = originalEmployee.FirstName,
            OriginalLastName = originalEmployee.LastName,
            OriginalSSN = NormalizedOrNull(originalEmployee.SSN),
            NewFirstName = string.IsNullOrWhiteSpace(editedEmployee.FirstName)
                               ? null
                           : editedEmployee.FirstName.Trim(),
            NewLastName = string.IsNullOrWhiteSpace(editedEmployee.LastName)
                             ? null
                              : editedEmployee.LastName.Trim(),
            NewSSN = NormalizedOrNull(editedEmployee.SSN),
            EmployerSK = employerSK,
            SecureUserSK = UserAccountService.GetUserSKClaim(),
            WageAdjustmentDetails = wageAdjustmentDetails
        };


    }

    /// <summary>
    /// Strips separators for the service call while keeping the leading "M" of an M00 number,
    /// so that employees without an SSN are not sent as an eight digit identifier.
    /// </summary>
    private static string? NormalizedOrNull(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : SsnHelper.Normalize(value);
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
        _isLoading = true;
        _showStep2Errors = false;
        _fieldIdsStep2.Clear();
        StateHasChanged();

        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        var wageAdjustmentDetails = BuildWageAdjustmentDetails(WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList);

        var wageAdjustmentRequest = BuildWageAdjustmentRequest(account?.Id ?? 0, wageAdjustmentDetails);

        var response = await WageAdjustmentReasonService.SavePendingWageReportAdjustmentByEmployeeAsync(wageAdjustmentRequest);

        WageAdjustmentResponseAndVoilationCheck(response, "Error occurred while saving.");
        if (_showStep2Errors)
        {
            _isLoading = false;
            _showSaveAndQuitModal = false;
            StateHasChanged();
            return;
        }

        _isLoading = false;
        _showSaveAndQuitModal = false;
        await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
        return;
    }

    private void WageAdjustmentResponseAndVoilationCheck(BoolResponse? response, string errorMessage)
    {
        var model = WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel;
        _step2MessageStore?.Clear();
        _fieldIdsStep2.Clear();

        if (response is null)
        {
            _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), errorMessage);
            _showStep2Errors = true;
            _step2EditContext.NotifyValidationStateChanged();
            return;
        }

        if (response.Value == false)
        {
            if (response.RuleViolations == null || response.RuleViolations.Length == 0)
            {
                _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), errorMessage);
            }
            else
            {
                foreach (var violation in response.RuleViolations)
                {
                    _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), violation.RuleViolation);
                }
            }
            _showStep2Errors = true;
            _step2EditContext.NotifyValidationStateChanged();
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

        if (_savedWageAdjustmentSK > 0)
        {
            _isLoading = true;
            var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
            var request = new WageAdjustmentDeletePendingRequest
            {
                EmployerSK = account?.Id ?? 0,
                SecureUserSK = UserAccountService.GetUserSKClaim(),
                WageAdjustmentSK = _savedWageAdjustmentSK
            };
            var response = await WageAdjustmentReasonService.DeletePendingWageReportAdjustmentByEmployeeAsync(request);

            _savedWageAdjustmentSK = 0;

            WageAdjustmentResponseAndVoilationCheck(response, "Error occurred while saving.");

            _isLoading = false;

            if (_showStep2Errors)
            {
                StateHasChanged();
                return;
            }
        }

        await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
        _savedWageAdjustmentSK = 0;
        _wageReportSK = null;
        _wageDetailsResponse = default!;
        _showErrors = false;
        _showStep2Errors = false;
        SSN = string.Empty;
        EmployeeIdentifierModelData = new EmployeeIdentifierModel();
        WageAdjustmentData = new List<WageAdjustmentModelOnly>();
        WageAdjustmentsMultipleQuarter = new WageAdjustmentsMultipleQuarterModelOnly
        {
            EmployeeIdentifierModel = new EmployeeIdentifierModel(),
            EmployeeIdentifierViewModel = new EmployeeIdentifierViewModel(),
            WageAdjustmentQuarterlyList = new List<WageAdjustmentModelOnly>()
        };
        _step2EditContext = new EditContext(WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel);
        _step2EditContext.OnFieldChanged += OnFieldChanged;
        _step2MessageStore = new ValidationMessageStore(_step2EditContext);
        _currentStep = 1;
        StateHasChanged();
    }

    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(SSN);
        _editContext.OnFieldChanged += OnFieldChanged;
        _step1MessageStore = new ValidationMessageStore(_editContext);
    }
    /// <summary>
    /// OnAuthorizedInitAsync
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isLoading = true;
        var reasons = await WageAdjustmentReasonService.GetWageAdjustmentReasons();
        foreach (var reason in reasons)
        {
            var item = new DropDownItem { Text = reason.ReasonText, Value = reason.CodeSk };
            DropDownItems.Add(item);
        }

        var response = await QuarterlyReportOrchestrator.GetPendingAdjustmentReportFromSessionAsync();
        var pending = response?.WageAdjustmentByEmployee;
        if (pending is not null)
        {
            _savedWageAdjustmentSK = pending.WageAdjustmentSK ?? 0;

            if (_savedWageAdjustmentSK <= 0)
            {
                _savedWageAdjustmentSK = pending.WageAdjustmentDetails?
                    .FirstOrDefault()?.WageAdjustmentSK ?? 0;
            }
            var freshEmployeeModel = new EmployeeIdentifierModel
            {
                FirstName = string.IsNullOrWhiteSpace(pending.EmployeeFirstName)
                            ? string.Empty
                            : pending.EmployeeFirstName,
                LastName = string.IsNullOrWhiteSpace(pending.EmployeeLastName)
                           ? string.Empty
                           : pending.EmployeeLastName,
                SSN = string.IsNullOrWhiteSpace(pending.EmployeeSSN)
                          ? string.Empty
                          : pending.EmployeeSSN
            };
            EmployeeIdentifierModelData = freshEmployeeModel;

            var employeeIdentifierViewModelData = new EmployeeIdentifierViewModel
            {
                FirstName = pending.OriginalFirstName,
                LastName = pending.OriginalLastName,
                SSN = pending.OriginalSSN
            };

            _wageDetailsResponse = await WageAdjustmentReasonService.GetWageDetailsBySSN(new WageDetailByUserRequest
            {
                SSN = SsnHelper.Normalize(pending.OriginalSSN),
                EmployerSK = pending.EmployerSK,
                SecureUserSK = UserAccountService.GetUserSKClaim()
            });

            WageAdjustmentData = [.. _wageDetailsResponse.WageReportDetailProxy
                .OrderByDescending(wage =>
                {
                    return wage.ReportYear;
                })
                .ThenByDescending(wage =>
                {
                    return wage.ReportQuarter;
                })
                .Select(wage =>
                {
                    return new WageAdjustmentModelOnly
                    {
                        Quarter = wage.ReportQuarter.ToString(),
                        Year = wage.ReportYear.ToString(),
                        QuarterlyWage = wage.GrossWages?.ToString("N2", CultureInfo.InvariantCulture),
                        WageReportSK = Convert.ToInt32(wage.WageReportSK),
                        WageReportDetailOrder = wage.Order,
                        AdjustmentReason = string.Empty
                    };
                })];

            foreach (var wage in WageAdjustmentData)
            {
                var saved = pending.WageAdjustmentDetails?.FirstOrDefault(x =>
                {
                    return x.ReportQuarter.ToString() == wage.Quarter &&
                                            x.ReportYear.ToString() == wage.Year &&
                                            x.WageReportSK == wage.WageReportSK &&
                                            x.WageReportDetailOrder == wage.WageReportDetailOrder;
                });
                if (saved != null)
                {
                    wage.AdjustedQuarterlyWage = saved.WageAmount;
                    wage.AdjustmentReason = saved.AdjustmentReasons?.FirstOrDefault()?.CodeSK.ToString()
                        ?? pending.WageAdjustmentTypeCodeSK.ToString();
                }
            }
            WageAdjustmentsMultipleQuarter = new WageAdjustmentsMultipleQuarterModelOnly
            {
                EmployeeIdentifierModel = freshEmployeeModel,
                EmployeeIdentifierViewModel = employeeIdentifierViewModelData,
                WageAdjustmentQuarterlyList = WageAdjustmentData
            };
            _step2EditContext = new EditContext(WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel);
            _step2EditContext.OnFieldChanged += OnFieldChanged;
            _step2MessageStore = new ValidationMessageStore(_step2EditContext);
            _currentStep = 2;
        }
        _isLoading = false;
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {

        _showErrors = false;
        _showStep2Errors = false;
        StateHasChanged();
        if (e.FieldIdentifier.FieldName.Equals(nameof(SSN)))
        {
            ValidateSSN(SsnHelper.Normalize(SSN));
        }
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _ssnFocused = false;
        _showSSN = false;
    }

    private Task HandleCancel()
    {
        if (_currentStep == 1)
        {
            NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments");
            return Task.CompletedTask;
        }
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
        _isLoading = true;
        StateHasChanged();

        if (_savedWageAdjustmentSK <= 0)
        {
            var pendingSession = await QuarterlyReportOrchestrator.GetPendingAdjustmentReportFromSessionAsync();
            _savedWageAdjustmentSK = pendingSession?.WageAdjustmentByEmployee?.WageAdjustmentSK ?? 0;

            if (_savedWageAdjustmentSK <= 0)
            {
                _savedWageAdjustmentSK = pendingSession?.WageAdjustmentByEmployee?
                    .WageAdjustmentDetails?.FirstOrDefault()?.WageAdjustmentSK ?? 0;
            }
        }

        if (_savedWageAdjustmentSK > 0)
        {
            var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
            var request = new WageAdjustmentDeletePendingRequest
            {
                EmployerSK = account?.Id ?? 0,
                SecureUserSK = UserAccountService.GetUserSKClaim(),
                WageAdjustmentSK = _savedWageAdjustmentSK
            };
            var response = await WageAdjustmentReasonService.DeletePendingWageReportAdjustmentByEmployeeAsync(request);
            if (response?.Value != true)
            {
                var model = WageAdjustmentsMultipleQuarter?.EmployeeIdentifierModel ?? new EmployeeIdentifierModel();
                _step2MessageStore?.Clear();
                _fieldIdsStep2.Clear();
                _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty),
                    "Unable to cancel the pending adjustment. Please try again.");
                _showStep2Errors = true;
                _step2EditContext.NotifyValidationStateChanged();
                _isLoading = false;
                StateHasChanged();
                return;
            }
        }
        await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
        _savedWageAdjustmentSK = 0;
        _currentStep = 1;
        _showStep2Errors = false;
        _showErrors = false;
        SSN = string.Empty;
        EmployeeIdentifierModelData = new EmployeeIdentifierModel();
        WageAdjustmentData = new List<WageAdjustmentModelOnly>();
        WageAdjustmentsMultipleQuarter = new WageAdjustmentsMultipleQuarterModelOnly
        {
            EmployeeIdentifierModel = new EmployeeIdentifierModel(),
            EmployeeIdentifierViewModel = new EmployeeIdentifierViewModel(),
            WageAdjustmentQuarterlyList = new List<WageAdjustmentModelOnly>()
        };
        _step2EditContext = new EditContext(WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel);
        _step2EditContext.OnFieldChanged += OnFieldChanged;
        _step2MessageStore = new ValidationMessageStore(_step2EditContext);
        _isLoading = false;
        NavigationManager.NavigateTo("tax-wage-report-adjustments/adjustments", true);
    }
    /// <summary>
    /// enables in step1 and when clicked navigate to missing reports
    /// </summary>
    private Task HandleBackClick()
    {
        if (_currentStep == 1)
        {
            NavigationManager.NavigateTo("quarterly-tax/missing-reports");
            return Task.CompletedTask;
        }

        if (_currentStep == 2)
        {
            var model = WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel;
            var hasAnyAdjustment = false;
            var isSSNEntered = !string.IsNullOrWhiteSpace(model.SSN);
            var isFirstNameEntered = !string.IsNullOrWhiteSpace(model.FirstName);
            var isLastNameEntered = !string.IsNullOrWhiteSpace(model.LastName);
            var fieldsChanged =
                (isSSNEntered ? 1 : 0) +
                (isFirstNameEntered ? 1 : 0) +
                (isLastNameEntered ? 1 : 0);
            foreach (var wage in WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList)
            {
                if (wage.AdjustedQuarterlyWage.HasValue)
                {
                    hasAnyAdjustment = true;
                    break;
                }
            }
            // empty continue
            if (fieldsChanged == 0 && !hasAnyAdjustment && _savedWageAdjustmentSK <= 0)
            {
                _currentStep--;
                StateHasChanged();
                return Task.CompletedTask;
            }

            _showBackModal = true;
            StateHasChanged();
            return Task.CompletedTask;
        }
        if (_currentStep == 3)
        {
            _currentStep--;
            StateHasChanged();
        }

        return Task.CompletedTask;

    }

    private void HandleEditClick()
    {
        if (_currentStep == 3)
        {
            _currentStep--;
        }
    }
    private void ValidateSSN(string normalizedSSN)
    {
        _step1MessageStore?.Clear();
        if (string.IsNullOrEmpty(normalizedSSN))
        {
            _step1MessageStore?.Add(new FieldIdentifier(this, nameof(SSN)), "SSN cannot be empty. ");
            _showErrors = true;
            _ssnFocused = true;
            _editContext.NotifyValidationStateChanged();
            _isLoading = false;
            StateHasChanged();
            return;
        }
        var isDuplicate = ExistingSSNs.Any(ssn =>
        {
            return string.Equals(SsnHelper.Normalize(ssn), normalizedSSN, StringComparison.OrdinalIgnoreCase);
        });
        if (isDuplicate)
        {
            _step1MessageStore?.Add(new FieldIdentifier(this, nameof(SSN)), "This SSN has already been added for adjustment.");
            _showErrors = true;
            _editContext.NotifyValidationStateChanged();
            _isLoading = false;
            StateHasChanged();
            return;
        }
    }

    private async Task HandleActionClick()
    {
        var account = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        if (_currentStep is 1)
        {
            _isLoading = true;
            StateHasChanged();
            var normalizedSSN = SsnHelper.Normalize(SSN);
            ValidateSSN(normalizedSSN);
            if (_showErrors)
            {
                return;
            }
            _wageDetailsResponse = await WageAdjustmentReasonService.GetWageDetailsBySSN(new WageDetailByUserRequest
            {
                SSN = normalizedSSN,
                EmployerSK = account?.Id ?? 0,
                SecureUserSK = UserAccountService.GetUserSKClaim(),
            });
            if (_wageDetailsResponse != null && _wageDetailsResponse.WageReportDetailProxy.Count() > 0)
            {
                var wageData = _wageDetailsResponse.WageReportDetailProxy[0];
                _wageReportSK = wageData?.WageReportSK;
                var employeeIdentifierViewModelData = new EmployeeIdentifierViewModel
                {
                    FirstName = wageData?.FirstName,
                    LastName = wageData?.LastName,
                    SSN = wageData?.SSN
                };

                var minYear = DateTime.Now.Year - 4;

                WageAdjustmentData = [.. _wageDetailsResponse.WageReportDetailProxy
                    .Where(wage => wage.ReportQuarter > 0 && wage.ReportYear >= minYear)
                    .OrderByDescending(wage =>
                    {
                        return wage.ReportYear;
                    })
                    .ThenByDescending(wage =>
                    {
                        return wage.ReportQuarter;
                    })
                    .Select(wage =>
                    {
                        return new WageAdjustmentModelOnly
                        {
                            Quarter = wage.ReportQuarter.ToString(),
                            Year = wage.ReportYear.ToString(),
                            QuarterlyWage = wage.GrossWages?.ToString("N2", CultureInfo.InvariantCulture),
                            AdjustmentReason = string.Empty,
                            WageReportSK = Convert.ToInt32(wage.WageReportSK),
                            WageReportDetailOrder = wage.Order
                        };
                    })];

                WageAdjustmentsMultipleQuarter = new WageAdjustmentsMultipleQuarterModelOnly
                {
                    EmployeeIdentifierModel = EmployeeIdentifierModelData,
                    EmployeeIdentifierViewModel = employeeIdentifierViewModelData,
                    WageAdjustmentQuarterlyList = WageAdjustmentData
                };
                var pendingResponse = await QuarterlyReportOrchestrator.GetPendingAdjustmentReportFromSessionAsync();
                var pending = pendingResponse?.WageAdjustmentByEmployee;
                if (pending is not null)
                {
                    _savedWageAdjustmentSK = pending.WageAdjustmentSK ?? 0;
                    foreach (var wage in WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList)
                    {
                        var savedWage = pending.WageAdjustmentDetails?.FirstOrDefault(x =>
                        {
                            return x.ReportQuarter.ToString() == wage.Quarter &&
                                   x.ReportYear.ToString() == wage.Year &&
                                   x.WageReportSK == wage.WageReportSK &&
                                   x.WageReportDetailOrder == wage.WageReportDetailOrder;
                        });
                        if (savedWage is not null)
                        {
                            wage.AdjustedQuarterlyWage = savedWage.WageAmount;
                            wage.AdjustmentReason = savedWage.AdjustmentReasons?.FirstOrDefault()?.CodeSK.ToString()
                                   ?? pending.WageAdjustmentTypeCodeSK.ToString();
                        }
                    }
                    WageAdjustmentData = WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList;
                }
                _step2EditContext = new EditContext(WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel);
                _step2EditContext.OnFieldChanged += OnFieldChanged;
                _step2MessageStore = new ValidationMessageStore(_step2EditContext);
                _currentStep++;
                _isLoading = false;
                StateHasChanged();
                return;
            }
            _step1MessageStore?.Add(new FieldIdentifier(this, nameof(SSN)), "SSN is invalid or does not exist.");
            _showErrors = true;
            _ssnFocused = true;
            _editContext.NotifyValidationStateChanged();
            _isLoading = false;
            return;
        }
        if (_currentStep is 2)
        {
            var model = WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel;
            _step2MessageStore?.Clear();
            _fieldIdsStep2.Clear();
            var hasErrors = false;
            var hasAnyAdjustment = false;
            var isSSNEntered = !string.IsNullOrWhiteSpace(model.SSN);
            var isFirstNameEntered = !string.IsNullOrWhiteSpace(model.FirstName);
            var isLastNameEntered = !string.IsNullOrWhiteSpace(model.LastName);
            var fieldsChanged =
                (isSSNEntered ? 1 : 0) +
                (isFirstNameEntered ? 1 : 0) +
                (isLastNameEntered ? 1 : 0);
            foreach (var wage in WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList)
            {
                if (wage.AdjustedQuarterlyWage.HasValue)
                {
                    hasAnyAdjustment = true;
                    break;
                }
            }
            if (fieldsChanged == 0 && !hasAnyAdjustment)
            {
                const string FieldKey = "no-adjustments";
                _step2MessageStore?.Add(new FieldIdentifier(model, FieldKey), "There are no adjustments to process.");
                hasErrors = true;
            }
            else if (fieldsChanged > 2)
            {
                const string FieldKey = "identifier-error";
                _step2MessageStore?.Add(new FieldIdentifier(model, FieldKey),
                    "You cannot change the Social Security Number, First Name and Last Name at the same time.");
                _fieldIdsStep2[FieldKey] = "modal-last-name";
                hasErrors = true;
            }
            // Length and character rules on the corrected names. The [RegularExpression] attributes
            // fire on field change, but a name restored from a saved adjustment is never "changed",
            // so it would otherwise reach the backend unchecked.
            var lastNameProblem = EmployeeFieldRules.DescribeNameProblem(model.LastName, "Last name");
            if (lastNameProblem is not null)
            {
                const string LastNameKey = "identifier-lastname";
                _step2MessageStore?.Add(new FieldIdentifier(model, LastNameKey), lastNameProblem);
                _step2MessageStore?.Add(new FieldIdentifier(model, nameof(EmployeeIdentifierModel.LastName)), lastNameProblem);
                _fieldIdsStep2[LastNameKey] = "modal-last-name";
                hasErrors = true;
            }

            var firstNameProblem = EmployeeFieldRules.DescribeNameProblem(model.FirstName, "First name");
            if (firstNameProblem is not null)
            {
                const string FirstNameKey = "identifier-firstname";
                _step2MessageStore?.Add(new FieldIdentifier(model, FirstNameKey), firstNameProblem);
                _step2MessageStore?.Add(new FieldIdentifier(model, nameof(EmployeeIdentifierModel.FirstName)), firstNameProblem);
                _fieldIdsStep2[FirstNameKey] = "modal-first-name";
                hasErrors = true;
            }

            if (isSSNEntered && SsnHelper.Normalize(model.SSN).Length < SsnHelper.IdentifierLength)
            {
                const string SSNKey = "wage-adj-ssn";
                _step2MessageStore?.Add(new FieldIdentifier(model, SSNKey), SsnFormatErrorMessage);
                _fieldIdsStep2[SSNKey] = SSNKey;
                hasErrors = true;
            }
            // An adjusted wage equal to the reported wage is a zero-variance detail that
            // the save service rejects. Caught here so the user sees which quarter is at
            // fault on Continue, rather than a generic failure on Submit.
            foreach (var wage in WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList)
            {
                if (!wage.IsAdjustedWageUnchanged())
                {
                    continue;
                }

                var fieldKey = AdjustedWageFieldKey(wage);
                _step2MessageStore?.Add(
                    new FieldIdentifier(model, fieldKey),
                    $"Q{wage.Quarter} {wage.Year}: {UnchangedWageErrorMessage}");
                _fieldIdsStep2[fieldKey] = fieldKey;
                hasErrors = true;
            }
            foreach (var wage in WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList)
            {
                if (!wage.AdjustedQuarterlyWage.HasValue || !string.IsNullOrEmpty(wage.AdjustmentReason))
                {
                    continue;
                }

                var fieldKey = AdjustmentReasonFieldKey(wage);
                _step2MessageStore?.Add(
                    new FieldIdentifier(model, fieldKey),
                    $"Q{wage.Quarter} {wage.Year}: Adjustment reason required when wage is entered.");
                _fieldIdsStep2[fieldKey] = fieldKey;
                hasErrors = true;
            }
            if (hasErrors)
            {
                _showStep2Errors = true;
                _step2EditContext.NotifyValidationStateChanged();
                return;
            }
            _showStep2Errors = false;
            _currentStep++;
            return;
        }
        if (_currentStep is 3)
        {
            _isLoading = true;
            StateHasChanged();

            var wageAdjustmentDetails = BuildWageAdjustmentDetails(
                WageAdjustmentsMultipleQuarter.WageAdjustmentQuarterlyList, true);

            var wageAdjustmentRequest = BuildWageAdjustmentRequest(account?.Id ?? 0, wageAdjustmentDetails);

            var response = await WageAdjustmentReasonService.SaveWageAdjustmentBySSN(wageAdjustmentRequest);
            if (response.ConfirmationNumber is not null)
            {
                _confirmationNumber = response.ConfirmationNumber;
                _pageState = PageState.Confirmation;
                await QuarterlyReportOrchestrator.ClearPendingAdjustmentFromSessionAsync();
            }
            else
            {
                SubmitResponseHasErrors(response);
            }
            _isLoading = false;
            return;
        }
        return;
    }

    private WageAdjustmentRequestBySSN[] BuildWageAdjustmentDetails(
        List<WageAdjustmentModelOnly> wageAdjustmentData, bool includeAll = false)
    {
        return includeAll
            ? [.. wageAdjustmentData.Select(x =>
            {
                return new WageAdjustmentRequestBySSN
                {
                    WageAdjustmentReasonCodeSK = ParseAdjustmentReason(x.AdjustmentReason),
                    AdjustedGrossWages = x.AdjustedQuarterlyWage,
                    WageReportSK = x.WageReportSK ?? Convert.ToInt32(_wageReportSK),
                    Order = x.WageReportDetailOrder ?? 0
                };
            })]
            : [.. wageAdjustmentData.Where(x =>
            {
                return x.AdjustedQuarterlyWage is not null;
            })
            .Select(x =>
            {
                return new WageAdjustmentRequestBySSN
                {
                    WageAdjustmentReasonCodeSK = ParseAdjustmentReason(x.AdjustmentReason),
                    AdjustedGrossWages = x.AdjustedQuarterlyWage,
                    WageReportSK = x.WageReportSK ?? Convert.ToInt32(_wageReportSK),
                    Order = x.WageReportDetailOrder ?? 0
                };
            })];
    }

    private static int? ParseAdjustmentReason(string? adjustmentReason)
    {
        return string.IsNullOrWhiteSpace(adjustmentReason) ? null : Convert.ToInt32(adjustmentReason);
    }

    private bool SubmitResponseHasErrors(SaveWageAdjustmentResponse? response)
    {
        var model = WageAdjustmentsMultipleQuarter.EmployeeIdentifierModel;
        _step2MessageStore?.Clear();

        var technicalErrorMessage = Configuration["Messages:TechnicalDifficulties"]
                   ?? "We are currently experiencing technical difficulties. Please try again later.";


        if (response is null)
        {
            _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), technicalErrorMessage);
            _showStep2Errors = true;
            _step2EditContext.NotifyValidationStateChanged();
            return true;
        }

        if (response.RuleViolations != null && response.RuleViolations.Length > 0)
        {
            foreach (var ruleViolation in response.RuleViolations)
            {
                if (ruleViolation != null)
                {
                    if (ruleViolation.RuleViolation.Contains("Original SSN, Last Name, First Name did not match", StringComparison.InvariantCultureIgnoreCase))
                    {
                        _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), "Original SSN, Last Name, First Name did not match for some reports");
                    }
                    else
                    {
                        _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), ruleViolation.RuleViolation);
                    }
                }
            }
            _showStep2Errors = true;
            _step2EditContext.NotifyValidationStateChanged();
            return true;
        }

        if (string.IsNullOrEmpty(response.ConfirmationNumber))
        {
            _step2MessageStore?.Add(new FieldIdentifier(model, string.Empty), technicalErrorMessage);
            _showStep2Errors = true;
            _step2EditContext.NotifyValidationStateChanged();
            return true;
        }

        return false;
    }

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

    /// <summary>
    /// DropDownItem.
    /// </summary>
    public partial class DropDownItem
    {
        /// <summary>
        /// OnInitialized
        /// </summary>
        public string? Text { get; set; } = "";

        /// <summary>
        /// OnInitialized
        /// </summary>
        public int? Value { get; set; }
    }
}
