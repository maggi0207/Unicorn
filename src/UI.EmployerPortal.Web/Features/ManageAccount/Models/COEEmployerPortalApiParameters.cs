namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Employer portal API parameters used by the Corporate Officer Exclusion worksheet labels.
/// </summary>
public class COEEmployerPortalApiParameters
{
    /// <summary>
    /// Gets or sets the currency-formatted FUTA wage threshold for Line 1
    /// </summary>
    public string PayrollSubjectFutaFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted FUTA tax rate for Line 2 (<c>FUTATaxRate</c>), e.g. 6.000.
    /// </summary>
    public string FutaTaxRateFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the formatted max FUTA credit rate for Lines 3 and 7
    /// </summary>
    public string MaxFutaCreditAvailableRateFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the currency-formatted taxable wage base for Line 4 (<c>TaxableWageBase</c>).
    /// </summary>
    public string TaxableWageBaseFormatted { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the raw FUTA tax rate percent value (<c>FUTATaxRate</c>).
    /// </summary>
    public decimal? FutaTaxRate { get; set; }

    /// <summary>
    /// Gets or sets the raw max FUTA credit available rate percent value
    /// </summary>
    public decimal? MaxFutaCreditAvailableRate { get; set; }
}
