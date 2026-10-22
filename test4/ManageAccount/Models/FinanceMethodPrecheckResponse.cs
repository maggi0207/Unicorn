namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// 
/// </summary>
public class FinanceMethodPrecheck
{
    /// <summary>
    /// 
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string CurrentFinanceMethod { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public int[]? EligibleYears { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool FinanceMethodChangeIsEligible { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string IneligibilityMessage { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string LegalName { get; set; } = string.Empty;
}
