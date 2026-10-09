namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Request model for exclusion updates
/// </summary>
public class UpdateExclusionsRequest
{
    /// <summary>
    /// updated exclusion amount
    /// </summary>
    public decimal UpdatedAmount { get; set; }

    /// <summary>
    /// Reason for the change
    /// </summary>
    public string Reason { get; set; } = string.Empty;
}
