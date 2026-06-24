using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

namespace UI.EmployerPortal.Razor.SharedComponents.Model;

/// <summary>
/// Represents a reusable address model used for mailing address and physical locations
/// in the employer registration wizard.
/// </summary>
public class AddressModel
{
    /// <summary>
    /// Existing employer-registration address identifier, used to update a previously saved address
    /// instead of creating a duplicate entry when the user saves and quits.
    /// </summary>
    public int RegistrationAddressSk { get; set; }

    /// <summary>
    /// Optional Name field.  Only required if 'IsNameVisible' is true.
    /// </summary>
    [RequiredIfNameVisible("IsNameVisible", ErrorMessage = "Name Required")]
    [MaxLength(255, ErrorMessage = "Name cannot exceed 255 characters")]
    public string? Name { get; set; }

    /// <summary>
    /// Country name. Defaults to "United States".
    /// </summary>
    [Required(ErrorMessage = "Country is required")]
    public string? Country { get; set; } = "United States";

    /// <summary>
    /// Primary address line. Maps to "Address Line 2" in SUITES backend.
    /// </summary>
    [AddressLine1Required]
    [AddressLine1MaxLength]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Secondary address line (optional). Maps to "Address Line 1" in SUITES backend.
    /// </summary>
    [MaxLength(64, ErrorMessage = "Address Line 2 cannot exceed 64 characters")]
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// Third address line (Only for Other International).
    /// </summary>
    [RequiredIfCountry("Other International", ErrorMessage = "Address Line 3 is required")]
    [MaxLength(64, ErrorMessage = "Address Line 3 cannot exceed 64 characters")]
    public string? AddressLine3 { get; set; }

    /// <summary>
    /// Fourth address line (Only for Other International).
    /// </summary>
    [RequiredIfCountry("Other International", ErrorMessage = "Address Line 4 is required")]
    [MaxLength(64, ErrorMessage = "Address Line 4 cannot exceed 64 characters")]
    public string? AddressLine4 { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    [MaxLength(64, ErrorMessage = "City cannot exceed 64 characters")]
    public string? City { get; set; }

    /// <summary>
    /// State abbreviation (e.g., "WI", "CA").
    /// </summary>
    [RequiredIfCountry("United States", ErrorMessage = "State is required")]
    public string? State { get; set; }

    /// <summary>
    /// Canadian Province.
    /// </summary>
    [RequiredIfCountry("Canada", ErrorMessage = "Province is required")]
    public string? Province { get; set; }

    /// <summary>
    /// 5-digit ZIP code.
    /// </summary>
    [RequiredIfCountry("United States", ErrorMessage = "Zip Code is required")]
    [MaxLength(5, ErrorMessage = "Zip Code cannot exceed 5 characters")]
    public string? Zip { get; set; }

    /// <summary>
    /// Canadian Postal Code.
    /// </summary>
    [RequiredIfCountry("Canada", ErrorMessage = "Postal Code is required")]
    [MaxLength(20, ErrorMessage = "Postal Code cannot exceed 20 characters")]
    public string? PostalCode { get; set; }

    /// <summary>
    /// Optional ZIP+4 extension.
    /// </summary>
    [MaxLength(4, ErrorMessage = "Zip Extension cannot exceed 4 characters")]
    public string? Extension { get; set; }

    /// <summary>
    /// If the Name field is visible, then it is required
    /// Optional phone number associated with this address (used when <c>ShowPhone</c> is enabled on <c>AddressField</c>).
    /// </summary>
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// International dialing prefix for the phone number (e.g. "+1", "+52").
    /// Defaults to "+1" (United States / Canada). Automatically updated when the country selection changes.
    /// </summary>
    public string? PhoneCountryCode { get; set; } = "+1";

    /// <summary>
    /// Type of phone number (e.g. Mobile, Work, Home).
    /// </summary>
    public string? PhoneType { get; set; }

    /// <summary>
    /// Optional extension for the phone number.
    /// </summary>
    public string? PhoneExtension { get; set; }

    /// <summary>
    /// If the Name field is visible, then it is required
    /// </summary>
    public bool IsNameVisible { get; set; } = false;

    /// <summary>
    /// Shared list of US states, territories, Canadian provinces, and military APO/FPO addresses.
    /// </summary>
    public static readonly List<SelectOption> States =
    [
        new() { Value = "AL", Text = "Alabama" },
        new() { Value = "AK", Text = "Alaska" },
        new() { Value = "AS", Text = "American Samoa" },
        new() { Value = "AZ", Text = "Arizona" },
        new() { Value = "AR", Text = "Arkansas" },
        new() { Value = "CA", Text = "California" },
        new() { Value = "CO", Text = "Colorado" },
        new() { Value = "CT", Text = "Connecticut" },
        new() { Value = "DE", Text = "Delaware" },
        new() { Value = "DC", Text = "District of Columbia" },
        new() { Value = "FM", Text = "Federated States of Micronesia" },
        new() { Value = "FL", Text = "Florida" },
        new() { Value = "GA", Text = "Georgia" },
        new() { Value = "GU", Text = "Guam" },
        new() { Value = "HI", Text = "Hawaii" },
        new() { Value = "ID", Text = "Idaho" },
        new() { Value = "IL", Text = "Illinois" },
        new() { Value = "IN", Text = "Indiana" },
        new() { Value = "IA", Text = "Iowa" },
        new() { Value = "KS", Text = "Kansas" },
        new() { Value = "KY", Text = "Kentucky" },
        new() { Value = "LA", Text = "Louisiana" },
        new() { Value = "ME", Text = "Maine" },
        new() { Value = "MH", Text = "Marshall Islands" },
        new() { Value = "MD", Text = "Maryland" },
        new() { Value = "MA", Text = "Massachusetts" },
        new() { Value = "MI", Text = "Michigan" },
        new() { Value = "MN", Text = "Minnesota" },
        new() { Value = "MS", Text = "Mississippi" },
        new() { Value = "MO", Text = "Missouri" },
        new() { Value = "MT", Text = "Montana" },
        new() { Value = "NE", Text = "Nebraska" },
        new() { Value = "NV", Text = "Nevada" },
        new() { Value = "NH", Text = "New Hampshire" },
        new() { Value = "NJ", Text = "New Jersey" },
        new() { Value = "NM", Text = "New Mexico" },
        new() { Value = "NY", Text = "New York" },
        new() { Value = "NC", Text = "North Carolina" },
        new() { Value = "ND", Text = "North Dakota" },
        new() { Value = "MP", Text = "Northern Mariana Islands" },
        new() { Value = "OH", Text = "Ohio" },
        new() { Value = "OK", Text = "Oklahoma" },
        new() { Value = "OR", Text = "Oregon" },
        new() { Value = "PW", Text = "Palau" },
        new() { Value = "PA", Text = "Pennsylvania" },
        new() { Value = "PR", Text = "Puerto Rico" },
        new() { Value = "RI", Text = "Rhode Island" },
        new() { Value = "SC", Text = "South Carolina" },
        new() { Value = "SD", Text = "South Dakota" },
        new() { Value = "TN", Text = "Tennessee" },
        new() { Value = "TX", Text = "Texas" },
        new() { Value = "UT", Text = "Utah" },
        new() { Value = "VT", Text = "Vermont" },
        new() { Value = "VI", Text = "Virgin Islands" },
        new() { Value = "VA", Text = "Virginia" },
        new() { Value = "WA", Text = "Washington" },
        new() { Value = "WV", Text = "West Virginia" },
        new() { Value = "WI", Text = "Wisconsin" },
        new() { Value = "WY", Text = "Wyoming" },
        new() { Value = "AB", Text = "Alberta" },
        new() { Value = "BC", Text = "British Columbia" },
        new() { Value = "MB", Text = "Manitoba" },
        new() { Value = "NB", Text = "New Brunswick" },
        new() { Value = "NL", Text = "Newfoundland and Labrador" },
        new() { Value = "NT", Text = "Northwest Territories" },
        new() { Value = "NS", Text = "Nova Scotia" },
        new() { Value = "NU", Text = "Nunavut" },
        new() { Value = "ON", Text = "Ontario" },
        new() { Value = "PE", Text = "Prince Edward Island" },
        new() { Value = "QC", Text = "Quebec" },
        new() { Value = "SK", Text = "Saskatchewan" },
        new() { Value = "YT", Text = "Yukon" },
        new() { Value = "AA", Text = "Armed Forces Americas" },
        new() { Value = "AE", Text = "Armed Forces Europe" },
        new() { Value = "AP", Text = "Armed Forces Pacific" },
    ];

    /// <summary>
    /// Shared list of supported countries.
    /// </summary>
    public static readonly List<SelectOption> Countries =
    [
        new() { Value = "United States", Text = "United States" },
        new() { Value = "Canada", Text = "Canada" },
       new() { Value = "Other International", Text = "Other International" },
    ];

    /// <summary>
    /// Returns a list of lines that can be displayed with simple line breaks.
    /// </summary>
    /// <returns></returns>
    public List<string> GetDisplayLines()
    {
        var lines = new List<string>();
        switch (Country)
        {
            case "United States":
                if (!string.IsNullOrWhiteSpace(AddressLine2))
                {
                    lines.Add(AddressLine2);
                }
                if (!string.IsNullOrWhiteSpace(AddressLine1))
                {
                    lines.Add(AddressLine1);
                }
                lines.Add($"{City} {State}, {Zip}{(!string.IsNullOrWhiteSpace(Extension) ? $"+{Extension}" : string.Empty)}");
                lines.Add(Country);
                break;
            case "Canada":
                if (!string.IsNullOrWhiteSpace(AddressLine2))
                {
                    lines.Add(AddressLine2);
                }
                if (!string.IsNullOrWhiteSpace(AddressLine1))
                {
                    lines.Add(AddressLine1);
                }
                lines.Add($"{City} {Province}, {PostalCode}{(!string.IsNullOrWhiteSpace(Extension) ? $"+{Extension}" : string.Empty)}");
                lines.Add(Country);
                break;
            default:
                if (!string.IsNullOrWhiteSpace(AddressLine2))
                {
                    lines.Add(AddressLine2);
                }
                if (!string.IsNullOrWhiteSpace(AddressLine1))
                {
                    lines.Add(AddressLine1);
                }
                if (!string.IsNullOrWhiteSpace(AddressLine3))
                {
                    lines.Add(AddressLine3);
                }
                if (!string.IsNullOrWhiteSpace(AddressLine4))
                {
                    lines.Add(AddressLine4);
                }
                break;
        }
        return lines;
    }
}

/// <summary>
/// The 'Name' field may not be visible on the form.  In that event,
/// the value is not required.
/// </summary>
public class RequiredIfNameVisibleAttribute : ValidationAttribute
{
    private readonly string _isNameVisible;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isNameVisible"></param>
    public RequiredIfNameVisibleAttribute(string isNameVisible)
    {
        _isNameVisible = isNameVisible;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var isValidationRequired = instance.GetType().GetProperty(_isNameVisible)?.GetValue(instance, null);

        var isRequired = isValidationRequired is not bool || (bool) isValidationRequired;

        if (isRequired)
        {
            var name = value == null ? String.Empty : value.ToString();
            return !String.IsNullOrWhiteSpace(name)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "Name is required ", new[] { context.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Requires a value if the selected country matches the specified criteria.
/// </summary>
public class RequiredIfCountryAttribute : ValidationAttribute
{
    private readonly string _country;

    /// <summary>
    /// Creates a new instance of RequiredIfCountryAttribute.
    /// </summary>
    /// <param name="country">The country that makes this property required.</param>
    public RequiredIfCountryAttribute(string country)
    {
        _country = country;
    }

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var countryPropertyValue = instance.GetType().GetProperty("Country")?.GetValue(instance, null) as string;

        if (countryPropertyValue == _country)
        {
            var strValue = value?.ToString();
            return !string.IsNullOrWhiteSpace(strValue)
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? $"{context.MemberName} is required", new[] { context.MemberName! });
        }

        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom required validation for AddressLine1 that displays "Street Address" for US and "Address Line 1" otherwise.
/// </summary>
public class AddressLine1RequiredAttribute : ValidationAttribute
{
    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var strValue = value?.ToString();
        if (string.IsNullOrWhiteSpace(strValue))
        {
            var instance = context.ObjectInstance;
            var country = instance.GetType().GetProperty("Country")?.GetValue(instance, null) as string;
            var label = country == "United States" ? "Street Address" : "Address Line 1";
            return new ValidationResult($"{label} is required", new[] { context.MemberName! });
        }
        return ValidationResult.Success;
    }
}

/// <summary>
/// Custom max length validation for AddressLine1 that dynamically labels as "Street Address" or "Address Line 1".
/// </summary>
public class AddressLine1MaxLengthAttribute : ValidationAttribute
{
    private readonly int _length = 64;

    /// <inheritdoc />
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var strValue = value?.ToString();
        if (!string.IsNullOrEmpty(strValue) && strValue.Length > _length)
        {
            var instance = context.ObjectInstance;
            var country = instance.GetType().GetProperty("Country")?.GetValue(instance, null) as string;
            var label = country == "United States" ? "Street Address" : "Address Line 1";
            return new ValidationResult($"{label} cannot exceed {_length} characters", new[] { context.MemberName! });
        }
        return ValidationResult.Success;
    }
}
