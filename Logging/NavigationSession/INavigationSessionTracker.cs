using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal interface INavigationSessionTracker
{
    Task<bool> StartNavigationSessionIfRequiredAsync(DateTime startedAt);
    Task LogNavigationSessionActivity(PageType pageCode, string actionDescription, DateTime occuredAt);
    Task LogNavigationSessionActivity(Func<Task> action, string actionDescription, DateTime occuredAt);
    Task LogNavigationSessionActivity(Action action, string actionDescription, DateTime occuredAt);
    Task<EventCallback> LogNavigationSessionActivity(EventCallback callback, string actionDescription, DateTime occuredAt);
}
