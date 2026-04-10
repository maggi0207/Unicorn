using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
/// BusinessActivity page component
/// </summary>
public partial class BusinessActivity : ComponentBase
{
    /// <summary>
    /// Business activity form model
    /// </summary>
    [Parameter] public BusinessActivityModel Model { get; set; } = new();

    private bool _showValidationSummary = false;
    private bool _isSessionLoaded = false;
    private bool _showConstructionWarning = false;
    private bool _showAllErrors = false;

    private List<string> ValidationErrors { get; set; } = [];
    private List<string> ValidationFieldIds { get; set; } = new();
    private HashSet<string> InvalidFields { get; set; } = [];
    private Dictionary<string, string> FieldErrors { get; set; } = [];
    private HashSet<string> TouchedFields { get; set; } = [];

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject] private ProtectedSessionStorage SessionStorage { get; set; } = default!;

    [Inject] private EmployerRegistrationModelStore ModelStore { get; set; } = default!;

    /// <summary>
    /// OnBackClicked
    /// </summary>
    [Parameter]
    public EventCallback OnBackClicked { get; set; }

    /// <summary>
    /// OnSaveAndQuitClicked
    /// </summary>
    [Parameter]
    public EventCallback OnSaveAndQuitClicked { get; set; }

    /// <summary>
    /// OnContinueClicked
    /// </summary>
    [Parameter]
    public EventCallback OnContinueClicked { get; set; }

    /// <summary>
    /// Load session data after first render
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            Model ??= ModelStore.EmployerRegistrationModel.BusinessActivityModel;
            //await LoadFromSession();
            _isSessionLoaded = true;
            StateHasChanged();
        }
    }

    /// <summary>
    /// Initialize fields on startup
    /// </summary>
    /// <returns></returns>
    protected override async void OnInitialized()
    {
        await ClearStoredData();
    }

    /// <summary>
    /// Load saved form data from session storage
    /// </summary>
    private async Task LoadFromSession()
    {
        try
        {
            var result = await SessionStorage.GetAsync<BusinessActivitySessionData>("BusinessActivityData");
            if (result.Success && result.Value != null)
            {
                var savedData = result.Value;
                Model.DateBusinessStarted = savedData.DateBusinessStarted;
                Model.DateFirstPaidEmployeesInWI = savedData.DateFirstPaidEmployeesInWI;
                Model.DateFirstPaidWagesInWI = savedData.DateFirstPaidWagesInWI;
                Model.PrincipalBusinessActivity = savedData.PrincipalBusinessActivity;
                Model.PrimaryBusinessActivityDescription = savedData.PrimaryBusinessActivityDescription;
                Model.SameAsPrimaryBusinessActivity = savedData.SameAsPrimaryBusinessActivity;
                Model.WisconsinSpecificBusinessActivity = savedData.WisconsinSpecificBusinessActivity;

                CheckConstructionWarning();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading from session: {ex.Message}");
        }
    }

    /// <summary>
    /// Save form data to session storage
    /// </summary>
    private async Task SaveToSession()
    {
        try
        {
            var sessionData = new BusinessActivitySessionData
            {
                DateBusinessStarted = Model.DateBusinessStarted,
                DateFirstPaidEmployeesInWI = Model.DateFirstPaidEmployeesInWI,
                DateFirstPaidWagesInWI = Model.DateFirstPaidWagesInWI,
                PrincipalBusinessActivity = Model.PrincipalBusinessActivity,
                PrimaryBusinessActivityDescription = Model.PrimaryBusinessActivityDescription,
                SameAsPrimaryBusinessActivity = Model.SameAsPrimaryBusinessActivity,
                WisconsinSpecificBusinessActivity = Model.WisconsinSpecificBusinessActivity
            };

            await SessionStorage.SetAsync("BusinessActivityData", sessionData);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving to session: {ex.Message}");
        }
    }

    /// <summary>
    /// Handle principal activity selection change
    /// </summary>
    private void OnPrincipalActivityChanged()
    {
        Model.PrimaryBusinessActivityDescription = string.Empty;
        Model.WisconsinSpecificBusinessActivity = string.Empty;

        InvokeAsync(StateHasChanged);
        CheckConstructionWarning();
        ValidateForm();
    }

    /// <summary>
    /// Check if construction warning should be displayed
    /// </summary>
    private void CheckConstructionWarning()
    {
        _showConstructionWarning = Model.PrincipalBusinessActivity.IsConstructionRelated();
    }

    /// <summary>
    /// IsFieldTouched
    /// </summary>
    /// <param name="fieldKey"></param>
    /// <returns></returns>
    private bool IsFieldTouched(string fieldKey)
    {
        return TouchedFields.Contains(fieldKey);
    }

    /// <summary>
    /// TouchAllFields
    /// </summary>
    private void TouchAllFields()
    {
        TouchedFields = [
                "DateBusinessStarted",
                "DateFirstPaidEmployeesInWI",
                "DateFirstPaidWagesInWI",
                "PrincipalBusinessActivity",
                "PrimaryBusinessActivityDescription",
                "WisconsinSpecificBusinessActivity"
            ];
    }
    /// <summary>
    /// OnFieldChanged
    /// </summary>
    /// <param name="fieldKey"></param>
    private void OnFieldChanged(string fieldKey)
    {
        TouchedFields.Add(fieldKey);
        ValidateForm();
    }

    private void OnFieldBlur(string fieldKey)
    {
        TouchedFields.Remove(fieldKey);
        ValidateForm();
    }

    private static string GetElementId(string fieldKey)
    {
        return fieldKey switch
        {
            "DateBusinessStarted" => "dateStarted",
            "DateFirstPaidEmployeesInWI" => "dateFirstPaid",
            "DateFirstPaidWagesInWI" => "dateFirstWages",
            "PrincipalBusinessActivity" => "principal-activity",
            "PrimaryBusinessActivityDescription" => "primaryDescription",
            "WisconsinSpecificBusinessActivity" => "wiDescription",
            _ => string.Empty
        };
    }

    private void AddFieldError(string fieldKey, string message)
    {
        ValidationErrors.Add(message);
        ValidationFieldIds.Add(GetElementId(fieldKey));
        InvalidFields.Add(fieldKey);
        if (!FieldErrors.ContainsKey(fieldKey))
        {
            FieldErrors[fieldKey] = message;
        }
    }

    private void ValidateForm()
    {
        ValidationErrors.Clear();
        ValidationFieldIds.Clear();
        InvalidFields.Clear();
        FieldErrors.Clear();

        if (ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.HavePaidEmployeesForWorkInWisconsin.HasValue
            && ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.HavePaidEmployeesForWorkInWisconsin.Value)
        {
            // Date Business Started
            if (!Model.DateBusinessStarted.HasValue)
            {
                AddFieldError("DateBusinessStarted", "Date business started or acquired is required");
            }
            else if (IsFutureDate(Model.DateBusinessStarted.Value))
            {
                AddFieldError("DateBusinessStarted", "Date business started must be today or earlier");
            }

            // Date First Paid Employees In WI
            if (!Model.DateFirstPaidEmployeesInWI.HasValue)
            {
                AddFieldError("DateFirstPaidEmployeesInWI", "Date you first had paid employees in WI is required");
            }
            else if (IsFutureDate(Model.DateFirstPaidEmployeesInWI.Value))
            {
                AddFieldError("DateFirstPaidEmployeesInWI", "Date you first had employees working in Wisconsin must be today or earlier");
            }

            // Date First Paid Wages In WI
            if (!Model.DateFirstPaidWagesInWI.HasValue)
            {
                AddFieldError("DateFirstPaidWagesInWI", "Date first paid wages for work performed in WI is required");
            }
            else
            {
                var wagesDate = Model.DateFirstPaidWagesInWI.Value.Date;

                //Must be between Date Business Started and today
                if (IsFutureDate(Model.DateFirstPaidWagesInWI.Value))
                {
                    AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin must be today or earlier");
                }

                if (Model.DateBusinessStarted.HasValue && wagesDate < Model.DateBusinessStarted.Value.Date)
                {
                    AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin must be between the date the business started and today OR you need to answer NO to Have you paid employees for work performed in Wisconsin on page 1");
                }
                else if (wagesDate > DateTime.Today)
                {
                    AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin must be between the date the business started and today");
                }

                if (!FieldErrors.ContainsKey("DateFirstPaidEmployeesInWI")
                    && Model.DateFirstPaidEmployeesInWI.HasValue
                    && wagesDate < Model.DateFirstPaidEmployeesInWI.Value.Date)
                {
                    AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin must be on or after the date you first had paid employees working in Wisconsin");
                }
            }
        }

        if (Model.PrincipalBusinessActivity == PrincipalBusinessActivityType.None)
        {
            AddFieldError("PrincipalBusinessActivity", "Principal Business Activity is required");
        }

        if (string.IsNullOrWhiteSpace(Model.PrimaryBusinessActivityDescription))
        {
            AddFieldError("PrimaryBusinessActivityDescription", "Primary Business Activity Description is required");
        }

        // Auto-copy primary description if checkbox is checked
        if (Model.SameAsPrimaryBusinessActivity)
        {
            Model.WisconsinSpecificBusinessActivity = Model.PrimaryBusinessActivityDescription;
        }

        StateHasChanged();
    }

    /// <summary>
    /// Called by wizard to trigger validation externally
    /// </summary>
    public async Task<bool> Validate()
    {
        _showValidationSummary = true;
        _showAllErrors = true;
        TouchAllFields();
        ValidateForm();

        if (ValidationErrors.Any())
        {
            await InvokeAsync(StateHasChanged);
            return false;
        }

        _showValidationSummary = false;
        await SaveToSession();
        await InvokeAsync(StateHasChanged);
        return true;
    }

    /// <summary>
    /// Clear business activity data from session (call after successful submission)
    /// </summary>
    public async Task ClearStoredData()
    {
        try
        {
            await SessionStorage.DeleteAsync("BusinessActivityData");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error clearing storage: {ex.Message}");
        }
    }

    private static bool IsFutureDate(DateTime date)
    {
        return date.Date > DateTime.Today;
    }
}
