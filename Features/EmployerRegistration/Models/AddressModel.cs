using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

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
    [Required(ErrorMessage = "Address Line 1 is required.")]
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
}
