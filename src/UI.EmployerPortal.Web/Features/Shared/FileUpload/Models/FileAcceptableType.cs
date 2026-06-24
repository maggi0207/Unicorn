namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;

/// <summary>
/// Categories of acceptable file ypes for the fileupload component.
/// Each area of the application uses a different ste of accepted extensions.
/// </summary>
public enum FileAcceptableType
{
    /// <summary>
    /// Wage report file
    /// </summary>
    WageReport,
    /// <summary>
    /// Registration
    /// </summary>
    Registration,
    /// <summary>
    /// Audit file (supporting documents requested during an audit)
    /// </summary>
    Audit,
    /// <summary>
    /// All common file types
    /// </summary>
    All
}

/// <summary>
/// Map <see cref="FileAcceptableType"/> enum values to their accepted file extensions.
/// </summary>
public static class FileTypeDefinitions
{
    private static readonly Dictionary<FileAcceptableType, List<KeyValuePair<string, string>>> FileAcceptableTypes = new()
    {
        [FileAcceptableType.WageReport] =
        [
            new(".txt", "Wisconsin UI Format"),
            new(".txt", "IRS SSA Tape Format"),
            new(".txt", "TRS SSA Disk Format"),
            new(".txt", "Tab-Delimited Text File Format")
        ],
        [FileAcceptableType.Registration] =
        [
            new(".pdf", "Portable Document Format"),
            new(".jpg", "Joint Photographic Experts Group"),
            new(".heic", "High Efficiency Image File Format"),
            new(".png", "Portable Network Format")
        ],
        [FileAcceptableType.All] =
        [
            new(".txt", "Tab-Delimited Text File Format"),
            new(".pdf", "Portable Document Format"),
            new(".jpg", "Joint Photographic Experts Group"),
            new(".heic", "High Efficiency Image File Format"),
            new(".png", "Portable Network Format")
        ]
    };

    /// <summary>
    /// Returns the list of accepted file types for the given category.
    /// </summary>
    /// <param name="type"></param>
    /// <returns></returns>
    public static List<KeyValuePair<string, string>> GetTypes(FileAcceptableType type)
    {
        return FileAcceptableTypes.TryGetValue(type, out var types) ? types : new();
    }
}
