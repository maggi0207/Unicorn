using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Provides address management operations for the Manage Addresses page,
/// using the AccountMaintenanceService WCF proxy.
/// </summary>
public interface IManageAddressService
{
    /// <summary>
    /// Retrieves all active addresses for the specified employer.
    /// </summary>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <returns>List of <see cref="AddressRowModel"/> for display in the table.</returns>
    Task<List<AddressRowModel>> GetAddressesAsync(int employerSK);

    /// <summary>
    /// Saves an address (add or edit) by routing to the correct WCF method
    /// based on the country format code (US / Canada / Other International).
    /// </summary>
    /// <param name="model">The address form model.</param>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <returns>A tuple with success flag and error message.</returns>
    Task<(bool success, string error)> SaveAddressAsync(AddressFormModel model, int employerSK);

    /// <summary>
    /// Deletes an existing employer address by its common client address SK.
    /// </summary>
    /// <param name="addressSK">The surrogate key of the address to remove.</param>
    /// <param name="employerSK">The employer surrogate key.</param>
    /// <returns>A tuple with success flag and error message.</returns>
    Task<(bool success, string error)> DeleteAddressAsync(long addressSK, int employerSK);
}
