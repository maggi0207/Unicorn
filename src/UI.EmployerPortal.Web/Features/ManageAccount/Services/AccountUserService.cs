using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
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

    Task<(bool success, string error)> MailAccessKeyAsync(string employerIdentifier);

    Task<(bool success, string keyOrError)> GenerateESPAccessKeyAsync(int employerServiceProviderSK, int roleCodeSK);

    Task<(bool success, string error)> ValidateESPAccessKeyAsync(string accessKey, string fein);

    Task<(bool success, string error)> ActivateESPAccessKeyAsync(string accessKey, string fein);

    int GetCurrentUserSk();
    Task<List<AccountUserModel>> GetEspAccountUsersAsync(int espUserSk);

    Task<(bool success, string message)> RemoveESPUser(int espSecureUserSK);
}

internal class AccountUserService : IAccountUserService
{
    private const string WorkerRelationshipExistsError = "Worker relationship already exists between this employer and user.";

    private readonly IAsyncRetryPolicy<AccountUserService> _retryPolicy;
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;
    private readonly IESPService _espService;
    private readonly IEmployerAccountService _employerAccountService;

    public AccountUserService(
        IAsyncRetryPolicy<AccountUserService> retryPolicy,
        IPortalUtilityService portalUtilityService,
        IUserAccountService userAccountService,
        IESPService espService,
        IEmployerAccountService employerAccountService)
    {
        _retryPolicy = retryPolicy;
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
        _espService = espService;
        _employerAccountService = employerAccountService;
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

    public async Task<(bool success, string message)> RemoveESPUser(int espSecureUserSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.RemoveESPUserAsync(new RemoveESPUserRequest
            {
                EmployerServiceProviderSecureUserSK = espSecureUserSK,
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

    public int GetCurrentUserSk()
    {
        return _userAccountService.GetUserSKClaim();
    }

    public async Task<List<AccountUserModel>> GetEspAccountUsersAsync(int espUserSk)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.GetESPUsersAsync(espUserSk);
        });

        if (response?.ESPEmployerSecureUserProxies == null || response.ESPEmployerSecureUserProxies.Length == 0)
        {
            return [];
        }

        var result = new List<AccountUserModel>(response.ESPEmployerSecureUserProxies.Length);
        foreach (var user in response.ESPEmployerSecureUserProxies)
        {

            result.Add(new AccountUserModel
            {
                LastName = user.LastName,
                FirstName = user.FirstName,
                Role = MapESPRole(user.Role),
                CreatedBy = user.CreatedBy,
                AccessGranted = user.AccessGranted,
                AccessKeyMasked = user.AccessKey,
                SecureUserSK = user.SecureUserSK,
                EmployerServiceProviderSecureUserSK = user.EmployerServiceProviderSecureUserSK
            });
        }

        return result;
    }

    private string MapESPRole(int? roleId)
    {
        return roleId.HasValue && Enum.IsDefined(typeof(EspUserRole), roleId.Value)
            ? ((EspUserRole) roleId.Value).ToString()
            : string.Empty;
    }


    public async Task<int[]> GetWorkerAssignedControlSKsAsync(int employerSK, int workerSecureUserSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainWorkerWebControlsAsync(employerSK, workerSecureUserSK);
        });

        return response?.WebSecurityControls == null
            ? []
            : [.. response.WebSecurityControls
                .Where(c =>
                {
                    return !PermissionMetadata.GetIsHidden(c.WebControlSK);
                })
                .Select(c =>
                {
                    return c.WebControlSK;
                })];
    }

    public async Task<List<PermissionGroup>> GetAllPermissionsAsync()
    {
        var response = await _retryPolicy.ExecuteAsync(_portalUtilityService.ObtainAllWebControlsAsync);
        return MapControlsToGroups(response?.WebSecurityControls);
    }

    internal static List<PermissionGroup> MapControlsToGroups(
        UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.WebSecurityControlProxy[]? controls)
    {
        if (controls == null || controls.Length == 0)
        {
            return [];
        }

        controls = controls.Where(c =>
        {
            return !PermissionMetadata.GetIsHidden(c.WebControlSK);
        }).ToArray();

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
                var items = controls
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
                    .ToList();

                var group = new PermissionGroup
                {
                    WebControlSK = g.WebControlSK,
                    PermissionWebControlSK = g.WebControlSK,
                    Name = PermissionMetadata.GetDisplayName(g.WebControlSK, g.Name ?? string.Empty),
                    SubText = PermissionMetadata.GetSubText(g.WebControlSK),

                    // Derived, not configured. A "Select All" header earns its place only when there
                    // is more than one child to select; note `items` is already past the hidden
                    // filter, so hiding a child can legitimately change how its parent renders.
                    HasSelectAll = items.Count > 1,
                    Items = items,
                };

                if (items.Count == 1)
                {
                    CollapseToSingleRow(group);
                }

                return group;
            })
            .OrderBy(g =>
            {
                return PermissionMetadata.GetRootGroupOrder(g.WebControlSK);
            })
            .ToList();

        return groups;
    }

    /// <summary>
    /// Renders a group that has exactly one child as a single "Select [Group]" row — the same shape a
    /// group with no children already uses. A "Select All" header sitting above one lone checkbox is
    /// noise, and it makes the user tick two boxes to express one thing.
    ///
    /// The row keeps the CHILD's WebControlSK as the permission it grants. That matters: for a group
    /// with children the parent SK is never sent to the backend — only the selected children are (see
    /// Permissions.GetSelectedWebControlSKs) — so granting the parent here would save an SK the
    /// backend has never been given for that group, and the permission the user asked for would
    /// silently not be granted.
    /// </summary>
    private static void CollapseToSingleRow(PermissionGroup group)
    {
        var only = group.Items[0];

        group.PermissionWebControlSK = only.WebControlSK;
        group.Name = PermissionMetadata.GetDisplayName(only.WebControlSK, only.Name ?? group.Name ?? string.Empty);

        if (string.IsNullOrWhiteSpace(group.SubText))
        {
            // Parents often carry no copy of their own — Appeals (273) is empty — so without this the
            // collapsed row would lose the only description the user had.
            group.SubText = only.SubText;
        }

        group.Items.Clear();
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
        if (await IsDuplicateWorkerRelationshipAsync(accessKey, employerAccountNumber))
        {
            return (false, WorkerRelationshipExistsError);
        }

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

    /// <summary>
    /// A user who already has worker access to an employer is escalated to manager when a second worker key
    /// is activated for that same employer. Detect that before activating so the caller can surface a banner
    /// instead of changing the user's role.
    /// </summary>
    private async Task<bool> IsDuplicateWorkerRelationshipAsync(string accessKey, string employerAccountNumber)
    {
        try
        {
            var keyEmployerSK = await GetWorkerKeyEmployerSKAsync(accessKey);

            return keyEmployerSK is not null
                && await IsAssociatedEmployerAsync(keyEmployerSK.Value, employerAccountNumber)
                && await IsWorkerForEmployerAsync(keyEmployerSK.Value);
        }
        catch
        {
            // The check is advisory: never fail activation because a lookup could not be completed.
            return false;
        }
    }

    /// <summary>
    /// Returns the employer the access key was generated for, or null when the key is not a worker key
    /// (manager keys legitimately upgrade an existing worker relationship).
    /// </summary>
    private async Task<int?> GetWorkerKeyEmployerSKAsync(string accessKey)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.LoadWebUserSecurityByAccessKeyAsync(new LoadWebUserSecurityByAccessKeyRequest
            {
                AccessKey = accessKey
            });
        });

        var webUserSecurity = response?.WebUserSecurity;
        if (webUserSecurity == null)
        {
            return null;
        }

        var keyRole = (EmployerRole) webUserSecurity.RoleCodeSK;
        return keyRole is EmployerRole.Worker or EmployerRole.UnassignedWorker ? webUserSecurity.EmployerSK : null;
    }

    /// <summary>
    /// Confirms the employer the key belongs to is the employer the user entered, and that the user is
    /// already associated with it.
    /// </summary>
    private async Task<bool> IsAssociatedEmployerAsync(int employerSK, string employerAccountNumber)
    {
        var accountDigits = DigitsOnly(employerAccountNumber);

        var employers = await _employerAccountService.GetEmployerAccounts();
        if (HasEmployerMatch(employers, employerSK, accountDigits))
        {
            return true;
        }

        // The association may have been granted after the cached list was loaded.
        employers = await _employerAccountService.GetEmployerAccounts(forceRefresh: true);
        return HasEmployerMatch(employers, employerSK, accountDigits);
    }

    private static bool HasEmployerMatch(List<EmployerAccount> employers, int employerSK, string accountDigits)
    {
        return employers.Any(employer =>
        {
            return employer.Id == employerSK && DigitsOnly(employer.UIAccountNo) == accountDigits;
        });
    }

    private async Task<bool> IsWorkerForEmployerAsync(int employerSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainWorkerRoleAndWebControlsAsync(employerSK, _userAccountService.GetUserSKClaim());
        });

        return response != null && (EmployerRole) response.RoleCodeSK == EmployerRole.Worker;
    }

    private static string DigitsOnly(string? value)
    {
        return string.IsNullOrEmpty(value) ? string.Empty : new string(value.Where(char.IsDigit).ToArray());
    }

    public async Task<(bool success, string error)> MailAccessKeyAsync(string employerIdentifier)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ProcessUCT16429ELetterAsync(new ProcessUCT16429ELetterRequest
            {
                SecureUserSK = secureUserSk,
                EmployerIdentifier = employerIdentifier
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

    public async Task<(bool success, string keyOrError)> GenerateESPAccessKeyAsync(int employerServiceProviderSK, int roleCodeSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.GenerateESPAccessKeyAsync(new GenerateESPUserKeyRequest
            {
                EmployerServiceProviderSK = employerServiceProviderSK,
                RoleCodeSK = roleCodeSK,
                SecureUserSK = _userAccountService.GetUserSKClaim()
            });
        });

        if (response?.RuleViolations == null || response.RuleViolations.Length == 0)
        {
            return (true, response?.AccessKeyCode ?? string.Empty);
        }

        var errors = string.Join(" ", response.RuleViolations.Select(v =>
        {
            return v.RuleViolation;
        }));
        return (false, errors);
    }

    public async Task<(bool success, string error)> ValidateESPAccessKeyAsync(string accessKey, string fein)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ValidateESPAccessKeyAsync(new RegisterESPUserKeyRequest
            {
                AccessKey = accessKey,
                FEIN = fein,
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

    public async Task<(bool success, string error)> ActivateESPAccessKeyAsync(string accessKey, string fein)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();
        fein = fein.Replace("-", "").Trim().ToString();
        var validationresponse = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ValidateESPAccessKeyAsync(new RegisterESPUserKeyRequest
            {
                AccessKey = accessKey,
                FEIN = fein,
                SecureUserSK = secureUserSk
            });
        });

        if (validationresponse.RuleViolations != null && validationresponse.RuleViolations.Length != 0)
        {
            var error = string.Join(" ", validationresponse.RuleViolations.Select(v =>
            {
                return v.RuleViolation;
            }));
            return (false, error);
        }
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ActivateESPAccessKeyAsync(new RegisterESPUserKeyRequest
            {
                AccessKey = accessKey,
                FEIN = fein,
                SecureUserSK = secureUserSk
            });
        });


        if (response?.RuleViolations == null || response.RuleViolations.Length == 0)
        {
            return response?.Value == true ? (true, string.Empty) : (false, "Error while activating access key");
        }

        var errors = string.Join(" ", response.RuleViolations.Select(v =>
        {
            return v.RuleViolation;
        }));
        return (false, errors);
    }
}
