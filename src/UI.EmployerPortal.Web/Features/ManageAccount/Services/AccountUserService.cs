using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

internal interface IAccountUserService
{
    Task<List<AccountUserModel>> GetAccountUsersAsync(int employerSK);

    Task<(bool success, string message)> DeleteUserAsync(int employerSK, int webUserSecuritySK);

    Task<int[]> GetWorkerAssignedControlSKsAsync(int employerSK, int workerSecureUserSK);

    Task<List<PermissionGroup>> GetAllPermissionsAsync();

    Task<(int managerCode, int workerCode)> GetRoleCodesAsync();

    Task<(bool success, string keyOrError)> GenerateAccessKeyAsync(int employerSK, int managerWebUserSecuritySK, int webUserRoleCode, int[]? selectedWebControlSKs);

    Task<(bool success, string message)> UpdateUserPermissionsAsync(int employerSK, int webUserSecuritySK, int[] selectedWebControlSKs);

    Task<(bool success, string error)> ActivateAccessKeyAsync(string accessKey, string employerAccountNumber);

    int GetCurrentUserSk();
}

internal class AccountUserService : IAccountUserService
{
    private readonly IAsyncRetryPolicy<AccountUserService> _retryPolicy;
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;

    public AccountUserService(
        IAsyncRetryPolicy<AccountUserService> retryPolicy,
        IPortalUtilityService portalUtilityService,
        IUserAccountService userAccountService)
    {
        _retryPolicy = retryPolicy;
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
    }

    public async Task<List<AccountUserModel>> GetAccountUsersAsync(int employerSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainUsersForEmployerAsync(employerSK, _userAccountService.GetUserSKClaim());
        });

        if (response?.WorkerItems == null || response.WorkerItems.Length == 0)
        {
            return [];
        }

        var result = new List<AccountUserModel>(response.WorkerItems.Length);
        foreach (var item in response.WorkerItems)
        {
            var worker = item.Worker;
            var assignedSKs = item.Controls?.Select(c =>
            {
                return c.WebControlSK;
            }).ToArray() ?? [];

            result.Add(new AccountUserModel
            {
                FirstName = worker.FirstName,
                LastName = worker.LastName,
                Role = worker.RoleDescription,
                CreatedBy = worker.CreatedByName,
                AccessKeyMasked = worker.AccessKeyMasked,
                AccessGranted = worker.DateAccessKeyUsed,
                WebUserSecuritySK = worker.WebUserSecuritySK,
                SecureUserSK = worker.SecureUserSK,
                AssignedWebControlSKs = assignedSKs
            });
        }

        return result;
    }

    public async Task<(bool success, string message)> UpdateUserPermissionsAsync(
        int employerSK,
        int webUserSecuritySK,
        int[] selectedWebControlSKs)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.UpdateWebControlsForUserAsync(
                _userAccountService.GetUserSKClaim(),
                employerSK,
                webUserSecuritySK,
                selectedWebControlSKs);
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

    public async Task<(bool success, string message)> DeleteUserAsync(int employerSK, int webUserSecuritySK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.RevokeWebUserAccessAsync(new RevokeAccessKeyRequest
            {
                EmployerSK = employerSK,
                SecureUserSK = _userAccountService.GetUserSKClaim(),
                WebUserSecuritySK = webUserSecuritySK
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

    public int GetCurrentUserSk()
    {
        return _userAccountService.GetUserSKClaim();
    }

    public async Task<int[]> GetWorkerAssignedControlSKsAsync(int employerSK, int workerSecureUserSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainWorkerWebControlsAsync(employerSK, workerSecureUserSK);
        });

        return response?.WebSecurityControls == null
            ? []
            : [.. response.WebSecurityControls.Select(c =>
                {
                    return c.WebControlSK;
                })];
    }

    public async Task<List<PermissionGroup>> GetAllPermissionsAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(_portalUtilityService.ObtainAllWebControlsAsync);
        return MapControlsToGroups(response?.WebSecurityControls);
    }

    private static List<PermissionGroup> MapControlsToGroups(
        UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.WebSecurityControlProxy[]? controls)
    {
        if (controls == null || controls.Length == 0)
        {
            return [];
        }

        // Use ParentWebControlSK == 0 to identify root/parent groups (more reliable than Level)
        var allSKs = controls.Select(c =>
        {
            return c.WebControlSK;
        }).ToHashSet();
        var parents = controls.Where(c =>
        {
            return c.ParentWebControlSK == 0 || !allSKs.Contains(c.ParentWebControlSK);
        });
        var groups = parents
            .Select(g =>
            {
                return new PermissionGroup
                {
                    WebControlSK = g.WebControlSK,
                    Name = PermissionMetadata.GetDisplayName(g.WebControlSK, g.Name ?? string.Empty),
                    SubText = PermissionMetadata.GetSubText(g.WebControlSK),
                    HasSelectAll = PermissionMetadata.GetHasSelectAll(g.WebControlSK),
                    Items = controls
                                    .Where(c =>
                                    {
                                        return c.ParentWebControlSK == g.WebControlSK;
                                    })
                                    .Select(c =>
                                    {
                                        return new PermissionItem
                                        {
                                            WebControlSK = c.WebControlSK,
                                            Name = PermissionMetadata.GetDisplayName(c.WebControlSK, c.Name ?? string.Empty),
                                            SubText = PermissionMetadata.GetSubText(c.WebControlSK)
                                        };
                                    })
                                    .ToList()
                };
            })
            .ToList();

        return groups;
    }

    public async Task<(int managerCode, int workerCode)> GetRoleCodesAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(_portalUtilityService.GetAllExternalAccessRolesAsync);
        if (response?.WebUserRoleCodes == null)
        {
            return (0, 0);
        }

        var hasManager = response.WebUserRoleCodes.Any(r =>
        {
            return r.ShortDescription?.Contains("Manager", StringComparison.OrdinalIgnoreCase) == true;
        });

        var hasWorker = response.WebUserRoleCodes.Any(r =>
        {
            return r.ShortDescription?.Contains("Worker", StringComparison.OrdinalIgnoreCase) == true;
        });

        return (hasManager ? 1 : 0, hasWorker ? 2 : 0);
    }

    public async Task<(bool success, string error)> ActivateAccessKeyAsync(string accessKey, string employerAccountNumber)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ActivateAccessKeyAsync(new AccessKeyRequest
            {
                AccessKey = accessKey,
                EmployerAccountNumber = employerAccountNumber,
                SecureUserSK = _userAccountService.GetUserSKClaim()
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
    public async Task<(bool success, string keyOrError)> GenerateAccessKeyAsync(
        int employerSK,
        int managerWebUserSecuritySK,
        int webUserRoleCode,
        int[]? selectedWebControlSKs)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.GenerateAccessKeyAsync(new GenerateAccessKeyRequest
            {
                EmployerSK = employerSK,
                SecureUserSK = _userAccountService.GetUserSKClaim(),
                ManagerWebUserSecuritySK = managerWebUserSecuritySK,
                WebUserRoleCode = webUserRoleCode,
                SelectedWebSecurityControlSKs = selectedWebControlSKs
            });
        });

        if (response?.RuleViolations == null || response.RuleViolations.Length == 0)
        {
            var key = response?.AccessKey ?? response?.AccessKeyProxy?.Key ?? string.Empty;
            return (true, key);
        }

        var errors = string.Join(" ", response.RuleViolations.Select(v =>
        {
            return v.RuleViolation;
        }));
        return (false, errors);
    }
}

