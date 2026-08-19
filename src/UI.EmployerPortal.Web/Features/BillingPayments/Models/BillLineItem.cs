namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Represents a single row in the billing table or credits table.
/// </summary>
public class BillLineItem
{
    /// <summary>Tax year.</summary>
    public int? Year { get; set; }

    /// <summary>Quarter number.</summary>
    public int? Quarter { get; set; }

    /// <summary>Bill type description (e.g. Tax, Interest, Penalty).</summary>
    public string BillTypeDescription { get; set; } = string.Empty;

    /// <summary>
    /// Due date. Displayed as "Date Due" in the billing table and "Effective Date" in the credits table.
    /// </summary>
    public DateTime? DueDate { get; set; }

    /// <summary>Balance amount (includes interest for bill rows).</summary>
    public decimal? Balance { get; set; }

    /// <summary>Balance amount excluding interest. Displayed as "Amount" in the billing table.</summary>
    public decimal? BalanceWithoutInterest { get; set; }

    /// <summary>Interest due amount. Used in the billing table Interest column only.</summary>
    public decimal? InterestDueAmount { get; set; }
}
