using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

public class AddressModel
{
    [Required(ErrorMessage = "Country is required.")]
    public string? Country { get; set; } = "United States";

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? AddressLine1 { get; set; }

    public string? AddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? City { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public string? State { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    public string? Zip { get; set; }

    public string? Extension { get; set; }
}
