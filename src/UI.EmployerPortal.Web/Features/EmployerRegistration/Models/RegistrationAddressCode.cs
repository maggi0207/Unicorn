namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Address Type for the Employer Registration Workflow
/// </summary>
public enum RegistrationAddressCode
{
    /// <summary>
    /// No address code
    /// </summary>
    NONE = 0,

    /// <summary>
    /// 
    /// </summary>
    DFI_Received = 1,

    /// <summary>
    /// Main Business Mailing Address
    /// </summary>
    Main_Business_Mailing = 2,

    /// <summary>
    /// 
    /// </summary>
    DOR_Received = 3,

    /// <summary>
    /// Contact Address
    /// </summary>
    Contact = 4,

    /// <summary>
    /// Physical Location Address
    /// </summary>
    Physical_Location = 5,

    /// <summary>
    /// Acquired Business Address
    /// </summary>
    Acquired_Business = 6,

    /// <summary>
    /// Fiscal Agent Address
    /// </summary>
    Fiscal_Agent = 7,
}
