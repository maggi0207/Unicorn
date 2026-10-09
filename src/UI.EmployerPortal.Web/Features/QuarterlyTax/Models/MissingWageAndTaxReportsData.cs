using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Data for the Missing Wage and Tax Reports PAge
/// </summary>
public class MissingWageAndTaxReportsData
{
    /// <summary>
    /// The employer account has one or more audits
    /// </summary>
    public bool HasActiveAudits { get; set; }

    /// <summary>
    /// Data for the Missing Reports grid
    /// </summary>
    public List<MissingReportModel> MissingReports { get; set; } = [];

    /// <summary>
    /// Amount of the available credit in the employer account
    /// </summary>
    public decimal AvailableCredit { get; set; }

    /// <summary>
    /// List of pending filing for this employer.
    /// </summary>
    public List<WageTaxFilingProxy> PendingReports { get; set; } = [];

    /// <summary>
    /// The actual period start date of the active audit
    /// </summary>
    public DateTime? AuditActualPeriodStartDate { get; set; }

    /// <summary>
    /// The actual period end date (audit date) of the active audit
    /// </summary>
    public DateTime? AuditActualPeriodEndDate { get; set; }

}
