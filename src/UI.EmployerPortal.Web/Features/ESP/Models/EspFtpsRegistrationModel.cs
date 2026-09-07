using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.Shared.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Form model backing the Employer Service Provider FTPS registration flow
/// (Business Information, Business Mailing Address and File Upload/Questions Contacts).
/// </summary>
public record EspFtpsRegistrationModel : IValidatableObject
{
    /// <summary>Uploading Business name.</summary>
    [Required(ErrorMessage = "Business Name is required")]
    [MaxLength(255, ErrorMessage = "Business Name cannot exceed 255 characters")]
    public string? BusinessName { get; set; }

    /// <summary>Country. Defaults to United States when first loaded.</summary>
    public string? Country { get; set; } = "United States";

    /// <summary>First line of the mailing address.</summary>
    [Required(ErrorMessage = "Address Line 1 is required")]
    [MaxLength(64, ErrorMessage = "Address Line 1 cannot exceed 64 characters")]
    public string? AddressLine1 { get; set; }

    /// <summary>Optional second line of the mailing address.</summary>
    [MaxLength(64, ErrorMessage = "Address Line 2 cannot exceed 64 characters")]
    public string? AddressLine2 { get; set; }

    /// <summary>City of the mailing address.</summary>
    [Required(ErrorMessage = "City is required")]
    [MaxLength(64, ErrorMessage = "City cannot exceed 64 characters")]
    public string? City { get; set; }

    /// <summary>State / province abbreviation of the mailing address.</summary>
    public string? State { get; set; }

    /// <summary>5-digit ZIP code.</summary>
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip Code is not a valid format.")]
    public string? ZipCode { get; set; }

    /// <summary>Optional ZIP+4 extension.</summary>
    [RegularExpression(@"^\d{4}$", ErrorMessage = "Zip +4 is not a valid format.")]
    public string? ZipExt { get; set; }

    /// <summary>Optional ZIP+4 extension.</summary>
    [RegularExpression(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$", ErrorMessage = "Canadian Postal Code is not a valid format.")]
    public string? CanadianPostalCode { get; set; }

    /// <summary>Optional fax number (formatted 999-999-9999).</summary>
    [RegularExpression(@"^\d{3}[-.]{0,1}\d{3}[-.]{0,1}\d{4}$", ErrorMessage = "Fax number is not a valid format.")]
    public string? FaxNumber { get; set; }

    #region File Upload Contact
    /// <summary>General contact name.</summary>
    [Required(ErrorMessage = "File Upload Name is required")]
    [MaxLength(64, ErrorMessage = "File Upload Name cannot exceed 64 characters")]
    public string? UploadContactName { get; set; }

    /// <summary>Phone number (formatted 999-999-9999).</summary>
    [Required(ErrorMessage = "Phone Number is required")]
    [RegularExpression(@"^\d{3}[-.]{0,1}\d{3}[-.]{0,1}\d{4}$", ErrorMessage = "Upload contact phone number is not a valid format.")]
    public string? UploadContactPhoneNumber { get; set; }

    /// <summary>Optional extension.</summary>
    [RegularExpression(@"^\d*$", ErrorMessage = "Extension is not a valid format.")]
    [MaxLength(10, ErrorMessage = "Extension cannot exceed 10 characters")]
    public string? UploadContactExtension { get; set; }

    /// <summary>Primary email address.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? UploadContactEmail { get; set; }

    /// <summary>Confirmation of the primary email address.</summary>
    [Required(ErrorMessage = "Email Confirm is required")]
    [EmailMatch(nameof(UploadContactEmail))]
    public string? UploadContactConfirmEmail { get; set; }

    #endregion
    /// <summary>
    /// Indicates whether the contact for questions about records is the same as the file upload contact.
    /// </summary>
    public bool IsSameAsFileUploadContact { get; set; }

    #region Records Questions Contact
    /// <summary>General contact first name.</summary>
    [Required(ErrorMessage = "Records Questions Name is required")]
    [MaxLength(64, ErrorMessage = "Records Questions Name cannot exceed 64 characters")]
    public string? QuestionsContactName { get; set; }

    /// <summary>Phone number (formatted 999-999-9999).</summary>
    [Required(ErrorMessage = "Phone Number is required")]
    [RegularExpression(@"^\d{3}[-.]{0,1}\d{3}[-.]{0,1}\d{4}$", ErrorMessage = "Questions contact phone number is not a valid format.")]
    public string? QuestionsContactPhoneNumber { get; set; }

    /// <summary>Optional extension.</summary>
    [RegularExpression(@"^\d*$", ErrorMessage = "Extension is not a valid format.")]
    [MaxLength(10, ErrorMessage = "Extension cannot exceed 10 characters")]
    public string? QuestionsContactExtension { get; set; }

    /// <summary>Primary email address.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? QuestionsContactEmail { get; set; }

    /// <summary>Confirmation of the primary email address.</summary>
    [Required(ErrorMessage = "Email Confirm is required")]
    [EmailMatch(nameof(QuestionsContactEmail))]
    public string? QuestionsContactConfirmEmail { get; set; }

    #endregion

    /// <summary>Custom validation</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // Require Zip code for U.S. addresses and Canadian Postal Code for Canada (none for International)
        if (Country == "United States" && string.IsNullOrWhiteSpace(ZipCode))
        {
            yield return new ValidationResult("Zip Code is required.", new[] { nameof(ZipCode) });
        }
        if (Country == "Canada" && string.IsNullOrWhiteSpace(CanadianPostalCode))
        {
            yield return new ValidationResult("Canadian Postal Code is required.", new[] { nameof(CanadianPostalCode) });
        }

        if ((Country == "United States" || Country == "Canada") && string.IsNullOrWhiteSpace(State))
        {
            var stateName = Country == "United States" ? "state" : "province";
            yield return new ValidationResult($"Please select a {stateName}.", new[] { nameof(State) });
        }
    }
}

/// <summary>
/// Defines a model for a person's contact information compatible with the
/// SaveFileContactRequest of the portal utilities' service SaveFtpUser endpoint.
/// </summary>
public record EspFtpsFileContactModel
{
    /// <summary>Name</summary>
    public string? Name { get; set; }

    /// <summary>Phone number.</summary>
    public string? PhoneNumber { get; set; }

    /// <summary>Optional extension.</summary>
    public string? Extension { get; set; }

    /// <summary>Email address.</summary>
    public string? Email { get; set; }
}
