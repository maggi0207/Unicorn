using UI.EmployerPortal.Web.Features.ESP.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>Loads all data needed for the ESP Payment Details page.</summary>
public interface IEspPaymentDetailService
{
    /// <summary>
    /// Returns full payment detail including payment info, bank info, contact info, and activity history.
    /// Returns null on service failure or when the payment is not found.
    /// </summary>
    Task<EspPaymentDetailModel?> GetPaymentDetailAsync(int eftPaymentSk);
}
