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

    /// <summary>
    /// Gets or sets the confirmation identifier when available after save or reload.
    /// </summary>
    public string ConfirmationId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the date the election was submitted when available after save or reload.
    /// </summary>
    public DateTime? DateSubmitted { get; set; }
}
