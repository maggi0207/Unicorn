namespace UI.EmployerPortal.Web.Features.EmployerInformation.Models;

/// <summary>
/// Data type used to pass the status change success message to the employer information page
/// </summary>
public class StatusChangeSuccessMessage
{
    /// <summary>
    /// Confirmation Number of the status change
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Message to be displayed on the employer information page
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// If the Status Change was a closure, this carries the closure reason
    /// </summary>
    public ClosureReason? ClosureReason { get; set; }

    /// <summary>
    /// If the Status Change was a reactivation, this carries the activation reason
    /// </summary>
    public ActivationReason? ActivationReason { get; set; }
}
