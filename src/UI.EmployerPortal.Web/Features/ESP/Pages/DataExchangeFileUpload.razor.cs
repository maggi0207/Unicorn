using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ESP.Components;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components.WageUpload;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;


/// <summary>
/// Data Exchange File Upload page
/// Contact information and file upload are presented together.
/// Upload only — no Append or Replace options.
/// </summary>

public partial class DataExchangeFileUpload
{
    [Inject] private IDataExchangeFileUploadService DataExchangeFileUploadService { get; set; } = default!;
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
    private DataExchangeConfirmationResult? _confirmation;
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private readonly Dictionary<string, string> _fieldIds = new();

    private DataExchangeFileUploadEntry? _taxFileUploadEntryRef;
    private ContactInformationEntry? _contactInfoRef;

    private readonly DataExchangeFileUploadReportModel _reportData = new();

    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = false;
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var queryStrings = QueryHelpers.ParseQuery(uri.Query);
        if (queryStrings.TryGetValue("source", out var source))
        {
            if (source == "test-environment")
            {
                _isSourceTestEnv = true;
            }
        }

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
                _reportData.DataExchangeFileData.UploadedFilePath = savedPath;
            }
        }

        // Submit.
        var secureUserSK = UserAccountService.GetUserSKClaim();

        var result = await DataExchangeFileUploadService.SubmitDataExchangeFileUploadAsync(
            _reportData.DataExchangeFileData.UploadedFilePath ?? string.Empty,
            secureUserSK,
            TaxFileUploadStatusCode.Requested,
            _reportData.DataExchangeFileData.RecordCount,
            isTestEnv: _isSourceTestEnv);

        if (result != null && result.Success)
        {
            if (!string.IsNullOrEmpty(result.ConfirmationNumber))
            {
                _confirmation = new DataExchangeConfirmationResult
                {
                    ConfirmationNumber = result.ConfirmationNumber ?? string.Empty,
                    FileName = _reportData.DataExchangeFileData?.FileName ?? string.Empty,
                    UploadDate = DateTime.Now,
                    RecordCount = _reportData?.DataExchangeFileData?.RecordCount ?? 0,
                };
                _isSubmitted = true;
                //route user to confirmation page.
            }
            await JS.InvokeVoidAsync("scrollToTop");
            _isSubmitted = true;
        }
        else
        {
            RegisterValidationError(result?.RuleViolations);
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
        NavigationManager.NavigateTo("esp/data-exchange/upload", forceLoad: true);
    }
}
