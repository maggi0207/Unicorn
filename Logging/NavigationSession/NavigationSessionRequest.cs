namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal sealed record NavigationSessionRequest
{
    public required string SessionId { get; init; } = string.Empty;
    public required string IpAddress { get; init; } = string.Empty;
    public required string UserAgent { get; init; } = string.Empty;
    public required string UserName { get; init; } = string.Empty;
    public required int SecureUserSk { get; init; }
    public required DateTime StartedAt { get; init; }
}
