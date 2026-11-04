using System.Reflection;

namespace UI.EmployerPortal.Web.Logging;

/// <summary>
/// Defines log event IDs for structured logging.
///
/// Guidelines:
/// 1. Each event ID should have a descriptive name that clearly represents the event being logged.
///    Wherever possible, the corresponding logging method should match the event name for clarity.
///
/// 2. Event ID values are immutable once assigned. Never change the numeric value after it has
///    been added and pushed to a deployable branch, as this could break log consistency across environments.
///
/// 3. Event IDs should never be deleted. If an event is no longer used, mark it with the [Obsolete]
///    attribute. The associated logging methods for obsolete events can be removed if desired, but
///    the numeric ID itself must remain to preserve historical integrity.
/// </summary>
internal static class LogEventIds
{
    // Authentication Log Events
    public const int EmployerLoggedInSuccessfully = 10_001;
    public const int EmployerLogInException = 10_002;
    public const int EmployerLogOutRequested = 10_003;
    public const int EmployerLogOutException = 10_004;
    public const int EmployerLoggedOutSuccessfully = 10_005;
    public const int TokenValidatedWithoutAuthentication = 10_006;
    public const int EmployerUserSKNullException = 10_007;
    public const int OidcSignedOutFallbackWithState = 10_008;
    public const int OidcSignedOutFallbackWithoutState = 10_009;


    // Retry Log Events
    public const int RetryAttempt = 11_001;

    // Service Log Events
    public const int SaveFileContactServiceException = 12_001;
    public const int SaveFtpUserServiceException = 12_002;
    public const int ObtainFileContactByTypeServiceException = 12_003;
    public const int LeviesServiceException = 12_004;
    public const int LevyDetailsException = 12_005;
    public const int EcommunicationsOptInSaveException = 12_006;
    public const int EFormPersistenceException = 12_007;
    public const int EFormPersistenceRuleViolation = 12_008;
    public const int EcommunicationsOptInGetException = 12_009;
    public const int LeviesServiceRuleViolation = 12_010;
    public const int LevyDocumentsException = 12_011;
    public const int LevyAnswerResponseException = 12_012;
    public const int LevyResponseSaveException = 12_013;

    public const int RulesOfConductGateRedirect = 12_014;
    public const int RulesOfConductAcknowledgementRuleViolation = 12_015;
    public const int RulesOfConductAcknowledgmentException = 12_016;
    public const int SideMenuAuthorizationException = 12_017;
    public const int SystemErrorViolation = 12_018;

    //landing page events
    public const int LandingDataRefreshRequested = 13_001;

    // Navigation Session Log Events
    public const int NavigationSessionDatabaseException = 14_001;
    public const int NavigationSessionActivityNotLogged = 14_002;
    public const int NavigationSessionTrackerVersion = 14_003;

    // Session Log Events
    public const int SessionReadFailed = 15_001;
    public const int SessionWriteFailed = 15_002;
    public const int SessionEntryCorrupt = 15_003;

    // This is an example of the appropriate way to deprecate a log event id as noted in bullet point 3 above
    [Obsolete]
    public const int ExampleDeprecatedEventId = -1;

    static LogEventIds()
    {
        ValidateUniqueLogEventIds();
    }

    private static void ValidateUniqueLogEventIds()
    {
        var hasDuplicateIds = typeof(LogEventIds)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
            .Where(x =>
            {
                return x.IsLiteral && !x.IsInitOnly && x.FieldType == typeof(int);
            })
            .GroupBy(x =>
            {
                return x.GetRawConstantValue();
            })
            .Any(x =>
            {
                return x.Count() > 1;
            });

        if (hasDuplicateIds)
        {
            throw new InvalidOperationException("Duplicate Log EventIds have been defined. EventIds must be unqiue.");
        }
    }
}
