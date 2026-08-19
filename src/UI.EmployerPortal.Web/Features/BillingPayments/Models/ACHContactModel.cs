
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;


namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;
/// <summary>
/// ACH Contact
/// </summary>
public class ACHContactModel
{
    /// <summary>
    /// The first name of the person uploading the file
    /// </summary>
    ///
    [Required(ErrorMessage = "Contact Name is required.")]
    public string ContactName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the phone number for upload contact
    /// </summary>
    ///
    [Required(ErrorMessage = "Phone Number is required.")]
    [PhoneNumberValidation]
    public string PhoneNumber { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the optional phone extension for the upload contact
    /// </summary>
    public string PhoneExt { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the phone number text for upload contact
    /// </summary>
    ///
    [Required(ErrorMessage = "Phone Number Format is required.")]
    public string PhoneNumberFormat { get; set; } = "United States/Canada";
    /// <summary>
    /// Gets or sets the email address for the upload contact
    /// </summary>
    ///
    [Required(ErrorMessage = "Email address is required.")]
    [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Enter a valid email address.")]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the confirmation email address for the upload contact
    /// </summary>
    ///
    [ConfirmEmailvalidation]
    public string ConfirmEmail { get; set; } = string.Empty;
    /// <summary>
    /// Returnvalue
    /// </summary>
    public int WebContactInformationsk { get; set; }
    /// <summary>
    /// InternationalFlag
    /// </summary>
    public bool InternationalFlag { get; set; }

    /// <summary>
    /// check
    /// </summary>
    public class ConfirmEmailvalidationAttribute : ValidationAttribute
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="validationContext"></param>
        /// <returns></returns>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (ACHContactModel) validationContext.ObjectInstance;
            var confirmEmail = value as string;
            if (string.IsNullOrWhiteSpace(confirmEmail))
            {
                return new ValidationResult("Verify Email Address is required.", new[] { validationContext.MemberName! });
            }
            var regex = new Regex(@"^[^@\s]+@[^@\s]+\.[^@\s]+$");
            return !regex.IsMatch(confirmEmail)
                ? new ValidationResult("Invalid email format.", new[] { validationContext.MemberName! })
                : confirmEmail != model.Email ? new ValidationResult("Email addresses do not match.", new[] { validationContext.MemberName! }) : ValidationResult.Success;
        }
    }
    /// <summary>
    /// Validates phone number digit count based on the selected phone number format
    /// </summary>
    public class PhoneNumberValidationAttribute : ValidationAttribute
    {

        /// <summary>
        /// Validates phone number digit count based on the selected phone number format
        /// </summary>
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            var model = (ACHContactModel) validationContext.ObjectInstance;
            var phone = value as string;
            if (string.IsNullOrWhiteSpace(phone))
            {
                return ValidationResult.Success; // [Required] handles empty
            }
            var digits = new string(phone.Where(char.IsDigit).ToArray());
            var isInternational = model.PhoneNumberFormat == "International";
            return digits.Length < 10
                ? new ValidationResult("Phone Number must be at least 10 digits.")
                : !isInternational && digits.Length > 10 ? new ValidationResult("Phone Number must be 10 digits.") : ValidationResult.Success;
        }
    }
}
