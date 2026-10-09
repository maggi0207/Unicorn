using System.ComponentModel.DataAnnotations;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
/// <summary>
/// Represents an employee record within a quarterly wage entry report. 
/// </summary>
public class EmployeeWageEntryData : IValidatableObject
{
    /// <summary>
    /// Get or sets employee Id --WageTaxFilingEmployeeSKField
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// Stable per-row identity for the UI. Generated here, never sent to or read from the backend.
    ///
    /// <see cref="Id" /> cannot serve this purpose. It is the WageTaxFilingEmployeeSK, and on a
    /// first-time filing the employees are fetched from SUITES before any filing row exists, so it is
    /// null for all of them and the service falls back to <c>Id = 0</c> for the ENTIRE table. Building
    /// element ids or validation keys from it collapsed every row onto one value: the table rendered
    /// duplicate ids, and a row's error either linked to the wrong box or could not be attributed at
    /// all. This is unique per instance and never changes, whatever the backend supplies.
    /// </summary>
    public Guid RowKey { get; } = Guid.NewGuid();
    /// <summary>
    /// Gets or sets WageTaxFilingSK
    /// </summary>
    public long? WageTaxFilingSK { get; set; }
    /// <summary>
    /// Get or sets the employee's last name. 
    /// </summary>
    [Required(ErrorMessage = "Last Name is required.")]
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.LastNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.LastNameCharactersMessage)]
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the employee's first name.
    /// </summary>
    [Required(ErrorMessage = "First Name is required.")]
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.FirstNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.FirstNameCharactersMessage)]
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the employee's social security number.
    /// </summary>
    [Required(ErrorMessage = "Employee's Social Security Number is Required. If not known, enter all zeros.")]
    [RegularExpression(@"^\d{3}-\d{2}-\d{4}$", ErrorMessage = "SSN must be in the format ###-##-####.")]
    public string SSN { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the employee's gross covered wages for the reporting quarter.
    /// </summary>
    /// <remarks>
    /// NULLABLE deliberately. Null means "the user has not entered a wage yet", which a non-nullable
    /// decimal cannot express - it starts at 0, which displays as "$0.00" in an untouched box and
    /// satisfies both attributes below, so an employee could be saved at zero without anyone choosing
    /// that. Zero is a legitimate wage; it just has to be entered on purpose. With the property
    /// nullable, <c>[Required]</c> enforces exactly that, and no screen needs to work around it.
    /// </remarks>
    [Required(ErrorMessage = "Quarterly wage is required.")]
    [Range(0, EmployeeFieldRules.MaxQuarterlyWages, ErrorMessage = EmployeeFieldRules.WagesTooLargeMessage)]
    public decimal? QuarterlyWages { get; set; }
    /// <summary>
    /// Gets or sets a value indicating whether this employee's record should be carried over to the next quarter. 
    /// </summary>
    public bool SaveForNextQuarter { get; set; }
    /// <summary>
    /// Custom Validation.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        yield break;
    }
}
