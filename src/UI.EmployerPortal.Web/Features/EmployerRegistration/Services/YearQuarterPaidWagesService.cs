using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;


/// <summary>
/// From a starting date, calculates all the quarters until the current date
/// </summary>
public class YearQuarterPaidWagesService : IYearQuarterPaidWagesService
{

    private readonly Dictionary<string, YearQuartersPaidWages> _yearQuarterPaidWages = new();

    /// <summary>
    /// 
    /// </summary>
    public DateTime EndDate { get; set; } = DateTime.Now;

    /// <summary>
    /// If there is a FUTA liability then a value greater than zero is required
    /// </summary>
    public bool WageEntryRequired { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="required"></param>
    /// <returns></returns>
    public void Update(bool required)
    {
        WageEntryRequired = required;
        foreach (var wageEntry in _yearQuarterPaidWages.Values)
        {
            wageEntry.WageEntryRequired = required;
        }
    }


    /// <summary>
    /// Returns a list by year of each quarter and wether or not the wage information is needed for each quarter
    /// </summary>
    /// <param name="dateFirstPaidWages"></param>
    /// <returns></returns>
    public List<YearQuartersPaidWages> GetYearsAndQuartersPaidWages(DateTime? dateFirstPaidWages)
    {
        var currentDate = dateFirstPaidWages;

        while (currentDate.HasValue && currentDate.Value < EndDate)
        {
            var quarter = GetQuarter(currentDate.Value);
            var year = currentDate.Value.Year.ToString();

            Add(year, quarter);

            currentDate = currentDate.Value.AddMonths(3);
        }

        return GetYearQuartersPaidWages();
    }

    private void Add(string year, string quarter)
    {
        if (!_yearQuarterPaidWages.ContainsKey(year))
        {
            _yearQuarterPaidWages.Add(year, new YearQuartersPaidWages(year, quarter));
        }
        else
        {
            _yearQuarterPaidWages[year].SetQuarter(quarter);
        }
    }

    private List<YearQuartersPaidWages> GetYearQuartersPaidWages()
    {
        return _yearQuarterPaidWages == null || _yearQuarterPaidWages.Count == 0
            ? new List<YearQuartersPaidWages>()
            : new List<YearQuartersPaidWages>(_yearQuarterPaidWages.Values);
    }

    private string GetQuarter(DateTime date)
    {
        return date.Month switch
        {
            1 or 2 or 3 => "Q1",
            4 or 5 or 6 => "Q2",
            7 or 8 or 9 => "Q3",
            10 or 11 or 12 => "Q4",
            _ => String.Empty
        };
    }

    /// <summary>
    /// Determines if the wages entered for any quarter meed the minimum amount for the Business Category.
    /// </summary>
    /// <param name="businessCategory"></param>
    /// <param name="paidWages"></param>
    /// <returns></returns>
    public bool PaidWagesMeetsQuarterlyMinimum(BusinessCategory businessCategory, IEnumerable<YearQuartersPaidWages> paidWages)
    {
        const decimal CommercialMinimum = 1500L;
        const decimal DomesticMinimum = 1000L;
        const decimal AgriculturalMinimum = 20000L;
        const decimal NonProfit501c3Minimum = 1500L;
        const decimal NonProfitOtherMinimum = 1500L;

        var list = new List<decimal?>();

        list.AddRange(GetQuarterlyWages(paidWages, q =>
        {
            return q.Q1Wages;
        }));

        list.AddRange(GetQuarterlyWages(paidWages, q =>
        {
            return q.Q2Wages;
        }));

        list.AddRange(GetQuarterlyWages(paidWages, q =>
        {
            return q.Q3Wages;
        }));

        list.AddRange(GetQuarterlyWages(paidWages, q =>
        {
            return q.Q4Wages;
        }));

        var max = list.Where(w =>
        {
            return w.HasValue;
        }).Max();

        return businessCategory switch
        {
            BusinessCategory.Commercial => max >= CommercialMinimum,
            BusinessCategory.Domestic => max >= DomesticMinimum,
            BusinessCategory.Agricultural => max >= AgriculturalMinimum,
            BusinessCategory.NonProfit_501c3 => max >= NonProfit501c3Minimum,
            BusinessCategory.NonProfit_Other => max >= NonProfitOtherMinimum,
            _ => false
        };
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="businessCategory"></param>
    /// <param name="paidWages"></param>
    /// <returns></returns>
    public QuarterYear GetQuarterYearWagesPaid(BusinessCategory businessCategory, IEnumerable<YearQuartersPaidWages> paidWages)
    {
        var minimumQualifyingWage = GetMinimunQualifyingWages(businessCategory);

        foreach (var wage in paidWages)
        {
            if (wage.Q1Wages.HasValue && wage.Q1Wages.Value >= minimumQualifyingWage)
            {
                return new QuarterYear() { Quarter = 1, Year = wage.GetYear() };
            }

            if (wage.Q2Wages.HasValue && wage.Q2Wages.Value >= minimumQualifyingWage)
            {
                return new QuarterYear() { Quarter = 2, Year = wage.GetYear() };
            }

            if (wage.Q3Wages.HasValue && wage.Q3Wages.Value >= minimumQualifyingWage)
            {
                return new QuarterYear() { Quarter = 3, Year = wage.GetYear() };
            }

            if (wage.Q4Wages.HasValue && wage.Q4Wages.Value >= minimumQualifyingWage)
            {
                return new QuarterYear() { Quarter = 4, Year = wage.GetYear() };
            }

        }

        return new QuarterYear();
    }

    private decimal GetMinimunQualifyingWages(BusinessCategory businessCategory)
    {
        const decimal CommercialMinimum = 1500L;
        const decimal DomesticMinimum = 1000L;
        const decimal AgriculturalMinimum = 20000L;
        const decimal NonProfit501c3Minimum = 1500L;
        const decimal NonProfitOtherMinimum = 1500L;

        return businessCategory switch
        {
            BusinessCategory.Commercial => CommercialMinimum,
            BusinessCategory.Domestic => DomesticMinimum,
            BusinessCategory.Agricultural => AgriculturalMinimum,
            BusinessCategory.NonProfit_501c3 => NonProfit501c3Minimum,
            BusinessCategory.NonProfit_Other => NonProfitOtherMinimum,
            _ => Decimal.Zero
        };
    }

    private IEnumerable<decimal?> GetQuarterlyWages<T>(IEnumerable<T> paidWages, Func<T, decimal?> wageSelector)
    {
        return paidWages
            .Where(w =>
            {
                return wageSelector(w).HasValue;
            })
            .Select(wageSelector);
    }

}
