using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

public class AccountDetailsModel
{
    [Required(ErrorMessage = "FEIN is required.")]
    public string FEIN { get; set; } = string.Empty;

    public string? ReasonForFeinChange { get; set; }

    [Required(ErrorMessage = "Legal Name is required.")]
    public string LegalName { get; set; } = string.Empty;

    public string? ReasonForLegalNameChange { get; set; }

    public string? TradeName { get; set; }

    [Required(ErrorMessage = "Phone Number is required.")]
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Extension { get; set; }

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Invalid Email Address format.")]
    public string EmailAddress { get; set; } = string.Empty;
}
