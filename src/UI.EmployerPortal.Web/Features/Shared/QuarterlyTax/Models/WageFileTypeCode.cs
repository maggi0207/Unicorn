namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// File type codes for wage file uploads
/// </summary>
public enum WageFileTypeCode
{
    /// <summary>
    /// Original wae report upload
    /// </summary>
    WageOriginal = 1,

    /// <summary>
    /// Append additional emplooyees to an existing wage report. 
    /// </summary>
    WageAppend = 5,

    /// <summary>
    /// Replace an existing wage report entirely.
    /// </summary>
    WageReplace = 6
}
