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
    [Display(Name = "Within 30 days")]
    WithinThirtyDays = 1,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "30 to 90 days")]
    ThirtyToNinetyDays = 2,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "6 months")]
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
