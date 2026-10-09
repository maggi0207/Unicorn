using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Pages;

/// <summary>
/// Get Client Access — lets an ESP request worker access to a client by entering an
/// access key and the associated UI account number.
/// </summary>
public partial class GetClientAccess
{
    [Inject]
    private IEspAccountService EspAccountService { get; set; } = default!;

    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly ClientAccessFormModel _form = new();
    private EditContext _editContext = default!;
    private readonly List<string> _ruleViolationMessages = [];
    private bool _showValidation;
    private bool _showValidationSummary;
    private List<string> _validationErrors = [];
    private readonly List<string> _validationFieldIds = [];
    private string? _successMessage = null;
    private bool _isLoading = false;
    private bool _isRequestKeyModalOpen = false;
    private bool _isMailingKey = false;
    private string? _mailKeyError = null;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_form);
    }

    private async Task HandleContinue()
    {
        if (_isLoading)
        {
            return;
        }
        _showValidation = true;
        _showValidationSummary = false;
        _ruleViolationMessages.Clear();
        _validationFieldIds.Clear();
        _successMessage = null;
        _isLoading = true;

        var (success, ruleViolations) = await EspAccountService.AddClientAsync(_form.AccessKey, _form.UIAccountNumber);

        _isLoading = false;

        if (success)
        {
            _successMessage = $"You have been granted access to {_form.UIAccountNumber}.";
            _form.AccessKey = string.Empty;
            _form.UIAccountNumber = string.Empty;
            _editContext = new EditContext(_form);
        }
        else
        {
            _ruleViolationMessages.AddRange(ruleViolations);
            foreach (var message in _ruleViolationMessages)
            {
                _validationFieldIds.Add(GetFieldIdForMessage(message));
            }
        }
    }
    private void HandleFormInvalid()
    {
        _showValidation = true;
        _showValidationSummary = true;
        _validationFieldIds.Clear();
        _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();

        foreach (var message in _validationErrors)
        {
            _validationFieldIds.Add(GetFieldIdForMessage(message));
        }
    }

    private static string GetFieldIdForMessage(string message)
    {
        return message switch
        {
            var m when m.Contains("Access Key", StringComparison.OrdinalIgnoreCase) => "gca-key",
            var m when m.Contains("Account Number", StringComparison.OrdinalIgnoreCase) => "gca-account",
            _ => string.Empty
        };
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("esp/account/clients");
    }

    private void OpenRequestAccessKeyModal()
    {
        _showValidationSummary = false;
        _mailKeyError = null;
        _isRequestKeyModalOpen = true;
    }

    private void CloseRequestAccessKeyModal()
    {
        _isRequestKeyModalOpen = false;
        _mailKeyError = null;
    }

    private void ClearMailKeyError()
    {
        _mailKeyError = null;
    }

    /// <summary>
    /// Requests that an access key be mailed to the client employer's main mailing address.
    /// The UCT-16429E letter service is shared with the employer flow — there is no
    /// ESP-specific operation — and it identifies the employer by UI Account Number or FEIN.
    /// </summary>
    private async Task HandleMailAccessKey(string employerIdentifier)
    {
        _mailKeyError = null;
        _isMailingKey = true;

        var (success, error) = await AccountUserService.MailAccessKeyAsync(employerIdentifier);

        _isMailingKey = false;

        if (success)
        {
            _isRequestKeyModalOpen = false;
            _ruleViolationMessages.Clear();
            _successMessage = "Access key will be mailed";
        }
        else
        {
            _mailKeyError = string.IsNullOrWhiteSpace(error)
                ? "Unable to request an access key. Please try again."
                : error;
        }
    }

    private sealed class ClientAccessFormModel
    {
        [Required(ErrorMessage = "Access Key is required.")]
        public string AccessKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "UI Account Number is required.")]
        [RegularExpression(@"^\d{6}-\d{3}-\d$", ErrorMessage = "UI Account Number is invalid")]
        public string UIAccountNumber { get; set; } = string.Empty;
    }
}
