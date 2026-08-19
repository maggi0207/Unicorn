namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// EftPaymentResponse
/// </summary>
public class EftPaymentResponse
{
    /// <summary>ConfirmationId</summary>
    public string ConfirmationId { get; set; } = string.Empty;

    /// <summary>EFTPaymentSK</summary>
    public int EFTPaymentSK { get; set; }

    /// <summary>IsAuthorized</summary>
    public bool IsAuthorized { get; set; } = false;

    /// <summary>transactionDateTime</summary>
    public DateTime TransactionDateTime { get; set; }

    /// <summary>ErrorMessage</summary>
    public string ErrorMessage { get; set; } = string.Empty;
}
