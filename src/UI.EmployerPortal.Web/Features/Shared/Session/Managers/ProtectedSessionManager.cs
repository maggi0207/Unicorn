using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.JSInterop;
namespace UI.EmployerPortal.Web.Features.Shared.Session.Managers;

/// <summary>
/// Session manager implementation using browser's ProtectedSessionStorage.
/// Data is stored in browser sessionStorage and encrypted using Data Protection API.
/// </summary>
public class ProtectedSessionManager : SessionManagerBase
{
    private readonly ProtectedSessionStorage _sessionStorage;

    /// <summary>
    /// Initialize a new instance of the <see cref="ProtectedSessionManager"/> class.
    /// </summary>
    /// <param name="sessionStorage">The protected session storage service.</param>
    public ProtectedSessionManager(ProtectedSessionStorage sessionStorage)
    {
        _sessionStorage = sessionStorage;
    }

    /// <inheritdoc />
    public override async Task<T?> GetAsync<T>() where T : class
    {
        try
        {
            var result = await _sessionStorage.GetAsync<T>(typeof(T).Name);
            return result.Success ? result.Value : default;
        }
        catch (InvalidOperationException)
        {
            //JS interop is not available during prerendering.
            return default;
        }
        catch (JSDisconnectedException)
        {
            //Circuit disconnected (e.g., during forceLoad navigation)
            return default;
        }
        catch (System.Security.Cryptography.CryptographicException)
        {
            //Data protection key changed (app restart/redeploy) - clear corrupted entry
            await _sessionStorage.DeleteAsync(typeof(T).Name);
            return default;
        }
    }

    /// <inheritdoc />
    public override async Task SetAsync<T>(T model) where T : class
    {
        try
        {
            await _sessionStorage.SetAsync(typeof(T).Name, model);
        }
        catch (JSDisconnectedException)
        {
            //Circuit disconnected - silently ignore.
        }
    }

    /// <inheritdoc />
    public override async Task ClearAsync<T>() where T : class
    {
        try
        {
            await _sessionStorage.DeleteAsync(typeof(T).Name);
        }
        catch (JSDisconnectedException)
        {
            //Circuit disconnected - silently ignore.
        }
    }
}
