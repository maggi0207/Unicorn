using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

/// <summary>
/// Model for Step 3 (Business Information) of the employer registration wizard.
/// Contains business details, mailing address, and physical location(s).
/// </summary>
public class BusinessInformationModel
{
    #region Business Details

    /// <summary>
    /// Federal Employer Identification Number in format 99-9999999.
    /// </summary>
    [Required(ErrorMessage = "FEIN is required.")]
    [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "FEIN must be in format 99-9999999.")]
    public string? FEIN { get; set; }

    /// <summary>
    /// Legal business name as registered with the state.
    /// </summary>
    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    /// <summary>
    /// Trade name or DBA (optional).
    /// </summary>
    public string? TradeName { get; set; }

    /// <summary>
    /// Business phone number in format 999-999-9999.
    /// </summary>
    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{4}$",
        ErrorMessage = "Phone Number must be in format 999-999-9999.")]
    public string? PhoneNumber { get; set; }

    /// <summary>
    /// Business contact email address.
    /// </summary>
    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    #endregion

    #region Mailing Address

    /// <summary>
    /// Business mailing address.
    /// </summary>
    public AddressModel MailingAddress { get; set; } = new();

    #endregion

    #region Physical Locations

    /// <summary>
    /// Physical business locations. At least one is required; maximum of three allowed.
    /// </summary>
    public List<AddressModel> PhysicalLocations { get; set; } = new()
    {
        new AddressModel()
    };

    #endregion
}
