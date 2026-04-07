using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
///
/// </summary>
public partial class PreliminaryQuestions
{
    [Inject] private RegistrationStateService RegistrationState { get; set; } = default!;

    private bool _formSubmitted = false;
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    private bool IsVisible<T>(Expression<Func<T>> fieldExpression)
    {
        var field = FieldIdentifier.Create(fieldExpression);
        return _formSubmitted || _touchedFields.Contains(field);
    }

    /// <summary>
    /// 
    /// </summary>
    private string? _peoValue;
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
    public static readonly IReadOnlyList<RadioOption<BusinessCategory?>> BusinessCategoryOptions = new[]
    {
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Commercial, Label = "Commercial"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Domestic, Label = "Domestic (in a private home)"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.Agricultural, Label = "Agricultural (Farming)"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.NonProfit_Other, Label = "Non-Profit (other)"},
    };

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
        new SelectOption {Value = FuturePayPeriod.WithinThirtyDays.ToString(), Text = "Within 30 days"},
        new SelectOption {Value = FuturePayPeriod.ThirtyToNinetyDays.ToString(), Text = "30 to 90 days"},
        new SelectOption {Value = FuturePayPeriod.SixMonths.ToString(), Text = "6 months"},
        new SelectOption {Value = FuturePayPeriod.OneYear.ToString(), Text = "One year"},
        new SelectOption {Value = FuturePayPeriod.MoreThanOneYear.ToString(), Text = "More than a year"},
    };

    // visibility delegates
    // 501(c)(3) visibility — driven by the standalone Yes/No question, not business category
    private bool Show501c3SubTree => Model.IsNonProfit501c3 == true;
    private bool ShowRulingUpload => Show501c3SubTree && Model.HasRulingFrom501c3IRS == true;
    private bool ShowHasAppliedQuestion => Show501c3SubTree && Model.HasRulingFrom501c3IRS == false;
    private bool ShowAppliedUpload => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == true;
    private bool ShowNotAppliedText => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == false;

    // existing visibility
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
            Model.LeasingStartDate = string.IsNullOrWhiteSpace(value)
                ? null
                : DateOnly.ParseExact(value, "yyyy-MM-dd");
        }
    }

    // data type fascades
    private string? LastEmploymentDateAsString
    {
        get => Model.LastEmploymentDate?.ToString("yyyy-MM-dd");
        set
        {
            Model.LastEmploymentDate = string.IsNullOrWhiteSpace(value)
                ? null
                : DateOnly.ParseExact(value, "yyyy-MM-dd");
        }
    }

    private string? LastPayrollDateAsString
    {
        get => Model.LastPayrollDate?.ToString("yyyy-MM-dd");
        set
        {
            Model.LastPayrollDate = string.IsNullOrWhiteSpace(value)
                ? null
                : DateOnly.ParseExact(value, "yyyy-MM-dd");
        }
    }

    private string? ExpectedFuturePayrollPeriodAsString
    {
        get => Model.ExpectedFuturePayrollPeriod.ToString();
        set
        {
            if (Enum.TryParse<FuturePayPeriod>(value, out var period))
            {
                Model.ExpectedFuturePayrollPeriod = period;
            }
        }
    }

    // change handlers

    /// <summary>
    /// Handles the standalone "Are you a non-profit 501(c)(3) organization?" Yes/No question.
    /// When changed to No, resets all 501(c)(3) sub-tree fields.
    /// </summary>
    private void OnIsNonProfit501c3Changed(bool? value)
    {
        Model.IsNonProfit501c3 = value;

        if (value != true)
        {
            Model.HasRulingFrom501c3IRS = null;
            Model.HasAppliedFor501c3WithIRS = null;
            Model.WillSupplyDocumentationLater = false;
            ResetField(() => Model.HasRulingFrom501c3IRS);
            ResetField(() => Model.HasAppliedFor501c3WithIRS);
        }

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.IsNonProfit501c3)));
    }

    private void OnBusinessCategoryChanged(BusinessCategory? value)
    {
        Model.BusinessCategory = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.BusinessCategory)));
    }

    private void OnHasRulingFrom501c3IRSChanged(bool? value)
    {
        Model.HasRulingFrom501c3IRS = value;
        Model.HasAppliedFor501c3WithIRS = null;
        Model.WillSupplyDocumentationLater = false;
        ResetField(() => Model.HasAppliedFor501c3WithIRS);
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HasRulingFrom501c3IRS)));
    }

    private void OnHasAppliedFor501c3WithIRSChanged(bool? value)
    {
        Model.HasAppliedFor501c3WithIRS = value;
        Model.WillSupplyDocumentationLater = false;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HasAppliedFor501c3WithIRS)));
    }

    private void OnWillSupplyDocumentationLaterChanged()
    {
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.WillSupplyDocumentationLater)));
    }

    private void OnAcquiredExistingBusinessChanged(bool? value)
    {
        Model.AcquiredExistingBusiness = value;

        ResetField(() => Model.KnowAcquiredBusinessAccountNumber);
        ResetField(() => Model.AcquiredBusinessAccountNumber);
        ResetField(() => Model.AcquiredBusinessName);
        ResetField(() => Model.AcquiredBusinessAddress);

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

        ResetField(() => Model.HaveEmployeesCurrentlyWorkingInWisconsin);
        ResetField(() => Model.ExpectFuturePayroll);
        ResetField(() => Model.ExpectedFuturePayrollPeriod);

        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin)));
    }

    private void OnHaveEmployeesCurrentlyWorkingInWisconsinChanged(bool? value)
    {
        Model.HaveEmployeesCurrentlyWorkingInWisconsin = value;
        Model.InformationIsAccurate = false;
       _messageStore.Clear(_editContext.Field(nameof(Model.InformationIsAccurate)));
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin)));
    }

    private void OnExpectFuturePayrollChanged(bool? value)
    {
        Model.ExpectFuturePayroll = value;
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
        _peoValue = null;
        Model.LeasingStartDate = null;
        Model.FiscalAgentName = null;
        Model.FiscalAgentUIAccountNumber = null;
        Model.OtherReason = null;
        Model.LastPayrollDate = null;
        Model.LastEmploymentDate = null;

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

    private async Task OnInput(string? value)
    {
        var digits = new string((value ?? "").Where(char.IsDigit).ToArray());
        if (digits.Length > 9)
        {
            digits = digits[..9];
        }

        _peoValue = digits.Length switch
        {
            > 2 => $"{digits[..2]}-{digits[2..]}",
            _ => digits
        };
        Model.PEOFEIN = _peoValue;
        await ValueChanged.InvokeAsync(Value);
    }
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

    /// <summary>
    /// Called by the parent wizard's HandleActionClick to validate before advancing.
    /// Saves the selected BusinessCategory to RegistrationStateService so Step 6 can read it.
    /// </summary>
    public bool Validate()
    {
        _formSubmitted = true;
        _messageStore.Clear();
        RunValidation();
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();

        var isValid = !_editContext.GetValidationMessages().Any();

        if (isValid)
        {
            // If user answered Yes to being a 501(c)(3) org, auto-populate Step 6 business category
            RegistrationState.PreliminaryBusinessCategory = Model.IsNonProfit501c3 == true
                ? BusinessCategory.NonProfit_501c3
                : Model.BusinessCategory;
        }

        return isValid;
    }

    private readonly string _uiAccountNumberRegex = @"^\d{6}-\d{3}-\d$";

    // conditional validation
    private void ValidateModel()
    {
        _messageStore.Clear();
        // FEIN
        if (IsVisible(() => Model.FEIN))
        {
            var field = _editContext.Field(nameof(Model.FEIN));
            if (string.IsNullOrWhiteSpace(Model.FEIN))
            {
                _messageStore.Add(field, "Enter the FEIN of the business.");
            }
            else if (!Regex.IsMatch(Model.FEIN, @"^\d{2}-\d{7}$"))
            {
                _messageStore.Add(field, "FEIN must match the given format.");
            }
        }
        // UI Account Number
        if (IsVisible(() => Model.UIAccountNumber) && !string.IsNullOrWhiteSpace(Model.UIAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.UIAccountNumber));
            if (!Regex.IsMatch(Model.UIAccountNumber, _uiAccountNumberRegex))
            {
                _messageStore.Add(field, "Employer UI Account Number must match the given format.");
            }
        }
        // 501(c)(3) standalone question
        if (IsVisible(() => Model.IsNonProfit501c3))
        {
            var field = _editContext.Field(nameof(Model.IsNonProfit501c3));
            if (!Model.IsNonProfit501c3.HasValue)
            {
                _messageStore.Add(field, "Answer if you are a non-profit organization as described in s.501(c)(3) of the IRS code.");
            }
        }
        // Business Category
        if (IsVisible(() => Model.BusinessCategory))
        {
            var field = _editContext.Field(nameof(Model.BusinessCategory));
            if (!Model.BusinessCategory.HasValue)
            {
                _messageStore.Add(field, "Select your business category.");
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

        // Acquired Existing Business
        if (IsVisible(() => Model.AcquiredExistingBusiness))
        {
            var field = _editContext.Field(nameof(Model.AcquiredExistingBusiness));
            if (!Model.AcquiredExistingBusiness.HasValue)
            {
                _messageStore.Add(field, "Select if you acquired an existing business.");
            }
        }
        // Question 2.1
        if (VisibilityQuestion_2_1 && IsVisible(() => Model.KnowAcquiredBusinessAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.KnowAcquiredBusinessAccountNumber));
            if (!Model.KnowAcquiredBusinessAccountNumber.HasValue)
            {
                _messageStore.Add(field, "Answer if you know the Account Number of the acquired business.");
            }
        }
        // Question 2.1.1
        if (VisibilityQuestion_2_1_1 && IsVisible(() => Model.AcquiredBusinessAccountNumber))
        {
            var field = _editContext.Field(nameof(Model.AcquiredBusinessAccountNumber));
            if (string.IsNullOrWhiteSpace(Model.AcquiredBusinessAccountNumber))
            {
                _messageStore.Add(field, "Enter the Account Number of the acquired business.");
            }
            else if (!Regex.IsMatch(Model.AcquiredBusinessAccountNumber, _uiAccountNumberRegex))
            {
                _messageStore.Add(field, "The Acquired Business UI Account Number must match the given format.");
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
                    _messageStore.Add(field, "Enter the Name of the acquired business.");
                }
            }
            if (Model.AcquiredBusinessAddress is not null)
            {
                ValidateAddressAnnotations(Model.AcquiredBusinessAddress);
            }
        }
        // Question 3
        if (VisibilityQuestion_3
            && IsVisible(() => Model.HavePaidEmployeesForWorkInWisconsin))
        {
            var field = _editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin));
            if (!Model.HavePaidEmployeesForWorkInWisconsin.HasValue)
            {
                _messageStore.Add(field, "Answer if you have paid employees for work completed in Wisconsin.");
            }
        }
        // Question 3.1
        if (VisibilityQuestion_3_1
            && IsVisible(() => Model.HaveEmployeesCurrentlyWorkingInWisconsin))
        {
            var field = _editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin));
            if (!Model.HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
            {
                _messageStore.Add(field, "Answer if you currently have employees working in the State of Wisconsin.");
            }
        }
        // Acknowledgement checkbox (Q3.1 Yes)
        if (VisibilityCheckbox_InfoAccurate && !Model.InformationIsAccurate)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.InformationIsAccurate)),
                "You must confirm that the information provided is true and accurate.");
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
                _messageStore.Add(field, "Answer if you expect to pay for work completed in the State of Wisconsin in the future.");
            }
        }
        // Question 3.2.1
        if (VisibilityQuestion_3_2_1 && IsVisible(() => Model.ExpectedFuturePayrollPeriod))
        {
            var field = _editContext.Field(nameof(Model.ExpectedFuturePayrollPeriod));
            if (!Model.ExpectedFuturePayrollPeriod.HasValue)
            {
                _messageStore.Add(field, "Pick the future time at which you expect to pay for work completed in the State of Wisconsin.");
            }
        }
        // Question 4
        if (VisibilityQuestion_4 && IsVisible(() => Model.SelectedNoEmployeeReason))
        {
            var field = _editContext.Field(nameof(Model.SelectedNoEmployeeReason));
            if (!Model.SelectedNoEmployeeReason.HasValue)
            {
                _messageStore.Add(field, "Select the reason you no longer have paid employees working in Wisconsin.");
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
                            "Enter the reason for business continuation.");
                    }
                    break;
                case NoEmployeeReason.LeasingFromPEO:
                    if (IsVisible(() => Model.PEOName) &&
                        string.IsNullOrWhiteSpace(Model.PEOName))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOName)),
                            "Enter the PEO Name.");
                    }
                    if (IsVisible(() => Model.PEOUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.PEOUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.PEOFEIN))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOUIAccountNumber)),
                            "Enter PEO UI Account Number or FEIN.");
                    }
                    if (!string.IsNullOrWhiteSpace(Model.PEOUIAccountNumber) &&
                        !Regex.IsMatch(Model.PEOUIAccountNumber, _uiAccountNumberRegex))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.PEOUIAccountNumber)),
                            "PEO UI Account Number must match the given format.");
                    }
                    if (IsVisible(() => Model.LeasingStartDate) &&
                        !Model.LeasingStartDate.HasValue)
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.LeasingStartDate)),
                            "Enter the date leasing agreement started.");
                    }
                    break;
                case NoEmployeeReason.FiscalAgent:
                    if (IsVisible(() => Model.FiscalAgentName) &&
                        string.IsNullOrWhiteSpace(Model.FiscalAgentName))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentName)),
                            "Enter the Fiscal Agent Name.");
                    }
                    if (IsVisible(() => Model.FiscalAgentUIAccountNumber) &&
                        string.IsNullOrWhiteSpace(Model.FiscalAgentUIAccountNumber))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentUIAccountNumber)),
                            "Enter the Fiscal Agent UI Account Number.");
                    }
                    if (!string.IsNullOrWhiteSpace(Model.FiscalAgentUIAccountNumber) &&
                        !Regex.IsMatch(Model.FiscalAgentUIAccountNumber, _uiAccountNumberRegex))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.FiscalAgentUIAccountNumber)),
                            "Fiscal Agent UI Account Number must match the given format.");
                    }
                    break;
                case NoEmployeeReason.Other:
                    if (IsVisible(() => Model.OtherReason) &&
                        string.IsNullOrWhiteSpace(Model.OtherReason))
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.OtherReason)),
                            "Enter the reason.");
                    }
                    break;
                case NoEmployeeReason.NotOperatingInWisconsin:
                    if (IsVisible(() => Model.LastEmploymentDate) &&
                        !Model.LastEmploymentDate.HasValue)
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.LastEmploymentDate)),
                            "Enter the last employment date.");
                    }
                    if (IsVisible(() => Model.LastPayrollDate) &&
                        !Model.LastPayrollDate.HasValue)
                    {
                        _messageStore.Add(
                            _editContext.Field(nameof(Model.LastPayrollDate)),
                            "Enter the last payroll date.");
                    }
                    break;
            }
        }

        _editContext.NotifyValidationStateChanged();
    }

    private void ValidateAddressAnnotations(AddressModel address)
    {
        var results = new List<ValidationResult>();
        var context = new ValidationContext(address);

        Validator.TryValidateObject(address, context, results, validateAllProperties: true);

        foreach (var result in results)
        {
            if (result.MemberNames != null && result.MemberNames.Any())
            {
                foreach (var memberName in result.MemberNames)
                {
                    var fieldIdentifier = new FieldIdentifier(address, memberName);
                    _messageStore.Add(fieldIdentifier, result.ErrorMessage ?? "Invalid value.");
                }
            }
            else
            {
                _messageStore.Add(new FieldIdentifier(address, string.Empty), result.ErrorMessage ?? "Invalid address.");
            }
        }
    }
}
