using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code behind for Wage File Upload Details page
/// Responsible for retrieving and exposing wage upload details data.
/// </summary>
public partial class WageFileUploadDetails
{
    /// <summary>
    /// File upload sk
    /// </summary>
    [Parameter]
    [SupplyParameterFromQuery]
    public int FileUploadSK { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IWageFileUploadDetailService WageFileUploadDetailService { get; set; } = default!;

    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    /// <summary>
    /// Confirmation Number
    /// </summary>
    public string? ConfirmationNumber { get; set; }

    private bool _isSourceTestEnv;

    /// <summary>
    /// Gets or sets the model bound to the UI
    /// </summary>
    protected WageUploadDetails Model { get; set; } = new();

    /// <summary>
    /// Loads wage file details based on FileUploadsk
    /// </summary>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isSourceTestEnv = TestEnvironmentSource.IsTest(NavigationManager);
        Model = await WageFileUploadDetailService.LoadWageFileUploadDetails(FileUploadSK) ?? new WageUploadDetails();
    }

    //Pagination Logic
    //Fatal Errors
    private int _fatalCurrentPage = 1;
    private int _fatalPageSize = 5;
    private string _fatalSortColumn = "uiAccount";
    private bool _fatalSortAsc = true;
    private int FatalStart => Model.FatalErrorRows.Count > 0 ? ((_fatalCurrentPage - 1) * _fatalPageSize) + 1 : 0;
    private int FatalEnd => Math.Min(_fatalCurrentPage * _fatalPageSize, Model.FatalErrorRows.Count);
    private int FatalTotalPages => (int) Math.Ceiling((double) Model.FatalErrorRows.Count / _fatalPageSize);
    private IEnumerable<WageUploadErrorRow> PagedFatalErrors
       => GetSortedFatal()
           .Skip((_fatalCurrentPage - 1) * _fatalPageSize)
           .Take(_fatalPageSize);
    private IEnumerable<WageUploadErrorRow> GetSortedFatal()
    {
        return _fatalSortColumn switch
        {
            "uiAccount" => SortFatal(x =>
            {
                return x.UIAccountNumber;
            }),
            "fein" => SortFatal(x =>
            {
                return x.FEIN;
            }),
            "quarterYear" => SortFatal(x =>
            {
                return x.QuarterYear;
            }),
            "ssn" => SortFatal(x =>
            {
                return x.SSN;
            }),
            "errorNumber" => SortFatal(x =>
            {
                return x.ErrorNumber;
            }),
            "description" => SortFatal(x =>
            {
                return x.Description;
            }),
            "data" => SortFatal(x =>
            {
                return x.Data;
            }),
            "line" => SortFatal(x =>
            {
                return x.LineNumber;
            }),
            _ => SortFatal(x =>
            {
                return x.UIAccountNumber;
            })
        };
    }
    private IOrderedEnumerable<WageUploadErrorRow> SortFatal<TKey>(Func<WageUploadErrorRow, TKey> key)
    {
        return _fatalSortAsc
            ? Model.FatalErrorRows.OrderBy(key)
            : Model.FatalErrorRows.OrderByDescending(key);
    }
    private void SortFatal(string column)
    {
        if (_fatalSortColumn == column)
        {
            _fatalSortAsc = !_fatalSortAsc;
        }
        else
        {
            _fatalSortColumn = column;
            _fatalSortAsc = true;
        }
        _fatalCurrentPage = 1;
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            SortFatal(column);
        }
    }

    private void HandleFatalPageSizeChanged(ChangeEventArgs e)
    {
        _fatalPageSize = int.Parse(e.Value?.ToString() ?? "10");
        _fatalCurrentPage = 1;
    }
    private void FatalFirst()
    {
        _fatalCurrentPage = 1;
    }

    private void FatalLast()
    {
        _fatalCurrentPage = FatalTotalPages;
    }

    private void FatalNext()
    {
        if (_fatalCurrentPage < FatalTotalPages)
        {
            _fatalCurrentPage++;
        }
    }
    private void FatalPrev()
    {
        if (_fatalCurrentPage > 1)
        {
            _fatalCurrentPage--;
        }
    }

    private MarkupString GetFatalSortIcon(string column)
    {
        var path = _fatalSortColumn == column
            ? (_fatalSortAsc ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg")
            : "images/sort/sort-icon.svg";
        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' />");
    }
    private string? GetFatalAriaSort(string column)
    {
        return _fatalSortColumn != column ? null : _fatalSortAsc ? "ascending" : "descending";
    }

    //Non Fatal Errors
    private int _nonFatalCurrentPage = 1;
    private int _nonFatalPageSize = 5;
    private string _nonFatalSortColumn = "uiAccount";
    private bool _nonFatalSortAscending = true;
    private int NonFatalStart => Model.NonFatalErrorRows.Count() > 0 ? ((_nonFatalCurrentPage - 1) * _nonFatalPageSize) + 1 : 0;
    private int NonFatalEnd => Math.Min(_nonFatalCurrentPage * _nonFatalPageSize, Model.NonFatalErrorRows.Count());
    private int NonFatalTotalPages => (int) Math.Ceiling((double) Model.NonFatalErrorRows.Count() / _nonFatalPageSize);

    private IEnumerable<WageUploadErrorRow> PagedNonFatalErrors
       => GetSortedNonFatal()
       .Skip((_nonFatalCurrentPage - 1) * _nonFatalPageSize)
       .Take(_nonFatalPageSize);

    private IEnumerable<WageUploadErrorRow> GetSortedNonFatal()
    {
        var sorted = _nonFatalSortColumn switch
        {
            "uiAccount" => SortNonFatal(x =>
            {
                return x.UIAccountNumber;
            }),
            "fein" => SortNonFatal(x =>
            {
                return x.FEIN;
            }),
            "quarterYear" => SortNonFatal(x =>
            {
                return x.QuarterYear;
            }),
            "ssn" => SortNonFatal(x =>
            {
                return x.SSN;
            }),
            "errorNumber" => SortNonFatal(x =>
            {
                return x.ErrorNumber;
            }),
            "description" => SortNonFatal(x =>
            {
                return x.Description;
            }),
            "data" => SortNonFatal(x =>
            {
                return x.Data;
            }),
            "line" => SortNonFatal(x =>
            {
                return x.LineNumber;
            }),
            _ => SortNonFatal(x =>
            {
                return x.UIAccountNumber;
            })
        };
        return sorted;
    }
    private IOrderedEnumerable<WageUploadErrorRow> SortNonFatal<TKey>(Func<WageUploadErrorRow, TKey> keySelector)
    {
        return _nonFatalSortAscending
            ? Model.NonFatalErrorRows.OrderBy(keySelector)
            : Model.NonFatalErrorRows.OrderByDescending(keySelector);
    }
    private void SortNonFatal(string column)
    {
        if (_nonFatalSortColumn == column)
        {
            _nonFatalSortAscending = !_nonFatalSortAscending;
        }
        else
        {
            _nonFatalSortColumn = column;
            _nonFatalSortAscending = true;
        }
        _nonFatalCurrentPage = 1;
    }

    private void HandleNonFatalHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            SortNonFatal(column);
        }
    }

    private void HandleNonFatalPageSizeChanged(ChangeEventArgs e)
    {
        _nonFatalPageSize = int.Parse(e.Value?.ToString() ?? "10");
        _nonFatalCurrentPage = 1;
    }
    private void NonFatalFirstPage()
    {
        if (_nonFatalCurrentPage > 1)
        {
            _nonFatalCurrentPage = 1;
        }
    }
    private void NonFatalPreviousPage()
    {
        if (_nonFatalCurrentPage > 1)
        {
            _nonFatalCurrentPage--;
        }
    }
    private void NonFatalNextPage()
    {
        if (_nonFatalCurrentPage < NonFatalTotalPages)
        {
            _nonFatalCurrentPage++;
        }
    }
    private void NonFatalLastPage()
    {
        if (_nonFatalCurrentPage < NonFatalTotalPages)
        {
            _nonFatalCurrentPage = NonFatalTotalPages;
        }
    }
    private MarkupString GetNonFatalSortIcon(string column)
    {
        string path;
        string altText;
        if (_nonFatalSortColumn == column)
        {
            path = _nonFatalSortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _nonFatalSortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }
        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }
    private string? GetNonFatalAriaSort(string column)
    {
        return _nonFatalSortColumn != column ? null : _nonFatalSortAscending ? "ascending" : "descending";
    }
}

