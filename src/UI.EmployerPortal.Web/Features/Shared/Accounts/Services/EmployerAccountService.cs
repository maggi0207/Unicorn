using System.Collections.Concurrent;
using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

internal interface IEmployerAccountService
{
    Task<List<EmployerAccount>> GetEmployerAccounts(bool forceRefresh = false);
    Task<List<string>> GetMissingQuartersInfo(int employerSk);
    Task<EmployerAccount> GetRegisterEmployerAccount(int employerSk);
}

internal class EmployerAccountService : IEmployerAccountService
{
    private readonly IAsyncRetryPolicy<EmployerAccountService> _retryPolicy;
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;
    private readonly ITaxWageReportingService _taxWageReportingService;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly ISessionManager _sessionManager;

    public EmployerAccountService(
        IAsyncRetryPolicy<EmployerAccountService> retryPolicy,
        IPortalUtilityService portalUtilityService,
        IUserAccountService userAccountService,
        ITaxWageReportingService taxWageReportingService,
        IAccountMaintenanceService accountMaintenanceService,
        ISessionManager sessionManager)

    {
        _retryPolicy = retryPolicy;
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
        _taxWageReportingService = taxWageReportingService;
        _accountMaintenanceService = accountMaintenanceService;
        _sessionManager = sessionManager;
    }

    public async Task<List<EmployerAccount>> GetEmployerAccounts(bool forceRefresh)
    {
        if (!forceRefresh)
        {
            var sessionEmployerAccounts = await _sessionManager.GetAsync<SessionAllEmployerAccounts>();

            if (sessionEmployerAccounts?.EmployerAccounts is { Count: > 0 })
            {
                return sessionEmployerAccounts.EmployerAccounts;
            }
        }

        var associatedEmployers = new ConcurrentBag<EmployerAccount>(); //Thread-safe collection, avoid locking. 

        var employers = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainAssociatedEmployersAsync(_userAccountService.GetUserSKClaim());
        });

        if (employers == null)
        {
            return [.. associatedEmployers];
        }

        // Filter to only those we currently have employer access claims for
        // Users will need to log out and in again to update claims and see newly added employers
        var employerSks = employers.EmployerProxies.Select(x =>
        {
            return x.CommonClientSK;
        });
        var authorizedEmployerSkSet = _userAccountService.RemoveEmployerSksWithoutAccess(employerSks).ToHashSet();
        var authorizedEmployers = employers.EmployerProxies.Where(x =>
            {
                return authorizedEmployerSkSet.Contains(x.CommonClientSK);
            });

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        await Parallel.ForEachAsync(
            source: authorizedEmployers,
            parallelOptions: new ParallelOptions
            {
                MaxDegreeOfParallelism = 32,
                CancellationToken = cts.Token
            },
            async (item, token) =>
            {
                /* Once Dashboard service is ready it may return balance and missing quarters, we can then swap this code */
                var balanceTask = _retryPolicy.ExecuteAsync(() =>
                {
                    return _taxWageReportingService.GetCurrentBalanceAsync(item.CommonClientSK, _userAccountService.GetUserSKClaim());
                });
                var missingQuartersTask = _retryPolicy.ExecuteAsync(() =>
                {
                    return _taxWageReportingService.GetMissingQuartersAsync(item.CommonClientSK, _userAccountService.GetUserSKClaim());
                });

                await Task.WhenAll(balanceTask, missingQuartersTask);
                var balance = balanceTask.Result;
                var missingReports = missingQuartersTask.Result;

                associatedEmployers.Add(new EmployerAccount()
                {
                    Id = item.CommonClientSK,
                    LegalName = item.LegalName,
                    UIAccountNo = item.UIAccountNumber,
                    BalanceDue = balance?.Value ?? 0,
                    IsTaxable = item.IsTaxable,
                    MissingQuarterlyReports = missingReports?.MissingReports?.Length ?? 0
                });
            });

        await _sessionManager.SetAsync(new SessionAllEmployerAccounts { EmployerAccounts = [.. associatedEmployers] });
        return [.. associatedEmployers];
    }

    public async Task<List<string>> GetMissingQuartersInfo(int employerSk)
    {
        var missingQuarters = new List<string>();
        var secureUserSK = _userAccountService.GetUserSKClaim();

        try
        {
            //WCF call to get Missing quarters.
            var missingResult = await _taxWageReportingService.GetMissingQuartersAsync(employerSk, secureUserSK);

            if (missingResult != null && missingResult.MissingReports != null)
            {
                missingQuarters = missingResult.MissingReports.Select(x =>
                {
                    ArgumentNullException.ThrowIfNull(x);
                    return x.FormattedQuarterYear;
                }).ToList();

                return missingQuarters;
            }
            else
            {
                return missingQuarters;
            }
        }
        catch (Exception)
        {
            throw new ApplicationException("Something went wrong.Please contact support if the issue continues.");
        }
    }

    public async Task<EmployerAccount> GetRegisterEmployerAccount(int employerSk)
    {
        //var associatedEmployers = new ConcurrentBag<EmployerAccount>(); //Thread-safe collection, avoid locking.
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var result = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.GetPortalEmployerProxyAsync(employerSk, secureUserSK);
        });

        if (result?.EmployerProxy == null)
        {
            return new EmployerAccount();
        }

        var balanceTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.GetCurrentBalanceAsync(employerSk, secureUserSK);
        });
        var missingQuartersTask = _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.GetMissingQuartersAsync(employerSk, secureUserSK);
        });

        await Task.WhenAll(balanceTask, missingQuartersTask);
        var balance = balanceTask.Result;
        var missingReports = missingQuartersTask.Result;

        return new EmployerAccount()
        {
            Id = result.EmployerProxy.CommonClientSK,
            LegalName = result.EmployerProxy.LegalName,
            UIAccountNo = result.EmployerProxy.UIAccountNumber,
            IsTaxable = result.EmployerProxy.IsTaxable,
            BalanceDue = balance?.Value ?? 0,
            MissingQuarterlyReports = missingReports?.MissingReports?.Length ?? 0
        };

    }

}
