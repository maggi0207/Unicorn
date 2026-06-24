using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// 
/// </summary>
public enum FuturePayPeriod
{
    /// <summary>
    /// None
    /// </summary>
    [Display(Name = "Within thirty days")]
    WithinThirtyDays = 1,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Thirty to ninety days")]
    ThirtyToNinetyDays = 2,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Six months")]
    SixMonths = 3,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "One year")]
    OneYear = 4,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "More than a year")]
    MoreThanOneYear = 5,
}
