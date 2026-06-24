using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
/// <summary>
/// 
/// </summary>
public interface IQuarterlyTaxCalculatorService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    void Calculate(TaxEntryModel model);
}
/// <summary>
/// Service
/// </summary>
public class QuarterlyTaxCalculatorService
    : IQuarterlyTaxCalculatorService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="model"></param>
    public void Calculate(TaxEntryModel model)
    {
        model.DefinedTaxablePayroll =
            model.TotalGrossCoveredWages - model.ExclusionOverride.EffectiveAmount;

        var calculatedTax =
            model.DefinedTaxablePayroll * model.TaxRate;

        model.TaxAssessed = Math.Round(calculatedTax, 2);
    }
}

