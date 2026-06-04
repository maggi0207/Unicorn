using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// 
/// </summary>
public class PreliminaryQuestionsModel : IEmployerRegistrationModelSection
{
    /// <summary>
    /// Federal Employer Identification Number.
    /// Validation is handled entirely by <see cref="UI.EmployerPortal.Razor.SharedComponents.Inputs.FEINField.ValidateFEIN"/>.
    /// </summary>
    public string? FEIN { get; set; }

    /// <summary>
    /// Unemployment Isurance Employer Account Number
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;


    //public BusinessCategory? BusinessCategory { get; set; } = null;

    /// <summary>
    /// Answer to "Are you a non-profit organization as described in s.501(c)(3) of the IRS code?"
    /// Drives the entire 501(c)(3) sub-tree visibility.
    /// </summary>
    public bool? IsNonProfitOrg { get; set; } = null;

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
    [MaxLength(128, ErrorMessage = "Acquired Business Name cannot exceed 128 characters")]
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

    /// <summary> RulingDocFilename </summary>
    public string? RulingDocFilename { get; set; }

    /// <summary> ArticlesOfIncorporationFilename </summary>
    public string? ArticlesOfIncorporationFilename { get; set; }

    /// <summary> IRSAcceptanceLetterFilename </summary>
    public string? IRSAcceptanceLetterFilename { get; set; }

    /// <summary> RulingDocFileMIMEType </summary>
    public string? RulingDocFileMIMEType { get; set; }

    /// <summary> ArticlesOfIncorporationFileMIMEType </summary>
    public string? ArticlesOfIncorporationFileMIMEType { get; set; }

    /// <summary> IRSAcceptanceLetterFileMIMEType </summary>
    public string? IRSAcceptanceLetterFileMIMEType { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public FileUploadState? RulingDocUploadState { get; set; } = FileUploadState.Default;

    /// <summary> ArticlesOfIncorporationFilename </summary>
    public FileUploadState? ArticlesOfIncorporationUploadState { get; set; } = FileUploadState.Default;

    /// <summary> IRSAcceptanceLetterFilename </summary>
    public FileUploadState? IRSAcceptanceLetterUploadState { get; set; } = FileUploadState.Default;
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
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses)
    {
        if (AcquiredExistingBusiness.HasValue && AcquiredExistingBusiness.Value
            && KnowAcquiredBusinessAccountNumber.HasValue && !KnowAcquiredBusinessAccountNumber.Value
            && IEmployerRegistrationModelSection.FindAddressHelper(addresses, RegistrationAddressCode.Acquired_Business, out var acquiredBusinessAddress))
        {
            AcquiredBusinessAddress = IEmployerRegistrationModelSection.ConvertAddressResponseToModel(acquiredBusinessAddress);
        }
    }

    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts)
    {
        return;
    }

    /// <inheritdoc />
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (!string.IsNullOrWhiteSpace(FEIN)) // 1.02
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FEIN_NUM, _response = FEIN.Replace("-", string.Empty), _responseDisplay = FEIN });
        }

        if (!string.IsNullOrWhiteSpace(UIAccountNumber)) //1.01
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_ACCT_NUM, _response = UIAccountNumber.Replace("-", string.Empty), _responseDisplay = UIAccountNumber });
        }

        if (IsNonProfitOrg.HasValue)
        {
            // 1.17
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_NON_PRFT_FLG,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsNonProfitOrg.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsNonProfitOrg.Value)
            });

            if (IsNonProfitOrg.Value
                && HasRulingFrom501c3IRS.HasValue)
            {
                // 1.18
                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_501C3_RULING_FLG,
                    _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasRulingFrom501c3IRS.Value),
                    _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasRulingFrom501c3IRS.Value)
                });

                if (HasRulingFrom501c3IRS.Value)
                {
                    if (WillSupplyDocumentationLater)
                    {
                        // 1.21
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_501C3_RULING,
                            _response = "True",
                            _responseDisplay = "Yes"
                        });
                    }
                    else if (!string.IsNullOrWhiteSpace(RulingDocFilename))
                    {
                        // 1.20
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_501C3_UPLD,
                            _response = RulingDocFilename,
                        });
                    }
                }

                else if (!HasRulingFrom501c3IRS.Value
                    && HasAppliedFor501c3WithIRS.HasValue)
                {
                    // 1.19
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_APPLY_501C3,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HasAppliedFor501c3WithIRS.Value),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HasAppliedFor501c3WithIRS.Value)
                    });

                    if (HasAppliedFor501c3WithIRS.Value)
                    {
                        if (WillSupplyDocumentationLater)
                        {
                            responses.AddRange(new List<SurveyResponse>()
                            {
                                // 1.23
                                new()
                                {
                                    _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_ARTCL_INCORP,
                                    _response = "True",
                                    _responseDisplay = "Yes",
                                },
                                // 1.25
                                new()
                                {
                                    _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_IRS_APP_ACCPT,
                                    _response = "True",
                                    _responseDisplay = "Yes",
                                },
                            });
                        }
                        else
                        {
                            if (!string.IsNullOrWhiteSpace(ArticlesOfIncorporationFilename))
                            {
                                // 1.22
                                responses.Add(new()
                                {
                                    _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_ARTCL_OF_CORP,
                                    _response = ArticlesOfIncorporationFilename,
                                });
                            }
                            if (!string.IsNullOrWhiteSpace(IRSAcceptanceLetterFilename))
                            {
                                // 1.24
                                responses.Add(new()
                                {
                                    _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_IRS_ACCPT,
                                    _response = IRSAcceptanceLetterFilename,
                                });
                            }
                        }
                    }

                    else if (!HasAppliedFor501c3WithIRS.Value)
                    {
                        if (WillSupplyDocumentationLater)
                        {
                            // 1.23
                            responses.Add(new()
                            {
                                _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_ARTCL_INCORP,
                                _response = "True",
                                _responseDisplay = "Yes",
                            });
                        }
                        else if (!string.IsNullOrWhiteSpace(ArticlesOfIncorporationFilename))
                        {
                            // 1.22
                            responses.Add(new()
                            {
                                _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_ARTCL_OF_CORP,
                                _response = ArticlesOfIncorporationFilename,
                            });
                        }
                    }
                }
            }
        }

        if (AcquiredExistingBusiness.HasValue)
        {
            // 1.03
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_FLG,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(AcquiredExistingBusiness.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(AcquiredExistingBusiness.Value)
            });

            if (AcquiredExistingBusiness.Value && KnowAcquiredBusinessAccountNumber.HasValue)
            {
                // 1.04
                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_ACCT_NUM_KNWN,
                    _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(KnowAcquiredBusinessAccountNumber.Value),
                    _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(KnowAcquiredBusinessAccountNumber.Value)
                });

                if (KnowAcquiredBusinessAccountNumber.Value && !string.IsNullOrWhiteSpace(AcquiredBusinessAccountNumber))
                {
                    // 1.05
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_ACCT_NUM,
                        _response = AcquiredBusinessAccountNumber.Replace("-", string.Empty),
                        _responseDisplay = AcquiredBusinessAccountNumber
                    });
                }

                else if (!KnowAcquiredBusinessAccountNumber.Value && !string.IsNullOrWhiteSpace(AcquiredBusinessName))
                {
                    // 1.06
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_NAM, _response = AcquiredBusinessName });
                }
            }

            else if (!AcquiredExistingBusiness.Value && HavePaidEmployeesForWorkInWisconsin.HasValue)
            {
                // 1.07
                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.PAID_EE_FLG,
                    _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HavePaidEmployeesForWorkInWisconsin.Value),
                    _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HavePaidEmployeesForWorkInWisconsin.Value)
                });

                if (HavePaidEmployeesForWorkInWisconsin.Value && HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
                {
                    // 1.10
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.STILL_EE_FLG,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HaveEmployeesCurrentlyWorkingInWisconsin.Value),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HaveEmployeesCurrentlyWorkingInWisconsin.Value)
                    });

                    if (HaveEmployeesCurrentlyWorkingInWisconsin.Value)
                    {
                        // 1.16
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_ACCURATE_INFO,
                            _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(InformationIsAccurate),
                            _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(InformationIsAccurate)
                        });
                    }

                    else if (!HaveEmployeesCurrentlyWorkingInWisconsin.Value)
                    {
                        if (SelectedNoEmployeeReason.HasValue)
                        {
                            var reasonDisplay = SelectedNoEmployeeReason.Value switch
                            {
                                NoEmployeeReason.BusinessActivityEnded => "Business activity has ended but business has not been sold",
                                NoEmployeeReason.NotOperatingInWisconsin => "No longer operating in Wisconsin but still operating in another state",
                                NoEmployeeReason.HaveSoldOrTransferredBusiness => "Business activity sold or transferred",
                                NoEmployeeReason.BusiessWithoutEmployees => "Business continuing without employees",
                                NoEmployeeReason.EmployingIndependentContractors => "Employing Independent Contractors",
                                NoEmployeeReason.Death => "Death",
                                NoEmployeeReason.LeasingFromPEO => "Leasing employees from Professional Employer Organization (PEO)",
                                NoEmployeeReason.FiscalAgent => "Fiscal Agent electing to be employer",
                                NoEmployeeReason.Other => "Other",
                                _ => SelectedNoEmployeeReason.Value.ToString()
                            };
                            // 1.14
                            responses.Add(new SurveyResponse()
                            {
                                _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_NO_LNGR_EE,
                                _response = ((int) SelectedNoEmployeeReason.Value + 1).ToString(),
                                _responseDisplay = reasonDisplay
                            });

                            if (SelectedNoEmployeeReason == NoEmployeeReason.LeasingFromPEO)
                            {
                                if (!string.IsNullOrWhiteSpace(PEOName))
                                {
                                    // 1.26
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NAME, _response = PEOName });
                                }

                                if (!string.IsNullOrWhiteSpace(PEOUIAccountNumber))
                                {
                                    // 1.27
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOUIAccountNumber.Replace("-", string.Empty) });
                                }

                                if (!string.IsNullOrWhiteSpace(PEOFEIN))
                                {
                                    // 1.27
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOFEIN.Replace("-", string.Empty) });
                                }

                                if (LeasingStartDate.HasValue)
                                {
                                    // 1.28
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_DATE, _response = LeasingStartDate.Value.ToString("MM/dd/yyyy") });
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.FiscalAgent)
                            {
                                if (!string.IsNullOrWhiteSpace(FiscalAgentName))
                                {
                                    // 1.29
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FSCL_AGNT_NAM, _response = FiscalAgentName });
                                }

                                if (!string.IsNullOrWhiteSpace(FiscalAgentUIAccountNumber))
                                {
                                    // 1.30
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FA_UI_ACCT_NUM, _response = FiscalAgentUIAccountNumber.Replace("-", string.Empty) });
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.BusiessWithoutEmployees)
                            {
                                if (!string.IsNullOrWhiteSpace(NoEmployeeExplanation))
                                {
                                    // 1.15
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_BUS_WITHOUT_EE, _response = NoEmployeeExplanation });
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.Other)
                            {
                                if (!string.IsNullOrWhiteSpace(OtherReason))
                                {
                                    // 1.31
                                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_OTHR_RSN, _response = OtherReason });
                                }
                            }
                        }

                        if (LastEmploymentDate.HasValue)
                        {
                            // 1.12
                            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_EMPL_DT, _response = LastEmploymentDate.Value.ToString("MM/dd/yyyy") });
                        }

                        if (LastPayrollDate.HasValue)
                        {
                            // 1.13
                            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_PYRL_DT, _response = LastPayrollDate.Value.ToString("MM/dd/yyyy") });
                        }
                    }
                }

                else if (!HavePaidEmployeesForWorkInWisconsin.Value && ExpectFuturePayroll.HasValue)
                {
                    // 1.09
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_FLG,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectFuturePayroll.Value),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectFuturePayroll.Value)
                    });
                    if (ExpectFuturePayroll.Value && ExpectedFuturePayrollPeriod.HasValue)
                    {
                        // 1.10
                        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_TIME, _response = ((int) ExpectedFuturePayrollPeriod.Value).ToString() });
                    }
                }
            }
        }

        return responses;
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        //if (!string.IsNullOrWhiteSpace(FEIN))
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.FEIN_NUM, _response = FEIN.Replace("-", string.Empty), _responseDisplay = FEIN });
        //}
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.FEIN_NUM, out var fein))
        {
            FEIN = IEmployerRegistrationModelSection.FormatFeinResponseString(fein.ReplyText);
        }

        //if (!string.IsNullOrWhiteSpace(UIAccountNumber))
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ER_ACCT_NUM, _response = UIAccountNumber.Replace("-", string.Empty), _responseDisplay = UIAccountNumber });
        //}
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ER_ACCT_NUM, out var employerUiAccountNumber))
        {
            UIAccountNumber = IEmployerRegistrationModelSection.FormatUiAccountNumberResponseString(employerUiAccountNumber.ReplyText);
        }

        //if (IsNonProfitOrg.HasValue)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_NON_PRFT_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsNonProfitOrg.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsNonProfitOrg.Value) });
        //}
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_NON_PRFT_FLG, out var is501c3)) // 1.17
        {
            IsNonProfitOrg = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(is501c3.ReplyText);

            if (IsNonProfitOrg.Value)
            {
                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_501C3_RULING_FLG, out var has501c3Ruling))
                {
                    HasRulingFrom501c3IRS = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(has501c3Ruling.ReplyText); // 1.18

                    if (HasRulingFrom501c3IRS.Value)
                    {
                        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_501C3_UPLD, out var rulingDocFileName))
                        {
                            RulingDocFilename = rulingDocFileName.ReplyText; // 1.20
                        }
                        else if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_501C3_RULING, out var continueWithoutRulingDoc))
                        {
                            WillSupplyDocumentationLater = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(continueWithoutRulingDoc.ReplyText); //1.21
                        }
                    }

                    else if (!HasRulingFrom501c3IRS.Value
                        && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_APPLY_501C3, out var hasAppliedFor501c3))
                    {
                        HasAppliedFor501c3WithIRS = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(hasAppliedFor501c3.ReplyText); // 1.19

                        if (HasAppliedFor501c3WithIRS.Value)
                        {
                            var articlesUploaded = IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_ARTCL_OF_CORP, out var articlesFilename);
                            var acceptanceUploaded = IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_IRS_ACCPT, out var acceptanceFilename);

                            if (articlesUploaded)
                            {
                                ArticlesOfIncorporationFilename = articlesFilename!.ReplyText; // 1.22
                            }
                            if (acceptanceUploaded)
                            {
                                IRSAcceptanceLetterFilename = acceptanceFilename!.ReplyText; // 1.24
                            }

                            var missArticles = IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_ARTCL_INCORP, out var articlesMissed);
                            var missAcceptance = IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_IRS_APP_ACCPT, out var acceptanceMissed);

                            if (!(articlesUploaded || acceptanceUploaded)
                                && ((missArticles && IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(articlesMissed!.ReplyText))
                                    || (missAcceptance && IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(acceptanceMissed!.ReplyText))))
                            {
                                WillSupplyDocumentationLater = true; // 1.23/1.25
                            }
                        }

                        else if (!HasAppliedFor501c3WithIRS.Value)
                        {
                            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_ARTCL_OF_CORP, out var articlesOfIncorpFilename))
                            {
                                ArticlesOfIncorporationFilename = articlesOfIncorpFilename.ReplyText; // 1.22
                            }
                            else if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_ARTCL_INCORP, out var continueWithoutArticlesOfIncorporation))
                            {
                                WillSupplyDocumentationLater = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(continueWithoutArticlesOfIncorporation.ReplyText); // 1.23
                            }
                        }
                    }
                }
            }
        }

        //if (AcquiredExistingBusiness.HasValue)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(AcquiredExistingBusiness.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(AcquiredExistingBusiness.Value) });
        //}
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ACQ_BUS_FLG, out var acquiredExistingBusiness))
        {
            AcquiredExistingBusiness = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(acquiredExistingBusiness.ReplyText);

            //if (visibilityQ2_1 && KnowAcquiredBusinessAccountNumber.HasValue)
            //{
            //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_ACCT_NUM_KNWN, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(KnowAcquiredBusinessAccountNumber.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(KnowAcquiredBusinessAccountNumber.Value) });
            //}
            if (AcquiredExistingBusiness.Value
                && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ACQ_ACCT_NUM_KNWN, out var knowAcquiredBusinessAccountNumber))
            {
                KnowAcquiredBusinessAccountNumber = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(knowAcquiredBusinessAccountNumber.ReplyText);

                //if (visibilityQ2_1_1 && !string.IsNullOrWhiteSpace(AcquiredBusinessAccountNumber))
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_ACCT_NUM, _response = AcquiredBusinessAccountNumber.Replace("-", string.Empty), _responseDisplay = AcquiredBusinessAccountNumber });
                //}
                if (KnowAcquiredBusinessAccountNumber.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ACQ_BUS_ACCT_NUM, out var acquiredBusinessUiAccountNumber))
                {
                    AcquiredBusinessAccountNumber = IEmployerRegistrationModelSection.FormatUiAccountNumberResponseString(acquiredBusinessUiAccountNumber.ReplyText);
                }

                //if (visibilityQ2_1_2 && !string.IsNullOrWhiteSpace(AcquiredBusinessName))
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ACQ_BUS_NAM, _response = AcquiredBusinessName });
                //}
                else if (!KnowAcquiredBusinessAccountNumber.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ACQ_BUS_NAM, out var acquiredBusinessName))
                {
                    AcquiredBusinessName = acquiredBusinessName.ReplyText;
                }
            }

            //if (visibilityQ3 && HavePaidEmployeesForWorkInWisconsin.HasValue)
            //{
            //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PAID_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HavePaidEmployeesForWorkInWisconsin.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HavePaidEmployeesForWorkInWisconsin.Value) });
            //}
            else if (!AcquiredExistingBusiness.Value
                && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PAID_EE_FLG, out var paidEeFlag))
            {
                HavePaidEmployeesForWorkInWisconsin = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(paidEeFlag.ReplyText);

                //if (visibilityQ3_1 && HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.STILL_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(HaveEmployeesCurrentlyWorkingInWisconsin.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(HaveEmployeesCurrentlyWorkingInWisconsin.Value) });
                //}
                if (HavePaidEmployeesForWorkInWisconsin.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.STILL_EE_FLG, out var haveEmployeesCurrentlyWorkingInWisconsin))
                {
                    HaveEmployeesCurrentlyWorkingInWisconsin = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(haveEmployeesCurrentlyWorkingInWisconsin.ReplyText);

                    //// InformationIsAccurate — shown when Q3.1 Yes (still have employees in WI)
                    //var visibilityCheckboxInfoAccurate = visibilityQ3_1 && HaveEmployeesCurrentlyWorkingInWisconsin == true;
                    //if (visibilityCheckboxInfoAccurate)
                    //{
                    //    responses.Add(new SurveyResponse()
                    //    {
                    //        _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_ACCURATE_INFO,
                    //        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(InformationIsAccurate),
                    //        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(InformationIsAccurate)
                    //    });
                    //}
                    if (HaveEmployeesCurrentlyWorkingInWisconsin.Value
                        && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_ACCURATE_INFO, out var informationAccurate))
                    {
                        InformationIsAccurate = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(informationAccurate.ReplyText);
                    }

                    //// SelectedNoEmployeeReason — shown when Q4 (Q3.1 No)
                    //if (visibilityQ4 && SelectedNoEmployeeReason.HasValue)
                    //{
                    //    var reasonDisplay = SelectedNoEmployeeReason.Value switch
                    //    {
                    //        NoEmployeeReason.BusinessActivityEnded => "Business activity has ended but business has not been sold",
                    //        NoEmployeeReason.NotOperatingInWisconsin => "No longer operating in Wisconsin but still operating in another state",
                    //        NoEmployeeReason.HaveSoldOrTransferredBusiness => "Business activity sold or transferred",
                    //        NoEmployeeReason.BusiessWithoutEmployees => "Business continuing without employees",
                    //        NoEmployeeReason.EmployingIndependentContractors => "Employing Independent Contractors",
                    //        NoEmployeeReason.Death => "Death",
                    //        NoEmployeeReason.LeasingFromPEO => "Leasing employees from Professional Employer Organization (PEO)",
                    //        NoEmployeeReason.FiscalAgent => "Fiscal Agent electing to be employer",
                    //        NoEmployeeReason.Other => "Other",
                    //        _ => SelectedNoEmployeeReason.Value.ToString()
                    //    };
                    //    responses.Add(new SurveyResponse()
                    //    {
                    //        _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_NO_LNGR_EE,
                    //        _response = SelectedNoEmployeeReason.Value.ToString(),
                    //        _responseDisplay = reasonDisplay
                    //    });
                    //}
                    else if (!HaveEmployeesCurrentlyWorkingInWisconsin.Value)
                    {
                        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_NO_LNGR_EE, out var noEEReason)
                            && Enum.TryParse<NoEmployeeReason>(noEEReason.ReplyText, out var noEEReasonValue))
                        {
                            SelectedNoEmployeeReason = noEEReasonValue;

                            if (SelectedNoEmployeeReason == NoEmployeeReason.LeasingFromPEO)
                            {
                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(PEOName))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NAME, _response = PEOName });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_PEO_NAME, out var peoName))
                                {
                                    PEOName = peoName.ReplyText;
                                }

                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(PEOUIAccountNumber))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOUIAccountNumber.Replace("-", string.Empty) });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_PEO_NUM, out var peoUiAccountNumber))
                                {
                                    PEOUIAccountNumber = IEmployerRegistrationModelSection.FormatUiAccountNumberResponseString(peoUiAccountNumber.ReplyText);
                                }

                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(PEOFEIN))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_NUM, _response = PEOFEIN.Replace("-", string.Empty) });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_PEO_NUM, out var peoFein))
                                {
                                    PEOFEIN = IEmployerRegistrationModelSection.FormatFeinResponseString(peoFein.ReplyText);
                                }

                                //if (visibilityQ4 && LeasingStartDate.HasValue)
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_PEO_DATE, _response = LeasingStartDate.Value.ToString("MM/dd/yyyy") });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_PEO_DATE, out var peoDate)
                                    && DateOnly.TryParse(peoDate.ReplyText, out var peoDateValue))
                                {
                                    LeasingStartDate = peoDateValue;
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.FiscalAgent)
                            {
                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(FiscalAgentName))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FSCL_AGNT_NAM, _response = FiscalAgentName });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_FSCL_AGNT_NAM, out var fiscalAgentName))
                                {
                                    FiscalAgentName = fiscalAgentName.ReplyText;
                                }

                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(FiscalAgentUIAccountNumber))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_FA_UI_ACCT_NUM, _response = FiscalAgentUIAccountNumber.Replace("-", string.Empty) });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_FA_UI_ACCT_NUM, out var fiscalAgentUiAccountNumber))
                                {
                                    FiscalAgentUIAccountNumber = IEmployerRegistrationModelSection.FormatUiAccountNumberResponseString(fiscalAgentUiAccountNumber.ReplyText);
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.BusiessWithoutEmployees)
                            {
                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(NoEmployeeExplanation))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_BUS_WITHOUT_EE, _response = NoEmployeeExplanation });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_BUS_WITHOUT_EE, out var noEeExplanation))
                                {
                                    NoEmployeeExplanation = noEeExplanation.ReplyText;
                                }
                            }

                            else if (SelectedNoEmployeeReason == NoEmployeeReason.Other)
                            {
                                //if (visibilityQ4 && !string.IsNullOrWhiteSpace(OtherReason))
                                //{
                                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_OTHR_RSN, _response = OtherReason });
                                //}
                                if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_OTHR_RSN, out var otherReason))
                                {
                                    OtherReason = otherReason.ReplyText;
                                }
                            }
                        }

                        //if (visibilityQ4 && LastEmploymentDate.HasValue)
                        //{
                        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_EMPL_DT, _response = LastEmploymentDate.Value.ToString("MM/dd/yyyy") });
                        //}
                        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LAST_EMPL_DT, out var lastEmploymentDate)
                            && DateOnly.TryParse(lastEmploymentDate.ReplyText, out var lastEmploymentDateValue))
                        {
                            LastEmploymentDate = lastEmploymentDateValue;
                        }

                        //if (visibilityQ4 && LastPayrollDate.HasValue)
                        //{
                        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LAST_PYRL_DT, _response = LastPayrollDate.Value.ToString("MM/dd/yyyy") });
                        //}
                        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LAST_PYRL_DT, out var lastPayrollDate)
                            && DateOnly.TryParse(lastPayrollDate.ReplyText, out var lastPayrollDateValue))
                        {
                            LastEmploymentDate = lastPayrollDateValue;
                        }
                    }
                }

                //if (visibilityQ3_2 && ExpectFuturePayroll.HasValue)
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(ExpectFuturePayroll.Value), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(ExpectFuturePayroll.Value) });
                //}
                else if (!HavePaidEmployeesForWorkInWisconsin.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.EXPT_PAY_EE_FLG, out var expectFuturePayroll))
                {
                    ExpectFuturePayroll = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(expectFuturePayroll.ReplyText);

                    //if (visibilityQ3_2_1 && ExpectedFuturePayrollPeriod.HasValue)
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.EXPT_PAY_EE_TIME, _response = ExpectedFuturePayrollPeriod.Value.ToString() });
                    //}
                    if (ExpectFuturePayroll.Value
                        && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.EXPT_PAY_EE_TIME, out var expectFuturePayrollPeriod))
                    {
                        if (Enum.TryParse<FuturePayPeriod>(expectFuturePayrollPeriod.ReplyText, out var expectFuturePayrollPeriodValue))
                        {
                            ExpectedFuturePayrollPeriod = expectFuturePayrollPeriodValue;
                        }
                        else
                        {
                            switch (expectFuturePayrollPeriod.ReplyText)
                            {
                                case "Within 30 days": ExpectedFuturePayrollPeriod = FuturePayPeriod.WithinThirtyDays; break;
                                case "30 to 90 days": ExpectedFuturePayrollPeriod = FuturePayPeriod.ThirtyToNinetyDays; break;
                                case "6 months": ExpectedFuturePayrollPeriod = FuturePayPeriod.SixMonths; break;
                                case "One year": ExpectedFuturePayrollPeriod = FuturePayPeriod.OneYear; break;
                                case "More than a year": ExpectedFuturePayrollPeriod = FuturePayPeriod.MoreThanOneYear; break;
                            }
                        }
                    }
                }
            }
        }
    }
}
