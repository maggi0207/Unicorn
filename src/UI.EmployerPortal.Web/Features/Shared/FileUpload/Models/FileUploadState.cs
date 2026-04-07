namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

/// <summary>
/// Represents the current display state of the FileUpload component.
/// </summary>
public enum FileUploadState
{
    /// <summary>
    /// No file has been selected; shows the drag-and-drop zone.
    /// </summary>
    Default,

    /// <summary>
    /// A file is actively being uploaded; shows the progress bar.
    /// </summary>
    Uploading,

    /// <summary>
    /// The upload completed successfully; shows re-upload/download/remove actions.
    /// </summary>
    Successful,

    /// <summary>
    /// The upload completed but validation rules were violated; shows error banner.
    /// </summary>
    ErrorsMustFix
}
