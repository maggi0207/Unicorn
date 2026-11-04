namespace UI.EmployerPortal.Web.Logging;

internal static partial class AuthenticationLogEvents
{
    [LoggerMessage(
        EventId = LogEventIds.EmployerLoggedInSuccessfully,
        Level = LogLevel.Information,
        Message = "User {userId} successfully logged in"
    )]
    public static partial void EmployerLoggedInSuccessfully(
        this ILogger logger,
        string userId
    );

    [LoggerMessage(
        EventId = LogEventIds.EmployerLogInException,
        Level = LogLevel.Error,
        Message = "An exception occurred during the user log in process"
    )]
    public static partial void EmployerLogInException(
        this ILogger logger,
        Exception exception
    );

    [LoggerMessage(
        EventId = LogEventIds.EmployerLogOutRequested,
        Level = LogLevel.Information,
        Message = "User {userId} log out requested"
    )]
    public static partial void EmployerLogOutRequested(
        this ILogger logger,
        string? userId = "unknown"
    );

    [LoggerMessage(
        EventId = LogEventIds.EmployerLogOutException,
        Level = LogLevel.Error,
        Message = "An exception occurred during the user log out process for user {userId}"
    )]
    public static partial void EmployerLogOutException(
        this ILogger logger,
        string userId,
        Exception exception
    );

    [LoggerMessage(
        EventId = LogEventIds.EmployerLoggedOutSuccessfully,
        Level = LogLevel.Information,
        Message = "User successfully logged out"
    )]
    public static partial void EmployerLoggedOutSuccessfully(
        this ILogger logger
    );

    [LoggerMessage(
        EventId = LogEventIds.TokenValidatedWithoutAuthentication,
        Level = LogLevel.Warning,
        Message = "A validate token was received however the user was not authenticated"
    )]
    public static partial void TokenValidatedWithoutAuthentication(
        this ILogger logger
    );

    [LoggerMessage(
    EventId = LogEventIds.OidcSignedOutFallbackWithState,
    Level = LogLevel.Warning,
    Message = "OIDC signed-out fallback invoked: Unable to unprotect state parameter '{state}'. Keys were desynced, rotated, or from another server node."
    )]
    public static partial void OidcSignedOutFallbackWithState(
    this ILogger logger,
    string state
    );

    [LoggerMessage(
    EventId = LogEventIds.OidcSignedOutFallbackWithoutState,
    Level = LogLevel.Information,
    Message = "OIDC signed-out fallback invoked without state parameter."
    )]
    public static partial void OidcSignedOutFallbackWithoutState(
    this ILogger logger
    );
}
