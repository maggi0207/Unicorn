namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Navigation Step Numbers
/// </summary>
public enum RegistrationStep
{
    /// <summary>
    /// Prelininary Questions - Step 1
    /// </summary>
    PrelininaryQuestions = 1,
    /// <summary>
    /// Ownership - Step 2
    /// </summary>
    Ownership = 2,
    /// <summary>
    /// Business Information - Step 3
    /// </summary>
    BusinessInformation = 3,
    /// <summary>
    /// Business Contact - Step 4
    /// </summary>
    BusinessContact = 4,
    /// <summary>
    /// Business Activity - Step 5
    /// </summary>
    BusinessActivity = 5,
    /// <summary>
    /// Unemployment Insurance Subjectivity - Step 6
    /// </summary>
    UISubjectivity = 6

}


/// <summary>
/// 
/// </summary>
public partial class Verification
{
    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    public int CurrentStep { get; set; } = 0;

    /// <summary>
    /// 
    /// </summary>
    [Parameter]
    public EventCallback<int> OnNavigateToStep { get; set; }

    [Inject]
    private IEmployerRegistrationService EmployerRegistrationService { get; set; } = default!;

    [Inject]
    private EmployerRegistrationModelStore ModelStore { get; set; } = default!;

    private List<SurveyResponse> Responses { get; set; } = new();

    private List<EmployerRegistrationQuestionDataProxy> Questions { get; set; } = new();

    private List<SurveyContact> Contacts { get; set; } = new();

    private List<Tuple<RegistrationAddressCode, AddressModel>> Addresses { get; set; } = new();

    private async Task EditSection(RegistrationStep step)
    {
        await OnNavigateToStep.InvokeAsync((int) step);
    }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        LoadResponses();
        _ = GetQuestions();
    }
    /// <LoadResponses />
    public void LoadResponses()
    {
        Responses = ModelStore.GetResponses();
        Contacts = ModelStore.GetContacts();
        Addresses = ModelStore.GetAddresses();
    }

    private async Task GetQuestions()
    {
        try
        {
            Questions = (await EmployerRegistrationService.LoadAllEmployerRegistrationQuestionsAsync()).Questions.ToList();
            StateHasChanged();
        }
        catch { }
    }

    private string GetEditIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/edit-icon.svg";
        return icon;
    }
}
