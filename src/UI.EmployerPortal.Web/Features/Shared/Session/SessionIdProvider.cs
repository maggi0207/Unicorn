namespace UI.EmployerPortal.Web.Features.Shared.Session;

/// <summary>
/// Provides a unique session identifier for the current user session.
/// </summary>
public interface ISessionIdProvider
{
    /// <summary>
    /// Gets the current session identifier. Generates a new one if not already set. 
    /// </summary>
    /// <returns>The unique session identifier.</returns>
    string GetSessionId();

    /// <summary>
    /// Sets the session identifier to a specific value. 
    /// </summary>
    /// <param name="sessionId">The session identifier to set.</param>
    void SetSessionId(string sessionId);
}

/// <summary>
/// Circuit-scoped session ID provider.
/// Generates a unique ID when first accessed and maintains it for the circuit lifetime.
/// </summary>
public class CircuitSessionIdProvider : ISessionIdProvider
{
    private string? _sessionId;

    /// <inheritdoc />
    public string GetSessionId()
    {
        if (string.IsNullOrEmpty(_sessionId))
        {
            _sessionId = Guid.NewGuid().ToString("N");
        }
        return _sessionId;
    }

    /// <inheritdoc />
    public void SetSessionId(string sessionId)
    {
        _sessionId = sessionId;
    }
}
