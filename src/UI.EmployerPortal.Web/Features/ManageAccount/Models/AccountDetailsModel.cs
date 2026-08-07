using System;
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
    /// Gets or sets the selected FEIN change reason code SK.
    /// Maps to the CD_SK column of the FEIN Change Reasons lookup table.
    /// Passed as string to bind to OutlinedSelectField.
    /// </summary>
    public string? ReasonForFeinChange { get; set; }

    /// <summary>
    /// Gets or sets the free-text explanation entered when "Other" is selected
    /// as the reason for the FEIN change. Maps to EmployerUpdate.FeinChangeReasonExplanation.
    /// </summary>
    [RequiredIf("ReasonForFeinChange", "5", ErrorMessage = "Explanation for FEIN Change is required.")]
    public string? FeinChangeReasonExplanation { get; set; }

    /// <summary>
    /// Gets or sets the employer's legal name.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required.")]
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the selected business name change reason code SK.
    /// Maps to the CD_SK column of the Business Name Change Reasons lookup table.
    /// Passed as string to bind to OutlinedSelectField.
    /// </summary>
    public string? ReasonForLegalNameChange { get; set; }

    /// <summary>
    /// Gets or sets the free-text explanation entered when "Other" is selected
    /// as the reason for the legal name change. Maps to EmployerUpdate.LegalNameChangeExplanation.
    /// </summary>
    [RequiredIf("ReasonForLegalNameChange", "5", ErrorMessage = "Explanation for Legal Name Change is required.")]
    public string? LegalNameChangeExplanation { get; set; }

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
    /// Gets or sets the international country code for the phone number.
    /// </summary>
    public string? CountryCode { get; set; }

    /// <summary>
    /// Gets or sets the primary email address.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string EmailAddress { get; set; } = string.Empty;
}

/// <summary>
/// Specifies that a data field value is required when a dependent property matches a specific target value.
/// </summary>
public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _dependentProperty;
    private readonly object _targetValue;

    /// <summary>
    /// Initializes a new instance of the <see cref="RequiredIfAttribute"/> class.
    /// </summary>
    /// <param name="dependentProperty">The name of the property that this validation depends on.</param>
    /// <param name="targetValue">The value the dependent property must have for this field to be required.</param>
    public RequiredIfAttribute(string dependentProperty, object targetValue)
    {
        _dependentProperty = dependentProperty;
        _targetValue = targetValue;
    }

    /// <summary>
    /// Validates the value based on the specified dependent property and target value.
    /// </summary>
    /// <param name="value">The value to validate.</param>
    /// <param name="validationContext">The context information about the validation operation.</param>
    /// <returns>A <see cref="ValidationResult"/> if validation fails; otherwise, <see cref="ValidationResult.Success"/>.</returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var propertyInfo = validationContext.ObjectType.GetProperty(_dependentProperty);
        if (propertyInfo != null)
        {
            var dependentValue = propertyInfo.GetValue(validationContext.ObjectInstance);
            if (Equals(dependentValue, _targetValue) && string.IsNullOrWhiteSpace(value as string))
            {
                return new ValidationResult(ErrorMessage ?? $"{validationContext.DisplayName} is required.", new[] { validationContext.MemberName! });
            }
        }
        return ValidationResult.Success;
    }
}
