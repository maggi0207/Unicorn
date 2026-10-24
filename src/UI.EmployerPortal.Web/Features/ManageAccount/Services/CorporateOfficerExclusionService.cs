using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

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
    Task<COESaveResponse> Save(CorporateOfficerExclusionRequest saveRequest);

    /// <summary>
    /// Recalculates the reasonableness worksheet from the provided payroll inputs.
    /// </summary>
    /// <param name="saveRequest">The recalculation request.</param>
    /// <returns>The mapped reasonableness worksheet response.</returns>
    Task<COEResonableResponse> Recalculate(COEReasonablenessRequest saveRequest);

    /// <summary>
    /// Loads an existing Corporate Officer Exclusion election when one is already in effect.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>Summary details when an election is in effect; otherwise null.</returns>
    Task<COEElectionSummary?> GetExistingElectionAsync(int employerSK, int secureUserSK);
}

/// <summary>
/// 
/// </summary>
public class CorporateOfficerExclusionService : ICorporateOfficerExclusionService
{

    private readonly IAccountSummaryService _accountSummaryService;
    private readonly IAccountMaintenanceService _accountMaintenanceService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountSummaryService"></param>
    /// <param name="accountMaintenanceService"></param>
    public CorporateOfficerExclusionService(IAccountSummaryService accountSummaryService, IAccountMaintenanceService accountMaintenanceService)
    {
        _accountSummaryService = accountSummaryService;
        _accountMaintenanceService = accountMaintenanceService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="employerSK"></param>
    /// <param name="year"></param>
    /// <param name="secureUserSK"></param>
    /// <returns></returns>
    public async Task<COEResonableResponse> GetData(int employerSK, int year, int secureUserSK)
    {
        var coeResonableResponse = new COEResonableResponse();
        var req = new COEReasonablenessRequest();
        req.year = year;
        req.EmployerSK = employerSK;
        req.SecureUserSK = secureUserSK;

        var response = await _accountSummaryService.GetCOEReasonablenessAsync(req);

        if (response != null && response.COEReasonablenessProxy != null)
        {
            coeResonableResponse.FUTACreditWithCOE = response.COEReasonablenessProxy.FUTACreditWithCOE.HasValue ?
                                                                    response.COEReasonablenessProxy.FUTACreditWithCOE.Value : 0;

            coeResonableResponse.FUTACreditWithoutCOE = response.COEReasonablenessProxy.FUTACreditWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTACreditWithoutCOE.Value : 0;

            coeResonableResponse.FUTAPayroll = response.COEReasonablenessProxy.FUTAPayroll.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTAPayroll.Value : 0;

            coeResonableResponse.FUTATax = response.COEReasonablenessProxy.FUTATax.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTATax.Value : 0;

            coeResonableResponse.MaxFUTACreditAvailable = response.COEReasonablenessProxy.MaxFUTACreditAvble.HasValue ?
                                                                   response.COEReasonablenessProxy.MaxFUTACreditAvble.Value : 0;

            coeResonableResponse.NetFUTATaxWithCOE = response.COEReasonablenessProxy.NetFUTATaxWithCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.NetFUTATaxWithCOE.Value : 0;

            coeResonableResponse.NetFUTATaxWithoutCOE = response.COEReasonablenessProxy.NetFUTATaxWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.NetFUTATaxWithoutCOE.Value : 0;

            coeResonableResponse.NetSavings = response.COEReasonablenessProxy.NetSavings.HasValue ?
                                                                   response.COEReasonablenessProxy.NetSavings.Value : 0;

            coeResonableResponse.TotalFUTACreditWithCOE = response.COEReasonablenessProxy.TotalFUTACreditWithCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.TotalFUTACreditWithCOE.Value : 0;

            coeResonableResponse.TotalFUTACreditWithoutCOE = response.COEReasonablenessProxy.TotalFUTACreditWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.TotalFUTACreditWithoutCOE.Value : 0;

            coeResonableResponse.TotalUITaxWithCOE = response.COEReasonablenessProxy.TotalUITaxWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.TotalUITaxWithCOE.Value : 0;

            coeResonableResponse.TotalUITaxWithoutCOE = response.COEReasonablenessProxy.TotalUITaxWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.TotalUITaxWithoutCOE.Value : 0;

            coeResonableResponse.WITaxablePayrollWithCOE = response.COEReasonablenessProxy.WITaxablePayrollWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithCOE.Value : 0;

            coeResonableResponse.WITaxablePayrollWithoutCOE = response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.Value : 0;

            coeResonableResponse.WIUITaxRate = response.COEReasonablenessProxy.WIUITaxRatePercentage.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxRatePercentage.Value : 0;

            coeResonableResponse.WIUITaxRatePercentage = response.COEReasonablenessProxy.WIUITaxRatePercentage.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxRatePercentage.Value : 0;
            coeResonableResponse.WIUITaxWithCOE = response.COEReasonablenessProxy.WIUITaxWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxWithCOE.Value : 0;

            coeResonableResponse.WIUITaxWithoutCOE = response.COEReasonablenessProxy.WIUITaxWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxWithoutCOE.Value : 0;
        }

        return coeResonableResponse;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    public async Task<COEResonableResponse> Recalculate(COEReasonablenessRequest saveRequest)
    {
        var coeResonableResponse = new COEResonableResponse();
        var response = await _accountSummaryService.GetCOEReasonablenessAsync(saveRequest);

        if (response != null && response.COEReasonablenessProxy != null)
        {
            coeResonableResponse.FUTACreditWithCOE = response.COEReasonablenessProxy.FUTACreditWithCOE.HasValue ?
                                                                    response.COEReasonablenessProxy.FUTACreditWithCOE.Value : 0;

            coeResonableResponse.FUTACreditWithoutCOE = response.COEReasonablenessProxy.FUTACreditWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTACreditWithoutCOE.Value : 0;

            coeResonableResponse.FUTAPayroll = response.COEReasonablenessProxy.FUTAPayroll.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTAPayroll.Value : 0;

            coeResonableResponse.FUTATax = response.COEReasonablenessProxy.FUTATax.HasValue ?
                                                                   response.COEReasonablenessProxy.FUTATax.Value : 0;

            coeResonableResponse.MaxFUTACreditAvailable = response.COEReasonablenessProxy.MaxFUTACreditAvble.HasValue ?
                                                                   response.COEReasonablenessProxy.MaxFUTACreditAvble.Value : 0;

            coeResonableResponse.NetFUTATaxWithCOE = response.COEReasonablenessProxy.NetFUTATaxWithCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.NetFUTATaxWithCOE.Value : 0;

            coeResonableResponse.NetFUTATaxWithoutCOE = response.COEReasonablenessProxy.NetFUTATaxWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.NetFUTATaxWithoutCOE.Value : 0;

            coeResonableResponse.NetSavings = response.COEReasonablenessProxy.NetSavings.HasValue ?
                                                                   response.COEReasonablenessProxy.NetSavings.Value : 0;

            coeResonableResponse.TotalFUTACreditWithCOE = response.COEReasonablenessProxy.TotalFUTACreditWithCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.TotalFUTACreditWithCOE.Value : 0;

            coeResonableResponse.TotalFUTACreditWithoutCOE = response.COEReasonablenessProxy.TotalFUTACreditWithoutCOE.HasValue ?
                                                                   response.COEReasonablenessProxy.TotalFUTACreditWithoutCOE.Value : 0;

            coeResonableResponse.TotalUITaxWithCOE = response.COEReasonablenessProxy.TotalUITaxWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.TotalUITaxWithCOE.Value : 0;

            coeResonableResponse.TotalUITaxWithoutCOE = response.COEReasonablenessProxy.TotalUITaxWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.TotalUITaxWithoutCOE.Value : 0;

            coeResonableResponse.WITaxablePayrollWithCOE = response.COEReasonablenessProxy.WITaxablePayrollWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithCOE.Value : 0;

            coeResonableResponse.WITaxablePayrollWithoutCOE = response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.Value : 0;

            coeResonableResponse.WIUITaxRate = response.COEReasonablenessProxy.WIUITaxRatePercentage.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxRatePercentage.Value : 0;

            coeResonableResponse.WIUITaxRatePercentage = response.COEReasonablenessProxy.WIUITaxRatePercentage.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxRatePercentage.Value : 0;
            coeResonableResponse.WIUITaxWithCOE = response.COEReasonablenessProxy.WIUITaxWithCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxWithCOE.Value : 0;

            coeResonableResponse.WIUITaxWithoutCOE = response.COEReasonablenessProxy.WIUITaxWithoutCOE.HasValue ?
                                                                  response.COEReasonablenessProxy.WIUITaxWithoutCOE.Value : 0;
        }

        return coeResonableResponse;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    public async Task<COESaveResponse> Save(CorporateOfficerExclusionRequest saveRequest)
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
    public async Task<COEElectionSummary?> GetExistingElectionAsync(int employerSK, int secureUserSK)
    {
        var request = new EmployerRequest
        {
            EmployerSK = employerSK,
            SecureUserSK = secureUserSK
        };

        var response = await _accountSummaryService.GetCorporateOfficerExclusionsAsync(request);
        if (response == null || response.CorporateOfficerExclusionProxies == null)
        {
            return null;
        }

        CorporateOfficerExclusionProxy? selectedElection = null;

        foreach (var proxy in response.CorporateOfficerExclusionProxies)
        {
            if (!IsElectionInEffect(proxy))
            {
                continue;
            }

            if (selectedElection == null)
            {
                selectedElection = proxy;
                continue;
            }

            var selectedSortDate = GetElectionSortDate(selectedElection);
            var proxySortDate = GetElectionSortDate(proxy);
            if (proxySortDate > selectedSortDate)
            {
                selectedElection = proxy;
            }
        }

        if (selectedElection == null)
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
    /// Determines whether the exclusion proxy represents an election that is currently in effect.
    /// </summary>
    /// <param name="proxy">The corporate officer exclusion proxy.</param>
    /// <returns>True when the election should block a new worksheet submission.</returns>
    private static bool IsElectionInEffect(CorporateOfficerExclusionProxy? proxy)
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
    private static DateTime GetElectionSortDate(CorporateOfficerExclusionProxy proxy)
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
