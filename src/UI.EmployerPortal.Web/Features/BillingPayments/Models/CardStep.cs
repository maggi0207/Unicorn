namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Specifies the steps in the Credit/Debit Card payment workflow.
/// </summary>
public enum CardStep
{
    /// <summary>Step 1 — User fills in billing/contact registration profile.</summary>
    Registration = 0,

    /// <summary>Step 2 — User reviews entered information before launching the payment modal.</summary>
    Verification = 1,

    /// <summary>Step 3 — Confirmation shown after successful card payment.</summary>
    Confirmation = 2
}
