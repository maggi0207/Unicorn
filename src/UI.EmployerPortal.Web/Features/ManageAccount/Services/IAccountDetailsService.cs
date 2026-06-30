using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Defines the contract for fetching and updating employer account details
/// via the AccountMaintenanceService WCF proxy.
/// </summary>
internal interface IAccountDetailsService
{
    /// <summary>
    /// Retrieves the current account details for the employer.
    /// </summary>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A task containing the account details model.</returns>
    Task<AccountDetailsModel> GetAccountDetailsAsync(int employerSK);

    /// <summary>
    /// Updates the employer demographics via the
    /// <see cref="IAccountMaintenanceService.UpdateEmployerDemographicsAsync"/> WCF operation.
    /// </summary>
    /// <param name="model">The model containing the updated account information.</param>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A tuple indicating success and any error messages from rule violations.</returns>
    Task<(bool success, string error)> UpdateAccountDetailsAsync(AccountDetailsModel model, int employerSK);
}
