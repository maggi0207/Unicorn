namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Represents a single ESP client relationship shown in the Manage Client Accounts grid.
/// </summary>
public class EspClientModel
{
    /// <summary>
    /// The client's legal name, shown in the "Account Name" column.
    /// </summary>
    public string AccountName { get; set; } = string.Empty;

    /// <summary>
    /// The client's UI account number, shown in the "UI Account No." column.
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// The client employer's common client surrogate key, used to identify the
    /// relationship when removing it.
    /// </summary>
    public int CommonClientSK { get; set; }
}
