//using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// AchPaymentFormModel
/// </summary>
public class AchPaymentFormModel
{
    /// <summary>
    /// Amount
    /// </summary>
    //[Required(ErrorMessage = "Payment Amount is required.")]
    public string? Amount { get; set; }

    /// <summary>Settlement Date</summary>
    public string? SettlementDate { get; set; }

    /// <summary>Validated Amount</summary>
    public decimal? ValidatedAmount { get; set; }

    /// <summary>Validated Settlement Date</summary>
    public DateOnly? ValidatedSettlementDate { get; set; }
}
