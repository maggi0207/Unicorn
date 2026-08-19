using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// View model for the Taxable Billing Detail page.
/// </summary>
public class BillingDetailViewModel
{
    /// <summary>Billing table rows.</summary>
    public List<BillLineItem> Bills { get; set; } = new();

    /// <summary>Credits table rows. Empty when no credits exist.</summary>
    public List<BillLineItem> Credits { get; set; } = new();

    /// <summary>Tax Due summary value.</summary>
    public decimal TaxDue { get; set; }

    /// <summary>Interest Due summary value.</summary>
    public decimal InterestDue { get; set; }

    /// <summary>Penalty Due summary value.</summary>
    public decimal PenaltyDue { get; set; }

    /// <summary>Collection Costs Due summary value.</summary>
    public decimal CollectionCostsDue { get; set; }

    /// <summary>NSF Fees Due summary value.</summary>
    public decimal NsfFeesDue { get; set; }

    /// <summary>Assessment Due summary value.</summary>
    public decimal AssessmentDue { get; set; }

    /// <summary>Total credits amount. Displayed as "Credit" in the Payment summary.</summary>
    public decimal TotalCredits { get; set; }

    /// <summary>Total pending EFT payments.</summary>
    public decimal TotalEftPayments { get; set; }

    /// <summary>Total outstanding balance.</summary>
    public decimal TotalOutstandingBalance { get; set; }

    /// <summary>Pre-populated value for the Amount to Pay field.</summary>
    public decimal AmountToPay { get; set; }

    /// <summary>Rule violations returned from the service.</summary>
    public List<RuleViolationItem> RuleViolations { get; set; } = new();
}
