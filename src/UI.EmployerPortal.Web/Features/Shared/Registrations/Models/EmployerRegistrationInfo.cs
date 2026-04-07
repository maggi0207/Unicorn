using System.ComponentModel.DataAnnotations;
namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

/// <summary>
/// Employer Registration Information
/// </summary>
public class EmployerRegistrationInfo : IValidatableObject
{

    /// <summary>
    /// account Id
    /// </summary>
    public int Id { get; init; }

    /// <summary>
    /// Employer has Registration Number or not
    /// </summary>
    [Required(ErrorMessage = "Select whether you have a registration number.")]
    public bool? HasRegistrationNumber { get; set; }

    /// <summary>
    /// Employer Registration Number
    /// </summary>
    public string? RegistrationNumber { get; set; }

    /// <summary>
    /// Federal Employer Identification Number (FEIN)
    /// Format: 99-999999
    /// </summary>
    public string? FEIN { get; set; }

    /// <summary>
    /// Survey Response SK used for WCF services
    /// </summary>
    public int? SurveyResponseSK { get; set; }

    /// <summary>
    /// Employer UI AccountNo
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Conditional Validations
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        //Registration Number is required when user selects "Yes"
        if (HasRegistrationNumber == true && string.IsNullOrWhiteSpace(RegistrationNumber))
        {
            yield return new ValidationResult("Registration Number is required if you answer Yes", new[] { nameof(RegistrationNumber) });
        }

        //FEIN is required when user selects "Yes"
        if (HasRegistrationNumber == true && string.IsNullOrWhiteSpace(FEIN))
        {
            yield return new ValidationResult("FEIN is required if you answer Yes", new[] { nameof(FEIN) });
        }

        //FEIN is required when user selects "Yes"
        if (HasRegistrationNumber == true && !string.IsNullOrWhiteSpace(FEIN) && !System.Text.RegularExpressions.Regex.IsMatch(FEIN!, @"^\d{2}-\d{7}$"))
        {
            yield return new ValidationResult("FEIN Number format must be 99-9999999", new[] { nameof(FEIN) });
        }
    }

}
