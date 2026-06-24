using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Business Activity form model for the employer registration workflow.
/// </summary>
public class BusinessActivityModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Gets or sets the date the business started or was acquired.
    /// </summary>
    [Required(ErrorMessage = "Date business started or acquired is required.")]
    public DateTime? DateBusinessStarted { get; set; }

    /// <summary>
    /// Gets or sets the date the employer first had paid employees working in Wisconsin.
    /// </summary>
    [Required(ErrorMessage = "Date you first had paid employees working in WI is required.")]
    public DateTime? DateFirstPaidEmployeesInWI { get; set; }

    /// <summary>
    /// Gets or sets the date the employer first paid wages for work performed in Wisconsin.
    /// </summary>
    [Required(ErrorMessage = "Date first paid wages for work performed in WI is required.")]
    public DateTime? DateFirstPaidWagesInWI { get; set; }

    /// <summary>
    /// Gets or sets the principal business activity type from the legacy system catalog.
    /// </summary>
    [Range(1, int.MaxValue, ErrorMessage = "Principal Business Activity is required.")]
    public PrincipalBusinessActivityType PrincipalBusinessActivity { get; set; } = PrincipalBusinessActivityType.None;

    /// <summary>
    /// Gets or sets the description of the employer's primary business activity.
    /// </summary>
    [Required(ErrorMessage = "Primary business activity description is required.")]
    [MaxLength(255, ErrorMessage = "Primary Business Activity Description cannot exceed 255 characters")]
    public string? PrimaryBusinessActivityDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Wisconsin-specific business activity is the same as the primary business activity.
    /// </summary>
    public bool SameAsPrimaryBusinessActivity { get; set; }

    /// <summary>
    /// Gets or sets the description of the employer's Wisconsin-specific business activity.
    /// </summary>
    [MaxLength(255, ErrorMessage = "Wisconsin Specific Business Activity cannot exceed 255 characters")]
    public string? WisconsinSpecificBusinessActivity { get; set; }

    ///<summary>
    ///Does the entity supply temporary workers to clients?
    /// </summary>
    public bool? SuppliesTemporaryWorkers { get; set; }

    ///<summary>
    ///Does the entity provide leased employees under one account?
    /// </summary>
    public bool? ProvidesEmployeeLeasing { get; set; }

    ///<summary>
    ///Explanantion when Employer Service is selected
    /// </summary>
    public string? EmployerServiceExplanation { get; set; }

    ///<summary>
    ///Own employees/client employees or a combination of both
    /// </summary>
    public string? EmployeeType { get; set; }

    ///<summary>
    ///No.of employees currently performing services in Wisconsin
    /// </summary>
    [Required(ErrorMessage = "You must enter the number of employees performing services in Wisconsin")]
    public string? EmployeeCount { get; set; }

    ///<summary>
    ///Services performed by employee in Wisconsin
    /// </summary>
    public string? ServicesDescription { get; set; }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses) { }

    /// <inheritdoc/>
    public void PutAddressSKs(RegistrationAddressProxy[] addresses) { }

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts) { }

    /// <summary>
    /// GetSurveyResponses
    /// </summary>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (DateBusinessStarted.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.BUS_ACQ_DT, _response = DateBusinessStarted.Value.ToString("MM/dd/yyyy") });
        }
        if (DateFirstPaidEmployeesInWI.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FST_EMPL_DT, _response = DateFirstPaidEmployeesInWI.Value.ToString("MM/dd/yyyy") });
        }
        if (DateFirstPaidWagesInWI.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FST_PYRL_DT, _response = DateFirstPaidWagesInWI.Value.ToString("MM/dd/yyyy") });
        }

        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.RGST_PRIN_ACTV_CD_SK, _response = $"{(int) PrincipalBusinessActivity}", _responseDisplay = PrincipalBusinessActivity.GetDisplayName() });
        if (!string.IsNullOrWhiteSpace(PrimaryBusinessActivityDescription))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRIN_OTHR_ACTV_DSC, _response = PrimaryBusinessActivityDescription });
        }

        if (!string.IsNullOrWhiteSpace(WisconsinSpecificBusinessActivity))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRIN_SALE_DSC, _response = WisconsinSpecificBusinessActivity });
        }
        if (SuppliesTemporaryWorkers.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_TMPR_CLNT_CNTRCT, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(SuppliesTemporaryWorkers.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(SuppliesTemporaryWorkers.Value) });
        }
        if (ProvidesEmployeeLeasing.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_NONTMPR_CLNT_CNTRCT, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ProvidesEmployeeLeasing.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ProvidesEmployeeLeasing.Value) });
        }
        if (!string.IsNullOrWhiteSpace(EmployerServiceExplanation))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_REASON, _response = EmployerServiceExplanation });
        }
        if (!string.IsNullOrWhiteSpace(EmployeeType))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_PYRL_SRVC_EE_TYPE, _response = EmployeeType });
        }
        if (!string.IsNullOrWhiteSpace(EmployeeCount))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_PYRL_SRVC_EE_CT, _response = EmployeeCount.ToString() });
        }
        if (!string.IsNullOrWhiteSpace(ServicesDescription))
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_SRVC_PYRL_SRVC_SRVC_TYPE, _response = ServicesDescription });
        }

        return responses;

    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        //if (DateBusinessStarted.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.BUS_ACQ_DT, out var businessAcquiredDate)
            && DateTime.TryParse(businessAcquiredDate.ReplyText, out var businessAcquiredDateValue))
        {
            DateBusinessStarted = businessAcquiredDateValue;
        }

        //if (DateFirstPaidEmployeesInWI.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.FST_EMPL_DT, out var firstEmploymentDate)
            && DateTime.TryParse(firstEmploymentDate.ReplyText, out var firstEmploymentDateValue))
        {
            DateFirstPaidEmployeesInWI = firstEmploymentDateValue;
        }

        //if (DateFirstPaidWagesInWI.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.FST_PYRL_DT, out var firstPayrollDate)
            && DateTime.TryParse(firstPayrollDate.ReplyText, out var firstPayrollDateValue))
        {
            DateFirstPaidWagesInWI = firstPayrollDateValue;
        }

        // PrincipalBusinessActivity
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.RGST_PRIN_ACTV_CD_SK, out var principalBusinessActivity)
            && Enum.TryParse<PrincipalBusinessActivityType>(principalBusinessActivity.ReplyText, out var principalBusinessActivityValue))
        {
            PrincipalBusinessActivity = principalBusinessActivityValue;
        }
        //if (!string.IsNullOrWhiteSpace(PrimaryBusinessActivityDescription))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRIN_SALE_DSC, out var primaryBusinessActivityDescription))
        {
            PrimaryBusinessActivityDescription = primaryBusinessActivityDescription.ReplyText;
        }

        //if (!string.IsNullOrWhiteSpace(WisconsinSpecificBusinessActivity))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRIN_OTHR_ACTV_DSC, out var wisconsinSpecificBusinessActivity))
        {
            WisconsinSpecificBusinessActivity = wisconsinSpecificBusinessActivity.ReplyText;
        }

        //if (SuppliesTemporaryWorkers.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_SRVC_NONTMPR_CLNT_CNTRCT, out var suppliesTemporaryWorkers))
        {
            SuppliesTemporaryWorkers = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(suppliesTemporaryWorkers.ReplyText);
        }

        //if (ProvidesEmployeeLeasing.HasValue)
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_SRVC_NONTMPR_CLNT_CNTRCT, out var providesEmployeeLeasing))
        {
            ProvidesEmployeeLeasing = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(providesEmployeeLeasing.ReplyText);
        }

        //if (!string.IsNullOrWhiteSpace(EmployerServiceExplanantion))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ACQ_BUS_FLG, out var employerServiceExplanation))
        {
            EmployerServiceExplanation = employerServiceExplanation.ReplyText;
        }

        //if (!string.IsNullOrWhiteSpace(EmployeeType))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_SRVC_PYRL_SRVC_EE_TYPE, out var employeeType))
        {
            EmployeeType = employeeType.ReplyText;
        }

        //if (!string.IsNullOrWhiteSpace(EmployeeCount))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_SRVC_PYRL_SRVC_EE_CT, out var employeeCount))
        {
            EmployeeCount = employeeCount.ReplyText;
        }

        //if (!string.IsNullOrWhiteSpace(ServicesDescription))
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_SRVC_PYRL_SRVC_SRVC_TYPE, out var serviceDescription))
        {
            ServicesDescription = serviceDescription.ReplyText;
        }
    }
}
