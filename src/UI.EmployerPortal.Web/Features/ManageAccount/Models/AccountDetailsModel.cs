using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Represents the view model for updating the employer's account details.
/// </summary>
public class AccountDetailsModel
{
    /// <summary>
    /// Gets or sets the Federal Employer Identification Number (FEIN).
    /// </summary>
    [Required(ErrorMessage = "FEIN is required.")]
    public string FEIN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected reason for changing the FEIN, if applicable.
    /// </summary>
    public string? ReasonForFeinChange { get; set; }

    /// <summary>
    /// Gets or sets the employer's legal name.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required.")]
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected reason for changing the Legal Name, if applicable.
    /// </summary>
    public string? ReasonForLegalNameChange { get; set; }

    /// <summary>
    /// Gets or sets the employer's optional trade name (DBA).
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Gets or sets the primary contact phone number.
    /// </summary>
    [Required(ErrorMessage = "Phone Number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number extension, if any.
    /// </summary>
    public string? Extension { get; set; }

    /// <summary>
    /// Gets or sets the primary email address.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string EmailAddress { get; set; } = string.Empty;
}
