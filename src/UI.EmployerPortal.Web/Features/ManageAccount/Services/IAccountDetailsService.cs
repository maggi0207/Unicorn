using System.Threading.Tasks;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

public interface IAccountDetailsService
{
    Task<AccountDetailsModel> GetAccountDetailsAsync();
    Task<bool> UpdateAccountDetailsAsync(AccountDetailsModel model);
}
