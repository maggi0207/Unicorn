using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Holds the full tax and wage detail data for a single previously filed quarterly report.
/// </summary>
public sealed class PreviouslyFiledReportDetailsModel
{
    /// <summary>
    /// Numeric quarter (1–4).
    /// </summary>
    public int Quarter { get; init; }

    /// <summary>
    /// Numeric year.
    /// </summary>
    public int Year { get; init; }

    /// <summary>
    /// Formatted display string, e.g. "Q2 2024".
    /// </summary>
    public string QuarterYear { get; init; } = string.Empty;

    // ── Report sub-heading ────────────────────────────────────────────
    /// <summary>
    /// DueDate
    /// </summary>
    public DateTime DueDate { get; init; }

    /// <summary>
    /// EffectiveDate
    /// </summary>
    public DateTime EffectiveDate { get; init; }

    /// <summary>"Balanced" or "Out of Balance".</summary>
    public string ReportStatus { get; init; } = string.Empty;

    // ── Employer details panel ────────────────────────────────────────
    /// <summary>
    /// LegalName
    /// </summary>
    public string EmployerLegalName { get; init; } = string.Empty;

    /// <summary>
    /// Fein
    /// </summary>
    public string EmployerFein { get; init; } = string.Empty;

    /// <summary>
    /// Address
    /// </summary>
    public string EmployerAddress { get; init; } = string.Empty;

    /// <summary>
    /// Phone
    /// </summary>
    public string EmployerPhone { get; init; } = string.Empty;

    /// <summary>
    /// TaxConfirmationNumber
    /// </summary>
    public string TaxConfirmationNumber { get; init; } = string.Empty;

    /// <summary>
    /// WagesConfirmationNumber
    /// </summary>
    public string WagesConfirmationNumber { get; init; } = string.Empty;

    /// <summary>
    /// Data for the TaxDetails component.
    /// </summary>
    public TaxDetailsModel TaxDetails { get; init; } = new();

    /// <summary>
    /// Data for the WageDetails component.
    /// </summary>
    public WageDetailsModel WageDetails { get; init; } = new();
}
