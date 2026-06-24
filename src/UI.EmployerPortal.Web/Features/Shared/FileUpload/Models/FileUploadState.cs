namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

/// <summary>
/// Upload State
/// </summary>
public enum FileUploadState
{
    /// <summary>
    /// Status None
    /// </summary>
    Default,
    /// <summary>
    /// Status Uploading
    /// </summary>
    Uploading,
    /// <summary>
    /// Status Uploaded
    /// </summary>
    Successful,
    /// <summary>
    /// 
    /// </summary>
    ErrorsMustFix
}
