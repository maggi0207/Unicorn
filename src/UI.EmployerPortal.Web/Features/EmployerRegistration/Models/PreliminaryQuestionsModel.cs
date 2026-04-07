using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// 
/// </summary>
public class PreliminaryQuestionsModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Federal Employer Identification Number
    /// </summary>
    public string FEIN { get; set; } = string.Empty;

    /// <summary>
    /// Unemployment Isurance Employer Account Number
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    ///
    /// </summary>
    public BusinessCategory? BusinessCategory { get; set; } = null;

    /// <summary>
    /// Answer to "Are you a non-profit organization as described in s.501(c)(3) of the IRS code?"
    /// When true the 501(c)(3) sub-tree is shown and Step 6 business category is auto-populated.
    /// </summary>
    public bool? IsNonProfit501c3 { get; set; } = null;

    /// <summary>
    /// Answer to "Do you have a 501(c)(3) ruling from the IRS?"
    /// Shown when IsNonProfit501c3 is true.
    /// </summary>
    public bool? HasRulingFrom501c3IRS { get; set; } = null;

    /// <summary>
    /// Answer to "Have you applied for 501(c)(3) status with the IRS?"
    /// Shown when IsNonProfit501c3 is true and HasRulingFrom501c3IRS is false.
    /// </summary>
    public bool? HasAppliedFor501c3WithIRS { get; set; } = null;

    /// <summary>
    /// Checkbox: "I will supply required documentation at a later date."
    /// Shown on all 501(c)(3) document upload paths as an alternative to uploading immediately.
    /// </summary>
    public bool WillSupplyDocumentationLater { get; set; } = false;

    /// <summary>
    ///
    /// </summary>
    public bool? AcquiredExistingBusiness { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public bool? KnowAcquiredBusinessAccountNumber { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public string AcquiredBusinessAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string AcquiredBusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public AddressModel? AcquiredBusinessAddress { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public bool? HavePaidEmployeesForWorkInWisconsin { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public bool? HaveEmployeesCurrentlyWorkingInWisconsin { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public DateOnly? LastEmploymentDate { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public DateOnly? LastPayrollDate { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public bool? ExpectFuturePayroll { get; set; } = null;

    /// <summary>
    /// Acknowledgement checkbox shown when the employer confirms they still have paid employees in Wisconsin.
    /// </summary>
    public bool InformationIsAccurate { get; set; } = false;

    /// <summary>
    /// 
    /// </summary>
    public FuturePayPeriod? ExpectedFuturePayrollPeriod { get; set; } = null;

    ///// <summary>
    ///// 
    ///// </summary>
    //public bool? HaveSoldOrTransferredBusiness { get; set; } = null;

    /// <summary>
    /// 
    /// </summary>
    public string? NoEmployeeExplanation { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? PEOName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? PEOUIAccountNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? PEOFEIN { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public DateOnly? LeasingStartDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? FiscalAgentName { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? FiscalAgentUIAccountNumber { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string? OtherReason { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public NoEmployeeReason? SelectedNoEmployeeReason { get; set; } = null;


    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        var addresses = new List<Tuple<RegistrationAddressCode, AddressModel>>();

        if (AcquiredBusinessAddress != null)
        {
            addresses.Add(Tuple.Create(RegistrationAddressCode.Acquired_Business, AcquiredBusinessAddress));
        }

        return addresses;
    }

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc />
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (!string.IsNullOrWhiteSpace(FEIN))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FEIN_NUM, _response = FEIN.Replace("-", string.Empty) });
        }

        if (!string.IsNullOrWhiteSpace(UIAccountNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_ACCT_NUM, _response = UIAccountNumber.Replace("-", string.Empty) });
        }

        if (AcquiredExistingBusiness.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(AcquiredExistingBusiness.Value) });
        }

        if (KnowAcquiredBusinessAccountNumber.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_ACCT_NUM_KNWN, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(KnowAcquiredBusinessAccountNumber.Value) });
        }

        if (!string.IsNullOrWhiteSpace(AcquiredBusinessAccountNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_ACCT_NUM, _response = AcquiredBusinessAccountNumber });
        }

        if (!string.IsNullOrWhiteSpace(AcquiredBusinessName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_NAM, _response = AcquiredBusinessName });
        }

        if (HavePaidEmployeesForWorkInWisconsin.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PAID_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HavePaidEmployeesForWorkInWisconsin.Value) });
        }

        if (ExpectFuturePayroll.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectFuturePayroll.Value) });
        }

        if (ExpectedFuturePayrollPeriod.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_TIME, _response = ExpectedFuturePayrollPeriod.Value.ToString() });
        }

        if (HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.STILL_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HaveEmployeesCurrentlyWorkingInWisconsin.Value) });
        }

        if (LastEmploymentDate.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_EMPL_DT, _response = LastEmploymentDate.Value.ToString("MM/dd/yyyy") });
        }

        if (LastPayrollDate.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_PYRL_DT, _response = LastPayrollDate.Value.ToString("MM/dd/yyyy") });
        }
        if (!string.IsNullOrWhiteSpace(PEOName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NAME, _response = PEOName });
        }
        if (!string.IsNullOrWhiteSpace(PEOUIAccountNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOUIAccountNumber });
        }
        if (!string.IsNullOrWhiteSpace(PEOFEIN))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOFEIN });
        }
        if (LeasingStartDate.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_DATE, _response = LeasingStartDate.Value.ToString("MM/dd/yyyy") });
        }
        if (!string.IsNullOrWhiteSpace(NoEmployeeExplanation))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_BUS_WITHOUT_EE, _response = NoEmployeeExplanation });
        }
        if (!string.IsNullOrWhiteSpace(OtherReason))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_OTHR_RSN, _response = OtherReason });
        }
        if (!string.IsNullOrWhiteSpace(FiscalAgentName))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FSCL_AGNT_NAM, _response = FiscalAgentName });
        }
        if (!string.IsNullOrWhiteSpace(FiscalAgentUIAccountNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FA_UI_ACCT_NUM, _response = FiscalAgentUIAccountNumber });
        }
        return responses;
    }
}
