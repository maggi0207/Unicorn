using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.Shared.Extensions;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Ownership data to store in session
/// </summary>
public class OwnershipSessionData : IEmployerRegistrationModelSection
{
    /// <summary>
    /// OwnershipType
    /// </summary>
    public OwnershipType OwnershipType { get; set; } = OwnershipType.None;
    /// <summary>
    /// IsOutsideUSA
    /// </summary>
    public bool IsOutsideUSA { get; set; }

    /// <summary>
    /// RegistrationState For member-based forms (LLC, LLP, LP, Partnership)
    /// </summary>
    public string? RegistrationState { get; set; }
    /// <summary>
    /// Members
    /// </summary>
    public List<OwnerMember>? Members { get; set; }
    /// <summary>
    /// MoreThanFive
    /// </summary>
    [Required(ErrorMessage = "Please select an option.")]
    public bool? MoreThanFive { get; set; }

    /// <summary>
    /// IncorporationState - For corporation forms
    /// </summary>
    public string? IncorporationState { get; set; }
    /// <summary>
    /// ForeignCountry
    /// </summary>
    public string? ForeignCountry { get; set; }
    /// <summary>
    /// Officers
    /// </summary>
    public List<OwnerMember>? Officers { get; set; }

    /// <summary>
    /// Owner For sole proprietorship
    /// </summary>
    public OwnerMember? Owner { get; set; }

    /// <summary>
    /// Limited Partnership Name (for LP type)
    /// </summary>
    public string? LimitedPartnershipName { get; set; }

    /// <summary>
    /// General Partner (for LP type)
    /// </summary>
    public OwnerMember? GeneralPartner { get; set; }

    /// <summary>
    /// Decedent (for Estate type)
    /// </summary>
    public OwnerMember? Decedent { get; set; }
    /// <summary>
    /// Personal Representative (for Estate type)
    /// </summary>
    public OwnerMember? PersonalRepresentative { get; set; }


    /// <inheritdoc/>
    public List<SurveyContact> GetSurveyContacts()
    {
        return OwnershipType switch
        {
            OwnershipType.LLC or OwnershipType.LLP or OwnershipType.Partnership or OwnershipType.LLCCorporation => Members?.Where(x =>
            {
                return !string.IsNullOrWhiteSpace(x.FirstName) || !string.IsNullOrWhiteSpace(x.LastName);
            }).Select(m =>
            {
                return new SurveyContact
                {
                    _surveyIndividualCode = (int) RegistrationIndividualCode.Member,
                    _firstName = m.FirstName,
                    _lastName = m.LastName,
                    _middleName = m.MiddleInitial,
                    _ownershipPercentage = (m.OwnershipPercentage ?? 0).ToString(),
                    _socialSecurityNumber = !string.IsNullOrWhiteSpace(m.SSN) ? m.SSN.Replace("-", string.Empty) : string.Empty,
                };
            }).ToList() ?? new(),
            OwnershipType.LP => (string.IsNullOrWhiteSpace(GeneralPartner?.FirstName) && string.IsNullOrWhiteSpace(GeneralPartner?.LastName)) ? new() : new()
                {
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.General_Partner,
                        _firstName = GeneralPartner!.FirstName,
                        _lastName = GeneralPartner.LastName,
                        _middleName = GeneralPartner.MiddleInitial,
                        _ownershipPercentage = (GeneralPartner.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = !string.IsNullOrWhiteSpace(GeneralPartner.SSN) ? GeneralPartner.SSN.Replace("-", string.Empty) : string.Empty,
                    }
                },
            OwnershipType.Corporation => Officers?.Where(off => !string.IsNullOrWhiteSpace(off.FirstName) || !string.IsNullOrWhiteSpace(off.LastName)).Select(off =>
            {
                return new SurveyContact
                {
                    _surveyIndividualCode = OfficerRoleToRegistrationIndividualCode.TryGetValue(off.Role, out var value) ? (int) value : (int) RegistrationIndividualCode.NONE,
                    _firstName = off.FirstName,
                    _lastName = off.LastName,
                    _middleName = off.MiddleInitial,
                    _ownershipPercentage = (off.OwnershipPercentage ?? 0).ToString(),
                    _socialSecurityNumber = !string.IsNullOrWhiteSpace(off.SSN) ? off.SSN.Replace("-", string.Empty) : string.Empty,
                };
            }).ToList() ?? new(),
            OwnershipType.SoleProprietorship or OwnershipType.Individual => (string.IsNullOrWhiteSpace(Owner?.FirstName) && string.IsNullOrWhiteSpace(Owner?.LastName)) ? new() : new()
                {
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Sole_Proprietor,
                        _firstName = Owner!.FirstName,
                        _lastName = Owner.LastName,
                        _middleName = Owner.MiddleInitial,
                        _ownershipPercentage = (Owner?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = !string.IsNullOrWhiteSpace(Owner!.SSN) ? Owner!.SSN.Replace("-", string.Empty) : string.Empty,
                    },
                },
            OwnershipType.Estate => new[]
                {
                    (!string.IsNullOrWhiteSpace(Decedent?.FirstName) || !string.IsNullOrWhiteSpace(Decedent?.LastName)) ? new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Decedent,
                        _firstName = Decedent!.FirstName,
                        _lastName = Decedent.LastName,
                        _middleName = Decedent.MiddleInitial,
                        _ownershipPercentage = (Decedent?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = !string.IsNullOrWhiteSpace(Decedent!.SSN) ? Decedent!.SSN.Replace("-", string.Empty) : string.Empty,
                    } : null,
                    (!string.IsNullOrWhiteSpace(PersonalRepresentative?.FirstName) || !string.IsNullOrWhiteSpace(PersonalRepresentative?.LastName)) ? new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Personal_Rep,
                        _firstName = PersonalRepresentative!.FirstName,
                        _lastName = PersonalRepresentative.LastName,
                        _middleName = PersonalRepresentative.MiddleInitial,
                        _ownershipPercentage = (PersonalRepresentative?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = !string.IsNullOrWhiteSpace(PersonalRepresentative!.SSN) ? PersonalRepresentative!.SSN.Replace("-", string.Empty) : string.Empty,
                    } : null
                }.OfType<SurveyContact>().ToList(),
            _ => new(),
        };
    }

    /// <inheritdoc/>
    public void LoadSurveyContacts(RegistrationIndividualProxy[] contacts)
    {
        switch (OwnershipType)
        {
            case OwnershipType.LLC:
            case OwnershipType.LLP:
            case OwnershipType.LLCCorporation:
            case OwnershipType.Partnership:
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.Member, out var memberMatches))
                {
                    Members = memberMatches.Select(c =>
                    {
                        return ConvertRegistrationIndividualProxyToOwnerMember(c)!;
                    }).ToList();
                }
                break;
            case OwnershipType.LP:
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.General_Partner,
                        out var generalPartners))
                {
                    GeneralPartner = ConvertRegistrationIndividualProxyToOwnerMember(generalPartners.FirstOrDefault());
                }
                break;
            case OwnershipType.Cooperative:
                Officers = contacts.Select(c =>
                {
                    var hasContactCode = RegistrationIndividualCodeToOfficerRole.TryGetValue((RegistrationIndividualCode)
                                c.RegistrationIndividualCodeSK, out var officerRole);

                    var individual = ConvertRegistrationIndividualProxyToOwnerMember(c)!;

                    individual.Role = officerRole!;

                    return hasContactCode ? individual : null;
                })
                    .OfType<OwnerMember>()
                    .ToList();
                break;
            case OwnershipType.SoleProprietorship:
            case OwnershipType.Individual:
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.Sole_Proprietor,
                    out var solePropietors))
                {
                    Owner = ConvertRegistrationIndividualProxyToOwnerMember(solePropietors.FirstOrDefault());
                }
                break;
            case OwnershipType.Estate:
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.Decedent, out var decedents))
                {
                    Decedent = ConvertRegistrationIndividualProxyToOwnerMember(decedents.FirstOrDefault());
                }
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.Personal_Rep, out var personalReps))
                {
                    PersonalRepresentative = ConvertRegistrationIndividualProxyToOwnerMember(personalReps.FirstOrDefault());
                }
                break;
            case OwnershipType.Corporation:
                Officers = contacts.Select(c =>
                {
                    var hasContactCode = RegistrationIndividualCodeToOfficerRole.TryGetValue((RegistrationIndividualCode)
                        c.RegistrationIndividualCodeSK, out var officerRole);

                    var individual = ConvertRegistrationIndividualProxyToOwnerMember(c)!;

                    individual.Role = officerRole!;

                    return hasContactCode ? individual : null;
                })
                  .OfType<OwnerMember>()
                  .ToList();
                break;

        }
    }

    private OwnerMember? ConvertRegistrationIndividualProxyToOwnerMember(RegistrationIndividualProxy? individual)
    {
        if (individual != null)
        {
            var hasOwnerShipPercentage = decimal.TryParse(individual.OwnershipPercentage, out var ownerShipPercentageValue);
            var ownerMember = new OwnerMember();

            ownerMember.FirstName = individual.FirstName;
            ownerMember.LastName = individual.LastName;
            ownerMember.MiddleInitial = individual.MiddleName;
            ownerMember.OwnershipPercentage = hasOwnerShipPercentage ? ownerShipPercentageValue : 0;
            ownerMember.SSN = string.IsNullOrWhiteSpace(individual.SocialSecurityNumber) ? string.Empty : individual.SocialSecurityNumber.Replace("-", string.Empty);

            return ownerMember;
        }
        return null;
    }

    /// <summary>
    /// Corporate Officer Services data (UCT-10056-E) — for Corporation and LLCCorporation
    /// when registrant has not paid and does not expect to pay employees
    /// </summary>
    public CorporateOfficerServicesModel? CorporateOfficerServices { get; set; }

    /// <summary>
    /// LLC Corporation documentation data — for LLCCorporation type
    /// </summary>
    public LlcDocumentationModel? LlcDocumentation { get; set; }

    /// <summary>
    /// Qualified Settlement Fund
    /// </summary>
    public QualifiedSettlementFundModel? QualifiedSettlementFund { get; set; }

    /// <summary>
    /// OwnerShipAgency Model
    /// </summary>
    public OwnershipAgency? OwnershipAgencies { get; set; } = new();

    /// <inheritdoc/>
    public List<SurveyResponse> GetSurveyResponses()
    {
        var responses = new List<SurveyResponse>();

        if (OwnershipType != OwnershipType.None)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.OWNR_CLS_CD_SK,
                _response = $"{(int) OwnershipType}",
                _responseDisplay = OwnershipType.GetDisplayName()
            });
        }

        if (OwnershipType == OwnershipType.Corporation)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.OUT_US,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsOutsideUSA),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsOutsideUSA)
            });

            //responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.OUT_US, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsOutsideUSA), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsOutsideUSA) });

            if (!IsOutsideUSA && !string.IsNullOrWhiteSpace(IncorporationState))
            {
                var stateName = AddressModel.States.FirstOrDefault(s =>
                {
                    return s.Value == IncorporationState;
                })?.Text ?? IncorporationState;
                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_ST_CD,
                    _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(IncorporationState).ToString(),
                    _responseDisplay = stateName
                });
            }

            if (IsOutsideUSA && ForeignCountry != null)
            {
                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_FGN_CTRY_NAM,
                    _response = ForeignCountry
                });
            }
        }
        if (OwnershipType is OwnershipType.LLC or OwnershipType.LLP or OwnershipType.LLCCorporation
            && RegistrationState != null)
        {
            var stateName = AddressModel.States.FirstOrDefault(s =>
            {
                return s.Value == RegistrationState;
            })?.Text ?? RegistrationState;

            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.LLC_RGST_ST_CD,
                _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(RegistrationState).ToString(),
                _responseDisplay = stateName
            });
        }

        if (OwnershipType is OwnershipType.LLC or OwnershipType.LLCCorporation
            && MoreThanFive.HasValue)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.LLC_OVR_FIV_MBR_FLG,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(MoreThanFive.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(MoreThanFive.Value)
            });
        }

        if (OwnershipType == OwnershipType.LP
            && LimitedPartnershipName != null)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.LMT_PTNR_NAM,
                _response = LimitedPartnershipName
            });
        }

        if (OwnershipType == OwnershipType.Partnership
            && MoreThanFive.HasValue)
        {
            responses.Add(new SurveyResponse()
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.PTNRSP_OVR_FIV_PTNR_FLG,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(MoreThanFive.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(MoreThanFive.Value)
            });
        }

        if (OwnershipType is OwnershipType.CityGovernmentAgency or OwnershipType.CountyGovernmentAgency or
           OwnershipType.FederalGovernmentAgency or OwnershipType.LocalGovernmentUnitNotListed or OwnershipType.SchoolDistrict or
           OwnershipType.StateGovernmentAgency or OwnershipType.StateGovernmentUnitNotListed or OwnershipType.Township or
           OwnershipType.Village or OwnershipType.IndianTribe
           )
        {
            if (OwnershipAgencies != null)
            {
                if (!string.IsNullOrWhiteSpace(OwnershipAgencies.Filepath))
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.GOV_EMP_DOC_UPLD,
                        _response = OwnershipAgencies.Filepath,
                        _responseDisplay = Path.GetFileName(OwnershipAgencies.Filepath)?.RemoveGUID()
                    });
                }
                else
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.GOV_EMP_DOC_UPLD,
                        _response = string.Empty,
                        _responseDisplay = "No"
                    });
                }

                responses.Add(new SurveyResponse()
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.MISS_UPLD_GOV_EMP_DOC,
                    _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(OwnershipAgencies.NoHasFile!),
                    _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(OwnershipAgencies.NoHasFile!)
                });
            }
        }

        if (OwnershipType is OwnershipType.LLCCorporation or OwnershipType.Corporation)
        {
            // 3082-3084: Corporate Officer Services
            if (CorporateOfficerServices != null)
            {
                if (CorporateOfficerServices.OfficersPerformServices.HasValue)
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PRFM_SRVC,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(CorporateOfficerServices.OfficersPerformServices.Value),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(
                            CorporateOfficerServices.OfficersPerformServices.Value)
                    });
                }

                if (CorporateOfficerServices.OfficersPerformServices == true && CorporateOfficerServices.ApproximatePayDate.HasValue)
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PAID_DT,
                        _response = CorporateOfficerServices.ApproximatePayDate.Value.ToString("MM/dd/yyyy")
                    });
                }

                if (CorporateOfficerServices.OfficersPerformServices == true && !string.IsNullOrEmpty(CorporateOfficerServices.NoPayExplanation))
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_NO_PAY_RSN,
                        _response = CorporateOfficerServices.NoPayExplanation
                    });
                }
            }

            // 3085-3091: LLC Documentation
            if (LlcDocumentation != null)
            {
                // 3085: Do you have the required documentation available to upload?
                if (LlcDocumentation.HasRequiredDocumentation.HasValue)
                {
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.LLD_REQ_DOC_AVL,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(
                            LlcDocumentation.HasRequiredDocumentation.Value),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(
                            LlcDocumentation.HasRequiredDocumentation.Value)
                    });
                }

                if (LlcDocumentation.HasRequiredDocumentation == true)
                {
                    // 3086: I will supply required documentation at a later date.
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(
                            LlcDocumentation.WillSupplyDocumentationLater),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(
                            LlcDocumentation.WillSupplyDocumentationLater)
                    });

                    // 3090: Uploaded file
                    if (!string.IsNullOrEmpty(LlcDocumentation.FilePath))
                    {
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_UPLD,
                            _response = LlcDocumentation.FilePath,
                            _responseDisplay = Path.GetFileName(LlcDocumentation.FilePath)?.RemoveGUID()
                        });
                    }
                }

                if (LlcDocumentation.HasRequiredDocumentation == false)
                {
                    // 3087: Reason documentation cannot be submitted
                    if (LlcDocumentation.NoDocumentationReason.HasValue)
                    {
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC_RSN,
                            _response = LlcDocumentation.NoDocumentationReason.Value.ToString()
                        });
                    }

                    // 3088: When do you plan to submit your application to the IRS?
                    if (LlcDocumentation.PlannedSubmissionDate.HasValue)
                    {
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_PLAN_SBMT_DT,
                            _response = LlcDocumentation.PlannedSubmissionDate.Value.ToString("MM/dd/yyyy")
                        });
                    }

                    // 3089: What date was the application submitted to the IRS?
                    if (LlcDocumentation.ApplicationSubmittedDate.HasValue)
                    {
                        responses.Add(new SurveyResponse()
                        {
                            _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_APP_SBMT_DT,
                            _response = LlcDocumentation.ApplicationSubmittedDate.Value.ToString("MM/dd/yyyy")
                        });
                    }

                    // 3091: I acknowledge that I will submit the required documentation
                    responses.Add(new SurveyResponse()
                    {
                        _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_LLC_ELEC_CORP,
                        _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(
                            LlcDocumentation.AcknowledgeSubmitDocumentation),
                        _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(
                            LlcDocumentation.AcknowledgeSubmitDocumentation)
                    });
                }
            }
        }
        // OwnershipType.QSF even though the user selected QSF. The sub-model is reliable.
        if (QualifiedSettlementFund is { } qsf)
        {
            AddQsfResponses(responses, qsf);
        }
        return responses;
    }

    /// <summary>
    /// Appends QSF-specific survey responses to the provided list.
    /// </summary>
    private static void AddQsfResponses(List<SurveyResponse> responses, QualifiedSettlementFundModel qsf)
    {
        // Were payments for services performed in Wisconsin?
        if (qsf.PaymentsForServices.HasValue)
        {
            responses.Add(new SurveyResponse
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.PAY_SRVS_QSF,
                _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(qsf.PaymentsForServices.Value),
                _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(qsf.PaymentsForServices.Value)
            });
        }

        if (qsf.PaymentsForServices == true)
        {
            // Legal name of the entity that received the services
            if (!string.IsNullOrWhiteSpace(qsf.EntityLegalName))
            {
                responses.Add(new SurveyResponse
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_LGL_NAM,
                    _response = qsf.EntityLegalName
                });
            }

            // Federal ID Number (FEIN)
            if (!string.IsNullOrWhiteSpace(qsf.FederalIdNumber))
            {
                responses.Add(new SurveyResponse
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_FEIN,
                    _response = qsf.FederalIdNumber.Replace("-", string.Empty),
                    _responseDisplay = qsf.FederalIdNumber
                });
            }

            // Wisconsin UI Account Number (optional)
            if (!string.IsNullOrWhiteSpace(qsf.WisconsinUiAccountNumber))
            {
                responses.Add(new SurveyResponse
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_UI_ACCT_NUM,
                    _response = qsf.WisconsinUiAccountNumber.Replace("-", string.Empty),
                    _responseDisplay = qsf.WisconsinUiAccountNumber
                });
            }

            // Wisconsin address where services were performed — stored as a
            // formatted text response (QSF_OWNR_WI_ADDRESS) so it appears in
            // the verification page without requiring a new RegistrationAddressCode.
            var addressDisplay = FormatQsfServiceAddress(qsf.ServiceAddress);
            if (!string.IsNullOrWhiteSpace(addressDisplay))
            {
                responses.Add(new SurveyResponse
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_WI_ADDRESS,
                    _response = addressDisplay,
                    _responseDisplay = addressDisplay
                });
            }
        }
        else if (qsf.PaymentsForServices == false)
        {
            // Reason for payments when not for services in Wisconsin
            if (!string.IsNullOrWhiteSpace(qsf.PaymentReason))
            {
                responses.Add(new SurveyResponse
                {
                    _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_PYMT_REASONS,
                    _response = qsf.PaymentReason
                });
            }
        }

        // Court-approved settlement agreement — file uploaded or "provide later"
        if (!string.IsNullOrWhiteSpace(qsf.FilePath))
        {
            responses.Add(new SurveyResponse
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_COURT_APR_STLMT,
                _response = qsf.FilePath,
                _responseDisplay = System.IO.Path.GetFileName(qsf.FilePath)?.RemoveGUID()
            });
        }
        else
        {
            responses.Add(new SurveyResponse
            {
                _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_COURT_APR_STLMT,
                _response = string.Empty,
                _responseDisplay = "No"
            });
        }
        responses.Add(new SurveyResponse
        {
            _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_QSF_STL_AGR,
            _response = qsf.WillProvideDocumentationLater ? "Yes" : "No",
            _responseDisplay = qsf.WillProvideDocumentationLater ? "Yes" : "No"
        });
    }

    /// <summary>
    /// Formats the QSF service address fields into a single display string.
    /// Returns an empty string if no address fields have been filled in.
    /// </summary>
    private static string FormatQsfServiceAddress(QsfServiceAddressModel address)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(address.AddressLine1))
        {
            parts.Add(address.AddressLine1);
        }

        if (!string.IsNullOrWhiteSpace(address.AddressLine2))
        {
            parts.Add(address.AddressLine2);
        }

        var cityStateParts = new List<string>();
        if (!string.IsNullOrWhiteSpace(address.City))
        {
            cityStateParts.Add(address.City);
        }

        if (!string.IsNullOrWhiteSpace(address.State))
        {
            cityStateParts.Add(address.State);
        }

        if (cityStateParts.Any())
        {
            var zip = address.ZipCode ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(address.ZipExtension))
            {
                zip += $"-{address.ZipExtension}";
            }

            parts.Add(string.IsNullOrWhiteSpace(zip)
                ? string.Join(", ", cityStateParts)
                : $"{string.Join(", ", cityStateParts)} {zip}");
        }

        return string.Join(", ", parts);
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses)
    {
        //if (OwnershipType != OwnershipType.None)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.OWNR_CLS_CD_SK, _response = $"{(int) OwnershipType}", _responseDisplay = OwnershipType.GetDisplayName() });
        //}
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.OWNR_CLS_CD_SK, out var ownershipTypeValue)
            && Enum.TryParse<OwnershipType>(ownershipTypeValue.ReplyText, out var ownershipTypeCodeValue))
        {
            OwnershipType = ownershipTypeCodeValue;
        }

        //responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.OUT_US, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(IsOutsideUSA), _responseDisplay = IEmployerRegistrationModelSection.ConvertBooleanResponseToDisplayString(IsOutsideUSA) });
        if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.OUT_US, out var outsideUS))
        {
            IsOutsideUSA = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(outsideUS.ReplyText);
        }

        //if (!IsOutsideUSA && IncorporationState != null)
        //{
        //    var stateName = AddressModel.States.FirstOrDefault(s =>
        //    {
        //        return s.Value == IncorporationState;
        //    })?.Text ?? IncorporationState;
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_ST_CD, _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(IncorporationState).ToString(), _responseDisplay = stateName });
        //}
        if (!IsOutsideUSA
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ICRP_ST_CD, out var incorporationStateValue)
            && int.TryParse(incorporationStateValue.ReplyText, out var incorporationStateCodeValue))
        {
            IncorporationState = EmployerRegistrationModelStore.GetStateProviceCodeFromAbbreviation(incorporationStateCodeValue);
        }

        //if (IsOutsideUSA && ForeignCountry != null)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_FGN_CTRY_NAM, _response = ForeignCountry });
        //}
        if (IsOutsideUSA
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.ICRP_FGN_CTRY_NAM, out var foreignCountryValue))
        {
            ForeignCountry = foreignCountryValue.ReplyText;
        }

        //if (OwnershipType is OwnershipType.LLC or OwnershipType.LLP
        //    && RegistrationState != null)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_RGST_ST_CD, _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(RegistrationState).ToString() });
        //}
        if (OwnershipType is OwnershipType.LLC or OwnershipType.LLP or OwnershipType.LLCCorporation
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_RGST_ST_CD, out var registrationStateValue)
            && int.TryParse(registrationStateValue.ReplyText, out var registrationStateCodeValue))
        {
            RegistrationState = EmployerRegistrationModelStore.GetStateProviceCodeFromAbbreviation(registrationStateCodeValue);
        }

        //if (OwnershipType == OwnershipType.LLC
        //    && MoreThanFive.HasValue)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_OVR_FIV_MBR_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(MoreThanFive.Value) });
        //}
        if (OwnershipType is OwnershipType.LLC or OwnershipType.LLCCorporation
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_OVR_FIV_MBR_FLG, out var moreThanFiveValue))
        {
            MoreThanFive = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(moreThanFiveValue.ReplyText);
        }

        //if (OwnershipType == OwnershipType.LP
        //    && LimitedPartnershipName != null)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LMT_PTNR_NAM, _response = LimitedPartnershipName });
        //}
        if (OwnershipType == OwnershipType.LP
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LMT_PTNR_NAM, out var limitedPartnershipNameValue))
        {
            LimitedPartnershipName = limitedPartnershipNameValue.ReplyText;
        }

        //if (OwnershipType == OwnershipType.Partnership
        //    && MoreThanFive.HasValue)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PTNRSP_OVR_FIV_PTNR_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(MoreThanFive.Value) });
        //}
        if (OwnershipType == OwnershipType.Partnership
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PTNRSP_OVR_FIV_PTNR_FLG, out var moreThanFivePartnershipValue))
        {
            MoreThanFive = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(moreThanFivePartnershipValue.ReplyText);
        }

        //if (OwnershipType != OwnershipType.QSF
        //    && Owner != null)
        //{
        //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_LGL_NAM, _response = IEmployerRegistrationModelSection.ConcatenateLegalName(Owner.FirstName, Owner.MiddleInitial, Owner.LastName) });
        //}
        if (OwnershipType != OwnershipType.QSF
            && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_LGL_NAM, out var qsfOwnerName)
            && IEmployerRegistrationModelSection.DeconstructLegalName(qsfOwnerName.ReplyText, out var ownerFirstName, out var ownerMiddleInitial, out var ownerLastName))
        {
            Owner ??= new();
            Owner.FirstName = ownerFirstName ?? string.Empty;
            Owner.MiddleInitial = ownerMiddleInitial ?? string.Empty;
            Owner.LastName = ownerLastName ?? string.Empty;
        }

        var isQsf = OwnershipType == OwnershipType.QSF ||
                    (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.OWNR_CLS_CD_SK, out var ownrCls) && ownrCls.ReplyText == "28") ||
                    responses.Any(r => r.QuestionSetItemSK is ((int) SurveyResponseItem.PAY_SRVS_QSF) or
                                        ((int) SurveyResponseItem.QSF_OWNR_COURT_APR_STLMT) or
                                        ((int) SurveyResponseItem.QSF_OWNR_FEIN) or
                                        ((int) SurveyResponseItem.QSF_OWNR_WI_ADDRESS));

        if (isQsf)
        {
            QualifiedSettlementFund ??= new();

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PAY_SRVS_QSF, out var paySrvsQsf))
            {
                QualifiedSettlementFund.PaymentsForServices = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(paySrvsQsf.ReplyText);
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_LGL_NAM, out var qsfLegalNameQsf))
            {
                QualifiedSettlementFund.EntityLegalName = qsfLegalNameQsf.ReplyText;
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_FEIN, out var qsfFein))
            {
                QualifiedSettlementFund.FederalIdNumber = qsfFein.ReplyText;
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_UI_ACCT_NUM, out var qsfUiAcct))
            {
                QualifiedSettlementFund.WisconsinUiAccountNumber = qsfUiAcct.ReplyText;
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_PYMT_REASONS, out var qsfPymtReasons))
            {
                QualifiedSettlementFund.PaymentReason = qsfPymtReasons.ReplyText;
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_COURT_APR_STLMT, out var qsfCourtApr)
                && !string.IsNullOrWhiteSpace(qsfCourtApr.ReplyText))
            {
                QualifiedSettlementFund.FilePath = qsfCourtApr.ReplyText;
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_QSF_STL_AGR, out var qsfMissDoc))
            {
                QualifiedSettlementFund.WillProvideDocumentationLater = qsfMissDoc.ReplyText.Equals("Yes", StringComparison.OrdinalIgnoreCase) ||
                                                                        IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(qsfMissDoc.ReplyText);
            }

            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.QSF_OWNR_WI_ADDRESS, out var qsfWiAddress))
            {
                var text = qsfWiAddress.ReplyText;
                var parts = text.Split(new[] { ", " }, StringSplitOptions.RemoveEmptyEntries).ToList();
                if (parts.Count > 0)
                {
                    var lastPart = parts.Last();
                    var stateZip = lastPart.Split(' ');
                    if (stateZip.Length >= 1)
                    {
                        var potentialState = stateZip[0];
                        if (potentialState.Length == 2)
                        {
                            QualifiedSettlementFund.ServiceAddress.State = potentialState;
                            if (stateZip.Length > 1)
                            {
                                var zipParts = stateZip[1].Split('-');
                                QualifiedSettlementFund.ServiceAddress.ZipCode = zipParts[0];
                                if (zipParts.Length > 1)
                                {
                                    QualifiedSettlementFund.ServiceAddress.ZipExtension = zipParts[1];
                                }
                            }
                            parts.RemoveAt(parts.Count - 1);

                            if (parts.Count > 0)
                            {
                                QualifiedSettlementFund.ServiceAddress.City = parts.Last();
                                parts.RemoveAt(parts.Count - 1);
                            }

                            if (parts.Count > 0)
                            {
                                QualifiedSettlementFund.ServiceAddress.AddressLine1 = parts[0];
                            }

                            if (parts.Count > 1)
                            {
                                QualifiedSettlementFund.ServiceAddress.AddressLine2 = parts[1];
                            }
                        }
                        else
                        {
                            QualifiedSettlementFund.ServiceAddress.AddressLine1 = text;
                        }
                    }
                }
            }
        }


        if (OwnershipType is OwnershipType.CityGovernmentAgency or OwnershipType.CountyGovernmentAgency or OwnershipType.FederalGovernmentAgency or OwnershipType.LocalGovernmentUnitNotListed or OwnershipType.SchoolDistrict or OwnershipType.StateGovernmentAgency or OwnershipType.StateGovernmentUnitNotListed or OwnershipType.Township or OwnershipType.Village
            )
        {
            OwnershipAgencies ??= new();
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.GOV_EMP_DOC_UPLD, out var hasFileValue)
                && !string.IsNullOrWhiteSpace(hasFileValue.ReplyText))
            {
                OwnershipAgencies.Filepath = hasFileValue.ReplyText;
            }
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.MISS_UPLD_GOV_EMP_DOC, out var noHasFileValue))
            {
                OwnershipAgencies.NoHasFile = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(noHasFileValue.ReplyText);
            }

        }

        ////Adding corporation by sv
        //if(OwnershipType == OwnershipType.Corporation)
        //     && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.RGST_PRIN_ACTV_CD_SK, out var has
        //{

        //}
        //if (OwnershipType == OwnershipType.LLCCorporation)
        //{
        if (OwnershipType == OwnershipType.LLCCorporation)
        {
            //// 3082-3084: Corporate Officer Services
            //if (CorporateOfficerServices != null)
            //{
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CORP_OFC_PRFM_SRVC, out var corporateOfficerPerformingServicesValue))
            {
                CorporateOfficerServices ??= new();

                //if (CorporateOfficerServices.OfficersPerformServices.HasValue)
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PRFM_SRVC, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(CorporateOfficerServices.OfficersPerformServices.Value) });
                //}
                CorporateOfficerServices.OfficersPerformServices = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(corporateOfficerPerformingServicesValue.ReplyText);

                if (CorporateOfficerServices.OfficersPerformServices.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CORP_OFC_PAID_DT, out var corporateOfficerPayDate)
                    && DateOnly.TryParse(corporateOfficerPayDate.ReplyText, out var corporateOfficerPayDateValue))
                {
                    //if (CorporateOfficerServices.OfficersPerformServices == true && CorporateOfficerServices.ApproximatePayDate.HasValue)
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PAID_DT, _response = CorporateOfficerServices.ApproximatePayDate.Value.ToString("MM/dd/yyyy") });
                    //}
                    CorporateOfficerServices.ApproximatePayDate = corporateOfficerPayDateValue;
                }

                if (CorporateOfficerServices.OfficersPerformServices.Value
                    && IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.CORP_OFC_NO_PAY_RSN, out var corporateOfficerNoPayReason))
                {
                    //if (CorporateOfficerServices.OfficersPerformServices == false && !string.IsNullOrEmpty(CorporateOfficerServices.NoPayExplanation))
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_NO_PAY_RSN, _response = CorporateOfficerServices.NoPayExplanation });
                    //}
                    CorporateOfficerServices.NoPayExplanation = corporateOfficerNoPayReason.ReplyText;
                }
            }

            //    // 3085-3091: LLC Documentation
            //    if (LlcDocumentation != null)
            //    {
            if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLD_REQ_DOC_AVL, out var lldHasDocumentation))
            {
                //// 3085: Do you have the required documentation available to upload?
                //if (LlcDocumentation.HasRequiredDocumentation.HasValue)
                //{
                //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLD_REQ_DOC_AVL, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(LlcDocumentation.HasRequiredDocumentation.Value) });
                //}
                LlcDocumentation ??= new();
                LlcDocumentation.HasRequiredDocumentation = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(lldHasDocumentation.ReplyText);

                //if (LlcDocumentation.HasRequiredDocumentation == true)
                //{
                if (LlcDocumentation.HasRequiredDocumentation.Value)
                {
                    //// 3086: I will supply required documentation at a later date.
                    //responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(LlcDocumentation.WillSupplyDocumentationLater) });
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_ELEC_CORP_DOC, out var willSupplyDocumentationLater))
                    {
                        LlcDocumentation.WillSupplyDocumentationLater = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(willSupplyDocumentationLater.ReplyText);
                    }

                    //// 3090: Uploaded file
                    //if (!string.IsNullOrEmpty(LlcDocumentation.FilePath))
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_UPLD, _response = LlcDocumentation.FilePath });
                    //}
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_ELEC_CORP_UPLD, out var uploadFilePath))
                    {
                        LlcDocumentation.FilePath = uploadFilePath.ReplyText;
                    }
                }

                //if (LlcDocumentation.HasRequiredDocumentation == false)
                //{
                if (!LlcDocumentation.HasRequiredDocumentation.Value)
                {
                    //// 3087: Reason documentation cannot be submitted
                    //if (LlcDocumentation.NoDocumentationReason.HasValue)
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC_RSN, _response = LlcDocumentation.NoDocumentationReason.Value.ToString() });
                    //}
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_ELEC_CORP_DOC_RSN, out var noDocumentationReason)
                        && Enum.TryParse<NoDocReason>(noDocumentationReason.ReplyText, out var noDocumentationReasonValue))
                    {
                        LlcDocumentation.NoDocumentationReason = noDocumentationReasonValue;
                    }

                    //// 3088: When do you plan to submit your application to the IRS?
                    //if (LlcDocumentation.PlannedSubmissionDate.HasValue)
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_PLAN_SBMT_DT, _response = LlcDocumentation.PlannedSubmissionDate.Value.ToString("MM/dd/yyyy") });
                    //}
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_ELEC_CORP_PLAN_SBMT_DT, out var plannedSubmissionDate)
                        && DateOnly.TryParse(plannedSubmissionDate.ReplyText, out var plannedSubmissionDateValue))
                    {
                        LlcDocumentation.PlannedSubmissionDate = plannedSubmissionDateValue;
                    }

                    //// 3089: What date was the application submitted to the IRS?
                    //if (LlcDocumentation.ApplicationSubmittedDate.HasValue)
                    //{
                    //    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_APP_SBMT_DT, _response = LlcDocumentation.ApplicationSubmittedDate.Value.ToString("MM/dd/yyyy") });
                    //}
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.LLC_ELEC_CORP_APP_SBMT_DT, out var applicationSubmittedDate)
                        && DateOnly.TryParse(applicationSubmittedDate.ReplyText, out var applicationSubmittedDateValue))
                    {
                        LlcDocumentation.ApplicationSubmittedDate = applicationSubmittedDateValue;
                    }

                    //// 3091: I acknowledge that I will submit the required documentation
                    //responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_LLC_ELEC_CORP, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToSummaryString(LlcDocumentation.AcknowledgeSubmitDocumentation) });
                    if (IEmployerRegistrationModelSection.FindResultHelper(responses, SurveyResponseItem.PRTL_MISS_DOC_LLC_ELEC_CORP, out var ackSubmitDocs))
                    {
                        LlcDocumentation.AcknowledgeSubmitDocumentation = IEmployerRegistrationModelSection.ConvertResponseStringToBoolean(ackSubmitDocs.ReplyText);
                    }
                }
            }
        }
    }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses) { }

    /// <inheritdoc/>
    public void PutAddressSKs(RegistrationAddressProxy[] addresses) { }

    private static readonly Dictionary<string, RegistrationIndividualCode> OfficerRoleToRegistrationIndividualCode = new()
    {
        { "President", RegistrationIndividualCode.President },
        { "Vice President", RegistrationIndividualCode.Vice_President },
        { "Secretary", RegistrationIndividualCode.Secretary },
        { "Treasurer", RegistrationIndividualCode.Treasurer },
    };

    private static readonly Dictionary<RegistrationIndividualCode, string> RegistrationIndividualCodeToOfficerRole = new()
    {
        { RegistrationIndividualCode.President, "President" },
        { RegistrationIndividualCode.Vice_President, "Vice President" },
        { RegistrationIndividualCode.Secretary, "Secretary" },
        { RegistrationIndividualCode.Treasurer, "Treasurer" },
    };
}
