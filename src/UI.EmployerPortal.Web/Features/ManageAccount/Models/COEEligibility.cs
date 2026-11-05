namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Result of <c>GetCOEEligibility</c> for the Corporate Officer Exclusion page.
/// </summary>
public class COEEligibility
{
    /// <summary>
    /// Gets or sets a value indicating whether the employer may start an election.
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Gets or sets the years the employer may elect. Usually one year.
    /// More than one when a new or transferred employer is also inside the standard filing window.
    /// </summary>
    public IReadOnlyList<int> EligibleYears { get; set; } = [];
}
