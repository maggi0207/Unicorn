using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.Dashboard;

/// <summary>
/// Interface for dashboard orchestration operations
/// Manages session data access for employer dashboard.
/// </summary>
public interface IDashboardOrchestrator
{
    /// <summary>
    /// Retrieves the currently selected employer account from session storage. 
    /// </summary>
    /// <returns>The selected account if available; otherwise null.</returns>
    Task<EmployerAccount?> GetSelectedEmployerAccountAsync();

    /// <summary>
    /// Remvoe curretly selected employer. This will be required in case if user select guest account.
    /// </summary>
    /// <returns></returns>
    Task RemoveEmployerAccountFromSession();
}

/// <summary>
/// Orchestrator for the employer dashbaord.
/// Handle session management and data retrieval for dashboard components.
/// </summary>
public class DashboardOrchestrator : IDashboardOrchestrator
{
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initialize a new instance of the <see cref="DashboardOrchestrator"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager for accessing session data.</param>
    public DashboardOrchestrator(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    ///<inheritdoc />
    public async Task<EmployerAccount?> GetSelectedEmployerAccountAsync()
    {
        var selectedAccount = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedAccount?.EmployerAccount;
    }

    ///<inheritdoc />
    public async Task RemoveEmployerAccountFromSession()
    {
        await _sessionManager.ClearAsync<SelectedEmployerAccount>();
    }
}
