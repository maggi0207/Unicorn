using Microsoft.Extensions.Options;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal static class DependencyInjection
{
    /// <summary>
    /// Registers either a real NavigationSessionTracker or a DummyNavigationSessionTracker
    /// depending on whether a NavigationDb connection string is available.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddNavigationSessionTracking(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<NavigationSessionOptions>(options =>
        {
            options.ConnectionString = configuration.GetConnectionString("NavigationDb") ?? string.Empty;
        });

        services.AddScoped<INavigationSessionTracker>(provider =>
        {
            var options = provider.GetRequiredService<IOptions<NavigationSessionOptions>>();

            return string.IsNullOrEmpty(options.Value.ConnectionString)
                ? new DummyNavigationSessionTracker(
                    provider.GetRequiredService<ILogger<DummyNavigationSessionTracker>>())
                : new NavigationSessionTracker(
                    options,
                    provider.GetRequiredService<ISessionManager>(),
                    provider.GetRequiredService<IUserAccountService>(),
                    provider.GetRequiredService<IHttpContextAccessor>(),
                    provider.GetRequiredService<ILogger<NavigationSessionTracker>>());
        });

        services.AddScoped<INavigationMonitor, NavigationMonitor>();

        return services;
    }
}
