using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents the reason an employer no longer has paid employees working in Wisconsin,
/// selected during the Preliminary Questions step of registration.
/// </summary>
public enum NoEmployeeReason
{
    /// <summary>Business activity has ended but the business has not been sold.</summary>
    [Display(Name = "BusinessActivityEnded")]
    BusinessActivityEnded = 0,

    /// <summary>No longer operating in Wisconsin but still operating in another state.</summary>
    [Display(Name = "NotOperatingInWisconsin")]
    NotOperatingInWisconsin = 1,

    /// <summary>Business activity has been sold or transferred.</summary>
    [Display(Name = "HaveSoldOrTransferredBusiness")]
    HaveSoldOrTransferredBusiness = 2,

    /// <summary>Business is continuing without employees.</summary>
    [Display(Name = "BusiessWithoutEmployees")]
    BusiessWithoutEmployees = 3,

    /// <summary>Employing independent contractors instead of direct employees.</summary>
    [Display(Name = "EmployingIndependentContractors")]
    EmployingIndependentContractors = 4,

    /// <summary>Employer has died.</summary>
    [Display(Name = "Death")]
    Death = 5,

    /// <summary>Leasing employees from a Professional Employer Organization (PEO).</summary>
    [Display(Name = "LeasingFromPEO")]
    LeasingFromPEO = 6,

    /// <summary>Acting as a Fiscal Agent electing to be the employer.</summary>
    [Display(Name = "FiscalAgent")]
    FiscalAgent = 7,

    /// <summary>Any other reason not covered by the above options.</summary>
    [Display(Name = "Other")]
    Other = 8,
}
