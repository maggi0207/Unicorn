namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// AccountUserModel
/// </summary>
public sealed record AccountUserModel
{
    /// <summary>
    /// LastName
    /// </summary>
    public string LastName { get; init; } = string.Empty;

    /// <summary>
    /// FirstName
    /// </summary>
    public string FirstName { get; init; } = string.Empty;

    /// <summary>
    /// Role
    /// </summary>
    public string Role { get; init; } = string.Empty;

    /// <summary>
    /// CreatedBy
    /// </summary>
    public string CreatedBy { get; init; } = string.Empty;

    /// <summary>
    /// CreatedBy
    /// </summary>
    public string AccessKeyMasked { get; init; } = string.Empty;

    /// <summary>
    /// AccessGranted
    /// </summary>
    public DateTime? AccessGranted { get; init; }

    /// <summary>
    /// WebUserSecuritySK
    /// </summary>
    public int WebUserSecuritySK { get; init; }              // row key for future Edit/Delete

    /// <summary>
    /// SecureUserSk
    /// </summary>
    public int SecureUserSK { get; init; }                   // needed for in-memory CreatedBy lookup

    /// <summary>
    /// AssignedWebControlSKs
    /// </summary>
    public int[] AssignedWebControlSKs { get; init; } = [];  // controls currently granted to this user
}
