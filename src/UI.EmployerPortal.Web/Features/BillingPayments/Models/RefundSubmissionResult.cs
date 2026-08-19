namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Result of a refund request submission.
/// </summary>
/// <param name="Success">Whether the refund request was submitted successfully.</param>
/// <param name="ErrorMessage">Error message to display when <paramref name="Success"/> is false.</param>
public record RefundSubmissionResult(bool Success, string? ErrorMessage);
