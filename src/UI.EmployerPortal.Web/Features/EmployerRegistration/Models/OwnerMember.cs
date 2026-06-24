namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents an owner or member of a business entity
/// </summary>
public class OwnerMember
{
    /// <summary>
    /// Role or title of the owner/member (e.g., President, Member 1)
    /// </summary>
    public string Role { get; set; } = string.Empty;

    /// <summary>
    /// First name of the owner/member
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Middle initial of the owner/member
    /// </summary>
    public string MiddleInitial { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the owner/member
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Social Security Number
    /// </summary>
    public string SSN { get; set; } = string.Empty;

    /// <summary>
    /// Ownership percentage (0-100)
    /// </summary>
    public decimal? OwnershipPercentage { get; set; }

    /// <summary>
    /// Tracks whether the SSN field is currently focused (for masking display)
    /// </summary>
    public bool IsSSNFocused { get; set; }
}
