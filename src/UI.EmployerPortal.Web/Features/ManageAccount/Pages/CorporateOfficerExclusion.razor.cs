using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.Dashboard;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// Corporate Officer Exclusion reasonableness worksheet and election confirmation page.
/// </summary>
public partial class CorporateOfficerExclusion
{
    /// <summary>
    /// Message shown when Submit is clicked before Calculate.
    /// </summary>
    private const string MustCalculateMessage = "You must calculate your net savings before electing this exclusion.";

    /// <summary>
    /// Message shown when the save call fails unexpectedly.
    /// </summary>
    private const string SubmitFailedMessage = "Worksheet failed to submit. Please try again.";

    /// <summary>
    /// Message shown on the success confirmation banner.
    /// </summary>
    private const string SubmitSuccessMessage = "Corporate Officer Exclusion submitted.";

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private ICorporateOfficerExclusionService CorporateOfficerExclusionService { get; set; } = default!;

    [Inject]
    private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

    [Inject]
    private IAsyncRetryPolicy<CorporateOfficerExclusionService> RetryPolicy { get; set; } = default!;

    private int _employerSK = 0;

    private COEResonableResponse _response = default!;

    private COESaveResponse? _saveResponse = default!;

    private string Message { get; set; } = default!;

    private bool? _isSaveSuccess = null;
    private bool _isRecalculating = false;
    private string? _recalculateStatus;

    private bool _isSaving = false;

    private bool _isCalculateClicked = false;
    private bool _isNoSavingsShown = false;

    /// <summary>
    /// True when Submit was clicked before Calculate.
    /// </summary>
    private bool _mustCalculateError = false;

    /// <summary>
    /// True when save threw or returned a null response.
    /// </summary>
    private bool _submitFailedError = false;

    /// <summary>
    /// Success banner text for the confirmation screen.
    /// </summary>
    private string SuccessBannerMessage => SubmitSuccessMessage;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorporateOfficerExclusion"/> page.
    /// </summary>
    /// <param name="sessionManager">Session manager used to resolve the selected employer.</param>
    public CorporateOfficerExclusion(ISessionManager sessionManager)
    {
        SessionManager = sessionManager;
    }

    /// <summary>
    /// After rendering, set focus on the h1 heading if this is the save success page contents.
    /// </summary>
    /// <param name="firstRender">True when this is the first render.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_saveResponse != null && _isSaveSuccess.HasValue && _isSaveSuccess.Value)
        {
            await JSRuntime.InvokeVoidAsync("focusElement", "save-heading");
        }
    }

    /// <summary>
    /// Loads the reasonableness worksheet data for the selected employer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task LoadData()
    {
        try
        {
            var selected = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = selected?.EmployerAccount?.Id ?? 0;
            if (_employerSK == 0)
            {
                return;
            }

            var secureUserSK = UserAccountService.GetUserSKClaim();

            _response = await CorporateOfficerExclusionService.GetData(_employerSK, DateTime.Now.Year, secureUserSK);
        }
        catch
        {
        }
        finally
        {
        }
    }

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadData();
    }

    /// <summary>
    /// Clears worksheet-level error banners before a new calculate or submit attempt.
    /// </summary>
    private void ClearWorksheetErrors()
    {
        _mustCalculateError = false;
        _submitFailedError = false;
    }

    /// <summary>
    /// Recalculates the reasonableness worksheet from the current payroll inputs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleRecalculateAsync()
    {
        ClearWorksheetErrors();
        _recalculateStatus = "Calculating";
        _isRecalculating = true;
        _isSaveSuccess = null;
        _saveResponse = null;

        try
        {
            var selectedClient = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

            if (selectedClient == null)
            {
                return;
            }

            var employerSk = selectedClient.Id;

            if (employerSk == 0)
            {
                return;
            }

            var secureUserSK = UserAccountService.GetUserSKClaim();

            try
            {
                var request = new Generated.ServiceClients.AccountSummaryService.COEReasonablenessRequest()
                {
                    EmployerSK = employerSk,
                    SecureUserSK = secureUserSK,
                    year = DateTime.Now.Year,
                    PayrollSubjectToFUTAWithoutCOE = _response.FUTAPayroll,
                    PayrollSubjectToWITaxRateWithCOE = _response.WITaxablePayrollWithCOE,
                    PayrollSubjectToWITaxRateWithoutCOE = _response.WITaxablePayrollWithoutCOE,
                };
                _isCalculateClicked = true;
                _response = await CorporateOfficerExclusionService.Recalculate(request);
            }
            catch
            {
                _isCalculateClicked = false;
                _submitFailedError = true;
            }
        }
        finally
        {
            _isRecalculating = false;
        }

        // Short delay to allow browser to register updated status message for accessibility
        await Task.Delay(500);
        _recalculateStatus = "Calculate complete";
        StateHasChanged();
        _recalculateStatus = "";
        StateHasChanged();
    }

    /// <summary>
    /// Submits the Corporate Officer Exclusion election after a successful calculate.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleSaveAsync()
    {
        if (_isSaving || _isRecalculating)
        {
            return;
        }

        ClearWorksheetErrors();

        if (!_isCalculateClicked || _response == null)
        {
            _mustCalculateError = true;
            _isSaveSuccess = false;
            return;
        }

        _isSaving = true;

        try
        {
            var selectedClient =
                await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

            if (selectedClient == null || selectedClient.Id == 0)
            {
                _submitFailedError = true;
                _isSaveSuccess = false;
                return;
            }

            var secureUserSK = UserAccountService.GetUserSKClaim();

            var request = new CorporateOfficerExclusionRequest
            {
                EmployerSK = selectedClient.Id,
                SecureUserSK = secureUserSK,
                NetSavings = _response.NetSavings,
                Year = DateTime.Now.Year,
                IsCalculateClicked = _isCalculateClicked,
                IsNoSavingsShown = _isNoSavingsShown
            };

            _saveResponse =
                await CorporateOfficerExclusionService.Save(request);

            if (_saveResponse == null)
            {
                _submitFailedError = true;
                _isSaveSuccess = false;
                return;
            }

            // Server returned the no-savings warning — stay on worksheet and show banner messages.
            if (_saveResponse.IsNoSavingsShown)
            {
                _isNoSavingsShown = true;
                _isSaveSuccess = false;
                EnsureWarningOrSubmitFailed();
                return;
            }

            // Handle the actual save result.
            _isSaveSuccess = _saveResponse.Success;

            if (!_saveResponse.Success)
            {
                // Business rule violations are shown via _saveResponse.WarningMessage.
                EnsureWarningOrSubmitFailed();
            }
        }
        catch
        {
            _submitFailedError = true;
            _isSaveSuccess = false;
            _saveResponse = null;
        }
        finally
        {
            _isSaving = false;
        }
    }

    /// <summary>
    /// Navigates to the UI Account Summary page (Cancel).
    /// </summary>
    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-summary", true);
    }

    /// <summary>
    /// Navigates to the UI Account Summary page after a successful election.
    /// </summary>
    private void HandleASBack()
    {
        Navigation.NavigateTo("manage-account/account-summary", true);
    }

    /// <summary>
    /// Updates FUTA payroll and requires Calculate again before Submit.
    /// </summary>
    /// <param name="value">The new FUTA payroll amount.</param>
    private void OnFUTAPayrollChanged(decimal? value)
    {
        _response.FUTAPayroll = value ?? 0.0m;
        InvalidateCalculation();
    }

    /// <summary>
    /// Updates WI taxable payroll without COE and requires Calculate again before Submit.
    /// </summary>
    /// <param name="value">The new WI taxable payroll without exclusion.</param>
    private void OnWITaxablePayrollWithoutCOEChanged(decimal? value)
    {
        _response.WITaxablePayrollWithoutCOE = value ?? 0.0m;
        InvalidateCalculation();
    }

    /// <summary>
    /// Updates WI taxable payroll with COE and requires Calculate again before Submit.
    /// </summary>
    /// <param name="value">The new WI taxable payroll with exclusion.</param>
    private void OnWITaxablePayrollWithCOEChanged(decimal? value)
    {
        _response.WITaxablePayrollWithCOE = value ?? 0.0m;
        InvalidateCalculation();
    }

    /// <summary>
    /// Marks the worksheet as needing Calculate after an input change.
    /// </summary>
    private void InvalidateCalculation()
    {
        _isCalculateClicked = false;
        ClearWorksheetErrors();
    }

    /// <summary>
    /// Shows the submit-failed banner when the save response has no warning messages to display.
    /// </summary>
    private void EnsureWarningOrSubmitFailed()
    {
        if (_saveResponse == null)
        {
            _submitFailedError = true;
            return;
        }

        if (_saveResponse.WarningMessage == null || _saveResponse.WarningMessage.Count == 0)
        {
            _submitFailedError = true;
        }
    }
}
