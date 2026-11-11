using UI.EmployerPortal.Web.Auth;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// The "last manager" rule, in one place. Two screens can trigger it — Account Users, where a
/// manager removes a row from the user list, and Account Access, where a user gives up their own
/// access to an employer — and they must not drift apart on what counts as the last manager.
/// </summary>
internal static class ManagerAccess
{
    /// <summary>
    /// Whether removing <paramref name="secureUserSK"/> would leave the employer account with no
    /// user who has Manager access.
    /// </summary>
    /// <remarks>
    /// In practice this is only ever true of a manager removing themselves: Account Users is
    /// manager-only, so any OTHER manager on the list is by definition not the last one. The test
    /// is still written against the user being removed rather than against "is this me", so it
    /// stays correct if the page is ever opened up to another role.
    /// </remarks>
    internal static bool IsLastManager(IEnumerable<AccountUserModel> users, int secureUserSK)
    {
        AccountUserModel? onlyManager = null;

        foreach (var user in users)
        {
            if (user.Role != nameof(EmployerRole.Manager))
            {
                continue;
            }

            if (onlyManager is not null)
            {
                return false;
            }

            onlyManager = user;
        }

        return onlyManager is not null && onlyManager.SecureUserSK == secureUserSK;
    }
}
