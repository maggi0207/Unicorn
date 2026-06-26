using System.Threading.Tasks;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Defines the contract for fetching and updating employer account details.
/// </summary>
public interface IAccountDetailsService
{
    /// <summary>
    /// Retrieves the current account details for the employer.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, containing the account details model.</returns>
    Task<AccountDetailsModel> GetAccountDetailsAsync();

    /// <summary>
    /// Updates the account details for the employer.
    /// </summary>
    /// <param name="model">The model containing the updated account information.</param>
    /// <returns>A task that represents the asynchronous operation, containing a boolean indicating success.</returns>
    Task<bool> UpdateAccountDetailsAsync(AccountDetailsModel model);
}
