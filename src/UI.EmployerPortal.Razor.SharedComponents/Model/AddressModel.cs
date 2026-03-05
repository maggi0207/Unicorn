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
    /// Country name. Defaults to "United States".
    /// </summary>
    [Required(ErrorMessage = "Country is required.")]
    public string? Country { get; set; } = "United States";

    /// <summary>
    /// Primary address line. Maps to "Address Line 2" in SUITES backend.
    /// </summary>
    [Required(ErrorMessage = "Street Address is required.")]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// Secondary address line (optional). Maps to "Address Line 1" in SUITES backend.
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City name.
    /// </summary>
    [Required(ErrorMessage = "City is required.")]
    public string? City { get; set; }

    /// <summary>
    /// State abbreviation (e.g., "WI", "CA").
    /// </summary>
    [Required(ErrorMessage = "State is required.")]
    public string? State { get; set; }

    /// <summary>
    /// 5-digit ZIP code.
    /// </summary>
    [Required(ErrorMessage = "Zip Code is required.")]
    public string? Zip { get; set; }

    /// <summary>
    /// Optional ZIP+4 extension.
    /// </summary>
    public string? Extension { get; set; }

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
        new() { Value = "Mexico", Text = "Mexico" },
    ];
}
