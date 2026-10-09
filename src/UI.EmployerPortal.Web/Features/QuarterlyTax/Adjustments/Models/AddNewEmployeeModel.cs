using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
/// <summary>
/// step1 class
/// </summary>
public class AddNewEmployeeModel
{
    /// <summary>
    /// Error message to show the validation
    /// </summary>
    [Required(ErrorMessage = "Quarter and Year are required.")]
    public string? SelectedQuarter { get; set; }
    /// <summary>
    /// Error message
    /// </summary>
    //  [Required(ErrorMessage = "Please Select a reason.")]
    public string? SelectedReason { get; set; }
    /// <summary>
    /// model
    /// </summary>
    public List<EmployeeEntryModel> EmployeeEntryModels { get; set; } = new();
    /// <summary>
    /// Gets or sets the wage adjustment SK for pending/resume scenario.
    /// </summary>
    public int? WageAdjustmentSK { get; set; }
}
/// <summary>
/// class for step2
/// </summary>
public class EmployeeEntryModel
{
    /// <summary>
    /// LastNmae
    /// </summary>
    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.LastNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.LastNameCharactersMessage)]
    public string? LastName { get; set; }
    /// <summary>
    /// FirstName
    /// </summary>
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.FirstNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.FirstNameCharactersMessage)]
    public string? FirstName { get; set; }
    /// <summary>
    /// SSN
    /// </summary>
    [Required(ErrorMessage = "Employee's Social Security Number is Required. If not known, enter all zeros.")]
    //[RegularExpression(@"^(?!000|666|9\d{2})\d{3}-(?!00)\d{2}-(?!0000)\d{4}$",
    //ErrorMessage = "Please enter a valid SSN.")]
    [ValidSSN]
    public string? SSN { get; set; }
    /// <summary>
    /// Wages
    /// </summary>
    [Required(ErrorMessage = "Quarterly wages are required.")]
    [Range(0, EmployeeFieldRules.MaxQuarterlyWages, ErrorMessage = EmployeeFieldRules.WagesTooLargeMessage)]
    public decimal? QuarterlyWages { get; set; }
    /// <summary>
    /// SSN Visibility 
    /// </summary>
    public bool IsSSNVisible { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public bool IsSSNFocused { get; set; } = false;
    // Blur-tracking flags
    /// <summary>
    /// 
    /// </summary>
    public bool LastNameTouched { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public bool FirstNameTouched { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public bool SSNTouched { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public bool WagesTouched { get; set; } = false;
    /// <summary>
    /// 
    /// </summary>
    public bool AddedAfterValidation { get; set; } = false;
}
/// <summary>
/// Custom validation attribute for SSN fields.
/// </summary>
public class ValidSSNAttribute : ValidationAttribute
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var ssn = value as string;
        var memberNames = new[] { context.MemberName ?? string.Empty };

        return IsValidSsn(ssn)
            ? ValidationResult.Success
            : new ValidationResult("Please enter a valid SSN.", memberNames);
    }

    /// <summary>
    /// Shared SSN validity check. Extracted so the exact same rules can be reused
    /// for inline UI validation (e.g. in AddEmployees.razor) without duplicating logic
    /// that can drift out of sync with this attribute.
    /// </summary>
    /// <param name="ssn">Raw SSN value, dashed or undashed.</param>
    /// <returns>True if the value is empty (Required attribute handles that separately) or a valid SSN.</returns>
    public static bool IsValidSsn(string? ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
        {
            return true;
        }
        var digits = new string(ssn.Where(char.IsDigit).ToArray());

        return digits switch
        {
            // All-zero SSN is the deliberate "unknown SSN" convention
            // so the backend generates an M00 number, mirroring the original Wage Entry screen.
            "000000000" => true,
            _ when digits.Length != 9 => false,
            _ when digits.Distinct().Count() == 1 => false,
            "123456789" => false,
            _ => System.Text.RegularExpressions.Regex.IsMatch(ssn,
                @"^(?!000|666|9\d{2})\d{3}-(?!00)\d{2}-(?!0000)\d{4}$")
        };
    }
}
