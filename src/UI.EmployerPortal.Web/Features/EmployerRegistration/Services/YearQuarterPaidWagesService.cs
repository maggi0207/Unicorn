using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Default implementation of <see cref="IYearQuarterPaidWagesService"/>.
/// Builds the quarterly wage table and validates the minimum wage thresholds
/// based on the employer's selected business category.
/// </summary>
public class YearQuarterPaidWagesService : IYearQuarterPaidWagesService
{
    /// <summary>Whether the employer has indicated a FUTA liability outside Wisconsin.</summary>
    private bool _hasFederalTaxLiability;

    /// <summary>
    /// Builds wage entry rows from the first-paid-wages year through the current year.
    /// Quarters that fall before <paramref name="dateFirstPaidWages"/> are disabled.
    /// </summary>
    /// <param name="dateFirstPaidWages">Date on which the employer first paid wages.</param>
    /// <returns>One <see cref="WageEntryModel"/> per calendar year.</returns>
    public List<WageEntryModel> GetYearsAndQuartersPaidWages(DateTime? dateFirstPaidWages)
    {
        var startDate = dateFirstPaidWages ?? DateTime.Today;
        var currentYear = DateTime.Today.Year;
        var rows = new List<WageEntryModel>();

        for (var year = startDate.Year; year <= currentYear; year++)
        {
            var row = new WageEntryModel { Year = year };

            // Disable quarters that precede the start date.
            if (year == startDate.Year)
            {
                var startQuarter = GetQuarter(startDate.Month);
                row.Q1Disabled = startQuarter > 1;
                row.Q2Disabled = startQuarter > 2;
                row.Q3Disabled = startQuarter > 3;
                row.Q4Disabled = startQuarter > 4;
            }

            rows.Add(row);
        }

        return rows;
    }

    /// <summary>
    /// Stores the FUTA liability flag for use in downstream logic.
    /// </summary>
    /// <param name="hasFederalTaxLiability">Whether the employer has a FUTA liability outside Wisconsin.</param>
    public void Update(bool hasFederalTaxLiability)
        => _hasFederalTaxLiability = hasFederalTaxLiability;

    /// <summary>
    /// Returns <c>true</c> when at least one enabled quarterly wage entry meets or exceeds
    /// the minimum threshold for the given <paramref name="businessCategory"/>.
    /// </summary>
    /// <param name="businessCategory">The employer's business category.</param>
    /// <param name="wages">The quarterly wage rows entered by the employer.</param>
    public bool PaidWagesMeetsQuarterlyMinimum(BusinessCategory businessCategory, List<WageEntryModel> wages)
    {
        if (wages == null || wages.Count == 0)
            return true;

        var minimum = GetMinimumWage(businessCategory);

        return wages.Any(row =>
            (!row.Q1Disabled && (row.Q1Wages ?? 0) >= minimum) ||
            (!row.Q2Disabled && (row.Q2Wages ?? 0) >= minimum) ||
            (!row.Q3Disabled && (row.Q3Wages ?? 0) >= minimum) ||
            (!row.Q4Disabled && (row.Q4Wages ?? 0) >= minimum));
    }

    /// <summary>Returns the quarterly minimum wage threshold for the given <paramref name="category"/>.</summary>
    /// <param name="category">The employer's business category.</param>
    private static decimal GetMinimumWage(BusinessCategory category)
        => category switch
        {
            BusinessCategory.Domestic => 1_000m,
            BusinessCategory.Agricultural => 20_000m,
            _ => 1_500m,
        };

    /// <summary>Returns the calendar quarter (1–4) for the given <paramref name="month"/>.</summary>
    /// <param name="month">A month number between 1 and 12.</param>
    private static int GetQuarter(int month)
        => (month - 1) / 3 + 1;
}
