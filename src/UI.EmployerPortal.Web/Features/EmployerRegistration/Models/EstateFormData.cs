namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Estate Form Data
/// </summary>
public class EstateFormData
{
    /// <summary>
    /// Decedent
    /// </summary>
    public OwnerMember Decedent { get; set; } = new();

    /// <summary>
    /// Personal Representative
    /// </summary>
    public OwnerMember PersonalRepresentative { get; set; } = new();
}
