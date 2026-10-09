using System.ComponentModel.DataAnnotations;
using System.Globalization;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.WageAdjustmentsMultipleQuarter.Models;



/// <summary>
/// Base model for Quarterly Reports.
/// </summary>
public class WageAdjustmentsMultipleQuarterModelOnly : TaxAndWageAdjustmentModel
{

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public string SearchSSN { get; set; } = string.Empty;


    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public EmployeeIdentifierViewModel EmployeeIdentifierViewModel { get; set; } = new();


    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public EmployeeIdentifierModel EmployeeIdentifierModel { get; set; } = new();


    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public List<WageAdjustmentModelOnly> WageAdjustmentQuarterlyList { get; set; } = new();
}


/// <summary>
/// Base model for Quarterly Reports.
/// </summary>

public class EmployeeIdentifierViewModel
{
    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public string? FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public string? LastName { get; set; } = string.Empty;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public string? SSN { get; set; } = string.Empty;


}

/// <summary>
/// Base model for Quarterly Reports.
/// </summary>

public class EmployeeIdentifierModel
{

    /// <summary>
    /// Corrected first name for the employee whose wages are being adjusted across quarters.
    /// </summary>
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.FirstNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.FirstNameCharactersMessage)]
    public string? FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Corrected last name for the employee whose wages are being adjusted across quarters.
    /// </summary>
    [StringLength(EmployeeFieldRules.NameMaxLength, ErrorMessage = EmployeeFieldRules.LastNameTooLongMessage)]
    [RegularExpression(EmployeeFieldRules.NamePattern, ErrorMessage = EmployeeFieldRules.LastNameCharactersMessage)]
    public string? LastName { get; set; } = string.Empty;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    /// <remarks>
    /// Also accepts the M00 number ("M" plus eight digits) assigned to an employee reported
    /// without a Social Security Number, for example M00-12-3456.
    /// </remarks>
    [RegularExpression(@"^(\d{3}|M\d{2})-\d{2}-\d{4}$",
        ErrorMessage = "SSN must be in the format ###-##-####, or M##-##-#### for an M00 number.")]

    public string? SSN { get; set; } = string.Empty;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    public bool ShowMessage { get; set; } = false;
}


/// <summary>
/// Base model for Quarterly Reports.
/// </summary>

public class WageAdjustmentModelOnly
{

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    public string Quarter { get; set; } = string.Empty;


    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    public string Year { get; set; } = string.Empty;
    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    public string? QuarterlyWage { get; set; } = string.Empty;
    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    [Range(0, EmployeeFieldRules.MaxQuarterlyWages, ErrorMessage = EmployeeFieldRules.WagesTooLargeMessage)]
    public decimal? AdjustedQuarterlyWage { get; set; }
    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>

    public string? AdjustmentReason { get; set; } = string.Empty;

    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public int? WageReportSK { get; set; }
    /// <summary>
    /// Reporting quarter display string (e.g., "Q2 2022")
    /// </summary>
    public int? WageReportDetailOrder { get; set; }

    /// <summary>
    /// True when an adjusted wage was entered but matches the wage already reported,
    /// which is not a real adjustment. Submitting it produces a zero-variance detail
    /// that the service rejects, so it has to be caught during entry. A blank field
    /// is not flagged.
    /// </summary>
    /// <remarks>
    /// <see cref="QuarterlyWage"/> is the reported amount formatted as "N2" with the
    /// invariant culture, so it is parsed back with matching styles.
    /// </remarks>
    public bool IsAdjustedWageUnchanged()
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (!AdjustedQuarterlyWage.HasValue)
        {
            return false;
        }
#pragma warning restore IDE0046 // Convert to conditional expression

        return decimal.TryParse(
                QuarterlyWage,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var reportedWage)
            && AdjustedQuarterlyWage.Value == reportedWage;
    }
}
