using System.ComponentModel.DataAnnotations;
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
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.NonProfit_501c3, Label = "Non-Profit with 501(c)(3) Ruling from IRS"},
      new RadioOption<BusinessCategory?> {Value = BusinessCategory.NonProfit_Other, Label = "Non-Profit (other)"},
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
    private bool VisibilityQuestion_2_1 => Model.AcquiredExistingBusiness.HasValue && Model.AcquiredExistingBusiness.Value;
    private bool VisibilityQuestion_2_1_1 => VisibilityQuestion_2_1 && Model.KnowAcquiredBusinessAccountNumber.HasValue && Model.KnowAcquiredBusinessAccountNumber.Value;
    private bool VisibilityQuestion_2_1_2 => VisibilityQuestion_2_1 && Model.KnowAcquiredBusinessAccountNumber.HasValue && !Model.KnowAcquiredBusinessAccountNumber.Value;
    private bool VisibilityQuestion_3 => Model.AcquiredExistingBusiness.HasValue && !Model.AcquiredExistingBusiness.Value;
    private bool VisibilityQuestion_3_1 => VisibilityQuestion_3 && Model.HavePaidEmployeesForWorkInWisconsin.HasValue && Model.HavePaidEmployeesForWorkInWisconsin.Value;
    private bool VisibilityQuestion_3_1_2 => VisibilityQuestion_3_1 && Model.HaveEmployeesCurrentlyWorkingInWisconsin.HasValue && !Model.HaveEmployeesCurrentlyWorkingInWisconsin.Value;
    private bool VisibilityCheckbox_InfoAccurate => VisibilityQuestion_3_1 && Model.HaveEmployeesCurrentlyWorkingInWisconsin == true;
    private bool VisibilityQuestion_3_2 => VisibilityQuestion_3 && Model.HavePaidEmployeesForWorkInWisconsin.HasValue && !Model.HavePaidEmployeesForWorkInWisconsin.Value;
    private bool VisibilityQuestion_3_2_1 => VisibilityQuestion_3_2 && Model.ExpectFuturePayroll.HasValue && Model.ExpectFuturePayroll.Value;
    private bool VisibilityQuestion_4 => Model.ExpectFuturePayroll.HasValue && !Model.ExpectFuturePayroll.Value;


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
    private void OnBusinessCategoryChanged(BusinessCategory? value)
    {
        Model.BusinessCategory = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.BusinessCategory)));
    }

    private void OnAcquiredExistingBusinessChanged(bool? value)
    {
        Model.AcquiredExistingBusiness = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.AcquiredExistingBusiness)));
    }

    private void OnKnowAcquiredBusinessAccountNumberChanged(bool? value)
    {
        Model.KnowAcquiredBusinessAccountNumber = value;

        // Note: When the AcquiredBusinessAddress model is null, it isn't validated
        Model.AcquiredBusinessAddress = !(value ?? true) ? new() : null;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.KnowAcquiredBusinessAccountNumber)));
    }

    private void OnHavePaidEmployeesForWorkInWisconsinChanged(bool? value)
    {
        Model.HavePaidEmployeesForWorkInWisconsin = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin)));
    }

    private void OnHaveEmployeesCurrentlyWorkingInWisconsinChanged(bool? value)
    {
        Model.HaveEmployeesCurrentlyWorkingInWisconsin = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin)));
    }

    private void OnExpectFuturePayrollChanged(bool? value)
    {
        Model.ExpectFuturePayroll = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.ExpectFuturePayroll)));
    }

    private void OnHaveSoldOrTransferredBusinessChanged(bool? value)
    {
        Model.HaveSoldOrTransferredBusiness = value;
        _editContext.NotifyFieldChanged(_editContext.Field(nameof(Model.HaveSoldOrTransferredBusiness)));
    }

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
        _messageStore = new ValidationMessageStore(_editContext);

        //Track validation state as user interacts
        _editContext.OnFieldChanged += (_, f) =>
        {
            _messageStore.Clear(f.FieldIdentifier);
        };
        _editContext.OnValidationRequested += (_, __) =>
        {
            ValidateModel();
        };
    }

    ///<Summary>
    /// Called by the parent wizard's HandleActionClick to validate be fore advancing
    ///</Summary>
    public bool Validate()
    {
        _formSubmitted = true;
        _messageStore.Clear();
        ValidateModel();
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();

        return !_editContext.GetValidationMessages().Any();
    }

    private readonly string _uiAccountNumberRegex = @"^\d{6}-\d{3}-\d$";

    // conditional validation
    private void ValidateModel()
    {
        _messageStore.Clear();

        // FEIN
        if (string.IsNullOrWhiteSpace(Model.FEIN))
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.FEIN)),
                "Enter the FEIN of the business.");
        }
        else if (!string.IsNullOrWhiteSpace(Model.FEIN)
            && !Regex.IsMatch(Model.FEIN, @"^\d{2}-\d{7}$"))
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.FEIN)),
                "FEIN must match the given format.");
        }

        // UI Account Number
        if (!string.IsNullOrWhiteSpace(Model.UIAccountNumber))
        {
            if (!Regex.IsMatch(Model.UIAccountNumber, _uiAccountNumberRegex))
            {
                _messageStore.Add(
                    _editContext.Field(nameof(Model.UIAccountNumber)),
                    "Employer UI Account Number must match the given format.");
            }
        }

        // Question 1
        if (!Model.BusinessCategory.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.BusinessCategory)),
                "Select your business category.");
        }

        // Question 2
        if (!Model.AcquiredExistingBusiness.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.AcquiredExistingBusiness)),
                "Select if you acquired an existing business.");
        }

        // Question 2.1
        if (VisibilityQuestion_2_1
            && !Model.KnowAcquiredBusinessAccountNumber.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.KnowAcquiredBusinessAccountNumber)),
                "Answer if you know the Account Number of the acquired business.");
        }

        // Question 2.1.1
        if (VisibilityQuestion_2_1_1
            && string.IsNullOrWhiteSpace(Model.AcquiredBusinessAccountNumber))
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.AcquiredBusinessAccountNumber)),
                "Enter the Account Number of the acquired business.");
        }
        else if (VisibilityQuestion_2_1_1
            && !Regex.IsMatch(Model.AcquiredBusinessAccountNumber, _uiAccountNumberRegex))
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.AcquiredBusinessAccountNumber)),
                "The Acquired Business UI Account Number must match the given format.");
        }

        // Question 2.1.2
        if (VisibilityQuestion_2_1_2)
        {
            if (string.IsNullOrWhiteSpace(Model.AcquiredBusinessName))
            {
                _messageStore.Add(
                    _editContext.Field(nameof(Model.AcquiredBusinessName)),
                    "Enter the Name of the acquired business.");
            }

            if (Model.AcquiredBusinessAddress is not null)
            {
                ValidateAddressAnnotations(Model.AcquiredBusinessAddress);
            }
        }

        // Question 3
        if (VisibilityQuestion_3
            && !Model.HavePaidEmployeesForWorkInWisconsin.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.HavePaidEmployeesForWorkInWisconsin)),
                "Answer if you have paid employees for work completed in the State of Wisconsin.");
        }

        // Question 3.1
        if (VisibilityQuestion_3_1
            && !Model.HaveEmployeesCurrentlyWorkingInWisconsin.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.HaveEmployeesCurrentlyWorkingInWisconsin)),
                "Answer if you currently have employees working in the State of Wisconsin.");
        }

        // Question 3.1.2
        if (VisibilityQuestion_3_1_2)
        {
            if (!Model.LastEmploymentDate.HasValue)
            {
                _messageStore.Add(
                    _editContext.Field(nameof(Model.LastEmploymentDate)),
                    "Enter the date that you last had employees employed for work in the State of Wisconsin.");
            }

            if (!Model.LastPayrollDate.HasValue)
            {
                _messageStore.Add(
                    _editContext.Field(nameof(Model.LastPayrollDate)),
                    "Enter the last date that you paid employees for work in the State of Wisconsin.");
            }
        }

        // Question 3.2
        if (VisibilityQuestion_3_2
            && !Model.ExpectFuturePayroll.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.ExpectFuturePayroll)),
                "Answer if you expect to pay for work completed in the State of Wisconsin in the future.");
        }

        // Question 3.2.1
        if (VisibilityQuestion_3_2_1
            && !Model.ExpectedFuturePayrollPeriod.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.ExpectedFuturePayrollPeriod)),
                "Pick the future time at which you expect to pay for work completed in the State of Wisconsin.");
        }

        // Acknowledgement checkbox (Q3.1 Yes)
        if (VisibilityCheckbox_InfoAccurate && !Model.InformationIsAccurate)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.InformationIsAccurate)),
                "You must confirm that the information provided is true and accurate.");
        }

        // Question 4
        if (VisibilityQuestion_4
            && !Model.HaveSoldOrTransferredBusiness.HasValue)
        {
            _messageStore.Add(
                _editContext.Field(nameof(Model.HaveSoldOrTransferredBusiness)),
                "Answer if you have sold or transferred your business.");
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
