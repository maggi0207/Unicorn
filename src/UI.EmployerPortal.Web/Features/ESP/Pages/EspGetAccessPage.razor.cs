using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
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

    private readonly EspAccessKeyFormModel _form = new();
    private EditContext _editContext = default!;
    private string? _serviceError = null;
    private string? _successMessage = null;
    private bool _isLoading = false;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_form);
    }

    private async Task HandleValidSubmit()
    {
        _serviceError = null;
        _successMessage = null;
        _isLoading = true;
        var (success, error) = await AccountUserService.ActivateESPAccessKeyAsync(_form.AccessKey, _form.FEIN);
        _isLoading = false;

        if (success)
        {
            await SessionManager.ClearAsync<SessionAllEmployerAccounts>();
            _successMessage = "ESP Access key activated successfully. Please log out and log back in to apply changes.";
            _form.AccessKey = string.Empty;
            _form.FEIN = string.Empty;
            _editContext = new EditContext(_form);
        }
        else
        {
            _serviceError = error;
        }
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("esp-dashboard");
    }

    private sealed class EspAccessKeyFormModel : IValidatableObject
    {
        [Required(ErrorMessage = "Access Key is required.")]
        public string AccessKey { get; set; } = string.Empty;

        public string FEIN { get; set; } = string.Empty;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (string.IsNullOrWhiteSpace(FEIN))
            {
                yield return new ValidationResult("FEIN is required.", [nameof(FEIN)]);
            }
            else if (!Regex.IsMatch(FEIN, @"^\d{2}-\d{7}$"))
            {
                yield return new ValidationResult("FEIN format is invalid (e.g. 12-3456789).", [nameof(FEIN)]);
            }
        }
    }
}
