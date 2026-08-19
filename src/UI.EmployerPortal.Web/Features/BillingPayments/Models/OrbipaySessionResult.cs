namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>Result from creating an Orbipay hosted form session.</summary>
public sealed record OrbipaySessionResult
{
    /// <summary>True if session created successfully.</summary>
    public bool Success { get; init; }

    /// <summary>HTML markup containing the Orbipay form and script tag.</summary>
    public string? HostedFormHtml { get; init; }

    /// <summary>Error message if creation failed.</summary>
    public string? ErrorMessage { get; init; }
}
