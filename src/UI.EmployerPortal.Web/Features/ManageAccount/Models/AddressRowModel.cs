namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Represents a single row in the Manage Addresses table.
/// </summary>
public class AddressRowModel
{
    /// <summary>The unique surrogate key for this address record.</summary>
    public long AddressSK { get; set; }

    /// <summary>The address type surrogate key (e.g. 1 = Main Business Mailing Address).</summary>
    public int AddressTypeCodeSK { get; set; }

    /// <summary>The display label for the address type (e.g. "Main Business Mailing Address").</summary>
    public string AddressType { get; set; } = string.Empty;

    /// <summary>The formatted address string for display in the table.</summary>
    public string FormattedAddress { get; set; } = string.Empty;

    /// <summary>
    /// Whether the Main Business Mailing Address is deletable.
    /// Per Figma, the primary mailing address cannot be deleted.
    /// </summary>
    public bool CanDelete { get; set; } = true;

    /// <summary>
    /// The country format code SK: 1=US, 2=Canada, 3=Other International.
    /// Used to determine which save endpoint to call on edit.
    /// </summary>
    public int CountryAddressFormatCodeSK { get; set; } = 1;

    /// <summary>Raw line one of the street address.</summary>
    public string LineOneAddress { get; set; } = string.Empty;

    /// <summary>Raw line two of the street address.</summary>
    public string? LineTwoAddress { get; set; }

    /// <summary>City name.</summary>
    public string? CityName { get; set; }

    /// <summary>State abbreviation (e.g. "WI").</summary>
    public string? StateCode { get; set; }

    /// <summary>State code SK for US addresses.</summary>
    public int? StateCodeSK { get; set; }

    /// <summary>ZIP code.</summary>
    public string? ZipCode { get; set; }

    /// <summary>ZIP extension (the +4 digits).</summary>
    public string? ZipExtensionCode { get; set; }

    /// <summary>Canadian postal code (for Canada addresses).</summary>
    public string? CanadianPostalCode { get; set; }

    /// <summary>Line 3 for international addresses.</summary>
    public string? LineThreeAddress { get; set; }

    /// <summary>Line 4 for international addresses.</summary>
    public string? LineFourAddress { get; set; }

    /// <summary>County name (optional, used in US addresses).</summary>
    public string? CountyName { get; set; }
}
