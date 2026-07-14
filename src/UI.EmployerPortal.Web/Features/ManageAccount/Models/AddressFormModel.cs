using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// View model for the Add / Edit address form.
/// </summary>
public class AddressFormModel : IValidatableObject
{
    /// <summary>
    /// The surrogate key of the existing address. Null when adding a new address.
    /// </summary>
    public long? AddressSK { get; set; }

    /// <summary>
    /// The address type surrogate key selected from the dropdown.
    /// </summary>
    public int AddressTypeCodeSK { get; set; }

    /// <summary>
    /// Country format: 1 = United States, 2 = Canada, 3 = Other International.
    /// </summary>
    public int CountryAddressFormatCodeSK { get; set; } = 1;

    public string? AddressTypeString { get; set; }
    public string? CountryString { get; set; } = "1";
    public string? StateString { get; set; }
    public string? ProvinceString { get; set; }

    // ── US / Canada shared ──────────────────────────────────

    /// <summary>Street address line 1.</summary>
    [MaxLength(100)]
    public string LineOneAddress { get; set; } = string.Empty;

    /// <summary>Street address line 2 (optional).</summary>
    [MaxLength(100)]
    public string? LineTwoAddress { get; set; }

    /// <summary>City name (required for US and Canada).</summary>
    [MaxLength(50)]
    public string? CityName { get; set; }

    // ── US-specific ─────────────────────────────────────────

    /// <summary>State code SK — required for US addresses.</summary>
    public int? StateCodeSK { get; set; }

    /// <summary>ZIP code (5 digits) — required for US.</summary>
    [MaxLength(10)]
    public string? ZipCode { get; set; }

    /// <summary>ZIP+4 extension — optional for US.</summary>
    [MaxLength(4)]
    public string? ZipExtension { get; set; }

    /// <summary>County name — optional for US.</summary>
    [MaxLength(50)]
    public string? CountyName { get; set; }

    // ── Canada-specific ─────────────────────────────────────

    /// <summary>Province code SK — required for Canada.</summary>
    public int? ProvinceCodeSK { get; set; }

    /// <summary>Canadian postal code (e.g. "K1A 0A9") — required for Canada.</summary>
    [MaxLength(10)]
    public string? CanadianPostalCode { get; set; }

    // ── Other International ─────────────────────────────────

    /// <summary>Line 3 — International only.</summary>
    [MaxLength(100)]
    public string? LineThreeAddress { get; set; }

    /// <summary>Line 4 — International only.</summary>
    public string? LineFourAddress { get; set; }

    /// <summary>
    /// Performs conditional validation for the address form fields based on the selected country format.
    /// </summary>
    /// <param name="validationContext">The validation context.</param>
    /// <returns>A collection of validation results for any failed rules.</returns>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrWhiteSpace(AddressTypeString) || !int.TryParse(AddressTypeString, out int at) || at <= 0)
        {
            yield return new ValidationResult("Please select an Address Type.", new[] { nameof(AddressTypeString) });
        }

        if (string.IsNullOrWhiteSpace(CountryString) || !int.TryParse(CountryString, out int c) || c < 1 || c > 3)
        {
            yield return new ValidationResult("Please select a Country.", new[] { nameof(CountryString) });
        }

        // 1 = US, 2 = Canada, 3 = Other International
        
        if (string.IsNullOrWhiteSpace(LineOneAddress))
        {
            yield return new ValidationResult("Address Line 1 is required.", new[] { nameof(LineOneAddress) });
        }

        // City is required for US and Canada
        if (CountryAddressFormatCodeSK is 1 or 2)
        {
            if (string.IsNullOrWhiteSpace(CityName))
            {
                yield return new ValidationResult("City is required.", new[] { nameof(CityName) });
            }
        }

        // US specific required fields
        if (CountryAddressFormatCodeSK == 1)
        {
            if (string.IsNullOrWhiteSpace(StateString) || !int.TryParse(StateString, out int st) || st <= 0)
            {
                yield return new ValidationResult("State is required.", new[] { nameof(StateString) });
            }
            if (string.IsNullOrWhiteSpace(ZipCode))
            {
                yield return new ValidationResult("Zip Code is required.", new[] { nameof(ZipCode) });
            }
        }

        // Canada specific required fields
        if (CountryAddressFormatCodeSK == 2)
        {
            if (string.IsNullOrWhiteSpace(ProvinceString) || !int.TryParse(ProvinceString, out int pr) || pr <= 0)
            {
                yield return new ValidationResult("Province is required.", new[] { nameof(ProvinceString) });
            }
            if (string.IsNullOrWhiteSpace(CanadianPostalCode))
            {
                yield return new ValidationResult("Postal Code is required.", new[] { nameof(CanadianPostalCode) });
            }
        }
    }
}
