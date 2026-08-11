using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq.Expressions;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// Code-behind for the Account Details update page.
/// </summary>
public partial class AccountDetails
{
    /// <summary>
    /// Gets or sets the navigation manager.
    /// </summary>
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets the account details service.
    /// </summary>
    [Inject] private IAccountDetailsService AccountDetailsService { get; set; } = default!;

    /// <summary>
    /// Gets or sets the session manager.
    /// </summary>
    [Inject] private ISessionManager SessionManager { get; set; } = default!;

    /// <summary>
    /// The view model holding the account details bound to the form.
    /// </summary>
    private AccountDetailsModel _model = new();

    /// <summary>
    /// The EditContext used for form validation and field change tracking.
    /// </summary>
    private EditContext _editContext = default!;

    /// <summary>
    /// Tracks whether the form has been submitted, controlling error visibility.
    /// </summary>
    private bool _formSubmitted = false;

    /// <summary>
    /// Tracks whether the form is currently saving.
    /// </summary>
    private bool _isSaving = false;

    /// <summary>
    /// The original model used to detect which fields were changed.
    /// </summary>
    private AccountDetailsModel? _originalModel;

    /// <summary>
    /// Tracks which fields the user has interacted with so errors show on field change.
    /// </summary>
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    /// <summary>
    /// The currently selected employer SK from the session.
    /// </summary>
    private int _employerSK = 0;

    /// <summary>
    /// The list of validation error messages displayed in the NotificationBanner.
    /// </summary>
    private readonly List<string> _validationErrors = new();

    /// <summary>
    /// The list of field IDs corresponding to validation errors, enabling banner-to-field navigation.
    /// </summary>
    private readonly List<string> _validationFieldIds = new();

    /// <summary>
    /// Options for the FEIN change reason dropdown.
    /// </summary>
    private readonly List<SelectOption> _feinReasonOptions = new()
    {
        new SelectOption { Value = "1", Text = "EntryError" },
        new SelectOption { Value = "5", Text = "Other (additional explanation required)" }
    };

    /// <summary>
    /// Options for the Legal Name change reason dropdown.
    /// </summary>
    private readonly List<SelectOption> _legalNameReasonOptions = new()
    {
        new SelectOption { Value = "1", Text = "Department of Financial Institutions Name Change" },
        new SelectOption { Value = "2", Text = "Department Error" },
        new SelectOption { Value = "5", Text = "Other (additional explanation required)" }
    };

    /// <summary>
    /// Initializes the EditContext with data from the backend and sets up field change tracking.
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var sessionState = await SessionManager.GetAsync<UI.EmployerPortal.Web.Features.Shared.Accounts.Models.SelectedEmployerAccount>();
        _employerSK = sessionState?.EmployerAccount?.Id ?? 0;

        if (_employerSK > 0)
        {
            _model = await AccountDetailsService.GetAccountDetailsAsync(_employerSK);
            _model.OriginalFEIN = _model.FEIN;
            _model.OriginalLegalName = _model.LegalName;
        }
        else
        {
            _model = new AccountDetailsModel();
        }

        // Clone the original model to detect changes later
        _originalModel = new AccountDetailsModel
        {
            FEIN = _model.FEIN,
            LegalName = _model.LegalName,
            TradeName = _model.TradeName,
            PhoneNumber = _model.PhoneNumber,
            Extension = _model.Extension,
            CountryCode = _model.CountryCode,
            EmailAddress = _model.EmailAddress
        };

        _editContext = new EditContext(_model);

        // Track field interactions for progressive error display
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);

            if (_formSubmitted)
            {
                InvokeAsync(() =>
                {
                    RefreshValidationSummary();
                    StateHasChanged();
                });
            }
            else
            {
                StateHasChanged();
            }
        };
    }

    /// <summary>
    /// Handles form submission. Validates the form first; if valid, saves to backend and navigates back.
    /// </summary>
    private async Task HandleSubmit()
    {
        _formSubmitted = true;
        _validationErrors.Clear();
        _validationFieldIds.Clear();

        var isValid = _editContext.Validate();

        RefreshValidationSummary();

        if (!isValid)
        {
            StateHasChanged();
            return;
        }

        if (_originalModel != null && !HasAnyFieldChanged())
        {
            NavigationManager.NavigateTo("/manage-account/account-summary");
            return;
        }

        ClearUnchangedReasons();

        _isSaving = true;
        StateHasChanged();

        _model.FEIN = _model.FEIN?.Replace("-", string.Empty) ?? string.Empty;
        _model.PhoneNumber = _model.PhoneNumber?.Replace("-", string.Empty) ?? string.Empty;

        var (success, error) = await AccountDetailsService.UpdateAccountDetailsAsync(_model, _employerSK);

        _isSaving = false;
        
        if (success)
        {
            var query = BuildSuccessQuery();
            NavigationManager.NavigateTo($"/manage-account/account-summary?{string.Join("&", query)}");
        }
        else
        {
            _validationErrors.Add(error);
            StateHasChanged();
        }
    }

    /// <summary>
    /// Determines whether any field has been modified compared to the original model.
    /// </summary>
    private bool HasAnyFieldChanged()
    {
        return _originalModel == null
            || FeinHasChanged()
            || LegalNameHasChanged()
            || _model.TradeName != _originalModel.TradeName
            || PhoneHasChanged()
            || _model.EmailAddress != _originalModel.EmailAddress;
    }

    /// <summary>
    /// Clears the reason and explanation fields for FEIN and Legal Name if those fields were not changed.
    /// </summary>
    private void ClearUnchangedReasons()
    {
        if (!FeinHasChanged())
        {
            _model.ReasonForFeinChange = null;
            _model.FeinChangeReasonExplanation = null;
        }

        if (!LegalNameHasChanged())
        {
            _model.ReasonForLegalNameChange = null;
            _model.LegalNameChangeExplanation = null;
        }
    }

    /// <summary>
    /// Builds the success query string parameters based on which fields were changed.
    /// </summary>
    private List<string> BuildSuccessQuery()
    {
        var query = new List<string> { "success=true" };

        if (_originalModel == null)
        {
            return query;
        }

        if (_model.FEIN != (_originalModel.FEIN?.Replace("-", string.Empty) ?? string.Empty))
        {
            query.Add("fein=1");
        }

        if (_model.LegalName != _originalModel.LegalName)
        {
            query.Add("name=1");
        }

        if (_model.TradeName != _originalModel.TradeName)
        {
            query.Add("trade=1");
        }

        if (PhoneHasChanged())
        {
            query.Add("phone=1");
        }

        if (_model.EmailAddress != _originalModel.EmailAddress)
        {
            query.Add("email=1");
        }

        return query;
    }

    /// <summary>
    /// Refreshes the validation summary by collecting all field errors in UI layout order.
    /// </summary>
    private void RefreshValidationSummary()
    {
        if (!_formSubmitted)
        {
            return;
        }

        _validationErrors.Clear();
        _validationFieldIds.Clear();

        // Collect errors in UI layout order
        var fieldOrder = new[]
        {
            nameof(_model.FEIN),
            nameof(_model.ReasonForFeinChange),
            nameof(_model.FeinChangeReasonExplanation),
            nameof(_model.LegalName),
            nameof(_model.ReasonForLegalNameChange),
            nameof(_model.LegalNameChangeExplanation),
            nameof(_model.PhoneNumber),
            nameof(_model.EmailAddress)
        };

        foreach (var fieldName in fieldOrder)
        {
            var fi = new FieldIdentifier(_model, fieldName);
            foreach (var error in _editContext.GetValidationMessages(fi))
            {
                _validationErrors.Add(error);
                _validationFieldIds.Add(fieldName);
            }
        }
    }

    /// <summary>
    /// Returns true when a field's errors should be visible (after form submission or user interaction).
    /// </summary>
    private bool IsVisible<T>(Expression<Func<T>> forExpression)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(forExpression));
    }

    /// <summary>
    /// Cancels the update operation and navigates back to the manage account page.
    /// </summary>
    private void Cancel()
    {
        NavigationManager.NavigateTo("/manage-account");
    }

    /// <summary>
    /// Determines whether the FEIN field has been modified from its original value.
    /// </summary>
    private bool FeinHasChanged()
    {
        if (_originalModel == null) 
        {
            return false;
        }
        var currentFeinUnformatted = _model.FEIN?.Replace("-", string.Empty) ?? string.Empty;
        var originalFeinUnformatted = _originalModel.FEIN?.Replace("-", string.Empty) ?? string.Empty;
        return currentFeinUnformatted != originalFeinUnformatted;
    }

    /// <summary>
    /// Determines whether the Legal Name field has been modified from its original value.
    /// </summary>
    private bool LegalNameHasChanged()
    {
        return _originalModel != null && _model.LegalName != _originalModel.LegalName;
    }

    /// <summary>
    /// Determines whether any phone-related field (number, extension, or country code) has been modified.
    /// </summary>
    private bool PhoneHasChanged()
    {
        if (_originalModel == null)
        {
            return false;
        }
        var currentPhoneUnformatted = _model.PhoneNumber?.Replace("-", string.Empty) ?? string.Empty;
        var originalPhoneUnformatted = _originalModel.PhoneNumber?.Replace("-", string.Empty) ?? string.Empty;
        return currentPhoneUnformatted != originalPhoneUnformatted
            || _model.Extension != _originalModel.Extension
            || _model.CountryCode != _originalModel.CountryCode;
    }
}
