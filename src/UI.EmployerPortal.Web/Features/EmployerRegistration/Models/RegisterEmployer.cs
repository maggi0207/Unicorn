namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
/// <summary>
/// Register Employer Confirmation Page
/// </summary>
public sealed record RegisterEmployer
{
    /// <summary>
    /// RuleId
    /// </summary>
    public string RuleId { get; set; } = string.Empty;
    /// <summary>
    /// RuleViolation
    /// </summary>
    public string RuleViolation { get; set; } = string.Empty;
    /// <summary>
    /// SuitesAccountNumber
    /// </summary>
    public string SuitesAccountNumber { get; set; } = string.Empty;
    /// <summary>
    /// UIAccountNumber
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;
}
