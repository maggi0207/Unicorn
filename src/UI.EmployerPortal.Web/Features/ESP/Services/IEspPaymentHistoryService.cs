using UI.EmployerPortal.Web.Features.ESP.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// Loads and maps ESP tax file EFT payment history for the ESP Payment History page.
/// </summary>
public interface IEspPaymentHistoryService
{
    /// <summary>
    /// Returns all tax file EFT payments for the current ESP user's login ID.
    /// Returns null on service failure.
    /// </summary>
    Task<IReadOnlyList<EspPaymentHistoryItem>?> GetPaymentHistoryAsync();
}
