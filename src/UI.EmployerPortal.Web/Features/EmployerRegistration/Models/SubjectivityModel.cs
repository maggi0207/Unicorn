using System.ComponentModel.DataAnnotations;
using Microsoft.FluentUI.AspNetCore.Components.Extensions;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Model for the UISubjectivity page
/// </summary>
public class SubjectivityModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Business Category
    /// </summary>
    public BusinessCategory? BusinessCategory { get; set; }

    /// <summary>
    /// HasAppliedFor501c3Status
    /// </summary>
    // [Required(ErrorMessage = "Applied for 501(c)(3) status is required.")]
    // TODO not sure exactly what is the field it is impacting to visible and not visible . Pls update
    // [RequiredIfVisibleAttribute("TODO", false, ErrorMessage = "Applied for 501(c)(3) status is required.")]
    public bool? HasAppliedFor501c3Status { get; set; } = null;
    /// <summary>
    /// Do you have employees who work in states other than Wisconsin?
    /// </summary>
    //This look like top-level question as per the flow diagram shared in the 559 user story .  Always visible in this flow 
    public bool? HasEmployeesOutsideWisconsin501 { get; set; } = null;
    /// <summary>
    /// Do you have employees who work in states other than Wisconsin?
    /// </summary>     
    //This look like top-level question as per the flow diagram shared in the 559 user story .  Always visible in this flow  
    public bool? HasEmployeesOutsideWisconsin { get; set; } = null;

    /// <summary>
    /// Do you have a Federal Unemployment Tax (FUTA) liability based on payrolls in any state other than Wisconsin?
    /// </summary>
    ///  Visible only when HasEmployeesOutsideWisconsin = Yes    
    public bool? HasFutaLiabilityInOtherStates { get; set; }

    /// <summary>
    /// Did you pay $1,500 or more in wages in a calendar quarter? (Employees path)
    /// (Visible when HasEmployeesOutsideWisconsin = No)
    /// </summary>
    public bool? PaidWagesOver1500Employees { get; set; }

    /// <summary>
    /// Did you pay $1,500 or more in wages or FUTA taxes in a calendar quarter? (Taxes path)
    /// (Visible when HasFutaLiabilityInOtherStates = No)
    /// </summary>
    [RequiredIfVisibleAttribute("HasFutaLiabilityInOtherStates", false, ErrorMessage = "Wages/Taxes in a calendar quarter is required")]
    public bool? PaidWagesOrTaxesOver1500 { get; set; }
    //[RequiredIfVisibleAttribute("PaidWagesOver1500Employees", true, ErrorMessage = "Quarter and Year is required")]
    //[RegularExpression(@"^$|^[1-4]\/\d{4}$", ErrorMessage = "Format must be q/yyyy")]
    //public string? FirstWageQuarterYearEmployees { get; set; }
    /// <summary>
    /// 
    /// </summary>
    /// Visible only when PaidWagesOrTaxesOver1500 = Yes
    [RequiredIfVisibleAttribute("PaidWagesOrTaxesOver1500", true, ErrorMessage = "Quarter and Year is required.")]
    [RegularExpression(@"^$|^[1-4]\/\d{4}$", ErrorMessage = "Quarter and Year must be in the format q/yyyy")]
    public string? QuarterYearFirstPaidTaxes { get; set; }
    /// <summary>
    /// Did you have at least one employee for any part of a day in 20 different weeks?
    /// (Visible when PaidWagesOrTaxesOver1500 = No)
    /// </summary>
    public bool? HasEmployeeIn20Weeks { get; set; }

    /// <summary>
    /// Date the 20th week ended (must be Saturday)
    /// (Visible when HasEmployeeIn20Weeks = Yes) 
    /// </summary>
    [RequiredIfVisibleAttribute("HasEmployeeIn20Weeks", true, ErrorMessage = "The date is not valid. Must be the week ending date of the 20th week. Format example: mm/dd/yyyy.")]
    [SaturdayOnly(ErrorMessage = "The date is not valid. Must be the week ending date of the 20th week. Format example: mm/dd/yyyy.")]
    public DateTime? Week20EndDate { get; set; }

    /// <summary>
    /// HasHasFutaLiabilityInOtherStatesOutsideWisconsin
    /// </summary>
    public bool? HasFutaLiabilityInOtherStatesOutsideWisconsin { get; set; }
    /// <summary>
    /// FinancialInstitution
    /// </summary>
    public AddressModel FinancialInstitution { get; set; } = new();
    /// <summary>
    /// 
    /// </summary>
    public bool? ExpectToPayWagesInAQuarter { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public bool? ExpectToHaveWagesInAQuarter { get; set; }
    /// <summary>
    /// 
    /// </summary>
    private string _whenExpectToHaveWagesInAQuarter = String.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string WhenExpectToHaveWagesInAQuarter
    {
        get => _whenExpectToHaveWagesInAQuarter;
        set => _whenExpectToHaveWagesInAQuarter = ParseLegacyWagesString(value);
    }

    private string _whenExpectToPayWagesInAQuarter = String.Empty;
    /// <summary>
    /// 
    /// </summary>
    public string WhenExpectToPayWagesInAQuarter
    {
        get => _whenExpectToPayWagesInAQuarter;
        set => _whenExpectToPayWagesInAQuarter = ParseLegacyWagesString(value);
    }

    private static string ParseLegacyWagesString(string value)
    {
        return value switch
        {
            "Within 30 days" => "1",
            "30 to 90 days" => "2",
            "6 months" => "3",
            "One year" => "4",
            "More than a year" => "5",
            _ => value
        };
    }

    /// <summary>
    /// 
    /// </summary>
    public List<YearQuartersPaidWages> Wages { get; set; } = new();

    /// <inheritdoc/>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (BusinessCategory.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.BUS_CAT_TXT, _response = ((int) BusinessCategory.Value).ToString(), _responseDisplay = BusinessCategory.Value.ToString() });
        }

        if (!string.IsNullOrWhiteSpace(FinancialInstitution.Name))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.BANK_NAM, _response = FinancialInstitution.Name });
        }

        if (HasEmployeesOutsideWisconsin.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EE_OUTSIDE_WI, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasEmployeesOutsideWisconsin.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasEmployeesOutsideWisconsin.Value) });
        }
        else if (HasEmployeesOutsideWisconsin501.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EE_OUTSIDE_WI, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasEmployeesOutsideWisconsin501.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasEmployeesOutsideWisconsin501.Value) });
        }

        if (BusinessCategory == Models.BusinessCategory.Domestic)
        {
            if (HasEmployeesOutsideWisconsin.HasValue && HasEmployeesOutsideWisconsin.Value
                && HasFutaLiabilityInOtherStates.HasValue) //6.10
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_FUTA_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasFutaLiabilityInOtherStates.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasFutaLiabilityInOtherStates.Value) });
            }

            // duplicate question?
            //if (PaidWagesOrTaxesOver1500.HasValue) //6.11 // have paid wi wages
            //{
            //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_PAID_WI_WGE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(PaidWagesOrTaxesOver1500.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(PaidWagesOrTaxesOver1500.Value) });

            //    if (PaidWagesOrTaxesOver1500.Value && PaidWagesOrTaxesOver1500.HasValue) //6.12 // have paid over 1k in a quarter in a year in wi
            //    {
            //        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_PD_1K_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(PaidWagesOrTaxesOver1500.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(PaidWagesOrTaxesOver1500.Value) });
            //    }
            //}

            if (ExpectToPayWagesPerformWI.HasValue) //6.13) // expect to pay wisconsin wages
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_XPCT_WI_WGE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToPayWagesPerformWI.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToPayWagesPerformWI.Value) });
            }

            if (ExpectToPayWagesInAQuarter.HasValue) //6.14) // expect to pay 1k in a quarter
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_XPT_PY_1K_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToPayWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToPayWagesInAQuarter.Value) });

                if (ExpectToPayWagesInAQuarter.Value && !string.IsNullOrWhiteSpace(WhenExpectToPayWagesInAQuarter)) //6.15) // when expect to pay 1k in a quarter
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.DMSTC_XPT_PY_1K_WHN_TXT, _response = WhenExpectToPayWagesInAQuarter });
                }
            }
        }

        if (BusinessCategory == Models.BusinessCategory.Agricultural)
        {
            if (HasEmployeesOutsideWisconsin.HasValue && HasEmployeesOutsideWisconsin.Value
                && HasFutaLiabilityInOtherStatesOutsideWisconsin.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AG_FTA_LBTY_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasFutaLiabilityInOtherStatesOutsideWisconsin.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasFutaLiabilityInOtherStatesOutsideWisconsin.Value) });
            }

            if (PayWagesPerformWI.HasValue) //6.21) // have futa has paid ag wages in wisconsin
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AFL_WI_WGS_PD_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(PayWagesPerformWI.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(PayWagesPerformWI.Value) });
            }

            if (ExpectToPayWagesPerformWI.HasValue) //6.22) // have futa expect to pay ag wages in wisconsin
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AFL_XPCT_PY_WI_WGS_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToPayWagesPerformWI.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToPayWagesPerformWI.Value) });

                if (ExpectToPayWagesPerformWI.Value && !string.IsNullOrWhiteSpace(WhenExpectToPayWagesInAQuarter)) //6.23) // have futa when expect future ag wages in wisconsin
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AFL_XPCT_PY_WI_WGS_WHN_TXT, _response = WhenExpectToPayWagesInAQuarter });
                }
            }

            if (PaidWagesOver1500Employees.HasValue) //6.24) // 20k cash wages for ag labor in quarter
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AG_PD_20K_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(PaidWagesOver1500Employees.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(PaidWagesOver1500Employees.Value) });
            }

            if (HasEmployeeIn20Weeks.HasValue) //6.25) // at least 10 employees for 20 weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AG_10_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasEmployeeIn20Weeks.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasEmployeeIn20Weeks.Value) });

                if (HasEmployeeIn20Weeks.Value && Week20EndDate.HasValue) //6.26) // when 20th week of 10 employees in 20 weeks in year
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AG_10_IN_20_WK20_END_DT, _response = Week20EndDate.Value.ToString("MM/dd/yyyy") });
                }
            }

            if (ExpectToPayWagesInAQuarter.HasValue) //6.27) // expect to pay 20k ag wages in quarter
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AX_PY_20K_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToPayWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToPayWagesInAQuarter.Value) });
            }

            if (ExpectToHaveWagesInAQuarter.HasValue) //6.28) // ag expect to have at least 10 employees for 20 weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AX_10_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToHaveWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToHaveWagesInAQuarter.Value) });
            }

            if (!string.IsNullOrWhiteSpace(WhenExpectToPayWagesInAQuarter)) //6.29) // ag when expect to pay 20k or have 10 employees for 20 weeks
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AX_20K_10IN20_WHN_TXT, _response = WhenExpectToPayWagesInAQuarter });
            }
            else if (!string.IsNullOrWhiteSpace(WhenExpectToHaveWagesInAQuarter))
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.AX_20K_10IN20_WHN_TXT, _response = WhenExpectToHaveWagesInAQuarter });
            }
        }

        if (BusinessCategory == Models.BusinessCategory.Commercial)
        {
            if (HasEmployeesOutsideWisconsin.HasValue && HasEmployeesOutsideWisconsin.Value
                && HasFutaLiabilityInOtherStates.HasValue) // commercial employer with futa payroll outside wisconsin
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CFTA_LBTY_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasFutaLiabilityInOtherStates.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasFutaLiabilityInOtherStates.Value) });
            }

            if (PaidWagesOver1500Employees.HasValue) // commercial employer has paid 1500 in quarter
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CMCL_PD_1500_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(PaidWagesOver1500Employees.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(PaidWagesOver1500Employees.Value) });
            }

            if (HasEmployeeIn20Weeks.HasValue) //6.32) // commercial employer had at least one employee during 20 different weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CMCL_1_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasEmployeeIn20Weeks.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasEmployeeIn20Weeks.Value) });

                if (HasEmployeeIn20Weeks.Value && Week20EndDate.HasValue) //6.33) // date of end of 20th week of having one employee for 20 different weeks in year
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CMCL_1_IN_20_WK20_END_DT, _response = Week20EndDate.Value.ToString("MM/dd/yyyy") });
                }
            }

            if (ExpectToPayWagesInAQuarter.HasValue) //6.34) // commercial employer expects to pay at least 1500 in wages in quarter in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CX_1500_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToPayWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToPayWagesInAQuarter.Value) });
            }

            if (ExpectToHaveWagesInAQuarter.HasValue) //6.35) // commercial employer expects to have at least one employee working 20 different weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CX_1_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToHaveWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToHaveWagesInAQuarter.Value) });
            }

            if (!string.IsNullOrWhiteSpace(WhenExpectToPayWagesInAQuarter)
                && Enum.TryParse<FuturePayPeriod>(WhenExpectToPayWagesInAQuarter, out var whenExpectToPayWagesInAQuarterValue)) //6.36) // future period when commercial employer expects to pay at least 1500 in quarter in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CX_1500_1IN20_WHN_TXT, _response = WhenExpectToPayWagesInAQuarter, _responseDisplay = whenExpectToPayWagesInAQuarterValue.GetDisplayName() });
            }
            else if (!string.IsNullOrWhiteSpace(WhenExpectToHaveWagesInAQuarter)
                && Enum.TryParse<FuturePayPeriod>(WhenExpectToHaveWagesInAQuarter, out var whenExpectToHaveWagesInAQuarterValue))
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CX_1500_1IN20_WHN_TXT, _response = WhenExpectToHaveWagesInAQuarter, _responseDisplay = whenExpectToHaveWagesInAQuarterValue.GetDisplayName() });
            }
        }

        if (BusinessCategory.HasValue
            && BusinessCategory.Value is Models.BusinessCategory.NonProfit_501c3 or Models.BusinessCategory.NonProfit_Other)
        {
            if (HasEmployeeIn20Weeks.HasValue) //6.40) // non-profit employer has at least 4 employees working in wisconsin on same day in 20 weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.NP_4_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasEmployeeIn20Weeks.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasEmployeeIn20Weeks.Value) });

                if (HasEmployeeIn20Weeks.Value && Week20EndDate.HasValue) //6.41) // end date of 20th week of non-profit having 4 employees working in wisconsin on same day in 20 weeks in year
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.NP_4_IN_20_END_DT, _response = Week20EndDate.Value.ToString("MM/dd/yyyy") });
                }
            }

            if (ExpectToHaveWagesInAQuarter.HasValue) //6.42) // employer expects to have 4 employees working in wisconsin on same day in 20 weeks in year
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.NP_XPCT_4_IN_20_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectToHaveWagesInAQuarter.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectToHaveWagesInAQuarter.Value) });

                if (ExpectToHaveWagesInAQuarter.Value && !string.IsNullOrWhiteSpace(WhenExpectToHaveWagesInAQuarter)) //6.43) // future period when employer expects to have 4 employees working in wisconsin on same day in 20 weeks in year
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.NP_XPCT_4_IN_20_WHN_TXT, _response = WhenExpectToHaveWagesInAQuarter });
                }
            }
        }

        var orderedWages = Wages.OrderBy(w =>
        {
            return w.Year;
        }).ToList();

        if (orderedWages != null && orderedWages.Count > 0)
        {
            var yearOne = orderedWages[0];

            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_ONE_YR, _response = yearOne.Year! });

            if (yearOne.Q1Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_ONE_Q1_WAGES, _response = yearOne.Q1Wages.Value.ToString() });
            }
            if (yearOne.Q2Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_ONE_Q2_WAGES, _response = yearOne.Q2Wages.Value.ToString() });
            }
            if (yearOne.Q3Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_ONE_Q3_WAGES, _response = yearOne.Q3Wages.Value.ToString() });
            }
            if (yearOne.Q4Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_ONE_Q4_WAGES, _response = yearOne.Q4Wages.Value.ToString() });
            }
        }

        if (orderedWages != null && orderedWages.Count > 1)
        {
            var yearTwo = orderedWages[1];

            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_TWO_YR, _response = yearTwo.Year! });

            if (yearTwo.Q1Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_TWO_Q1_WAGES, _response = yearTwo.Q1Wages.Value.ToString() });
            }
            if (yearTwo.Q2Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_TWO_Q2_WAGES, _response = yearTwo.Q2Wages.Value.ToString() });
            }
            if (yearTwo.Q3Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_TWO_Q3_WAGES, _response = yearTwo.Q3Wages.Value.ToString() });
            }
            if (yearTwo.Q4Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_TWO_Q4_WAGES, _response = yearTwo.Q4Wages.Value.ToString() });
            }
        }

        if (orderedWages != null && orderedWages.Count > 2)
        {
            var yearThree = orderedWages[2];

            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_THREE_YR, _response = yearThree.Year! });

            if (yearThree.Q1Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_THREE_Q1_WAGES, _response = yearThree.Q1Wages.Value.ToString() });
            }
            if (yearThree.Q2Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_THREE_Q2_WAGES, _response = yearThree.Q2Wages.Value.ToString() });
            }
            if (yearThree.Q3Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_THREE_Q3_WAGES, _response = yearThree.Q3Wages.Value.ToString() });
            }
            if (yearThree.Q4Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_THREE_Q4_WAGES, _response = yearThree.Q4Wages.Value.ToString() });
            }
        }

        if (orderedWages != null && orderedWages.Count > 3)
        {
            var yearFour = orderedWages[3];

            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_FOUR_YR, _response = yearFour.Year! });

            if (yearFour.Q1Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_FOUR_Q1_WAGES, _response = yearFour.Q1Wages.Value.ToString() });
            }
            if (yearFour.Q2Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_FOUR_Q2_WAGES, _response = yearFour.Q2Wages.Value.ToString() });
            }
            if (yearFour.Q3Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_FOUR_Q3_WAGES, _response = yearFour.Q3Wages.Value.ToString() });
            }
            if (yearFour.Q4Wages.HasValue)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_YR_FOUR_Q4_WAGES, _response = yearFour.Q4Wages.Value.ToString() });
            }
        }

        return responses;
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        // (BusinessCategory.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BUS_CAT_TXT, out var busCat)
            && Enum.TryParse<BusinessCategory>(busCat.ReplyText, out var busCatValue))
        {
            BusinessCategory = busCatValue;
        }

        // (!string.IsNullOrWhiteSpace(FinancialInstitution.Name))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BANK_NAM, out var bankName))
        {
            FinancialInstitution.Name = bankName.ReplyText;
        }

        // (HasEmployeesOutsideWisconsin.HasValue)
        if (BusinessCategory != Models.BusinessCategory.NonProfit_501c3 && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.EE_OUTSIDE_WI, out var eeOutsideWisconsin))
        {
            HasEmployeesOutsideWisconsin = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(eeOutsideWisconsin.ReplyText);
        }
        else if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.EE_OUTSIDE_WI, out var eeOutsideWisconsin501))
        {
            HasEmployeesOutsideWisconsin501 = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(eeOutsideWisconsin501.ReplyText);
        }

        if (BusinessCategory == Models.BusinessCategory.Domestic)
        {
            // 6.10 // (HasFutaLiabilityInOtherStates.HasValue)
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_FUTA_FLG, out var hasFutaLiabilityOutsideWisconsin))
            {
                HasFutaLiabilityInOtherStates = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasFutaLiabilityOutsideWisconsin.ReplyText);
            }

            // duplicate questions?
            //if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_PAID_WI_WGE_FLG, out var hasPaidWagesInWisconsin)) // 6.11
            //{
            //    PaidWagesOrTaxesOver1500 = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasPaidWagesInWisconsin.ReplyText);

            //    if (PaidWagesOrTaxesOver1500.Value && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_PD_1K_FLG, out var hasPaid1kWagesInWisconsin)) // 6.12
            //    {
            //        PaidWagesOrTaxesOver1500 = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasPaid1kWagesInWisconsin.ReplyText);
            //    }
            //}

            // (6.13) // expect to pay wisconsin wages
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_PAID_WI_WGE_FLG, out var hasWisconsinWages))
            {
                ExpectToPayWagesPerformWI = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasWisconsinWages.ReplyText);
            }

            // (6.14) // expect to pay 1k in a quarter
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_XPT_PY_1K_FLG, out var expectToPay1000InQuarter))
            {
                ExpectToPayWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(expectToPay1000InQuarter.ReplyText);

                // (6.15) // when expect to pay 1k in a quarter
                if (ExpectToPayWagesInAQuarter.Value && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.DMSTC_XPT_PY_1K_WHN_TXT, out var whenExpectToPay1kInQuarter))
                {
                    WhenExpectToPayWagesInAQuarter = whenExpectToPay1kInQuarter.ReplyText;
                }
            }
        }

        if (BusinessCategory == Models.BusinessCategory.Agricultural)
        {
            // (HasFutaLiabilityInOtherStatesOutsideWisconsin.HasValue)
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AG_FTA_LBTY_FLG, out var hasFutaLiabilityInStatesOutsideWisconsin2))
            {
                HasFutaLiabilityInOtherStatesOutsideWisconsin = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasFutaLiabilityInStatesOutsideWisconsin2.ReplyText);
            }

            // (6.21) // has paid futa ag wages in wisconsin
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AFL_WI_WGS_PD_FLG, out var paidFutaAgWagesInWisconsin))
            {
                PayWagesPerformWI = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(paidFutaAgWagesInWisconsin.ReplyText);
            }

            // (6.22) // expect to pay futa ag wages in wisconsin
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AFL_XPCT_PY_WI_WGS_FLG, out var expectToPayFutaWagesInWisconsin))
            {
                ExpectToPayWagesPerformWI = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(expectToPayFutaWagesInWisconsin.ReplyText);

                // (6.23) // when expect future futa ag wages in wisconsin
                if (ExpectToPayWagesPerformWI.Value && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BUS_CAT_TXT, out var whenExpectFutreFutaAgWagesInWisconsin))
                {
                    WhenExpectToPayWagesInAQuarter = whenExpectFutreFutaAgWagesInWisconsin.ReplyText;
                }
            }

            // (6.24) // 20k cash wages for ag labor in quarter
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AG_PD_20K_FLG, out var havePaid20kCashWagesForAgLaborInQuarter))
            {
                PaidWagesOver1500Employees = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(havePaid20kCashWagesForAgLaborInQuarter.ReplyText);
            }

            // (6.25) // at least 10 ag employees for 20 weeks in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AG_10_IN_20_FLG, out var haveAtLeast10EmployeesFor20WeeksInYear))
            {
                HasEmployeeIn20Weeks = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(haveAtLeast10EmployeesFor20WeeksInYear.ReplyText);

                // (6.26) // when 20th week of 10 ag employees in 20 weeks in year
                if (HasEmployeeIn20Weeks.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AG_10_IN_20_WK20_END_DT, out var when20thWeekOf20EmployeesIn20WeeksInYear)
                    && DateTime.TryParse(when20thWeekOf20EmployeesIn20WeeksInYear.ReplyText, out var when20thWeekOf20EmployeesIn20WeeksInYearValue))
                {
                    // probably an enum, fix here and in the get mapping: example on 381
                    Week20EndDate = when20thWeekOf20EmployeesIn20WeeksInYearValue;
                }
            }

            // (6.27) // expect to pay 20k ag wages in quarter
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AX_PY_20K_FLG, out var expectToPay20kAgWagesInQuarter))
            {
                ExpectToPayWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(expectToPay20kAgWagesInQuarter.ReplyText);
            }

            // (6.28) // ag expect to have at least 10 employees for 20 weeks in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AX_PY_20K_FLG, out var expectToHaveAtLeast10AgEmployeesFor20WeeksInYear))
            {
                ExpectToHaveWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(expectToHaveAtLeast10AgEmployeesFor20WeeksInYear.ReplyText);
            }

            // (6.29) // ag when expect to pay 20k or have 10 employees for 20 weeks
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BUS_CAT_TXT, out var whenExpectToHaveAtLeast10AgEmployeesFor20WeeksInYear))
            {
                if (ExpectToPayWagesInAQuarter.HasValue && ExpectToPayWagesInAQuarter.Value)
                {
                    WhenExpectToPayWagesInAQuarter = whenExpectToHaveAtLeast10AgEmployeesFor20WeeksInYear.ReplyText;
                }
                else if (ExpectToHaveWagesInAQuarter.HasValue && ExpectToHaveWagesInAQuarter.Value)
                {
                    WhenExpectToHaveWagesInAQuarter = whenExpectToHaveAtLeast10AgEmployeesFor20WeeksInYear.ReplyText;
                }
            }
        }

        if (BusinessCategory == Models.BusinessCategory.Commercial)
        {
            // commercial employer with futa payroll outside wisconsin
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CFTA_LBTY_FLG, out var commercialEmployerHasFutaPayrollOutsideWisconsin))
            {
                HasFutaLiabilityInOtherStatesOutsideWisconsin = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(commercialEmployerHasFutaPayrollOutsideWisconsin.ReplyText);
            }

            // commercial employer has paid 1500 in quarter
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CMCL_PD_1500_FLG, out var commercialEmployerHasPaid1500InQuarter))
            {
                PaidWagesOver1500Employees = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(commercialEmployerHasPaid1500InQuarter.ReplyText);
            }

            // (6.32) // commercial employer had at least one employee during 20 different weeks in year
            if (BusinessCategory == Models.BusinessCategory.Commercial
                && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CMCL_1_IN_20_FLG, out var commercialEmployerHadAtLeastOneEmployeeFor20WeeksInYear))
            {
                HasEmployeeIn20Weeks = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(commercialEmployerHadAtLeastOneEmployeeFor20WeeksInYear.ReplyText);

                // (6.33) // date of end of 20th week of having one employee for 20 different weeks in year
                if (HasEmployeeIn20Weeks.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.AG_10_IN_20_WK20_END_DT, out var when20thWeekWithOneCommercialEmployeeFor20WeeksInYear)
                    && DateTime.TryParse(when20thWeekWithOneCommercialEmployeeFor20WeeksInYear.ReplyText, out var when20thWeekWithOneCommercialEmployeeFor20WeeksInYearValue))
                {
                    Week20EndDate = when20thWeekWithOneCommercialEmployeeFor20WeeksInYearValue;
                }
            }

            // (6.34) // commercial employer expects to pay at least 1500 in wages in quarter in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CX_1500_FLG, out var commercialEmployerExpectsToPay1500InQuarterInYear))
            {
                ExpectToPayWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(commercialEmployerExpectsToPay1500InQuarterInYear.ReplyText);
            }

            // (6.35) // commercial employer expects to have at least one employee working 20 different weeks in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CX_1_IN_20_FLG, out var commercialEmployerExpectsToOneEmployeeFor20WeeksInYear))
            {
                ExpectToHaveWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(commercialEmployerExpectsToOneEmployeeFor20WeeksInYear.ReplyText);
            }

            // (6.36) // future period when commercial employer expects to pay at least 1500 in quarter in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CX_1500_1IN20_WHN_TXT, out var whenExpectToPay1500CommercialWagesInQuarter))
            {
                if (ExpectToPayWagesInAQuarter.HasValue && ExpectToPayWagesInAQuarter.Value)
                {
                    WhenExpectToPayWagesInAQuarter = whenExpectToPay1500CommercialWagesInQuarter.ReplyText;
                }
                else if (ExpectToHaveWagesInAQuarter.HasValue && ExpectToHaveWagesInAQuarter.Value)
                {
                    WhenExpectToHaveWagesInAQuarter = whenExpectToPay1500CommercialWagesInQuarter.ReplyText;
                }
            }
        }

        if (BusinessCategory is Models.BusinessCategory.NonProfit_501c3 or Models.BusinessCategory.NonProfit_Other)
        {
            // (6.40) // non-profit employer has at least 4 employees working in wisconsin on same day in 20 weeks in year
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.NP_4_IN_20_FLG, out var nonProfit4EmployeesForSameDayIn20WeeksInYear))
            {
                HasEmployeeIn20Weeks = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(nonProfit4EmployeesForSameDayIn20WeeksInYear.ReplyText);

                // (6.41) // end date of 20th week of non-profit having 4 employees working in wisconsin on same day in 20 weeks in year
                if (HasEmployeeIn20Weeks.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.NP_4_IN_20_END_DT, out var whenNonProfit4EmployeesForSameDayIn20WeeksInYear)
                    && DateTime.TryParse(whenNonProfit4EmployeesForSameDayIn20WeeksInYear.ReplyText, out var whenNonProfit4EmployeesForSameDayIn20WeeksInYearValue))
                {
                    // probably an enum, fix here and in the get mapping: example on 381
                    Week20EndDate = whenNonProfit4EmployeesForSameDayIn20WeeksInYearValue;
                }
            }

            // (6.42) // non profit employer expects to have 4 employees working in wisconsin on same day in 20 weeks in year
            if ((BusinessCategory == Models.BusinessCategory.NonProfit_501c3 || BusinessCategory == Models.BusinessCategory.NonProfit_Other)
                && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.NP_XPCT_4_IN_20_FLG, out var nonProfitExpects4EmployeesForSameDayIn20WeeksInYear))
            {
                ExpectToHaveWagesInAQuarter = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(nonProfitExpects4EmployeesForSameDayIn20WeeksInYear.ReplyText);

                //if (6.43) // future period when employer expects to have 4 employees working in wisconsin on same day in 20 weeks in year
                if (ExpectToHaveWagesInAQuarter.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.NP_XPCT_4_IN_20_WHN_TXT, out var whenNonProfitExpects4EmployeesForSameDayIn20WeeksInYear))
                {
                    // probably not dateonly, will have to fix in get mapping
                    WhenExpectToHaveWagesInAQuarter = whenNonProfitExpects4EmployeesForSameDayIn20WeeksInYear.ReplyText;
                }
            }
        }

        // year one wages
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_ONE_YR, out var yearOne))
        {
            var yearOneWages = new YearQuartersPaidWages()
            {
                Year = yearOne.ReplyText,
            };

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_ONE_Q1_WAGES, out var yearOneQ1Wages)
                && decimal.TryParse(yearOneQ1Wages.ReplyText, out var yearOneQ1WagesValue))
            {
                yearOneWages.Q1Wages = yearOneQ1WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_ONE_Q2_WAGES, out var yearOneQ2Wages)
                && decimal.TryParse(yearOneQ2Wages.ReplyText, out var yearOneQ2WagesValue))
            {
                yearOneWages.Q2Wages = yearOneQ2WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_ONE_Q3_WAGES, out var yearOneQ3Wages)
                && decimal.TryParse(yearOneQ3Wages.ReplyText, out var yearOneQ3WagesValue))
            {
                yearOneWages.Q1Wages = yearOneQ3WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_ONE_Q4_WAGES, out var yearOneQ4Wages)
                && decimal.TryParse(yearOneQ4Wages.ReplyText, out var yearOneQ4WagesValue))
            {
                yearOneWages.Q1Wages = yearOneQ4WagesValue;
            }

            Wages.Add(yearOneWages);
        }

        // year two wages
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_TWO_YR, out var yearTwo))
        {
            var yearTwoWages = new YearQuartersPaidWages()
            {
                Year = yearTwo.ReplyText,
            };

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_TWO_Q1_WAGES, out var yearTwoQ1Wages)
                && decimal.TryParse(yearTwoQ1Wages.ReplyText, out var yearTwoQ1WagesValue))
            {
                yearTwoWages.Q1Wages = yearTwoQ1WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_TWO_Q2_WAGES, out var yearTwoQ2Wages)
                && decimal.TryParse(yearTwoQ2Wages.ReplyText, out var yearTwoQ2WagesValue))
            {
                yearTwoWages.Q2Wages = yearTwoQ2WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_TWO_Q3_WAGES, out var yearTwoQ3Wages)
                && decimal.TryParse(yearTwoQ3Wages.ReplyText, out var yearTwoQ3WagesValue))
            {
                yearTwoWages.Q1Wages = yearTwoQ3WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_TWO_Q4_WAGES, out var yearTwoQ4Wages)
                && decimal.TryParse(yearTwoQ4Wages.ReplyText, out var yearTwoQ4WagesValue))
            {
                yearTwoWages.Q1Wages = yearTwoQ4WagesValue;
            }

            Wages.Add(yearTwoWages);
        }

        // year three wages
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_YR, out var yearThree))
        {
            var yearThreeWages = new YearQuartersPaidWages()
            {
                Year = yearThree.ReplyText,
            };

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q1_WAGES, out var yearThreeQ1Wages)
                && decimal.TryParse(yearThreeQ1Wages.ReplyText, out var yearThreeQ1WagesValue))
            {
                yearThreeWages.Q1Wages = yearThreeQ1WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q2_WAGES, out var yearThreeQ2Wages)
                && decimal.TryParse(yearThreeQ2Wages.ReplyText, out var yearThreeQ2WagesValue))
            {
                yearThreeWages.Q2Wages = yearThreeQ2WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q3_WAGES, out var yearThreeQ3Wages)
                && decimal.TryParse(yearThreeQ3Wages.ReplyText, out var yearThreeQ3WagesValue))
            {
                yearThreeWages.Q1Wages = yearThreeQ3WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q4_WAGES, out var yearThreeQ4Wages)
                && decimal.TryParse(yearThreeQ4Wages.ReplyText, out var yearThreeQ4WagesValue))
            {
                yearThreeWages.Q1Wages = yearThreeQ4WagesValue;
            }

            Wages.Add(yearThreeWages);
        }

        // year four wages
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_YR, out var yearFour))
        {
            var yearFourWages = new YearQuartersPaidWages()
            {
                Year = yearFour.ReplyText,
            };

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q1_WAGES, out var yearFourQ1Wages)
                && decimal.TryParse(yearFourQ1Wages.ReplyText, out var yearFourQ1WagesValue))
            {
                yearFourWages.Q1Wages = yearFourQ1WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q2_WAGES, out var yearFourQ2Wages)
                && decimal.TryParse(yearFourQ2Wages.ReplyText, out var yearFourQ2WagesValue))
            {
                yearFourWages.Q2Wages = yearFourQ2WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q3_WAGES, out var yearFourQ3Wages)
                && decimal.TryParse(yearFourQ3Wages.ReplyText, out var yearFourQ3WagesValue))
            {
                yearFourWages.Q1Wages = yearFourQ3WagesValue;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_YR_THREE_Q4_WAGES, out var yearFourQ4Wages)
                && decimal.TryParse(yearFourQ4Wages.ReplyText, out var yearFourQ4WagesValue))
            {
                yearFourWages.Q1Wages = yearFourQ4WagesValue;
            }

            Wages.Add(yearFourWages);
        }
    }

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts) { }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        var addresses = new List<Tuple<RegistrationAddressCode, AddressModel>>();

        if (!string.IsNullOrWhiteSpace(FinancialInstitution.AddressLine1))
        {
            addresses.Add(Tuple.Create(RegistrationAddressCode.DFI_Received, FinancialInstitution));
        }
        return addresses;
    }
    /// <summary>
    /// 
    /// </summary>
    public string AFL_XPCT_PY_WI_WGS_WHN_TXT { get; set; } = String.Empty;
    /// <summary>
    /// 
    /// </summary>
    public SubjectivityModel() { }
    /// <summary>
    /// Have you paid agricultural wages for work performed in Wisconsin?
    /// (Visible when HasFutaLiabilityInOtherStates = Yes)
    /// </summary>
    public bool? PayWagesPerformWI { get; set; }
    /// <summary>
    /// Do you expect to pay agricultural wages for work in Wisconsin
    /// </summary>
    [RequiredIfVisibleAttribute("ExpectToPayWagesPerformWI", false, ErrorMessage = "When to pay wages for work in Wisconsin?")]
    public bool? ExpectToPayWagesPerformWI { get; set; }
    /// <summary>
    /// NoFutaAggWages
    /// </summary>
    public bool? AX_10_IN_20_FLG { get; set; }
    /// <summary>
    /// NoFutaAggWages
    /// </summary>
    public bool? EmployePayagriculturalWages20k { get; set; }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses)
    {
        // fight need to load financial institution address
        if (IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.DFI_Received, out var financialInstitutionAddress))
        {
            FinancialInstitution = IEmployerRegistrationModelSection.ConvertAddressResponseToModel(financialInstitutionAddress);
        }
    }
}

/// <summary>
/// Determines if the date is a Saturday
/// </summary>
public class SaturdayOnlyAttribute : ValidationAttribute
{

    /// <summary>
    /// Only Valid if the Date is a Saturday
    /// </summary>
    /// <param name="value"></param>
    /// <param name="validationContext"></param>
    /// <returns></returns>
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        return value is DateTime dateTime
            ? dateTime.DayOfWeek == DayOfWeek.Saturday
                ? ValidationResult.Success
                : new ValidationResult(ErrorMessage ?? "The selected date must be a Saturday.", new[] { validationContext.MemberName! })
            : ValidationResult.Success;
    }
}

/// <summary>
/// The 'Name' field may not be visible on the form.  In that event,
/// the value is not required.
/// </summary>
public class RequiredIfVisibleAttribute : ValidationAttribute
{
    private readonly string _isVisible;
    private readonly bool _expectedValue;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="isVisible"></param>
    /// <param name="expectedValue"></param>
    public RequiredIfVisibleAttribute(string isVisible, bool expectedValue)
    {
        _isVisible = isVisible;
        _expectedValue = expectedValue;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="value"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    //protected override ValidationResult? IsValid(object? value, ValidationContext context)
    //{
    //    var instance = context.ObjectInstance;
    //    var isValidationRequired = instance.GetType().GetProperty(_isVisible)?.GetValue(instance, null);

    //    var isRequired = isValidationRequired is not bool || (bool) isValidationRequired;

    //    if (isRequired == _expectedValue)
    //    {
    //        var name = value == null ? String.Empty : value.ToString();
    //        return !String.IsNullOrWhiteSpace(name)
    //            ? ValidationResult.Success
    //            : new ValidationResult(ErrorMessage ?? "Name is required ", new[] { context.MemberName! });
    //    }

    //    return ValidationResult.Success;
    //}

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    protected override ValidationResult? IsValid(object? value, ValidationContext context)
    {
        var instance = context.ObjectInstance;
        var dependentProperty = instance.GetType().GetProperty(_isVisible);
        if (dependentProperty == null)
        {
            return new ValidationResult(
                $"Unknown property: {_isVisible}",
                new[] { context.MemberName! });
        }
        var dependentValue = dependentProperty.GetValue(instance, null);
        // If controlling field is not a bool, skip validation
        if (dependentValue is not bool actualValue)
        {
            return ValidationResult.Success;
        }
        // If this field is NOT visible, do not validate it
        if (actualValue != _expectedValue)
        {
            return ValidationResult.Success;
        }
        // Field is visible, so now it is required
        if (value == null)
        {
            return new ValidationResult(
                ErrorMessage ?? $"{context.MemberName} is required.",
                new[] { context.MemberName! });
        }
        if (value is string text && string.IsNullOrWhiteSpace(text))
        {
            return new ValidationResult(
                ErrorMessage ?? $"{context.MemberName} is required.",
                new[] { context.MemberName! });
        }
        return ValidationResult.Success;
    }

}

