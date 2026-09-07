using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Form model backing the Employer Service Provider registration flow
/// (Business Information, Business Mailing Address and General Contact).
/// </summary>
public class EspRegistrationModel : IValidatableObject
{
    // ───────────────────────── Business Information ─────────────────────────

    /// <summary>Legal business name of the service provider.</summary>
    [Required(ErrorMessage = "Legal Name is required")]
    [MaxLength(255, ErrorMessage = "Legal Name cannot exceed 255 characters")]
    public string? LegalName { get; set; }

    /// <summary>Federal Employer Identification Number (formatted 99-9999999).</summary>
    [Required(ErrorMessage = "FEIN is required")]
    public string? Fein { get; set; }

    /// <summary>Primary phone number (formatted 999-999-9999).</summary>
    [Required(ErrorMessage = "Phone Number is required")]
    [TenDigitPhone(ErrorMessage = "Phone Number must be 10 digits")]
    public string? PhoneNumber { get; set; }

    /// <summary>Optional fax number (formatted 999-999-9999).</summary>
    public string? FaxNumber { get; set; }

    /// <summary>Optional public website address.</summary>
    [MaxLength(255, ErrorMessage = "Website cannot exceed 255 characters")]
    public string? Website { get; set; }

    /// <summary>Primary email address.</summary>
    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Enter a valid email address.")]
    public string? Email { get; set; }

    /// <summary>Confirmation of the primary email address.</summary>
    [Required(ErrorMessage = "Email Confirm is required")]
    public string? ConfirmEmail { get; set; }

    /// <summary>Optional SIDES broker number.</summary>
    public string? SidesNumber { get; set; }

    // ─────────────────────── Business Mailing Address ───────────────────────

    /// <summary>
    /// Business mailing address. Uses the shared, country-aware <see cref="AddressModel"/>
    /// so the address fields (State/Province, Zip/Postal Code, international lines) switch
    /// based on the selected country.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    // ───────────────────────────── General Contact ──────────────────────────

    /// <summary>General contact first name.</summary>
    [Required(ErrorMessage = "First Name is required")]
    [MaxLength(64, ErrorMessage = "First Name cannot exceed 64 characters")]
    public string? FirstName { get; set; }

    /// <summary>General contact last name.</summary>
    [Required(ErrorMessage = "Last Name is required")]
    [MaxLength(64, ErrorMessage = "Last Name cannot exceed 64 characters")]
    public string? LastName { get; set; }

    /// <summary>Cross-field validation: confirm email must match the email.</summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(Email)
            && !string.IsNullOrWhiteSpace(ConfirmEmail)
            && !string.Equals(Email, ConfirmEmail, StringComparison.OrdinalIgnoreCase))
        {
            yield return new ValidationResult("Email addresses do not match.", new[] { nameof(ConfirmEmail) });
        }
    }
}
