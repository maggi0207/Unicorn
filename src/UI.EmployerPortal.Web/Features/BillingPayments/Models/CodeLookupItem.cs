namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Maps the common WCF response item shape returned by
/// <c>GetUSStateCodes</c> and <c>GetCountryCodes</c>.
/// Each item has a numeric key, a long display name, and a short code.
/// </summary>
public sealed class CodeLookupItem
{
    /// <summary>Numeric surrogate key from the WCF service (e.g. 2, 73, 231).</summary>
    public int CodeSK { get; init; }

    /// <summary>Full display name (e.g. "Alaska", "UNITED ARAB EMIRATES").</summary>
    public string LongDescription { get; init; } = string.Empty;

    /// <summary>Short code / abbreviation (e.g. "AK", "AE").</summary>
    public string ShortDescription { get; init; } = string.Empty;
}
