namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents one calendar year's worth of quarterly gross wage entries used in
/// the Unemployment Insurance Subjectivity wage table.
/// </summary>
public class WageEntryModel
{
    /// <summary>The calendar year this row represents.</summary>
    public int Year { get; set; }

    /// <summary>Gross wages paid in Q1 (January – March).</summary>
    public decimal? Q1Wages { get; set; }

    /// <summary>Gross wages paid in Q2 (April – June).</summary>
    public decimal? Q2Wages { get; set; }

    /// <summary>Gross wages paid in Q3 (July – September).</summary>
    public decimal? Q3Wages { get; set; }

    /// <summary>Gross wages paid in Q4 (October – December).</summary>
    public decimal? Q4Wages { get; set; }

    /// <summary>When true, the Q1 input is read-only (period predates the first wage date).</summary>
    public bool Q1Disabled { get; set; }

    /// <summary>When true, the Q2 input is read-only (period predates the first wage date).</summary>
    public bool Q2Disabled { get; set; }

    /// <summary>When true, the Q3 input is read-only (period predates the first wage date).</summary>
    public bool Q3Disabled { get; set; }

    /// <summary>When true, the Q4 input is read-only (period predates the first wage date).</summary>
    public bool Q4Disabled { get; set; }
}
