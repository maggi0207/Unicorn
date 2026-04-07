using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using PortalQuestionResponseItem = UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService.PortalQuestionResponseItem;
using SurveyResponses = UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService.SurveyResponses;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration;

internal class EmployerRegistrationModelStore
{
    private readonly IEmployerRegistrationService _employerRegistrationService;
    private readonly IUserAccountService _userAccountService;
    private readonly NavigationManager _navigationManager;

    public EmployerRegistrationModel EmployerRegistrationModel { get; set; } = new();

    public EmployerRegistrationModelStore(
        IEmployerRegistrationService employerRegistrationService,
        IUserAccountService userAccountService,
        NavigationManager navigationManager)
    {
        _employerRegistrationService = employerRegistrationService;
        _userAccountService = userAccountService;
        _navigationManager = navigationManager;
    }

    public List<SurveyResponse> GetResponses()
    {
        return EmployerRegistrationModel.GetSurveyResponses();
    }

    public List<SurveyContact> GetContacts()
    {
        return EmployerRegistrationModel.GetSurveyContacts();
    }

    public List<Tuple<RegistrationAddressCode, AddressModel>> GetAddresses()
    {
        return EmployerRegistrationModel.GetSurveyAddresses();
    }

    /// <summary>
    /// Save the current model to the API and Register/Redirect to the appropriate page if successful.
    /// </summary>
    /// <returns>A list of rule violations from the related API calls. Empty list if none.</returns>
    public async Task<List<string>> Save()
    {
        var registrationQuestions = await _employerRegistrationService.LoadAllEmployerRegistrationQuestionsAsync();
        var surveyResponses = new List<PortalQuestionResponseItem>();

        surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.GetSurveyResponses()));

        var secureUserSkClaim = _userAccountService.GetUserSKClaim();

        var saveRequest = new SurveyResponses()
        {
            SecureUserSK = secureUserSkClaim,
            SurveyStartDate = DateTime.Now,
            SurveyCompletionDate = DateTime.Now,
            SurveyNumberText = !string.IsNullOrWhiteSpace(EmployerRegistrationModel.SurveyNumber) ? EmployerRegistrationModel.SurveyNumber : null,
            Responses = surveyResponses.ToArray(),
        };

        var saveResult = await _employerRegistrationService.SavePortalResponsesAsync(saveRequest);

        if (saveResult.RuleViolations.Any())
        {
            return saveResult.RuleViolations.Select(rv =>
            {
                return rv.RuleViolation;
            }).ToList();
        }
        else if (saveResult.SurveyResponseSK == null)
        {
            return new() { "There was a problem with submitting the Survey." };
        }

        foreach (var (addressCode, address) in EmployerRegistrationModel.GetSurveyAddresses())
        {
            var addressSaveResult = await SaveRegistrationAddress(secureUserSkClaim, (int) saveResult.SurveyResponseSK, address, addressCode);

            if (addressSaveResult.RuleViolations.Any())
            {
                return addressSaveResult.RuleViolations.Select(rv =>
                {
                    return rv.RuleViolation;
                }).ToList();
            }
        }

        var contactsRequest = new SurveyContacts()
        {
            SecureUserSK = secureUserSkClaim,
            SurveyResponseSK = saveResult.SurveyResponseSK,
            Individuals = EmployerRegistrationModel.GetSurveyContacts().Select(sc =>
            {
                return new PortalIndividualContactItemRequest()
                {
                    RegistrationIndividualCodeSK = sc._surveyIndividualCode,
                    FirstName = sc._firstName,
                    LastName = sc._lastName,
                    MiddleName = sc._middleName,
                    OwnershipPercentage = sc._ownershipPercentage,
                    SocialSecurityNumber = sc._socialSecurityNumber,
                };
            }).ToArray(),
        };

        if (contactsRequest.Individuals.Any())
        {
            var contactsResponse = await _employerRegistrationService.SaveIndividualContactsAsync(contactsRequest);
            if (contactsResponse.RuleViolations.Any())
            {
                return contactsResponse.RuleViolations.Select(rv =>
                {
                    return rv.RuleViolation;
                }).ToList();
            }
        }

        var match = await _employerRegistrationService.HasExactMatchAsync(saveResult.SurveyResponseSK.Value);

        if (!match.Value)
        {
            match = await _employerRegistrationService.HasPartialMatchAsync(saveResult.SurveyResponseSK.Value);
        }

        if (match.Value)
        {
            // planned path registration
            _navigationManager.NavigateTo("/employer-registration/planned-path");
            EmployerRegistrationModel.Clear();
            return new();
        }
        else
        {
            // register employer
            var registerRequest = new RequestForEmployerRegistration()
            {
                SecureUserSK = secureUserSkClaim,
                SurveyResponseSK = saveResult.SurveyResponseSK.Value
            };

            var registerResponse = await _employerRegistrationService.RegisterEmployerAsync(registerRequest);

            if (registerResponse.RuleViolations.Any())
            {
                return registerResponse.RuleViolations.Select(rv =>
                {
                    return rv.RuleViolation;
                }).ToList();
            }
            else
            {
                _navigationManager.NavigateTo("/employer-registration/SuccessRegistration");
                EmployerRegistrationModel.Clear();
                return new();
            }
        }
    }

    public async Task SavePartial(int step)
    {
        var registrationQuestions = await _employerRegistrationService.LoadAllEmployerRegistrationQuestionsAsync();
        var surveyResponses = new List<PortalQuestionResponseItem>();

        if (1 <= step)
        {
            surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.PreliminaryQuestionsModel.GetSurveyResponses()));
        }

        if (2 <= step)
        {
            surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.OwnershipSessionData.GetSurveyResponses()));
        }

        if (3 <= step)
        {
            surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.BusinessInformationModel.GetSurveyResponses()));
        }

        if (4 <= step)
        {
            surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.BusinessContactModel.GetSurveyResponses()));
        }

        if (5 <= step)
        {
            surveyResponses.AddRange(MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(registrationQuestions.Questions, EmployerRegistrationModel.BusinessActivityModel.GetSurveyResponses()));
        }

        // step6 add responses

        var secureUserSkClaim = _userAccountService.GetUserSKClaim();

        var saveRequest = new SurveyResponses()
        {
            SecureUserSK = secureUserSkClaim,
            SurveyStartDate = DateTime.Now,
            SurveyCompletionDate = DateTime.Now,
            SurveyNumberText = !string.IsNullOrWhiteSpace(EmployerRegistrationModel.SurveyNumber) ? EmployerRegistrationModel.SurveyNumber : null,
            Responses = surveyResponses.ToArray(),
        };

        var response = await _employerRegistrationService.SavePortalResponsesAsync(saveRequest);
        EmployerRegistrationModel.SurveyNumber = response.SurveyNumber;
    }

    public void LoadSurveyResponse(string surveyNumber)
    {
        throw new NotImplementedException();
    }

    private static List<PortalQuestionResponseItem> MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(
        EmployerRegistrationQuestionDataProxy[] registrationQuestions,
        List<SurveyResponse> surveyResponses)
    {
        return surveyResponses.Select(sr =>
        {
            var match = registrationQuestions.FirstOrDefault(q =>
            {
                return q.QuestionSetItemSK == sr._surveyResponseItemSk;
            });

            return match != null
                ? new PortalQuestionResponseItem()
                {
                    QuestionSetItemSK = match.QuestionSetItemSK,
                    QuestionSetCodeSK = match.QuestionSetCodeSK,
                    ReplyEntryTime = DateTime.Now,
                    ReplyText = sr._response,
                }
                : null;
        }).OfType<PortalQuestionResponseItem>()
        .ToList();
    }

    private async Task<PortalRegistrationResponse> SaveRegistrationAddress(
        int secureUserSK,
        int surveyResponseSK,
        AddressModel address,
        RegistrationAddressCode addressCode)
    {
        switch (address.Country)
        {
            case "United States":
                var usRequest = new PortalRegistrationAddressRequestUnitedStates()
                {
                    SecureUserSK = secureUserSK,
                    SurveyResponseSK = surveyResponseSK,
                    RegistrationAddressCodeSK = (int) addressCode,
                    LineTwoAddress = address.AddressLine1,
                    LineOneAddress = address.AddressLine2,
                    CityName = address.City,
                    StateCodeSK = GetStateProvinceAbbreviationFromCode(address.State),
                    ZipCode = address.Zip,
                    ZipExtension = address.Extension,
                };
                return await _employerRegistrationService.SaveRegistrationAddressUnitedStatesAsync(usRequest);

            case "Canada":
                var caRequest = new PortalRegistrationAddressRequestCanada()
                {
                    SurveyResponseSK = surveyResponseSK,
                    RegistrationAddressCodeSK = (int) addressCode,
                    LineTwoAddress = address.AddressLine1,
                    LineOneAddress = address.AddressLine2,
                    CityName = address.City,
                    ProvinceCodeSK = GetStateProvinceAbbreviationFromCode(address.State),
                    CanadianPostalCode = address.Zip,
                };
                return await _employerRegistrationService.SaveRegistrationAddressCanadaAsync(caRequest);

            default:
                var otherRequest = new PortalRegistrationAddressRequestOtherInternational()
                {
                    SurveyResponseSK = surveyResponseSK,
                    RegistrationAddressCodeSK = (int) addressCode,
                    LineOneAddress = address.AddressLine1,
                    LineTwoAddress = address.AddressLine2,
                    LineThreeAddress = string.Empty, // model.AddressLine3,
                    LineFourAddress = string.Empty, // model.AddressLine4,
                };
                return await _employerRegistrationService.SaveRegistrationAddressOtherInternationalAsync(otherRequest);
        }
    }

    private static readonly Dictionary<string, int> StateProvinceAbbreviationToCode = new()
    {
        ["AL"] = 1,
        ["AK"] = 2,
        ["AS"] = 3,
        ["AZ"] = 4,
        ["AR"] = 5,
        ["CA"] = 6,
        ["CO"] = 7,
        ["CT"] = 8,
        ["DE"] = 9,
        ["DC"] = 10,
        ["FM"] = 11,
        ["FL"] = 12,
        ["GA"] = 13,
        ["GU"] = 14,
        ["HI"] = 15,
        ["ID"] = 16,
        ["IL"] = 17,
        ["IN"] = 18,
        ["IA"] = 19,
        ["KS"] = 20,
        ["KY"] = 21,
        ["LA"] = 22,
        ["ME"] = 23,
        ["MH"] = 24,
        ["MD"] = 25,
        ["MA"] = 26,
        ["MI"] = 27,
        ["MN"] = 28,
        ["MS"] = 29,
        ["MO"] = 30,
        ["MT"] = 31,
        ["NE"] = 32,
        ["NV"] = 33,
        ["NH"] = 34,
        ["NJ"] = 35,
        ["NM"] = 36,
        ["NY"] = 37,
        ["NC"] = 38,
        ["ND"] = 39,
        ["MP"] = 40,
        ["OH"] = 41,
        ["OK"] = 42,
        ["OR"] = 43,
        ["PW"] = 44,
        ["PA"] = 45,
        ["PR"] = 46,
        ["RI"] = 47,
        ["SC"] = 48,
        ["SD"] = 49,
        ["TN"] = 50,
        ["TX"] = 51,
        ["UT"] = 52,
        ["VT"] = 53,
        ["VI"] = 54,
        ["VA"] = 55,
        ["WA"] = 56,
        ["WV"] = 57,
        ["WI"] = 58,
        ["WY"] = 59,
        ["AB"] = 60,
        ["BC"] = 61,
        ["MB"] = 62,
        ["NB"] = 63,
        ["NL"] = 64,
        ["NT"] = 65,
        ["NS"] = 66,
        ["NU"] = 67,
        ["ON"] = 68,
        ["PE"] = 69,
        ["QC"] = 70,
        ["SK"] = 71,
        ["YT"] = 72,
        ["AA"] = 73,
        ["AE"] = 74,
        ["AP"] = 75,
    };

    private static int GetStateProvinceAbbreviationFromCode(string? stateAbbreviation)
    {
        return stateAbbreviation != null
            ? StateProvinceAbbreviationToCode.TryGetValue(stateAbbreviation, out var code) ? code : 0
            : 0;
    }
}
