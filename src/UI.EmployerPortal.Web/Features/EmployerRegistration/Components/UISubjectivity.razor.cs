namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Validation;
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

    private bool _isFormValid = true;
    private bool _formSubmitted = false;
    private bool _showAddressErrors = false;
    private bool _insufficientQuarterlyWageEntered = false;
    private int DecimalPlaces { get; set; } = 2;

    private readonly DateTime? _dateFirstPaidWages = DateTime.Parse("7/1/2023");  //TODO: This value is passed in from Step 5 

    private EditContext _subjectivityContext = default!;

    private CustomValidator? _customValidator;

    /// <summary>
    /// True when BusinessCategory was pre-selected as NonProfit_501c3 from Step 1.
    /// Causes the radio group to render as a disabled locked option instead of interactive.
    /// </summary>
    private bool _businessCategoryLockedFromStep1 = false;

    /// <summary>
    /// 
    /// </summary>
    public required SubjectivityModel SubjectivityModel { get; set; }

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
    /// 
    /// </summary>
    public static readonly List<SelectOption> ExpectedWagesCatagories = new()
    {
      new SelectOption() {  Text = "Within 30 Days", Value = "Within 30 Days"},
      new SelectOption() {  Text = "30 to 90 days", Value = "30 to 90 days"},
      new SelectOption() {  Text = "6 months", Value = "6 months"},
      new SelectOption() {  Text = "One year", Value = "One year"},
      new SelectOption() {  Text = "More than a year", Value = "More than a year"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> ExpectToPayWagesInAQuarter = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasAppliedFor501c3Status = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasEmployeesOutsideWisconsin = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasHasFutaLiabilityInOtherStates = new[]
{
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };


    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasTaxLiabilityOutsideWisconsin = new[]
{
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasPaidWagesOver1500Employees = new[]
{
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasPaidWagesOrTaxesOver1500 = new[]
{
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> HasPaidHasEmployeeIn20Weeks = new[]
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
        var wages = PaidWagesService.GetYearsAndQuartersPaidWages(_dateFirstPaidWages);

        SubjectivityModel = new SubjectivityModel() { Wages = wages };

        _subjectivityContext = new EditContext(SubjectivityModel);

        var preliminaryCategory = ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.BusinessCategory;

        if (preliminaryCategory == BusinessCategory.NonProfit_501c3)
        {
            _businessCategoryLockedFromStep1 = true;
            SubjectivityModel.BusinessCategory = BusinessCategory.NonProfit_501c3;
            BusinessCategory = BusinessCategory.NonProfit_501c3;
        }

        _subjectivityContext.OnFieldChanged += (_, __) =>
        {
            _isFormValid = _subjectivityContext.Validate();
            StateHasChanged();
        };

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

        return BusinessCategory switch
        {
            BusinessCategory.Commercial => commercial,
            BusinessCategory.Domestic => domestic,
            BusinessCategory.Agricultural => String.Empty,
            BusinessCategory.NonProfit_501c3 => commercial,
            BusinessCategory.NonProfit_Other => nonProfit,
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
        }
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
        });
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
            SubjectivityModel.FirstWageQuarterYearEmployees = null;
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
            SubjectivityModel.ExpectToPayWagesInAQuarter = null;
            SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
        }
        await InvokeAsync(() =>
        {
            // CHANGED: validate again after clearing hidden fields**
            _subjectivityContext.Validate();
        });

    }


    private async Task OnHasFutaLiabilityInOtherStatesChange(bool? value)
    {
        SubjectivityModel.HasFutaLiabilityInOtherStates = value;
        PaidWagesService.Update(value ?? false);
        // CHANGED: clear dependent fields**
        if (value == true)
        {
            SubjectivityModel.PaidWagesOrTaxesOver1500 = null;
            SubjectivityModel.QuarterYearFirstPaidTaxes = null;
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
        }
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
        });
    }

    private async Task OnHasFutaLiabilityInOtherStatesOutsideWisconsinChange(bool? value)
    {
        await InvokeAsync(() =>
        {
            SubjectivityModel.HasHasFutaLiabilityInOtherStatesOutsideWisconsin = value;

            _subjectivityContext.NotifyValidationStateChanged();

        });
    }

    private async Task OnPaidWagesOver1500EmployeesChange(bool? value)
    {
        SubjectivityModel.PaidWagesOver1500Employees = value;
        if (value == false)
        {
            SubjectivityModel.FirstWageQuarterYearEmployees = null;
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
        }
        else if (value == true)
        {
            // CHANGED: if user selects Yes, downstream No-branch fields should be cleared**
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
        }
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
        });
    }


    private async Task OnPaidWagesOrTaxesOver1500Change(bool? value)
    {
        SubjectivityModel.PaidWagesOrTaxesOver1500 = value;
        // CHANGED: clear dependent fields**
        if (value == false)
        {
            SubjectivityModel.QuarterYearFirstPaidTaxes = null;
        }
        else if (value == true)
        {
            SubjectivityModel.HasEmployeeIn20Weeks = null;
            SubjectivityModel.Week20EndDate = null;
        }
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
        });
    }

    private async Task OnHasEmployeeIn20WeeksChange(bool? value)
    {
        SubjectivityModel.HasEmployeeIn20Weeks = value;
        // CHANGED**
        if (value == false)
        {
            SubjectivityModel.Week20EndDate = null;
        }
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
        });
    }

    private async Task OnEmployee20WeeksFourOrMoreChange(bool? value)
    {
        await InvokeAsync(() =>
        {
            SubjectivityModel.Employee20WeeksFourOrMore = value;

            _subjectivityContext.NotifyValidationStateChanged();

        });
    }


    private async Task OnWeek20EndDateChange(DateTime? value)
    {
        await InvokeAsync(() =>
        {
            SubjectivityModel.Week20EndDate = value;

            _subjectivityContext.NotifyValidationStateChanged();

        });
    }

    private async Task OnBusinessCatagoryChanged(BusinessCategory? value)
    {
        SubjectivityModel.BusinessCategory = value;
        if (value.HasValue)
        {
            BusinessCategory = value.Value;
        }
        // CHANGED: reset flow-dependent answers**
        SubjectivityModel.HasAppliedFor501c3Status = null;
        SubjectivityModel.HasEmployeesOutsideWisconsin = null;
        SubjectivityModel.HasFutaLiabilityInOtherStates = null;
        SubjectivityModel.PaidWagesOver1500Employees = null;
        SubjectivityModel.PaidWagesOrTaxesOver1500 = null;
        SubjectivityModel.FirstWageQuarterYearEmployees = null;
        SubjectivityModel.QuarterYearFirstPaidTaxes = null;
        SubjectivityModel.HasEmployeeIn20Weeks = null;
        SubjectivityModel.Week20EndDate = null;
        SubjectivityModel.ExpectToPayWagesInAQuarter = null;
        SubjectivityModel.WhenExpectToPayWagesInAQuarter = string.Empty;
        await InvokeAsync(() =>
        {
            // CHANGED**
            _subjectivityContext.Validate();
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

    private bool ShowQuarterYearFirstPaid()
    {
        return SubjectivityModel.BusinessCategory == BusinessCategory.NonProfit_501c3;
    }

    /// <summary>
    /// Called by wizard to  trigger validation externally
    /// </summary>
    public async Task<bool> Validate()
    {
        try
        {
            _insufficientQuarterlyWageEntered = !(PaidWagesService.PaidWagesMeetsQuarterlyMinimum(
                SubjectivityModel.BusinessCategory ?? BusinessCategory.Commercial, SubjectivityModel.Wages));

            _formSubmitted = true;
            _showAddressErrors = false;
            _customValidator?.ClearErrors();

            var validationErrors = ValidateModels();

            if (!_isFormValid || validationErrors.Any() || _insufficientQuarterlyWageEntered)
            {
                _customValidator?.DisplayErrors(validationErrors);
                StateHasChanged();
                return false;
            }

            await InvokeAsync(StateHasChanged);
            return _isFormValid;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"UISUbjectivity.Validate crashed :{ex.Message}");
            Console.WriteLine(ex.StackTrace);
            return false;
        }
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


    private Dictionary<FieldIdentifier, List<string>> ValidateModels()
    {
        var subjectivityErrors = ValidateSubjectivityModel();
        var addressErrors = ValidateAddressModel();
        _showAddressErrors = addressErrors.Any();


        return subjectivityErrors.Concat(addressErrors.Where(kvp =>
        {
            return !subjectivityErrors.ContainsKey(kvp.Key);
        }))
                   .ToDictionary(kvp =>
                   {
                       return kvp.Key;
                   }, kvp =>
                   {
                       return kvp.Value;
                   });

    }

    private Dictionary<FieldIdentifier, List<string>> ValidateSubjectivityModel()
    {
        var errors = new Dictionary<FieldIdentifier, List<string>>();
        var results = new List<ValidationResult>();

        var addressCtx = new ValidationContext(SubjectivityModel);

        Validator.TryValidateObject(SubjectivityModel, addressCtx, results, true);

        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                var fi = new FieldIdentifier(SubjectivityModel, memberName);
                if (!errors.ContainsKey(fi))
                {
                    errors[fi] = new List<string>();
                }

                errors[fi].Add(result.ErrorMessage ?? "This field is invalid.");
            }
        }

        return errors;
    }

    private Dictionary<FieldIdentifier, List<string>> ValidateAddressModel()
    {
        var errors = new Dictionary<FieldIdentifier, List<string>>();
        var results = new List<ValidationResult>();

        var addressCtx = new ValidationContext(SubjectivityModel.FinancialInstitution);

        Validator.TryValidateObject(SubjectivityModel.FinancialInstitution, addressCtx, results, true);

        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                var fi = new FieldIdentifier(SubjectivityModel.FinancialInstitution, memberName);
                if (!errors.ContainsKey(fi))
                {
                    errors[fi] = new List<string>();
                }

                errors[fi].Add(result.ErrorMessage ?? "This field is invalid.");
            }
        }

        return errors;
    }

    private MarkupString GetMasterAlertErrorIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/master-alert-error.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private bool Section1Visible()
    {
        return false;
    }
    private bool Section2Visible()
    {
        return false;
    }
    private bool Section3Visible()
    {
        return BusinessCategory != BusinessCategory.Unknown;
    }
    private bool Section4Visible()
    {
        return (SubjectivityModel.HasFutaLiabilityInOtherStates.HasValue && !SubjectivityModel.HasFutaLiabilityInOtherStates.Value) ||
            (SubjectivityModel.HasEmployeesOutsideWisconsin == false && BusinessCategory != BusinessCategory.NonProfit_501c3);

    }
    private bool Section5Visible()
    {
        return SubjectivityModel.PaidWagesOver1500Employees ?? false;
    }
    private bool Section6Visible()
    {
        return BusinessCategory != BusinessCategory.NonProfit_501c3 &&
            SubjectivityModel.HasEmployeesOutsideWisconsin.HasValue && SubjectivityModel.HasEmployeesOutsideWisconsin.Value;
    }
    private bool Section7Visible()
    {
        return false;
    }
    private bool Section8Visible()
    {
        return false;
    }
    private bool Section9Visible()
    {
        return (SubjectivityModel.HasFutaLiabilityInOtherStates.HasValue && !SubjectivityModel.HasFutaLiabilityInOtherStates.Value) ||
            (SubjectivityModel.HasEmployeesOutsideWisconsin == false) &&
            (BusinessCategory == BusinessCategory.Commercial || BusinessCategory == BusinessCategory.NonProfit_Other);
        ;
    }
    private bool Section10Visible()
    {
        return (SubjectivityModel.HasEmployeeIn20Weeks ?? false) &&
            (BusinessCategory == BusinessCategory.Commercial || BusinessCategory == BusinessCategory.NonProfit_Other);
    }

    private bool Section11Visible()
    {
        return BusinessCategory == BusinessCategory.NonProfit_501c3;
    }

    private bool Section12Visible()
    {
        return BusinessCategory != BusinessCategory.NonProfit_501c3 &&
            SubjectivityModel.HasEmployeesOutsideWisconsin.HasValue && SubjectivityModel.HasEmployeesOutsideWisconsin.Value;
    }

    private bool Section13Visible()
    {
#pragma warning disable IDE0046 // Convert to conditional expression

        if (BusinessCategory == BusinessCategory.Commercial)
        {
            return SubjectivityModel.PaidWagesOver1500Employees == false && SubjectivityModel.HasEmployeeIn20Weeks == false;
        }

        if (BusinessCategory == BusinessCategory.Domestic)
        {
            return SubjectivityModel.PaidWagesOver1500Employees == false;
        }

        if (BusinessCategory == BusinessCategory.Agricultural)
        {
            return SubjectivityModel.PaidWagesOver1500Employees == false && SubjectivityModel.HasEmployeeIn20Weeks == false;
        }

        if (BusinessCategory != BusinessCategory.NonProfit_501c3)
        {
            return false;
        }

        if (BusinessCategory == BusinessCategory.NonProfit_Other)
        {
            return SubjectivityModel.PaidWagesOver1500Employees == false && SubjectivityModel.HasEmployeeIn20Weeks == false;
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        return false;

    }

    private bool Section14Visible()
    {
        return SubjectivityModel.ExpectToPayWagesInAQuarter ?? false;
    }
}


