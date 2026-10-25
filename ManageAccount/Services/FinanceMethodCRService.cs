using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

internal interface IFinanceMethodCRService
{
    Task<FinanceMethodResponse> Save(int employerSK, int secureUserSK, int year);

    Task<FinanceMethodPrecheck?> GetFinanceMethodData(int employerSK, int secureUserSK);

    Task<bool> FinanceMethodChangeIsEligible(int employerSK, int secureUserSK);
}

/// <summary>
/// /
/// </summary>
public class FinanceMethodCRService : IFinanceMethodCRService
{
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    /// <summary>
    /// /
    /// </summary>
    public FinanceMethodCRService(IAccountMaintenanceService accountMaintenanceService)
    {
        _accountMaintenanceService = accountMaintenanceService;
    }

    /// <summary>
    /// /
    /// </summary>
    /// <returns></returns>
    public async Task<FinanceMethodPrecheck?> GetFinanceMethodData(int employerSK, int secureUserSK)
    {
        var req = new EmployerRequest() { EmployerSK = employerSK, SecureUserSK = secureUserSK };
        var response = await _accountMaintenanceService.FinanceMethodRequestPrecheckAsync(req);

        return response != null
            ? new FinanceMethodPrecheck()
            {
                AccountNumber = response.AccountNumber,
                CurrentFinanceMethod = response.CurrentFinanceMethod,
                EligibleYears = response.EligibleYears,
                LegalName = response.LegalName,
                FinanceMethodChangeIsEligible = response.FinanceMethodChangeIsEligible,
                IneligibilityMessage = response.IneligibilityMessage,
            }
            : null;
    }


    /// <summary>
    /// /
    /// </summary>
    /// <returns></returns>
    public async Task<FinanceMethodResponse> Save(int employerSK, int secureUserSK, int year)
    {
        return await _accountMaintenanceService.CreateNewFinanceMethodConversionRequestAsync(employerSK, secureUserSK, year);
    }

    /// <summary>
    /// /
    /// </summary>
    /// <param name="employerSK"></param>
    /// <param name="secureUserSK"></param>
    /// <returns></returns>
    public async Task<bool> FinanceMethodChangeIsEligible(int employerSK, int secureUserSK)
    {
        var req = new EmployerRequest() { EmployerSK = employerSK, SecureUserSK = secureUserSK };
        var response = await _accountMaintenanceService.FinanceMethodRequestPrecheckAsync(req);

        return response != null && response.FinanceMethodChangeIsEligible;
    }
}

