namespace UI.EmployerPortal.Web.Logging;

internal static partial class DashboardLogEvents
{
    [LoggerMessage(
    EventId = LogEventIds.EcommunicationsOptInSaveException,
    Level = LogLevel.Error,
    Message = "Failed to save eCommunication enrollment for employer {employerSK}.")]
    public static partial void EcommunicationsOptInSaveFailed(
        this ILogger logger,
        int employerSK,
        Exception exception);

    [LoggerMessage(
    EventId = LogEventIds.EcommunicationsOptInGetException,
    Level = LogLevel.Error,
    Message = "Failed to retrieve eCommunication enrollment status for employer {employerSK}.")]
    public static partial void EcommunicationsOptInGetFailed(
        this ILogger logger,
        int employerSK,
        Exception exception);
}
