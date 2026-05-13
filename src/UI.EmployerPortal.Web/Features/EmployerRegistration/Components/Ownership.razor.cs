using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components.OwnershipForms;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
/// Ownership component for employer registration
/// </summary>
public partial class Ownership
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    [Inject] private EmployerRegistrationModelStore ModelStore { get; set; } = default!;

    /// <summary>
    /// 
    /// </summary>
    [CascadingParameter]
    public EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Model Injection
    /// </summary>
    [Parameter]
    public OwnershipSessionData Model { get; set; } = default!;

    private OwnershipType SelectedOwnershipType { get; set; } = OwnershipType.None;
    private List<string> ValidationErrors { get; set; } = new();
    private List<string> ValidationFieldIds { get; set; } = new();
    //private bool _shouldValidate = false;
    private bool _showValidationSummary = false;
    //private bool IsContinueEnabled = true;>// IsFormValid();
    private object? _childFormData;
    private bool HasPaidEmployeesInWI { get; set; }
    private bool ExpectsFuturePayroll { get; set; }

    // --- New section refs and data ---
    private CorporateOfficerServicesSection? _officerServicesSectionRef;
    private LlcDocumentationSection? _llcDocSectionRef;
    private QualifiedSettlementFundSection? _qsfSectionRef;
    private GovernmentEmpDocumentation? _owaSectionRef;
    private CorporateOfficerServicesModel _corporateOfficerServicesData = new();
    private LlcDocumentationModel _llcDocumentationData = new();
    private QualifiedSettlementFundModel _qsfData = new();
    private OwnershipAgency _owaData = new();
    private Func<List<ValidationItem>>? _officerServicesValidateCallback;
    private Func<List<ValidationItem>>? _llcDocValidateCallback;
    private Func<List<ValidationItem>>? _qsfValidateCallback;
    private Func<List<ValidationItem>>? _owagencyDocValidateCallback;
    private readonly HashSet<string> _sectionFieldsIds = [];
    private bool ShowCorporateOfficerServicesSection => Model.OwnershipType is (OwnershipType.Corporation or OwnershipType.LLCCorporation)
                                                        && !HasPaidEmployeesInWI
                                                        && !ExpectsFuturePayroll;

    private bool ShowLlcDocumentationSection => Model.OwnershipType == OwnershipType.LLCCorporation;

    private bool ShowQsfSection => Model.OwnershipType == OwnershipType.QSF
                                                && (!HasPaidEmployeesInWI || !ExpectsFuturePayroll);

    private bool ShowOWASection => Model.OwnershipType == OwnershipType.CityGovernmentAgency;
    private bool ShowOWACSection => Model.OwnershipType == OwnershipType.CountyGovernmentAgency;
    private bool ShowOWAFSection => Model.OwnershipType == OwnershipType.FederalGovernmentAgency;
    private bool ShowOWALSection => Model.OwnershipType == OwnershipType.LocalGovernmentUnitNotListed;
    private bool ShowOWASCSection => Model.OwnershipType == OwnershipType.SchoolDistrict;
    private bool ShowOWASTSection => Model.OwnershipType == OwnershipType.StateGovernmentAgency;
    private bool ShowOWASTUSection => Model.OwnershipType == OwnershipType.StateGovernmentUnitNotListed;
    private bool ShowOWATSection => Model.OwnershipType == OwnershipType.Township;
    private bool ShowOWAVSection => Model.OwnershipType == OwnershipType.Village;
    private bool _isValidating = false;
    private List<ValidationItem>? _capturedChildItems;
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        base.OnInitialized();
        HasPaidEmployeesInWI = ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.HavePaidEmployeesForWorkInWisconsin.HasValue
            && ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.HavePaidEmployeesForWorkInWisconsin.Value == true;

        ExpectsFuturePayroll = ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.ExpectFuturePayroll.HasValue
            && ModelStore.EmployerRegistrationModel.PreliminaryQuestionsModel.ExpectFuturePayroll.Value == true;

        EditContext = new EditContext(Model);
        RestoreSectionData();
    }

    /// <summary>
    /// Called by parent wizard for validation
    /// </summary>
    public async Task<bool> Validate()
    {
        _isValidating = true;
        _capturedChildItems = null;
        try
        {
            _showValidationSummary = true;
            ValidationErrors.Clear();
            ValidationFieldIds.Clear();
            EnsureOwnershipTypeValidationError();
            _validateCallback?.Invoke();
            if (_capturedChildItems is { Count: > 0 })
            {
                foreach (var item in _capturedChildItems.DistinctBy(i =>
                {
                    return i.Message;
                }))
                {
                    if (!ValidationErrors.Contains(item.Message))
                    {
                        ValidationErrors.Add(item.Message);
                        ValidationFieldIds.Add(item.FieldId);
                    }
                }
            }
            _sectionFieldsIds.Clear();
            AggregateSectionErrors(ShowLlcDocumentationSection, _llcDocValidateCallback);
            AggregateSectionErrors(ShowCorporateOfficerServicesSection, _officerServicesValidateCallback);
            AggregateSectionErrors(ShowQsfSection, _qsfValidateCallback);
            AggregateSectionErrors(ShowOWASection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWACSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWAFSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWALSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASCSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASTSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASTUSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWATSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWAVSection, _owagencyDocValidateCallback);

            _showValidationSummary = !IsFormValid();
            await InvokeAsync(StateHasChanged);
            return IsFormValid();
        }
        finally
        {
            _isValidating = false;
            _capturedChildItems = null;
        }
    }

    /// <summary>
    /// AggregateSectionErrors
    /// </summary>
    /// <param name="showSection"></param>
    /// <param name="validateCallback"></param>
    private void AggregateSectionErrors(bool showSection, Func<List<ValidationItem>>? validateCallback)
    {
        if (!showSection || validateCallback is null)
        {
            return;
        }

        foreach (var item in validateCallback())
        {
            if (!ValidationErrors.Contains(item.Message))
            {
                ValidationErrors.Add(item.Message);
                ValidationFieldIds.Add(item.FieldId);
                if (!string.IsNullOrEmpty(item.FieldId))
                {
                    _sectionFieldsIds.Add(item.FieldId);
                }
            }
        }
    }
    /// <summary>
    /// RefreshSectionValidationSummary
    /// </summary>
    public void RefreshSectionValidationSummary()
    {
        for (var i = ValidationFieldIds.Count - 1; i >= 0; i--)
        {
            if (!string.IsNullOrEmpty(ValidationFieldIds[i])
                    && _sectionFieldsIds.Contains(ValidationFieldIds[i]))
            {
                ValidationErrors.RemoveAt(i);
                ValidationFieldIds.RemoveAt(i);
            }
        }
        _sectionFieldsIds.Clear();
        AggregateSectionErrors(ShowLlcDocumentationSection, _llcDocValidateCallback);
        AggregateSectionErrors(ShowCorporateOfficerServicesSection, _officerServicesValidateCallback);
        AggregateSectionErrors(ShowQsfSection, _qsfValidateCallback);
        AggregateSectionErrors(ShowOWASection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWACSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWAFSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWALSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWASCSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWASTSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWASTUSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWATSection, _owagencyDocValidateCallback);
        AggregateSectionErrors(ShowOWAVSection, _owagencyDocValidateCallback);
        StateHasChanged();
    }

    // Component mapping dictionary
    private readonly Dictionary<OwnershipType, Type> _ownershipComponentMap = new()
    {
        { OwnershipType.Corporation, typeof(CorporationOwnershipForm) },
        { OwnershipType.LLCCorporation, typeof(MemberBasedOwnershipForm) },
        { OwnershipType.LLC, typeof(MemberBasedOwnershipForm) },
        { OwnershipType.LLP, typeof(MemberBasedOwnershipForm) },
        { OwnershipType.LP, typeof(LimitedPartnershipOwnershipForm) },
        { OwnershipType.Partnership, typeof(MemberBasedOwnershipForm) },
        { OwnershipType.SoleProprietorship, typeof(SoleProprietorshipOwnershipForm) },
        { OwnershipType.Individual, typeof(SoleProprietorshipOwnershipForm) },
        { OwnershipType.Estate, typeof(EstateOwnershipForm) },
        //{ OwnershipType.CityGovernmentAgency, typeof(GovernmentEmpDocumentation) },
        //{ OwnershipType.CountyGovernmentAgency, typeof(GovernmentEmpDocumentation) },
        //{ OwnershipType.StateGovernmentAgency, typeof(GovernmentEmpDocumentation) },
        //{ OwnershipType.FederalGovernmentAgency, typeof(GovernmentEmpDocumentation) }
    };

    // Configuration mapping using enum
    private readonly Dictionary<OwnershipType, OwnershipFormConfig> _configurationMap = new()
    {
        {
            OwnershipType.Corporation,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "corporation",
                TypeDisplayName = "Corporation",
                StateLabel = "Incorporation State",
                RequiresForeignCountry = true,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the principal officers.",
                PredefinedRoles = ["President", "Vice President", "Secretary", "Treasurer"]
            }
        },
        {
            OwnershipType.LLCCorporation,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "llc-corporation",
                TypeDisplayName = "LLC Electing to be Treated as a Corporation",
                MemberLabel = "Member",
                StateLabel = "Registration State",
                RequiresState = true,
                MaxEntries = 5,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the members. If there are more than five members, list the five with the highest ownership percentages."
            }
        },
        {
            OwnershipType.LLC,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "llc",
                TypeDisplayName = "Limited Liability Company (LLC)",
                MemberLabel = "Member",
                StateLabel = "Registration State",
                RequiresState = true,
                MaxEntries = 5,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the members. If there are more than five members, list the five with the highest ownership percentages."
            }
        },
        {
            OwnershipType.LLP,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "llp",
                TypeDisplayName = "Limited Liability Partnership (LLP)",
                MemberLabel = "Partner",
                StateLabel = "Registration State",
                RequiresState = true,
                MaxEntries = 5,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the partners. If there are more than five partners, list the five with the highest ownership percentages."
            }
        },
        {
            OwnershipType.LP,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "lp",
                TypeDisplayName = "Limited Partnership (LP)",
                MemberLabel = "Partner",
                StateLabel = "Registration State",
                RequiresState = true,
                MaxEntries = 5,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the partners. If there are more than five partners, list the five with the highest ownership percentages."
            }
        },
        {
            OwnershipType.Partnership,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "partnership",
                TypeDisplayName = "Partnership (not LLC or Corporation)",
                MemberLabel = "Partner",
                StateLabel = "Registration State",
                RequiresState = false,
                MaxEntries = 5,
                InstructionText = "Enter the names, Social Security Numbers, and ownership percentages of the partners. If there are more than five partners, list the five with the highest ownership percentages."
            }
        },
        {
            OwnershipType.SoleProprietorship,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "sole-proprietorship",
                TypeDisplayName = "Sole Proprietorship (not LLC or Corporation)",
                MaxEntries = 1
            }
        },
        {
            OwnershipType.Individual,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "individual",
                TypeDisplayName = "Individual",
                MaxEntries = 1
            }
        },
        {
            OwnershipType.Estate,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "estate",
                TypeDisplayName = "Estate",
                MaxEntries = 2
            }
        }, {
            OwnershipType.CityGovernmentAgency,
            new OwnershipFormConfig
            {
                OwnershipTypeValue = "citygovagency",
                TypeDisplayName = "CityGovernmentAgency",
                MaxEntries = 1
            }
        },
    };

    private void EnsureOwnershipTypeValidationError()
    {
        var requiredMessage = "Ownership Type is required.";
        if (Model.OwnershipType == OwnershipType.None)
        {
            if (!ValidationErrors.Contains(requiredMessage))
            {
                ValidationErrors.Insert(0, requiredMessage);
                ValidationFieldIds.Insert(0, "ownership-type");
            }
        }
        else
        {
            var idx = ValidationErrors.IndexOf(requiredMessage);
            if (idx >= 0)
            {
                ValidationErrors.RemoveAt(idx);
                if (idx < ValidationFieldIds.Count)
                {
                    ValidationFieldIds.RemoveAt(idx);
                }
            }
        }
    }

    private async Task OnOwnershipTypeChanged()
    {
        try
        {
            ValidationErrors.Clear();
            ValidationFieldIds.Clear();
            _showValidationSummary = false;
            _childFormData = null;
            _validateCallback = null;
            _officerServicesValidateCallback = null;
            _llcDocValidateCallback = null;
            _qsfValidateCallback = null;
            _owagencyDocValidateCallback = null;
            // Reset section data when ownership type changes
            _corporateOfficerServicesData = new();
            _llcDocumentationData = new();
            _qsfData = new();
            _owaData = new();
            Model.CorporateOfficerServices = null;
            Model.LlcDocumentation = null;
            Model.QualifiedSettlementFund = null;



            StateHasChanged();
            await Task.Delay(100);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in OnOwnershipTypeChanged: {ex.Message}");
        }
    }

    private OwnershipFormConfig? GetCurrentConfig()
    {
        return Model.OwnershipType == OwnershipType.None || !_configurationMap.ContainsKey(Model.OwnershipType)
            ? null
            : _configurationMap[Model.OwnershipType];
    }

    private Dictionary<string, object> GetComponentParameters()
    {
        var config = GetCurrentConfig();
        var parameters = new Dictionary<string, object>();

        if (config != null)
        {
            parameters.Add("Config", config);
            parameters.Add("OnValidationChanged", EventCallback.Factory.Create<List<ValidationItem>>(this, OnChildValidationChanged));
            parameters.Add("RegisterValidate", (Action<Func<List<string>>>) RegisterValidateCallback);
            parameters.Add("ContinueData", Model);
            // Create correctly-typed EventCallback to match each child component's parameter type
            switch (Model.OwnershipType)
            {
                case OwnershipType.LLC:
                case OwnershipType.LLP:
                case OwnershipType.LLCCorporation:
                case OwnershipType.Partnership:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<MemberBasedFormData>(this, OnFormDataChanged));
                    break;

                case OwnershipType.LP:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<LimitedPartnershipFormData>(this, OnFormDataChanged));
                    break;

                case OwnershipType.Corporation:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<CorporationFormData>(this, OnFormDataChanged));
                    break;

                case OwnershipType.SoleProprietorship:
                case OwnershipType.Individual:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<OwnerMember>(this, OnFormDataChanged));
                    break;

                case OwnershipType.Estate:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<EstateFormData>(this, OnFormDataChanged));
                    break;
                case OwnershipType.CityGovernmentAgency:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<OwnershipAgency>(this, OnFormDataChanged));
                    break;
                case OwnershipType.CountyGovernmentAgency:
                    parameters.Add("OnDataChanged",
                        EventCallback.Factory.Create<OwnershipAgency>(this, OnFormDataChanged));
                    break;
            }
        }

        return parameters;
    }

    private void OnChildValidationChanged(List<ValidationItem> items)
    {
        if (_isValidating)
        {
            _capturedChildItems = items;
            return;
        }
        var distinct = items.DistinctBy(i =>
        {
            return i.Message;
        }).ToList();
        ValidationErrors = distinct.Select(i =>
        {
            return i.Message;
        }).ToList();
        ValidationFieldIds = distinct.Select(i =>
        {
            return i.FieldId;
        }).ToList();
        if (_showValidationSummary)
        {
            _sectionFieldsIds.Clear();
            AggregateSectionErrors(ShowLlcDocumentationSection, _llcDocValidateCallback);
            AggregateSectionErrors(ShowCorporateOfficerServicesSection, _officerServicesValidateCallback);
            AggregateSectionErrors(ShowQsfSection, _qsfValidateCallback);
            AggregateSectionErrors(ShowOWASection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWACSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWAFSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWALSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASCSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASTSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWASTUSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWATSection, _owagencyDocValidateCallback);
            AggregateSectionErrors(ShowOWAVSection, _owagencyDocValidateCallback);
        }
        StateHasChanged();
    }

    private void OnFormDataChanged(object formData)
    {
        _childFormData = formData;
        MoveChildDataToModel();
    }

    private void MoveChildDataToModel()
    {
        // Save form-specific data based on ownership type
        if (_childFormData != null)
        {
            switch (Model.OwnershipType)
            {
                case OwnershipType.LLC:
                case OwnershipType.LLP:
                case OwnershipType.LLCCorporation:
                case OwnershipType.Partnership:
                    if (_childFormData is MemberBasedFormData memberData)
                    {
                        Model.RegistrationState = memberData.RegistrationState;
                        Model.Members = memberData.Members;
                        Model.MoreThanFive = memberData.MoreThanFive;
                    }
                    break;

                case OwnershipType.LP:
                    if (_childFormData is LimitedPartnershipFormData lpData)
                    {
                        Model.RegistrationState = lpData.RegistrationState;
                        Model.LimitedPartnershipName = lpData.LimitedPartnershipName;
                        Model.GeneralPartner = lpData.GeneralPartner;
                    }
                    break;

                case OwnershipType.Corporation:
                    if (_childFormData is CorporationFormData corpData)
                    {
                        Model.IncorporationState = corpData.IncorporationState;
                        Model.ForeignCountry = corpData.ForeignCountry;
                        Model.Officers = corpData.Officers;
                    }
                    break;

                case OwnershipType.SoleProprietorship:
                case OwnershipType.Individual:
                    if (_childFormData is OwnerMember owner)
                    {
                        Model.Owner = owner;
                    }
                    break;

                case OwnershipType.Estate:
                    if (_childFormData is EstateFormData estateData)
                    {
                        Model.Decedent = estateData.Decedent;
                        Model.PersonalRepresentative = estateData.PersonalRepresentative;
                    }
                    break;
                case OwnershipType.CityGovernmentAgency:
                    if (_childFormData is OwnershipAgency ownershipAgency)
                    {
                        Model.OwnershipAgencies?.HasFile = ownershipAgency.HasFile;
                        Model.OwnershipAgencies?.NoHasFile = ownershipAgency.NoHasFile;
                    }
                    break;
                case OwnershipType.CountyGovernmentAgency:
                    if (_childFormData is OwnershipAgency cownershipAgency)
                    {
                        Model.OwnershipAgencies?.HasFile = cownershipAgency.HasFile;
                        Model.OwnershipAgencies?.NoHasFile = cownershipAgency.NoHasFile;
                    }
                    break;

            }
        }
    }

    private void OnCorporateOfficerServicesChanged(CorporateOfficerServicesModel data)
    {
        _corporateOfficerServicesData = data;
        Model.CorporateOfficerServices = data;
        if (_showValidationSummary)
        {
            RefreshSectionValidationSummary();
        }
    }

    private void OnLlcDocumentationChanged(LlcDocumentationModel data)
    {
        _llcDocumentationData = data;
        Model.LlcDocumentation = data;
        if (_showValidationSummary)
        {
            RefreshSectionValidationSummary();
        }
    }

    private void OnQsfDataChanged(QualifiedSettlementFundModel data)
    {
        _qsfData = data;
        Model.QualifiedSettlementFund = data;
        if (_showValidationSummary)
        {
            RefreshSectionValidationSummary();
        }
    }

    private void OnOwaDataChanged(OwnershipAgency data)
    {
        _owaData = data;
        Model.OwnershipAgencies = data;

        if (_showValidationSummary)
        {
            RefreshSectionValidationSummary();
        }
    }

    private void RegisterOfficerServicesValidateCallback(Func<List<ValidationItem>> callback)
    {
        _officerServicesValidateCallback = callback;
    }

    private void RegisterLlcDocValidateCallback(Func<List<ValidationItem>> callback)
    {
        _llcDocValidateCallback = callback;
    }

    private void RegisterQsfValidateCallback(Func<List<ValidationItem>> callback)
    {
        _qsfValidateCallback = callback;
    }
    private void RegisterOWAValidateCallback(Func<List<ValidationItem>> callback)
    {
        _owagencyDocValidateCallback = callback;
    }
    /// <summary>
    /// Restores section data from the model (for back-navigation/session continuity).
    /// </summary>
    private void RestoreSectionData()
    {
        if (Model.CorporateOfficerServices != null)
        {
            _corporateOfficerServicesData = Model.CorporateOfficerServices;
        }

        if (Model.LlcDocumentation != null)
        {
            _llcDocumentationData = Model.LlcDocumentation;
        }

        if (Model.QualifiedSettlementFund != null)
        {
            _qsfData = Model.QualifiedSettlementFund;
        }
        if (Model.OwnershipAgencies != null)
        {
            _owaData = Model.OwnershipAgencies;
        }

    }

    private bool IsFormValid()
    {
        // Basic validation - ownership type must be selected
        if (Model.OwnershipType == OwnershipType.None)
        {
            return false;
        }

        // Check if there are any validation errors from child component
        return !ValidationErrors.Any();
    }

    private Func<List<string>>? _validateCallback;
    ///// <summary>
    ///// RegisterValidateCallback
    ///// </summary>
    ///// <param name="callback"></param>
    private void RegisterValidateCallback(Func<List<string>> callback)
    {
        _validateCallback = callback;
    }
}
