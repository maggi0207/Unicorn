using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
/// <summary>
/// Represents an owner Agency
/// </summary>
public class OwnershipAgency
{
    /// <summary>
    /// File check
    /// </summary>
    public bool HasFile { get; set; } = false;
    /// <summary>
    /// No File 
    /// </summary>
    public bool NoHasFile { get; set; } = false;
    /// <summary>
    /// UploadedFileName
    /// </summary>
    public string? Filepath { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? FileMIMEType { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public FileUploadState? FileUploadState { get; set; }
}
