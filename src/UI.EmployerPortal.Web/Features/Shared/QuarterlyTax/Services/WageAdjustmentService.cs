using UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using TaxReportResponse = UI.EmployerPortal.Generated.ServiceClients.TaxWageAdjustmentService.TaxReportResponse;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

/// <summary>
///
/// </summary>
internal interface IWageAdjustmentService
{
    /// <summary>
    /// Calls LoadWageReportAsync
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    Task<WageReportResponse> GetWageAdjustmentReport(int year, int quarter);

    /// <summary>
    /// Calls LoadTaxReportAsync
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    Task<TaxReportResponse> GetTaxAdjustmentReport(int year, int quarter);

    /// <summary>
    /// Calls SaveTaxReportAdjustmentAsync
    /// </summary>
    /// <param name="taxWageModel"></param>
    ///
    /// <returns></returns>
    Task<SaveTaxAdjustmentResponse> SaveTaxReportAdjustment(TaxAndWageAdjustmentEntryModel taxWageModel);
    /// <summary>
    /// Loads previously-reported employees for the selected quarter/year.
    /// </summary>
    Task<List<PreviouslyReportedEmployee>> GetPreviouslyReportedEmployeesAsync(int quarter, int year);

    /// <summary>
    /// Searches previously-reported employees by name or SSN.
    /// </summary>
    Task<List<PreviouslyReportedEmployee>> SearchEmployeesAsync(int quarter, int year, string searchTerm);

    /// <summary>
    /// Load Wage Report by quarter and year   
    /// </summary>
    Task<WageReportProxy?> LoadWageReportAsync(int quarter, int year);

    /// <summary>
    /// Retrieves wage adjustment reason options.
    /// </summary>
    Task<List<WageAdjustmentReasonOption>> GetAdjustmentReasonsAsync();

    /// <summary>
    /// Submits the wage adjustment and returns the confirmation number and any violations.
    /// </summary>
    Task<SaveWageAdjustmentResponse> SubmitWageAdjustmentAsync(WageAdjustmentModel model);

    /// <summary>
    /// Returns the raw pending wage adjustment response for the current employer.
    /// </summary>
    Task<PendingWageAdjustmentResponse?> GetPendingAdjustmentsAsync();
    /// <summary>
    /// Saves additional employees as a pending draft (Save and Quit).
    /// </summary>
    Task<BoolResponse> SavePendingWageAdjustmentAdditionalEmployeesAsync(
        WageAdjustmentAdditionEmployeesRequest request);

    /// <summary>
    /// Deletes a pending wage adjustment for additional employees.
    /// Used on cancel and back navigation from Step 2 to Step 1.
    /// </summary>
    Task<BoolResponse> DeletePendingWageReportAdjustmentFormAdditionalEmployeesAsync(
        int wageAdjustmentSK);

    /// <summary>
    /// Submits additional employees wage adjustment (final submit).
    /// </summary>
    Task<SaveWageAdjustmentResponse> SubmitAdditionalEmployeesAsync(
        WageAdjustmentAdditionEmployeesRequest request);
    /// <summary>
    /// Gets pending additional employees adjustment by quarter and year.
    /// </summary>
    Task<StageWageAdjustmentByQuarterProxy?> GetPendingAdditionalEmployeesAsync(
        int quarter, int year);

    /// <summary>
    /// Save a pending (draft) wage adjustment by quarter via SavePendingReportAdjustmentByQuarterAsyc
    /// </summary>
    Task<BoolResponse> SavePendingWageAdjustmentByQuarterAsync(WageAdjustmentModel model);

    /// <summary>
    /// Removes any pending (draft) wage adjustment by quarter by saving an empty detail set.
    /// </summary>
    Task<BoolResponse> RemovePendingWageAdjustmentByQuarterAsync(WageAdjustmentModel model);

}

/// <summary>
///
/// </summary>
internal class WageAdjustmentService : IWageAdjustmentService
{
    private readonly ITaxWageAdjustmentService _taxWageAdjustmentService;
    private readonly IUserAccountService _userAccountService;
    private readonly ISessionManager _sessionManager;

    /// <summary>
    ///
    /// </summary>
    ///<param name = "userAccountService" ></param >
    /// <param name="taxWageAdjustmentService"></param>
    /// <param name="sessionManager"></param>
    public WageAdjustmentService(
        IUserAccountService userAccountService,
        ITaxWageAdjustmentService taxWageAdjustmentService,
        ISessionManager sessionManager)
    {
        _userAccountService = userAccountService;
        _taxWageAdjustmentService = taxWageAdjustmentService;
        _sessionManager = sessionManager;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    public async Task<WageReportResponse> GetWageAdjustmentReport(int year, int quarter)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();

        var secureUserSK = _userAccountService.GetUserSKClaim();
        var reportRequest = new WageReportRequest();
        reportRequest.Year = year;
        reportRequest.Quarter = quarter;
        reportRequest.SecureUserSK = secureUserSK;

        if (selectedEmployer is not null)
        {
            reportRequest.EmployerSK = selectedEmployer!.EmployerAccount!.Id;
            var wageReportResponse = await _taxWageAdjustmentService.LoadWageReportAsync(reportRequest);

            return wageReportResponse;
        }
        else
        {
            return new WageReportResponse();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="year"></param>
    /// <param name="quarter"></param>
    /// <returns></returns>
    public async Task<TaxReportResponse> GetTaxAdjustmentReport(int year, int quarter)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var secureUserSK = _userAccountService.GetUserSKClaim();

        if (selectedEmployer is not null)
        {
            var employerSK = selectedEmployer!.EmployerAccount!.Id;

            var wageReportResponse = await _taxWageAdjustmentService.LoadTaxReportAsync(employerSK, quarter, year, secureUserSK);

            return wageReportResponse;
        }
        else
        {
            return new TaxReportResponse();
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="taxWageModel"></param>
    /// <returns></returns>
    public async Task<SaveTaxAdjustmentResponse> SaveTaxReportAdjustment(TaxAndWageAdjustmentEntryModel taxWageModel)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var adjustedGrossWageAmountVariance = (taxWageModel.TotalGrossCoveredWages ?? 0m) -
            (taxWageModel.PreviousTaxWageValues?.PreviousTotalGrossCoveredWages ?? 0m);

        var adjustedExclusionAmountVariance = (taxWageModel.LessExclusionWages ?? 0m) -
            (taxWageModel.PreviousTaxWageValues?.PreviousLessExclusionWages ?? 0m);

        var adjustedTaxablePayrollVariance = (taxWageModel.DefinedTaxableIncome ?? 0m) -
            (taxWageModel.PreviousTaxWageValues?.PreviousDefinedTaxableIncome ?? 0m);

        var request = new TaxReportAdjustmentRequest()
        {
            SecureUserSK = secureUserSK,
            ReportQuarter = taxWageModel.Quarter,
            ReportYear = taxWageModel.Year ?? 0,
            AdjustToZeroExplanation = taxWageModel.WageChangeText,
            AdjustedGrossWageAmount = adjustedGrossWageAmountVariance,
            AdjustedExclusionAmount = adjustedExclusionAmountVariance,
            AdjustmentReason = taxWageModel.CodeSK ?? 0,
            HasPayrollAdjustment = taxWageModel.HasPayrollAdjustment,
            MonthOneEmployeeCount = taxWageModel.EmployeeCountQtr1 ?? 0,
            MonthTwoEmployeeCount = taxWageModel.EmployeeCountQtr2 ?? 0,
            MonthThreeEmployeeCount = taxWageModel.EmployeeCountQtr3 ?? 0,
            TaxablePayrollAmount = adjustedTaxablePayrollVariance,
            ContactEmailAddress = taxWageModel.ContactEmailAddress,
        };

        if (selectedEmployer is not null)
        {
            var employerSK = selectedEmployer!.EmployerAccount!.Id;
            request.EmployerSK = employerSK;
            var wageReportResponse = await _taxWageAdjustmentService.SaveTaxReportAdjustmentAsync(request);

            return wageReportResponse;
        }
        else
        {
            return new SaveTaxAdjustmentResponse();
        }
    }

    public async Task<List<PreviouslyReportedEmployee>> GetPreviouslyReportedEmployeesAsync(int quarter, int year)
    {
        return await SearchEmployeesAsync(quarter, year, string.Empty);
    }

    /// <inheritdoc />
    public async Task<List<PreviouslyReportedEmployee>> SearchEmployeesAsync(int quarter, int year, string searchTerm)
    {
        var employees = new List<PreviouslyReportedEmployee>();
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return employees;
        }

        var request = new SearchEmployeesRequest
        {
            EmployerSK = (int) selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            Quarter = quarter,
            Year = year,
            LastName = searchTerm,
            FirstName = string.Empty,
            SSN = string.Empty,
        };

        var response = await _taxWageAdjustmentService.SearchEmployeesAsync(request);
        if (response?.WageTaxFilingEmployeeProxies is null)
        {
            return employees;
        }

        foreach (var proxy in response.WageTaxFilingEmployeeProxies)
        {
            employees.Add(new PreviouslyReportedEmployee
            {
                LastName = proxy.LastName ?? string.Empty,
                FirstName = proxy.FirstName ?? string.Empty,
                SSN = proxy.SSN ?? string.Empty,
                QuarterlyWages = proxy.GrossWages ?? 0m,
                Order = proxy.Order ?? 0,
            });
        }
        return employees;
    }

    /// <inheritdoc />
    public async Task<List<WageAdjustmentReasonOption>> GetAdjustmentReasonsAsync()
    {
        var reasons = new List<WageAdjustmentReasonOption>();

        var response = await _taxWageAdjustmentService.ObtainWageAdjustmentReasonsAsync();
        if (response is null)
        {
            return reasons;
        }
        foreach (var reason in response)
        {
            reasons.Add(new WageAdjustmentReasonOption
            {
                CodeSK = reason.CodeSK,
                ReasonText = reason.ReasonText ?? string.Empty,
            });
        }
        return reasons;
    }

    /// <inheritdoc />
    public async Task<SaveWageAdjustmentResponse> SubmitWageAdjustmentAsync(WageAdjustmentModel model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var response = new SaveWageAdjustmentResponse();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return response;
        }

        var selectedKey = model.SelectedQuarterKey ?? string.Empty;
        var parts = selectedKey.Split(' ');
        var quarter = int.Parse(parts[0].TrimStart('Q'));
        var year = int.Parse(parts[1]);
        var details = model.Employees.AdjustmentEmployees.Select(e =>
        {
            return new WageAdjustmentRequestByQuarter
            {
                OriginalLastName = e.OriginalLastName,
                OriginalFirstName = e.OriginalFirstName,
                OriginalSSN = e.OriginalSSN,
                NewLastName = e.CorrectedLastName,
                NewFirstName = e.CorrectedFirstName,
                NewSSN = e.CorrectedSSN?.Replace("-", "").ToString(),
                AdjustedGrossWages = e.AdjustedQuarterlyWages,
                WageAdjustmentDetailSK = e.WageAdjustmentDetailSKField,
                WageAdjustmentReasonCodeSK = e.AdjustmentReasonCodeSK,
                Order = e.Order,
            };
        }).ToArray();
        var request = new WageAdjustmentByQuarterRequest
        {
            EmployerSK = selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            Quarter = quarter,
            Year = year,
            WageReportSK = model.WageAdjustmentSK ?? model.WageReportSK,
            WageAdjustmentSK = model.WageAdjustmentSK,
            WageAdjustmentDetails = details,
        };

        response = await _taxWageAdjustmentService.SaveWageAdjustmentByQuarterAsync(request);

        return response;
    }

    public async Task<PendingWageAdjustmentResponse?> GetPendingAdjustmentsAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return null;
        }

        var request = new EmployerRequest
        {
            EmployerSK = (int) selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
        };

        var response = await _taxWageAdjustmentService.GetPendingWageReportAdjustmentAsync(request);
        return response;
    }

    public async Task<WageReportProxy?> LoadWageReportAsync(int quarter, int year)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return null;
        }
        var request = new WageReportRequest
        {
            EmployerSK = (int) selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            Quarter = quarter,
            Year = year,
        };
        var response = await _taxWageAdjustmentService.LoadWageReportAsync(request);
        return response?.WageReportProxies?.FirstOrDefault(p =>
        {
            return p.Quarter == quarter && p.Year == year;
        });
    }

    public async Task<BoolResponse> SavePendingWageAdjustmentByQuarterAsync(WageAdjustmentModel model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return new BoolResponse { Value = false };
        }

        var selectedKey = model.SelectedQuarterKey ?? string.Empty;
        var parts = selectedKey.Split(' ');
        var quarter = int.Parse(parts[0].TrimStart('Q'));
        var year = int.Parse(parts[1]);
        var details = model.Employees.AdjustmentEmployees.Select(e =>
        {
            return new WageAdjustmentRequestByQuarter
            {
                OriginalLastName = e.OriginalLastName,
                OriginalFirstName = e.OriginalFirstName,
                OriginalSSN = e.OriginalSSN,
                NewLastName = e.CorrectedLastName,
                NewFirstName = e.CorrectedFirstName,
                NewSSN = e.CorrectedSSN?.Replace("-", "").ToString(),
                AdjustedGrossWages = e.AdjustedQuarterlyWages,
                WageAdjustmentReasonCodeSK = e.AdjustmentReasonCodeSK,
                WageAdjustmentDetailSK = e.WageAdjustmentDetailSKField,
                Order = e.Order,
            };
        }).ToArray();
        var request = new WageAdjustmentByQuarterRequest
        {
            EmployerSK = (int) selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            Quarter = quarter,
            Year = year,
            WageAdjustmentSK = model.WageAdjustmentSK,
            WageReportSK = model.WageReportSK,
            WageAdjustmentDetails = details,
        };
        var response = await _taxWageAdjustmentService.SavePendingWageReportAdjustmentByQuarterAsync(request);
        return response;
    }

    /// <inheritdoc />
    public async Task<BoolResponse> RemovePendingWageAdjustmentByQuarterAsync(WageAdjustmentModel model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return new BoolResponse { Value = false };
        }

        var request = new WageAdjustmentDeletePendingRequest
        {
            EmployerSK = selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            WageAdjustmentSK = model.WageAdjustmentSK ?? 0
        };
        var response = await _taxWageAdjustmentService.DeletePendingWageReportAdjustmentByQuarterAsync(request);
        return response;
    }
    /// <inheritdoc />
    public async Task<BoolResponse> SavePendingWageAdjustmentAdditionalEmployeesAsync(
        WageAdjustmentAdditionEmployeesRequest request)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return new BoolResponse();
        }

        request.EmployerSK = (int) selectedEmployer.EmployerAccount.Id;
        request.SecureUserSK = _userAccountService.GetUserSKClaim();
        return await _taxWageAdjustmentService
            .SavePendingWageAdjustmentAdditionalEmployeesAsync(request);
    }

    /// <inheritdoc />
    public async Task<BoolResponse> DeletePendingWageReportAdjustmentFormAdditionalEmployeesAsync(
        int wageAdjustmentSK)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return new BoolResponse();
        }

        var request = new WageAdjustmentDeletePendingRequest
        {
            EmployerSK = (int) selectedEmployer.EmployerAccount.Id,
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            WageAdjustmentSK = wageAdjustmentSK
        };
        return await _taxWageAdjustmentService
            .DeletePendingWageReportAdjustmentFormAdditionalEmployeesAsync(request);
    }

    /// <inheritdoc />
    public async Task<SaveWageAdjustmentResponse> SubmitAdditionalEmployeesAsync(
        WageAdjustmentAdditionEmployeesRequest request)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer?.EmployerAccount is null)
        {
            return new SaveWageAdjustmentResponse();
        }

        request.EmployerSK = (int) selectedEmployer.EmployerAccount.Id;
        request.SecureUserSK = _userAccountService.GetUserSKClaim();
        return await _taxWageAdjustmentService
            .SaveWageAdjustmentAdditionalEmployeesAsync(request);
    }

    /// <inheritdoc />
    public async Task<StageWageAdjustmentByQuarterProxy?> GetPendingAdditionalEmployeesAsync(
        int quarter, int year)
    {
        var response = await GetPendingAdjustmentsAsync();
        return response?.WageAdjustmentsAppended?
            .FirstOrDefault(p =>
            {
                return p.ReportQuarter == quarter && p.ReportYear == year;
            });
    }
}
