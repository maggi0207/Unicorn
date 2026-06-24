namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Limited Partnership Form Data
/// </summary>
public class LimitedPartnershipFormData
{
    /// <summary>
    /// Registration State
    /// </summary>
    public string RegistrationState { get; set; } = string.Empty;

    /// <summary>
    /// Limited Partnership Name
    /// </summary>
    public string LimitedPartnershipName { get; set; } = string.Empty;

    /// <summary>
    /// General Partner
    /// </summary>
    public OwnerMember GeneralPartner { get; set; } = new();
}
