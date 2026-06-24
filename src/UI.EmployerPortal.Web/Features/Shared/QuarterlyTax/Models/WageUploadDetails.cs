namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

/// <summary>
/// Represents detailed information about a wage file upload 
/// </summary>
public class WageUploadDetails
{
    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the file upload sk
    /// </summary>
    public int FileUploadSKDetail { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was uploaded.
    /// </summary>
    public DateTime? UploadDate { get; set; }

    /// <summary>
    /// Gets or sets the name of the user who uploaded the file
    /// </summary>
    public string UploadedBy { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the method used to upload the file( e.g,HTTP,FTP)
    /// </summary>
    public string UploadType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the type/format of the uploaded file
    /// </summary>
    public string FileType { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the size of the file(in bytes or defined unit)
    /// </summary>
    public int FileSize { get; set; }

    /// <summary>
    /// Gets or sets the total number of records in the file
    /// </summary>
    public int RecordCount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the file format is valid
    /// </summary>
    public bool IsValidFormat { get; set; }

    /// <summary>
    /// Gets or sets the number of non fatal errorsfound in the file.
    /// </summary>
    public int NonFatalErrors { get; set; }

    /// <summary>
    /// Gets or sets the number of fatal erros found in the file
    /// </summary>
    public int FatalErrors { get; set; }

    /// <summary>
    /// Gets or sets the current processing status of the file 
    /// </summary>
    public string FileStatus { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the number of reports successfully acceptedfor processing
    /// </summary>
    public int ReportsAccepted { get; set; }

    /// <summary>
    /// Gets or sets the number of reports rejected during validation
    /// </summary>
    public int ReportsRejected { get; set; }

    /// <summary>
    /// Error Messages 1
    /// </summary>
    public List<string> FileErrors { get; set; } = new();

    /// <summary>
    ///  Error Messages 2
    /// </summary>
    public List<string> Warnings { get; set; } = new();

    /// <summary>
    /// Fatal error rows
    /// </summary>
    public List<WageUploadErrorRow> FatalErrorRows { get; set; } = new();

    /// <summary>
    /// Non fatal error rows
    /// </summary>
    public List<WageUploadErrorRow> NonFatalErrorRows { get; set; } = new();
}
/// <summary>
/// Represents one row in error table
/// </summary>
public class WageUploadErrorRow
{
    /// <summary>
    /// 1st column in error table
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// 2nd column in error table
    /// </summary>
    public string FEIN { get; set; } = string.Empty;

    /// <summary>
    /// 3rd column in error table
    /// </summary>
    public string QuarterYear { get; set; } = string.Empty;
    /// <summary>
    /// 4th column in error table
    /// </summary>
    public string SSN { get; set; } = string.Empty;

    /// <summary>
    /// 5th column in error table
    /// </summary>
    public int? ErrorNumber { get; set; }

    /// <summary>
    /// 6th column in error table
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// 7th column in error table
    /// </summary>
    public string Data { get; set; } = string.Empty;

    /// <summary>
    /// 8th column in error table
    /// </summary>
    public int LineNumber { get; set; }

}
