using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Used by the UISubjectivity to indicate which year and quarter wages need to be entered.
/// </summary>
public class YearQuartersPaidWages
{
    /// <summary>
    /// 
    /// </summary>
    public string? Year { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool Q1Disabled { get; set; } = true;
    /// <summary>
    /// 
    /// </summary>
    public bool Q2Disabled { get; set; } = true;
    /// <summary>
    /// 
    /// </summary>
    public bool Q3Disabled { get; set; } = true;
    /// <summary>
    /// 
    /// </summary>
    public bool Q4Disabled { get; set; } = true;

    /// <summary>
    /// 
    /// </summary>
    [RequiredIfTrue("WageEntryRequired", "Q1Disabled", ErrorMessage = "Wage entry is required")]
    public decimal? Q1Wages { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [RequiredIfTrue("WageEntryRequired", "Q2Disabled", ErrorMessage = "Wage entry is required")]
    public decimal? Q2Wages { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [RequiredIfTrue("WageEntryRequired", "Q3Disabled", ErrorMessage = "Wage entry is required")]
    public decimal? Q3Wages { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [RequiredIfTrue("WageEntryRequired", "Q4Disabled", ErrorMessage = "Wage entry is required")]
    public decimal? Q4Wages { get; set; }

    /// <summary>
    /// If there is a FUTA liability then a value greater than zero is required
    /// </summary>
    public bool WageEntryRequired { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public YearQuartersPaidWages(bool wageEntryRequired = false)
    {
        WageEntryRequired = wageEntryRequired;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    public YearQuartersPaidWages(string year, string quarter)
    {
        Year = year;
        SetQuarter(quarter);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="quarter"></param>
    public void SetQuarter(string? quarter)
    {
        if (!string.IsNullOrEmpty(quarter))
        {
            if (quarter.Equals("Q1"))
            {
                Q1Disabled = false;
            }

            if (quarter.Equals("Q2"))
            {
                Q2Disabled = false;
            }

            if (quarter.Equals("Q3"))
            {
                Q3Disabled = false;
            }

            if (quarter.Equals("Q4"))
            {
                Q4Disabled = false;
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public int GetYear()
    {
        return UInt32.TryParse(Year, out var year) ? (int) year : default;
    }
}

/// <summary>
/// 
/// </summary>
public class RequiredIfTrueAttribute : ValidationAttribute
{
    private readonly string _requiredIfTrue;
    private readonly string _isDisabled;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="requiredIfTrue"></param>
    /// <param name="isDisabled"></param>
    public RequiredIfTrueAttribute(string requiredIfTrue, string isDisabled)
    {
        _requiredIfTrue = requiredIfTrue;
        _isDisabled = isDisabled;
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
        var isValidationRequired = instance.GetType().GetProperty(_requiredIfTrue)?.GetValue(instance, null);
        var isValueDisabled = instance.GetType().GetProperty(_isDisabled)?.GetValue(instance, null);

        var isRequired = isValidationRequired is not bool || (bool) isValidationRequired;
        var isDisabled = isValueDisabled is not bool || (bool) isValueDisabled;

        if (isRequired && !isDisabled)
        {

            var wage = value as decimal?;
            return wage.HasValue && wage.Value >= Decimal.Zero
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Wage entry is required ", new[] { context.MemberName! });
        }

        return ValidationResult.Success;
    }
}

