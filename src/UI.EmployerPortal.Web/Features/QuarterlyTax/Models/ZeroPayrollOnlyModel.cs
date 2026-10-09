using static UI.EmployerPortal.Web.Features.QuarterlyTax.Components.ZeroTaxPayrollDetails;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Unified data model for the ZeroPayrollReport page.
/// Acts as the single source of truth shared across step 1 (ZeroTaxEntry) and step 2 (ZeroTaxPayrollDetails).
/// </summary>
public class ZeroPayrollOnlyModel : BaseQuarterlyTaxReportModel
{
    /// <summary>
    /// Wage threshold amount for exclusions
    /// </summary>
    public decimal ExclusionThreshold { get; set; }

    /// <summary>
    /// Zero tax entry data for step 1.
    /// </summary>
    public ZeroTaxEntryModel ZeroTaxEntryData { get; set; } = new();

    /// <summary>
    /// Zero tax details data for step 2 verification.
    /// </summary>
    public ZeroTaxDetailsModel ZeroTaxDetailsData { get; set; } = new();
}
