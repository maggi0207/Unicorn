namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal static partial class NavigationSessionLogEvents
{
    [LoggerMessage(
        EventId = LogEventIds.NavigationSessionDatabaseException,
        Level = LogLevel.Error,
        Message = "An exception occurred when trying to persist navigation session tracking"
    )]
    public static partial void LogNavigationSessionDatabaseException(
        this ILogger logger,
        Exception exception
    );

    [LoggerMessage(
        EventId = LogEventIds.NavigationSessionActivityNotLogged,
        Level = LogLevel.Warning,
        Message = "Navigation Activity was not tracked for page code {pageCode} and action: {actionDescription}"
    )]
    public static partial void LogNavigationSessionActivityNotLogged(
        this ILogger logger,
        PageType pageCode,
        string actionDescription
    );

    [LoggerMessage(
        EventId = LogEventIds.NavigationSessionTrackerVersion,
        Level = LogLevel.Information,
        Message = "Using {version} Navigation Logging"
    )]
    public static partial void LogNavigationSessionTrackerVersion(
        this ILogger logger,
        string version
    );
}
