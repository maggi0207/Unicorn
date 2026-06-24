namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Configuration for ownership form display and behavior
/// </summary>
public class OwnershipFormConfig
{
    /// <summary>
    /// Ownership type value (e.g., "llc", "corporation")
    /// </summary>
    public string OwnershipTypeValue { get; set; } = string.Empty;

    /// <summary>
    /// Display name for the ownership type
    /// </summary>
    public string TypeDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Label for member/owner entries (e.g., "Member", "Partner")
    /// </summary>
    public string MemberLabel { get; set; } = "Member";

    /// <summary>
    /// Instruction text shown above the owner/member entry form
    /// </summary>
    public string InstructionText { get; set; } = string.Empty;

    /// <summary>
    /// Whether a state selection is required
    /// </summary>
    public bool RequiresState { get; set; }

    /// <summary>
    /// Whether foreign country option is available
    /// </summary>
    public bool RequiresForeignCountry { get; set; }

    /// <summary>
    /// Label for state/registration field
    /// </summary>
    public string StateLabel { get; set; } = "State";

    /// <summary>
    /// Maximum number of owner/member entries
    /// </summary>
    public int MaxEntries { get; set; } = 1;

    /// <summary>
    /// List of predefined roles (e.g., for corporation officers)
    /// </summary>
    public List<string> PredefinedRoles { get; set; } = new();

    /// <summary>
    /// Header text
    /// </summary>
    public string Header { get; set; } = "Owner Information";
}
