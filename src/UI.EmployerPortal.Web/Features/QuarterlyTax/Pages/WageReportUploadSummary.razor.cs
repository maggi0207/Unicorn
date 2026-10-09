using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Represents the WageReportUploadSummary page.
/// Displays a list of wage files uploaded by the authenticated employer user
/// </summary>
public partial class WageReportUploadSummary
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IWageFileUploadDetailService WageFileUploadDetailService { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    private IEnumerable<WageFileModel> _gridData = new List<WageFileModel>();

    private string _sortColumn = "uploadDate";

    private bool _sortAscending = false;

    private bool _isSourceTestEnv;
    /// <summary>
    /// Initializes the component and loads the wage file data.
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);

        if (_isSourceTestEnv)
        {
            var data = await WageFileUploadDetailService.LoadTestWageFileUploads();
            _gridData = PerformSort(_sortColumn, _sortAscending, data).ToList();
        }
        else
        {
            var data = await WageFileUploadDetailService.LoadWageFileUploads();
            _gridData = PerformSort(_sortColumn, _sortAscending, data).ToList();
        }

    }

    private void Sort(string column)
    {
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }

        _gridData = PerformSort(_sortColumn, _sortAscending, _gridData).ToList();
    }

    private IEnumerable<WageFileModel> PerformSort(string column, bool asc, IEnumerable<WageFileModel> data)
    {
        Func<WageFileModel, object> order = column switch
        {
            "uploadDate" => x =>
            {
                return x.UploadDate;
            }
            ,
            "contactName" => x =>
            {
                return x.ContactName ?? "";
            }
            ,
            "fileName" => x =>
            {
                return x.FileName ?? "";
            }
            ,
            "fileType" => x =>
            {
                return x.FileType ?? "";
            }
            ,
            "confirmation" => x =>
            {
                return x.ConfirmationNumber ?? "";
            }
            ,
            "error" => x =>
            {
                return x.ContactName ?? "";
            }
            ,
            "errorCount" => x =>
            {
                return x.ErrorCount;
            }
            ,
            "processedDate" => x =>
            {
                return x.ProcessedDate ?? DateTime.MinValue;
            }
            ,
            _ => x =>
            {
                return x.UploadDate;
            }
        };
        return asc ? data.OrderBy(order) : data.OrderByDescending(order);
    }
    private void NavigateToDetails(WageFileModel file)
    {
        NavigationManager.NavigateTo(TestEnvironmentSource.Preserve($"quarterly-tax/wage-upload-details?fileUploadSK={file.FileUploadSK}", _isSourceTestEnv));
    }
    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }
    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private MarkupString GetSortIcon(string column)
    {
        string path;
        string altText;

        if (_sortColumn == column)
        {
            path = _sortAscending ? "images/sort/sort-icon-desc.svg" : "images/sort/sort-icon-asc.svg";
            altText = _sortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }
        return GetImageAndAltText(path, altText);
    }
    private MarkupString GetImageAndAltText(string path, string altText)
    {
        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }
}

/// <summary>
/// Represents a wage file uploaded by the employer
/// </summary>
public class WageFileModel
{
    /// <summary>
    /// Gets or sets the date and time when the file was uploaded.
    /// </summary>
    public DateTime UploadDate { get; set; }

    /// <summary>
    /// Gets or sets the name of the contact associated with the file upload.
    /// </summary>
    public string? ContactName { get; set; }

    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public string? FileName { get; set; }

    /// <summary>
    /// Gets or sets the type/format of teh uploaded file.
    /// </summary>
    public string? FileType { get; set; }

    /// <summary>
    /// Gets or sets the confirmation number assigned to the file upload.
    /// </summary>
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Gets or sets the number of error identified in the uploaded files.
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the date when the file was processed.
    /// </summary>
    public DateTime? ProcessedDate { get; set; }

    /// <summary>
    /// Gets or sets the file upload sk
    /// </summary>
    public int FileUploadSK { get; set; }
}
