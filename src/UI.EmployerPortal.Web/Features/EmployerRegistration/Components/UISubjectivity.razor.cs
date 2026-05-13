namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
/// <summary>
/// 
/// </summary>
public partial class UISubjectivity
{
    [Inject]
    private IYearQuarterPaidWagesService PaidWagesService { get; set; } = default!;
    [Inject]
    private EmployerRegistrationModelStore ModelStore { get; set; } = default!;

    private bool _formSubmitted = false;
    private bool _showAddressErrors = false;
    private bool _insufficientQuarterlyWageEntered = false;
    private bool _wageCheckflag = false;
    private int DecimalPlaces { get; set; } = 2;
    private DateTime? _dateFirstPaidWages;
    private EditContext _subjectivityContext = default!;
    //private CustomValidator? _customValidator;
    private ValidationMessageStore _messageStore = default!;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private List<string> ValidationErrors { get; set; } = new();
    private List<string> ValidationFieldIds { get; set; } = new();
    private bool IsVisible<T>(Expression<Func<T>> fieldExpression)
    {
        var field = FieldIdentifier.Create(fieldExpression);
        return _formSubmitted || _touchedFields.Contains(field);
    }
    /// <summary>
    /// Describes which of the 5 scenarios applies, driving what options are locked/disabled/selectable in Step 6.
    /// </summary>
    private enum BusinessCategoryScenario
    {
        /// <summary>Step 1 = Yes (NonProfit 501c3). Locked: NonProfit_501c3. Disabled: Commercial, NonProfit_Other.</summary>
        LockedNonProfit501c3,
        /// <summary>Step 1 = No, Step 5 = Agricultural. Locked: Agricultural. Disabled: Commercial, NonProfit_Other.</summary>
        LockedAgricultural,
        /// <summary>Step 1 = No, Step 5 = Domestic. Locked: Domestic. Disabled: Commercial, NonProfit_Other.</summary>
        LockedDomestic,
        /// <summary>Step 1 = No, Step 5 = Construction. Locked: Commercial. Disabled: NonProfit_Other.</summary>
        LockedCommercial,
        /// <summary>Step 1 = No, Step 5 = Other. Both Commercial and NonProfit_Other are selectable. Commercial pre-selected.</summary>
        FreeChoice,
    }

    /// <summary>
    /// The resolved scenario computed from Step 1 and Step 5 during OnInitialized.
    /// </summary>
    private BusinessCategoryScenario _scenario = BusinessCategoryScenario.FreeChoice;

    /// <summary>
    /// The model passed from the steps component to be used here
    /// </summary>
    [Parameter]
    public required SubjectivityModel SubjectivityModel { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public BusinessCategory BusinessCategory { get; set; } = BusinessCategory.Unknown;


    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<BusinessCategory?>> BusinessCatagories = new[]
    {
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Commercial, Label = "Commercial"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Domestic, Label = "Domestic(in a private home)"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Agricultural, Label = "Agricultural(Farming)"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.NonProfit_501c3, Label = "Non-Profit with 501(c) (3) Ruling from IRS"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.NonProfit_Other, Label = "Non-Profit(other)"}

    };

    /// <summary>
    /// Alternative selectable options shown below the locked disabled Non-Profit 501(c)(3) radio
    /// when the business category was pre-populated from Step 1. Only Commercial and Non-Profit (other)
    /// are offered as corrections — Domestic and Agricultural are not valid alternatives for a
    /// previously declared 501(c)(3) organisation.
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<BusinessCategory?>> BusinessCategoriesLockedAlternatives = new[]
    {
        new RadioOption<BusinessCategory?> { Value = BusinessCategory.Commercial,      Label = "Commercial" },
        new RadioOption<BusinessCategory?> { Value = BusinessCategory.NonProfit_Other, Label = "Non-Profit (other)" },
    };
    /// <summary>
    /// 
    /// </summary>
    public static readonly List<SelectOption> ExpectedWagesCatagories = new()
    {
      new SelectOption() { Text = "Within 30 Days", Value = "1" },
      new SelectOption() { Text = "30 to 90 days",  Value = "2" },
      new SelectOption() { Text = "6 months",        Value = "3" },
      new SelectOption() { Text = "One year",        Value = "4" },
      new SelectOption() { Text = "More than a year",Value = "5" }
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> YesNoRadioOptions = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public class RegistrationModel
    {
        /// <summary>
        /// 
        /// </summary>
        public bool? HasRegistrationNumber { get; set; } = null;
    }

    /// <summary>
    /// 
    /// </summary>
    protected override void OnInitialized()
    {
        _dateFirstPaidWages = ModelStore.EmployerRegistrationModel.BusinessActivityModel.DateFirstPaidWagesInWI;
        var wages = PaidWagesService.GetYearsAndQuartersPaidWages(_dateFirstPaidWages);
        // SubjectivityModel = new SubjectivityModel() { Wages = wages };
        if (SubjectivityModel.BusinessCategory == null)
        {
            SubjectivityModel.Wages = wages;
        }

        _subjectivityContext = new EditContext(SubjectivityModel);
        _messageStore = new ValidationMessageStore(_subjectivityContext);
        var isNonProfitFromStep1 = ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.IsNonProfitOrg == true;
        var step5Activity = ModelStore.EmployerRegistrationModel.BusinessActivityModel.PrincipalBusinessActivity;

        _scenario = ResolveScenario(isNonProfitFromStep1, step5Activity);

        // Pre-set the locked/default category so downstream visibility methods work correctly
        var lockedCategory = _scenario switch
        {
            BusinessCategoryScenario.LockedNonProfit501c3 => BusinessCategory.NonProfit_501c3,
            BusinessCategoryScenario.LockedAgricultural => BusinessCategory.Agricultural,
            BusinessCategoryScenario.LockedDomestic => BusinessCategory.Domestic,
            BusinessCategoryScenario.LockedCommercial => BusinessCategory.Commercial,
            BusinessCategoryScenario.FreeChoice => BusinessCategory.Commercial,
            _ => BusinessCategory.Commercial,
        };
        if (SubjectivityModel.BusinessCategory != null && SubjectivityModel.BusinessCategory != lockedCategory)
        {
            //reset the fields
            SubjectivityModel.HasEmployeesOutsideWisconsin501 = null;
            SubjectivityModel.HasEmployeesOutsideWisconsin = null;
            SubjectivityModel.HasFutaLiabilityInOtherStates = null;
            SubjectivityModel.PaidWagesOver1500Employees = null;
            //Wages
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Employee20WeeksFourOrMore = null;
            SubjectivityModel.Week20EndDate = null;
            SubjectivityModel.ExpectToPayWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
            SubjectivityModel.ExpectToHaveWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToHaveWagesInAQuarter = string.Empty;
            SubjectivityModel.PayWagesPerformWI = null;
            SubjectivityModel.ExpectToPayWagesPerformWI = null;
            SubjectivityModel.Wages = wages;
            ResetField(() => SubjectivityModel.HasEmployeesOutsideWisconsin501);
            ResetField(() => SubjectivityModel.HasEmployeesOutsideWisconsin);
            ResetField(() => SubjectivityModel.HasFutaLiabilityInOtherStates);
            ResetField(() => SubjectivityModel.PaidWagesOver1500Employees);
            ResetField(() => SubjectivityModel.Employee20WeeksFourOrMore);
            ResetField(() => SubjectivityModel.Week20EndDate);
            ResetField(() => SubjectivityModel.ExpectToPayWagesInAQuarter);
            ResetField(() => SubjectivityModel.WhenExpectToPayWagesInAQuarter);
            ResetField(() => SubjectivityModel.ExpectToHaveWagesInAQuarter);
            ResetField(() => SubjectivityModel.WhenExpectToHaveWagesInAQuarter);
            ResetField(() => SubjectivityModel.PayWagesPerformWI);
            ResetField(() => SubjectivityModel.ExpectToPayWagesPerformWI);
        }

        SubjectivityModel.BusinessCategory = lockedCategory;
        BusinessCategory = lockedCategory;
        _subjectivityContext.OnFieldChanged += (_, f) =>
        {
            _touchedFields.Add(f.FieldIdentifier);
            RunValidation(f.FieldIdentifier);

            _subjectivityContext.NotifyValidationStateChanged();
        };
        _subjectivityContext.OnValidationRequested += (_, __) =>
        {
            RunValidation();
        };

    }
    /// <summary>
    /// Determines the business category scenario for Step 6 based on Step 1 (non-profit answer)
    /// and Step 5 (principal business activity selection).
    /// </summary>
    private static BusinessCategoryScenario ResolveScenario(bool isNonProfitFromStep1, PrincipalBusinessActivityType activity)
    {
        if (isNonProfitFromStep1)
        {
            return BusinessCategoryScenario.LockedNonProfit501c3;
        }

        var isAgricultural = activity is
            PrincipalBusinessActivityType.AgricultureFarming or
            PrincipalBusinessActivityType.AgricultureRaisingCropsFood or
            PrincipalBusinessActivityType.AgricultureRaisingLivestock;

        if (isAgricultural)
        {
            return BusinessCategoryScenario.LockedAgricultural;
        }

        var isDomestic = activity is
            PrincipalBusinessActivityType.DomesticEmployNannyOrBabysitter or
            PrincipalBusinessActivityType.DomesticRecipientOfHomeHelp or
            PrincipalBusinessActivityType.DomesticRecipientOfInHomeHealthcare or
            PrincipalBusinessActivityType.DomesticFiscalAgentElectingToBeEmployer;

        if (isDomestic)
        {
            return BusinessCategoryScenario.LockedDomestic;
        }

        var isConstruction = activity is
            PrincipalBusinessActivityType.ConstructionSpecialtyTrades or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeRelated or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesCarpentry or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesConcrete or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesEarthMoving or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesElectricians or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesHardwoodFlooring or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesIronWork or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesPainters or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesPlumbers or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRemodelingRepairAdditions or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoadWork or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesRoofing or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesSiding or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradesUtilityConstruction or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeElectronicsInstallations or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeFlooringExceptHardwood or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeHeatingAndCooling or
            PrincipalBusinessActivityType.ConstructionSpecialtyTradeWhitewashing;

        return isConstruction ? BusinessCategoryScenario.LockedCommercial : BusinessCategoryScenario.FreeChoice;
    }

    private string GetPermittedExclusionsUrl()
    {
        return "https://dwd.wisconsin.gov/ui201/t2201.htm#exclude";
    }

    private string GetNoteText()
    {
        return String.Concat("You do not have to pay UI taxes on certain persons. ",
            "When answering the following questions, do not include weeks of employment or wages paid ",
            "to persons from the list of ");
    }

    private string GetWagesQuestion()
    {
        var commercial = "Have you paid $1,500 or more in wages in a calendar quarter?";
        var domestic = "Have you paid $1,000 or more in cash wages in a calendar quarter?";
        var agricultural = "Have you paid $20,000 or more in cash wages for agricultural labor in a calendar quarter?";
        var nonProfit = "Have you paid $1,500 or more in wages in a calendar quarter?";

        return BusinessCategory switch
        {
            BusinessCategory.Commercial => commercial,
            BusinessCategory.Domestic => domestic,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => commercial,
            BusinessCategory.NonProfit_Other => nonProfit,
            _ => commercial
        };
    }

    private string GetExpectToPayWagesQuestion()
    {
        var commercial = "Do you expect to pay $1,500 or more in wages in a calendar quarter?";
        var domestic = "Do you expect to pay $1,000 or more in cash wages in a calendar quarter?";
        var nonProfit = "Do you expect to pay $1,500 or more in wages in a calendar quarter?";
        var agricultural = "Do you expect to pay $20,000 or more in cash wages for agricultural labor in a calendar quarter?";
        return BusinessCategory switch
        {
            BusinessCategory.Commercial => commercial,
            BusinessCategory.Domestic => domestic,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => string.Empty,
            BusinessCategory.NonProfit_Other => nonProfit,
            _ => String.Empty
        };
    }
    private string GetExpectToHaveWagesQuestion()
    {
        var commercial = "Do you expect to have at least 1 full- or part-time employee working for you in 20 different calendar weeks in a calendar year?";
        //var domestic = "Do you expect to pay $1,000 or more in cash wages in a calendar quarter?";
        var nonProfit_501c3 = "Do you expect to have at least 4 full- or part-time employees working for you on the same day in 20 different calendar weeks in a calendar year?";
        var agricultural = "Do you expect to have at least 10 full- or part-time agricultural employees working for you on the same day in 20 different calendar weeks in a calendar year?";
        return BusinessCategory switch
        {
            BusinessCategory.Commercial => commercial,
            BusinessCategory.Domestic => string.Empty,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => nonProfit_501c3,
            BusinessCategory.NonProfit_Other => string.Empty,
            _ => String.Empty
        };
    }
    private string EmployeeIn20Weeks()
    {
        var commercial = "Have you had at least one full- or part-time employee working for you in 20 different calendar weeks in a calendar year?";
        var agricultural = "Have you had at least 10 full- or part-time agricultural employees working for you on the same day in 20 different calendar weeks in a calendar year?";
        return BusinessCategory switch
        {
            BusinessCategory.Commercial => commercial,
            BusinessCategory.Domestic => String.Empty,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => commercial,
            BusinessCategory.NonProfit_Other => String.Empty,
            _ => String.Empty
        };
    }
    private string PaidWagesPerformWI()
    {
        var agricultural = "Have you paid agricultural wages for work performed in Wisconsin?";
        var domestic = "Have you paid domestic wages for work performed in Wisconsin?";
        return BusinessCategory switch
        {
            BusinessCategory.Commercial => String.Empty,
            BusinessCategory.Domestic => domestic,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => String.Empty,
            BusinessCategory.NonProfit_Other => String.Empty,
            _ => String.Empty
        };
    }
    private string ExpectPayWagesPerformWI()
    {
        var agricultural = "Do you expect to pay agricultural wages for work in Wisconsin?";
        var domestic = " Do you expect to pay domestic wages for work performed in Wisconsin?";
        return BusinessCategory switch
        {
            BusinessCategory.Commercial => String.Empty,
            BusinessCategory.Domestic => domestic,
            BusinessCategory.Agricultural => agricultural,
            BusinessCategory.NonProfit_501c3 => String.Empty,
            BusinessCategory.NonProfit_Other => String.Empty,
            _ => String.Empty
        };
    }

    private async Task OnExpectToPayWagesInAQuarterChange(bool? value)
    {
        SubjectivityModel.ExpectToPayWagesInAQuarter = value;
        // CHANGED**
        if (value == false)
        {
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
            ResetField(() => SubjectivityModel.WhenExpectToPayWagesInAQuarter);
        }
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.ExpectToPayWagesInAQuarter)));
    }
    private async Task OnExpectToHaveWagesInAQuarterChange(bool? value)
    {
        SubjectivityModel.ExpectToHaveWagesInAQuarter = value;
        if (value == false)
        {
            SubjectivityModel.WhenExpectToHaveWagesInAQuarter = string.Empty;
            ResetField(() => SubjectivityModel.WhenExpectToHaveWagesInAQuarter);
        }
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.ExpectToHaveWagesInAQuarter)));
    }


    private async Task OnHasEmployeesOutsideWisconsinChange(bool? value)

    {
        SubjectivityModel.HasEmployeesOutsideWisconsin = value;
        // CHANGED: clear dependent fields when branch changes**
        if (value == false)
        {
            SubjectivityModel.HasFutaLiabilityInOtherStates = null;
            SubjectivityModel.PaidWagesOrTaxesOver1500 = null;
            SubjectivityModel.QuarterYearFirstPaidTaxes = null;
        }
        else if (value == true)
        {
            SubjectivityModel.PaidWagesOver1500Employees = null;
            //SubjectivityModel.FirstWageQuarterYearEmployees = null;
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
            SubjectivityModel.ExpectToPayWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
            SubjectivityModel.AX_10_IN_20_FLG = null;
            SubjectivityModel.ExpectToHaveWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToHaveWagesInAQuarter = string.Empty;
            ResetField(() => SubjectivityModel.WhenExpectToHaveWagesInAQuarter);
        }
        ResetField(() => SubjectivityModel.HasEmployeesOutsideWisconsin);
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeesOutsideWisconsin)));

    }
    private async Task OnHasEmployeesOutsideWisconsinChange501(bool? value)
    {
        SubjectivityModel.HasEmployeesOutsideWisconsin501 = value;
        ResetField(() => SubjectivityModel.HasEmployeesOutsideWisconsin501);
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeesOutsideWisconsin501)));
    }

    private async Task OnHasFutaLiabilityInOtherStatesChange(bool? value)
    {
        SubjectivityModel.HasFutaLiabilityInOtherStates = value;
        PaidWagesService.Update(value ?? false);
        // CHANGED: clear dependent fields**
        if (value == true)
        {
            SubjectivityModel.PaidWagesOrTaxesOver1500 = null;
            ResetField(() => SubjectivityModel.PaidWagesOrTaxesOver1500);
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
            SubjectivityModel.ExpectToPayWagesInAQuarter = null;
            SubjectivityModel.PaidWagesOver1500Employees = null;
            ResetField(() => SubjectivityModel.WhenExpectToPayWagesInAQuarter);
            ResetField(() => SubjectivityModel.ExpectToPayWagesInAQuarter);
            ResetField(() => SubjectivityModel.PaidWagesOver1500Employees);
            // SubjectivityModel.Wages.Clear();               

        }
        else
        {
            SubjectivityModel.PayWagesPerformWI = null;
            SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT = string.Empty;
            SubjectivityModel.ExpectToPayWagesPerformWI = null;
            ResetField(() => SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT);
            ResetField(() => SubjectivityModel.PayWagesPerformWI);
            ResetField(() => SubjectivityModel.ExpectToPayWagesPerformWI);
        }

        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeesOutsideWisconsin501)));
    }

    private async Task OnPaidWagesOver1500EmployeesChange(bool? value)
    {
        SubjectivityModel.PaidWagesOver1500Employees = value;
        if (value == false)
        {
            foreach (var item in SubjectivityModel.Wages)
            {
                item.Q1Wages = null;
                item.Q2Wages = null;
                item.Q3Wages = null;
                item.Q4Wages = null;
            }

        }
        else
        {
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
            SubjectivityModel.ExpectToPayWagesInAQuarter = null;
            ResetField(() => SubjectivityModel.WhenExpectToPayWagesInAQuarter);
            ResetField(() => SubjectivityModel.ExpectToPayWagesInAQuarter);
        }
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.PaidWagesOver1500Employees)));
    }

    private async Task OnHasEmployeeIn20WeeksChange(bool? value)
    {
        SubjectivityModel.HasEmployeeIn20Weeks = value;
        // CHANGED**
        if (value == false)
        {
            SubjectivityModel.Week20EndDate = null;
            ResetField(() => SubjectivityModel.Week20EndDate);
        }
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeeIn20Weeks)));
    }

    private async Task OnEmployee20WeeksFourOrMoreChange(bool? value)
    {
        SubjectivityModel.Employee20WeeksFourOrMore = value;
        if (value != true)
        {
            SubjectivityModel.Week20EndDate = null;
            ResetField(() => SubjectivityModel.Week20EndDate);
        }
        else
        {
            SubjectivityModel.ExpectToHaveWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToHaveWagesInAQuarter = string.Empty;
            ResetField(() => SubjectivityModel.ExpectToHaveWagesInAQuarter);
            ResetField(() => SubjectivityModel.WhenExpectToHaveWagesInAQuarter);
        }

        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.Employee20WeeksFourOrMore)));
    }
    private async Task OnHasPayWagesPerformWI(bool? value)
    {

        SubjectivityModel.PayWagesPerformWI = value;
        if (value == true)
        {
            SubjectivityModel.ExpectToPayWagesPerformWI = null;
            ResetField(() => SubjectivityModel.ExpectToPayWagesPerformWI);
        }
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.PayWagesPerformWI)));
    }
    private async Task OnExpectToPayWagesPerformWI(bool? value)
    {

        SubjectivityModel.ExpectToPayWagesPerformWI = value;
        if (value != true)
        {
            SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT = string.Empty;
            ResetField(() => SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT);
        }

        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.ExpectToPayWagesPerformWI)));
    }
    private async Task OnBusinessCatagoryChanged(BusinessCategory? value)
    {

        SubjectivityModel.BusinessCategory = value;
        if (value.HasValue)
        {
            BusinessCategory = value.Value;
        }
        // Reset all flow-dependent answers so downstream sections start clean.
        SubjectivityModel.HasAppliedFor501c3Status = null;
        SubjectivityModel.HasEmployeesOutsideWisconsin = null;
        SubjectivityModel.HasFutaLiabilityInOtherStates = null;
        SubjectivityModel.PaidWagesOver1500Employees = null;
        SubjectivityModel.PaidWagesOrTaxesOver1500 = null;
        //SubjectivityModel.FirstWageQuarterYearEmployees = null;
        SubjectivityModel.QuarterYearFirstPaidTaxes = null;
        SubjectivityModel.HasEmployeeIn20Weeks = null;
        SubjectivityModel.Week20EndDate = null;
        SubjectivityModel.ExpectToPayWagesInAQuarter = null;
        SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
        SubjectivityModel.ExpectToHaveWagesInAQuarter = null;
        SubjectivityModel.WhenExpectToHaveWagesInAQuarter = string.Empty;
        // Notify the EditContext so OnFieldChanged fires → StateHasChanged() → UI re-renders.
        _subjectivityContext.NotifyFieldChanged(_subjectivityContext.Field(nameof(SubjectivityModel.BusinessCategory)));

        await InvokeAsync(() =>
        {
            _subjectivityContext.Validate();
            StateHasChanged();
        });
    }

    private async Task OnHasAppliedFor501c3StatusChange(bool? value)
    {
        await InvokeAsync(() =>
        {
            SubjectivityModel.HasAppliedFor501c3Status = value;

            _subjectivityContext.NotifyValidationStateChanged();

        });
    }

    private void RunValidation(FieldIdentifier? changedField = null)
    {
        if (changedField.HasValue)
        {
            // Clear only that field’s errors
            _messageStore.Clear(changedField.Value);
        }
        else
        {
            // Full validation (on submit)
            _messageStore.Clear();
        }

        ValidateModel();
        _subjectivityContext.NotifyValidationStateChanged();
    }
    /// <summary>
    /// Called by wizard to  trigger validation externally
    /// </summary>
    public async Task<bool> Validate()
    {
        _formSubmitted = true;
        _messageStore.Clear();
        RunValidation();
        _subjectivityContext.NotifyValidationStateChanged();
        StateHasChanged();
        return !_subjectivityContext.GetValidationMessages().Any();
    }
    private void ResetField<T>(Expression<Func<T>> fieldExpression)
    {
        var field = FieldIdentifier.Create(fieldExpression);
        _messageStore.Clear(field);
        _touchedFields.Remove(field);
    }

    /// <summary>
    /// Called by wizard to  trigger validation externally
    /// </summary>
    private void ValidateModel()
    {
        _messageStore.Clear();
        ValidationErrors.Clear();
        ValidationFieldIds.Clear();
        _insufficientQuarterlyWageEntered = Section5Visible && !(PaidWagesService.PaidWagesMeetsQuarterlyMinimum(BusinessCategory, SubjectivityModel.Wages));
        _wageCheckflag = Section5Visible && ValidateWages(SubjectivityModel.Wages);
        _showAddressErrors = true;
        // Block navigation when the user must correct their Step 1 answer before proceeding.
        // 501(c)(3) sub-tree validation
        if (IsVisible(() => SubjectivityModel.HasEmployeesOutsideWisconsin501) && Section0Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeesOutsideWisconsin501));
            if (!SubjectivityModel.HasEmployeesOutsideWisconsin501.HasValue)
            {
                _messageStore.Add(field, "Select if you have employees who work in states other than Wisconsin");
            }
        }
        if (IsVisible(() => SubjectivityModel.HasEmployeesOutsideWisconsin) && Section3Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeesOutsideWisconsin));
            if (!SubjectivityModel.HasEmployeesOutsideWisconsin.HasValue)
            {
                _messageStore.Add(field, "Select employees who work in states other than Wisconsin");
            }
        }
        if (SubjectivityModel.HasAppliedFor501c3Status == true && Section2Visible())
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.HasAppliedFor501c3Status));
            if (SubjectivityModel.HasAppliedFor501c3Status == true)
            {
                _messageStore.Add(field, "Please return to step one to correct your answer regarding your 501(c)(3) designation.");
            }
        }
        if (IsVisible(() => SubjectivityModel.HasFutaLiabilityInOtherStates) && Section6Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.HasFutaLiabilityInOtherStates));
            if (!SubjectivityModel.HasFutaLiabilityInOtherStates.HasValue)
            {
                _messageStore.Add(field, "Federal Unemployment Tax (FUTA) liability is required");
            }
        }
        if (IsVisible(() => SubjectivityModel.PaidWagesOver1500Employees) && Section4Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.PaidWagesOver1500Employees));
            if (!SubjectivityModel.PaidWagesOver1500Employees.HasValue)
            {
                _messageStore.Add(field, "Wages in a calendar quarter is required");
            }
        }
        if (IsVisible(() => SubjectivityModel.HasEmployeeIn20Weeks) && Section9Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.HasEmployeeIn20Weeks));
            if (!SubjectivityModel.HasEmployeeIn20Weeks.HasValue)
            {
                _messageStore.Add(field, "Select if you had employees");
            }
        }
        if (IsVisible(() => SubjectivityModel.Employee20WeeksFourOrMore) && Section11Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.Employee20WeeksFourOrMore));
            if (!SubjectivityModel.Employee20WeeksFourOrMore.HasValue)
            {
                _messageStore.Add(field, "Select if you have had employees working for you in Wisconsin for 20 different calendar weeks");
            }
        }
        if (IsVisible(() => SubjectivityModel.Week20EndDate) && Section10Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.Week20EndDate));
            if (!SubjectivityModel.Week20EndDate.HasValue)
            {
                _messageStore.Add(field, "Enter Week End Date?");
            }
        }
        if (IsVisible(() => SubjectivityModel.ExpectToHaveWagesInAQuarter) && Section18Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.ExpectToHaveWagesInAQuarter));
            if (!SubjectivityModel.ExpectToHaveWagesInAQuarter.HasValue)
            {
                _messageStore.Add(field, "Select if you expect to have employees working for you in Wisconsin for 20 different calendar weeks");
            }
        }
        if (IsVisible(() => SubjectivityModel.WhenExpectToHaveWagesInAQuarter) && Section19Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.WhenExpectToHaveWagesInAQuarter));
            if (string.IsNullOrEmpty(SubjectivityModel.WhenExpectToHaveWagesInAQuarter))
            {
                _messageStore.Add(field, "Select When to have 4 part or full time employees working for you?");
            }
        }


        if (IsVisible(() => SubjectivityModel.PayWagesPerformWI) && Section15Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.PayWagesPerformWI));
            if (!SubjectivityModel.PayWagesPerformWI.HasValue)
            {
                _messageStore.Add(field, "Please Pay wages for work performed in Wisconsin?");
            }
        }
        if (IsVisible(() => SubjectivityModel.ExpectToPayWagesPerformWI) && Section16Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.ExpectToPayWagesPerformWI));
            if (!SubjectivityModel.ExpectToPayWagesPerformWI.HasValue)
            {
                _messageStore.Add(field, "Select When to expect to pay");
            }
        }
        if (IsVisible(() => SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT) && Section17Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT));
            if (string.IsNullOrEmpty(SubjectivityModel.AFL_XPCT_PY_WI_WGS_WHN_TXT))
            {
                _messageStore.Add(field, "Select the period value");
            }
        }
        if (IsVisible(() => SubjectivityModel.ExpectToPayWagesInAQuarter) && Section13Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.ExpectToPayWagesInAQuarter));
            if (!SubjectivityModel.ExpectToPayWagesInAQuarter.HasValue)
            {
                _messageStore.Add(field, "Select to pay wages");
            }
        }
        if (IsVisible(() => SubjectivityModel.WhenExpectToPayWagesInAQuarter) && Section14Visible)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.WhenExpectToPayWagesInAQuarter));
            if (string.IsNullOrEmpty(SubjectivityModel.WhenExpectToPayWagesInAQuarter))
            {
                _messageStore.Add(field, "Select When to pay wages");
            }
        }
        if (_insufficientQuarterlyWageEntered)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.PaidWagesOver1500Employees));
            _messageStore.Add(field, "Insufficient quarterly wages reported");
        }
        if (_wageCheckflag)
        {
            var field = _subjectivityContext.Field(nameof(SubjectivityModel.PaidWagesOver1500Employees));
            _messageStore.Add(field, "Wages Can not be blank Or First Paid Quater Wage must be greater than Zero.");
        }
        //Bind Notification Banner
        var properties = SubjectivityModel.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var fieldId = new FieldIdentifier(SubjectivityModel, prop.Name);
            var errors = _subjectivityContext.GetValidationMessages(fieldId);

            foreach (var error in errors)
            {
                ValidationErrors.Add(error);
                ValidationFieldIds.Add(prop.Name);
            }
        }
        _subjectivityContext.NotifyValidationStateChanged();
    }
    /// <summary>
    /// 
    /// </summary>   
    /// <param name="paidWages"></param>
    /// <returns></returns>
    public bool ValidateWages(IEnumerable<YearQuartersPaidWages> paidWages)
    {

        var orderedWages = paidWages.OrderBy(w =>
        {
            return w.Year;
        }).ToList();

        if (orderedWages != null && orderedWages.Count > 0)
        {
            _wageCheckflag = orderedWages.Any(o =>
            {
                return (!o.Q1Disabled && o.Q1Wages == null) ||
                                                 (!o.Q2Disabled && o.Q2Wages == null) ||
                                                 (!o.Q3Disabled && o.Q3Wages == null) ||
                                                 (!o.Q4Disabled && o.Q4Wages == null);
            });
            var yearOne = orderedWages[0];
            //if (yearOne.Q1Wages.HasValue && yearOne.Q1Wages.Value <= 0 && yearOne.Q1Disabled ==false)
            //{
            //    _wageCheckflag = true;
            //}

            var yearonecheck = new[]
              {
                    (Disabled:yearOne.Q1Disabled,wage:yearOne.Q1Wages),
                    (Disabled:yearOne.Q2Disabled,wage:yearOne.Q2Wages),
                    (Disabled:yearOne.Q3Disabled,wage:yearOne.Q3Wages),
                    (Disabled:yearOne.Q4Disabled,wage:yearOne.Q4Wages),
                };
#pragma warning disable IDE0042 // Deconstruct variable declaration
            foreach (var o in yearonecheck)
            {
                if (!o.Disabled)
                {
                    if (o.wage.HasValue && o.wage <= 0)
                    {
                        _wageCheckflag = true;
                    }
                    break;//First Quarter Check
                }

            }
#pragma warning restore IDE0042 // Deconstruct variable declaration
        }
        return _wageCheckflag;
    }
    /// <summary>
    /// Returns the disclaimer paragraph shown above the quarterly wages table.
    /// Text varies by business category; the wages table is hidden entirely for NonProfit_501c3.
    /// </summary>
    private MarkupString GetWagesTableDisclaimer()
    {
        var exclusionsUrl = GetPermittedExclusionsUrl();

        var permittedExclusionsLink = $"<a href='{exclusionsUrl}' target='_blank' rel='noopener noreferrer' class='link-underline-primary'>Permitted Exclusions</a>";
        var excludedEmploymentLink = $"<a href='{exclusionsUrl}' target='_blank' rel='noopener noreferrer' class='link-underline-primary'>excluded employment</a>";
        var excludedEmployeesLink = $"<a href='{exclusionsUrl}' target='_blank' rel='noopener noreferrer' class='link-underline-primary'>excluded employees</a>";

        return BusinessCategory switch
        {
            BusinessCategory.Commercial or BusinessCategory.NonProfit_Other =>
                new MarkupString(
                    "Enter your gross quarterly payrolls below. Include all wages paid through the date that you complete this report. " +
                    "Do not estimate the amount of wages you expect to pay in the future. Show wages paid only for work performed solely or primarily in Wisconsin. " +
                   $"<strong>Do not enter the wages of Wisconsin residents who work entirely outside of Wisconsin. " +
                   $"Do not include wages paid to persons with {permittedExclusionsLink}.</strong>"),

            BusinessCategory.Domestic =>
                new MarkupString(
                    "Please provide the following quarterly payroll totals reflecting only cash wages paid for domestic employment in Wisconsin through the current date. " +
                   $"<strong>Do not include wages paid for {excludedEmploymentLink}.</strong>"),

            BusinessCategory.Agricultural =>
                new MarkupString(
                    "Complete the following record of your quarterly agricultural payroll in Wisconsin. Show gross cash wages paid in each calendar quarter. " +
                   $"<strong>Do not include wages for {excludedEmployeesLink}.</strong>"),

            _ => new MarkupString(string.Empty)
        };
    }


    private bool Section2Visible()
    {
        // Regular flow: NonProfit_Other is selected.
        if (BusinessCategory == BusinessCategory.NonProfit_Other)
        {
            return true;
        }

        // Locked flow: Step 1 pre-set 501(c)(3) but the user is now switching to a different
        // category. Show the question so they can confirm they have not applied for 501(c)(3)
        // status — if they have, they must return to Step 1 to correct their answer.
        return _scenario == BusinessCategoryScenario.LockedNonProfit501c3
            && BusinessCategory != BusinessCategory.NonProfit_501c3
            && BusinessCategory != BusinessCategory.Unknown;
    }
    //Visibulity Defined
    private bool Section3Visible => BusinessCategory != BusinessCategory.NonProfit_501c3;
    private bool Section10Visible => SubjectivityModel.HasEmployeeIn20Weeks ?? false || SubjectivityModel.Employee20WeeksFourOrMore == true;
    private bool Section6Visible => BusinessCategory != BusinessCategory.NonProfit_501c3 && SubjectivityModel.HasEmployeesOutsideWisconsin == true;
    private bool Section15Visible => SubjectivityModel.HasFutaLiabilityInOtherStates == true && (BusinessCategory == BusinessCategory.Agricultural || BusinessCategory == BusinessCategory.Domestic);
    private bool Section16Visible => SubjectivityModel.PayWagesPerformWI == false;
    private bool Section17Visible => SubjectivityModel.ExpectToPayWagesPerformWI == true;
    private bool Section14Visible => SubjectivityModel.ExpectToPayWagesInAQuarter == true;
    private bool Section5Visible => SubjectivityModel.PaidWagesOver1500Employees == true;
    private bool Section13Visible => BusinessCategory != BusinessCategory.NonProfit_501c3 && SubjectivityModel.PaidWagesOver1500Employees == false;
    private bool Section9Visible => SubjectivityModel.PaidWagesOver1500Employees.HasValue && (BusinessCategory == BusinessCategory.Commercial || BusinessCategory == BusinessCategory.Agricultural);


    //Non Profit with 501c3
    private bool Section0Visible => BusinessCategory == BusinessCategory.NonProfit_501c3;
    private bool Section11Visible => BusinessCategory == BusinessCategory.NonProfit_501c3;
    private bool Section18Visible => (SubjectivityModel.Employee20WeeksFourOrMore == false || SubjectivityModel.ExpectToPayWagesInAQuarter == false) && BusinessCategory != BusinessCategory.Domestic;
    private bool Section19Visible => SubjectivityModel.ExpectToHaveWagesInAQuarter == true;

    //Domestic


    private bool Section4Visible => (SubjectivityModel.HasFutaLiabilityInOtherStates == false || SubjectivityModel.HasEmployeesOutsideWisconsin == false) && BusinessCategory != BusinessCategory.NonProfit_501c3;

}
