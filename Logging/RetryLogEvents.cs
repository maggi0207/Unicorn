namespace UI.EmployerPortal.Web.Logging;

internal static partial class RetryLogEvents
{
    [LoggerMessage(
        EventId = LogEventIds.RetryAttempt,
        Level = LogLevel.Error,
        Message = "Attempt {retryNumber}/{retryLimit}, {duration}ms"
    )]
    public static partial void RetryAttempt(
        this ILogger logger,
        int retryNumber,
        int retryLimit,
        double duration,
        Exception exception
    );

    [LoggerMessage(
    EventId = LogEventIds.SystemErrorViolation,
    Level = LogLevel.Error,
    Message = "System error returned by {ServiceName} on page '{Page}': {OriginalMessage}"
)]
    public static partial void SystemErrorViolation(
    this ILogger logger,
    string serviceName,
    string page,
    string originalMessage
);
}
