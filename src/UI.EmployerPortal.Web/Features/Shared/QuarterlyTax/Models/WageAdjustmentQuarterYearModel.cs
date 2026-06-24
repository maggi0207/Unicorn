using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
///
/// </summary>
public class WageAdjustmentQuarterYearModel
{
    /// <summary>
    /// Quarter Year
    /// </summary>
    [Required(ErrorMessage = "Quarter and Year are required.")]
    public string? QuarterYear { get; set; }
}
