using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.WageUpload;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Layout;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Wage Upload Report
/// </summary>
public partial class WageUploadReport
{
    [Inject] private IWageFileUploadService WageFileUploadService { get; set; } = default!;
    [Inject] private IContactInformationService ContactInformationService { get; set; } = default!;
    [Inject] private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject] private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    [Inject] private IESPDashboardService ESPDashboardService { get; set; } = default!;

    [Inject]
    private ILayoutOrchestator LayoutOrchestator { get; set; } = default!;



    /// <summary>
    /// 
    /// </summary>
    public enum PageState
    {
        /// <summary>
        /// 
        /// </summary>
        Wizard,
        /// <summary>
        /// 
        /// </summary>
        Confirmation
    }

    private PageState _pageState = PageState.Wizard;
    private bool _isLoading;
    private bool _isSourceTestEnv;
    private string? _confirmationNumber;
    private int _currentStep = 1;
    private bool _showValidationSummary;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private readonly Dictionary<string, string> _fieldIds = new();
    private bool _isActingAsEsp = false;

    private WageFileUploadEntry? _wageFileUploadEntryRef;
    private ContactInformationEntry? _contactInfoRef;

    private readonly List<WizardStep> _wizardSteps =
    [
        new() { StepNumber = 1, Title = "Contact Information", ActionButtonText="Continue"},
        new() { StepNumber = 2, Title = "Select Method and File Upload", ActionButtonText="Submit"}
    ];

    private readonly WageUploadReportModel _reportData = new();

    /// <summary>
    /// 
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);
        _editContext = new EditContext(_reportData);
        _validationMessageStore = new ValidationMessageStore(_editContext);
        _isActingAsEsp = await LayoutOrchestator.IsServiceProviderActingAsEspAsync();
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var result = await ContactInformationService.ObtainFileContact(secureUserSK);

        if (result != null)
        {
            _reportData.ContactData.CopyFrom(result!);
        }
    }

    private async Task HandleActionClick()
    {
        ClearSubmitErrors();

        if (_currentStep == 1 && _contactInfoRef != null)
        {
            if (!_contactInfoRef.IsValid())
            {
                return;
            }
        }

        if (_currentStep == 2 && _wageFileUploadEntryRef != null)
        {
            if (!_wageFileUploadEntryRef.IsValid())
            {
                return;
            }
        }

        _isLoading = true;
        StateHasChanged();
        await Task.Yield();
        await OnNextAsync();
        _isLoading = false;
    }

    private async Task OnNextAsync()
    {
        var ok = await ProcessCurrentStepAsync();
        if (ok && _currentStep < _wizardSteps.Count)
        {
            _currentStep++;
        }
    }

    private async Task<bool> ProcessCurrentStepAsync()
    {
        switch (_currentStep)
        {
            case 1:
                return true;
            case 2:
                //Persist the file-upload contact before file submission.

                if (_isActingAsEsp)
                {
                    if (_wageFileUploadEntryRef != null)
                    {
                        var savedPath = await _wageFileUploadEntryRef.SaveFileAsync();
                        if (!string.IsNullOrEmpty(savedPath))
                        {
                            _reportData.WageFileData.UploadedFilePath = savedPath;
                        }
                    }
                    var filePath = _reportData.WageFileData.UploadedFilePath;
                    if (filePath != null)
                    {
                        var fileName = _reportData.WageFileData.FileName ?? string.Empty;
                        var fileExtension = Path.GetExtension(filePath);
                        var fileContent = File.ReadAllText(filePath);

                        //Validate File
                        var validationResponse = await ESPDashboardService.ValidateWageReportUploadAsync(fileContent, fileName, fileExtension);
                        if (validationResponse.RuleViolations.Length > 0)
                        {
                            return RegisterESPValidationError(validationResponse.RuleViolations);
                        }
                        var employerSk = await ESPDashboardService.GetEmployerSkAsync();
                        var fileTypeCode = _reportData.WageReportType switch
                        {
                            "Original" => WageFileTypeCode.WageOriginal,
                            "Append" => WageFileTypeCode.WageAppend,
                            "Replace" => WageFileTypeCode.WageReplace,
                            _ => WageFileTypeCode.WageOriginal
                        };

                        //Upload File
                        var wageFileUploadRequest = new WageFileUploadRequest()
                        {
                            FilePath = filePath,
                            CommonClientSK = employerSk ?? 0,
                            SecureUserSk = UserAccountService.GetUserSKClaim(),
                            IsTestFile = _isSourceTestEnv,
                            FileRecordCount = validationResponse.RecordCount,
                            FileUploadFormatCodeSK = validationResponse.FileUploadFormatCodeSK,
                            WebUserFileStatusCodeSK = (int) WageFileStatusCode.Requested,
                            WebUserFileTypeCodeSK = (int) fileTypeCode,
                            OriginalFileName = _reportData.WageFileData.FileName
                        };
                        var fileUploadResponse = await ESPDashboardService.UploadWageReportAsync(wageFileUploadRequest);
                        if (!string.IsNullOrEmpty(fileUploadResponse.ConfirmationNumber))
                        {
                            await JS.InvokeVoidAsync("scrollToTop");
                            _confirmationNumber = fileUploadResponse.ConfirmationNumber;
                            _pageState = PageState.Confirmation;
                            return true;
                        }
                        return RegisterESPValidationError(fileUploadResponse.RuleViolations);
                    }
                    return true;
                }
                else
                {
                    var contactSavedResponse = await ContactInformationService.SaveFileContact(_reportData.ContactData);
                    if (contactSavedResponse.RuleViolations != null && contactSavedResponse.RuleViolations.Length > 0)
                    {
                        return RegisterValidationError(contactSavedResponse.RuleViolations);
                    }

                    if (_wageFileUploadEntryRef != null)
                    {
                        var savedPath = await _wageFileUploadEntryRef.SaveFileAsync();
                        if (!string.IsNullOrEmpty(savedPath))
                        {
                            _reportData.WageFileData.UploadedFilePath = savedPath;
                        }
                    }
                    var fileTypeCode = _reportData.WageReportType switch
                    {
                        "Original" => WageFileTypeCode.WageOriginal,
                        "Append" => WageFileTypeCode.WageAppend,
                        "Replace" => WageFileTypeCode.WageReplace,
                        _ => WageFileTypeCode.WageOriginal
                    };

                    var result = await WageFileUploadService.SubmitWageFileUploadAsync(
                        _reportData.WageFileData.UploadedFilePath ?? string.Empty,
                        WageFileStatusCode.Requested,
                        fileTypeCode,
                        _reportData.WageFileData.RecordCount,
                        _reportData.WageFileData.FileUploadFormatCodeSK,
                        _isSourceTestEnv,
                        _reportData.WageFileData.FileName!);

                    if (!string.IsNullOrEmpty(result.ConfirmationNumber))
                    {
                        await JS.InvokeVoidAsync("scrollToTop");
                        _confirmationNumber = result.ConfirmationNumber;
                        _pageState = PageState.Confirmation;
                        return true;
                    }

                    return RegisterValidationError(result.RuleViolations);
                }
        }
        return true;
    }

    private bool RegisterESPValidationError(RuleViolationProxy[] ruleViolations)
    {
        if (ruleViolations != null && ruleViolations.Length > 0)
        {
            if (ruleViolations != null && ruleViolations.Length > 0)
            {
                ShowSubmitErrors(ruleViolations.Select(v =>
                {
                    return v.RuleViolation ?? string.Empty;
                }));
            }
            _isLoading = false;
        }
        else
        {
            ShowSubmitErrors(new[]
            {
                        Configuration["Messages:TechnicalDifficulties"]
                        ?? "We are currently experiencing technical difficulties. Please try again later."
                    });
            _isLoading = false;
        }
        return false;
    }

    private bool RegisterValidationError(UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.RuleViolationProxy[] ruleViolations)
    {
        if (ruleViolations != null && ruleViolations.Length > 0)
        {
            if (ruleViolations != null && ruleViolations.Length > 0)
            {
                ShowSubmitErrors(ruleViolations.Select(v =>
                {
                    return v.RuleViolation ?? string.Empty;
                }));
            }
            _isLoading = false;
        }
        else
        {
            ShowSubmitErrors(new[]
            {
                        Configuration["Messages:TechnicalDifficulties"]
                        ?? "We are currently experiencing technical difficulties. Please try again later."
                    });
            _isLoading = false;
        }
        return false;
    }
    private void HandleBackClick()
    {
        if (_currentStep == 1)
        {
            if (_isSourceTestEnv)
            {
                NavigationManager.NavigateTo("quarterly-tax/validation-test");
                return;
            }

            NavigationManager.NavigateTo("quarterly-tax/missing-reports");
        }
    }

    private void ShowSubmitErrors(IEnumerable<string> errors)
    {
        _validationMessageStore.Clear();
        foreach (var error in errors)
        {
            _validationMessageStore.Add(new FieldIdentifier(_reportData, string.Empty), error);
        }

        _showValidationSummary = true;
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();
    }

    private void ClearSubmitErrors()
    {
        _validationMessageStore.Clear();
        _showValidationSummary = false;
        _editContext.NotifyValidationStateChanged();
    }

    private string ConfirmationReportType
    {
        get
        {
            return _reportData.WageReportType switch
            {
                "Original" => "Original",
                "Append" => "Appended",
                "Replace" => "Replacement",
                _ => _reportData.WageReportType
            };
        }
    }
}
