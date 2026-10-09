using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;

/// <summary>
/// Modal for searching and adding previously-reported employees to the wage adjustment.
/// </summary>
public partial class AddEmployeesModal
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    /// <summary>
    /// Gets or sets whether the modal is visible.
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }
    /// <summary>
    /// Gets or sets the list of previously-reported employees to display.
    /// </summary>
    [Parameter]
    public List<PreviouslyReportedEmployee> PreviouslyReportedEmployees { get; set; } = [];
    /// <summary>
    /// Gets or sets callback invoked when the modal is closed.
    /// </summary>
    [Parameter]
    public EventCallback OnClose { get; set; }
    /// <summary>
    /// Gets or sets list of already added employees to the Adjustment Component
    /// </summary>
    [Parameter]
    public List<WageAdjustmentEmployeeData> AddedEmployees { get; set; } = [];
    /// <summary>
    /// Gets or sets callback invoked when an employee is added.
    /// </summary>
    [Parameter]
    public EventCallback<PreviouslyReportedEmployee> OnAddEmployee { get; set; }
    /// <summary>
    /// Callback to remove employee from adjustment
    /// </summary>
    [Parameter]
    public EventCallback<PreviouslyReportedEmployee> OnRemoveEmployee { get; set; }

    private string _searchText = string.Empty;
    private readonly HashSet<string> _addedEmployeeKeys = [];
    private bool _previousIsOpen;
    private int _pageSize = 10;
    private int _currentPage = 1;
    private bool _ssnVisible = false;
    private readonly HashSet<string> _visibleSsnKeys = [];
    private ElementReference _modalElement;
    private IJSObjectReference? _module;
    private bool _wasOpen;

    /// <summary>
    /// Set focus on modal and trap focus until closed
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !_wasOpen)
        {
            _wasOpen = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _modalElement);
        }
        else if (!IsOpen && _wasOpen)
        {
            _wasOpen = false;
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("closeFocusTrap");
            }
        }
    }

    // --- Computed properties ---
    private IEnumerable<PreviouslyReportedEmployee> FilteredEmployees
    => string.IsNullOrWhiteSpace(_searchText)
    ? PreviouslyReportedEmployees
    : PreviouslyReportedEmployees.Where(e =>
    {
        var search = _searchText.ToLower();
        //Compare identifiers normalized so a search matches whether or not the user typed the
        //hyphens, and so an M00 number matches regardless of the case of its leading "M".
        var normalizedSearch = SsnHelper.Normalize(_searchText);
        return e.LastName.ToLower().Contains(search)
      || e.FirstName.ToLower().Contains(search)
      || (normalizedSearch.Length > 0
          && SsnHelper.Normalize(e.SSN).Contains(normalizedSearch, StringComparison.OrdinalIgnoreCase));
    });

    private int TotalPages
    => (int) Math.Ceiling(FilteredEmployees.Count() / (double) _pageSize);

    private IEnumerable<PreviouslyReportedEmployee> PagedEmployees
      => FilteredEmployees
      .Skip((_currentPage - 1) * _pageSize)
      .Take(_pageSize);

    private string PaginationLabel
    {
        get
        {
            var total = FilteredEmployees.Count();
            var start = total == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;
            var end = Math.Min(_currentPage * _pageSize, total);
            return $"{start}-{end} of {total}";
        }
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
        _currentPage = 1;
    }

    private void ToggleSsnVisibility()
    {
        _ssnVisible = !_ssnVisible;
        if (_ssnVisible)
        {
            foreach (var employee in FilteredEmployees)
            {
                _visibleSsnKeys.Add(GetEmployeeKey(employee));
            }
        }
        else
        {
            _visibleSsnKeys.Clear();
        }
    }

    private void ToggleRowSsnVisibility(string employeeKey)
    {
        if (!_visibleSsnKeys.Remove(employeeKey))
        {
            _visibleSsnKeys.Add(employeeKey);
        }
    }

    private string MaskSsn(string ssn, string employeeKey)
    {
        return _ssnVisible || _visibleSsnKeys.Contains(employeeKey) ? SsnHelper.FormatSSN(ssn) : SsnHelper.MaskSSN(ssn);
    }

    private async Task HandleAddAll()
    {
        foreach (var employee in FilteredEmployees)
        {
            var key = GetEmployeeKey(employee);
            if (!_addedEmployeeKeys.Contains(key))
            {
                _addedEmployeeKeys.Add(key);
                await OnAddEmployee.InvokeAsync(employee);
            }
        }
        StateHasChanged();
    }

    private async Task HandleRemoveEmployee(PreviouslyReportedEmployee employee)
    {
        _addedEmployeeKeys.Remove(GetEmployeeKey(employee));
        await OnRemoveEmployee.InvokeAsync(employee);
        StateHasChanged();
    }

    private void HandlePageSizeChange(ChangeEventArgs e)
    {
        _pageSize = int.Parse(e.Value?.ToString() ?? "10");
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

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (IsOpen && !_previousIsOpen)
        {
            _searchText = string.Empty;
            _addedEmployeeKeys.Clear();
            foreach (var employee in AddedEmployees)
            {
                _addedEmployeeKeys.Add($"{employee.OriginalSSN}|{employee.Order}");
            }
        }
        _previousIsOpen = IsOpen;
    }

    private async Task HandleAddEmployee(PreviouslyReportedEmployee employee)
    {
        _addedEmployeeKeys.Add(GetEmployeeKey(employee));
        await OnAddEmployee.InvokeAsync(employee);
        StateHasChanged();
    }

    private Task Close()
    {
        _addedEmployeeKeys.Clear();
        return OnClose.InvokeAsync();
    }

    private static string GetEmployeeKey(PreviouslyReportedEmployee employee)
    {
        return $"{employee.SSN}|{employee.Order}";
    }
}
