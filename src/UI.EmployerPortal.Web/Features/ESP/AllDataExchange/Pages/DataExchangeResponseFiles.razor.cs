using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Models;
using UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Services;


namespace UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Pages;

/// <summary>
/// Represents the DataExchangeResponseFiles page.
/// Displays a list of tax data exchange response files uploaded by the authenticated employer/ESP user.
/// </summary>

public partial class DataExchangeResponseFiles
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IESPDataExchangeService ESPDataExchangeService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    private List<DataExchangeResponseModel> _allData = new();

    private IEnumerable<DataExchangeResponseModel> _gridData = new List<DataExchangeResponseModel>();

    private string _sortColumn = "uploadDate";

    private bool _sortAscending = false;

    private bool _isSourceTestEnv;

    private static readonly int[] PageSizeOptions = { 10, 25, 50 };

    private int _pageSize = 10;

    private int _currentPage = 1;

    private int _totalItems;

    private int TotalPages => _pageSize == 0 ? 1 : (int) Math.Ceiling((double) _totalItems / _pageSize);

    /// <summary>
    /// Initializes the component and loads the response files data.
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = false;
        var uri = NavigationManager.ToAbsoluteUri(NavigationManager.Uri);
        var queryStrings = QueryHelpers.ParseQuery(uri.Query);
        if (queryStrings.TryGetValue("source", out var source))
        {
            if (source == "test-environment")
            {
                _isSourceTestEnv = true;
            }
        }

        if (_isSourceTestEnv)
        {
            //var data = await TaxFileUploadDetailService.LoadTestTaxFileUploads();
            //_allData = PerformSort(_sortColumn, _sortAscending, data).ToList();
        }
        else
        {
            var data = await ESPDataExchangeService.GetDataExchangeResponseFilesAsync();
            _allData = PerformSort(_sortColumn, _sortAscending, data).ToList();
        }

        _totalItems = _allData.Count;
        ApplyPaging();
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

        _allData = PerformSort(_sortColumn, _sortAscending, _allData).ToList();
        _currentPage = 1;
        ApplyPaging();
    }

    private IEnumerable<DataExchangeResponseModel> PerformSort(string column, bool asc, IEnumerable<DataExchangeResponseModel> data)
    {
        Func<DataExchangeResponseModel, object> order = column switch
        {
            "requestfileName" => x =>
            {
                return x.RequestFileName ?? "";
            }
            ,
            "uploadDate" => x =>
            {
                return x.UploadDateTime;
            }
            ,
            "recordCount" => x =>
            {
                return x.RecordCount;
            }
            ,
            "confirmation" => x =>
            {
                return x.ConfirmationID ?? "";
            }
            ,
            "responsefileName" => x =>
            {
                return x.ResponseFileName ?? "";
            }
            ,

            _ => x =>
            {
                return x.UploadDateTime;
            }
        };
        return asc ? data.OrderBy(order) : data.OrderByDescending(order);
    }

    private void ApplyPaging()
    {
        _gridData = _allData
            .Skip((_currentPage - 1) * _pageSize)
            .Take(_pageSize)
            .ToList();
    }

    private void OnPageSizeChanged(ChangeEventArgs e)
    {
        if (int.TryParse(e.Value?.ToString(), out var newSize))
        {
            _pageSize = newSize;
            _currentPage = 1;
            ApplyPaging();
        }
    }

    private void GoToFirstPage()
    {
        _currentPage = 1;
        ApplyPaging();
    }

    private void GoToPreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
            ApplyPaging();
        }
    }

    private void GoToNextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
            ApplyPaging();
        }
    }

    private void GoToLastPage()
    {
        _currentPage = TotalPages;
        ApplyPaging();
    }

    private int PageRangeStart => _totalItems == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;

    private int PageRangeEnd => Math.Min(_currentPage * _pageSize, _totalItems);

    private async Task DownloadFile(DataExchangeResponseModel file)
    {

        if (file == null)
        {
            return;
        }

        var request = new DownloadESPDXFileRequestModel
        {
            FileUploadDetailSK = (int) file.FileUploadSK

        };


        var fileResponse = await ESPDataExchangeService.DownloadESPDXFileAsync(request);
        if (fileResponse == null)
        {
            return;
        }
        var stream = new MemoryStream(fileResponse.File);
        using var streamRef = new DotNetStreamReference(stream);
        await JS.InvokeVoidAsync("handleFileFromStream", file.RequestFileName, streamRef, false);

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
            path = _sortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
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

    private MarkupString GetPagingIcon(string iconName, bool isDisabled)
    {
        // NOTE: verify this path against the actual wwwroot location of the paging
        // icons (page-first.svg, page-prev.svg, page-next.svg, page-last.svg and
        // their -disabled variants) before relying on this -- confirmed the icon
        // filenames exist in Solution Explorer under a "paging" folder, but not
        // yet confirmed the full relative path under wwwroot/images.
        var path = isDisabled
            ? $"images/paging/{iconName}-disabled.svg"
            : $"images/paging/{iconName}.svg";
        return new MarkupString($"<img src='{Assets[path]}' class='page-icon' alt='' />");
    }
}
