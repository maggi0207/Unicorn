namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// Represents a single Employer Service Provider that currently has access
/// to the employer's account, for display on the Manage ESP Access page.
/// </summary>
public class EmployerServiceProviderAccessModel
{
    /// <summary>
    /// 
    /// </summary>
    public int EmployerServiceProviderSK { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string BusinessName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public DateTime AccessGranted { get; set; }
}
