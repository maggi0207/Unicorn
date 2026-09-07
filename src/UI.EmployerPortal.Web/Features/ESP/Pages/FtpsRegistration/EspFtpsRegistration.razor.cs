using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.ESP.Components.FtpsRegistration;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Logging;

namespace UI.EmployerPortal.Web.Features.ESP.Pages.FtpsRegistration;

/// <summary>
/// Employer Service Provider FTPS (file exchange) registration page.
/// After certifying understanding of acceptable use,
/// walks the user through data-entry, submission and confirmation.
/// </summary>
public partial class EspFtpsRegistration
{
    [Inject] private IEspFtpsRegistrationService EspFtpsRegistrationService { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;
    [Inject] private ILogger<EspFtpsRegistration> Logger { get; set; } = default!;

    private EspFtpsRegistrationModel _model = new();
    private EspFtpsRegistrationEntry _entryRef = default!;
    private readonly List<string> _ruleViolationMessages = [];
    private enum PageState { Certify, Entry, Confirm }
    private PageState _pageState = PageState.Certify;
    private bool _isEntryLoading;
    private bool _isServiceError = false;
    private bool _userAcknowledgedAcceptedUsePolicy;

    /// <summary>
    /// Reacts to the certification page checkbox
    /// </summary>
    /// <param name="value">Indicates whether the user read the acceptable use policy.</param>
    private void HandleCertifiedChanged(bool value)
    {
        _userAcknowledgedAcceptedUsePolicy = value;
    }

    /// <summary>
    /// Submit the form
    /// </summary>
    private async Task HandleSubmit()
    {
        if (_entryRef is not null && !_entryRef.IsValid())
        {
            return;
        }

        ClearRuleViolations();
        _isEntryLoading = true;
        StateHasChanged();
        await Task.Yield();

        // 1. Create the file contact
        CreateFileContactResponse fileContactResponse;
        try
        {
            fileContactResponse = await EspFtpsRegistrationService.SaveFileContactAsync(_model);
            if (fileContactResponse.RuleViolations.Length != 0)
            {
                AddRuleViolations(fileContactResponse.RuleViolations);
                _pageState = PageState.Entry;
                await ConcludeSubmitAsync();
                return;
            }
        }
        catch (Exception ex)
        {
            LogErrorSavingFileContact(Logger, _model.BusinessName ?? "[unknown business]", ex);
            _isServiceError = true;
            await ConcludeSubmitAsync();
            return;
        }

        // 2. Request FTp access for the current user.
        try
        {
            var response = await EspFtpsRegistrationService.SaveFtpUserAsync();
            if (response.RuleViolations.Length > 0)
            {
                _isServiceError = true;
            }
            else
            {
                _pageState = PageState.Confirm;
            }
        }
        catch (Exception ex)
        {
            LogErrorSavingFtpUser(Logger, _model.BusinessName ?? "[unknown business]", ex);
            _isServiceError = true;
        }
        finally
        {
            await ConcludeSubmitAsync();
        }
    }

    // Helper to conclude the submit handler process
    private async Task ConcludeSubmitAsync()
    {
        _isEntryLoading = false;
        await JS.InvokeVoidAsync("scrollToTop");
        StateHasChanged();
    }

    // Helper to add rule violations for either service response
    private void AddRuleViolations(RuleViolationProxy[] violations)
    {
        _ruleViolationMessages.AddRange(violations
            .Select(v => v.RuleViolation ?? string.Empty)
            .Where(message => !string.IsNullOrWhiteSpace(message)));
    }

    /// <summary>
    /// Navigate to the dashboard
    /// </summary>
    private void HandleGoToDashboard()
    {
        NavigationManager.NavigateTo("esp-dashboard");
    }

    private void ClearRuleViolations()
    {
        _ruleViolationMessages.Clear();
    }

    [LoggerMessage(
        EventId = LogEventIds.SaveFileContactServiceException,
        Level = LogLevel.Error,
        Message = "Failed to save FTPS file contact for {BusinessName}.")]
    private static partial void LogErrorSavingFileContact(ILogger logger, string businessName, Exception ex);

    [LoggerMessage(
        EventId = LogEventIds.SaveFtpUserServiceException,
        Level = LogLevel.Error,
        Message = "Failed to save FTPS FTP user for {BusinessName}.")]
    private static partial void LogErrorSavingFtpUser(ILogger logger, string businessName, Exception ex);
}
