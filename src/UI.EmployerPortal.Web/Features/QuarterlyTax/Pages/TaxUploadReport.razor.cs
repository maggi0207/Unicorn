using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.TaxUpload;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.WageUpload;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Tax File Upload page
/// Contact information and file upload are presented together.
/// Upload only — no Append or Replace options.
/// </summary>
public partial class TaxUploadReport
{
    [Inject] private ITaxFileUploadService TaxFileUploadService { get; set; } = default!;
    [Inject] private IContactInformationService ContactInformationService { get; set; } = default!;
    [Inject] private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    private bool _isSubmitted;
    private bool _isLoading;
    private bool _showValidationSummary;
    private bool _isSourceTestEnv;
    private string _confirmationNumber = string.Empty;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private readonly Dictionary<string, string> _fieldIds = new();

    private TaxFileUploadEntry? _taxFileUploadEntryRef;
    private ContactInformationEntry? _contactInfoRef;

    private readonly TaxUploadReportModel _reportData = new();

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);

        _editContext = new EditContext(_reportData);
        _validationMessageStore = new ValidationMessageStore(_editContext);

        var secureUserSK = UserAccountService.GetUserSKClaim();
        var result = await ContactInformationService.ObtainFileContact(secureUserSK);
        if (result != null)
        {
            _reportData.ContactData.CopyFrom(result!);
        }
    }

    private async Task HandleSubmitClick()
    {
        ClearSubmitErrors();

        var contactValid = _contactInfoRef?.IsValid() ?? false;
        var fileValid = _taxFileUploadEntryRef?.IsValid() ?? false;

        if (!fileValid)
        {
            var fileErrors = new List<string>();
            if (!_reportData.TaxFileData.IsUploaded)
            {
                fileErrors.Add("Please upload a tax report file.");
            }
            if (_reportData.TaxFileData.HasBlockingErrors)
            {
                fileErrors.Add("Please fix file errors before continuing.");
            }
            ShowSubmitErrors(fileErrors);
        }
        if (!contactValid || !fileValid)
        {
            return;
        }

        _isLoading = true;
        StateHasChanged();
        await Task.Yield();

        // Persist contact info.
        var contactSaved = await ContactInformationService.SaveFileContact(_reportData.ContactData);
        if (contactSaved.RuleViolations != null && contactSaved.RuleViolations.Length > 0)
        {
            RegisterValidationError(contactSaved.RuleViolations);
            _isLoading = false;
            return;
        }

        // Stage the file.
        if (_taxFileUploadEntryRef != null)
        {
            var savedPath = await _taxFileUploadEntryRef.SaveFileAsync();
            if (!string.IsNullOrEmpty(savedPath))
            {
                _reportData.TaxFileData.UploadedFilePath = savedPath;
            }
        }

        // Submit.
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var originalFileName = _reportData.TaxFileData.FileName ?? string.Empty;
        var result = await TaxFileUploadService.SubmitTaxFileUploadAsync(
            _reportData.TaxFileData.UploadedFilePath ?? string.Empty,
            secureUserSK,
            TaxFileUploadStatusCode.Requested,
            _reportData.TaxFileData.RecordCount,
            originalFileName,
            isTestEnv: _isSourceTestEnv);

        if (result.Success)
        {
            _confirmationNumber = result.ConfirmationNumber ?? string.Empty;
            await JS.InvokeVoidAsync("scrollToTop");
            _isSubmitted = true;
        }
        else
        {
            RegisterValidationError(result.RuleViolations);
        }

        _isLoading = false;
    }

    private void HandleBackClick()
    {
        NavigationManager.NavigateTo("esp-dashboard");
    }

    private void RegisterValidationError(
        UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.RuleViolationProxy[]? ruleViolations)
    {
        if (ruleViolations != null && ruleViolations.Length > 0)
        {
            ShowSubmitErrors(ruleViolations.Select(v =>
            {
                return v.RuleViolation ?? string.Empty;
            }));
        }
        else
        {
            ShowSubmitErrors(new[]
            {
                Configuration["Messages:TechnicalDifficulties"]
                    ?? "We are currently experiencing technical difficulties. Please try again later."
            });
        }
    }

    private void RegisterValidationError(
        UI.EmployerPortal.Generated.ServiceClients.ESPService.RuleViolationProxy[]? ruleViolations)
    {
        if (ruleViolations != null && ruleViolations.Length > 0)
        {
            ShowSubmitErrors(ruleViolations.Select(v =>
            {
                return v.RuleViolation ?? string.Empty;
            }));
        }
        else
        {
            ShowSubmitErrors(new[]
            {
                Configuration["Messages:TechnicalDifficulties"]
                    ?? "We are currently experiencing technical difficulties. Please try again later."
            });
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
    private void HandleFileAnotherReportClick()
    {
        NavigationManager.NavigateTo(
            TestEnvironmentSource.Preserve("quarterly-tax/tax-file-upload", _isSourceTestEnv), forceLoad: true);
    }
}
