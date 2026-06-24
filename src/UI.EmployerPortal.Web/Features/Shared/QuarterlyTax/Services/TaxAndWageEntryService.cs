using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

using SummaryReportProxy = UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService.SummaryReportProxy;
using WageReportProxy = UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService.WageReportProxy;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

/// <summary>
/// Inteface to retrieving tax and wage entry report data.
/// </summary>
public interface ITaxAndWageEntryService
{
    /// <summary>
    /// Retrieves all missing quarterly reports for selected employer.
    /// </summary>
    /// <returns></returns>
    Task<MissingWageAndTaxReportsData> GetMissingWageAndTaxReports();

    /// <summary>
    /// Retrieves the quarterly tax and wage entry report data. 
    /// </summary>
    /// <returns>Teh report model containing employee wage data for the quarter.</returns>
    Task<TaxAndWageEntryReportModel> GetTaxAndWageEntryReportDataAsync(long? wageTaxFilingSk, MissingReportModel? missingReportModel);

    /// <summary>
    /// Employer account has available credit.
    /// </summary>
    /// <returns></returns>
    Task<decimal> GetAvailableCredit();

    /// <summary>
    /// Get available filing method.
    /// </summary>
    /// <returns></returns>
    Task<QuarterlyReportSelectionMethod?> GetAvailableFilingMethod(MissingReportModel missingReportModel, TaxWageFilingType reportFilingType);

    /// <summary>
    ///  Get available filing methods based on hard coded values
    /// </summary>
    /// <param name="filingType"></param>
    /// <returns></returns>
    Task<QuarterlyReportSelectionMethod> GetAvailableTaxAndReportFilingMethod(TaxWageFilingType filingType);

    /// <summary>
    /// Get available filing methods.
    /// </summary>
    /// <returns></returns>
    Task<List<QuarterlyReportSelectionMethod>> GetAvailableFilingMethods(MissingReportModel missingReportModel);

    /// <summary>
    /// Get tax rate for selected employer and year.
    /// </summary>
    /// <param name="year"></param>
    /// <returns></returns>
    Task<TaxRate> GetTaxRateForYear(int year);

    /// <summary>
    /// Get tax rate for all years for selected employer.
    /// </summary>
    /// <returns></returns>
    Task<List<TaxRate>> GetAllTaxRateForEmployer();

    /// <summary>
    /// Validates and submits a tax-only filing, returning the confirmation number.
    /// </summary>
    Task<WageTaxFilingResponse> SubmitTaxReportAsync(WageTaxFilingProxy filing);

    /// <summary>
    /// GetCalculatedExclusionAmountAsync
    /// </summary>
    /// <param name="missingReportModel"></param>
    /// <param name="employees"></param>
    /// <returns></returns>
    Task<decimal> GetCalculatedExclusionAmountAsync(MissingReportModel missingReportModel, List<CalculateWageTaxFilingEmployeeProxy> employees);

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<bool> GetOutOfBalanceAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<List<WageReportProxy>> GetOutOfBalanceWageReportsAsync();

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    Task<List<SummaryReportProxy>> GetOutOfBalanceSummaryReportsAsync();

    /// <summary>
    /// Get tax report by year and quarter
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    Task<TaxEntryModel> GetTaxReportByYearAndQuarterAsync(int year, int quarter);

    /// <summary>
    /// Save the current report as pending draft without submitting it. 
    /// </summary>
    /// <param name="filing"></param>
    /// <returns></returns>
    Task<SaveWageTaxFilingResponse> SaveTaxReportAsync(WageTaxFilingProxy filing);

    /// <summary>
    /// Retrieves all pending tax/wage filings for the current employer. 
    /// </summary>
    /// <returns></returns>
    Task<WageTaxFilingPendingEmployerResponse> GetPendingTaxWagesForEmployerAsync();

    /// <summary>
    /// Retrieves pending wage tax report by WageTaxFilingSK
    /// </summary>
    /// <param name="wageTaxFilingSK"></param>
    /// <returns></returns>
    Task<WageTaxFilingProxy?> GetPendingTaxReportByWageTaxFilingSK(long wageTaxFilingSK);

    /// <summary>
    /// Delete report saved previously using save and quit function.
    /// </summary>
    /// <param name="wageTaxFilingSK"></param>
    /// <returns></returns>
    Task<WageTaxFilingDeletedResponse> DeletePendingTaxReportByWageTaxFilingSK(int wageTaxFilingSK);

    /// <summary>
    /// Get wage report by quarter year.
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    Task<WageReportProxy?> GetWageReportByQuarterYear(int year, int quarter);

    /// <summary>
    /// Retrieves all previously filed wage and tax report summaries for the selected employer.
    /// </summary>
    Task<List<PreviouslyFiledReportModel>> GetPreviouslyFiledReportsAsync();

    /// <summary>
    /// Retrieves the full tax and wage details for a previously filed report for the given quarter and year.
    /// </summary>
    Task<PreviouslyFiledReportDetailsModel> GetPreviouslyFiledReportDetailsAsync(int quarter, int year);

    /// <summary>
    /// CheckDeferralEligibilityAsync
    /// </summary>
    /// <param name="employerSk"></param>
    /// <param name="deferralYear"></param>
    /// <returns></returns>
    Task<DeferralEligibilityResponse> CheckDeferralEligibilityAsync(int employerSk, int deferralYear);

    /// <summary>
    /// ElectFirstQuarterDeferralAsync
    /// </summary>
    /// <param name="commonClientSk"></param>
    /// <param name="deferralYear"></param>
    /// <param name="electionTime"></param>
    /// <returns></returns>
    Task<DeferralElectionResponse> ElectFirstQuarterDeferralAsync(int commonClientSk, int deferralYear, DateTime electionTime);

}

/// <summary>
/// Service that provides tax and wage entry report data.
/// </summary>
internal class TaxAndWageEntryService : ITaxAndWageEntryService
{
    private readonly ITaxWageReportingService _taxWageReportingService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly IAccountSummaryService _accountSummaryService;
    private readonly IAsyncRetryPolicy<TaxAndWageEntryService> _retryPolicy;
    private readonly ISessionManager _sessionManager;
    private readonly string _genericErrorMessage;

    public TaxAndWageEntryService(
           ITaxWageReportingService taxWageReportingService,
           IUserAccountService userAccountService,
           IAccountMaintenanceService accountMaintenanceService,
           IAccountSummaryService accountSummaryService,
           IAsyncRetryPolicy<TaxAndWageEntryService> retryPolicy,
           ISessionManager sessionManager,
           IConfiguration configuration)
    {
        _taxWageReportingService = taxWageReportingService;
        _userAccountService = userAccountService;
        _accountMaintenanceService = accountMaintenanceService;
        _accountSummaryService = accountSummaryService;
        _retryPolicy = retryPolicy;
        _sessionManager = sessionManager;
        _genericErrorMessage = configuration["Messages:TechnicalDifficulties"]
            ?? "We are currently experiencing technical difficulties. Please try again later.";
    }

    /// <inheritdoc />
    public async Task<MissingWageAndTaxReportsData> GetMissingWageAndTaxReports()
    {
        MissingWageAndTaxReportsData missingWageAndTaxReportsData = new();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            var missingReportsResponse = await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetMissingQuartersAsync(selectedEmployer.EmployerAccount!.Id, _userAccountService.GetUserSKClaim());
            });

            if (missingReportsResponse != null)
            {
                foreach (var quarter in missingReportsResponse.MissingReports)
                {
                    missingWageAndTaxReportsData.MissingReports.Add(new MissingReportModel
                    {
                        FormattedQuarterYear = quarter.FormattedQuarterYear,
                        Quarter = quarter.Quarter,
                        Year = quarter.Year,
                        Description = quarter.Description,
                        IsLate = quarter.isLate,
                        SelectValue = quarter.SelectValue,
                        ReportName = quarter.ReportName,
                        DueDate = quarter.DueDate,
                        Status = quarter.Status,
                    });
                }

                missingWageAndTaxReportsData.HasActiveAudits = missingReportsResponse.EmployerActiveAuditFlag;
            }
        }

        return missingWageAndTaxReportsData;
    }

    /// <inheritdoc />
    public async Task<decimal> GetAvailableCredit()
    {
        var availbaleCredit = 0m;
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            var availableCreditResponse = await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetAvailableCreditAsync(selectedEmployer.EmployerAccount!.Id, _userAccountService.GetUserSKClaim());
            });
            return availableCreditResponse.Value;
        }

        return availbaleCredit;
    }

    /// <inheritdoc />
    public async Task<TaxAndWageEntryReportModel> GetTaxAndWageEntryReportDataAsync(long? wageTaxFilingSK, MissingReportModel? missingReportModel)
    {
        var model = new TaxAndWageEntryReportModel();
        WageTaxFilingProxy? pendingReport = null;

        if (wageTaxFilingSK is not null)
        {
            pendingReport = await GetPendingTaxReportByWageTaxFilingSK(wageTaxFilingSK.Value);
        }

        if (pendingReport is not null)
        {
            model.WageTaxFilingSK = pendingReport.WageTaxFilingSK;
            model.TaxEntry.Quarter = pendingReport.Quarter ?? missingReportModel!.Quarter;
            model.TaxEntry.TaxYear = pendingReport.Year ?? missingReportModel!.Year;
            model.TaxEntry.EmployeeCountMonth1 = pendingReport.Month1EmployeeCount ?? 0;
            model.TaxEntry.EmployeeCountMonth2 = pendingReport.Month2EmployeeCount ?? 0;
            model.TaxEntry.EmployeeCountMonth3 = pendingReport.Month3EmployeeCount ?? 0;
            model.TaxEntry.TotalGrossCoveredWages = pendingReport.GrossWages ?? 0m;
            model.TaxEntry.GrossWageDiscrepancyExplanation = pendingReport.GrossWageOutOfBalanceExplanation;
            model.TaxEntry.ExclusionOverride.OverrideAmount = pendingReport.Exclusions ?? 0m;
            model.TaxEntry.ExclusionOverride.OverrideReason = pendingReport.ExclusionAmountReason;
            model.TaxEntry.ExclusionOverride.CalculatedExclusionAmount = pendingReport.CalculatedExclusionAmount ?? 0m;
            model.TaxEntry.ExclusionOverride.IsOverride =
                !string.IsNullOrEmpty(pendingReport.ExclusionAmountReason)
                || ((pendingReport.Exclusions ?? 0m) != (pendingReport.CalculatedExclusionAmount ?? 0m));
            model.TaxEntry.DefinedTaxablePayroll = pendingReport.TaxablePayrollAmount ?? 0;
            model.TaxEntry.WageTaxFilingSK = pendingReport.WageTaxFilingSK;
            model.WageEntry.WageTaxFilingSK = pendingReport.WageTaxFilingSK;
            if (pendingReport.Employees != null && pendingReport.Employees.Length > 0)
            {
                model.WageEntry.Employees.Clear();

                foreach (var employee in pendingReport.Employees)
                {
                    model.WageEntry.Employees.Add(new EmployeeWageEntryData()
                    {
                        Id = employee.WageTaxFilingEmployeeSK ?? 0,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        SSN = employee.SSN,
                        QuarterlyWages = employee.WageAmount ?? 0m,
                        SaveForNextQuarter = employee.SaveFlag ?? true,
                        WageTaxFilingSK = employee.WageTaxFilingSK,
                    });
                }
            }
        }
        else
        {
            //first time
            var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
            if (selectedEmployer != null && missingReportModel != null)
            {
                var employeeResponse =
                    await _retryPolicy.ExecuteAsync(() =>
                    {
                        return _taxWageReportingService.GetSuitesEmployeesAsync(selectedEmployer.EmployerAccount!.Id, missingReportModel.Quarter, missingReportModel.Year, _userAccountService.GetUserSKClaim());
                    });
                foreach (var employee in employeeResponse.WageTaxFilingEmployeeProxies)
                {
                    model.WageEntry.Employees.Add(new EmployeeWageEntryData()
                    {
                        Id = employee.WageTaxFilingEmployeeSK ?? 0,
                        FirstName = employee.FirstName,
                        LastName = employee.LastName,
                        SSN = employee.SSN,
                        QuarterlyWages = 0m,
                        SaveForNextQuarter = employee.SaveFlag ?? true
                    });
                }
            }
        }
        return model;
    }

    /// <summary>
    /// Pass one of these values to this method.
    /// </summary>
    /// <returns></returns>
    public async Task<QuarterlyReportSelectionMethod> GetAvailableTaxAndReportFilingMethod(TaxWageFilingType filingType)
    {
        return filingType switch
        {
            TaxWageFilingType.TaxAndWageEntry => new QuarterlyReportSelectionMethod() { CodeSK = 1, ShortDescription = "Tax and Wage" },
            TaxWageFilingType.TaxEntry => new QuarterlyReportSelectionMethod() { CodeSK = 2, ShortDescription = "Tax" },
            TaxWageFilingType.TaxAndWageUpload => new QuarterlyReportSelectionMethod() { CodeSK = 2, ShortDescription = "Tax" },
            TaxWageFilingType.WageEntry => new QuarterlyReportSelectionMethod() { CodeSK = 3, ShortDescription = "Wage" },
            TaxWageFilingType.ZeroPayroll => new QuarterlyReportSelectionMethod() { CodeSK = 2, ShortDescription = "Tax" },
            _ => new QuarterlyReportSelectionMethod() { CodeSK = 1, ShortDescription = "Tax and Wage" },
        };
    }

    public async Task<List<QuarterlyReportSelectionMethod>> GetAvailableFilingMethods(MissingReportModel missingReportModel)
    {
        List<QuarterlyReportSelectionMethod> selectionMethods = [];
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetAvailableTaxWageFilingMethodsAsync(
                    selectedEmployer!.EmployerAccount!.Id,
                    missingReportModel.Year,
                    missingReportModel.Quarter,
                    _userAccountService.GetUserSKClaim());
            });

        foreach (var selectionMethodResponse in response.SelectionMethods)
        {
            selectionMethods.Add(new QuarterlyReportSelectionMethod()
            {
                CodeSK = selectionMethodResponse.SelectionMethod.CodeSK,
                ShortDescription = selectionMethodResponse.SelectionMethod.ShortDescription,
                IsEligible = selectionMethodResponse.IsEligible,
                EligibleReasons = selectionMethodResponse.EligibleReasons.ToList() ?? [],
            });
        }

        return selectionMethods;
    }

    public async Task<QuarterlyReportSelectionMethod?> GetAvailableFilingMethod(MissingReportModel missingReportModel, TaxWageFilingType reportType)
    {
        List<QuarterlyReportSelectionMethod> selectionMethods = [];
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetAvailableTaxWageFilingMethodsAsync(
                    selectedEmployer!.EmployerAccount!.Id,
                    missingReportModel.Year,
                    missingReportModel.Quarter,
                    _userAccountService.GetUserSKClaim());
            });

        foreach (var selectionMethodResponse in response.SelectionMethods)
        {
            selectionMethods.Add(new QuarterlyReportSelectionMethod()
            {
                CodeSK = selectionMethodResponse.SelectionMethod.CodeSK,
                ShortDescription = selectionMethodResponse.SelectionMethod.ShortDescription,
                IsEligible = selectionMethodResponse.IsEligible,
                EligibleReasons = selectionMethodResponse.EligibleReasons.ToList() ?? [],
            });
        }

        return selectionMethods.FirstOrDefault(i =>
        {
            return i.CodeSK == (int) reportType;
        });
    }


    public async Task<TaxRate> GetTaxRateForYear(int year)
    {
        TaxRate model = new();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetActiveTaxRateAsync(selectedEmployer!.EmployerAccount!.Id, year, _userAccountService.GetUserSKClaim());
            });
        var taxRatesResponse = response?.UITaxRates?.FirstOrDefault();
        if (taxRatesResponse != null)
        {
            model.AdministrativeFee = taxRatesResponse.AdministrativeFee;
            model.AdministrativeFeeProgramIntegrity = taxRatesResponse.AdministrativeFeeProgramIntegrity;
            model.BasicRate = taxRatesResponse.BasicRate;
            model.EffectiveDate = taxRatesResponse.EffectiveDate;
            model.EmployerSK = taxRatesResponse.EmployerSK;
            model.EndDate = taxRatesResponse.EndDate;
            model.RateNoticeCodeSK = taxRatesResponse.RateNoticeCodeSK;
            model.RateNoticeDescription = taxRatesResponse.RateNoticeDescription;
            model.ReserveFundBalanceRateFactorAmount = taxRatesResponse.ReserveFundBalanceRateFactorAmount;
            model.SolvencyRate = taxRatesResponse.SolvencyRate;
            model.TaxRateYear = taxRatesResponse.TaxRateYear;
            model.TaxablePayrollRateFactorAmount = taxRatesResponse.TaxablePayrollRateFactorAmount;
            model.TotalRate = taxRatesResponse.TotalRate;
            model.UITaxRateSK = taxRatesResponse.UITaxRateSK;
        }
        return model;
    }

    public async Task<List<TaxRate>> GetAllTaxRateForEmployer()
    {
        List<TaxRate> models = [];
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetActiveTaxRatesAsync(selectedEmployer!.EmployerAccount!.Id, _userAccountService.GetUserSKClaim());
            });
        if (response != null && response.UITaxRates != null && response.UITaxRates.Length > 0)
        {
            foreach (var taxRatesResponse in response.UITaxRates)
            {
                TaxRate model = new()
                {
                    AdministrativeFee = taxRatesResponse.AdministrativeFee,
                    AdministrativeFeeProgramIntegrity = taxRatesResponse.AdministrativeFeeProgramIntegrity,
                    BasicRate = taxRatesResponse.BasicRate,
                    EffectiveDate = taxRatesResponse.EffectiveDate,
                    EmployerSK = taxRatesResponse.EmployerSK,
                    EndDate = taxRatesResponse.EndDate,
                    RateNoticeCodeSK = taxRatesResponse.RateNoticeCodeSK,
                    RateNoticeDescription = taxRatesResponse.RateNoticeDescription,
                    ReserveFundBalanceRateFactorAmount = taxRatesResponse.ReserveFundBalanceRateFactorAmount,
                    SolvencyRate = taxRatesResponse.SolvencyRate,
                    TaxRateYear = taxRatesResponse.TaxRateYear,
                    TaxablePayrollRateFactorAmount = taxRatesResponse.TaxablePayrollRateFactorAmount,
                    TotalRate = taxRatesResponse.TotalRate,
                    UITaxRateSK = taxRatesResponse.UITaxRateSK
                };

                models.Add(model);
            }
        }
        return models;
    }

    public async Task<WageTaxFilingResponse> SubmitTaxReportAsync(WageTaxFilingProxy filing)
    {
        var userSK = _userAccountService.GetUserSKClaim();

        filing.SourceText = "Internet - Nelnet";
        // Step 1: Validate
        var validationResult = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.ValidateWageTaxFilingAsync(filing, userSK);
        });

        if (validationResult.RuleViolations.Any())
        {
            return validationResult;
        }

        // Step 2: Process / Submit
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.ProcessWageTaxFilingAsync(filing, userSK);
        });

        return response;
    }

    public async Task<decimal> GetCalculatedExclusionAmountAsync(MissingReportModel missingReportModel, List<CalculateWageTaxFilingEmployeeProxy> employees)
    {
        var result = 0m;
        List<QuarterlyReportSelectionMethod> selectionMethods = [];
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.CalculateWageTaxFilingExclusionsAsync(
                    new CalculateWageTaxFilingProxy()
                    {
                        EmployerSK = selectedEmployer!.EmployerAccount!.Id,
                        Quarter = missingReportModel.Quarter,
                        Year = missingReportModel.Year,
                        Employees = [.. employees]
                    },
                    _userAccountService.GetUserSKClaim());
            });

        if (response != null && (response.RuleViolations == null || response.RuleViolations.Length == 0))
        {
            result = response.Exclusions.ExclusionAmount;
        }
        return result;
    }

    public async Task<bool> GetOutOfBalanceAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.OutOfBalanceAsync(
                         selectedEmployer!.EmployerAccount!.Id,
                    _userAccountService.GetUserSKClaim());
            });

        return response != null && (response.RuleViolations == null || response.RuleViolations.Length == 0) && response.Value;
    }

    public async Task<List<WageReportProxy>> GetOutOfBalanceWageReportsAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetOutOfBalanceWageReportsAsync(
                         selectedEmployer!.EmployerAccount!.Id,
                    _userAccountService.GetUserSKClaim());
            });

        return response != null && (response.RuleViolations == null || response.RuleViolations.Length == 0) && (response.WageReportProxies != null && response.WageReportProxies.Count() > 0)
            ? response.WageReportProxies.ToList()
            : [];
    }

    public async Task<List<SummaryReportProxy>> GetOutOfBalanceSummaryReportsAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.GetOutOfBalanceSummaryReportsAsync(
                         selectedEmployer!.EmployerAccount!.Id,
                    _userAccountService.GetUserSKClaim());
            });

        return response != null && (response.RuleViolations == null || response.RuleViolations.Length == 0) && (response.SummaryReports != null && response.SummaryReports.Count() > 0)
            ? response.SummaryReports.ToList()
            : [];
    }

    public async Task<TaxEntryModel> GetTaxReportByYearAndQuarterAsync(int year, int quarter)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response =
            await _retryPolicy.ExecuteAsync(() =>
            {
                return _taxWageReportingService.LoadTaxReportsAsync(
                         selectedEmployer!.EmployerAccount!.Id,
                         quarter, year, true,
                    _userAccountService.GetUserSKClaim());
            });

        if (response != null && response.TaxReports != null && response.TaxReports.Length > 0)
        {
            var taxReport = response.TaxReports.FirstOrDefault(t =>
            {
                return t.Quarter == quarter && t.Year == year && t.IsActual == true;
            });

            if (taxReport is not null)
            {
                var startMonth = ((quarter - 1) * 3) + 1;
                return new TaxEntryModel
                {
                    MonthName1 = new DateTime(2000, startMonth, 1).ToString("MMMM"),
                    MonthName2 = new DateTime(2000, startMonth + 1, 1).ToString("MMMM"),
                    MonthName3 = new DateTime(2000, startMonth + 2, 1).ToString("MMMM"),
                    EmployeeCountMonth1 = taxReport.EmployeeMonthOneCount,
                    EmployeeCountMonth2 = taxReport.EmployeeMonthTwoCount,
                    EmployeeCountMonth3 = taxReport.EmployeeMonthThreeCount,
                    TotalGrossCoveredWages = taxReport.GrossWageAmount,
                    DefinedTaxablePayroll = taxReport.ActualTaxablePayrollAmount,
                    TaxRate = taxReport.Rate,
                    TaxAssessed = taxReport.ReportedPaidAmount,
                    DueDate = taxReport.DueDate,
                    ExclusionOverride = new ExclusionOverrideModel()
                    {
                        CalculatedExclusionAmount = taxReport.ExclusionAmount,
                    }
                };
            }
        }

        return new TaxEntryModel();
    }

    /// <inheritdoc />
    public async Task<List<PreviouslyFiledReportModel>> GetPreviouslyFiledReportsAsync()
    {
        var models = new List<PreviouslyFiledReportModel>();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount == null)
        {
            return models;
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.GetWageTaxSummariesAsync(selectedEmployer.EmployerAccount.Id, _userAccountService.GetUserSKClaim());
        });

        if (response?.WageTaxSummaries == null)
        {
            return models;
        }

        foreach (var proxy in response.WageTaxSummaries)
        {
            var quarterText = $"Q{proxy.Quarter} {proxy.Year}";

            _ = int.TryParse(proxy.Quarter, out var quarter);
            _ = int.TryParse(proxy.Year, out var year);

            models.Add(new PreviouslyFiledReportModel
            {
                QuarterYear = quarterText,
                WageReportGrossWages = proxy.WageGrossWages,
                TaxReportGrossWages = proxy.TaxGrossWages,
                TaxReportExclusions = proxy.TaxExclusions,
                TaxReportTaxablePayroll = proxy.TaxablePayroll,
                Quarter = quarter,
                Year = year,
            });
        }

        return models;
    }

    /// <inheritdoc />
    public async Task<PreviouslyFiledReportDetailsModel> GetPreviouslyFiledReportDetailsAsync(int quarter, int year)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount == null)
        {
            return new PreviouslyFiledReportDetailsModel { Quarter = quarter, Year = year, QuarterYear = $"Q{quarter} {year}" };
        }

        var employerSK = selectedEmployer.EmployerAccount.Id;
        var userSK = _userAccountService.GetUserSKClaim();

        var taxReportTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.LoadTaxReportsAsync(employerSK, quarter, year, true, userSK);
        });
        var wageReportTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.LoadWageReportAsync(employerSK, quarter, year, userSK);
        });
        var employerTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.GetPortalEmployerProxyAsync(employerSK, userSK);
        });
        var wageTaxSummaryTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.GetWageTaxSummariesAsync(employerSK, userSK);
        });

        await Task.WhenAll(taxReportTask, wageReportTask, employerTask, wageTaxSummaryTask);

        var taxReport = taxReportTask.Result?.TaxReports?.FirstOrDefault();
        var wageReport = wageReportTask.Result?.WageReportProxies ?? [];
        var employer = employerTask.Result?.EmployerProxy;
        var wageTaxSummaries = wageTaxSummaryTask.Result?.WageTaxSummaries;

        var (m1, m2, m3) = GetQuarterMonthNames(quarter);

        var taxDetails = new TaxDetailsModel
        {
            Quarter = quarter,
            TaxYear = year,
            ReportingQuarter = $"Q{quarter} {year}",
            EmployeeCount1stMonth = taxReport?.EmployeeMonthOneCount,
            FirstMonthName = m1,
            EmployeeCount2ndMonth = taxReport?.EmployeeMonthTwoCount,
            SecondMonthName = m2,
            EmployeeCount3rdMonth = taxReport?.EmployeeMonthThreeCount,
            ThirdMonthName = m3,
            TotalGrossCoveredWages = taxReport?.GrossWageAmount ?? 0m,
            LessExclusions = taxReport?.ExclusionAmount ?? 0m,
            DefinedPayroll = taxReport?.TaxablePayrollAmount ?? 0m,
            TaxRate = taxReport?.Rate ?? 0m,
            TaxAssessed = taxReport?.ComputedAmount ?? 0m,
        };

        List<EmployeeWageData> employees = [];

        var employeeWages = wageReport.FirstOrDefault();
        if (employeeWages != null)
        {
            employees = employeeWages.WageDetails
            .Select(e =>
            {
                return new EmployeeWageData
                {
                    FirstName = e.FirstName ?? string.Empty,
                    LastName = e.LastName ?? string.Empty,
                    SSN = e.SSN ?? string.Empty,
                    QuarterlyWages = e.GrossWages ?? 0m,
                    SaveForNextQuarter = e.Deleted == true ? "No" : "Yes"
                };
            }).ToList();
        }

        var addressParts = new[]
        {
            employer?.AddressLine1 ?? string.Empty,
            employer?.AddressLine2 ?? string.Empty,
            employer?.City ?? string.Empty,
            employer?.State ?? string.Empty,
            employer?.Zip
        }.Where(p =>
        {
            return !string.IsNullOrWhiteSpace(p);
        });

        var wageTaxSumary = wageTaxSummaries?.FirstOrDefault(x =>
        {
            return x.Quarter == quarter.ToString() && x.Year == year.ToString();
        });

        return new PreviouslyFiledReportDetailsModel
        {
            Quarter = quarter,
            Year = year,
            QuarterYear = $"Q{quarter} {year}",
            DueDate = taxReport?.DueDate ?? default,
            EffectiveDate = taxReport?.EffectiveDate ?? default,
            ReportStatus = taxReport?.ReportStatus ?? string.Empty,
            EmployerLegalName = employer?.LegalName ?? selectedEmployer.EmployerAccount.LegalName,
            EmployerFein = employer?.FEIN ?? string.Empty,
            EmployerAddress = string.Join(", ", addressParts),
            EmployerPhone = employer?.PhoneNumber ?? string.Empty,
            TaxConfirmationNumber = wageTaxSumary?.TaxConfirmationID ?? "No Confirmation Number On Record",
            WagesConfirmationNumber = wageTaxSumary?.WageConfirmationID ?? "No Confirmation Number On Record",
            TaxDetails = taxDetails,
            WageDetails = new WageDetailsModel { Employees = employees }
        };
    }

    private static (string first, string second, string third) GetQuarterMonthNames(int quarter)
    {
        return quarter switch
        {
            1 => ("January", "February", "March"),
            2 => ("April", "May", "June"),
            3 => ("July", "August", "September"),
            4 => ("October", "November", "December"),
            _ => (string.Empty, string.Empty, string.Empty),
        };
    }

    public async Task<SaveWageTaxFilingResponse> SaveTaxReportAsync(WageTaxFilingProxy filing)
    {
        var userSK = _userAccountService.GetUserSKClaim();
        filing.SourceText = "Internet - Nelnet";

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.SavePendingWageTaxFilingAsync(new WageTaxFilingRequest() { wtfProxy = filing, SecureUserSK = userSK });
        });

        return response;
    }

    public async Task<WageTaxFilingPendingEmployerResponse> GetPendingTaxWagesForEmployerAsync()
    {
        var userSK = _userAccountService.GetUserSKClaim();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.GetSuitesPendingTaxWagesForEmployerAsync(new EmployerRequest() { EmployerSK = selectedEmployer!.EmployerAccount!.Id, SecureUserSK = userSK });
        });

        return response;
    }

    public async Task<WageTaxFilingProxy?> GetPendingTaxReportByWageTaxFilingSK(long wageTaxFilingSK)
    {
        var response = await GetPendingTaxWagesForEmployerAsync();
        if (response != null && response.WageTaxFilingCollection != null && response.WageTaxFilingCollection.Count() > 0)
        {
            var pendingReport = response.WageTaxFilingCollection.FirstOrDefault(r =>
            {
                return r.WageTaxFilingSK == wageTaxFilingSK;
            });

            if (pendingReport is not null)
            {
                return pendingReport;
            }
        }

        return null;
    }
    public async Task<WageTaxFilingDeletedResponse> DeletePendingTaxReportByWageTaxFilingSK(int wageTaxFilingSK)
    {
        var userSK = _userAccountService.GetUserSKClaim();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.DeleteSuitesPendingTaxWagesForEmployerAsync(
                new DeleteWageTaxFilingProxy()
                {
                    EmployerSK = selectedEmployer!.EmployerAccount!.Id,
                    SecureUserSK = userSK,
                    WageTaxFilingSK = wageTaxFilingSK
                });
        });

        return response;
    }

    public async Task<WageReportProxy?> GetWageReportByQuarterYear(int year, int quarter)
    {
        var userSK = _userAccountService.GetUserSKClaim();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.LoadWageReportAsync(selectedEmployer!.EmployerAccount!.Id, quarter, year, userSK);
        });

        if (response != null && response.WageReportProxies != null && response.WageReportProxies.Length > 0)
        {
            var wageReport = response.WageReportProxies.FirstOrDefault(w =>
            {
                return w.Year == year && w.Quarter == quarter;
            });

            return wageReport;
        }

        return null;
    }

    public async Task<DeferralEligibilityResponse> CheckDeferralEligibilityAsync(int employerSk, int deferralYear)
    {
        var eligibilityResponse = await _accountSummaryService.GetFirstQuarterDeferralForYearAsync(employerSk, deferralYear, _userAccountService.GetUserSKClaim());
        var isEligible = eligibilityResponse.EmployerEligibleForFirstQuarterDeferralElection;

        var ruleViolations = new List<RuleViolationItem>();
        if (eligibilityResponse.RuleViolations.Length > 0)
        {
            foreach (var viloation in eligibilityResponse.RuleViolations)
            {
                ruleViolations.Add(new RuleViolationItem { RuleID = viloation.RuleID, RuleViolation = viloation.RuleViolation });
            }
        }

        return new DeferralEligibilityResponse
        {
            IsEligible = isEligible,
            RuleViolations = ruleViolations,
        };
    }

    public async Task<DeferralElectionResponse> ElectFirstQuarterDeferralAsync(int commonClientSk, int deferralYear, DateTime electionTime)
    {
        var deferralResponse = await _accountSummaryService.ElectForFirstQuarterDeferralAsync(commonClientSk, deferralYear, electionTime, _userAccountService.GetUserSKClaim());

        var ruleViolations = new List<RuleViolationItem>();
        var electionConfirmationNumber = deferralResponse.ElectionConfirmationNumber;
        if (deferralResponse.RuleViolations != null && deferralResponse.RuleViolations.Length > 0)
        {
            foreach (var viloation in deferralResponse.RuleViolations)
            {
                ruleViolations.Add(new RuleViolationItem { RuleID = viloation.RuleID, RuleViolation = viloation.RuleViolation });
            }
        }

        if (String.IsNullOrWhiteSpace(electionConfirmationNumber))
        {
            ruleViolations.Add(new RuleViolationItem { RuleViolation = string.IsNullOrEmpty(deferralResponse.ElectionStatus) ? _genericErrorMessage : deferralResponse.ElectionStatus });
            return new DeferralElectionResponse
            {
                RuleViolations = ruleViolations
            };
        }

        return new DeferralElectionResponse
        {
            ElectionConfirmationNumber = electionConfirmationNumber,
            ElectionStatus = deferralResponse.ElectionStatus,
            RuleViolations = ruleViolations
        };
    }
}
