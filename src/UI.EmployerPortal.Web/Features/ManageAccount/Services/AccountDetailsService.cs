using System.Threading.Tasks;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

public class AccountDetailsService : IAccountDetailsService
{
    // Normally, this service would inject a WCF client proxy to communicate with the backend.
    // Example: private readonly AccountManagementServiceClient _client;

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
