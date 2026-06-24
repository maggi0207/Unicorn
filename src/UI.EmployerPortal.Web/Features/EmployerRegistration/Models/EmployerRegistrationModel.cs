using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;


/// <summary>
/// The model for the employer registration process
/// </summary>
public class EmployerRegistrationModel
{
    /// <summary>
    /// The number use to continue a registration
    /// </summary>
    public string SurveyResponseSk { get; set; } = string.Empty;

    /// <summary>
    /// Visible Id of the survey when save and quit has been called
    /// </summary>
    public string SurveyNumber { get; set; } = string.Empty;


    /// <summary>
    /// Step 1: The model for the preliminary questions part of the emmployer registration process
    /// </summary>
    public PreliminaryQuestionsModel PreliminaryQuestionsModel { get; set; } = new();

    /// <summary>
    /// Step 2: The model for the ownership part of the employer registration process
    /// </summary>
    public OwnershipSessionData OwnershipSessionData { get; set; } = new();

    /// <summary>
    /// Step 3: The model for the Business Information as part of the employer registration process
    /// </summary>
    public BusinessInformationModel BusinessInformationModel { get; set; } = new();

    /// <summary>
    /// Step 4: The model for the Business Contacts part of the employer registration process
    /// </summary>
    public BusinessContactModel BusinessContactModel { get; set; } = new();

    /// <summary>
    /// Step 5: The model for the BusinessActivity part of the employer registration process
    /// </summary>
    public BusinessActivityModel BusinessActivityModel { get; set; } = new();

    /// <summary>
    /// Step 6: The model for the UI Subjectivity part of the employer registration process
    /// </summary>
    public SubjectivityModel SubjectivityModel { get; set; } = new();

    /// <summary>
    /// Get an aggregate list of question responses from the child models
    /// </summary>
    /// <returns></returns>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();
        responses.AddRange(GetPreliminaryQuestionsSurveyResponses());
        responses.AddRange(GetOwnershipQuestionsSurveyResponses());
        responses.AddRange(GetBusinessInformationSurveyResponses());
        responses.AddRange(GetBusinessContactSurveyResponses());
        responses.AddRange(GetBusinessActivitySurveyResponses());
        responses.AddRange(SubjectivityModel.GetSurveyResponses());
        return responses;
    }

    /// <summary>
    /// Fill the child models with the response items from the WCF service
    /// </summary>
    /// <param name="responses"></param>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        PreliminaryQuestionsModel.LoadSurveyResponses(responses);
        OwnershipSessionData.LoadSurveyResponses(responses);
        BusinessInformationModel.LoadSurveyResponses(responses);
        BusinessContactModel.LoadSurveyResponses(responses);
        BusinessActivityModel.LoadSurveyResponses(responses);
        SubjectivityModel.LoadSurveyResponses(responses);
    }

    /// <summary>
    /// Get an aggregate list of contacts from the child models
    /// </summary>
    /// <returns></returns>
    public List<SurveyContact> GetSurveyContacts()
    {
        var contacts = new List<SurveyContact>();
        contacts.AddRange(PreliminaryQuestionsModel.GetSurveyContacts());
        contacts.AddRange(OwnershipSessionData.GetSurveyContacts());
        contacts.AddRange(BusinessInformationModel.GetSurveyContacts());
        contacts.AddRange(BusinessContactModel.GetSurveyContacts());
        contacts.AddRange(BusinessActivityModel.GetSurveyContacts());
        contacts.AddRange(SubjectivityModel.GetSurveyContacts());
        return contacts;
    }

    /// <summary>
    /// Calls each of the child models to load their contacts
    /// </summary>
    /// <param name="contacts"></param>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts)
    {
        PreliminaryQuestionsModel.LoadSurveyContacts(contacts);
        OwnershipSessionData.LoadSurveyContacts(contacts);
        BusinessInformationModel.LoadSurveyContacts(contacts);
        BusinessContactModel.LoadSurveyContacts(contacts);
        BusinessActivityModel.LoadSurveyContacts(contacts);
        SubjectivityModel.LoadSurveyContacts(contacts);
    }

    /// <summary>
    /// Get an aggregate list of addresses from the child models
    /// </summary>
    /// <returns></returns>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        var addresses = new List<Tuple<RegistrationAddressCode, AddressModel>>();
        addresses.AddRange(PreliminaryQuestionsModel.GetSurveyAddresses());
        addresses.AddRange(OwnershipSessionData.GetSurveyAddresses());
        addresses.AddRange(BusinessInformationModel.GetSurveyAddresses());
        addresses.AddRange(BusinessContactModel.GetSurveyAddresses());
        addresses.AddRange(BusinessActivityModel.GetSurveyAddresses());
        addresses.AddRange(SubjectivityModel.GetSurveyAddresses());
        return addresses;
    }

    /// <summary>
    /// Fill the child models with the addresses from the WCF service
    /// </summary>
    /// <param name="addresses"></param>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses)
    {
        PreliminaryQuestionsModel.LoadSurveyAddresses(addresses);
        OwnershipSessionData.LoadSurveyAddresses(addresses);
        BusinessInformationModel.LoadSurveyAddresses(addresses);
        BusinessContactModel.LoadSurveyAddresses(addresses);
        BusinessActivityModel.LoadSurveyAddresses(addresses);
        SubjectivityModel.LoadSurveyAddresses(addresses);
    }

    /// <summary>
    /// PreliminaryQuestionsModel
    /// </summary>
    private List<SurveyResponse> GetPreliminaryQuestionsSurveyResponses()
    {
        return PreliminaryQuestionsModel.GetSurveyResponses();
    }
    /// <summary>
    /// OwnershipQuestionsModel
    /// </summary>
    /// <returns></returns>
    private List<SurveyResponse> GetOwnershipQuestionsSurveyResponses()
    {
        return OwnershipSessionData.GetSurveyResponses();
    }
    /// <summary>
    /// BusinessInformationModel
    /// </summary>
    private List<SurveyResponse> GetBusinessInformationSurveyResponses()
    {
        return BusinessInformationModel.GetSurveyResponses();
    }
    /// <summary>
    /// BusinessContactModel
    /// </summary>
    private List<SurveyResponse> GetBusinessContactSurveyResponses()
    {
        return BusinessContactModel.GetSurveyResponses();
    }
    /// <summary>
    /// BusinessActivityModel
    /// </summary>
    private List<SurveyResponse> GetBusinessActivitySurveyResponses()
    {
        return BusinessActivityModel.GetSurveyResponses();
    }

    /// <summary>
    /// Clears the model.
    /// </summary>
    public void Clear()
    {
        SurveyResponseSk = string.Empty;
        SurveyNumber = string.Empty;

        PreliminaryQuestionsModel = new();
        OwnershipSessionData = new();
        BusinessInformationModel = new();
        BusinessContactModel = new();
        BusinessActivityModel = new();
        SubjectivityModel = new();
    }
}
