using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Layout;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

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
    private IEmployerAccessProvider EmployerAccessProvider { get; set; } = default!;

    [Inject]
    private IEmployerAccountService EmployerAccountService { get; set; } = default!;
    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private ILayoutOrchestator LayoutOrchestator { get; set; } = default!;

    private readonly AccessKeyFormModel _form = new();
    private EditContext _editContext = default!;
    private string? _serviceError = null;
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

    private async Task HandleValidSubmit()
    {
        _serviceError = null;
        _successMessage = null;
        _isLoading = true;
        var (success, error) = await AccountUserService.ActivateAccessKeyAsync(_form.AccessKey, _form.EmployerAccountNumber);


        if (success)
        {
            _successMessage = "Access key activated successfully.";
            _form.AccessKey = string.Empty;
            _form.EmployerAccountNumber = string.Empty;
            _editContext = new EditContext(_form);
            await EmployerAccessProvider.ResetEmployerAccess();
            await LayoutOrchestator.RequestEmployerListRefreshAsync("Employer access added");
        }
        else
        {
            _serviceError = error;
        }
        _isLoading = false;
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("landing-page");
    }

    private void OpenRequestAccessKeyModal()
    {
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
    private async Task HandleMailAccessKey(string employerIdentifier)
    {
        _mailKeyError = null;
        _isMailingKey = true;

        var (success, error) = await AccountUserService.MailAccessKeyAsync(employerIdentifier);

        _isMailingKey = false;

        if (success)
        {
            _isRequestKeyModalOpen = false;
            _serviceError = null;
            _successMessage = "Access key will be mailed";
        }
        else
        {
            _mailKeyError = string.IsNullOrWhiteSpace(error)
                ? "Unable to request an access key. Please try again."
                : error;
        }
    }

    private sealed class AccessKeyFormModel
    {
        [Required(ErrorMessage = "Access Key is required.")]
        public string AccessKey { get; set; } = string.Empty;

        [Required(ErrorMessage = "UI Account Number is required.")]
        [RegularExpression(@"^\d{6}-\d{3}-\d$", ErrorMessage = "UI Account Number is invalid")]
        public string EmployerAccountNumber { get; set; } = string.Empty;
    }
}
