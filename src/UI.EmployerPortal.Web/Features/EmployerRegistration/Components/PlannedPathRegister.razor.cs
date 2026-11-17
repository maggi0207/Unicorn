using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;
namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
/// <summary>
/// Calling Wcf Service for EmployerRegistration
/// </summary>
public partial class PlannedPathRegister
{
    [Inject]
    private IEmployerRegistrationServices EmployerRegistrationService { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    private List<RegisterEmployer> _allEmployerRegisterAccount = [];

    [Inject]
    private EmployerRegistrationModelStore ModelStore { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    /// <summary>
    /// Call Service
    /// </summary>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            if (int.TryParse(ModelStore.EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSk))
            {
                var plannedPathResponse = await SessionManager.GetAsync<SessionEmployerRegistrationResponse>();
                if (plannedPathResponse != null && plannedPathResponse.UIAccountNumber != null)
                {
                    _allEmployerRegisterAccount = new List<RegisterEmployer>();
                    _allEmployerRegisterAccount.Add(new RegisterEmployer()
                    {
                        SurveyNumber = plannedPathResponse.SurveyNumber,
                        UIAccountNumber = Regex.Replace(plannedPathResponse.UIAccountNumber, @"^(\d{6})(\d{3})(\d{1})$", "$1-$2-$3")
                    });
                }
                else
                {
                    _allEmployerRegisterAccount = await EmployerRegistrationService.GetRegisterEmployer(surveyResponseSk);
                }
            }
            StateHasChanged();
        }
    }
}
