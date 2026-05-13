using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents the expected future payroll period for an employer who does not currently have
/// paid employees in Wisconsin but expects to pay in the future.
/// Integer values match the <c>future_period</c> code table (CD_SK 1–5).
/// </summary>
public enum FuturePayPeriod
{
    /// <summary>Employer expects to pay within 30 days.</summary>
    [Display(Name = "Within thirty days")]
    WithinThirtyDays = 1,

    /// <summary>Employer expects to pay between 30 and 90 days.</summary>
    [Display(Name = "Thirty to ninety days")]
    ThirtyToNinetyDays = 2,

    /// <summary>Employer expects to pay within six months.</summary>
    [Display(Name = "Six months")]
    SixMonths = 3,

    /// <summary>Employer expects to pay within one year.</summary>
    [Display(Name = "One year")]
    OneYear = 4,

    /// <summary>Employer expects to pay in more than one year.</summary>
    [Display(Name = "More than a year")]
    MoreThanOneYear = 5,
}
