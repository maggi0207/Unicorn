using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;
/// <summary>
/// Voluntary Contribution Acknowledgement
/// </summary>

public partial class VoluntaryContributionAck
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IBankAccountOrchestrator BankAccountOrchestrator { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary> Selected Payment Method from the parent page</summary>
    [Parameter]
    [SupplyParameterFromQuery(Name = "payment")]
    public string? Selectedpayment { get; set; }

    /// <summary>
    /// Model to hold Voluntary Contribution Parameters
    /// </summary>
    public VoluntaryContribution Model { get; set; } = new();
    /// <summary>
    /// Model to hold difference amount
    /// </summary>
    public decimal _differenceAmount;


    private EditContext? _editContext;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    /// <summary>_showValidationSummary</summary>
    private bool _showValidationSummary = false;

    /// <summary>Show Acknowledgement had error</summary>
    private bool ShowAckError => !_acknowledged && (_ackTouched || _showValidationSummary);
    private readonly List<string> _validationErrors = [];

    private bool _ackTouched;
    private bool _acknowledged = false;
    private readonly string? _acknowledgementError = "You must acknowledge to proceed";

    /// <summary>_showValidationSummary</summary>
    private List<string> _bannerFieldIds = new();

    /// <summary>OnInitialized</summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
    }
    /// <summary>OnInitializedAsync</summary>
    protected override async Task OnInitializedAsync()
    {

        var sessionVCRequired = await BankAccountOrchestrator.GetVCRequiredToSessionAsync();
        if (!string.IsNullOrEmpty(sessionVCRequired))
        {
            Model.VcRequired = decimal.Parse(sessionVCRequired);
        }

        var sessionData = await BankAccountOrchestrator.GetVCPaymentToSessionAsync();

        if (!string.IsNullOrEmpty(sessionData))
        {
            Model.PaymentAmount = decimal.Parse(sessionData);
        }
        _differenceAmount = Model.VcRequired - Model.PaymentAmount;
        _editContext = new EditContext(Model);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _editContext.Validate();
            StateHasChanged();
        };
        await base.OnInitializedAsync();
    }

    private void OnAckChanged(ChangeEventArgs e)
    {
        _acknowledged = (bool) (e.Value ?? false);
        _ackTouched = true;
        ValidateForBanner();
        StateHasChanged();
    }


    private async Task PaymentAsync()
    {
        _showValidationSummary = true;
        if (!ValidateForBanner())
        {
            await JSRuntime.InvokeVoidAsync("scrollTo", new { top = 0, behavior = "smooth" });
            return;
        }
        var returnUrl = "billing-payments/voluntary-contribution";
        if (Selectedpayment == "ACH")
        {

            Nav.NavigateTo($"billing-payments/bank-account-payment-ach?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        else if (Selectedpayment == "Card")
        {
            Nav.NavigateTo($"billing-payments/card-payment?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
    }



    private void HandleContinue()
    {
        ValidateForBanner();
        StateHasChanged();
    }

    private void OnInvalid()
    {
        ValidateForBanner();
        StateHasChanged();
    }

    /// <summary>
    /// Validates the form and returns true if valid, false otherwise.
    /// Called by parent wizard to validate before navigation.
    /// </summary>
    public bool Validate()
    {
        if (_editContext == null)
        {
            return false;
        }
        var isValid = _editContext?.Validate() ?? false;
        var customValid = ValidateForBanner();
        StateHasChanged();
        return isValid && customValid;
    }

    private bool ValidateForBanner()

    {

        _validationErrors.Clear();
        var fieldIds = new List<string>();
        //_acknowledged = false;

        if (!_acknowledged && _acknowledgementError is not null)
        {
            if (_ackTouched || _showValidationSummary)
            {
                _acknowledged = false;
                _validationErrors.Add(_acknowledgementError);
                fieldIds.Add("ackAuthorizeCheckbox");
            }
        }

        _bannerFieldIds = fieldIds;

        _showValidationSummary = _validationErrors.Count > 0;

        StateHasChanged();

        return !_showValidationSummary;

    }

    private void GoBack()
    {

        Nav.NavigateTo("billing-payments/voluntary-contribution");

    }
}

