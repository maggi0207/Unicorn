using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// 
/// </summary>
public partial class FinanceMethodCR
{
    private FinanceMethodPrecheck? _response = default!;

    private string Message { get; set; } = default!;

    private bool? _isSaveSuccess = null;

    private bool _isSaving = false;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IEmployerAccountService EmployerAccountService { get; set; } = default!;

    [Inject]
    private IFinanceMethodCRService FinanceMethodCRService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IAsyncRetryPolicy<EmployerAccountService> RetryPolicy { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    private int Year { get; set; } = default;

    private List<SelectOption>? SelectOption { get; set; }

    private string Title { get; set; } = string.Empty;
    private int SelectedYear { get; set; } = default;
    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    protected override async Task OnAuthorizedInitAsync()
    {
        Year = DateTime.Now.Year + 1;
        SelectedYear = Year;
        SelectOption = new List<SelectOption>() { new() { Text = Year.ToString(), Value = Year.ToString() } };
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

        _response = await RetryPolicy.ExecuteAsync(() =>
       {
           return FinanceMethodCRService.GetFinanceMethodData(employerSk, secureUserSK);
       });


    }

    private void OnEffectiveYearChanged(string year)
    {
        SelectedYear = Convert.ToInt32(year);
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
            var res = await FinanceMethodCRService.Save(employerSk, secureUserSK, SelectedYear);
            _isSaveSuccess = res != null;
            Title = $"Thank you for submitting your request for tax financing to be effective 01/01/{SelectedYear}.";
            Message = $"The department will review your request and a written decision will be mailed to you by March 1 of {SelectedYear}.";
        }
        catch
        {
            _isSaveSuccess = false;
        }

        _isSaving = false;
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-summary");
    }
}
