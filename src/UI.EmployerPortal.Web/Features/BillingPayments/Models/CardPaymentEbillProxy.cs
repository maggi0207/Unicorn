namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// CardPaymentEbillProxy
/// </summary>
public class CardPaymentEbillProxy
{
    /// <summary>
    /// RegistrationSK
    /// </summary>
    public int RegistrationSK { get; set; }

    /// <summary>
    /// ConfirmationID
    /// </summary>
    public string? ConfirmationID { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? BillerProductCode { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? PaymentMethod { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? LastFourAccountNumber { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public decimal PaymentAmount { get; set; } = 0;

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public decimal ConvenienceFee { get; set; } = 0;

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? BankRoutingNumber { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? BankAccountType { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// BillerProductCode
    /// </summary>
    public string? CardType { get; set; }
}
