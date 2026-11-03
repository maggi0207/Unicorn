namespace UI.EmployerPortal.Web.Features.OtherFunctions.Models;

/// <summary>
/// Week Date Range Model
/// </summary>
public class BenefitWeekDateRangeModel
{

    /// <summary>
    /// FiscalYear
    /// </summary>
    public DateModel? FiscalYear { get; set; }

    /// <summary>
    /// FiscalYear
    /// </summary>
    public DateModel? QuarterYear { get; set; }

    /// <summary>
    /// FiscalYear
    /// </summary>
    public DateModel? CurrentWeek { get; set; }

    /// <summary>
    /// FiscalYear
    /// </summary>
    public DateModel? CustomDateRange { get; set; }
}

/// <summary>
///  Date  Model
/// </summary>

public class DateModel
{
    /// <summary>
    /// Start Date
    /// </summary>

    public DateOnly? Start { get; set; }

    /// <summary>
    /// End Date
    /// </summary>

    public DateOnly? End { get; set; }

}


