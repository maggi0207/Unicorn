using System.Threading.Tasks;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Provides a mocked implementation for fetching and updating employer account details.
/// Connects to backend WCF services when fully integrated.
/// </summary>
public class AccountDetailsService : IAccountDetailsService
{
    // Normally, this service would inject a WCF client proxy to communicate with the backend.
    // Example: private readonly AccountManagementServiceClient _client;

    /// <summary>
    /// Retrieves the current account details for the employer.
    /// Simulates a backend WCF call with mock data.
    /// </summary>
    /// <returns>A task containing the mock <see cref="AccountDetailsModel"/>.</returns>
    public async Task<AccountDetailsModel> GetAccountDetailsAsync()
    {
        // Simulate an asynchronous call to a WCF service
        await Task.Delay(100);

        return new AccountDetailsModel
        {
            FEIN = "11-2233445",
            LegalName = "Goyette & Sons",
            PhoneNumber = "(608) 266-5793",
            Extension = "99",
            EmailAddress = "jsmith@goyetteandsons.com"
        };
    }

    /// <summary>
    /// Updates the account details for the employer.
    /// Simulates sending the updated model to the backend WCF service.
    /// </summary>
    /// <param name="model">The model containing the updated account information.</param>
    /// <returns>A task containing a boolean indicating if the save was successful.</returns>
    public async Task<bool> UpdateAccountDetailsAsync(AccountDetailsModel model)
    {
        // Simulate data mapping to the WCF proxy object.
        // As per unicorn_project_skills.md, if there's a [PropertyName]Specified property for 
        // value types, we must explicitly set it to true for serialization.
        
        // Example:
        // var request = new UpdateAccountRequest { 
        //     FEIN = model.FEIN, 
        //     LegalName = model.LegalName 
        // };
        // await _client.UpdateAccountInformationAsync(request);

        await Task.Delay(100);
        return true; // Simulate successful save
    }
}
