using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;

namespace UI.EmployerPortal.Web.Features.ESP.Services;


/// <summary>
/// Session-backed implementation of <see cref="IESPBankAccountSessionStore"/>.
/// </summary>
public class ESPBankAccountSessionStore : IESPBankAccountSessionStore
{
    private const string Key = "espbankAccountEditIntent";
    private readonly ProtectedSessionStorage _storage;

    /// <summary>
    /// Initializes a new instance backed by protected session storage.
    /// </summary>
    public ESPBankAccountSessionStore(ProtectedSessionStorage storage)
    {
        _storage = storage;

    }

    /// <summary>
    /// Stores the bank account key that the user intents to edit, to be read after navigating
    /// to manage bank account page
    /// </summary>
    public async Task SetAsync(int bankAccountSk)
    {
        await _storage.SetAsync(Key, bankAccountSk);
    }

    /// <summary>
    /// Retrieves previously stored edit intent if any.
    /// </summary>
    public async Task<int?> GetAsync()
    {
        var result = await _storage.GetAsync<int>(Key);
        return result.Success ? result.Value : null;
    }

    /// <summary>
    /// Clears the stored edit intent after it has been consumed.
    /// </summary>
    public async Task ClearAsync()
    {
        await _storage.DeleteAsync(Key);
    }
}
