using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// ManageEspAccess
/// </summary>
public partial class ManageEspAccess
{
    [Inject]
    private IEmployerServiceProviderAccessService EmployerServiceProviderAccessService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    private List<EmployerServiceProviderAccessModel> _espList = [];
    private bool _isLoading = true;
    private string? _loadError;

    private int _employerSK = 0;

    private bool _showRemoveModal = false;
    private bool _isRemoving = false;
    private EmployerServiceProviderAccessModel? _espToRemove = null;

    private bool _showSuccessBanner = false;
    private string _removedEspName = string.Empty;

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadEspAccessAsync();
    }

    private async Task LoadEspAccessAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;

            if (_employerSK == 0)
            {
                _loadError = "Unable to determine the current employer account.";
                return;
            }

            _espList = await EmployerServiceProviderAccessService.GetEspAccessListAsync(_employerSK);
        }
        catch
        {
            _loadError = "Something went wrong loading employer service provider access. Please try again.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void OpenRemoveModal(EmployerServiceProviderAccessModel esp)
    {
        _espToRemove = esp;
        _showRemoveModal = true;
        _showSuccessBanner = false;
    }

    private void CancelRemove()
    {
        if (_isRemoving)
        {
            return;
        }
        _showRemoveModal = false;
        _espToRemove = null;
    }

    private async Task ConfirmRemove()
    {
        if (_espToRemove == null || _employerSK == 0)
        {
            return;
        }

        _isRemoving = true;

        var (success, message) = await EmployerServiceProviderAccessService.RemoveEspAccessAsync(
            _employerSK, _espToRemove.EmployerServiceProviderSK);

        _isRemoving = false;

        if (success)
        {
            _removedEspName = _espToRemove.BusinessName;
            _showSuccessBanner = true;
            _showRemoveModal = false;
            _espToRemove = null;
            await LoadEspAccessAsync();
        }
        else
        {
            _loadError = string.IsNullOrWhiteSpace(message) ? "Unable to remove ESP access." : message;
            _showRemoveModal = false;
        }
    }
}
