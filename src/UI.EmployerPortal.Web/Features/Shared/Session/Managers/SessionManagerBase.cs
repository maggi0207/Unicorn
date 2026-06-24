
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Session.Managers;

/// <summary>
/// Base class for session manager implementations.
/// Provides common functionality shared across different storage backends.
/// </summary>
public abstract class SessionManagerBase : ISessionManager
{
    /// <inheritdoc />    
    public abstract Task<T?> GetAsync<T>() where T : class, ISessionModel;

    /// <inheritdoc />
    public abstract Task SetAsync<T>(T model) where T : class, ISessionModel;

    /// <inheritdoc />
    public abstract Task ClearAsync<T>() where T : class, ISessionModel;

    /// <inheritdoc />
    public async Task<bool> IsGuestAccountAsync()
    {
        var selectedEmployerAccount = await GetAsync<SelectedEmployerAccount>();
        return selectedEmployerAccount?.EmployerAccount == null;
    }
}
