namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// File processing status codes returned by validation service
/// </summary>
public enum WageFileStatusCode
{
    /// <summary>
    /// File has been received and is awaiting processing. 
    /// </summary>
    Requested = 1,

    /// <summary>
    /// File has been validated and processed successfully.
    /// </summary>
    Processed = 2,

    /// <summary>
    /// File validation failed with errors.
    /// </summary>
    Errored = 3
}
