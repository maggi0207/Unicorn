namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents a single row in the Previously Filed Reports grid.
/// </summary>
public sealed record PreviouslyFiledReportModel
{
    /// <summary>
    /// Display string for the quarter and year, e.g. "Q4 2022".
    /// </summary>
    public string QuarterYear { get; init; } = string.Empty;

    /// <summary>
    /// Gross wages from the wage report.
    /// </summary>
    public decimal? WageReportGrossWages { get; init; }

    /// <summary>
    /// Gross wages from the tax report.
    /// </summary>
    public decimal? TaxReportGrossWages { get; init; }

    /// <summary>
    /// Exclusions from the tax report.
    /// </summary>
    public decimal? TaxReportExclusions { get; init; }

    /// <summary>
    /// Taxable payroll from the tax report.
    /// </summary>
    public decimal? TaxReportTaxablePayroll { get; init; }

    /// <summary>
    /// Numeric quarter (1–4) used for sorting and navigation.
    /// </summary>
    public int Quarter { get; init; }

    /// <summary>
    /// Numeric year used for sorting and navigation.
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// True when a wage report has been filed for this quarter.
    /// </summary>
    public bool HasWageReport { get; init; }

    /// <summary>
    /// True when a tax report has been filed for this quarter.
    /// </summary>
    public bool HasTaxReport { get; init; }
}
