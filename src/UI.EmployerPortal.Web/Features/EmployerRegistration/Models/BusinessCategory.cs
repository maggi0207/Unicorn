using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Business Category enumeration
/// </summary>
public enum BusinessCategory
{
    /// <summary>
    /// None
    /// </summary>
    [Display(Name = "Unknown")]
    Unknown = 0,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Agricultural (Farming)")]
    Agricultural = 1,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Non-Profit")]
    NonProfit = 2,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Domestic (in a private home)")]
    Domestic = 3,

    /// <summary>
    ///
    /// </summary>
    [Display(Name = "Commercial")]
    Commercial = 4,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Non-Prtofit with 501(c)(3) Ruling from IRS")]
    NonProfit_501c3 = 5,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Non-Profit (other)")]
    NonProfit_Other = 6,
}
