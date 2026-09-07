namespace UI.EmployerPortal.Web.Features.ESP.Models;

/// <summary>
/// View model for a single row in the ESP Make ACH Payment pending-files table.
/// </summary>
public sealed record PendingTaxFilePayment
{
    /// <summary>
    /// Surrogate key identifying the uploaded file, used to navigate to the Tax File Upload Summary page.
    /// </summary>
    public int FileUploadDetailSk { get; init; }

    /// <summary>
    /// Date and time the file was uploaded.
    /// </summary>
    public DateTime UploadDate { get; init; }

    /// <summary>
    /// Original name of the uploaded file.
    /// </summary>
    public string FileName { get; init; } = string.Empty;

    /// <summary>
    /// Unique confirmation identifier assigned to the file upload.
    /// </summary>
    public string ConfirmationId { get; init; } = string.Empty;

    /// <summary>
    /// Count of reports in the file that passed review without errors.
    /// </summary>
    public int ReportsWithoutErrors { get; init; }

    /// <summary>
    /// Total payment amount included in the file, pending EFT payment initiation.
    /// </summary>
    public decimal PaymentAmount { get; init; }
}
