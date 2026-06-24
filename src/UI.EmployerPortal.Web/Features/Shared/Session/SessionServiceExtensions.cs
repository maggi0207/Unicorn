using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.Shared.Session;

/// <summary>
/// Session storage provider types.
/// </summary>
public enum SessionStorageType
{
    /// <summary>
    /// Uses browser's ProtectedSessionStorage.
    /// Data is stored client-side and encrypted.
    /// Suitable for single-server deployments.
    /// </summary>
    Protected,

    /// <summary>
    /// Uses SQL Server distributed cache.
    /// Data is stored server-side in SQL Server.
    /// Suitable for multi-server deployments and when client-side storage is not desired.
    /// </summary>
    Distributed
}

/// <summary>
/// Extension methods for configuring session management services.
/// </summary>
public static class SessionServiceExtensions
{
    /// <summary>
    /// Adds session management services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="storageType">The type of session storage to use.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSessionManagement(
        this IServiceCollection services,
        SessionStorageType storageType = SessionStorageType.Protected)
    {
        switch (storageType)
        {
            case SessionStorageType.Protected:
                services.AddScoped<ISessionManager, ProtectedSessionManager>();
                break;

            case SessionStorageType.Distributed:
                services.AddScoped<ISessionIdProvider, CircuitSessionIdProvider>();
                services.AddScoped<ISessionManager, DistributedSessionManager>();
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(storageType));
        }

        return services;
    }

    /// <summary>
    /// Adds SQL Server distributed cache for session storage.
    /// Call this before AddSessionManagement when using SessionStorageType.Distributed.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="connectionString">The SQL Server connection string.</param>
    /// <param name="schemaName">The schema name (default: dbo).</param>
    /// <param name="tableName">The table name (default: SessionStore).</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSqlServerSessionStorage(
        this IServiceCollection services,
        string connectionString,
        string schemaName = "dbo",
        string tableName = "SessionStore")
    {
        services.AddDistributedSqlServerCache(options =>
        {
            options.ConnectionString = connectionString;
            options.SchemaName = schemaName;
            options.TableName = tableName;
        });

        return services;
    }
}
