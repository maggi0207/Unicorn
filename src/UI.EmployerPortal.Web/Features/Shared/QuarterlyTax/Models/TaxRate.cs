namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// TaxRateModel
/// </summary>
public class TaxRate
{
    /// <summary>
    /// 
    /// </summary>
    public decimal? AdministrativeFee { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal? AdministrativeFeeProgramIntegrity { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal BasicRate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime EffectiveDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int EmployerSK { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateTime EndDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int RateNoticeCodeSK { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string RateNoticeDescription { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public decimal ReserveFundBalanceRateFactorAmount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal SolvencyRate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int TaxRateYear { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal TaxablePayrollRateFactorAmount { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal TotalRate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int UITaxRateSK { get; set; }
}
