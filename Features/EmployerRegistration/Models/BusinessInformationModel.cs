using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

public class BusinessInformationModel
{
    #region Business Details

    [Required(ErrorMessage = "FEIN is required.")]
    [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "FEIN must be in format 99-9999999.")]
    public string? FEIN { get; set; }

    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    public string? TradeName { get; set; }

    [Required(ErrorMessage = "Phone Number is required.")]
    [RegularExpression(@"^\d{3}-\d{3}-\d{4}$",
        ErrorMessage = "Phone Number must be in format 999-999-9999.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    #endregion

    #region Mailing Address

    public AddressModel MailingAddress { get; set; } = new();

    #endregion

    #region Physical Locations

    public List<AddressModel> PhysicalLocations { get; set; } = new()
    {
        new AddressModel()
    };

    #endregion
}
