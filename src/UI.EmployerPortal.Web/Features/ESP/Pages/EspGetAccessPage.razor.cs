using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Layout;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Page for ESP users to enter an access key and FEIN to gain access to an employer account.
/// </summary>
public partial class EspGetAccessPage
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;
    [Inject]
    private ILayoutOrchestator LayoutOrchestator { get; set; } = default!;

    private readonly EspAccessKeyFormModel _form = new();
    private EditContext _editContext = default!;
    private string? _serviceError = null;
    private string? _successMessage = null;
    private bool _isLoading = false;
    private bool _showValidationError = false;

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        { nameof(EspAccessKeyFormModel.AccessKey), "gak-key" },
        { nameof(EspAccessKeyFormModel.FEIN), "gak-fein" }
    };

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_form);
    }

    private async Task HandleValidSubmit()
    {
        _showValidationError = false;
        _serviceError = null;
        _successMessage = null;
        _isLoading = true;
        var (success, error) = await AccountUserService.ActivateESPAccessKeyAsync(_form.AccessKey, _form.FEIN);
        _isLoading = false;

        if (success)
        {
            await SessionManager.ClearAsync<SessionAllEmployerAccounts>();
            await LayoutOrchestator.RequestEmployerListRefreshAsync("ESP account was added");
            _successMessage = "ESP Access Key activated successfully.";
            _form.AccessKey = string.Empty;
            _form.FEIN = string.Empty;
            _editContext = new EditContext(_form);
        }
        else
        {
            _serviceError = error;
        }
    }

    private void HandleInvalidSubmit()
    {
        _showValidationError = true;
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("esp-dashboard");
    }

    private sealed class EspAccessKeyFormModel
    {
        [Required(ErrorMessage = "Access Key is required.")]
        public string AccessKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "FEIN is required.")]
        [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "FEIN format is invalid (e.g. 12-3456789).")]
        public string FEIN { get; set; } = string.Empty;
    }
}
