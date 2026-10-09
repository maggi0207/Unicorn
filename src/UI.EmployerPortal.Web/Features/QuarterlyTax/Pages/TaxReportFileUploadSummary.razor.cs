using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Represents the TaxReportFileSummary page.
/// Displays a list of tax report files uploaded by the authenticated employer user
/// </summary>
public partial class TaxReportFileUploadSummary
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ITaxFileUploadDetailService TaxFileUploadDetailService { get; set; } = default!;
    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    private IEnumerable<TaxReportFileModel> _gridData = new List<TaxReportFileModel>();
    private bool _isSourceTestEnv;
    private string _sortColumn = "uploadDate";
    private bool _sortAscending = false;
    /// <summary>
    /// 
    /// </summary>
    protected TaxReportFileModel Model { get; set; } = new();
    //Pagination
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int StartRow => _gridData.Any() ? ((_currentPage - 1) * _pageSize) + 1 : 0;
    private int EndRow => Math.Min(_currentPage * _pageSize, _gridData.Count());
    private int TotalPages => (int) Math.Ceiling((double) _gridData.Count() / _pageSize);
    private IEnumerable<TaxReportFileModel> PagedGridData
           => _gridData
               .Skip((_currentPage - 1) * _pageSize)
               .Take(_pageSize);
    /// <summary>
    /// Initializes the component and loads the wage file data.
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);

        if (_isSourceTestEnv)
        {
            var data = await TaxFileUploadDetailService.LoadTestTaxFileUploads();
            _gridData = PerformSort(_sortColumn, _sortAscending, data).ToList();
        }
        else
        {
            var data = await TaxFileUploadDetailService.LoadTaxFileUploads();
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
        _currentPage = 1;
        _gridData = PerformSort(_sortColumn, _sortAscending, _gridData).ToList();
    }
    private IEnumerable<TaxReportFileModel> PerformSort(string column, bool asc, IEnumerable<TaxReportFileModel> data)
    {
        Func<TaxReportFileModel, object> order = column switch
        {
            "fileName" => x =>
            {
                return x.FileName ?? "";
            }
            ,
            "uploadDate" => x =>
            {
                return x.UploadDate;
            }
            ,
            "confirmationNumber" => x =>
            {
                return x.ConfirmationNumber ?? "";
            }
            ,
            "fileStatus" => x =>
            {
                return x.FileStatus ?? "";
            }
            ,
            "statusDateTime" => x =>
            {
                return x.StatusDate ?? DateTime.MinValue;
            }
            ,
            "reportCount" => x =>
            {
                return x.ReportCount;
            }
            ,
            "errorCount" => x =>
            {
                return x.ErrorCount;
            }
            ,
            _ => x =>
            {
                return x.UploadDate;
            }
        };
        return asc ? data.OrderBy(order) : data.OrderByDescending(order);
    }
    private void NavigateToDetails(TaxReportFileModel file)
    {
        NavigationManager.NavigateTo(
            TestEnvironmentSource.Preserve($"quarterly-tax/tax-upload-details?fileUploadSK={file.FileUploadSK}", _isSourceTestEnv));
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
    private void HandlePageSizeChanged(ChangeEventArgs e)
    {
        _pageSize = int.Parse(e.Value?.ToString() ?? "10");
        _currentPage = 1;
    }
    private void FirstPage()
    {
        _currentPage = 1;
    }
    private void LastPage()
    {
        _currentPage = TotalPages;
    }
    private void NextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
        }
    }
    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
        }
    }
}
/// <summary>
/// Represents a wage file uploaded by the employer
/// </summary>
public class TaxReportFileModel
{
    /// <summary>
    /// Gets or sets the name of the uploaded file.
    /// </summary>
    public string? FileName { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the file was uploaded.
    /// </summary>
    public DateTime UploadDate { get; set; }
    /// <summary>
    /// Gets or sets the status of the uploaded file.
    /// </summary>
    public string? FileStatus { get; set; }
    /// <summary>
    /// Gets or sets the confirmation number assigned to the file upload.
    /// </summary>
    public string? ConfirmationNumber { get; set; }
    /// <summary>
    /// Gets or sets the date and time when the file status is set.
    /// </summary>
    public DateTime? StatusDate { get; set; }
    /// <summary>
    /// Gets or sets the number of error identified in the uploaded files.
    /// </summary>
    public int ErrorCount { get; set; }
    /// <summary>
    /// Gets or sets the number of reports identified in the uploaded files.
    /// </summary>
    public int ReportCount { get; set; }
    /// <summary>
    /// Gets or sets the file upload sk
    /// </summary>
    public int FileUploadSK { get; set; }
}

