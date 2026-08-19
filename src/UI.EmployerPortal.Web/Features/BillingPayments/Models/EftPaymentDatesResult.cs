//using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// 
/// </summary>
public sealed class EftPaymentDatesResult
{
    /// <summary>
    /// BankHolidays
    /// </summary>
    public IReadOnlyList<DateOnly> BankHolidays { get; set; } = [];
    /// <summary>
    /// FirstAvailableSettlementDate
    /// </summary>
    public DateOnly FirstAvailableSettlementDate { get; set; }
}
