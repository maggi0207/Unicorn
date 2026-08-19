namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// EftPaymentRequest
/// </summary>
public class EftPaymentRequest
{
    /// <summary>BankAccountSK</summary>
    public int BankAccountSK { get; set; }

    /// <summary>FileUploadDetailSK</summary>
    public int FileUploadDetailSK { get; set; }

    /// <summary>PaymentAmount</summary>
    public decimal PaymentAmount { get; set; }

    /// <summary>PaymentSettlementDate</summary>
    public DateTime PaymentSettlementDate { get; set; }

    /// <summary>PaymentType</summary>
    public int PaymentType { get; set; }

    /// <summary>SecureUserSK</summary>
    public int SecureUserSK { get; set; }

    /// <summary>SentDate</summary>
    public System.DateTime SentDate { get; set; }

    /// <summary>EFTPaymentSK</summary>
    public int EFTPaymentSK { get; set; }

    /// <summary>EmployerSK</summary>
    public int EmployerSK { get; set; }
}
