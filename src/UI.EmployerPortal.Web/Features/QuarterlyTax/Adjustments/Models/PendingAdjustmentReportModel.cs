using UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;

/// <summary>
/// PendingAdjustmentReportModel
/// </summary>
public class PendingAdjustmentReportModel
{
    /// <summary>
    /// WageAdjustmentByQuarter
    /// </summary>
    public StageWageAdjustmentByQuarterProxy? WageAdjustmentByQuarter { get; set; }

    /// <summary>
    /// WageAdjustmentByEmployee
    /// </summary>
    public StageWageAdjustmentByEmployeeProxy? WageAdjustmentByEmployee { get; set; }

    /// <summary>
    /// WageAdjustmentAppended
    /// </summary>
    public StageWageAdjustmentByQuarterProxy? WageAdjustmentAppended { get; set; }

    /// <summary>
    /// HasPending
    /// </summary>
    public bool HasPending
        => WageAdjustmentByQuarter is not null ||
        WageAdjustmentByEmployee is not null ||
        WageAdjustmentAppended is not null;
}
