using System.Globalization;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using AccountMaintenance = UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using AccountSummary = UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;

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
    Task<COEReasonableResponse> GetData(int employerSK, int year, int secureUserSK);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    Task<COESaveResponse> Save(AccountMaintenance.CorporateOfficerExclusionRequest saveRequest);

    /// <summary>
    /// 
    /// </summary>
    /// <param name="saveRequest"></param>
    /// <returns></returns>
    Task<COEReasonableResponse> Calculate(AccountSummary.COEReasonablenessRequest saveRequest);

    /// <summary>
    /// Loads an existing Corporate Officer Exclusion election when one is already in effect.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <returns>Summary details when an election is in effect.</returns>
    Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK);

    /// <summary>
    /// Loads EmployerPortalAPIParameters used by the reasonableness worksheet labels
    /// (FUTA threshold/rates and taxable wage base).
    /// </summary>
    /// <returns>Formatted parameter values for Lines 1–4 and 7.</returns>
    Task<COEEmployerPortalApiParameters> GetEmployerPortalApiParametersAsync();

    /// <summary>
    /// Loads the default Corporate Officer Exclusion effective date from SaveNewCOEElection
    /// (<c>CorporateOfficeExclusionEffectiveDate</c>) without completing an election.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <param name="secureUserSK">The signed-in secure user surrogate key.</param>
    /// <param name="year">The estimate / election year sent on the request.</param>
    /// <returns>The service effective date when available; otherwise null.</returns>
    Task<DateTime?> GetDefaultEffectiveDateAsync(int employerSK, int secureUserSK, int year);
}

/// <summary>
/// 
/// </summary>
public class CorporateOfficerExclusionService : ICorporateOfficerExclusionService
{
    private const string TaxableWageBaseParameterName = "TaxableWageBase";
    private const string PayrollSubjectFutaParameterName = "PayrollSubjectFUTACOEReasonableness";
    private const string FutaTaxRateParameterName = "FUTATaxRate";
    private const string MaxFutaCreditAvailableRateParameterName = "MaxFUTACreditAvailableRate";

    private readonly AccountSummary.IAccountSummaryService _accountSummaryService;
    private readonly AccountMaintenance.IAccountMaintenanceService _accountMaintenanceService;
    private readonly IPortalUtilityService _portalUtilityService;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="accountSummaryService"></param>
    /// <param name="accountMaintenanceService"></param>
    /// <param name="portalUtilityService"></param>
    public CorporateOfficerExclusionService(
        AccountSummary.IAccountSummaryService accountSummaryService,
        AccountMaintenance.IAccountMaintenanceService accountMaintenanceService,
        IPortalUtilityService portalUtilityService)
    {
        _accountSummaryService = accountSummaryService;
        _accountMaintenanceService = accountMaintenanceService;
        _portalUtilityService = portalUtilityService;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="employerSK"></param>
    /// <param name="year"></param>
    /// <param name="secureUserSK"></param>
    /// <returns></returns>
    public async Task<COEReasonableResponse> GetData(int employerSK, int year, int secureUserSK)
    {
        var coeResonableResponse = new COEReasonableResponse();
        var req = new AccountSummary.COEReasonablenessRequest();
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
    public async Task<COEReasonableResponse> Calculate(AccountSummary.COEReasonablenessRequest saveRequest)
    {
        var coeResonableResponse = new COEReasonableResponse();
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
                reqResponse.WarningMessage = res.RuleViolations.Select(x => x.RuleViolation).ToList();
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
    /// <returns>Summary details when an election is in effect.</returns>
    public async Task<COEElectionSummary> GetExistingElectionAsync(int employerSK, int secureUserSK)
    {
        var request = new AccountSummary.EmployerRequest
        {
            EmployerSK = employerSK,
            SecureUserSK = secureUserSK
        };

        var emptySummary = new COEElectionSummary
        {
            IsAlreadyElected = false
        };

        var response = await _accountSummaryService.GetCorporateOfficerExclusionsAsync(request);
        if (response?.CorporateOfficerExclusionProxies == null)
        {
            return emptySummary;
        }

        AccountSummary.CorporateOfficerExclusionProxy selectedElection = null!;
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

            if (GetElectionSortDate(proxy) > GetElectionSortDate(selectedElection))
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
    private static bool IsElectionInEffect(AccountSummary.CorporateOfficerExclusionProxy proxy)
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
    private static DateTime GetElectionSortDate(AccountSummary.CorporateOfficerExclusionProxy proxy)
    {
        return proxy.ReceivedDate
            ?? proxy.RequestDate
            ?? proxy.DecisionDate
            ?? proxy.EffectiveDate
            ?? DateTime.MinValue;
    }

    /// <inheritdoc />
    public async Task<COEEmployerPortalApiParameters> GetEmployerPortalApiParametersAsync()
    {
        var parametersByName = await LoadEmployerPortalApiParametersAsync();

        var result = new COEEmployerPortalApiParameters();

        result.PayrollSubjectFutaFormatted = FormatCurrencyParameter(
            GetParameterValue(parametersByName, PayrollSubjectFutaParameterName));
        result.TaxableWageBaseFormatted = FormatCurrencyParameter(
            GetParameterValue(parametersByName, TaxableWageBaseParameterName));

        var futaTaxRate = ParseDecimalParameter(GetParameterValue(parametersByName, FutaTaxRateParameterName));
        result.FutaTaxRate = futaTaxRate;
        result.FutaTaxRateFormatted = FormatRateParameter(futaTaxRate, GetParameterValue(parametersByName, FutaTaxRateParameterName));

        var maxFutaCreditRate = ParseDecimalParameter(
            GetParameterValue(parametersByName, MaxFutaCreditAvailableRateParameterName));
        result.MaxFutaCreditAvailableRate = maxFutaCreditRate;
        result.MaxFutaCreditAvailableRateFormatted = FormatRateParameter(
            maxFutaCreditRate,
            GetParameterValue(parametersByName, MaxFutaCreditAvailableRateParameterName));

        return result;
    }

    /// <summary>
    /// Loads EmployerPortalAPIParameters / system parameters keyed by parameter name.
    /// </summary>
    /// <returns>Case-insensitive parameter name to value map.</returns>
    private async Task<Dictionary<string, string>> LoadEmployerPortalApiParametersAsync()
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var all = await _portalUtilityService.GetAllSystemParametersAsync();
        if (all?.SystemParameters != null)
        {
            foreach (var parameter in all.SystemParameters)
            {
                if (parameter == null || string.IsNullOrWhiteSpace(parameter.ParameterName))
                {
                    continue;
                }

                map[parameter.ParameterName] = parameter.ParameterValue ?? string.Empty;
            }
        }

        await EnsureParameterAsync(map, PayrollSubjectFutaParameterName);
        await EnsureParameterAsync(map, FutaTaxRateParameterName);
        await EnsureParameterAsync(map, MaxFutaCreditAvailableRateParameterName);
        await EnsureParameterAsync(map, TaxableWageBaseParameterName);

        return map;
    }

    /// <summary>
    /// Fetches a single missing parameter by name when it was not present in the bulk response.
    /// </summary>
    /// <param name="map">Parameter map to update.</param>
    /// <param name="parameterName">The EmployerPortalAPIParameters name.</param>
    private async Task EnsureParameterAsync(Dictionary<string, string> map, string parameterName)
    {
        if (map.ContainsKey(parameterName) && !string.IsNullOrWhiteSpace(map[parameterName]))
        {
            return;
        }

        var direct = await _portalUtilityService.GetSystemParameterAsync(parameterName);
        var value = direct?.SystemParameter?.ParameterValue;
        if (!string.IsNullOrWhiteSpace(value))
        {
            map[parameterName] = value;
        }
    }

    /// <summary>
    /// Gets a parameter value from the loaded map.
    /// </summary>
    /// <param name="map">Parameter map.</param>
    /// <param name="parameterName">Parameter name.</param>
    /// <returns>Raw parameter value, or empty string.</returns>
    private static string GetParameterValue(Dictionary<string, string> map, string parameterName)
    {
        return map.TryGetValue(parameterName, out var value) ? value : string.Empty;
    }

    /// <summary>
    /// Formats a wage-threshold parameter as currency when numeric.
    /// </summary>
    /// <param name="rawValue">Raw parameter value.</param>
    /// <returns>Currency string, raw value, or empty string.</returns>
    private static string FormatCurrencyParameter(string rawValue)
    {
        return string.IsNullOrWhiteSpace(rawValue)
            ? string.Empty
            : TryParseDecimal(rawValue, out var amount) ? amount.ToString("C") : rawValue;
    }

    /// <summary>
    /// Formats a rate for worksheet labels as percent text (e.g. 6.000).
    /// Fractional values like 0.06 are shown as 6.000; values already in percent form are unchanged.
    /// </summary>
    private static string FormatRateParameter(decimal? parsedRate, string rawValue)
    {
        if (!parsedRate.HasValue)
        {
            return rawValue ?? string.Empty;
        }

        var rate = parsedRate.Value is > 0m and < 1m
            ? parsedRate.Value * 100m
            : parsedRate.Value;

        return rate.ToString("0.000", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Parses a decimal parameter value.
    /// </summary>
    /// <param name="rawValue">Raw parameter value.</param>
    /// <returns>Parsed decimal, or null.</returns>
    private static decimal? ParseDecimalParameter(string rawValue)
    {
        return TryParseDecimal(rawValue, out var value) ? value : null;
    }

    /// <summary>
    /// Attempts to parse a decimal using invariant then current culture.
    /// </summary>
    /// <param name="rawValue">Raw string value.</param>
    /// <param name="value">Parsed value when successful.</param>
    /// <returns>True when parsing succeeds.</returns>
    private static bool TryParseDecimal(string rawValue, out decimal value)
    {
        return decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.InvariantCulture, out value)
            || decimal.TryParse(rawValue, NumberStyles.Any, CultureInfo.CurrentCulture, out value);
    }

    /// <inheritdoc />
    public async Task<DateTime?> GetDefaultEffectiveDateAsync(int employerSK, int secureUserSK, int year)
    {
        // Team guidance: intro/estimate default date comes from SaveNewCOEElection
        // (CorporateOfficeExclusionEffectiveDate). Call with IsCalculateClicked=false so the
        // service can return the computed default without completing an election.
        var request = new AccountMaintenance.CorporateOfficerExclusionRequest
        {
            EmployerSK = employerSK,
            SecureUserSK = secureUserSK,
            Year = year,
            NetSavings = 0m,
            IsCalculateClicked = false,
            IsNoSavingsShown = false
        };

        var res = await _accountMaintenanceService.SaveNewCOEElectionAsync(request);
        return res == null || res.Success || res.CorporateOfficeExclusionEffectiveDate.Year < 2000
            ? null
            : res.CorporateOfficeExclusionEffectiveDate;
    }
}
