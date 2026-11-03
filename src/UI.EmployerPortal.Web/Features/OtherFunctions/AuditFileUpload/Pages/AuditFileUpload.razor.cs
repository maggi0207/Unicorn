using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Routing;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Components;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Extensions;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Components;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Pages;

/// <summary>
/// Audit File Upload page. Lets an employer user verify their contact information and upload
/// an audit file for the selected UI account, then shows a confirmation once received.
/// </summary>
public partial class AuditFileUpload : IDisposable
{
    [Inject] private IAuditFileUploadService AuditFileUploadService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IConfiguration Configuration { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ISessionManager SessionManager { get; set; } = default!;
    [Inject] private IUploadFolderResolver UploadFolderResolver { get; set; } = default!;

    private EmployerAccount? _employerAccountNotSavedInSession;

    /// <summary>
    /// The state the page is currently rendering.
    /// </summary>
    private enum PageState
    {
        Form,
        Confirmation
    }

    private PageState _pageState = PageState.Form;
    private bool _isLoading = true;
    private bool _isEmployerSelected = false;
    private string _folderPath = string.Empty;
    private IReadOnlyList<string> _lookupErrors = [];
    private string? _lastEnteredUIAccountNumber;
    private string? _lastEnteredFein;

    private const string StandaloneRoute = "audit-file-upload";
    private const string EmployerContextRoute = "other-functions/upload-audit";
    private bool? _entryIsStandalone;

    private string CurrentPath
        => NavigationManager.ToBaseRelativePath(NavigationManager.Uri)
        .Split('?')[0]
        .TrimEnd('/');

    private bool IsStandaloneEntry
        => CurrentPath.Equals(StandaloneRoute, StringComparison.OrdinalIgnoreCase);

    private bool IsOnAuditFileUpload =>
        IsStandaloneEntry || CurrentPath.Equals(EmployerContextRoute, StringComparison.OrdinalIgnoreCase);

    private readonly AuditFileUploadModel _model = new();
    private AuditFileUploadResult _uploadResult = new();

    private AuditContactInformation? _contactFormRef;
    private FileUpload2? _fileUploadRef;

    private EditContext _submitEditContext = default!;
    private ValidationMessageStore _submitMessageStore = default!;
    private bool _showSubmitErrors;
    private bool _submitAttempted;
    private readonly Dictionary<string, string> _submitFieldIds = new();

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        _submitEditContext = new EditContext(_model);
        _submitMessageStore = new ValidationMessageStore(_submitEditContext);

        NavigationManager.LocationChanged += OnLocationChanged;

        _entryIsStandalone = IsStandaloneEntry;
        await ResolveEntryAsync(_entryIsStandalone.Value);
    }

    /// <inheritdoc/>
    protected void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (!IsOnAuditFileUpload)
        {
            return;
        }

        var standalone = IsStandaloneEntry;

        if (_entryIsStandalone == standalone)
        {
            return;
        }

        InvokeAsync(async () =>
        {
            _entryIsStandalone = standalone;
            await ResolveEntryAsync(standalone);
            StateHasChanged();
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private async Task ResolveEntryAsync(bool standalone)
    {
        _isLoading = true;

        _employerAccountNotSavedInSession = null;
        _lookupErrors = [];
        _lastEnteredUIAccountNumber = null;
        _lastEnteredFein = null;
        _pageState = PageState.Form;
        _uploadResult = new AuditFileUploadResult();
        _model.Contact = new AuditContactModel();
        _model.UploadedFilePath = null;
        _model.FileName = null;
        _model.IsFileUploaded = false;
        _submitAttempted = false;
        ClearSubmitErrors();

        if (standalone)
        {
            var hadAccount = await SessionManager.GetAsync<SelectedEmployerAccount>() is not null
                || await SessionManager.GetAsync<SessionSelectedEmployerAccount>() is not null;
            await SessionManager.ClearAsync<SessionSelectedEmployerAccount>();
            await SessionManager.ClearAsync<SelectedEmployerAccount>();
            if (hadAccount)
            {
                NavigationManager.NavigateTo(NavigationManager.Uri, forceLoad: true);
                return;
            }
        }

        var selectedEmployer = standalone
            ? null
            : await SessionManager.GetAsync<SelectedEmployerAccount>();

        _isEmployerSelected = selectedEmployer != null;

        if (!_isEmployerSelected)
        {
            _isLoading = false;
            return;
        }

        _folderPath = UploadFolderResolver.Resolve("FileUpload:AuditFile:FolderPath") ?? string.Empty;

        var existingContact = await AuditFileUploadService.ObtainContactAsync();
        if (existingContact is not null)
        {
            _model.Contact = existingContact;
        }

        _isLoading = false;
    }

    private async Task HandleAccountLookupContinue((string UIAccountNumber, string FEIN) args)
    {
        _lastEnteredUIAccountNumber = args.UIAccountNumber;
        _lastEnteredFein = args.FEIN;

        _lookupErrors = [];
        _isLoading = true;
        StateHasChanged();
        await Task.Yield();

        _employerAccountNotSavedInSession = await AuditFileUploadService.LookupEmployerAsync(args.UIAccountNumber, args.FEIN);

        if (_employerAccountNotSavedInSession is null)
        {
            _isLoading = false;
            _lookupErrors = ["No UI account was found matching the provided UI Account Number and FEIN. Please verify your information and try again."];
            StateHasChanged();
            return;
        }

        _isEmployerSelected = true;

        _folderPath = UploadFolderResolver.Resolve("FileUpload:AuditFile:FolderPath") ?? string.Empty;

        var existingContact = await AuditFileUploadService.ObtainContactAsync();
        if (existingContact is not null)
        {
            _model.Contact = existingContact;
        }

        _isLoading = false;
    }
    private void HandleFileUploaded(string filePath)
    {
        _model.UploadedFilePath = filePath;
        _model.FileName = filePath.ToDisplayFileName();
        _model.IsFileUploaded = true;

        RefreshSubmitErrors();
    }
    private void HandleFileRemoved()
    {
        _model.UploadedFilePath = null;
        _model.FileName = null;
        _model.IsFileUploaded = false;
    }

    private async Task HandleSubmit()
    {
        _submitAttempted = true;

        var isContactValid = _contactFormRef?.IsValid() ?? false;

        BuildSubmitErrors();

        var fileMissing = !_model.IsFileUploaded || string.IsNullOrEmpty(_model.UploadedFilePath);

        if (!isContactValid || fileMissing)
        {
            StateHasChanged();
            return;
        }

        _isLoading = true;
        StateHasChanged();
        await Task.Yield();

        var recordCount = _fileUploadRef != null ? await _fileUploadRef.GetRecordCountAsync() : 0;

        var result = await AuditFileUploadService.SubmitAuditFileUploadAsync(
             _model.Contact,
             _model.UploadedFilePath!,
             _model.FileName ?? string.Empty,
             recordCount,
             _employerAccountNotSavedInSession);

        _isLoading = false;

        if (result.IsSuccess)
        {
            _uploadResult = result;
            _pageState = PageState.Confirmation;
            await JS.InvokeVoidAsync("scrollToTop");
            return;
        }

        var messages = result.RuleViolations.Count > 0
            ? result.RuleViolations
            : new[]
            {
                Configuration["Messages:TechnicalDifficulties"]
                ?? "We are currently experiencing technical difficulties. Please try again later."
            };

        _submitMessageStore.Clear();
        foreach (var message in messages)
        {
            _submitMessageStore.Add(new FieldIdentifier(_model, string.Empty), message);
        }
        _showSubmitErrors = true;
        _submitEditContext.NotifyValidationStateChanged();
        StateHasChanged();
    }

    /// <summary>
    /// Only rebuilds after a submit has been attempted, matching how field-level errors on
    /// the contact form only surface after submit or per-field interaction.
    /// </summary>
    private void RefreshSubmitErrors()
    {
        if (!_submitAttempted)
        {
            return;
        }

        BuildSubmitErrors();
        StateHasChanged();
    }

    private void BuildSubmitErrors()
    {
        _submitMessageStore.Clear();
        _submitFieldIds.Clear();

        var contactErrors = _contactFormRef?.GetCurrentErrors() ?? [];
        var index = 0;
        foreach (var (message, targetId) in contactErrors)
        {
            var key = $"Contact_{index++}";
            if (targetId is not null)
            {
                _submitFieldIds[key] = targetId;
            }
            _submitMessageStore.Add(new FieldIdentifier(_model, key), message);
        }

        if (!_model.IsFileUploaded || string.IsNullOrEmpty(_model.UploadedFilePath))
        {
            const string FileKey = "AuditFile";
            _submitFieldIds[FileKey] = "file-upload-browse-button";
            _submitMessageStore.Add(new FieldIdentifier(_model, FileKey), "Please choose an audit file to upload.");
        }

        _showSubmitErrors = _submitMessageStore.GetType() is not null && _submitEditContext.GetValidationMessages().Any();
        _submitEditContext.NotifyValidationStateChanged();
    }

    private void HandleBack()
    {
        if (_employerAccountNotSavedInSession is not null)
        {
            _employerAccountNotSavedInSession = null;
            _isEmployerSelected = false;
            _lookupErrors = [];
            ClearSubmitErrors();
            return;
        }

        NavigationManager.NavigateTo("other-functions/select-other-functions");
    }

    private void HandleUploadAnother()
    {
        _model.UploadedFilePath = null;
        _model.FileName = null;
        _model.IsFileUploaded = false;
        _uploadResult = new AuditFileUploadResult();
        _pageState = PageState.Form;
        _submitAttempted = false;

        ClearSubmitErrors();

        if (_employerAccountNotSavedInSession is not null)
        {
            _employerAccountNotSavedInSession = null;
            _isEmployerSelected = false;
            _lookupErrors = [];
            _model.Contact = new AuditContactModel();
            return;
        }

        _model.Contact = new AuditContactModel();
        _isLoading = true;

        // Re-fetch the saved contact so the form is pre-populated for the next upload.
        _ = ReloadContactAsync();
    }

    private async Task ReloadContactAsync()
    {
        var existingContact = await AuditFileUploadService.ObtainContactAsync();
        if (existingContact is not null)
        {
            _model.Contact = existingContact;
        }

        _isLoading = false;
        StateHasChanged();
    }

    private void ClearSubmitErrors()
    {
        _submitMessageStore.Clear();
        _submitFieldIds.Clear();
        _showSubmitErrors = false;
        _submitEditContext.NotifyValidationStateChanged();
    }
}
