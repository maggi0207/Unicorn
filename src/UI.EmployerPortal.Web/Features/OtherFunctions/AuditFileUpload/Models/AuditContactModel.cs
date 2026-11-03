using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

/// <summary>
/// Contact information captured on the Audit File Upload page.
/// Mirrors the mockup layout: a "Business Uploading the File" section (business name + address + fax)
/// and a "File Upload Contact" section (first/last name, phone, e-mail with confirmation).
/// </summary>
public class AuditContactModel : IValidatableObject
{
    /// <summary>
    /// The registered name of the business uploading the file.
    /// </summary>
    [Required(ErrorMessage = "Business Name is required.")]
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Mailing address for the business uploading the file. Re-uses the shared <see cref="AddressModel"/>.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    /// <summary>
    /// Optional business fax number.
    /// </summary>
    public string FaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// First name of the file upload contact.
    /// </summary>
    [Required(ErrorMessage = "Contact First Name is required.")]
    public string ContactFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name of the file upload contact.
    /// </summary>
    [Required(ErrorMessage = "Contact Last Name is required.")]
    public string ContactLastName { get; set; } = string.Empty;

    /// <summary>
    /// Phone number for the file upload contact.
    /// </summary>
    [Required(ErrorMessage = "Phone Number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Optional phone extension for the file upload contact.
    /// </summary>
    public string PhoneExt { get; set; } = string.Empty;

    /// <summary>
    /// E-mail address for the file upload contact.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Confirmation of the e-mail address. Must match <see cref="Email"/>.
    /// </summary>
    [Required(ErrorMessage = "Confirm Email Address is required.")]
    public string ConfirmEmail { get; set; } = string.Empty;

    /// <inheritdoc />
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var emailAttr = new EmailAddressAttribute();

        if (!string.IsNullOrWhiteSpace(Email) && !emailAttr.IsValid(Email))
        {
            yield return new ValidationResult("Enter a valid email address.", new[] { nameof(Email) });
        }
        if (!string.IsNullOrWhiteSpace(ConfirmEmail) && !emailAttr.IsValid(ConfirmEmail))
        {
            yield return new ValidationResult("Enter a valid confirm email address.", new[] { nameof(ConfirmEmail) });
        }
        if (emailAttr.IsValid(Email) && emailAttr.IsValid(ConfirmEmail)
            && !string.Equals(Email, ConfirmEmail, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Email addresses do not match.", new[] { nameof(ConfirmEmail) });
        }
    }

    /// <summary>
    /// Populates this model from the shared <see cref="ContactModel"/> returned by ObtainFileContact.
    /// The single <see cref="ContactModel.ContactName"/> is split into first/last name on the first space.
    /// </summary>
    public void CopyFrom(ContactModel source)
    {
        if (source is null)
        {
            return;
        }

        BusinessName = source.BusinessName;
        FaxNumber = source.FaxNumber;

        MailingAddress = new AddressModel
        {
            AddressLine1 = source.MailingAddress.AddressLine1,
            AddressLine2 = source.MailingAddress.AddressLine2,
            AddressLine3 = source.MailingAddress.AddressLine3,
            AddressLine4 = source.MailingAddress.AddressLine4,
            IsAddressLine3RequiredInCurrentScreen = false,
            IsAddressLine4RequiredInCurrentScreen = false,
            Country = string.IsNullOrWhiteSpace(source.MailingAddress.Country) ? "United States" : source.MailingAddress.Country,
            City = source.MailingAddress.City,
            State = source.MailingAddress.State,
            Zip = source.MailingAddress.Zip,
            Extension = source.MailingAddress.Extension
        };

        (ContactFirstName, ContactLastName) = SplitName(source.ContactName);
        PhoneNumber = source.UploadPhoneNumber;
        PhoneExt = source.UploadExt;
        Email = source.UploadEmail;
        ConfirmEmail = source.ConfirmUploadEmail;
    }

    /// <summary>
    /// Maps this model back onto a shared <see cref="ContactModel"/> for SaveFileContact.
    /// First and last name are combined into the single ContactName field, and the "records"
    /// contact is treated as identical to the upload contact (SameAsFileUpload).
    /// </summary>
    public ContactModel ToContactModel()
    {
        var fullName = $"{ContactFirstName} {ContactLastName}".Trim();

        return new ContactModel
        {
            BusinessName = BusinessName,
            FaxNumber = FaxNumber,
            MailingAddress = new AddressModel
            {
                AddressLine1 = MailingAddress.AddressLine1,
                AddressLine2 = MailingAddress.AddressLine2,
                AddressLine3 = MailingAddress.AddressLine3,
                AddressLine4 = MailingAddress.AddressLine4,
                IsAddressLine3RequiredInCurrentScreen = false,
                IsAddressLine4RequiredInCurrentScreen = false,
                Country = MailingAddress.Country,
                City = MailingAddress.City,
                State = MailingAddress.State,
                Zip = MailingAddress.Zip,
                Extension = MailingAddress.Extension
            },
            ContactName = fullName,
            UploadPhoneNumber = PhoneNumber,
            UploadExt = PhoneExt,
            UploadEmail = Email,
            ConfirmUploadEmail = ConfirmEmail,

            // Audit upload has no separate "records" contact, so mirror the upload contact.
            SameAsFileUpload = true,
            RecordContactName = fullName,
            RecordPhone = PhoneNumber,
            RecordExt = PhoneExt,
            RecordEmail = Email,
            ConfirmationRecordEmail = ConfirmEmail
        };
    }

    private static (string First, string Last) SplitName(string? fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
        {
            return (string.Empty, string.Empty);
        }

        var trimmed = fullName.Trim();
        var firstSpace = trimmed.IndexOf(' ');
        return firstSpace < 0
            ? (trimmed, string.Empty)
            : (trimmed[..firstSpace], trimmed[(firstSpace + 1)..].Trim());
    }
}
