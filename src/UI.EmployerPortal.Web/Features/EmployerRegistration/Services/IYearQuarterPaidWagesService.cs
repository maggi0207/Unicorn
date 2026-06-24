using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// From a starting date, calculates all the quarters until the current date
/// </summary>
public interface IYearQuarterPaidWagesService
{
    /// <summary>
    /// Sets and Last Date for the range of the results.  
    /// </summary>
    DateTime EndDate { get; set; }

    /// <summary>
    /// If there is a FUTA liability then a value greater than zero is required
    /// </summary>
    bool WageEntryRequired { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="required"></param>
    void Update(bool required);

    /// <summary>
    /// Returns a list by year of each quarter and wether or not the wage information is needed for each quarter
    /// </summary>
    /// <param name="dateFirstPaidWages"></param>
    /// <returns></returns>
    List<YearQuartersPaidWages> GetYearsAndQuartersPaidWages(DateTime? dateFirstPaidWages);

    /// <summary>
    /// Determines if the wages entered for any quarter meed the minimum amount for the Business Category.
    /// </summary>
    /// <param name="businessCategory"></param>
    /// <param name="paidWages"></param>
    /// <returns></returns>
    bool PaidWagesMeetsQuarterlyMinimum(BusinessCategory businessCategory, IEnumerable<YearQuartersPaidWages> paidWages);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="businessCategory"></param>
    /// <param name="paidWages"></param>
    /// <returns></returns>
    QuarterYear GetQuarterYearWagesPaid(BusinessCategory businessCategory, IEnumerable<YearQuartersPaidWages> paidWages);
}
