using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code-behind for the Previously Filed Reports page.
/// Displays a paginated, sortable grid of the employer's historical quarterly tax and wage report summaries.
/// </summary>
public partial class PreviouslyFiledReports
{
    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    // ── State ──────────────────────────────────────────────────────────────────
    private List<PreviouslyFiledReportModel> _reports = [];
    private bool _isLoading = true;

    // ── Pagination ─────────────────────────────────────────────────────────────
    private int _currentPage = 1;
    private int _pageSize = 20;
    private int PaginationStart => _reports.Count > 0 ? ((_currentPage - 1) * _pageSize) + 1 : 0;
    private int PaginationEnd => Math.Min(_currentPage * _pageSize, _reports.Count);
    private int TotalPages => (int) Math.Ceiling((double) _reports.Count / _pageSize);

    // ── Sorting ────────────────────────────────────────────────────────────────
    private string _sortColumn = "quarterYear";
    private bool _sortAscending = false;

    private IEnumerable<PreviouslyFiledReportModel> PagedReports => GetPagedReports();

    // ── Lifecycle ──────────────────────────────────────────────────────────────
    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _reports = await TaxAndWageEntryService.GetPreviouslyFiledReportsAsync();
        _isLoading = false;
    }

    // ── Data ───────────────────────────────────────────────────────────────────
    private IEnumerable<PreviouslyFiledReportModel> GetPagedReports()
    {
        var sorted = _sortColumn switch
        {
            "quarterYear" => SortBy(r =>
            {
                return (r.Year * 10) + r.Quarter;
            }),
            "wageGrossWages" => SortBy(r =>
            {
                return r.WageReportGrossWages ?? 0m;
            }),
            "taxGrossWages" => SortBy(r =>
            {
                return r.TaxReportGrossWages ?? 0m;
            }),
            "taxExclusions" => SortBy(r =>
            {
                return r.TaxReportExclusions ?? 0m;
            }),
            "taxablePayroll" => SortBy(r =>
            {
                return r.TaxReportTaxablePayroll ?? 0m;
            }),
            _ => SortBy(r =>
            {
                return (r.Year * 10) + r.Quarter;
            }),
        };

        return sorted
            .Skip((_currentPage - 1) * _pageSize)
            .Take(_pageSize);
    }

    private IOrderedEnumerable<PreviouslyFiledReportModel> SortBy<TKey>(
        Func<PreviouslyFiledReportModel, TKey> keySelector)
    {
        return _sortAscending
            ? _reports.OrderBy(keySelector)
            : _reports.OrderByDescending(keySelector);
    }

    // ── Sorting ────────────────────────────────────────────────────────────────
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
    }

    private MarkupString GetSortIcon(string column)
    {
        var path = _sortColumn == column
            ? _sortAscending ? "images/sort/sort-icon-desc.svg" : "images/sort/sort-icon-asc.svg"
            : "images/sort/sort-icon.svg";

        return new MarkupString($"<img aria-hidden='true' src='{Assets[path]}' class='sort-icon' alt='' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    // ── Pagination ─────────────────────────────────────────────────────────────
    private void HandlePageSizeChanged(ChangeEventArgs e)
    {
        _pageSize = int.Parse(e.Value?.ToString() ?? "20");
        _currentPage = 1;
    }

    private void FirstPage()
    {
        if (_currentPage > 1)
        {
            _currentPage = 1;
        }
    }
    private void PreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
        }
    }
    private void NextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
        }
    }
    private void LastPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage = TotalPages;
        }
    }

    // ── Helpers ────────────────────────────────────────────────────────────────
    private static string FormatCurrency(decimal? value)
    {
        return value.HasValue ? value.Value.ToString("C") : "-";
    }

    private void HandleViewDetails(PreviouslyFiledReportModel report)
    {
        NavigationManager.NavigateTo($"quarterly-tax/previously-filed/details?quarter={report.Quarter}&year={report.Year}");
    }
}
