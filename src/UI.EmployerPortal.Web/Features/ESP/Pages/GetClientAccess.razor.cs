using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.ESP.Services;

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
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly ClientAccessFormModel _form = new();
    private EditContext _editContext = default!;
    private readonly List<string> _ruleViolationMessages = [];
    private string? _successMessage = null;
    private bool _isLoading = false;

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_form);
    }

    private async Task HandleContinue()
    {
        _ruleViolationMessages.Clear();
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
        }
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("esp/account/clients");
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
