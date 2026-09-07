namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// Result of loading the ESP pending tax file payments list, including a user-friendly
/// error message when the load fails.
/// </summary>
public sealed record PendingTaxFilePaymentsResult
{
    /// <summary>
    /// The pending file uploads. Empty when there are none or when <see cref="ErrorMessage"/> is set.
    /// </summary>
    public IReadOnlyList<PendingTaxFilePayment> Payments { get; init; } = [];

    /// <summary>
    /// User-friendly message to display when the load failed. Null on success.
    /// </summary>
    public string? ErrorMessage { get; init; }
}
