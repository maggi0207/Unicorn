namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Response model for Corporate Officer Exclusion save operations.
/// </summary>
public class COESaveResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the no-savings warning was shown.
    /// </summary>
    public bool IsNoSavingsShown { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the save succeeded.
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Gets or sets warning or rule-violation messages from the backend.
    /// </summary>
    public List<string> WarningMessage { get; set; } = new List<string>();

    /// <summary>
    /// Gets or sets the account number.
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Corporate Officer Exclusion effective date.
    /// </summary>
    public DateTime? CorporateOfficeExclusionEffectiveDate { get; set; }

    /// <summary>
    /// Gets or sets the Corporate Officer Exclusion status description.
    /// </summary>
    public string CorporateOfficeExclusionStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the Corporate Officer Exclusion status surrogate key.
    /// </summary>
    private int CorporateOfficeExclusionStatusSK { get; set; }

    /// <summary>
    /// Gets or sets the employer legal name.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UI account number.
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation identifier when available after save or reload.
    /// </summary>
    public string ConfirmationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the election was submitted when available after save or reload.
    /// </summary>
    public DateTime? DateSubmitted { get; set; }
}

/// <summary>
/// Display model for the Corporate Officer Exclusion intro and already-elected confirmation screens.
/// Kept in this file so office paste only needs one Models overwrite.
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
