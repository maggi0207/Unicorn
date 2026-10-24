using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using AccountMaintenance = UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using AccountSummary = UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Service contract for Corporate Officer Exclusion worksheet and election operations.
/// </summary>
public interface ICorporateOfficerExclusionService
{
    /// <summary>
    /// Loads the reasonableness worksheet values for the employer and year.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="year">The tax year for the worksheet.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>The mapped reasonableness worksheet response.</returns>
    Task<COEResonableResponse> GetData(int employerSK, int year, int secureUserSK);

    /// <summary>
    /// Saves a new Corporate Officer Exclusion election.
    /// </summary>
    /// <param name="saveRequest">The election save request.</param>
    /// <returns>The mapped save response.</returns>
    Task<COESaveResponse> Save(AccountMaintenance.CorporateOfficerExclusionRequest saveRequest);

    /// <summary>
    /// Recalculates the reasonableness worksheet from the provided payroll inputs.
    /// </summary>
    /// <param name="saveRequest">The recalculation request.</param>
    /// <returns>The mapped reasonableness worksheet response.</returns>
    Task<COEResonableResponse> Recalculate(AccountSummary.COEReasonablenessRequest saveRequest);

    /// <summary>
    /// Loads an existing Corporate Officer Exclusion election when one is already in effect.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>Summary details when an election is in effect; otherwise null.</returns>
    Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK);
}

/// <summary>
/// Corporate Officer Exclusion worksheet and election service implementation.
/// </summary>
public class CorporateOfficerExclusionService : ICorporateOfficerExclusionService
{
    private readonly AccountSummary.IAccountSummaryService _accountSummaryService;
    private readonly AccountMaintenance.IAccountMaintenanceService _accountMaintenanceService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CorporateOfficerExclusionService"/> class.
    /// </summary>
    /// <param name="accountSummaryService">Account summary WCF client.</param>
    /// <param name="accountMaintenanceService">Account maintenance WCF client.</param>
    public CorporateOfficerExclusionService(
        AccountSummary.IAccountSummaryService accountSummaryService,
        AccountMaintenance.IAccountMaintenanceService accountMaintenanceService)
    {
        _accountSummaryService = accountSummaryService;
        _accountMaintenanceService = accountMaintenanceService;
    }

    /// <summary>
    /// Loads the reasonableness worksheet values for the employer and year.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="year">The tax year for the worksheet.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>The mapped reasonableness worksheet response.</returns>
    public async Task<COEResonableResponse> GetData(int employerSK, int year, int secureUserSK)
    {
        var coeResonableResponse = new COEResonableResponse();
        var req = new AccountSummary.COEReasonablenessRequest();
        req.year = year;
        req.EmployerSK = employerSK;
        req.SecureUserSK = secureUserSK;

        var response = await _accountSummaryService.GetCOEReasonablenessAsync(req);
        MapReasonablenessProxy(response, coeResonableResponse);
        return coeResonableResponse;
    }

    /// <summary>
    /// Recalculates the reasonableness worksheet from the provided payroll inputs.
    /// </summary>
    /// <param name="saveRequest">The recalculation request.</param>
    /// <returns>The mapped reasonableness worksheet response.</returns>
    public async Task<COEResonableResponse> Recalculate(AccountSummary.COEReasonablenessRequest saveRequest)
    {
        var coeResonableResponse = new COEResonableResponse();
        var response = await _accountSummaryService.GetCOEReasonablenessAsync(saveRequest);
        MapReasonablenessProxy(response, coeResonableResponse);
        return coeResonableResponse;
    }

    /// <summary>
    /// Saves a new Corporate Officer Exclusion election.
    /// </summary>
    /// <param name="saveRequest">The election save request.</param>
    /// <returns>The mapped save response.</returns>
    public async Task<COESaveResponse> Save(AccountMaintenance.CorporateOfficerExclusionRequest saveRequest)
    {
        var reqResponse = new COESaveResponse();
        var res = await _accountMaintenanceService.SaveNewCOEElectionAsync(saveRequest);

        if (res != null)
        {
            reqResponse.Success = res.Success;
            reqResponse.IsNoSavingsShown = res.IsNoSavingsShown;
            reqResponse.AccountNumber = res.AccountNumber;
            reqResponse.UIAccountNumber = res.UIAccountNumber;
            reqResponse.CorporateOfficeExclusionStatus = res.CorporateOfficeExclusionStatus;
            reqResponse.LegalName = res.LegalName;
            reqResponse.CorporateOfficeExclusionEffectiveDate = res.CorporateOfficeExclusionEffectiveDate;

            if (res.RuleViolations != null && res.RuleViolations.Length > 0)
            {
                foreach (var ruleViolation in res.RuleViolations)
                {
                    if (ruleViolation != null && !string.IsNullOrWhiteSpace(ruleViolation.RuleViolation))
                    {
                        reqResponse.WarningMessage.Add(ruleViolation.RuleViolation);
                    }
                }
            }
            else if (!string.IsNullOrWhiteSpace(res.WarningMessage))
            {
                reqResponse.WarningMessage.Add(res.WarningMessage);
            }
        }

        return reqResponse;
    }

    /// <summary>
    /// Loads an existing Corporate Officer Exclusion election when one is already in effect.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>Summary details when an election is in effect; otherwise null.</returns>
    public async Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK)
    {
        var request = new AccountSummary.EmployerRequest
        {
            EmployerSK = employerSK,
            SecureUserSK = secureUserSK
        };

        var response = await _accountSummaryService.GetCorporateOfficerExclusionsAsync(request);
        if (response == null || response.CorporateOfficerExclusionProxies == null)
        {
            return null;
        }

        AccountSummary.CorporateOfficerExclusionProxy selectedElection = null;
        var hasSelectedElection = false;

        foreach (var proxy in response.CorporateOfficerExclusionProxies)
        {
            if (!IsElectionInEffect(proxy))
            {
                continue;
            }

            if (!hasSelectedElection)
            {
                selectedElection = proxy;
                hasSelectedElection = true;
                continue;
            }

            var selectedSortDate = GetElectionSortDate(selectedElection);
            var proxySortDate = GetElectionSortDate(proxy);
            if (proxySortDate > selectedSortDate)
            {
                selectedElection = proxy;
            }
        }

        if (!hasSelectedElection || selectedElection == null)
        {
            return null;
        }

        var summary = new COEElectionSummary
        {
            IsAlreadyElected = true,
            EffectiveDate = selectedElection.EffectiveDate,
            DateSubmitted = selectedElection.ReceivedDate ?? selectedElection.RequestDate ?? selectedElection.DecisionDate
        };

        if (selectedElection.CorporateOfficerExclusionSK.HasValue)
        {
            summary.ConfirmationId = selectedElection.CorporateOfficerExclusionSK.Value.ToString();
        }

        return summary;
    }

    /// <summary>
    /// Maps a reasonableness WCF response onto the UI response model.
    /// </summary>
    /// <param name="response">The WCF reasonableness response.</param>
    /// <param name="coeResonableResponse">The UI response model to populate.</param>
    private static void MapReasonablenessProxy(
        AccountSummary.COEReasonablenessResponse response,
        COEResonableResponse coeResonableResponse)
    {
        if (response == null || response.COEReasonablenessProxy == null)
        {
            return;
        }

        var proxy = response.COEReasonablenessProxy;

        coeResonableResponse.FUTACreditWithCOE = proxy.FUTACreditWithCOE.HasValue ? proxy.FUTACreditWithCOE.Value : 0;
        coeResonableResponse.FUTACreditWithoutCOE = proxy.FUTACreditWithoutCOE.HasValue ? proxy.FUTACreditWithoutCOE.Value : 0;
        coeResonableResponse.FUTAPayroll = proxy.FUTAPayroll.HasValue ? proxy.FUTAPayroll.Value : 0;
        coeResonableResponse.FUTATax = proxy.FUTATax.HasValue ? proxy.FUTATax.Value : 0;
        coeResonableResponse.MaxFUTACreditAvailable = proxy.MaxFUTACreditAvble.HasValue ? proxy.MaxFUTACreditAvble.Value : 0;
        coeResonableResponse.NetFUTATaxWithCOE = proxy.NetFUTATaxWithCOE.HasValue ? proxy.NetFUTATaxWithCOE.Value : 0;
        coeResonableResponse.NetFUTATaxWithoutCOE = proxy.NetFUTATaxWithoutCOE.HasValue ? proxy.NetFUTATaxWithoutCOE.Value : 0;
        coeResonableResponse.NetSavings = proxy.NetSavings.HasValue ? proxy.NetSavings.Value : 0;
        coeResonableResponse.TotalFUTACreditWithCOE = proxy.TotalFUTACreditWithCOE.HasValue ? proxy.TotalFUTACreditWithCOE.Value : 0;
        coeResonableResponse.TotalFUTACreditWithoutCOE = proxy.TotalFUTACreditWithoutCOE.HasValue ? proxy.TotalFUTACreditWithoutCOE.Value : 0;
        coeResonableResponse.TotalUITaxWithCOE = proxy.TotalUITaxWithCOE.HasValue ? proxy.TotalUITaxWithCOE.Value : 0;
        coeResonableResponse.TotalUITaxWithoutCOE = proxy.TotalUITaxWithoutCOE.HasValue ? proxy.TotalUITaxWithoutCOE.Value : 0;
        coeResonableResponse.WITaxablePayrollWithCOE = proxy.WITaxablePayrollWithCOE.HasValue ? proxy.WITaxablePayrollWithCOE.Value : 0;
        coeResonableResponse.WITaxablePayrollWithoutCOE = proxy.WITaxablePayrollWithoutCOE.HasValue ? proxy.WITaxablePayrollWithoutCOE.Value : 0;
        coeResonableResponse.WIUITaxRate = proxy.WIUITaxRatePercentage.HasValue ? proxy.WIUITaxRatePercentage.Value : 0;
        coeResonableResponse.WIUITaxRatePercentage = proxy.WIUITaxRatePercentage.HasValue ? proxy.WIUITaxRatePercentage.Value : 0;
        coeResonableResponse.WIUITaxWithCOE = proxy.WIUITaxWithCOE.HasValue ? proxy.WIUITaxWithCOE.Value : 0;
        coeResonableResponse.WIUITaxWithoutCOE = proxy.WIUITaxWithoutCOE.HasValue ? proxy.WIUITaxWithoutCOE.Value : 0;
    }

    /// <summary>
    /// Determines whether the exclusion proxy represents an election that is currently in effect.
    /// </summary>
    /// <param name="proxy">The corporate officer exclusion proxy.</param>
    /// <returns>True when the election should block a new worksheet submission.</returns>
    private static bool IsElectionInEffect(AccountSummary.CorporateOfficerExclusionProxy proxy)
    {
        if (proxy == null || !proxy.CorporateOfficerExclusionSK.HasValue)
        {
            return false;
        }

        var status = proxy.COEStatusDescription ?? string.Empty;
        if (status.IndexOf("suspend", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return false;
        }

        if (proxy.EffectiveDate.HasValue)
        {
            return true;
        }

        if (status.IndexOf("elect", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        if (status.IndexOf("effect", StringComparison.OrdinalIgnoreCase) >= 0)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Gets the date used to choose the most recent election proxy.
    /// </summary>
    /// <param name="proxy">The corporate officer exclusion proxy.</param>
    /// <returns>The best available date for sorting.</returns>
    private static DateTime GetElectionSortDate(AccountSummary.CorporateOfficerExclusionProxy proxy)
    {
        if (proxy.ReceivedDate.HasValue)
        {
            return proxy.ReceivedDate.Value;
        }

        if (proxy.RequestDate.HasValue)
        {
            return proxy.RequestDate.Value;
        }

        if (proxy.DecisionDate.HasValue)
        {
            return proxy.DecisionDate.Value;
        }

        if (proxy.EffectiveDate.HasValue)
        {
            return proxy.EffectiveDate.Value;
        }

        return DateTime.MinValue;
    }
}
