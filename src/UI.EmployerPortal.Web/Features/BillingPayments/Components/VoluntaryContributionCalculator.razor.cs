using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.BillingPayments.Services;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using recalcRequest = UI.EmployerPortal.Generated.ServiceClients.VoluntaryContributionService.RecalculateSavingsTaxInfoRequest;
using voluntaryRequest = UI.EmployerPortal.Generated.ServiceClients.VoluntaryContributionService.VoluntaryRequest;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;
/// <summary>
/// /
/// </summary>

public partial class VoluntaryContributionCalculator
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject] private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;
    [Inject] private IVoluntaryContributionServices VoluntaryContributionServices { get; set; } = default!;
    [Inject] private IBankAccountOrchestrator BankAccountOrchestrator { get; set; } = default!;
    /// <summary>
    /// 
    /// </summary>
    public VoluntaryContribution Model { get; set; } = new();
    private EmployerAccount? _employerSK;
    private EditContext? _editContext;
    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private string? _selectedpayment;
    /// <summary>_showValidationSummary</summary>
    private bool _showValidationSummary = false;
    private readonly List<string> _validationErrors = [];
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

        var request = new voluntaryRequest
        {
            EmployerSK = employerSk,
            SecureUserSK = secureUserSK,
            Year = DateTime.Now.Year
        };
        var result = await VoluntaryContributionServices.GetVoluntaryContributionDetail(request);
        if (result != null)
        {
            Model.EstimatedTaxablePayroll = result.EstimatedTaxablePayroll;
            Model.PaymentAmount = result.PaymentAmount;
            Model.NetTaxSavings = result.NetTaxSavings;
            Model.ReserveFundBalance = result.ReserveFundBalance;
            Model.ReserveFundPercentage = result.ReserveFundPercentage;
            Model.TaxRateForYear = result.TaxRateForYear;
            Model.TaxSavingsBasedOnEstimatedPayroll = result.TaxSavingsBasedOnEstimatedPayroll;
            Model.TaxablePayRoll = result.TaxablePayRoll;
            Model.VcRequired = result.VcRequired;
            Model.Lowerrate = result.Lowerrate;
        }
        // await BankAccountOrchestrator.SavePaymentToSessionAsync("0");
        var sessionData = await BankAccountOrchestrator.GetPaymentToSessionAsync();
        if (!string.IsNullOrEmpty(sessionData))
        {
            Model.PaymentAmount = decimal.Parse(sessionData);
            _selectedpayment = sessionData;
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
        Model.PaymentAmount = value ?? 0m;
        _touchedFields.Add(FieldIdentifier.Create(() => Model.PaymentAmount));
        ValidateForBanner();
    }
    private async Task PaymentAsync()
    {
        _showValidationSummary = true;
        if (!ValidateForBanner())
        {
            return;
        }
        var returnUrl = "billing-payments/voluntary-contribution";
        if (_selectedpayment == "ACH")
        {
            Model.SelectedPaymentMethod = _selectedpayment;
            await BankAccountOrchestrator.SavePaymentToSessionAsync(Model.PaymentAmount.ToString(), "Voluntary Contribution");
            Nav.NavigateTo($"billing-payments/bank-account-payment-ach?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        else if (_selectedpayment == "Card")
        {
            Model.SelectedPaymentMethod = _selectedpayment;
            await BankAccountOrchestrator.SavePaymentToSessionAsync(Model.PaymentAmount.ToString(), "Voluntary Contribution");
            Nav.NavigateTo($"billing-payments/card-payment?returnUrl={Uri.EscapeDataString(returnUrl)}");
        }
        else if (_selectedpayment == "Check")
        {
            Model.SelectedPaymentMethod = _selectedpayment;
            await BankAccountOrchestrator.SavePaymentToSessionAsync(Model.PaymentAmount.ToString(), "Voluntary Contribution");
            Nav.NavigateTo("billing-payments/pay-by-check-voluntary");
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

    private async Task RecalculatePayment()
    {
        _employerSK = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        var secureUserSK = UserAccountService.GetUserSKClaim();
        var employerSk = _employerSK?.Id ?? 0;

        var request = new recalcRequest
        {
            EmployerSK = employerSk,
            SecureUserSK = secureUserSK,
            EstPayrollAmt = Model.EstimatedTaxablePayroll,
            Year = DateTime.Now.Year
        };
        var result = await VoluntaryContributionServices.GetRecalculateVoluntaryContributionDetail(request);
        if (result != null)
        {
            Model.EstimatedTaxablePayroll = result.EstimatedTaxablePayroll;
            Model.PaymentAmount = result.PaymentAmount;
            Model.NetTaxSavings = result.NetTaxSavings; //NetSavings
            Model.ReserveFundBalance = result.ReserveFundBalance;
            Model.ReserveFundPercentage = result.ReserveFundPercentage;
            Model.TaxRateForYear = result.TaxRateForYear;
            Model.TaxSavingsBasedOnEstimatedPayroll = result.TaxSavingsBasedOnEstimatedPayroll;
            Model.TaxablePayRoll = result.TaxablePayRoll;
            Model.VcRequired = result.VcRequired;
            Model.DisclaimerText = result.DisclaimerText;
            Model.Lowerrate = result.Lowerrate;
        }
        _showValidationSummary = true;
        await InvokeAsync(StateHasChanged);
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
        _amountHasError = false;
        _paymentMethodHasError = false;

        if (Model.PaymentAmount <= 0)
        {
            if (_touchedFields.Contains(FieldIdentifier.Create(() => Model.PaymentAmount)) || _showValidationSummary)
            {
                _amountHasError = true;
                _validationErrors.Add("Amount to pay is required and must be greater than $0.00");
            }
        }
        if (string.IsNullOrWhiteSpace(_selectedpayment))
        {
            if (_touchedFields.Contains(FieldIdentifier.Create(() => _selectedpayment)) || _showValidationSummary)
            {
                _paymentMethodHasError = true;
                _validationErrors.Add("Payment method selection is required");
            }

        }
        _showValidationSummary = _validationErrors.Count > 0;
        StateHasChanged();
        return !_showValidationSummary;
    }
    private void GoBack()
    {

        Nav.NavigateTo("billing-payments/payment-options?source=flow");

    }
}

