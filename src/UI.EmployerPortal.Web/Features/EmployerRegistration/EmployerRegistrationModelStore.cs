using System.ServiceModel;
using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;
using PortalQuestionResponseItem = UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService.PortalQuestionResponseItem;
using SurveyResponses = UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService.SurveyResponses;


namespace UI.EmployerPortal.Web.Features.EmployerRegistration;

internal class EmployerRegistrationModelStore
{
    private readonly IUploadServices _uploadServices;
    private readonly IEmployerRegistrationService _employerRegistrationService;
    private readonly IUserAccountService _userAccountService;
    private readonly IConfiguration _configuration;
    private readonly NavigationManager _navigationManager;

    private readonly string _technicalDifficulties;
    private readonly string _generalError = "There was an error, please try again.";

    private List<FileUploadService> _wcffileservices = [];
    private string? _uploadedFilePath;
    private string? _uploadedFilePathRullingDoc;
    private string? _uploadedFilePathIrs;
    private string? _uploadedFilePathArticle;

    private bool _skipSubjectivity = false;
    private bool _notLiable = false;
    private readonly ISessionManager _sessionManager;

    public EmployerRegistrationModel EmployerRegistrationModel { get; set; } = new();

    public EmployerRegistrationModelStore(
        IEmployerRegistrationService employerRegistrationService,
        IUserAccountService userAccountService,
        IUploadServices uploadServices,
        IConfiguration configuration,
        NavigationManager navigationManager,
        ISessionManager sessionManager)
    {
        _employerRegistrationService = employerRegistrationService;
        _userAccountService = userAccountService;
        _navigationManager = navigationManager;
        _configuration = configuration;
        _uploadServices = uploadServices;
        _sessionManager = sessionManager;

        _technicalDifficulties = configuration.GetValue<string>("Messages:TechnicalDifficulties") ?? "There was an error.";
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

    public async Task<bool> SkipSubjectivity()
    {
        if (_skipSubjectivity)
        {
            var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);

            if (hasSurveyResponseSk)
            {
                var match = await _employerRegistrationService.HasPartialMatchAsync(surveyResponseSkValue);

                if (!match.Value)
                {
                    match = await _employerRegistrationService.HasExactMatchAsync(surveyResponseSkValue);
                }

                return match.Value;
            }
        }

        return _notLiable;
    }

    /// <summary>
    /// Clear the employer registration model, meant to be called when starting a new registration in case the model contains old information
    /// </summary>
    public void ClearModel()
    {
        EmployerRegistrationModel.Clear();
    }

    /// <summary>
    /// Save the current model to the API and Register/Redirect to the appropriate page if successful.
    /// </summary>
    /// <returns>A list of rule violations from the related API calls. Empty list if none.</returns>
    public async Task<List<string>> CompleteRegistration()
    {
        if (int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSk))
        {
            try
            {
                var secureUserSkClaim = _userAccountService.GetUserSKClaim();

                var match = await _employerRegistrationService.HasExactMatchAsync(surveyResponseSk);

                if (!match.Value)
                {
                    match = await _employerRegistrationService.HasPartialMatchAsync(surveyResponseSk);
                }

                if (match.Value)
                {
                    // planned path registration
                    _navigationManager.NavigateTo("employer-registration/planned-path");
                    return new();
                }
                else
                {
                    // register employer
                    var registerRequest = new RequestForEmployerRegistration()
                    {
                        SecureUserSK = secureUserSkClaim,
                        SurveyResponseSK = surveyResponseSk
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
                        var sessionEmployerRegistrationInfo = new SessionEmployerRegistrationInfo()
                        {
                            EmployerSk = registerResponse.EmployerSK ?? 0,
                            EmployerGenerationSk = registerResponse.CorrespondenceGenerationSK ?? 0
                        };

                        await _sessionManager.SetAsync<SessionEmployerRegistrationInfo>(sessionEmployerRegistrationInfo);

                        _navigationManager.NavigateTo("employer-registration/SuccessRegistration");
                        return new();
                    }
                }
            }
            catch
            {
                return new() { _technicalDifficulties };
            }
        }
        else
        {
            return new() { _technicalDifficulties };
        }
    }

    /// <summary>
    /// Save the response to the WCF service that indicates they would like email notification of
    /// </summary>
    /// <returns></returns>
    public async Task<bool> MailId()
    {
        try
        {
            var response = await SaveResponsesToWcfService(new List<PortalQuestionResponseItem>
            {
                new()
                {
                    QuestionSetItemSK = (int) SurveyResponseItem.EMAIL_NOTIFY,
                    ReplyText = "True",
                    ReplyEntryTime = DateTime.Now,
                }
            }, true);

            return response.Item2 != null;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Save the responses for the current step. Suppresses validation errors, does not emit errors.
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public async Task<List<string>> SavePartial(int step)
    {
        var model = GetStepModel(step);

        if (model != null)
        {
            await SaveModel(model, true);
        }

        await Loadfile(step);

        return new();
    }

    /// <summary>
    /// Save the responses for the current step. Return an error indicating what had the issue if there is one.
    /// </summary>
    /// <param name="step"></param>
    /// <returns></returns>
    public async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> SaveStep(int step)
    {
        var model = GetStepModel(step);

        return model != null
            ? await SaveModel(model)
            : Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
    }

    public async Task ContinueSurvey(
        string? surveyNumber,
        string? fein)
    {
        try
        {
            var secureUserSKClaim = _userAccountService.GetUserSKClaim();

            var continueRegistrationRequest = new ContinueRegistrationRequest()
            {
                SecureUserSK = secureUserSKClaim,
                FEIN = fein,
                SurveyNumberText = surveyNumber ?? string.Empty
            };

            var contiueRegistrationResponse = await _employerRegistrationService.ContinueRegistrationAsync(continueRegistrationRequest);

            if (!contiueRegistrationResponse.SurveyResponseSK.HasValue)
            {
                return;
            }

            EmployerRegistrationModel.SurveyResponseSk = contiueRegistrationResponse.SurveyResponseSK?.ToString() ?? string.Empty;

            LoadSurvey(contiueRegistrationResponse);

        }
        catch
        {
            return;
        }
    }

    private void LoadSurvey(PortalContinueRegistrationResponse summary)
    {
        EmployerRegistrationModel.LoadSurveyResponses(summary.Responses);
        EmployerRegistrationModel.LoadSurveyAddresses(summary.Addresses);
        EmployerRegistrationModel.LoadSurveyContacts(summary.Contacts);
    }

    private IEmployerRegistrationModelSection? GetStepModel(int step)
    {
        return step switch
        {
            1 => EmployerRegistrationModel.PreliminaryQuestionsModel,
            2 => EmployerRegistrationModel.OwnershipSessionData,
            3 => EmployerRegistrationModel.BusinessInformationModel,
            4 => EmployerRegistrationModel.BusinessContactModel,
            5 => EmployerRegistrationModel.BusinessActivityModel,
            6 => EmployerRegistrationModel.SubjectivityModel,
            _ => null
        };
    }

    private PortalIndividualContactItemRequest ConvertContactToSaveRequest(SurveyContact contact)
    {
        return new PortalIndividualContactItemRequest()
        {
            RegistrationIndividualCodeSK = contact._surveyIndividualCode,
            FirstName = contact._firstName,
            LastName = contact._lastName,
            MiddleName = contact._middleName,
            OwnershipPercentage = contact._ownershipPercentage,
            SocialSecurityNumber = contact._socialSecurityNumber,
        };
    }

    public async Task Loadfile(int step)
    {
        var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);
        if (hasSurveyResponseSk)
        {
            _uploadedFilePath = EmployerRegistrationModel.OwnershipSessionData.OwnershipAgencies!.Filepath;
            _uploadedFilePathRullingDoc = EmployerRegistrationModel.PreliminaryQuestionsModel.RulingDocFilename;
            _uploadedFilePathIrs = EmployerRegistrationModel.PreliminaryQuestionsModel.IRSAcceptanceLetterFilename;
            _uploadedFilePathArticle = EmployerRegistrationModel.PreliminaryQuestionsModel.ArticlesOfIncorporationFilename;
            if (!string.IsNullOrEmpty(_uploadedFilePath) && 2 == step)
            {
                _wcffileservices = await _uploadServices.LoadFileWcf(_uploadedFilePath!, EmployerRegistrationModel.SurveyNumber, surveyResponseSkValue);
            }
            if (1 == step)
            {
                if (!string.IsNullOrEmpty(_uploadedFilePathRullingDoc))
                {
                    _wcffileservices = await _uploadServices.LoadFileWcf(_uploadedFilePathRullingDoc!, EmployerRegistrationModel.SurveyNumber, surveyResponseSkValue);
                }
                if (!string.IsNullOrEmpty(_uploadedFilePathIrs))
                {
                    _wcffileservices = await _uploadServices.LoadFileWcf(_uploadedFilePathIrs!, EmployerRegistrationModel.SurveyNumber, surveyResponseSkValue);
                }
                if (!string.IsNullOrEmpty(_uploadedFilePathArticle))
                {
                    _wcffileservices = await _uploadServices.LoadFileWcf(_uploadedFilePathArticle!, EmployerRegistrationModel.SurveyNumber, surveyResponseSkValue);
                }
            }
        }
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
                    EmployerRegistrationAddressSK = address.RegistrationAddressSk != 0 ? address.RegistrationAddressSk : null,
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
                    ProvinceCodeSK = GetStateProvinceAbbreviationFromCode(address.Province),
                    CanadianPostalCode = address.PostalCode,
                    EmployerRegistrationAddressSK = address.RegistrationAddressSk != 0 ? address.RegistrationAddressSk : null,
                };
                return await _employerRegistrationService.SaveRegistrationAddressCanadaAsync(caRequest);

            default:
                var otherRequest = new PortalRegistrationAddressRequestOtherInternational()
                {
                    SurveyResponseSK = surveyResponseSK,
                    RegistrationAddressCodeSK = (int) addressCode,
                    LineOneAddress = address.AddressLine1,
                    LineTwoAddress = address.AddressLine2,
                    LineThreeAddress = address.AddressLine3 ?? string.Empty,
                    LineFourAddress = address.AddressLine4 ?? string.Empty,
                    EmployerRegistrationAddressSK = address.RegistrationAddressSk != 0 ? address.RegistrationAddressSk : null,
                };
                return await _employerRegistrationService.SaveRegistrationAddressOtherInternationalAsync(otherRequest);
        }
    }

    public async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> PutSurveyAddressSKs(IEmployerRegistrationModelSection model)
    {
        try
        {
            var secureUserSkClaim = _userAccountService.GetUserSKClaim();
            var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);

            if (hasSurveyResponseSk)
            {
                var addressResponse = await _employerRegistrationService.GetPortalRegistrationAddressesAsync(new PortalRegistrationAddressRequest
                {
                    SurveyResponseSK = surveyResponseSkValue
                });

                model.PutAddressSKs(addressResponse.Addresses);
            }

            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
        }
        catch (CommunicationException)
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _technicalDifficulties });
        }
        catch
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _generalError });
        }
    }

    private async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> SaveResponsesToWcfService(
        List<PortalQuestionResponseItem>? responses,
        bool suppressRegistrationViolations = false)
    {
        try
        {
            var secureUserSkClaim = _userAccountService.GetUserSKClaim();
            var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);

            if (responses != null)
            {
                var saveResponseRequest = new SurveyResponses()
                {
                    SecureUserSK = secureUserSkClaim,
                    SurveyStartDate = DateTime.Now,
                    SurveyCompletionDate = DateTime.Now,
                    SurveyResponseSK = hasSurveyResponseSk ? surveyResponseSkValue : null,
                    SurveyNumberText = !string.IsNullOrWhiteSpace(EmployerRegistrationModel.SurveyNumber) ? EmployerRegistrationModel.SurveyNumber : null,
                    Responses = responses?.ToArray(),
                    PerformValidationFlag = !suppressRegistrationViolations,
                };

                var saveResponseResult = await _employerRegistrationService.SavePortalResponsesAsync(saveResponseRequest);

                // if we only have registration violations that say to skip subjectivity, then we can do that, otherwise we are going to have to call again anyways
                if (saveResponseResult.RegistrationViolations.Any()
                    && saveResponseResult.RegistrationViolations.All(rv =>
                    {
                        return rv.ExitToSummaryFlag ?? false;
                    }))
                {
                    if (!_skipSubjectivity)
                    {
                        _skipSubjectivity = true;
                    }
                    return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
                }
                else
                {
                    _skipSubjectivity = false;
                }
                if (saveResponseResult.RegistrationViolations.Any()
                    && saveResponseResult.RegistrationViolations.All(rv =>
                    {
                        return rv.NotLiableRegistrationFlag ?? false;
                    }))
                {
                    if (!_notLiable)
                    {
                        _notLiable = true;
                    }
                    return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
                }
                else
                {
                    _notLiable = false;
                }

                // violations with Ids
                if (saveResponseResult.RegistrationViolations.Any())
                {
                    return Tuple.Create((List<EmployerRegistrationValidationError>?) saveResponseResult.RegistrationViolations.Select(rv =>
                    {
                        return new EmployerRegistrationValidationError
                        {
                            ItemId = rv.QuestionSetItemSK,
                            ErrorType = ValidationErrorType.Response,
                            ValidationErrors = rv.RuleViolations.ToList(),
                        };
                    }).ToList(), (List<string>?) null);
                }

                // violations without Ids
                if (saveResponseResult.RuleViolations.Any())
                {
                    return Tuple.Create((List<EmployerRegistrationValidationError>?) null,
                        (List<string>?) saveResponseResult.RuleViolations.Select(rv =>
                        {
                            return rv.RuleViolation;
                        }).ToList());
                }

                EmployerRegistrationModel.SurveyResponseSk = saveResponseResult.SurveyResponseSK?.ToString() ?? throw new ArgumentNullException("Validation Passed but the service returned no Survey Id");
                surveyResponseSkValue = saveResponseResult.SurveyResponseSK.Value;
                hasSurveyResponseSk = true;

                EmployerRegistrationModel.SurveyNumber = saveResponseResult.SurveyNumber;
            }

            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
        }
        catch (CommunicationException)
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _technicalDifficulties });
        }
        catch
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _generalError });
        }
    }

    private async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> SaveAddressesToWcfService(List<Tuple<RegistrationAddressCode, AddressModel>>? addresses = null)
    {
        try
        {
            var secureUserSkClaim = _userAccountService.GetUserSKClaim();
            var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);

            if (addresses != null && hasSurveyResponseSk)
            {
                foreach (var (addressCode, address) in addresses)
                {
                    var addressSaveResult = await SaveRegistrationAddress(secureUserSkClaim, surveyResponseSkValue, address, addressCode);

                    if (addressSaveResult.RuleViolations.Any())
                    {
                        return Tuple.Create((List<EmployerRegistrationValidationError>?) null,
                            (List<string>?) addressSaveResult.RuleViolations.Select(rv =>
                            {
                                return rv.RuleViolation;
                            }).ToList());
                    }
                }
            }

            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
        }
        catch (CommunicationException)
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _technicalDifficulties });
        }
        catch
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _generalError });
        }
    }

    private async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> SaveContactsToWcfService(List<SurveyContact>? contacts = null)
    {
        try
        {
            var secureUserSkClaim = _userAccountService.GetUserSKClaim();
            var hasSurveyResponseSk = int.TryParse(EmployerRegistrationModel.SurveyResponseSk, out var surveyResponseSkValue);

            if (contacts != null && contacts.Count > 0 && hasSurveyResponseSk)
            {
                var contactSaveRequest = new SurveyContacts()
                {
                    SecureUserSK = secureUserSkClaim,
                    SurveyResponseSK = surveyResponseSkValue,
                    Individuals = contacts.Select(ConvertContactToSaveRequest).ToArray(),
                };

                var contactSaveResponse = await _employerRegistrationService.SaveIndividualContactsAsync(contactSaveRequest);

                if (contactSaveResponse.RuleViolations.Any())
                {
                    return Tuple.Create((List<EmployerRegistrationValidationError>?) null,
                        (List<string>?) contactSaveResponse.RuleViolations.Select(rv =>
                        {
                            return rv.RuleViolation;
                        }).ToList());
                }
            }

            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
        }
        catch (CommunicationException)
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _technicalDifficulties });
        }
        catch
        {
            return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) new List<string>() { _generalError });
        }
    }

    private async Task<Tuple<List<EmployerRegistrationValidationError>?, List<string>?>> SaveModel(IEmployerRegistrationModelSection model, bool suppressRegistrationValidations = false)
    {
        var questionsResponse = await _employerRegistrationService.LoadAllEmployerRegistrationQuestionsAsync();

        var responses = MapSurveyResponseToPortalQuestionResponseItemExcludeUnmatched(questionsResponse.Questions, model.GetSurveyResponses());
        if (responses.Any())
        {
            var responseViolations = await SaveResponsesToWcfService(responses, suppressRegistrationValidations);

            if (responseViolations.Item1 != null || responseViolations.Item2 != null)
            {
                return responseViolations;
            }
        }

        var addresses = model.GetSurveyAddresses();
        if (addresses.Any())
        {
            var addressViolations = await SaveAddressesToWcfService(addresses);

            if (addressViolations.Item1 != null || addressViolations.Item2 != null)
            {
                return addressViolations;
            }

            var putAddressSkViolations = await PutSurveyAddressSKs(model);

            if (putAddressSkViolations.Item1 != null || addressViolations.Item2 != null)
            {
                return putAddressSkViolations;
            }
        }

        var contacts = model.GetSurveyContacts();
        if (contacts.Any())
        {
            var contactViolations = await SaveContactsToWcfService(contacts);

            if (contactViolations.Item1 != null || contactViolations.Item2 != null)
            {
                return contactViolations;
            }
        }

        return Tuple.Create((List<EmployerRegistrationValidationError>?) null, (List<string>?) null);
    }

    public class EmployerRegistrationValidationError
    {
        public int ItemId { get; set; }

        public ValidationErrorType ErrorType { get; set; }

        public List<string> ValidationErrors { get; set; } = new();
    }

    public enum ValidationErrorType
    {
        Response = 1,
        Address = 2,
        Contact = 3,
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

    internal static int GetStateProvinceAbbreviationFromCode(string? stateAbbreviation)
    {
        return stateAbbreviation != null
            ? StateProvinceAbbreviationToCode.TryGetValue(stateAbbreviation, out var code) ? code : 0
            : 0;
    }

    public static string GetStateProviceCodeFromAbbreviation(int code)
    {
        return StateProvinceAbbreviationToCode.FirstOrDefault(v =>
        {
            return v.Value == code;
        }).Key;
    }
}
