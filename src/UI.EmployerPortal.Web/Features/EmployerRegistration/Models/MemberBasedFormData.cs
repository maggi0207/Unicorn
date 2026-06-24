namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
/// <summary>
/// MemberBasedFormData
/// </summary>
public class MemberBasedFormData
{
    /// <summary>
    /// RegistrationState
    /// </summary>
    public string RegistrationState { get; set; } = string.Empty;
    /// <summary>
    /// Members
    /// </summary>
    public List<OwnerMember> Members { get; set; } = new();
    /// <summary>
    /// MoreThanFive
    /// </summary>
    public bool? MoreThanFive { get; set; }
}
