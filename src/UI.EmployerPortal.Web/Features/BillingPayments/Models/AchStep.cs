namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Specifies the steps in the ACH (Automated Clearing House) processing workflow.
/// </summary>
public enum AchStep
{
    /// <summary>Initial step where the user enters their bank account information.</summary>
    Entry = 0,
    /// <summary>Step where the system verifies the bank account information provided by the user.</summary>
    VerifyAuthorize = 1,
    /// <summary>Final step where the user confirms the ACH payment after successful verification.</summary>
    Confirmation = 2,
    /// <summary>Step where the user edits a previously submitted ACH payment.</summary>
    Edit = 3,
    /// <summary>Step where the user verifies the details before confirming cancellation.</summary>
    VerifyCancel = 4,
    /// <summary>Confirmation step shown after a payment has been successfully cancelled.</summary>
    CancelConfirmation = 5,
    /// <summary>Printable view of the payment confirmation details.</summary>
    PrintPreview = 6
}
