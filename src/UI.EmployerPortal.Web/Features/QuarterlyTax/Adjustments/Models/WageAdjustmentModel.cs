using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
/// <summary>
/// Root model for the Wage Report Adjustment wizard.
/// </summary>
public class WageAdjustmentModel
{
    /// <summary>
    /// Gets or sets the quarter selection step data.
    /// </summary>
    [Required(ErrorMessage = "Quarter and Year are required.")]
    public string? SelectedQuarterKey { get; set; }
    /// <summary>
    /// Gets or sets the employees with adjustments.
    /// </summary>
    public WageAdjustmentEmployeesModel Employees { get; set; } = new();
    /// <summary>
    /// Gets or sets the wage report SK of the selected quarter's report.
    /// </summary>
    public int WageReportSK { get; set; }
    /// <summary>
    /// Gets or sets the confirmation number after successful submission.
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the wage adjustment SK of the selected quarter's report.
    /// </summary>
    public int? WageAdjustmentSK { get; set; }
    /// <summary>
    /// Placeholder property used as a field identifier for server-side submission violation messages.
    /// </summary>
    public string SubmitError { get; set; } = string.Empty;
}

/// <summary>
/// Represents a single quarter/year option in the dropdown.
/// </summary>
public class WageReportQuarterOption
{
    /// <summary>
    /// Gets or sets the composite key "quarter|year".
    /// </summary>
    public string Key { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the display label, e.g. "Q1 2022".
    /// </summary>
    public string DisplayLabel { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the quarter number (1-4).
    /// </summary>
    public int Quarter { get; set; }
    /// <summary>
    /// Gets or sets the tax year.
    /// </summary>
    public int Year { get; set; }
    /// <summary>
    /// Gets or sets the wage report SK for this report.
    /// </summary>
    public long WageReportSK { get; set; }
}

/// <summary>
/// Model for Step 2 - Employees with Wage Adjustments.
/// </summary>
public class WageAdjustmentEmployeesModel
{
    /// <summary>
    /// Gets or sets the list of employees selected for adjustment.
    /// </summary>
    public List<WageAdjustmentEmployeeData> AdjustmentEmployees { get; set; } = [];
    /// <summary>
    /// Gets or sets the list of previously-reported employees available to add.
    /// </summary>
    public List<PreviouslyReportedEmployee> PreviouslyReportedEmployees { get; set; } = [];
    /// <summary>
    /// Gets or sets the available wage adjustment reasons.
    /// </summary>
    public List<WageAdjustmentReasonOption> AdjustmentReasons { get; set; } = [];
}

/// <summary>
/// Represents a single employee row in the wage adjustment table.
/// </summary>
public class WageAdjustmentEmployeeData
{
    /// <summary>
    /// Gets or sets the unique client-side ID for tracking rows.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();
    /// <summary>
    /// Gets or sets the original last name as reported.
    /// </summary>
    public string OriginalLastName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the original first name as reported.
    /// </summary>
    public string OriginalFirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the original SSN as reported.
    /// </summary>
    public string OriginalSSN { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the original quarterly wages as reported.
    /// </summary>
    public decimal OriginalQuarterlyWages { get; set; }
    /// <summary>
    /// Gets or sets the corrected last name (nullable = no change).
    /// </summary>
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.LastNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.LastNameCharactersMessage)]
    public string? CorrectedLastName { get; set; }
    /// <summary>
    /// Gets or sets the corrected first name (nullable = no change).
    /// </summary>
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.FirstNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.FirstNameCharactersMessage)]
    public string? CorrectedFirstName { get; set; }
    /// <summary>
    /// Gets or sets the corrected SSN (nullable = no change).
    /// </summary>
    public string? CorrectedSSN { get; set; }
    /// <summary>
    /// Gets or sets the adjusted quarterly wages (nullable = no change).
    /// </summary>
    [Range(0, EmployeeFieldRules.MaxQuarterlyWages, ErrorMessage = EmployeeFieldRules.WagesTooLargeMessage)]
    public decimal? AdjustedQuarterlyWages { get; set; }
    /// <summary>
    /// Gets or sets the selected wage adjustment reason code SK.
    /// </summary>
    public int? AdjustmentReasonCodeSK { get; set; }
    /// <summary>
    /// Gets or sets WageAdjustmentDetailSKField
    /// </summary>
    public int? WageAdjustmentDetailSKField { get; set; }
    /// <summary>
    /// Gets or sets WageAdjustmentSKField
    /// </summary>
    public int? WageAdjustmentSKField { get; set; }
    /// <summary>
    /// Gets or sets AdjustmentReasonValidation string place holder 
    /// </summary>
    public string? AdjustmentReasonValidation { get; set; }
    /// <summary>
    /// Gets or sets the order/index from the original report.
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// Represents an employee from a previously-submitted wage report available for adjustment.
/// </summary>
public class PreviouslyReportedEmployee
{
    /// <summary>
    /// Gets or sets the last name.
    /// </summary>
    public string LastName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the first name.
    /// </summary>
    public string FirstName { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the SSN.
    /// </summary>
    public string SSN { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the quarterly wages.
    /// </summary>
    public decimal QuarterlyWages { get; set; }
    /// <summary>
    /// Gets or sets the order/index in the original report.
    /// </summary>
    public int Order { get; set; }
}

/// <summary>
/// Represents a wage adjustment reason option.
/// </summary>
public class WageAdjustmentReasonOption
{
    /// <summary>
    /// Gets or sets the code SK.
    /// </summary>
    public int CodeSK { get; set; }
    /// <summary>
    /// Gets or sets the display text.
    /// </summary>
    public string ReasonText { get; set; } = string.Empty;
}
