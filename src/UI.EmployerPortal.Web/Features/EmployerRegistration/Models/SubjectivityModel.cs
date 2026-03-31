using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Model for the UISubjectivity page
/// </summary>
public class SubjectivityModel
{
    /// <summary>
    /// Business Catagory
    /// </summary>
    public BusinessCategory? BusinessCategory { get; set; }

    /// <summary>
    /// HasAppliedFor501c3Status
    /// </summary>
    [Required(ErrorMessage = "Applied for 501(c)(3) status is required.")]
    public bool? HasAppliedFor501c3Status { get; set; }

    /// <summary>
    /// Do you have employees who work in states other than Wisconsin?
    /// </summary>
    [Required(ErrorMessage = "Employees who work in other states is required")]
    public bool? EmployeesInOtherStates { get; set; }

    /// <summary>
    /// Do you have a Federal Unemployment Tax (FUTA) liability based on payrolls in any state other than Wisconsin?
    /// </summary>
    [Required(ErrorMessage = "Federal Unemployment Tax (FUTA) liability is required")]
    public bool? FederalTaxLiability { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [RequiredIfVisible("EmployeesInOtherStates", false, ErrorMessage = "Wages in a calendar quarter is required")]
    public bool? PaidWagesInAQuarterEmployees { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [RequiredIfVisible("FederalTaxLiability", false, ErrorMessage = "Wages in a calendar quarter is required")]
    public bool? PaidWagesInAQuarterTaxes { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [RegularExpression(@"^[1-4]\/\d{4}$", ErrorMessage = "Quarter and Year must be in the format q/yyyy")]
    public string? QuarterYearFirstPaidEmployees { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [Required]
    [RegularExpression(@"^[1-4]\/\d{4}$", ErrorMessage = "Quarter and Year must be in the format q/yyyy")]
    public string? QuarterYearFirstPaidTaxes { get; set; }

    /// <summary>
    /// 
    /// </summary>
    [RequiredIfVisible("FederalTaxLiability", false, ErrorMessage = "Employee question is required")]
    public bool? Employee20Weeks { get; set; }

    /// <summary>
    /// Only Saturday is valid
    /// </summary>
    [Required(ErrorMessage = "A Saturday date must be selected")]
    [SaturdayOnly(ErrorMessage = "The selected date must be a Saturday.")]
    public DateTime? DateWeek20Ended { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool? Employee20WeeksFourOrMore { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool? HasFederalTaxLiabilityOutsideWisconsin { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public AddressModel FinancialInstitution { get; set; } = new();

    /// <summary>
    /// 
    /// </summary>
    public bool? ExpectToPayWagesInAQuarter { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string WhenExpectToPayWagesInAQuarter { get; set; } = String.Empty;

    /// <summary>
    /// 
    /// </summary>
    public required List<YearQuartersPaidWages> Wages { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public SubjectivityModel() { }

}

/// <summary>
/// Determines if the date is a Saturday
/// </summary>
public class SaturdayOnlyAttribute : ValidationAttribute
{

    /// <summary>
    /// Only Valid if the Date is a Saturday
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is DateTime dateTime
            ? dateTime.DayOfWeek == DayOfWeek.Saturday
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "The selected date must be a Saturday.", new[] { validationContext.MemberName! })
            : ValidationResult.Success;
    }
}

/// <summary>
/// The 'Name' field may not be visible on the form.  In that event,
/// the value is not required.
/// </summary>
public class RequiredIfVisibleAttribute : ValidationAttribute
{
    private readonly string _isVisible;
    private readonly bool _expectedValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isVisible"></param>
    /// <param name="expectedValue"></param>
    public RequiredIfVisibleAttribute(string isVisible, bool expectedValue)
    {
        _isVisible = isVisible;
        _expectedValue = expectedValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var isValidationRequired = instance.GetType().GetProperty(_isVisible)?.GetValue(instance, null);

        var isRequired = isValidationRequired is not bool || (bool) isValidationRequired;

        if (isRequired == _expectedValue)
        {
            var name = value == null ? String.Empty : value.ToString();
            return !String.IsNullOrWhiteSpace(name)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Name is required ", new[] { context.MemberName! });
        }

        return ValidationResult.Success;
    }
}

