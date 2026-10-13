namespace UI.EmployerPortal.Web.Features.ManageAccount.Models;

/// <summary>
/// 
/// </summary>
public class COESaveResponse
{
    /// <summary>
    /// 
    /// </summary>
    public bool IsNoSavingsShown { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public List<string> WarningMessage { get; set; } = new List<string>();

    /// <summary>
    /// 
    /// </summary>
    public string AccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public DateTime? CorporateOfficeExclusionEffectiveDate { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string CorporateOfficeExclusionStatus { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    private int CorporateOfficeExclusionStatusSK { get; set; }

    /// <summary>
    /// 
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// 
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;


}
