using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Model for the Business Information step (Step 3) of employer registration.
/// Used with EditForm + DataAnnotationsValidator for validation.
/// </summary>
public class BusinessInformationModel
{
    // Business Details
    [Required(ErrorMessage = "FEIN is required.")]
    public string? FEIN { get; set; }

    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    public string? TradeName { get; set; }

    [Required(ErrorMessage = "Phone Number is required.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    // Mailing Address
    [Required(ErrorMessage = "Country is required.")]
    public string? MailingCountry { get; set; } = "United States";

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? MailingAddressLine1 { get; set; }

    public string? MailingAddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? MailingCity { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public string? MailingState { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    public string? MailingZip { get; set; }

    public string? MailingExtension { get; set; }

    // Physical Location 1
    [Required(ErrorMessage = "Country is required.")]
    public string? PhysicalCountry { get; set; } = "United States";

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? PhysicalAddressLine1 { get; set; }

    public string? PhysicalAddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? PhysicalCity { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public string? PhysicalState { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    public string? PhysicalZip { get; set; }

    public string? PhysicalExtension { get; set; }
}
