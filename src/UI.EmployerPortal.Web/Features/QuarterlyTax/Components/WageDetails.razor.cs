using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

/// <summary>
/// Collapsible Wage Details component for displaying wage information in verification and review pages.
/// </summary>
public partial class WageDetails
{
    /// <summary>
    /// Gets or sets the wage report data to display
    /// </summary>
    [Parameter]
    public WageDetailsModel? WageData { get; set; }

    /// <summary>
    /// Event callback invoked when the Edit link is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnEdit { get; set; }

    /// <summary>
    /// Gets Or Sets Confirmation Number
    /// If null or empty, nothing is rendered. Other Usages are unaffected.
    /// </summary>
    [Parameter]
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Gets Or Sets whether the Edit button is shown. Defaults to true.
    /// Set to false on read-only pages. Other Usages are unaffected.
    /// </summary>
    [Parameter]
    public bool ShowEdit { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the component is initially expanded.
    /// </summary>
    [Parameter]
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Captures additional attributes (like style, class, id) to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _isExpanded;
    private string _sortColumn = "LastName";
    private bool _sortAscending = true;
    private readonly HashSet<string> _visibleSSNs = new();
    private WageDetailsModel? _previousWageData;
    private bool _showAllSSNs = false;
    private int _currentPage = 1;
    private int _pageSize = 50;

    private List<EmployeeWageData>? _sortedEmployees;

    private bool _isPreparingPrint;

    private int TotalEmployeeCount => WageData?.Employees?.Count ?? 0;
    private int TotalPages => TotalEmployeeCount == 0 ? 1 : (int) Math.Ceiling(TotalEmployeeCount / (double) _pageSize);
    private int PaginationStart => TotalEmployeeCount == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;
    private int PaginationEnd => Math.Min(_currentPage * _pageSize, TotalEmployeeCount);

    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override void OnInitialized()
    {
        _isExpanded = IsExpanded;
    }

    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        if (WageData != _previousWageData)
        {
            _previousWageData = WageData;
            _sortedEmployees = null;
        }
    }

    /// <summary>
    /// Renders every employee, then invoke <paramref name="print"/>, then drops them again.
    /// </summary>
    /// <param name="print"></param>
    /// <returns></returns>
    public async Task PrintAsync(Func<Task> print)
    {
        _isPreparingPrint = true;
        StateHasChanged();

        await Task.Yield();

        try
        {
            await print();
        }
        finally
        {
            _isPreparingPrint = false;
            StateHasChanged();
        }
    }

    private IEnumerable<EmployeeWageData> GetSortedEmployees() =>
        _sortedEmployees ??= [.. SortedEmployees()];

    private void ToggleAllSSNs()
    {
        _showAllSSNs = !_showAllSSNs;
        if (_showAllSSNs)
        {
            var allSSNs = WageData?.Employees?.Select(e =>
            {
                return e.SSN;
            }) ?? Enumerable.Empty<string>();

            foreach (var ssn in allSSNs)
            {
                _visibleSSNs.Add(ssn);
            }
        }
        else
        {
            _visibleSSNs.Clear();
        }
    }

    private void ToggleExpand()
    {
        _isExpanded = !_isExpanded;
    }

    private async Task HandleEditClick()
    {
        await OnEdit.InvokeAsync();
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
        _sortedEmployees = null;
    }

    private IEnumerable<EmployeeWageData> SortedEmployees()
    {
        if (WageData?.Employees == null)
        {
            return Enumerable.Empty<EmployeeWageData>();
        }

        var employees = WageData.Employees.AsEnumerable();

        employees = _sortColumn switch
        {
            "LastName" => _sortAscending
                ? employees.OrderBy(e =>
                {
                    return e.LastName;
                })
                : employees.OrderByDescending(e =>
                {
                    return e.LastName;
                }),
            "FirstName" => _sortAscending
                ? employees.OrderBy(e =>
                {
                    return e.FirstName;
                })
                : employees.OrderByDescending(e =>
                {
                    return e.FirstName;
                }),
            "SSN" => _sortAscending
                ? employees.OrderBy(e =>
                {
                    return e.SSN;
                })
                : employees.OrderByDescending(e =>
                {
                    return e.SSN;
                }),
            "QuarterlyWages" => _sortAscending
                ? employees.OrderBy(e =>
                {
                    return e.QuarterlyWages;
                })
                : employees.OrderByDescending(e =>
                {
                    return e.QuarterlyWages;
                }),
            "SaveForNextQuarter" => _sortAscending
                ? employees.OrderBy(e =>
                {
                    return e.SaveForNextQuarter;
                })
                : employees.OrderByDescending(e =>
                {
                    return e.SaveForNextQuarter;
                }),
            _ => employees
        };

        return employees;
    }

    private IEnumerable<EmployeeWageData> GetPagedEmployees()
    {
        return GetSortedEmployees().Skip((_currentPage - 1) * _pageSize).Take(_pageSize);
    }

    private void HandlePageSizeChange(ChangeEventArgs e)
    {
        _pageSize = int.Parse(e.Value?.ToString() ?? "50");
        _currentPage = 1;
    }

    private void GoToFirstPage() { _currentPage = 1; }
    private void GoToPreviousPage()
    {
        if (_currentPage > 1)
        {
            _currentPage--;
        }
    }
    private void GoToNextPage()
    {
        if (_currentPage < TotalPages)
        {
            _currentPage++;
        }
    }
    private void GoToLastPage() { _currentPage = TotalPages; }

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

    private void ToggleSSNVisibility(string ssn)
    {
        if (_visibleSSNs.Contains(ssn))
        {
            _visibleSSNs.Remove(ssn);
        }
        else
        {
            _visibleSSNs.Add(ssn);
        }
    }

    private bool IsSSNVisible(string ssn)
    {
        return _visibleSSNs.Contains(ssn);
    }

    private string GetDisplaySSN(string ssn)
    {
        return string.IsNullOrWhiteSpace(ssn) ? string.Empty : IsSSNVisible(ssn) ? FormatSSN(ssn) : MaskSSN(ssn);
    }

    private string FormatSSN(string ssn)
    {
        // Remove any non-digit characters
        var digits = new string(ssn.Where(char.IsDigit).ToArray());

        if (digits.Length < 9)
        {
            return ssn; // Return as-is if not enough digits
        }

        // Format as XXX-XX-XXXX
        return $"{digits[..3]}-{digits.Substring(3, 2)}-{digits.Substring(5, 4)}";
    }

    private string MaskSSN(string ssn)
    {
        if (string.IsNullOrWhiteSpace(ssn))
        {
            return string.Empty;
        }

        // Remove any non-digit characters
        var digits = new string(ssn.Where(char.IsDigit).ToArray());

        if (digits.Length < 4)
        {
            return "•••-••-••••";
        }

        // Mask format: ••• •• - last 4 digits
        var lastFour = digits[^4..];
        return $"••• •• - {lastFour}";
    }
}

/// <summary>
/// Data model for Wage Details component
/// </summary>
public class WageDetailsModel
{
    /// <summary>
    /// List of employee wage data
    /// </summary>
    public List<EmployeeWageData> Employees { get; set; } = new();

    /// <summary>
    /// Total gross covered wages for the quarter (calculated from employee wages)
    /// </summary>
    public decimal TotalGrossCoveredWages => Employees?.Sum(e =>
    {
        return e.QuarterlyWages;
    }) ?? 0;

    /// <summary>
    /// True when the wage report's total gross wages don't match the previously filed tax report.
    /// </summary>
    public bool IsOutOfBalance { get; set; }

    /// <summary>
    /// Gross wages from the previously filed tax report, used for the out-of-balance comparison display.
    /// </summary>
    public decimal TaxReportGrossWages { get; set; }
}

/// <summary>
/// Employee wage data
/// </summary>
public class EmployeeWageData
{
    /// <summary>
    /// Employee's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Employee's Social Security Number
    /// </summary>
    public string SSN { get; set; } = string.Empty;

    /// <summary>
    /// Quarterly wages for the employee
    /// </summary>
    public decimal QuarterlyWages { get; set; }

    /// <summary>
    /// Whether to save this employee for the next quarter
    /// </summary>
    public string SaveForNextQuarter { get; set; } = string.Empty;
}
