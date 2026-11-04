namespace UI.EmployerPortal.Web.Logging;

/// <summary>
/// Structured log events for the Rules of Conduct acknowledgement gate (ADR-020).
/// </summary>
internal static partial class RulesOfConductLogEvents
{
    [LoggerMessage(
        EventId = LogEventIds.RulesOfConductAcknowledgementRuleViolation,
        Level = LogLevel.Warning,
        Message = "Rules of Conduct service call '{OperationName}' returned rule violation {RuleId}: {RuleViolation}"
    )]
    public static partial void RulesOfConductAcknowledgementRuleViolation(this ILogger logger, string operationName, string ruleId, string ruleViolation);

    [LoggerMessage(
        EventId = LogEventIds.RulesOfConductAcknowledgmentException,
        Level = LogLevel.Error,
        Message = "Rules of Conduct service call '{OperationName}' threw an exception"
    )]
    public static partial void RulesOfConductAcknowledgementException(this ILogger logger, string operationName, Exception exception);

    [LoggerMessage(
        EventId = LogEventIds.RulesOfConductGateRedirect,
        Level = LogLevel.Information,
        Message = "Rules of Conduct gate blocked {path} for an unacknowledged user; redirecting to the acknowledgement page."
    )]
    public static partial void RulesOfConductGateRedirect(this ILogger logger, string path);
}
