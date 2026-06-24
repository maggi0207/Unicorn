namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// QuarterlyReportSelectionMethod
/// </summary>
public class QuarterlyReportSelectionMethod
{
    /// <summary>
    /// CodeSK
    /// </summary>
    public int CodeSK { get; set; }

    /// <summary>
    /// Values:
    /// "Tax Report Only"
    /// "Tax Report with Employee Wage Entry"
    /// "Tax Report with Employee Wage Upload"
    /// "Wage Entry"
    /// "Wage Upload"
    /// "Zero Payroll this Quarter Report"
    /// </summary>
    public string ShortDescription { get; set; } = string.Empty;

    /// <summary>
    /// Example values:
    /// Online entry of wage detail with automated tax calculations.
    /// Online entry of tax report with wage file.
    /// No wages paid this quater
    /// </summary>
    public string LongDescription { get; set; } = string.Empty;

    /// <summary>
    /// If user is eligible to file this type of report
    /// </summary>
    public bool IsEligible { get; set; } = true;

    /// <summary>
    /// Example values are:
    /// Previous report had over 150 employees
    /// A Tax Report was previously filed for this quarter.
    /// A wage report was previously filed for this quarter.
    /// </summary>
    public List<string> EligibleReasons { get; set; } = [];
}
