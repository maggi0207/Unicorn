using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// 
/// </summary>
public interface ICorporateOfficerExclusionService
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="employerSK"></param>
    /// <param name="year"></param>
    /// <param name="secureUserSK"></param>
    /// <returns></returns>
    Task<COEResonableResponse> GetData(int employerSK, int year, int secureUserSK);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    Task<COESaveResponse> Save(CorporateOfficerExclusionRequest saveRequest);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    Task<COEResonableResponse> Recalculate(COEReasonablenessRequest saveRequest);

    /// <summary>
    /// Loads an existing Corporate Officer Exclusion election when one is already in effect.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>Summary details when an election is in effect.</returns>
    Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK);
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

            coeResonableResponse.FUTAPayroll = (response.COEReasonablenessProxy.FUTAPayroll.HasValue && response.COEReasonablenessProxy.FUTAPayroll.Value != 0) ?
                                                                   response.COEReasonablenessProxy.FUTAPayroll.Value : null;

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

            coeResonableResponse.WITaxablePayrollWithCOE = (response.COEReasonablenessProxy.WITaxablePayrollWithCOE.HasValue && response.COEReasonablenessProxy.WITaxablePayrollWithCOE.Value != 0) ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithCOE.Value : null;

            coeResonableResponse.WITaxablePayrollWithoutCOE = (response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.HasValue && response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.Value != 0) ?
                                                                  response.COEReasonablenessProxy.WITaxablePayrollWithoutCOE.Value : null;

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

            if (res.RuleViolations.Length > 0)
            {
                reqResponse.WarningMessage = res.RuleViolations.Select(x => x.RuleViolation).ToList();

            }
            else
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
    /// <returns>Summary details when an election is in effect.</returns>
    public async Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK)
    {
        var request = new EmployerRequest
        {
            EmployerSK = employerSK,
            SecureUserSK = secureUserSK
        };

        var emptySummary = new COEElectionSummary
        {
            IsAlreadyElected = false
        };

        var response = await _accountSummaryService.GetCorporateOfficerExclusionsAsync(request);
        if (response == null || response.CorporateOfficerExclusionProxies == null)
        {
            return emptySummary;
        }

        var selectedElection = new CorporateOfficerExclusionProxy();
        var hasSelectedElection = false;

        foreach (var proxy in response.CorporateOfficerExclusionProxies)
        {
            if (proxy == null || !IsElectionInEffect(proxy))
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

        if (!hasSelectedElection)
        {
            return emptySummary;
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
    private static bool IsElectionInEffect(CorporateOfficerExclusionProxy proxy)
    {
        var status = proxy?.COEStatusDescription ?? string.Empty;
        return proxy != null
            && proxy.CorporateOfficerExclusionSK.HasValue
            && status.IndexOf("suspend", StringComparison.OrdinalIgnoreCase) < 0
            && (proxy.EffectiveDate.HasValue
                || status.IndexOf("elect", StringComparison.OrdinalIgnoreCase) >= 0
                || status.IndexOf("effect", StringComparison.OrdinalIgnoreCase) >= 0);
    }

    /// <summary>
    /// Gets the date used to choose the most recent election proxy.
    /// </summary>
    /// <param name="proxy">The corporate officer exclusion proxy.</param>
    /// <returns>The best available date for sorting.</returns>
    private static DateTime GetElectionSortDate(CorporateOfficerExclusionProxy proxy)
    {
        return proxy.ReceivedDate
            ?? proxy.RequestDate
            ?? proxy.DecisionDate
            ?? proxy.EffectiveDate
            ?? DateTime.MinValue;
    }
}
