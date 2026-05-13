using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

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
            OwnershipType.LLC or OwnershipType.LLP or OwnershipType.Partnership => Members?.Select(m =>
                {
                    return new SurveyContact
                    {
                        _surveyIndividualCode = (int) RegistrationIndividualCode.Member,
                        _firstName = m.FirstName,
                        _lastName = m.LastName,
                        _middleName = m.MiddleInitial,
                        _ownershipPercentage = (m.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = m.SSN,
                    };
                }).ToList() ?? new(),
            OwnershipType.LP => new()
                {
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.General_Partner,
                        _firstName = GeneralPartner!.FirstName,
                        _lastName = GeneralPartner.LastName,
                        _middleName = GeneralPartner.MiddleInitial,
                        _ownershipPercentage = (GeneralPartner.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = GeneralPartner.SSN,
                    }
                },
            OwnershipType.Corporation or OwnershipType.LLCCorporation => Officers?.Select(off =>
                {
                    return new SurveyContact
                    {
                        _surveyIndividualCode = OfficerRoleToRegistrationIndividualCode.TryGetValue(off.Role, out var value) ? (int) value : (int) RegistrationIndividualCode.NONE,
                        _firstName = off.FirstName,
                        _lastName = off.LastName,
                        _middleName = off.MiddleInitial,
                        _ownershipPercentage = (off.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = off.SSN,
                    };
                }).ToList() ?? new(),
            OwnershipType.SoleProprietorship or OwnershipType.Individual => new()
                {
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Sole_Proprietor,
                        _firstName = Owner!.FirstName,
                        _lastName = Owner.LastName,
                        _middleName = Owner.MiddleInitial,
                        _ownershipPercentage = (Owner?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = Owner!.SSN,
                    },
                },
            OwnershipType.Estate => new()
                {
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Decedent,
                        _firstName = Decedent!.FirstName,
                        _lastName = Decedent.LastName,
                        _middleName = Decedent.MiddleInitial,
                        _ownershipPercentage = (Decedent?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = Decedent!.SSN,
                    },
                    new SurveyContact
                    {
                        _surveyIndividualCode = (int)RegistrationIndividualCode.Personal_Rep,
                        _firstName = PersonalRepresentative!.FirstName,
                        _lastName = PersonalRepresentative.LastName,
                        _middleName = PersonalRepresentative.MiddleInitial,
                        _ownershipPercentage = (PersonalRepresentative?.OwnershipPercentage ?? 0).ToString(),
                        _socialSecurityNumber = PersonalRepresentative!.SSN,
                    },
                },
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
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.General_Partner, out var generalPartners))
                {
                    GeneralPartner = ConvertRegistrationIndividualProxyToOwnerMember(generalPartners.FirstOrDefault());
                }
                break;
            case OwnershipType.Cooperative:
            case OwnershipType.LLCCorporation:
                Officers = contacts.Select(c =>
                    {
                        var hasContactCode = RegistrationIndividualCodeToOfficerRole.TryGetValue((RegistrationIndividualCode) c.RegistrationIndividualCodeSK, out var officerRole);

                        var individual = ConvertRegistrationIndividualProxyToOwnerMember(c)!;

                        individual.Role = officerRole!;

                        return hasContactCode ? individual : null;
                    })
                    .OfType<OwnerMember>()
                    .ToList();
                break;
            case OwnershipType.SoleProprietorship:
            case OwnershipType.Individual:
                if (IEmployerRegistrationModelSection.FindContactsHelper(contacts, RegistrationIndividualCode.Sole_Proprietor, out var solePropietors))
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
            ownerMember.SSN = individual.SocialSecurityNumber;

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
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.OWNR_CLS_CD_SK, _response = $"{(int) OwnershipType}", _responseDisplay = OwnershipType.ToString() });
        }

        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.OUT_US, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(IsOutsideUSA) });

        if (!IsOutsideUSA && IncorporationState != null)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_ST_CD, _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(IncorporationState).ToString() });
        }

        if (IsOutsideUSA && ForeignCountry != null)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.ICRP_FGN_CTRY_NAM, _response = ForeignCountry });
        }

        if ((new List<OwnershipType> { OwnershipType.LLC, OwnershipType.LLP }).Contains(OwnershipType)
            && RegistrationState != null)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_RGST_ST_CD, _response = EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(RegistrationState).ToString() });
        }

        if (OwnershipType == OwnershipType.LLC
            && MoreThanFive.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_OVR_FIV_MBR_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(MoreThanFive.Value) });
        }

        if (OwnershipType == OwnershipType.LP
            && LimitedPartnershipName != null)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LMT_PTNR_NAM, _response = LimitedPartnershipName });
        }

        if (OwnershipType == OwnershipType.Partnership
            && MoreThanFive.HasValue)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PTNRSP_OVR_FIV_PTNR_FLG, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(MoreThanFive.Value) });
        }

        if (OwnershipType != OwnershipType.QSF
            && Owner != null)
        {
            responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.QSF_OWNR_LGL_NAM, _response = IEmployerRegistrationModelSection.ConcatenateLegalName(Owner.FirstName, Owner.MiddleInitial, Owner.LastName) });
        }
        if (OwnershipType == OwnershipType.CityGovernmentAgency)
        {
            if (OwnershipAgencies != null)
            {
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.GOV_EMP_DOC_UPLD, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(OwnershipAgencies.HasFile!) });
                responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.MISS_UPLD_GOV_EMP_DOC, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(OwnershipAgencies.NoHasFile!) });
            }
        }

        if (OwnershipType == OwnershipType.LLCCorporation)
        {
            // 3082-3084: Corporate Officer Services
            if (CorporateOfficerServices != null)
            {
                if (CorporateOfficerServices.OfficersPerformServices.HasValue)
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PRFM_SRVC, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(CorporateOfficerServices.OfficersPerformServices.Value) });
                }

                if (CorporateOfficerServices.OfficersPerformServices == true && CorporateOfficerServices.ApproximatePayDate.HasValue)
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_PAID_DT, _response = CorporateOfficerServices.ApproximatePayDate.Value.ToString("yyyy-MM-dd") });
                }

                if (CorporateOfficerServices.OfficersPerformServices == false && !string.IsNullOrEmpty(CorporateOfficerServices.NoPayExplanation))
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.CORP_OFC_NO_PAY_RSN, _response = CorporateOfficerServices.NoPayExplanation });
                }
            }

            // 3085-3091: LLC Documentation
            if (LlcDocumentation != null)
            {
                // 3085: Do you have the required documentation available to upload?
                if (LlcDocumentation.HasRequiredDocumentation.HasValue)
                {
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLD_REQ_DOC_AVL, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(LlcDocumentation.HasRequiredDocumentation.Value) });
                }

                if (LlcDocumentation.HasRequiredDocumentation == true)
                {
                    // 3086: I will supply required documentation at a later date.
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(LlcDocumentation.WillSupplyDocumentationLater) });

                    // 3090: Uploaded file
                    if (!string.IsNullOrEmpty(LlcDocumentation.FilePath))
                    {
                        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_UPLD, _response = LlcDocumentation.FilePath });
                    }
                }

                if (LlcDocumentation.HasRequiredDocumentation == false)
                {
                    // 3087: Reason documentation cannot be submitted
                    if (LlcDocumentation.NoDocumentationReason.HasValue)
                    {
                        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_DOC_RSN, _response = LlcDocumentation.NoDocumentationReason.Value.ToString() });
                    }

                    // 3088: When do you plan to submit your application to the IRS?
                    if (LlcDocumentation.PlannedSubmissionDate.HasValue)
                    {
                        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_PLAN_SBMT_DT, _response = LlcDocumentation.PlannedSubmissionDate.Value.ToString("yyyy-MM-dd") });
                    }

                    // 3089: What date was the application submitted to the IRS?
                    if (LlcDocumentation.ApplicationSubmittedDate.HasValue)
                    {
                        responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.LLC_ELEC_CORP_APP_SBMT_DT, _response = LlcDocumentation.ApplicationSubmittedDate.Value.ToString("yyyy-MM-dd") });
                    }

                    // 3091: I acknowledge that I will submit the required documentation
                    responses.Add(new SurveyResponse() { _surveyResponseItemSk = (int) SurveyResponseItem.PRTL_MISS_DOC_LLC_ELEC_CORP, _response = IEmployerRegistrationModelSection.ConvertBooleanResponseToString(LlcDocumentation.AcknowledgeSubmitDocumentation) });
                }
            }
        }

        return responses;
    }

    /// <inheritdoc/>
    public void LoadSurveyResponses(SurveyResponseItemProxy[] responses) { }

    /// <inheritdoc/>
    public List<Tuple<RegistrationAddressCode, AddressModel>> GetSurveyAddresses()
    {
        return new();
    }

    /// <inheritdoc/>
    public void LoadSurveyAddresses(RegistrationAddressProxy[] addresses) { }

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
