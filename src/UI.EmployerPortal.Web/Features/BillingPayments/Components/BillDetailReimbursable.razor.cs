using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.BillingPayments.Services;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using EmployerRequest = UI.EmployerPortal.Generated.ServiceClients.BillDetailService.EmployerRequest;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;
/// <summary>
/// 
/// </summary>
public partial class BillDetailReimbursable
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject] private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject] private IBillDetailServices BillDetailServices { get; set; } = default!;
    [Inject] private IBankAccountOrchestrator BankAccountOrchestrator { get; set; } = default!;
    /// <summary>Model</summary>
    public ReimbursableBillingDetail Model { get; set; } = new();
    private EmployerAccount? _employerSK;
    private EditContext? _editContext;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private string? _selectedpayment;
    private bool _isLoading = true;
    /// <summary>_showValidationSummary</summary>
    private bool _showValidationSummary = false;
    private readonly List<string> _validationErrors = [];
    private readonly List<string> _validationFieldIds = [];
    /// <summary>_amountHasError</summary>
    public bool _amountHasError = false;
    /// <summary>_showValidationSummary</summary>
    public bool _paymentMethodHasError = false;
    /// <summary>OnInitialized</summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
    }
    /// <summary>OnInitializedAsync</summary>
    protected override async Task OnInitializedAsync()
    {
        _employerSK = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var employerSk = _employerSK?.Id ?? 0;

        var request = new EmployerRequest
        {
            EmployerSK = employerSk,
            SecureUserSK = secureUserSK
        };

        var result = await BillDetailServices.GetReimburseBillingDetail(request);
        if (result != null)
        {
            Model.AmountToPay = result.AmountToPay;
            Model.TotalEFTPayments = result.TotalEFTPayments;
            Model.TotalOutstandingBalance = result.TotalOutstandingBalance;
            _isLoading = false;
        }

        //var sessionData = await BankAccountOrchestrator.GetPaymentToSessionAsync();
        //if (!string.IsNullOrEmpty(sessionData))
        //{
        //    Model.AmountToPay = decimal.Parse(sessionData);
        //    _selectedpayment = sessionData;
        //}
        // await BankAccountOrchestrator.SavePaymentToSessionAsync("0");

        var sessionData = await BankAccountOrchestrator.GetPaymentStateFromSessionAsync();
        if (sessionData != null)
        {
            if (sessionData.AmountToPay > 0)
            {
                Model.AmountToPay = sessionData.AmountToPay;
            }
            if (!string.IsNullOrEmpty(sessionData.SelectedPaymentMethod))
            {
                _selectedpayment = sessionData.SelectedPaymentMethod;
            }
        }

        _editContext = new EditContext(Model);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _editContext.Validate();
            StateHasChanged();
        };
        await base.OnInitializedAsync();
    }
    private void OnAmountChanged(decimal? value)
    {
        Model.AmountToPay = value ?? 0m;
        _touchedFields.Add(FieldIdentifier.Create(() => Model.AmountToPay));
        ValidateForBanner();
    }
    private async Task PaymentAsync()
    {
        _showValidationSummary = true;
        if (!ValidateForBanner())
        {
            return;
        }
        var paymentState = new PaymentState()
        {
            AmountToPay = Model.AmountToPay,
            BalanceDue = Model.TotalOutstandingBalance,
            SelectedPaymentMethod = _selectedpayment
        };
        Model.SelectedPaymentMethod = _selectedpayment;
        await BankAccountOrchestrator.SavePaymentStateToSessionAsync(paymentState);
        await BankAccountOrchestrator.SavePaymentToSessionAsync(Model.AmountToPay.ToString(), "Payment");
        switch (_selectedpayment)
        {
            case "ACH":
                Nav.NavigateTo("billing-payments/bank-account-payment-ach");
                break;
            case "Card":
                Nav.NavigateTo("billing-payments/card-payment");
                break;
            case "Check":
                Nav.NavigateTo($"billing-payments/pay-by-check?returnUrl={Uri.EscapeDataString("billing-payments")}");
                break;
        }
    }

    private void OnpaymentChanged(ChangeEventArgs e)
    {
        _selectedpayment = e.Value?.ToString() ?? "";
        _touchedFields.Add(FieldIdentifier.Create(() => _selectedpayment));
        ValidateForBanner();
    }

    private void HandleContinue()
    {
        ValidateForBanner();
        StateHasChanged();
    }

    /// <summary>Called by EditForm when top-level validation fails.</summary>

    private void OnInvalid()
    {
        ValidateForBanner();
        StateHasChanged();
    }
    /// <summary>
    /// Gets the current validation state.
    /// </summary>
    private bool IsValid => _editContext?.Validate() ?? false;
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
        _validationFieldIds.Clear();
        _amountHasError = false;
        _paymentMethodHasError = false;

        if (Model.AmountToPay <= 0)
        {
            if (_touchedFields.Contains(FieldIdentifier.Create(() => Model.AmountToPay)) || _showValidationSummary)
            {
                _amountHasError = true;
                _validationErrors.Add("Amount to pay is required and must be greater than $0.00");
                _validationFieldIds.Add("amount");
            }
        }
        if (string.IsNullOrWhiteSpace(_selectedpayment))
        {
            if (_touchedFields.Contains(FieldIdentifier.Create(() => _selectedpayment)) || _showValidationSummary)
            {
                _paymentMethodHasError = true;
                _validationErrors.Add("Payment method selection is required");
                _validationFieldIds.Add("ach");
            }

        }
        _showValidationSummary = _validationErrors.Count > 0;
        StateHasChanged();
        return !_showValidationSummary;
    }
}
