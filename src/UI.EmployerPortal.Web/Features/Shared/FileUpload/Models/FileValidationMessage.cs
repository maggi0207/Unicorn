namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

/// <summary>
/// An error or warning from file validation
/// </summary>
public class FileValidationMessage
{
    /// <summary>
    /// error or warning message 
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// boolen flag to drive if it's error or warning.
    /// </summary>
    public bool IsError { get; set; }
}
