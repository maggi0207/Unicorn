using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

/// <summary>
/// Model backing the UI Account Number + FEIN lookup form shown when no employer is selected.
/// </summary>
public class AuditAccountLookupModel
{
    /// <summary>
    /// UI Account Number
    /// </summary>
    [Required(ErrorMessage = "UI Account Number is required.")]
    [RegularExpression(@"^\d{6}-\d{3}-\d{1}$", ErrorMessage = "UI Account Number must be in format 999999-999-9")]
    public string UIAccountNumber { get; set; } = string.Empty;
    /// <summary>
    /// FEIN for Account Fetch
    /// </summary>
    [Required(ErrorMessage = "FEIN is required.")]
    [RegularExpression(@"^\d{2}-\d{7}$", ErrorMessage = "FEIN must be in format 99-9999999")]
    public string FEIN { get; set; } = string.Empty;
}
