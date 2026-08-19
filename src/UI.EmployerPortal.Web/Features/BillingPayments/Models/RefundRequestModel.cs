using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Form model for the Request a Refund page.
/// </summary>
public class RefundRequestModel
{
    /// <summary>Refund amount, pre-populated from the employer's total credit balance.</summary>
    public decimal RefundAmount { get; set; }

    /// <summary>Payee name, pre-populated from the employer's legal name on file.</summary>
    public string PayeeName { get; set; } = string.Empty;

    /// <summary>Refund address, pre-populated from the employer's main business mailing address.</summary>
    public string RefundAddress { get; set; } = string.Empty;

    /// <summary>Email address the employer will be notified at if the refund request is denied.</summary>
    [Required(ErrorMessage = "Email Address is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address")]
    public string EmailAddress { get; set; } = string.Empty;

    /// <summary>Optional additional information/comments to include with the refund request.</summary>
    [MaxLength(500, ErrorMessage = "Additional Information cannot exceed 500 characters")]
    public string? AdditionalInformation { get; set; }
}
