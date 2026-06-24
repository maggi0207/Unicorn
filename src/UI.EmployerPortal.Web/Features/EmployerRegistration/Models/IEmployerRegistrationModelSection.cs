using System.Diagnostics.CodeAnalysis;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Interface to make sure all the registration child models implement a similar method for exporting survey resonses
/// </summary>
public interface IEmployerRegistrationModelSection
{
    /// <summary>
    /// This should return a list of the survey responses from their 
    /// </summary>
    /// <returns></returns>
    List<SurveyResponse> GetSurveyResponses();

    /// <summary>
    /// Returns a list of contacts from the Section
    /// </summary>
    /// <returns></returns>
    List<SurveyContact> GetSurveyContacts();

    /// <summary>
    /// Returns a list of addresses from the Section
    /// </summary>
    /// <returns></returns>
    List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses();

    /// <summary>
    /// Used to populate the address SKs after a WCF save has happened
    /// </summary>
    /// <param name="addresses"></param>
    void PutAddressSKs(RegistrationAddressProxy[] addresses);

    /// <summary>
    /// Loads responses into the data model from a list provided by the WCF service.
    /// </summary>
    /// <param name="responses">The list of questions provided by the WCF service.</param>
    void LoadSurveyResponses(SurveyResponseItemProxy[] responses);

    /// <summary>
    /// Loads contacts into the data model from a list provided by the WCF service.
    /// </summary>
    /// <param name="contacts"></param>
    void LoadSurveyContacts(RegistrationIndividualProxy[] contacts);

    /// <summary>
    /// Loads addresses into the data model from a list provided by the WCF service.
    /// </summary>
    /// <param name="addresses">The list of addresses provided by the WCF service.</param>
    void LoadSurveyAddresses(RegistrationAddressProxy[] addresses);

    /// <summary>
    /// Convert a boolean response to the string value expected by the save response API call
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    static string ConvertBooleanResponseToString(bool response)
    {
        return response ? "True" : "False";
    }

    /// <summary>
    /// Convert a boolean response to the string value to be passed to the WCF endpoint
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    static string ConvertBooleanResponseToSummaryString(bool response)
    {
        return response ? "Yes" : "No";
    }

    /// <summary>
    /// Convert a boolean response to the string value to be displayed on teh summary screen
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    static string ConvertBooleanResponseToDisplayString(bool response)
    {
        return response ? "Yes" : "No";
    }

    /// <summary>
    /// Convert a resonse string to the associated boolean value for the models
    /// </summary>
    /// <param name="response">The response string returned by the WCF service</param>
    /// <returns>The boolean value associated with string Yes/No</returns>
    static bool ConvertResponseStringToBoolean(string response)
    {
        return response.ToLowerInvariant() == "true";
    }

    /// <summary>
    /// Format an FEIN string with no punctuation (the WCF service format) to the component format (##-#######)
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    static string FormatFeinResponseString(string response)
    {
        return $"{response[..2]}-{response[2..]}";
    }

    /// <summary>
    /// Format a UI Account Number string with no punctuation (the WCF service format) to the component format (######-###-#)
    /// </summary>
    /// <param name="response"></param>
    /// <returns></returns>
    static string FormatUiAccountNumberResponseString(string response)
    {
        return $"{response[..6]}-{response[6..9]}-{response[9..]}";
    }

    /// <summary>
    /// Returns a string that is the legal name concatenated in forward (firstName lastName) order
    /// </summary>
    /// <param name="firstName"></param>
    /// <param name="middleInitial"></param>
    /// <param name="lastName"></param>
    /// <returns></returns>
    static string ConcatenateLegalName(string firstName, string middleInitial, string lastName)
    {
        return $"{firstName}{(string.IsNullOrWhiteSpace(middleInitial) ? string.Empty : $" {middleInitial}.")} {lastName}";
    }

    /// <summary>
    /// A name is generally collected as three parts, but the WCF service sometimes wants a concatenaded name.
    /// This function breaks a concatenated name into its constituent parts for loading.
    /// </summary>
    static bool DeconstructLegalName(
        string legalName,
        out string? firstName,
        out string? middleInitial,
        out string? lastName)
    {
        firstName = null;
        middleInitial = null;
        lastName = null;

        var parts = legalName.Split(" ");
        if (parts.Length == 2)
        {
            firstName = parts[0];
            lastName = parts[1];
            return true;
        }
        if (parts.Length == 3)
        {
            firstName = parts[0];
            middleInitial = parts[1].Replace(".", string.Empty);
            lastName = parts[2];
            return true;
        }
        return false;
    }

    /// <summary>
    /// Convers the address proxy returned by the WCF service 
    /// </summary>
    /// <param name="address"></param>
    /// <returns></returns>
    static AddressModel ConvertAddressResponseToModel(RegistrationAddressProxy address)
    {
        return address.CountryAddressFormatTypeCode switch
        {
            "United States" => new()
            {
                Country = "United States",
                AddressLine1 = address.LineTwoAddress,
                AddressLine2 = address.LineOneAddress,
                City = address.CityName,
                Zip = address.ZipCode,
                Extension = address.ZipCodeExtension,
                State = address.StateCode,
                RegistrationAddressSk = address.EmployerRegistrationAddressSK,
            },
            "Canada" => new()
            {
                Country = "Canada",
                AddressLine1 = address.LineTwoAddress,
                AddressLine2 = address.LineOneAddress,
                City = address.CityName,
                Zip = address.CanadianPostalCode,
                State = address.StateCode,
                RegistrationAddressSk = address.EmployerRegistrationAddressSK,
            },
            _ => new()
            {
                Country = "Other International",
                AddressLine1 = address.LineTwoAddress,
                AddressLine2 = address.LineOneAddress,
                AddressLine3 = address.LineThreeAddress,
                AddressLine4 = address.LineFourAddress,
                RegistrationAddressSk = address.EmployerRegistrationAddressSK,
            },
        };
    }

    /// <summary>
    /// Helps to find a result in the list of result summaries returned by the WCF service
    /// </summary>
    /// <param name="questions">The list of response summaries from the WCF service.</param>
    /// <param name="responseItem">The respnose item we are looking for.</param>
    /// <param name="match">The result if there is one, null otherwise.</param>
    /// <returns>True if there is a match, false otherwise.</returns>
    static bool FindResultHelper(
        SurveyResponseItemProxy[] questions,
        SurveyResponseItem responseItem,
        [NotNullWhen(true)] out SurveyResponseItemProxy? match)
    {
        match = questions.FirstOrDefault(q =>
        {
            return q.QuestionSetItemSK == (int) responseItem;
        });

        return match != null;
    }

    /// <summary>
    /// Hellps to find an address in the list of addresses returned by the WCF service
    /// </summary>
    /// <param name="addresses">The list of addresses from the WCF service.</param>
    /// <param name="addressCode">The address code we are looking for.</param>
    /// <param name="match">The result if there is one, null otherwise.</param>
    /// <returns>True if there is a match, false otherwise.</returns>
    static bool FindAddressHelper(
        RegistrationAddressProxy[] addresses,
        RegistrationAddressCode addressCode,
        [NotNullWhen(true)] out RegistrationAddressProxy? match)
    {
        match = addresses.FirstOrDefault(a =>
        {
            return a.RegistrationAddressCodeSK == (int) addressCode;
        });

        return match != null;
    }

    /// <summary>
    /// Helps to find addresses in the list of addresses returned by the WCF service
    /// </summary>
    /// <param name="addresses"></param>
    /// <param name="addressCode"></param>
    /// <param name="matches"></param>
    /// <returns></returns>
    static bool FindAddressesHelper(
        RegistrationAddressProxy[] addresses,
        RegistrationAddressCode addressCode,
        out List<RegistrationAddressProxy> matches)
    {
        matches = addresses.Where(a =>
        {
            return a.RegistrationAddressCodeSK == (int) addressCode;
        }).ToList();

        return matches.Any();
    }

    /// <summary>
    /// Helps to find contacts in the list of contacts returned by the WCF service
    /// </summary>
    /// <param name="contacts"></param>
    /// <param name="individualCode"></param>
    /// <param name="matches"></param>
    /// <returns></returns>
    static bool FindContactsHelper(
        RegistrationIndividualProxy[] contacts,
        RegistrationIndividualCode individualCode,
        out List<RegistrationIndividualProxy> matches)
    {
        matches = contacts.Where(c =>
        {
            return c.RegistrationIndividualCodeSK == (int) individualCode;
        }).ToList();

        return matches.Any();
    }
}

/// <summary>
/// Used to map a response string to a specific response sk 
/// </summary>
public record SurveyResponse
{
    /// <summary>
    /// Response Item ID for the response
    /// </summary>
    public int _surveyResponseItemSk;

    /// <summary>
    /// String value for the response
    /// </summary>
    public string _response = string.Empty;

    /// <summary>
    /// String value to show in the summary view fallback is the _response value
    /// </summary>
    public string? _responseDisplay = null;
}

/// <summary>
/// Used to map a response contact to a specific response individual code
/// </summary>
public record SurveyContact
{
    /// <summary>
    /// Individual Code for the contact
    /// </summary>
    public int _surveyIndividualCode;

    /// <summary>
    /// First Name of the contact
    /// </summary>
    public string _firstName = string.Empty;

    /// <summary>
    /// Last Name of the contact
    /// </summary>
    public string _lastName = string.Empty;

    /// <summary>
    /// Middle Name of the contact
    /// </summary>
    public string _middleName = string.Empty;

    /// <summary>
    /// Ownership Percentage of the contact
    /// </summary>
    public string _ownershipPercentage = string.Empty;

    /// <summary>
    /// Social Security Number of the contact
    /// </summary>
    public string _socialSecurityNumber = string.Empty;
}
