namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// /
/// </summary>
public class COESaveRequest
{
    /// <summary>
    /// 
    /// </summary>
    public int EmployerSK { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsCalculateClicked { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool IsNoSavingsShown { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public decimal NetSavings { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int SecureUserSK { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public int Year { get; set; }
}
