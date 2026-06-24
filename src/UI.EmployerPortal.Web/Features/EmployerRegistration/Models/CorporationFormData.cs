namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
/// <summary>
/// CorporationFormData
/// </summary>
public class CorporationFormData
{
    /// <summary>
    /// IncorporationState
    /// </summary>
    public string IncorporationState { get; set; } = string.Empty;
    /// <summary>
    /// ForeignCountry
    /// </summary>
    public string ForeignCountry { get; set; } = string.Empty;
    /// <summary>
    /// Officers
    /// </summary>
    public List<OwnerMember> Officers { get; set; } = new();
}
