using System.Security.Claims;
using UI.EmployerPortal.Web.Auth;

namespace UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

internal interface IUserAccountService
{
    bool IsAuthenticated();
    ClaimsPrincipal? GetCurrentUser();
    IEnumerable<Claim> GetClaims();
    string? GetClaimValue(string type);
    string? GetCurrentUserId();
    int GetUserSKClaim();
    string? GetFirstName();
    string? GetLastName();
    string? GetUserID();
    string? GetOktaUUID();
    bool HasEmployerRole(int employerSk, EmployerRole role);
    bool HasEmployerAccess(int employerSk);
    IEnumerable<int> RemoveEmployerSksWithoutAccess(IEnumerable<int> employerSks);
    bool HasEmployerPermission(int employerSk, EmployerPermissions permission);
    bool HasAllEmployerPermissions(int employerSk, IReadOnlyList<EmployerPermissions> checks);
    bool HasAnyEmployerPermissions(int employerSk, IReadOnlyList<EmployerPermissions> checks);
}

internal class UserAccountService : IUserAccountService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserAccountService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ClaimsPrincipal? GetCurrentUser()
    {
        return _httpContextAccessor.HttpContext?.User;
    }

    public IEnumerable<Claim> GetClaims()
    {
        return GetCurrentUser()?.Claims ?? [];
    }

    public string? GetClaimValue(string type)
    {
        return GetCurrentUser()?.FindFirst(type)?.Value;
    }

    public int GetUserSKClaim()
    {
        return int.TryParse(GetClaimValue(DwdClaimNames.SecureUserSKClaimName), out var value) ? value : 0;
    }

    public string? GetCurrentUserId()
    {
        return GetCurrentUser()?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
    }

    public bool IsAuthenticated()
    {
        return GetCurrentUser()?.Identity?.IsAuthenticated ?? false;
    }

    public string? GetFirstName()
    {
        return GetClaimValue(DwdClaimNames.FirstNameClaimName);
    }

    public string? GetLastName()
    {
        return GetClaimValue(DwdClaimNames.LastNameClaimName);
    }

    public string? GetUserID()
    {
        return GetClaimValue(DwdClaimNames.UserIDClaimName);
    }

    public string? GetOktaUUID()
    {
        return GetClaimValue(DwdClaimNames.OktaUUIDClaimName);
    }

    public bool HasEmployerAccess(int employerSk)
    {
        return HasEmployerPermission(employerSk, EmployerPermissions.None);
    }

    public bool HasEmployerPermission(int employerSk, EmployerPermissions permission)
    {
        return GetEmployerAccesses()
            .Any(x =>
            {
                return x.HasPermission(employerSk, permission);
            });
    }

    public bool HasAllEmployerPermissions(int employerSk, IReadOnlyList<EmployerPermissions> checks)
    {
        var combinedPermission = checks.Aggregate(EmployerPermissions.None, (accumulator, source) =>
        {
            return accumulator | source;
        });
        return GetEmployerAccesses()
            .Any(x =>
            {
                return x.HasPermission(employerSk, combinedPermission);
            });
    }

    public bool HasAnyEmployerPermissions(int employerSk, IReadOnlyList<EmployerPermissions> checks)
    {
        var employerAccess = GetEmployerAccesses()
            .FirstOrDefault(x =>
            {
                return x.EmployerSk == employerSk;
            });
        return employerAccess is not null && checks
            .Any(x =>
            {
                return employerAccess.HasPermission(employerSk, x);
            });
    }

    public IEnumerable<int> RemoveEmployerSksWithoutAccess(IEnumerable<int> employerSks)
    {
        return GetEmployerAccesses()
            .Select(x =>
            {
                return x.EmployerSk;
            })
            .Intersect(employerSks);
    }

    private List<EmployerAccess> GetEmployerAccesses()
    {
        return GetClaims()
            .Where(x =>
            {
                return x.Type == DwdClaimNames.EmployerAccessClaimName;
            })
            .Select(EmployerAccess.CreateEmployerAccessFromClaim)
            .ToList();
    }

    public bool HasEmployerRole(int employerSk, EmployerRole role)
    {
        return GetEmployerAccesses()
            .Any(x =>
            {
                return x.EmployerSk == employerSk && x.Role == role;
            });
    }
}
