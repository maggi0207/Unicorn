namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Display model for the Corporate Officer Exclusion intro and already-elected confirmation screens.
/// </summary>
public class COEElectionSummary
{
    /// <summary>
    /// Gets or sets the formatted UI account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the employer legal name.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation identifier when an election already exists.
    /// </summary>
    public string ConfirmationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the election was submitted.
    /// </summary>
    public DateTime? DateSubmitted { get; set; }

    /// <summary>
    /// Gets or sets the Corporate Officer Exclusion effective date.
    /// </summary>
    public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether an election is already in effect for this employer.
    /// </summary>
    public bool IsAlreadyElected { get; set; }
}
