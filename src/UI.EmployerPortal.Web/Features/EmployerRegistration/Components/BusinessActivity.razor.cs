using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
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


    /// <summary>Tracks whether the form has been submitted at least once.</summary>
    private bool _formSubmitted = false;

    /// <summary>Tracks which fields have been interacted with so errors show on blur.</summary>

    private readonly HashSet<FieldIdentifier> _touchedFields = new();


    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

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

    private EditContext _editContext = default!;
    private bool _hasValidationErrors = false;

    /// <summary>
    /// Initialize fields on startup
    /// </summary>
    /// <returns></returns>
    protected override async void OnInitialized()
    {
        _editContext = new EditContext(Model);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            _hasValidationErrors = _editContext.GetValidationMessages().Any();
            StateHasChanged();
        };
        CheckConstructionWarning();
    }

    /// <summary>
    /// Handle principal activity selection change
    /// </summary>
    private void OnPrincipalActivityChanged()
    {
        Model.PrimaryBusinessActivityDescription = string.Empty;
        Model.WisconsinSpecificBusinessActivity = string.Empty;

        Model.SuppliesTemporaryWorkers = null;
        Model.ProvidesEmployeeLeasing = null;
        Model.EmployerServiceExplanation = string.Empty;

        Model.EmployeeType = string.Empty;
        Model.EmployeeCount = string.Empty;
        Model.ServicesDescription = string.Empty;

        InvokeAsync(StateHasChanged);
        CheckConstructionWarning();
        ValidateForm();
    }

    private void OnTempWorkersChanged(bool? value)
    {
        Model.SuppliesTemporaryWorkers = value;

        if (value == true || Model.ProvidesEmployeeLeasing == true)
        {
            Model.EmployerServiceExplanation = string.Empty;
        }
        OnFieldChanged("SuppliesTemporaryWorkers");
        ValidateForm();
    }

    private void OnEmployeeLeasingChanged(bool? value)
    {
        Model.ProvidesEmployeeLeasing = value;

        if (value == true || Model.SuppliesTemporaryWorkers == true)
        {
            Model.EmployerServiceExplanation = string.Empty;
        }
        OnFieldChanged("ProvidesEmployeeLeasing");
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
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> YesNoRadioOptions = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

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
                "WisconsinSpecificBusinessActivity",
                "SuppliesTemporaryWorkers",
                "ProvidesEmployeeLeasing",
                "EmployerServiceExplanantion",
                "EmployeeType",
                "ServicesDescription",
                "EmployeeCount",
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

    private void OnFieldBlur(Expression<Func<object?>> fieldExpression)

    {
        var fieldIdentifier = FieldIdentifier.Create(fieldExpression);
        _touchedFields.Add(fieldIdentifier);

        ValidateForm();
        StateHasChanged();
    }
    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
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
            "SuppliesTemporaryWorkers" => "tempWorkers",
            "ProvidesEmployeeLeasing" => "employeeLeasing",
            "EmployerServiceExplanantion" => "employeeExplanation",
            "EmployeeType" => "employeeType",
            "EmployeeCount" => "employeeCount",
            "ServicesDescription" => "servicesDescription",

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
                AddFieldError("DateBusinessStarted", "Date business started is not valid. Format example: mm/dd/yyyy");
            }
            else if (IsFutureDate(Model.DateBusinessStarted.Value))
            {
                AddFieldError("DateBusinessStarted", "Date business started must be today or earlier");
            }

            // Date First Paid Employees In WI
            if (!Model.DateFirstPaidEmployeesInWI.HasValue)
            {
                AddFieldError("DateFirstPaidEmployeesInWI", "Date you first had employees working in Wisconsin is not valid. Format example: mm/dd/yyyy");
            }
            else if (IsFutureDate(Model.DateFirstPaidEmployeesInWI.Value))
            {
                AddFieldError("DateFirstPaidEmployeesInWI", "Date you first had employees working in Wisconsin must be today or earlier");
            }

            // Date First Paid Wages In WI
            if (!Model.DateFirstPaidWagesInWI.HasValue)
            {
                AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin is not valid. Format example: mm/dd/yyyy");
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
                    AddFieldError("DateFirstPaidWagesInWI", "Date you first paid wages for work performed in Wisconsin must be on or after the date you first had employees working in Wisconsin.");
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

        if (Model.PrincipalBusinessActivity == PrincipalBusinessActivityType.EmployerServices)
        {
            if (!Model.SuppliesTemporaryWorkers.HasValue)
            {
                AddFieldError("SuppliesTemporaryWorkers", "The question must be answered to continue");
            }
            if (!Model.ProvidesEmployeeLeasing.HasValue)
            {
                AddFieldError("ProvidesEmployeeLeasing", "The question must be answered to continue");
            }
            if (Model.SuppliesTemporaryWorkers == false && Model.ProvidesEmployeeLeasing == false && string.IsNullOrWhiteSpace(Model.EmployerServiceExplanation))
            {
                AddFieldError("EmployerServiceExplanantion", "Explanation is required");
            }
        }

        if (Model.PrincipalBusinessActivity == PrincipalBusinessActivityType.EmployerServicesPayrollService)
        {
            if (string.IsNullOrWhiteSpace(Model.EmployeeType))
            {
                AddFieldError("EmployeeType", "The question must be answered to continue");
            }
            if (string.IsNullOrWhiteSpace(Model.EmployeeCount))
            {
                AddFieldError("EmployeeCount", "You must enter the number of employees performing services in Wisconsin");
            }
            if (string.IsNullOrWhiteSpace(Model.ServicesDescription))
            {
                AddFieldError("ServicesDescription", "The question must be answered to continue");
            }
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
        await InvokeAsync(StateHasChanged);
        return true;
    }

    private static bool IsFutureDate(DateTime date)
    {
        return date.Date > DateTime.Today;
    }

    private void HandleContinue()
    {
        _formSubmitted = true;
        _hasValidationErrors = false;
        StateHasChanged();
    }

    /// <summary>Called by EditForm when top-level validation fails.</summary>

    private void OnInvalid()
    {
        _formSubmitted = true;
        _hasValidationErrors = true;
        StateHasChanged();
    }


}
