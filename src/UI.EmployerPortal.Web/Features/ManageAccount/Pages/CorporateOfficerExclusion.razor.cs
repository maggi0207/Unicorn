using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;
/// <summary>
///
/// </summary>
public partial class CorporateOfficerExclusion
{

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private ICorporateOfficerExclusionService CorporateOfficerExclusionService { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IAsyncRetryPolicy<CorporateOfficerExclusionService> RetryPolicy { get; set; } = default!;

    private int _employerSK = 0;

    private COEResonableResponse _response = default!;

    private COESaveResponse? _saveResponse = default!;

    private string Message { get; set; } = default!;

    private bool? _isSaveSuccess = null;
    private bool _isRecalculating = false;
    private string? _recalculateStatus;

    private bool _isSaving = false;

    private bool _isCalculateClicked = false;
    /// <summary>
    ///
    /// </summary>
    /// <param name="sessionManager"></param>
    public CorporateOfficerExclusion(ISessionManager sessionManager)
    {
        SessionManager = sessionManager;
    }

    /// <summary>
    /// After rendering, set focus on the h1 heading if this is the save success page contents
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_saveResponse != null && _isSaveSuccess.HasValue && _isSaveSuccess.Value)
        {
            await JSRuntime.InvokeVoidAsync("focusElement", "save-heading");
        }
    }

    private async Task LoadData()
    {
        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;
            if (_employerSK == 0)
            {
                return;
            }

            var secureUserSK = UserAccountService.GetUserSKClaim();

            _response = await CorporateOfficerExclusionService.GetData(_employerSK, DateTime.Now.Year, secureUserSK);

        }
        catch
        {
        }
        finally
        {
        }
    }
    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadData();
    }

    private async Task HandleRecalculateAsync()
    {
        _recalculateStatus = "Recalculating";
        _isRecalculating = true;
        _isSaveSuccess = null;
        _saveResponse = null;
        var selectedClient = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

        if (selectedClient == null)
        {
            return;
        }

        var employerSk = selectedClient.Id;

        if (employerSk == 0)
        {
            return;
        }

        var secureUserSK = UserAccountService.GetUserSKClaim();

        try
        {
            var request = new Generated.ServiceClients.AccountSummaryService.COEReasonablenessRequest()
            {
                EmployerSK = employerSk,
                SecureUserSK = secureUserSK,
                year = DateTime.Now.Year,
                PayrollSubjectToFUTAWithoutCOE = _response.FUTAPayroll,
                PayrollSubjectToWITaxRateWithCOE = _response.WITaxablePayrollWithCOE,
                PayrollSubjectToWITaxRateWithoutCOE = _response.WITaxablePayrollWithoutCOE,
            };
            _isCalculateClicked = true;
            _response = await CorporateOfficerExclusionService.Recalculate(request);
        }
        catch
        {
        }

        _isRecalculating = false;
        //Short delay to allow browser to register updated ststus message for accessibility
        await Task.Delay(500);
        _recalculateStatus = "Recalculate complete";
        StateHasChanged();
        _recalculateStatus = "";
        StateHasChanged();
    }


    private async Task HandleSaveAsync()
    {
        _isSaving = true;

        var selectedClient = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

        if (selectedClient == null)
        {
            return;
        }

        var employerSk = selectedClient.Id;

        if (employerSk == 0)
        {
            return;
        }

        var secureUserSK = UserAccountService.GetUserSKClaim();

        try
        {
            var request = new CorporateOfficerExclusionRequest()
            {
                EmployerSK = employerSk,
                SecureUserSK = secureUserSK,
                NetSavings = _response.NetSavings,
                Year = DateTime.Now.Year,
                IsCalculateClicked = _isCalculateClicked
            };

            _saveResponse = await CorporateOfficerExclusionService.Save(request);
            _isSaveSuccess = _saveResponse?.Success;
        }
        catch
        {
            _isSaveSuccess = false;
        }

        _isSaving = false;
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-summary", true);
    }

    private void HandleASBack()
    {
        Navigation.NavigateTo("manage-account/account-details");
    }

    private void OnFUTAPayrollChanged(decimal? value) => _response.FUTAPayroll = value ?? 0.0m;

    private void OnWITaxablePayrollWithoutCOEChanged(decimal? value) => _response.WITaxablePayrollWithoutCOE = value ?? 0.0m;

    private void OnWITaxablePayrollWithCOEChanged(decimal? value) => _response.WITaxablePayrollWithCOE = value ?? 0.0m;

}
