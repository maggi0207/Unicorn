using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
/// <summary>
/// Data Model for Contact Entry page, including business and contact details
/// </summary>
public class ContactModel : IValidatableObject
{
    /// <summary>
    /// The registered name of the business
    /// </summary>
    ///
    [Required(ErrorMessage = "Business Name is required.")]
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// Reusing existing AddressModel for the Business section
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    /// <summary>
    /// Gets or sets the business fax number
    /// </summary>
    ///
    public string FaxNumber { get; set; } = string.Empty;

    /// <summary>
    /// The first name of the person uploading the file
    /// </summary>
    ///
    [Required(ErrorMessage = "Contact Name is required.")]
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number for upload contact.
    /// </summary>
    [Required(ErrorMessage = "Phone Number is required.")]
    public string UploadPhoneNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the optional phone extension for the upload contact
    /// </summary>
    public string UploadExt { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the email address for the upload contact
    /// </summary>
    ///
    [Required(ErrorMessage = "Email address is required.")]
    public string UploadEmail { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation email address for the upload contact
    /// </summary>
    ///
    [Required(ErrorMessage = "Confirm email address is required.")]
    public string ConfirmUploadEmail { get; set; } = string.Empty;

    /// <summary>
    ///Gets or sets a value indicating whether the record contact information is identical to file upload contact information
    /// </summary>
    public bool SameAsFileUpload { get; set; } = false;

    /// <summary>
    ///Gets or sets the first name for permanent records if different from upload contact
    /// </summary>
    public string RecordContactName { get; set; } = string.Empty;

    /// <summary>
    ///Gets or sets the phone number for permanent records if different from upload contact
    /// </summary>
    [Required(ErrorMessage = "Record Phone Number is required.")]
    public string RecordPhone { get; set; } = string.Empty;

    /// <summary>
    ///Gets or sets the optional phone extension for permanent records
    /// </summary>
    public string RecordExt { get; set; } = string.Empty;

    /// <summary>
    ///Gets or sets the email address for permanent records if different from upload contact
    /// </summary>
    public string RecordEmail { get; set; } = string.Empty;

    /// <summary>
    ///Gets or sets the confirmation email for permanent records
    /// </summary>
    public string ConfirmationRecordEmail { get; set; } = string.Empty;

    /// <summary>
    /// Validate method.
    /// NOTE: Only validates the File Upload Contact email fields. Phone and Record*
    /// field validation is owned exclusively by ContactInformationEntry's code-behind
    /// to avoid duplicate validation messages, since &lt;DataAnnotationsValidator/&gt;
    /// and the code-behind's manual ValidationMessageStore are two separate stores on
    /// the same EditContext.
    /// </summary>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var emailAttr = new EmailAddressAttribute();

        if (!string.IsNullOrWhiteSpace(UploadEmail) && !emailAttr.IsValid(UploadEmail))
        {
            yield return new ValidationResult("Enter a valid email address.", new[] { nameof(RecordEmail) });
        }
        if (!string.IsNullOrWhiteSpace(ConfirmUploadEmail) && !emailAttr.IsValid(ConfirmUploadEmail))
        {
            yield return new ValidationResult("Enter a valid confirm email address.", new[] { nameof(ConfirmUploadEmail) });
        }
        if (emailAttr.IsValid(UploadEmail) && emailAttr.IsValid(ConfirmUploadEmail)
            && !string.Equals(UploadEmail, ConfirmUploadEmail, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Email addresses do not match.", new[] { nameof(ConfirmUploadEmail) });
        }
    }


    /// <summary>
    /// Clone object
    /// </summary>
    /// <param name="source"></param>
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
            Country = source.MailingAddress.Country,
            City = source.MailingAddress.City,
            State = source.MailingAddress.State,
            Province = source.MailingAddress.State,
            Zip = source.MailingAddress.Zip,
            Extension = source.MailingAddress.Extension,
            PostalCode = source.MailingAddress.PostalCode,
        };

        ContactName = source.ContactName;
        UploadPhoneNumber = source.UploadPhoneNumber;
        UploadExt = source.UploadExt;
        UploadEmail = source.UploadEmail;
        ConfirmUploadEmail = source.ConfirmUploadEmail;

        RecordContactName = source.RecordContactName;
        RecordPhone = source.RecordPhone;
        RecordExt = source.RecordExt;
        RecordEmail = source.RecordEmail;
        ConfirmationRecordEmail = source.ConfirmationRecordEmail;
    }
}

