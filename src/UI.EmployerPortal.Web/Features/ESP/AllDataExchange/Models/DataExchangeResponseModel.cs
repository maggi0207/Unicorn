namespace UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Models;

/// <summary>
/// Represents a single DataExchangeResponseFile in DataExchangeResponseFiles grid.
/// </summary>
public class DataExchangeResponseModel
{
    /// <summary>
    /// Gets or sets the unique identifier for this file upload.
    /// </summary>
    public long FileUploadSK { get; set; }

    /// <summary>
    /// "Request File Name" column.
    /// </summary>
    public string RequestFileName { get; set; } = string.Empty;

    /// <summary>
    /// Upload Date Time.
    /// </summary>
    public DateTime UploadDateTime { get; set; }

    /// <summary>
    /// Record Count
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Confirmation ID.
    /// </summary>
    public string ConfirmationID { get; set; } = string.Empty;

    /// <summary>
    /// "Response File Name" column.
    /// </summary>
    public string ResponseFileName { get; set; } = string.Empty;

}
