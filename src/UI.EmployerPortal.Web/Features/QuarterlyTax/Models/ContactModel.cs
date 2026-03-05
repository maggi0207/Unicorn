using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Data model for the Contact Information page, including business and contact details.
/// </summary>
public class ContactModel
{
    /// <summary>
    /// Gets or sets the registered name of the business.
    /// </summary>
    [Required(ErrorMessage = "Business name is required.")]
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the mailing address for the business.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    /// <summary>
    /// Gets or sets the business fax number.
    /// </summary>
    public string FaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the first name of the person uploading the file.
    /// </summary>
    [Required(ErrorMessage = "First name is required.")]
    public string ContactFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name of the person uploading the file.
    /// </summary>
    [Required(ErrorMessage = "Last name is required.")]
    public string ContactLastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number for the upload contact.
    /// </summary>
    [Required(ErrorMessage = "Phone number is required.")]
    public string UploadPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone extension for the upload contact.
    /// </summary>
    public string UploadExt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the upload contact.
    /// </summary>
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string UploadEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation email address for the upload contact.
    /// </summary>
    [Required(ErrorMessage = "Please confirm your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Compare(nameof(UploadEmail), ErrorMessage = "Email addresses do not match.")]
    public string ConfirmUploadEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets a value indicating whether the record contact is the same as the upload contact.
    /// </summary>
    public bool SameAsFileUpload { get; set; } = false;

    /// <summary>
    /// Gets or sets the first name for permanent records.
    /// </summary>
    [Required(ErrorMessage = "First name is required.")]
    public string RecordFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the last name for permanent records.
    /// </summary>
    [Required(ErrorMessage = "Last name is required.")]
    public string RecordLastName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number for permanent records.
    /// </summary>
    [Required(ErrorMessage = "Phone number is required.")]
    public string RecordPhone { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone extension for permanent records.
    /// </summary>
    public string RecordExt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for permanent records.
    /// </summary>
    [Required(ErrorMessage = "Email address is required.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string RecordEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation email for permanent records.
    /// </summary>
    [Required(ErrorMessage = "Please confirm your email address.")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    [Compare(nameof(RecordEmail), ErrorMessage = "Email addresses do not match.")]
    public string ConfirmationRecordEmail { get; set; } = string.Empty;
}
