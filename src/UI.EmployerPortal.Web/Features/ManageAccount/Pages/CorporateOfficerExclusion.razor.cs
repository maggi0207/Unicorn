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
/// Corporate Officer Exclusion intro, reasonableness worksheet, and election confirmation page.
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
    /// Message shown on the already-elected confirmation banner.
    /// </summary>
    private const string SubmitSuccessMessage = "Corporate Officer Exclusion submitted.";

    /// <summary>
    /// Centered success copy shown immediately after a successful election (Figma Success).
    /// </summary>
    private const string ElectionSubmittedMessage = "Your election successfully submitted.";

    /// <summary>
    /// Banner shown after the user requests a mailed Initial Determination copy.
    /// </summary>
    private const string MailCopyBannerMessage = "Initial Determination letter will be mailed to your main business mailing address.";

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
    /// True when Line 1 FUTA payroll (Without Exclusion) is missing.
    /// </summary>
    private bool _futaPayrollMissing = false;

    /// <summary>
    /// True when Line 4 WI payroll without exclusion is missing.
    /// </summary>
    private bool _wiPayrollWithoutMissing = false;

    /// <summary>
    /// Top validation banner messages collected in field layout order.
    /// </summary>
    private List<string> _fieldValidationMessages = [];

    /// <summary>
    /// True while the intro screen is shown before the worksheet.
    /// </summary>
    private bool _showIntro = true;

    /// <summary>
    /// True when the employer already has a Corporate Officer Exclusion in effect.
    /// </summary>
    private bool _isAlreadyElected = false;

    /// <summary>
    /// Account number shown on intro / already-elected / success cards.
    /// </summary>
    private string _accountNumber = string.Empty;

    /// <summary>
    /// Legal name shown on intro / already-elected / success cards.
    /// </summary>
    private string _legalName = string.Empty;

    /// <summary>
    /// Effective date shown on intro / already-elected cards.
    /// </summary>
    private DateTime? _effectiveDate;

    /// <summary>
    /// Confirmation ID when an election already exists or was just saved.
    /// </summary>
    private string _confirmationId = string.Empty;

    /// <summary>
    /// Date submitted when an election already exists or was just saved.
    /// </summary>
    private DateTime? _dateSubmitted;

    /// <summary>
    /// True when the user requested mailing a copy of the Initial Determination letter.
    /// </summary>
    private bool _mailCopyRequested = false;

    /// <summary>
    /// Success banner text for the already-elected confirmation screen.
    /// </summary>
    private string SuccessBannerMessage => SubmitSuccessMessage;

    /// <summary>
    /// Centered success text for a freshly submitted election.
    /// </summary>
    private string ElectionSuccessMessage => ElectionSubmittedMessage;

    /// <summary>
    /// Mail-copy confirmation banner text.
    /// </summary>
    private string MailCopyMessage => MailCopyBannerMessage;

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
        if ((_saveResponse != null && _isSaveSuccess.HasValue && _isSaveSuccess.Value) || _isAlreadyElected)
        {
            await JSRuntime.InvokeVoidAsync("focusElement", "save-heading");
        }
    }

    /// <summary>
    /// Loads account header data, existing election state, and worksheet values.
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

            _accountNumber = FormatUIAccountNumber(selected?.EmployerAccount?.UIAccountNo ?? string.Empty);
            _legalName = selected?.EmployerAccount?.LegalName ?? string.Empty;
            _effectiveDate = new DateTime(DateTime.Now.Year, 1, 1);

            var secureUserSK = UserAccountService.GetUserSKClaim();

            var existingElection = await CorporateOfficerExclusionService.GetExistingElectionAsync(_employerSK, secureUserSK);
            if (existingElection.IsAlreadyElected)
            {
                ApplyExistingElection(existingElection);
                return;
            }

            _showIntro = true;
            _isAlreadyElected = false;
            _response = await CorporateOfficerExclusionService.GetData(_employerSK, DateTime.Now.Year, secureUserSK);
        }
        catch
        {
        }
        finally
        {
        }
    }

    /// <summary>
    /// Applies an existing election summary to the already-elected confirmation view.
    /// </summary>
    /// <param name="existingElection">The existing election summary.</param>
    private void ApplyExistingElection(COEElectionSummary existingElection)
    {
        _isAlreadyElected = true;
        _showIntro = false;
        _isSaveSuccess = true;
        _confirmationId = existingElection.ConfirmationId;
        _dateSubmitted = existingElection.DateSubmitted;

        if (existingElection.EffectiveDate.HasValue)
        {
            _effectiveDate = existingElection.EffectiveDate;
        }

        if (!string.IsNullOrWhiteSpace(existingElection.AccountNumber))
        {
            _accountNumber = FormatUIAccountNumber(existingElection.AccountNumber);
        }

        if (!string.IsNullOrWhiteSpace(existingElection.LegalName))
        {
            _legalName = existingElection.LegalName;
        }

        _saveResponse = new COESaveResponse
        {
            Success = true,
            AccountNumber = _accountNumber,
            LegalName = _legalName,
            CorporateOfficeExclusionEffectiveDate = _effectiveDate,
            ConfirmationId = _confirmationId,
            DateSubmitted = _dateSubmitted
        };
    }

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadData();
    }

    /// <summary>
    /// Advances from the intro screen to the reasonableness worksheet.
    /// </summary>
    private void HandleNext()
    {
        _showIntro = false;
    }

    /// <summary>
    /// Clears worksheet-level error banners before a new calculate or submit attempt.
    /// </summary>
    private void ClearWorksheetErrors()
    {
        _mustCalculateError = false;
        _submitFailedError = false;
        ClearFieldValidationErrors();
    }

    /// <summary>
    /// Clears required-field validation state on the worksheet.
    /// </summary>
    private void ClearFieldValidationErrors()
    {
        _futaPayrollMissing = false;
        _wiPayrollWithoutMissing = false;
        _fieldValidationMessages = [];
    }

    /// <summary>
    /// Validates the required payroll inputs and builds the top error banner list.
    /// FUTA Without and WI UI Without must be greater than zero ($0.00 is treated as missing per Figma).
    /// WI UI With may be zero when all officer wages are excluded.
    /// </summary>
    /// <returns>True when required fields are present.</returns>
    private bool ValidateRequiredPayrollFields()
    {
        ClearFieldValidationErrors();

        if (_response == null)
        {
            _submitFailedError = true;
            return false;
        }

        if (_response.FUTAPayroll <= 0)
        {
            _futaPayrollMissing = true;
            _fieldValidationMessages.Add("Payroll Subject to FUTA missing.");
        }

        if (_response.WITaxablePayrollWithoutCOE <= 0)
        {
            _wiPayrollWithoutMissing = true;
            _fieldValidationMessages.Add("Payroll Subject to WI UI Tax missing.");
        }

        return !_futaPayrollMissing && !_wiPayrollWithoutMissing;
    }

    /// <summary>
    /// Recalculates the reasonableness worksheet from the current payroll inputs.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task HandleRecalculateAsync()
    {
        ClearWorksheetErrors();

        if (!ValidateRequiredPayrollFields())
        {
            _isCalculateClicked = false;
            return;
        }

        _recalculateStatus = "Calculating";
        _isRecalculating = true;
        _isSaveSuccess = null;
        _saveResponse = null;

        try
        {
            var selectedClient = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();

            if (selectedClient == null)
            {
                _submitFailedError = true;
                return;
            }

            var employerSk = selectedClient.Id;

            if (employerSk == 0)
            {
                _submitFailedError = true;
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

        if (!ValidateRequiredPayrollFields())
        {
            _isSaveSuccess = false;
            return;
        }

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
                EnsureWarningOrSubmitFailed();
                return;
            }

            if (!string.IsNullOrWhiteSpace(_saveResponse.AccountNumber))
            {
                _accountNumber = FormatUIAccountNumber(_saveResponse.AccountNumber);
            }

            if (!string.IsNullOrWhiteSpace(_saveResponse.LegalName))
            {
                _legalName = _saveResponse.LegalName;
            }

            if (_saveResponse.CorporateOfficeExclusionEffectiveDate.HasValue)
            {
                _effectiveDate = _saveResponse.CorporateOfficeExclusionEffectiveDate;
            }

            await EnrichSuccessFromExistingElectionAsync(selectedClient.Id, secureUserSK);
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
    /// Loads confirmation id and submitted date after a successful save when available from exclusions.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    private async Task EnrichSuccessFromExistingElectionAsync(int employerSK, int secureUserSK)
    {
        try
        {
            var existingElection = await CorporateOfficerExclusionService.GetExistingElectionAsync(employerSK, secureUserSK);
            if (!existingElection.IsAlreadyElected)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(existingElection.ConfirmationId))
            {
                _confirmationId = existingElection.ConfirmationId;
                if (_saveResponse != null)
                {
                    _saveResponse.ConfirmationId = existingElection.ConfirmationId;
                }
            }

            if (existingElection.DateSubmitted.HasValue)
            {
                _dateSubmitted = existingElection.DateSubmitted;
                if (_saveResponse != null)
                {
                    _saveResponse.DateSubmitted = existingElection.DateSubmitted;
                }
            }

            if (existingElection.EffectiveDate.HasValue)
            {
                _effectiveDate = existingElection.EffectiveDate;
                if (_saveResponse != null)
                {
                    _saveResponse.CorporateOfficeExclusionEffectiveDate = existingElection.EffectiveDate;
                }
            }
        }
        catch
        {
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
    /// Opens UI Documents so the employer can view the Initial Determination letter when available.
    /// </summary>
    private void HandleViewInitialDetermination()
    {
        Navigation.NavigateTo("ui-documents", true);
    }

    /// <summary>
    /// Records that the employer requested a mailed copy of the Initial Determination letter.
    /// </summary>
    private void HandleRequestMailCopy()
    {
        _mailCopyRequested = true;
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

    /// <summary>
    /// Formats a UI account number as ######-###-# when 10 digits are present.
    /// </summary>
    /// <param name="accountNo">The raw or partially formatted account number.</param>
    /// <returns>The formatted account number, or the original value when it cannot be formatted.</returns>
    private static string FormatUIAccountNumber(string accountNo)
    {
        if (string.IsNullOrWhiteSpace(accountNo))
        {
            return string.Empty;
        }

        var digits = accountNo.Replace("-", string.Empty);
        if (digits.Length == 10)
        {
            return $"{digits[..6]}-{digits[6..9]}-{digits[9..]}";
        }

        return accountNo;
    }
}
