using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal sealed class DummyNavigationSessionTracker(ILogger<DummyNavigationSessionTracker> logger) : INavigationSessionTracker
{
    public Task LogNavigationSessionActivity(PageType pageType, string actionDescription, DateTime occuredAt)
    {
        return Task.CompletedTask;
    }

    public Task LogNavigationSessionActivity(Func<Task> action, string actionDescription, DateTime occuredAt)
    {
        return action();
    }

    public Task LogNavigationSessionActivity(Action action, string actionDescription, DateTime occuredAt)
    {
        action();
        return Task.CompletedTask;
    }

    public async Task<EventCallback> LogNavigationSessionActivity(EventCallback callback, string actionDescription, DateTime occuredAt)
    {
        await callback.InvokeAsync();
        return callback;
    }

    public Task<bool> StartNavigationSessionIfRequiredAsync(DateTime startedAt)
    {
        logger.LogNavigationSessionTrackerVersion("DUMMY");
        return Task.FromResult(false);
    }
}
