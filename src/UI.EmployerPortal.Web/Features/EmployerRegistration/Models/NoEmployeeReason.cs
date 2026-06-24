using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Select reasons for 
/// </summary>
public enum NoEmployeeReason
{
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "BusinessActivityEnded")]
    BusinessActivityEnded = 0,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "NotOperatingInWisconsin")]
    NotOperatingInWisconsin = 1,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "HaveSoldOrTransferredBusiness")]
    HaveSoldOrTransferredBusiness = 2,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "BusiessWithoutEmployees")]
    BusiessWithoutEmployees = 3,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "EmployingIndependentContractors")]
    EmployingIndependentContractors = 4,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Death")]
    Death = 5,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "LeasingFromPEO")]
    LeasingFromPEO = 6,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "FiscalAgent")]
    FiscalAgent = 7,
    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Other")]
    Other = 8,
}
