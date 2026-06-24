using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Session.Models;

/// <summary>
/// SessionEmployerAccounts
/// </summary>
public class SessionAllEmployerAccounts : ISessionModel
{
    /// <summary>
    /// Store all the Employer Accounts
    /// </summary>
    public List<EmployerAccount>? EmployerAccounts { get; set; } = default!;
}

/// <summary>
/// SelectedEmployerAccount
/// </summary>
public class SessionSelectedEmployerAccount : ISessionModel
{
    /// <summary>
    /// Store the selected Employer Account No
    /// </summary>
    public string SelectedEmployerAccountValue { get; set; } = string.Empty;
}
