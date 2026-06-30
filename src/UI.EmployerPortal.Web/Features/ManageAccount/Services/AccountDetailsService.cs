using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Provides integration with the AccountMaintenanceService WCF proxy
/// for fetching and updating employer account details.
/// Uses <see cref="IAccountMaintenanceService.UpdateEmployerDemographicsAsync"/>
/// to persist demographic changes.
/// </summary>
internal class AccountDetailsService : IAccountDetailsService
{
    private readonly IAsyncRetryPolicy<AccountDetailsService> _retryPolicy;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly IUserAccountService _userAccountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDetailsService"/> class.
    /// </summary>
    /// <param name="retryPolicy">The retry policy for resilient WCF calls.</param>
    /// <param name="accountMaintenanceService">The WCF proxy for account maintenance operations.</param>
    /// <param name="userAccountService">Provides the authenticated user's secure user SK.</param>
    public AccountDetailsService(
        IAsyncRetryPolicy<AccountDetailsService> retryPolicy,
        IAccountMaintenanceService accountMaintenanceService,
        IUserAccountService userAccountService)
    {
        _retryPolicy = retryPolicy;
        _accountMaintenanceService = accountMaintenanceService;
        _userAccountService = userAccountService;
    }

    /// <summary>
    /// Retrieves the current account details for the specified employer
    /// by calling <see cref="IAccountMaintenanceService.GetPortalEmployerProxyAsync"/>.
    /// </summary>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A populated <see cref="AccountDetailsModel"/>.</returns>
    public async Task<AccountDetailsModel> GetAccountDetailsAsync(int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.GetPortalEmployerProxyAsync(employerSK, secureUserSK);
        });

        if (response?.EmployerProxy == null)
        {
            return new AccountDetailsModel();
        }

        var employer = response.EmployerProxy;

        return new AccountDetailsModel
        {
            FEIN = employer.FEIN ?? string.Empty,
            LegalName = employer.LegalName ?? string.Empty,
            TradeName = employer.TradeName,
            PhoneNumber = employer.PhoneNumber ?? string.Empty,
            Extension = string.Empty,
            EmailAddress = string.Empty
        };
    }

    /// <summary>
    /// Updates the employer demographics by mapping the <see cref="AccountDetailsModel"/>
    /// to an <see cref="EmployerDemographic"/> and calling
    /// <see cref="IAccountMaintenanceService.UpdateEmployerDemographicsAsync"/>.
    /// </summary>
    /// <param name="model">The model containing the updated account information.</param>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A tuple indicating success and any error messages from rule violations.</returns>
    public async Task<(bool success, string error)> UpdateAccountDetailsAsync(AccountDetailsModel model, int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var empDemo = new EmployerDemographic
        {
            EmployerSK = employerSK,
            Fein = model.FEIN,
            TradeName = model.TradeName ?? string.Empty,
            EmailAddress = model.EmailAddress,
            LocalNumber = model.PhoneNumber,
            Extension = model.Extension ?? string.Empty,
            FeinChangeReasonCodeSK = int.TryParse(model.ReasonForFeinChange, out var feinReasonSK) ? feinReasonSK : 0
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.UpdateEmployerDemographicsAsync(empDemo, secureUserSK);
        });

        if (response?.RuleViolations == null || response.RuleViolations.Length == 0)
        {
            return (true, string.Empty);
        }

        var errors = string.Join(" ", response.RuleViolations.Select(v =>
        {
            return v.RuleViolation;
        }));
        return (false, errors);
    }
}
