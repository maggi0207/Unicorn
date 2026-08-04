namespace UI.EmployerPortal.Web.Features.EmployerInformation.Models;

/// <summary>
/// Model containing field backing members for the status change page
/// </summary>
public class StatusChangeModel
{
    /// <summary>
    /// 
    /// </summary>
    public DateOnly? EmploymentDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateOnly? PayrollDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public ClosureReason? ClosureReason { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public ActivationReason? ActivationReason { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string Comments { get; set; } = string.Empty;
}
