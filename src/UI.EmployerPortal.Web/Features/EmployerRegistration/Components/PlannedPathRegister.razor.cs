using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
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
                _allEmployerRegisterAccount = await EmployerRegistrationService.GetRegisterEmployer(surveyResponseSk);
            }
            StateHasChanged();
        }
    }
}
