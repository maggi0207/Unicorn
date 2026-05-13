using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
/// 
/// </summary>
public partial class PreliminaryQuestions
{
    private bool _formSubmitted = false;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    private string? _leasingStartDateRaw;
    private string? _lastEmploymentDateRaw;
    private string? _lastPayrollDateRaw;

    // Tracks whether a file has been uploaded in each 501(c)(3) upload section.
    // If false the user must check WillSupplyDocumentationLater to proceed.
    private bool _rulingDocUploaded = false;
    private bool _appliedDocUploaded = false;
    private bool _notAppliedDocUploaded = false;

    private List<string> ValidationErrors { get; set; } = new();
    private List<string> ValidationFieldIds { get; set; } = new();

    private bool IsVisible<T>(Expression<Func<T>> fieldExpression)
    {
        var field = FieldIdentifier.Create(fieldExpression);
        return _formSubmitted || _touchedFields.Contains(field);
    }

    /// <summary>
    /// 
    /// </summary>
    public enum FileType
    {
        /// <summary>
        /// 
        /// </summary>
        Ruling,
        /// <summary>
        /// /
        /// </summary>
        IRS,
        /// <summary>
        /// 
        /// </summary>
        INC,
        /// <summary>
        /// 
        /// </summary>
        GOV
    }
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public string? Value { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    /// <summary>
    /// 
    /// </summary>
    [Parameter] public PreliminaryQuestionsModel Model { get; set; } = new();

    // option lists
    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<NoEmployeeReason?>> NoEmployeReasonOptions = new[]
    {
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.BusinessActivityEnded, Label = "Business activity has ended but business has not been sold" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.NotOperatingInWisconsin, Label = "No longer operating in Wisconsin but still operating in another state" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.HaveSoldOrTransferredBusiness, Label = "Business activity sold or transferred" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.BusiessWithoutEmployees, Label = "Business continuing without employees" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.EmployingIndependentContractors, Label = "Employing Independent Contractors" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.Death, Label = "Death" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.LeasingFromPEO, Label = "Leasing employees from Professional Employer Organization (PEO)" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.FiscalAgent, Label = "Fiscal Agent electing to be employer" },
      new RadioOption<NoEmployeeReason?> { Value = NoEmployeeReason.Other, Label = "Other" }
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly IReadOnlyList<RadioOption<bool?>> YesNoRadioOptions = new[]
    {
      new RadioOption<bool?> {Value =true, Label = "Yes"},
      new RadioOption<bool?> {Value =false, Label = "No"}
    };

    /// <summary>
    /// 
    /// </summary>
    public static readonly List<SelectOption> FuturePayPeriodOptions = new()
    {
        new SelectOption {Value = "1", Text = "Within 30 days"},
        new SelectOption {Value = "2", Text = "30 to 90 days"},
        new SelectOption {Value = "3", Text = "6 months"},
        new SelectOption {Value = "4", Text = "One year"},
        new SelectOption {Value = "5", Text = "More than a year"},
    };

    // 501(c)(3) visibility — driven by the Yes/No non-profit question
    private bool Show501c3SubTree => Model.IsNonProfitOrg == true;
    private bool ShowRulingUpload => Show501c3SubTree && Model.HasRulingFrom501c3IRS == true;
    private bool ShowHasAppliedQuestion => Show501c3SubTree && Model.HasRulingFrom501c3IRS == false;
    private bool ShowAppliedUpload => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == true;
    private bool ShowNotAppliedText => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == false;


    // visibility delegates
    private bool VisibilityQuestion_2_1 => Model.AcquiredExistingBusiness.HasValue && Model.AcquiredExistingBusiness.Value;
    private bool VisibilityQuestion_2_1_1 => VisibilityQuestion_2_1 && Model.KnowAcquiredBusinessAccountNumber.HasValue && Model.KnowAcquiredBusinessAccountNumber.Value;
    private bool VisibilityQuestion_2_1_2 => VisibilityQuestion_2_1 && Model.KnowAcquiredBusinessAccountNumber.HasValue && !Model.KnowAcquiredBusinessAccountNumber.Value;
    private bool VisibilityQuestion_3 => Model.AcquiredExistingBusiness.HasValue && !Model.AcquiredExistingBusiness.Value;
    private bool VisibilityQuestion_3_1 => VisibilityQuestion_3 && Model.HavePaidEmployeesForWorkInWisconsin.HasValue && Model.HavePaidEmployeesForWorkInWisconsin.Value;
    private bool VisibilityQuestion_3_1_2 => VisibilityQuestion_3_1 && Model.HaveEmployeesCurrentlyWorkingInWisconsin.HasValue && !Model.HaveEmployeesCurrentlyWorkingInWisconsin.Value;
    private bool VisibilityQuestion_3_2 => VisibilityQuestion_3 && Model.HavePaidEmployeesForWorkInWisconsin.HasValue && !Model.HavePaidEmployeesForWorkInWisconsin.Value;
    private bool VisibilityQuestion_3_2_1 => VisibilityQuestion_3_2 && Model.ExpectFuturePayroll.HasValue && Model.ExpectFuturePayroll.Value;
    //private bool VisibilityQuestion_4 => Model.ExpectFuturePayroll.HasValue && !Model.ExpectFuturePayroll.Value;
    private bool VisibilityCheckbox_InfoAccurate => VisibilityQuestion_3_1 && Model.HaveEmployeesCurrentlyWorkingInWisconsin == true;
    private bool VisibilityQuestion_4 => VisibilityQuestion_3_1 && Model.HaveEmployeesCurrentlyWorkingInWisconsin == false;

    private string? LeasingStartDateAsString
    {
        get => Model.LeasingStartDate?.ToString("yyyy-MM-dd");
        set
        {
            _leasingStartDateRaw = value;
            Model.LeasingStartDate = !string.IsNullOrWhiteSpace(value) && DateOnly.TryParseExact(value, "yyyy-MM-dd", out var d) ? d : null;
        }
    }

    // data type fascades
    private string? LastEmploymentDateAsString
    {
        get => Model.LastEmploymentDate?.ToString("yyyy-MM-dd");
        set
        {
            _lastEmploymentDateRaw = value;
            Model.LastEmploymentDate = !string.IsNullOrWhiteSpace(value) && DateOnly.TryParseExact(value, "yyyy-MM-dd", out var d) ? d : null;
        }
    }

    private string? LastPayrollDateAsString
    {
        get => Model.LastPayrollDate?.ToString("yyyy-MM-dd");
        set
        {
            _lastPayrollDateRaw = value;
            Model.LastPayrollDate = !string.IsNullOrWhiteSpace(value) && DateOnly.TryParseExact(value, "yyyy-MM-dd", out var d) ? d : null;
        }
    }

    private string? ExpectedFuturePayrollPeriodAsString
    {
        get => Model.ExpectedFuturePayrollPeriod.HasValue ? ((int)Model.ExpectedFuturePayrollPeriod.Value).ToString() : null;
        set
        {
            if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(FuturePayPeriod), intValue))
            {
                Model.ExpectedFuturePayrollPeriod = (FuturePayPeriod)intValue;
            }
            else
            {
                Model.ExpectedFuturePayrollPeriod = null;
            }
        }
    }

    // change handlers
    private void OnIsNonProfitOrgChanged(bool? value)
    {
        Model.IsNonProfitOrg = value;
        if (value != true)
        {
            Model.HasRulingFrom501c3IRS = null;
            Model.HasAppliedFor501c3WithIRS = null;
            Model.WillSupplyDocumentationLater = false;

            ResetField(() => Model.HasRulingFrom501c3IRS);
            ResetField(() => Model.HasAppliedFor501c3WithIRS);
        }
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.IsNonProfitOrg)));
    }

    private void OnAcquiredExistingBusinessChanged(bool? value)
    {
        Model.AcquiredExistingBusiness = value;

        //Clear
        Model.KnowAcquiredBusinessAccountNumber = null;
        Model.AcquiredBusinessAccountNumber = string.Empty;
        Model.AcquiredBusinessName = string.Empty;
        Model.AcquiredBusinessAddress = null;

        Model.HavePaidEmployeesForWorkInWisconsin = null;
        Model.HaveEmployeesCurrentlyWorkingInWisconsin = null;
        Model.InformationIsAccurate = false;
        Model.ExpectFuturePayroll = null;
        Model.SelectedNoEmployeeReason = null;
        Model.NoEmployeeExplanation = null;
        Model.PEOName = null;
        Model.PEOUIAccountNumber = null;
        Model.PEOFEIN = null;
        Model.LeasingStartDate = null;
        Model.FiscalAgentName = null;
        Model.FiscalAgentUIAccountNumber = null;
        Model.OtherReason = null;
        Model.LastEmploymentDate = null;
        Model.LastPayrollDate = null;

        ResetField(() => Model.KnowAcquiredBusinessAccountNumber);
        ResetField(() => Model.AcquiredBusinessAccountNumber);
        ResetField(() => Model.AcquiredBusinessName);
        ResetField(() => Model.AcquiredBusinessAddress);
        ResetField(() => Model.HavePaidEmployeesForWorkInWisconsin);
        ResetField(() => Model.HaveEmployeesCurrentlyWorkingInWisconsin);
        ResetField(() => Model.ExpectFuturePayroll);
        ResetField(() => Model.ExpectedFuturePayrollPeriod);
        ResetField(() => Model.SelectedNoEmployeeReason);

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.AcquiredExistingBusiness)));
    }

    private void OnKnowAcquiredBusinessAccountNumberChanged(bool? value)
    {
        Model.KnowAcquiredBusinessAccountNumber = value;

        ResetField(() => Model.AcquiredBusinessAccountNumber);
        ResetField(() => Model.AcquiredBusinessName);
        ResetField(() => Model.AcquiredBusinessAddress);

        // Note: When the AcquiredBusinessAddress model is null, it isn't validated
        Model.AcquiredBusinessAddress = !(value ?? true) ? new() : null;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.KnowAcquiredBusinessAccountNumber)));
    }

    private void OnHavePaidEmployeesForWorkInWisconsinChanged(bool? value)
    {
        Model.HavePaidEmployeesForWorkInWisconsin = value;

        //Clear Q3.1 (shown when Yes)
        Model.HaveEmployeesCurrentlyWorkingInWisconsin = null;
        Model.InformationIsAccurate = false;

        //Clear Q3.2 (shown when No)
        Model.SelectedNoEmployeeReason = null;
        Model.NoEmployeeExplanation = null;
        Model.PEOName = null;
        Model.PEOUIAccountNumber = null;
        Model.PEOFEIN = null;
        Model.LeasingStartDate = null;
        Model.FiscalAgentName = null;
        Model.FiscalAgentUIAccountNumber = null;
        Model.OtherReason = null;
        Model.LastEmploymentDate = null;
        Model.LastPayrollDate = null;

        ResetField(() => Model.HaveEmployeesCurrentlyWorkingInWisconsin);
        ResetField(() => Model.ExpectFuturePayroll);
        ResetField(() => Model.ExpectedFuturePayrollPeriod);
        ResetField(() => Model.SelectedNoEmployeeReason);

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin)));
    }

    private void OnHaveEmployeesCurrentlyWorkingInWisconsinChanged(bool? value)
    {
        Model.HaveEmployeesCurrentlyWorkingInWisconsin = value;
        Model.InformationIsAccurate = false;

        //Clear Q4 branch (shown when No)
        Model.SelectedNoEmployeeReason = null;
        Model.NoEmployeeExplanation = null;
        Model.PEOName = null;
        Model.PEOUIAccountNumber = null;
        Model.PEOFEIN = null;
        Model.LeasingStartDate = null;
        Model.FiscalAgentName = null;
        Model.FiscalAgentUIAccountNumber = null;
        Model.OtherReason = null;
        Model.LastEmploymentDate = null;
        Model.LastPayrollDate = null;

        ResetField(() => Model.SelectedNoEmployeeReason);

        _messageStore.Clear(_editContext.Field(nameof(Model.InformationIsAccurate)));
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin)));
    }

    private void OnExpectFuturePayrollChanged(bool? value)
    {
        Model.ExpectFuturePayroll = value;

        //Clear
        Model.ExpectedFuturePayrollPeriod = null;
        ResetField(() => Model.ExpectedFuturePayrollPeriod);

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.ExpectFuturePayroll)));
    }
    private void OnInformationIsAccurateChanged()
    {
        _messageStore.Clear(_editContext.Field(nameof(Model.InformationIsAccurate)));
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.InformationIsAccurate)));
    }
    //private void OnHaveSoldOrTransferredBusinessChanged(bool? value)
    //{
    //    Model.HaveSoldOrTransferredBusiness = value;
    //    _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HaveSoldOrTransferredBusiness)));
    //}

    private void OnNoEmployeReasonChanged(NoEmployeeReason? value)
    {
        Model.SelectedNoEmployeeReason = value;
        Model.NoEmployeeExplanation = null;
        Model.PEOName = null;
        Model.PEOUIAccountNumber = null;
        Model.PEOFEIN = null;
        Model.LeasingStartDate = null;
        Model.FiscalAgentName = null;
        Model.FiscalAgentUIAccountNumber = null;
        Model.OtherReason = null;
        Model.LastPayrollDate = null;
        Model.LastEmploymentDate = null;
        _leasingStartDateRaw = null;
        _lastEmploymentDateRaw = null;
        _lastPayrollDateRaw = null;

        ResetField(() => Model.NoEmployeeExplanation);
        ResetField(() => Model.PEOName);
        ResetField(() => Model.PEOUIAccountNumber);
        ResetField(() => Model.PEOFEIN);
        ResetField(() => Model.LeasingStartDate);
        ResetField(() => Model.FiscalAgentName);
        ResetField(() => Model.FiscalAgentUIAccountNumber);
        ResetField(() => Model.OtherReason);
        ResetField(() => Model.LastEmploymentDate);
        ResetField(() => Model.LastPayrollDate);

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.SelectedNoEmployeeReason)));

        RunValidation();
    }

    //private async Task OnInput(string? value)
    //{
    //    var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
    //    if (digits.Length > 9)
    //    {
    //        digits = digits[..9];
    //    }

    //    _peoValue = digits.Length switch
    //    {
    //        > 2 => $"{digits[..2]}-{digits[2..]}",
    //        _ => digits
    //    };
    //    Model.PEOFEIN = _peoValue;
    //    _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.PEOFEIN)));
    //}
    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
        _messageStore = new ValidationMessageStore(_editContext);

        //Track validation state as user interacts
        _editContext.OnFieldChanged += (_, f) =>
        {
            _touchedFields.Add(f.FieldIdentifier);
            RunValidation(f.FieldIdentifier);

            _editContext.NotifyValidationStateChanged();
        };
        _editContext.OnValidationRequested += (_, __) =>
        {
            RunValidation();
        };
    }

    private void RunValidation(FieldIdentifier? changedField = null)
    {
        if (changedField.HasValue)
        {
            // Clear only that field’s errors
            _messageStore.Clear(changedField.Value);
        }
        else
        {
            // Full validation (on submit)
            _messageStore.Clear();
        }

        ValidateModel();
        _editContext.NotifyValidationStateChanged();
    }

    private void ResetField<T>(Expression<Func<T>> fieldExpression)
    {
        var field = FieldIdentifier.Create(fieldExpression);
        _messageStore.Clear(field);
        _touchedFields.Remove(field);
    }

    private void OnHasRulingFrom501c3IRSChanged(bool? value)
    {
        Model.HasRulingFrom501c3IRS = value;
        Model.HasAppliedFor501c3WithIRS = null;
        Model.WillSupplyDocumentationLater = false;
        _rulingDocUploaded = false;
        _appliedDocUploaded = false;
        _notAppliedDocUploaded = false;
        ResetField(() => Model.HasAppliedFor501c3WithIRS);
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HasRulingFrom501c3IRS)));
    }

    private void OnHasAppliedFor501c3WithIRSChanged(bool? value)
    {
        Model.HasAppliedFor501c3WithIRS = value;
        Model.WillSupplyDocumentationLater = false;
        _appliedDocUploaded = false;
        _notAppliedDocUploaded = false;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HasAppliedFor501c3WithIRS)));
    }

    private void OnWillSupplyDocumentationLaterChanged()
    {
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.WillSupplyDocumentationLater)));
    }

    /// <summary>
    /// /// Called by the wizard's Save and Quit handler to ensure FEIN is provided
    /// before creating a registration record without one.
    /// Only validates FEIN — other Step 1 fields are not checked.
    /// </summary>

    public bool ValidateFEINOnly()
    {
        _formSubmitted = true;
        ValidationErrors.Clear();
        ValidationFieldIds.Clear();
        _messageStore.Clear(_editContext.Field(nameof(Model.FEIN)));

        var feinResult = FEINField.ValidateFEIN(Model.FEIN);

        if (!feinResult.IsValid)
        {
            _messageStore.Add(_editContext.Field(nameof(Model.FEIN)), feinResult.ErrorMessage);
            ValidationErrors.Add(feinResult.ErrorMessage);
            ValidationFieldIds.Add("FEIN");
        }

        _editContext.NotifyValidationStateChanged();
        StateHasChanged();

        return feinResult.IsValid;
    }

    ///<Summary>
    /// Called by the parent wizard's HandleActionClick to validate be fore advancing
    ///</Summary>
    public bool Validate()
    {
        _formSubmitted = true;
        _messageStore.Clear();
        RunValidation();
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();

        return !_editContext.GetValidationMessages().Any();
    }

    private readonly string _uiAccountNumberRegex = @"^\d{6}-\d{3}-\d$";

    // conditional validation
    private void ValidateModel()
    {
        _messageStore.Clear();
        ValidationErrors.Clear();
        ValidationFieldIds.Clear();

        var addressErrors = new Dictionary<FieldIdentifier, List<string>>();

        // FEIN — all rules delegated to FEINField.ValidateFEIN()
        if (IsVisible(() => Model.FEIN))
        {
            var feinResult = FEINField.ValidateFEIN(Model.FEIN);
            if (!feinResult.IsValid)
            {
                _messageStore.Add(_editContext.Field(nameof(Model.FEIN)), feinResult.ErrorMessage);
            }
        }
        // UI Account Number
        if (IsVisible(() => Model.UIAccountNumber) && !string.IsNullOrWhiteSpace(Model.UIAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.UIAccountNumber));
            if (!Regex.IsMatch(Model.UIAccountNumber, _uiAccountNumberRegex))
            {
                _messageStore.Add(field, "Account Number is not a valid format (999999-999-9)");
            }
        }
        // Non-profit question
        if (IsVisible(() => Model.IsNonProfitOrg))
        {
            var field = _editContext.Field(nameof(Model.IsNonProfitOrg));
            if (!Model.IsNonProfitOrg.HasValue)
            {
                _messageStore.Add(field, "Answer if you are a non-profit organization as described in s.501(c)(3) of the IRS code");
            }
        }
        // 501(c)(3) sub-tree validation
        if (Show501c3SubTree && IsVisible(() => Model.HasRulingFrom501c3IRS))
        {
            var field = _editContext.Field(nameof(Model.HasRulingFrom501c3IRS));
            if (!Model.HasRulingFrom501c3IRS.HasValue)
            {
                _messageStore.Add(field, "Answer if you have a 501(c)(3) ruling from the IRS.");
            }
        }
        if (ShowHasAppliedQuestion && IsVisible(() => Model.HasAppliedFor501c3WithIRS))
        {
            var field = _editContext.Field(nameof(Model.HasAppliedFor501c3WithIRS));
            if (!Model.HasAppliedFor501c3WithIRS.HasValue)
            {
                _messageStore.Add(field, "Answer if you have applied for 501(c)(3) status with the IRS.");
            }
        }
        // Require a file upload OR the "supply later" checkbox for each visible upload section
        var supplyLaterField = _editContext.Field(nameof(Model.WillSupplyDocumentationLater));
        const string UploadOrCheckboxMsg = "Please upload the required documentation or select 'I will supply required documentation at a later date'";
        if (ShowRulingUpload && !_rulingDocUploaded && !Model.WillSupplyDocumentationLater && string.IsNullOrEmpty(Model.RulingDocFilename))
        {
            _messageStore.Add(supplyLaterField, UploadOrCheckboxMsg);
        }
        if (ShowAppliedUpload && !_appliedDocUploaded && !Model.WillSupplyDocumentationLater && string.IsNullOrEmpty(Model.IRSAcceptanceLetterFilename))
        {
            _messageStore.Add(supplyLaterField, UploadOrCheckboxMsg);
        }
        if (ShowNotAppliedText && !_notAppliedDocUploaded && !Model.WillSupplyDocumentationLater && string.IsNullOrEmpty(Model.ArticlesOfIncorporationFilename))
        {
            _messageStore.Add(supplyLaterField, UploadOrCheckboxMsg);
        }
        // Acquired Existing Business
        if (IsVisible(() => Model.AcquiredExistingBusiness))
        {
            var field = _editContext.Field(nameof(Model.AcquiredExistingBusiness));
            if (!Model.AcquiredExistingBusiness.HasValue)
            {
                _messageStore.Add(field, "Select if you acquired an existing business");
            }
        }
        // Question 2.1
        if (VisibilityQuestion_2_1 && IsVisible(() => Model.KnowAcquiredBusinessAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.KnowAcquiredBusinessAccountNumber));
            if (!Model.KnowAcquiredBusinessAccountNumber.HasValue)
            {
                _messageStore.Add(field, "Answer if you know the Account Number of the acquired business");
            }
        }
        // Question 2.1.1
        if (VisibilityQuestion_2_1_1 && IsVisible(() => Model.AcquiredBusinessAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.AcquiredBusinessAccountNumber));
            if (string.IsNullOrWhiteSpace(Model.AcquiredBusinessAccountNumber))
            {
                _messageStore.Add(field, "Enter the Account Number of the acquired business");
            }
            else if (VisibilityQuestion_2_1_1
         && !Regex.IsMatch(Model.AcquiredBusinessAccountNumber, _uiAccountNumberRegex))
            {
                _messageStore.Add(
                    _editContext.Field(nameof(Model.AcquiredBusinessAccountNumber)),
                    "Account Number is not a valid format (999999-999-9)");
            }
        }
        // Question 2.1.2
        if (VisibilityQuestion_2_1_2)
        {
            if (IsVisible(() => Model.AcquiredBusinessName))
            {
                var field = _editContext.Field(nameof(Model.AcquiredBusinessName));
                if (string.IsNullOrWhiteSpace(Model.AcquiredBusinessName))
                {
                    _messageStore.Add(field, "Enter the Name of the acquired business");
                }
            }
            if (Model.AcquiredBusinessAddress is not null)
            {
                addressErrors = ValidateAddressAnnotations(Model.AcquiredBusinessAddress);
            }
        }
        // Question 3
        if (VisibilityQuestion_3
            && IsVisible(() => Model.HavePaidEmployeesForWorkInWisconsin))
        {
            var field = _editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin));
            if (!Model.HavePaidEmployeesForWorkInWisconsin.HasValue)
            {
                _messageStore.Add(field, "Answer if you have paid employees for work completed in Wisconsin");
            }
        }
        // Question 3.1
        if (VisibilityQuestion_3_1
            && IsVisible(() => Model.HaveEmployeesCurrentlyWorkingInWisconsin))
        {
            var field = _editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin));
            if (!Model.HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
            {
                _messageStore.Add(field, "Answer if you currently have employees working in the State of Wisconsin");
            }
        }
        // Acknowledgement checkbox (Q3.1 Yes)
        if (VisibilityCheckbox_InfoAccurate && !Model.InformationIsAccurate)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.InformationIsAccurate)),
                "You must confirm that the information provided is true and accurate");
        }

        // Question 3.1.2
        //if (VisibilityQuestion_3_1_2)
        //{
        //    if (IsVisible(() => Model.LastEmploymentDate))
        //    {
        //        var field = _editContext.Field(nameof(Model.LastEmploymentDate));
        //        if (!Model.LastEmploymentDate.HasValue)
        //        {
        //            _messageStore.Add(field, "Enter the last employment date.");
        //        }
        //    }
        //    if (IsVisible(() => Model.LastPayrollDate))
        //    {
        //        var field = _editContext.Field(nameof(Model.LastPayrollDate));
        //        if (!Model.LastPayrollDate.HasValue)
        //        {
        //            _messageStore.Add(field, "Enter the last payroll date.");
        //        }
        //    }
        //---No longer needed as per story 171}
        // Question 3.2
        if (VisibilityQuestion_3_2 && IsVisible(() => Model.ExpectFuturePayroll))
        {
            var field = _editContext.Field(nameof(Model.ExpectFuturePayroll));
            if (!Model.ExpectFuturePayroll.HasValue)
            {
                _messageStore.Add(field, "Answer if you expect to pay for work completed in the State of Wisconsin in the future");
            }
        }
        // Question 3.2.1
        if (VisibilityQuestion_3_2_1 && IsVisible(() => Model.ExpectedFuturePayrollPeriod))
        {
            var field = _editContext.Field(nameof(Model.ExpectedFuturePayrollPeriod));
            if (!Model.ExpectedFuturePayrollPeriod.HasValue)
            {
                _messageStore.Add(field, "Pick the future time at which you expect to pay for work completed in the State of Wisconsin");
            }
        }
        // Question 4
        if (VisibilityQuestion_4 && IsVisible(() => Model.SelectedNoEmployeeReason))
        {
            var field = _editContext.Field(nameof(Model.SelectedNoEmployeeReason));
            if (!Model.SelectedNoEmployeeReason.HasValue)
            {
                _messageStore.Add(field, "Select the reason you no longer have paid employees working in Wisconsin");
            }
        }
        // Conditional cases
        if (Model.SelectedNoEmployeeReason.HasValue)
        {
            switch (Model.SelectedNoEmployeeReason.Value)
            {
                case NoEmployeeReason.BusiessWithoutEmployees:
                    if (IsVisible(() => Model.NoEmployeeExplanation) &&
                        string.IsNullOrWhiteSpace(Model.NoEmployeeExplanation))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.NoEmployeeExplanation)),
                            "Enter the reason for business continuation");
                    }
                    break;
                case NoEmployeeReason.LeasingFromPEO:
                    if (IsVisible(() => Model.PEOName) &&
                        string.IsNullOrWhiteSpace(Model.PEOName))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOName)),
                            "Enter the PEO Name");
                    }
                    if (IsVisible(() => Model.PEOUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.PEOUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.PEOFEIN))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOUIAccountNumber)),
                            "Enter PEO UI Account Number or PEO FEIN");
                    }
                    if (!string.IsNullOrWhiteSpace(Model.PEOUIAccountNumber) &&
                       !Regex.IsMatch(Model.PEOUIAccountNumber, _uiAccountNumberRegex))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOUIAccountNumber)),
                            "PEO UI Account Number must match the given format");
                    }
                    if (!string.IsNullOrWhiteSpace(_leasingStartDateRaw) && !Model.LeasingStartDate.HasValue)
                    {
                        _messageStore.Add(FieldIdentifier.Create(() => LeasingStartDateAsString), "Enter a valid date");
                    }
                    else if (IsVisible(() => Model.LeasingStartDate))
                    {
                        if (!Model.LeasingStartDate.HasValue)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LeasingStartDateAsString), "Enter the date leasing agreement started");
                        }
                        else if (Model.LeasingStartDate.Value < DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, 1, 1).AddYears(-6)))
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LeasingStartDateAsString), "The leasing agreement start date must be within the last six years");
                        }
                    }
                    break;
                case NoEmployeeReason.FiscalAgent:
                    if (IsVisible(() => Model.FiscalAgentName) &&
                        string.IsNullOrWhiteSpace(Model.FiscalAgentName))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentName)),
                            "Enter the Fiscal Agent Name");
                    }
                    if (IsVisible(() => Model.FiscalAgentUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.FiscalAgentUIAccountNumber))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentUIAccountNumber)),
                            "Enter the Fiscal Agent UI Account Number");
                    }
                    if (!string.IsNullOrWhiteSpace(Model.FiscalAgentUIAccountNumber) &&
                        !Regex.IsMatch(Model.FiscalAgentUIAccountNumber, _uiAccountNumberRegex))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentUIAccountNumber)),
                            "Fiscal Agent UI Account Number must match the given format");
                    }
                    break;
                case NoEmployeeReason.Other:
                    if (IsVisible(() => Model.OtherReason) &&
                        string.IsNullOrWhiteSpace(Model.OtherReason))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.OtherReason)),
                            "Enter the reason");
                    }
                    break;
                case NoEmployeeReason.NotOperatingInWisconsin:
                    var dateMin = DateOnly.FromDateTime(new DateTime(DateTime.Today.Year, 1, 1).AddYears(-6));
                    var dateMax = DateOnly.FromDateTime(DateTime.Today.AddDays(14));

                    if (!string.IsNullOrWhiteSpace(_lastEmploymentDateRaw) && !Model.LastEmploymentDate.HasValue)
                    {
                        _messageStore.Add(FieldIdentifier.Create(() => LastEmploymentDateAsString), "Enter a valid date");
                    }
                    else if (IsVisible(() => Model.LastEmploymentDate))
                    {
                        if (!Model.LastEmploymentDate.HasValue)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastEmploymentDateAsString), "Last Employment Date must be in a valid format (mm/dd/yyyy)");
                        }
                        else if (Model.LastEmploymentDate.Value < dateMin)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastEmploymentDateAsString), $"Last Employment Date must be on or after {dateMin:MM/dd/yyyy}");
                        }
                        else if (Model.LastEmploymentDate.Value > dateMax)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastEmploymentDateAsString), "Last Employment Date must be no later than today plus 2 weeks");
                        }
                    }
                    if (!string.IsNullOrWhiteSpace(_lastPayrollDateRaw) && !Model.LastPayrollDate.HasValue)
                    {
                        _messageStore.Add(FieldIdentifier.Create(() => LastPayrollDateAsString), "Enter a valid date");
                    }
                    else if (IsVisible(() => Model.LastPayrollDate))
                    {
                        if (!Model.LastPayrollDate.HasValue)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastPayrollDateAsString), "Last Payroll Date must be in a valid format (mm/dd/yyyy)");
                        }
                        else if (Model.LastPayrollDate.Value < dateMin)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastPayrollDateAsString), $"Last Payroll Date must be on or after {dateMin:MM/dd/yyyy}");
                        }
                        else if (Model.LastPayrollDate.Value > dateMax)
                        {
                            _messageStore.Add(FieldIdentifier.Create(() => LastPayrollDateAsString), "Last Payroll Date must be no later than today plus 2 weeks");
                        }
                    }
                    break;
            }
        }

        //Bind Notification Banner
        var properties = Model.GetType().GetProperties();
        foreach (var prop in properties)
        {
            var fieldId = new FieldIdentifier(Model, prop.Name);
            var errors = _editContext.GetValidationMessages(fieldId);

            foreach (var error in errors)
            {
                ValidationErrors.Add(error);
                ValidationFieldIds.Add(prop.Name);
            }
        }

        // Date facade fields store errors against component-level identifiers, not model properties.
        // Collect them separately so the top banner includes them.
        var dateFacades = new (FieldIdentifier Fi, string HtmlId)[]
        {
            (FieldIdentifier.Create(() => LastEmploymentDateAsString), "LastEmploymentDate"),
            (FieldIdentifier.Create(() => LastPayrollDateAsString),    "LastPayrollDate"),
            (FieldIdentifier.Create(() => LeasingStartDateAsString),   "LeasingStartDate"),
        };
        foreach (var (fi, htmlId) in dateFacades)
        {
            foreach (var error in _editContext.GetValidationMessages(fi))
            {
                ValidationErrors.Add(error);
                ValidationFieldIds.Add(htmlId);
            }
        }

        //validation for address
        if (addressErrors.Count != 0)
        {
            ValidationErrors.AddRange(addressErrors.Values.SelectMany(v =>
            {
                return v;
            }).ToList());
            ValidationFieldIds.AddRange(addressErrors.Keys.Select(k =>
            {
                return k.FieldName.ToString() ?? string.Empty;
            }).ToList());
        }

        _editContext.NotifyValidationStateChanged();
    }

    //private void ValidateAddressAnnotations(AddressModel address)
    //{
    //    var results = new List<ValidationResult>();
    //    var context = new ValidationContext(address);

    //    Validator.TryValidateObject(address, context, results, validateAllProperties: true);

    //    foreach (var result in results)
    //    {
    //        if (result.MemberNames != null && result.MemberNames.Any())
    //        {
    //            foreach (var memberName in result.MemberNames)
    //            {
    //                var fieldIdentifier = new FieldIdentifier(address, memberName);
    //                _messageStore.Add(fieldIdentifier, result.ErrorMessage ?? "Invalid value.");
    //            }
    //        }
    //        else
    //        {
    //            _messageStore.Add(new FieldIdentifier(address, string.Empty), result.ErrorMessage ?? "Invalid address.");
    //        }
    //    }
    //}

    /// <summary>Runs DataAnnotations validation on the contact address model.</summary>
    private Dictionary<FieldIdentifier, List<string>> ValidateAddressAnnotations(AddressModel address)
    {
        var errors = new Dictionary<FieldIdentifier, List<string>>();
        var ctx = new ValidationContext(address);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(address, ctx, results, true);

        foreach (var result in results)
        {
            foreach (var memberName in result.MemberNames)
            {
                var fi = new FieldIdentifier(address, memberName);

                if (_touchedFields.Contains(fi))
                {
                    _messageStore.Add(fi, result.ErrorMessage ?? "Invalid value.");
                }

                if (!errors.ContainsKey(fi))
                {
                    errors[fi] = new List<string>();
                }
                errors[fi].Add(result.ErrorMessage ?? "This field is invalid");
            }
        }
        return errors;
    }

}
