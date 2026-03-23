using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Provides operations for building and validating the quarterly wage entry table
/// shown during the Unemployment Insurance Subjectivity step.
/// </summary>
public interface IYearQuarterPaidWagesService
{
    /// <summary>
    /// Builds the list of <see cref="WageEntryModel"/> rows covering every calendar year
    /// from the year of <paramref name="dateFirstPaidWages"/> through the current year.
    /// Quarters that precede <paramref name="dateFirstPaidWages"/> are disabled.
    /// </summary>
    /// <param name="dateFirstPaidWages">The date on which the employer first paid wages, or <c>null</c> to use the current date.</param>
    /// <returns>One <see cref="WageEntryModel"/> per calendar year.</returns>
    List<WageEntryModel> GetYearsAndQuartersPaidWages(DateTime? dateFirstPaidWages);

    /// <summary>
    /// Updates internal state in response to a change in the employer's FUTA liability answer.
    /// </summary>
    /// <param name="hasFederalTaxLiability"><c>true</c> if the employer has a FUTA liability outside Wisconsin.</param>
    void Update(bool hasFederalTaxLiability);

    /// <summary>
    /// Returns <c>true</c> when the entered wages satisfy the minimum quarterly threshold
    /// required for the given <paramref name="businessCategory"/>.
    /// </summary>
    /// <param name="businessCategory">The employer's business category.</param>
    /// <param name="wages">The list of quarterly wage rows entered by the employer.</param>
    bool PaidWagesMeetsQuarterlyMinimum(BusinessCategory businessCategory, List<WageEntryModel> wages);
}
