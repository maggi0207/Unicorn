using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.Shared.Layout;
/// <summary>
/// Interface for LayoutOrchestator operations
/// </summary>
public interface ILayoutOrchestator
{
    /// <summary>
    /// Check if its a guest account
    /// </summary>
    /// <returns></returns>
    Task<bool> IsGuestAccountAsync();
}

/// <summary>
/// Orchestrator for the Layout page.
/// Handle account selection and session management.
/// </summary>
public class LayoutOrchestator : ILayoutOrchestator
{
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initialize a new instance of the <see cref="LayoutOrchestator"/> class. 
    /// </summary>
    /// <param name="sessionManager">The session manager for storing session data.</param>
    public LayoutOrchestator(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    ///<inheritdoc />
    public async Task<bool> IsGuestAccountAsync()
    {
        return await _sessionManager.IsGuestAccountAsync();
    }
}
