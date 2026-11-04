namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal sealed record NavigationSessionActivity
{
    public required NavigationSessionSk NavigationSessionSk { get; init; }
    public required PageType PageTypeSk { get; init; }
    public required string ActionDescription { get; init; } = string.Empty;
    public required string UserName { get; init; } = string.Empty;
    public int? ActivityYear { get; init; }
    public int? ActivityQuarter { get; init; }
    public int? EmployerSk { get; init; }
    public DateTime OccurredAt { get; init; }
}
