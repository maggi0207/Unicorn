using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Ownership type enumeration
/// </summary>
public enum OwnershipType
{
    /// <summary>
    /// None
    /// </summary>
    [Display(Name = "Select Ownership Type")]
    None = 0,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Limited Liability Company (LLC)")]
    LLC = 11,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "LLC Electing to be Treated as a Corporation")]
    LLCCorporation = 24,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Sole Proprietorship (not LLC or Corporation)")]
    SoleProprietorship = 17,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Corporation")]
    Corporation = 4,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Partnership (not LLC or Corporation)")]
    Partnership = 15,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Limited Liability Partnership (LLP)")]
    LLP = 12,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Limited Partnership (LP)")]
    LP = 23,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Association")]
    Association = 1,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Cooperative")]
    Cooperative = 3,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Estate")]
    Estate = 6,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Fiduciary")]
    Fiduciary = 8,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Individual")]
    Individual = 27,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Joint Venture")]
    JointVenture = 10,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Qualified Settlement Fund (QSF)")]
    QSF = 28,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Union")]
    Union = 21,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "City Government Agency")]
    CityGovernmentAgency = 2,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "County Government Agency")]
    CountyGovernmentAgency = 5,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Federal Government Agency")]
    FederalGovernmentAgency = 7,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Indian Tribe")]
    IndianTribe = 22,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Local Government Unit not listed")]
    LocalGovernmentUnitNotListed = 13,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "School District")]
    SchoolDistrict = 16,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "State Government Agency")]
    StateGovernmentAgency = 18,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "State Government Unit not listed")]
    StateGovernmentUnitNotListed = 14,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Township")]
    Township = 19,

    /// <summary>
    /// 
    /// </summary>
    [Display(Name = "Village")]
    Village = 20,

}
