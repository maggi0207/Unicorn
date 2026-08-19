using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Validation;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Pages;
/// <summary>
/// ACH Contact
/// </summary>
public partial class ACHContact
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    /// <summary>
    /// Gets or sets the primary data model for the form
    /// </summary>
    public ACHContactModel Model { get; set; } = new();
    [Inject]
    private IContactInformationService ContactInformationService { get; set; } = default!;
    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Tracks if the form has been attempted to be submitted
    /// </summary>
    private EditContext _editContext = default!;

    /// <summary>Reference to CustomValidator for displaying nested object errors.</summary>

    private CustomValidator? _customValidator;

    /// <summary>Tracks whether the form has been submitted at least once.</summary>

    private bool _formSubmitted = false;

    /// <summary>Tracks whether the current form state has any validation errors.</summary>

    private bool _showValidationSummary;
    private List<string> _validationErrors = [];
    private readonly List<string> _validationFieldIds = [];
    /// <summary>Tracks which fields have been interacted with so errors show on blur.</summary>

    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private EmployerAccount? _employerSK;
    private bool _contactexist = false;
    private bool Showback { get; set; } = false;


    private void InitializedEditContext()
    {
        _editContext = new EditContext(Model);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            if (_showValidationSummary)
            {
                _editContext.Validate();
                _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();
                _validationFieldIds.Clear();

                foreach (var message in _validationErrors)
                {
                    _validationFieldIds.Add(GetFieldIdForMessage(message));
                }
                _showValidationSummary = _validationErrors.Any();
            }
            StateHasChanged();
        };
    }
    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override void OnInitialized()
    {
        InitializedEditContext();
    }
    /// <summary>
    /// Pageload
    /// </summary>
    /// <returns></returns>
    protected override async Task OnInitializedAsync()
    {
        if (!await PageAuthorizationService.AuthorizeAsync(AuthorizationPolicies.RequiresAnyPaymentsPermission))
        {
            return;
        }
        var uri = new Uri(Nav.Uri);
        var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
        Showback = query["showback"] == "true";

        //SERVICE CALL

        _employerSK = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

        var contactTypeCodeSK = 4;
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var employerSk = _employerSK?.Id ?? 0;

        var result = await ContactInformationService.GetEmployerWebContact(secureUserSK, employerSk, contactTypeCodeSK);

        if (result != null)
        {
            Model = result;
            InitializedEditContext();
            // _contactexist = true;
        }
    }
    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
    }
    private void HandleContinue()
    {
        _formSubmitted = true;
        StateHasChanged();
    }

    private async Task OnPhoneNumberFormatChanged(string value)
    {
        Model.PhoneNumberFormat = value;
        Model.PhoneNumber = string.Empty;
        StateHasChanged();
    }

    /// <summary>Called by EditForm when top-level validation fails.</summary>

    private void OnInvalid()
    {
        _formSubmitted = true;
        _showValidationSummary = true;
        _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();
        _validationFieldIds.Clear();

        foreach (var message in _validationErrors)
        {
            _validationFieldIds.Add(GetFieldIdForMessage(message));
        }
        StateHasChanged();
    }

    private static string GetFieldIdForMessage(string message)
    {
        return message switch
        {
            var m when m.Contains("Contact Name", StringComparison.OrdinalIgnoreCase) => "con-name",
            var m when m.Contains("Phone Number Format", StringComparison.OrdinalIgnoreCase) => "con-phone-format",
            var m when m.Contains("Phone Number", StringComparison.OrdinalIgnoreCase) => "con-phone",
            var m when m.Contains("Email", StringComparison.OrdinalIgnoreCase)
                                    && !m.Contains("Verify", StringComparison.OrdinalIgnoreCase)
                                    && !m.Contains("match", StringComparison.OrdinalIgnoreCase)
                                    && !m.Contains("Invalid Email", StringComparison.OrdinalIgnoreCase)
                                    => "record-email",
            var m when m.Contains("Verify Email", StringComparison.OrdinalIgnoreCase) => "record-confirm-email",
            var m when m.Contains("match", StringComparison.OrdinalIgnoreCase) => "record-confirm-email",
            var m when m.Contains("Invalid email", StringComparison.OrdinalIgnoreCase) => "record-confirm-email",
            _ => string.Empty
        };
    }
    /// <summary>
    ///
    /// </summary>
    public static readonly List<SelectOption> PhoneNumberFormat = new()
    {
        new SelectOption {Value = "United States/Canada", Text = "United States/Canada"},
        new SelectOption {Value = "International", Text = "International"},

    };

    private void GoBack()
    {
        //Nav.NavigateTo(Nav.BaseUri);
        if (Showback)
        {
            Nav.NavigateTo("billing-payments/bank-account-payment-ach");
        }
    }
    private async Task Save()
    {
        _contactexist = false;
        _formSubmitted = true;
        if (!_editContext.Validate())
        {
            OnInvalid();
            return;
        }
        _showValidationSummary = false;
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var employerSk = _employerSK?.Id ?? 0;
        Model.InternationalFlag = Model.PhoneNumberFormat == "International";

        var result = await ContactInformationService.GetEmployerWebContact(UserAccountService.GetUserSKClaim(), _employerSK?.Id ?? 0, 4);
        if (result != null)
        {
            Model.WebContactInformationsk = result.WebContactInformationsk;
            _contactexist = true;
            await ContactInformationService.SaveWebContact(Model, secureUserSK, employerSk);
        }
        else
        {
            _contactexist = true;
            await ContactInformationService.SaveWebContact(Model, secureUserSK, employerSk);
        }

    }
    //        // _contactexist = true;


}
