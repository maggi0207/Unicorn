namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// Initializes a new instance backed by protected session storage.
/// </summary>
public interface IBankAccountSessionStore
{
    /// <summary>
    /// Stores the bank account key that the user intents to edit, to be read after navigating
    /// to manage bank account page
    /// </summary>
    Task SetAsync(int bankAccountSk);

    /// <summary>
    /// Retrieves previously stored edit intent if any.
    /// </summary>
    Task<int?> GetAsync();

    /// <summary>
    /// Clears the stored edit intent after it has been consumed.
    /// </summary>
    Task ClearAsync();
}
