using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;
/// <summary>
/// Page for entering an access key and employer account number to gain access to a shared employer account.
/// </summary>
public partial class GetAccountAccessPage
{
    [Inject]
    private IAccountUserService AccountUserService { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;
    private readonly AccessKeyFormModel _form = new();
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
        var (success, error) = await AccountUserService.ActivateAccessKeyAsync(_form.AccessKey, _form.EmployerAccountNumber);
        _isLoading = false;

        if (success)
        {
            await SessionManager.ClearAsync<SessionAllEmployerAccounts>();
            _successMessage = "Access key activated successfully. Please log out and log back in to load your updated employer accounts.";
            _form.AccessKey = string.Empty;
            _form.EmployerAccountNumber = string.Empty;
            _editContext = new EditContext(_form);
        }
        else
        {
            _serviceError = error;
        }
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("landing-page");
    }

    private sealed class AccessKeyFormModel
    {
        [Required(ErrorMessage = "Access Key is required.")]
        public string AccessKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "Account number is required.")]
        [RegularExpression(@"^\d{6}-\d{3}-\d$", ErrorMessage = "UI Account Number is invalid")]
        public string EmployerAccountNumber { get; set; } = string.Empty;
    }
}
