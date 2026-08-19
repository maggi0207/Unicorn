//using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// PaymentType
/// </summary>
public enum PaymentType
{
    /// <summary>Payment</summary>
    Payment = 1,
    /// <summary>VoluntaryContribution</summary>
    VoluntaryContribution = 2,
    /// <summary>SinglePaymentEFT</summary>
    SinglePaymentEFT = 3,
    /// <summary>LevyACH</summary>
    LevyACH = 4
}
