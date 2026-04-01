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
    /// 
    /// </summary>
    public FuturePayPeriod? ExpectedFuturePayrollPeriod { get; set; } = null;

    /// <summary>
    ///
    /// </summary>
    public bool? HaveSoldOrTransferredBusiness { get; set; } = null;

    /// <summary>
    /// Acknowledgement checkbox shown when the employer confirms they still have paid employees in Wisconsin.
    /// </summary>
    public bool InformationIsAccurate { get; set; } = false;

    /// <summary>
    /// See interface
    /// </summary>
    /// <returns></returns>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (!string.IsNullOrWhiteSpace(FEIN))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FEIN_NUM, _response = FEIN });
        }

        if (!string.IsNullOrWhiteSpace(UIAccountNumber))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_ACCT_NUM, _response = UIAccountNumber });
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

        return responses;
    }
}
