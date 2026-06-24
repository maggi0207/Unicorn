namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// Response model for checking first quarter deferral eligibility.
/// Maps to WCF: FirstQuarterDeferralForYearResponse (extends PortalResponseBase).
/// </summary>

public class DeferralEligibilityResponse
{
    /// <summary>
    /// Whether the employer is eligible for first quarter deferral election.
    /// </summary>
    public bool IsEligible { get; set; }

    /// <summary>
    /// Any rule violations returned from the service.
    /// </summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
}

/// <summary>
/// Response model for electing first quarter deferral.
/// Maps to WCF: ElectForFirstQuarterDeferralResponse (extends PortalResponseBase).
/// </summary>
public class DeferralElectionResponse
{
    /// <summary>
    /// The confirmation number issued for the election.
    /// </summary>
    public string? ElectionConfirmationNumber { get; set; }

    /// <summary>
    /// The status of the election (e.g. "Elected").
    /// </summary>
    public string? ElectionStatus { get; set; }

    /// <summary>
    /// Any rule violations returned from the service.
    /// </summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
}

/// <summary>
/// RuleViolationItem
/// </summary>
public class RuleViolationItem
{
    /// <summary>
    /// RuleID
    /// </summary>
    public string RuleID { get; set; } = String.Empty;

    /// <summary>
    /// RuleViolation
    /// </summary>
    public string RuleViolation { get; set; } = String.Empty;
}
