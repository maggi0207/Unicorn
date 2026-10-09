using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents a missing quarterly report that requires user action 
/// </summary>
public class MissingReportModel : IValidatableObject
{
    /// <summary>
    /// Gets or sets the description of the report.
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the due date for the report.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets the quarter identifier (e.g., "1", "2")
    /// </summary>
    public int Quarter { get; set; }

    /// <summary>
    /// Gets or sets the reporting quarter.
    /// </summary>
    public int Year { get; set; }

    /// <summary>
    /// Gets or sets the formatted quarter and report year display string.
    /// </summary>
    public string FormattedQuarterYear { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the name of the report.
    /// </summary>
    public string ReportName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the select value used for report identification. 
    /// </summary>
    public string SelectValue { get; set; } = string.Empty;

    /// <summary>
    /// Status description, like "Late" or "Current report needed".
    /// </summary>
    public string Status { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the report is past due.
    /// </summary>
    public bool IsLate { get; set; } = default;

    /// <summary>
    /// Gets or sets the WageTaxFilingSK for a pensing (in-progress) report. 
    /// </summary>
    public int? PendingWageTaxFilingSK { get; set; }

    /// <summary>
    /// Gets or sets the filing type code sk for a pending report. 
    /// </summary>
    public int? PendingFilingTypeCodeSK { get; set; }

    /// <summary>
    /// Gets or sets the Reporting Selection code sk for a pending report. 
    /// </summary>
    public int? PendingReportingSelectionCodeSK { get; set; }

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
