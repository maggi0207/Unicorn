using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents the Business Information step (Step 3) of the Employer Registration process.
/// This model is used with EditForm and DataAnnotationsValidator
/// to capture and validate employer business details,
/// mailing address, and primary physical location information.
/// </summary>
public class BusinessInformationModel
{
    #region Business Details

    /// <summary>
    /// Gets or sets the Federal Employer Identification Number (FEIN).
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "FEIN is required.")]
    public string? FEIN { get; set; }

    /// <summary>
    /// Gets or sets the legal registered name of the business.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Gets or sets the trade name (Doing Business As - DBA) of the business.
    /// This field is optional.
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Gets or sets the primary business phone number.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Phone Number is required.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Gets or sets the primary business email address.
    /// Must be a valid email format.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    #endregion

    #region Mailing Address

    /// <summary>
    /// Gets or sets the mailing country.
    /// Defaults to "United States".
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Country is required.")]
    public string? MailingCountry { get; set; } = "United States";

    /// <summary>
    /// Gets or sets the first line of the mailing address.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? MailingAddressLine1 { get; set; }

    /// <summary>
    /// Gets or sets the second line of the mailing address (Apartment, Suite, etc.).
    /// This field is optional.
    /// </summary>
    public string? MailingAddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the mailing city.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "City is required.")]
    public string? MailingCity { get; set; }

    /// <summary>
    /// Gets or sets the mailing state.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "State is required.")]
    public string? MailingState { get; set; }

    /// <summary>
    /// Gets or sets the mailing ZIP or postal code.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Zip Code is required.")]
    public string? MailingZip { get; set; }

    /// <summary>
    /// Gets or sets the mailing ZIP code extension (e.g., ZIP+4).
    /// This field is optional.
    /// </summary>
    public string? MailingExtension { get; set; }

    #endregion

    #region Physical Location

    /// <summary>
    /// Gets or sets the physical location country.
    /// Defaults to "United States".
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Country is required.")]
    public string? PhysicalCountry { get; set; } = "United States";

    /// <summary>
    /// Gets or sets the first line of the physical address.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? PhysicalAddressLine1 { get; set; }

    /// <summary>
    /// Gets or sets the second line of the physical address (Apartment, Suite, etc.).
    /// This field is optional.
    /// </summary>
    public string? PhysicalAddressLine2 { get; set; }

    /// <summary>
    /// Gets or sets the physical city.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "City is required.")]
    public string? PhysicalCity { get; set; }

    /// <summary>
    /// Gets or sets the physical state.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "State is required.")]
    public string? PhysicalState { get; set; }

    /// <summary>
    /// Gets or sets the physical ZIP or postal code.
    /// This field is required.
    /// </summary>
    [Required(ErrorMessage = "Zip Code is required.")]
    public string? PhysicalZip { get; set; }

    /// <summary>
    /// Gets or sets the physical ZIP code extension (e.g., ZIP+4).
    /// This field is optional.
    /// </summary>
    public string? PhysicalExtension { get; set; }

    #endregion
}
