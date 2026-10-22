using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Feature service for the employer's view of Employer Service Provider access:
/// listing ESPs currently granted access, and revoking that access.
/// </summary>
public interface IEmployerServiceProviderAccessService
{
    /// <summary>Retrieves the list of ESPs currently granted access to the given employer account.</summary>
    Task<List<EmployerServiceProviderAccessModel>> GetEspAccessListAsync(int employerSK);

    /// <summary>Revokes a specific ESP's access to the given employer account.</summary>
    Task<(bool success, string message)> RemoveEspAccessAsync(int employerSK, int employerServiceProviderSK);
}

/// <inheritdoc />
internal class EmployerServiceProviderAccessService : IEmployerServiceProviderAccessService
{
    private readonly IESPService _espService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<EmployerServiceProviderAccessService> _retryPolicy;

    /// <summary>Initializes a new instance of the <see cref="EmployerServiceProviderAccessService"/> class.</summary>
    public EmployerServiceProviderAccessService(
        IESPService espService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<EmployerServiceProviderAccessService> retryPolicy)
    {
        _espService = espService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc />
    public async Task<List<EmployerServiceProviderAccessModel>> GetEspAccessListAsync(int employerSK)
    {
        if (employerSK == 0)
        {
            return [];
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ObtainESPsByEmployerAsync(new FindEmployerServiceProvidersRequest
            {
                EmployerSK = employerSK
            });
        });

        if (response?.Items == null || response.Items.Length == 0)
        {
            return [];
        }

        var result = new List<EmployerServiceProviderAccessModel>(response.Items.Length);
        foreach (var proxy in response.Items)
        {
            result.Add(new EmployerServiceProviderAccessModel
            {
                EmployerServiceProviderSK = proxy.EmployerServiceProviderSK,
                BusinessName = proxy.BusinessName ?? string.Empty,
                AccessGranted = proxy.AccessGranted
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<(bool success, string message)> RemoveEspAccessAsync(int employerSK, int employerServiceProviderSK)
    {
        if (employerSK == 0 || employerServiceProviderSK == 0)
        {
            return (false, "Unable to determine the ESP or employer account.");
        }

        var userSK = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.RemoveESPRelationshipAsync(new RemoveESPClientRequest
            {
                EmployerSK = employerSK,
                EmployerServiceProviderSK = employerServiceProviderSK,
                SecureUserSk = userSK
            });
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
