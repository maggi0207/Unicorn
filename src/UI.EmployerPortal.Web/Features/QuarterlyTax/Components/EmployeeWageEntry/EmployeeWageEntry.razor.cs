using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.EmployeeWageEntry;

/// <summary>
/// Represents an employee record within a quarterly wage report entry
/// </summary>
public partial class EmployeeWageEntry
{
    /// <summary>
    /// Gets or sets the list of employees to display in the wage entry table
    /// </summary>
    [Parameter]
    public EmployeeWageEntryModel WageEntryData { get; set; } = default!;

    /// <summary>
    /// The edit context for form validation.
    /// </summary>
    private EditContext _editContext = default!;
    private ValidationMessageStore _messageStore = default!;

    /// <summary>
    /// Controls whether validation errors are displayed. 
    /// </summary>
    private bool _showErrors;

    /// <summary>
    /// Maps model field names to their corrosponding HTML element IDs for validation summary scrolling.
    /// </summary>
    private readonly Dictionary<string, string> _fieldIds = new()
    {
        // "employee-wage-table", not "employee-wages-table" - the id on the <table>. The plural
        // typo meant the summary rendered this as a link that focused nothing.
        // The aggregate "total exceeds" error belongs on the total, not on the table - focusing
        // the table dropped the user into the middle of the rows with no idea what to look at.
        [nameof(EmployeeWageEntryModel.Employees)] = "total-gross-covered-wages",
    };

    /// <summary>
    /// Maps a per-row key onto the employee field that owns its messages, so the summary can link
    /// a row's error. Falls back to the root model for the aggregate keys above.
    /// </summary>
    private readonly Dictionary<string, FieldIdentifier> _rowFields = [];

    /// <summary>
    /// The wage box's element id for a row. Derived from <see cref="EmployeeWageEntryData.RowKey" />,
    /// which is unique per row and independent of anything the backend supplies.
    /// </summary>
    private static string ElementIdFor(EmployeeWageEntryData employee) => $"wage-{employee.RowKey}";

    private FieldIdentifier ResolveField(string key) =>
        _rowFields.TryGetValue(key, out var rowField) ? rowField : _editContext.Field(key);

    private string _sortColumn = "lastName";
    private bool _sortAscending = true;

    /// <summary>
    /// Sort history, most recent first. Earlier sorts act as tie-breakers
    /// so sorting is stable across successive column sorts.
    /// </summary>
    private readonly List<(string Column, bool Ascending)> _sortHistory = [("lastName", true), ("firstName", true)];

    private string _searchText = string.Empty;
    private readonly HashSet<string> _visibleSsns = [];
    private bool _showAllSsns;
    private bool _isEditEmployeeModalOpen;
    private bool _isEditMode;
    private EmployeeWageEntryData _modalEmployee = new();
    private string? _originalSsn;
    private bool _isDeleteEmployeeModalOpen;
    private EmployeeWageEntryData? _deleteEmployee;
    //private bool _isSaveWithoutWagesModalOpen;
    //private TaskCompletionSource<bool>? _zeroWagesTcs;

    private List<EmployeeWageEntryData> FilteredEmployees => GetFilteredEmployees();

    private decimal TotalGrossWages => WageEntryData.Employees.Where(e =>
    {
        return e.SaveForNextQuarter;
    }).Sum(e =>
    {
        // Null means "not entered yet"; it contributes nothing to the displayed total.
        return e.QuarterlyWages ?? 0m;
    });

    /// <summary>
    /// Initializes the edit context for the wage entry model.
    /// </summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(WageEntryData);
        _messageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += ClearManualMessagesFor;
        _showErrors = false;
    }

    /// <summary>
    /// Validates the wage entry form and shows errors if invalid.
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsValid()
    {
        // Validate() clears the DataAnnotations store and re-validates the ROOT model only - it does
        // not recurse into Employees. Any per-row message raised on blur is wiped by it, which is why
        // the inline errors used to vanish the moment the user pressed Continue. Re-run the row rules
        // here and record them ourselves, so they survive and stay linkable.
        var rootValid = _editContext.Validate();
        var rowsValid = ValidateEmployees();

        if (!rootValid || !rowsValid)
        {
            _showErrors = true;
            _editContext.NotifyValidationStateChanged();
            return false;
        }

        //if (WageEntryData.Employees.Any(e =>
        //{
        //    return e.QuarterlyWages == 0m && e.SaveForNextQuarter;
        //}))
        //{
        //    _zeroWagesTcs = new TaskCompletionSource<bool>();
        //    _isSaveWithoutWagesModalOpen = true;
        //    StateHasChanged();
        //    await _zeroWagesTcs.Task;
        //}

        return true;
    }

    /// <summary>
    /// Validates each employee row and records any failures against that row's own field, registering
    /// the key so the summary can link to the input.
    /// </summary>
    /// <summary>
    /// Drops the messages THIS component wrote for a field as soon as the user changes it.
    ///
    /// DataAnnotationsValidator withdraws its own messages when a field is re-validated, but knows
    /// nothing about the ones recorded here, so a corrected wage kept showing its error until the
    /// next Continue. Only the changed field is cleared - clearing the store would take the other
    /// rows with it.
    /// </summary>
    private void ClearManualMessagesFor(object? sender, FieldChangedEventArgs e)
    {
        _messageStore.Clear(e.FieldIdentifier);
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Registers every row's wage field - all rows, whether or not they are currently valid.
    ///
    /// WHY up front rather than when a rule fails: the summary turns a message into a link by asking
    /// which registered key owns it, so a row that is not registered cannot be linked. Registering only
    /// FAILING rows, inside <see cref="ValidateEmployees" />, left these maps holding whatever the last
    /// Continue happened to produce. Correct the first of several rows sharing one message and the maps
    /// still described the state before the correction, so the single summary link resolved back to the
    /// row that had just been fixed.
    ///
    /// The Wage Adjustments screen has always registered this way and its links retarget
    /// correctly; this puts the two screens on the same footing.
    ///
    /// Registration is cheap, idempotent and independent of validity, so running it on every parameter
    /// set keeps the maps in step with the rows actually on screen.
    /// </summary>
    private void RegisterAllRowFields()
    {
        const string Member = nameof(EmployeeWageEntryData.QuarterlyWages);

        // ADDITIVE - deliberately never clears.
        //
        // Registration is what lets the summary attribute a message to a field and render it as a link;
        // an unregistered key means the message falls back to plain text. Clearing first makes that
        // outcome reachable: any render where Employees is briefly empty or partial - a reload, a step
        // change, data arriving late - would wipe every key and strip the links off messages that are
        // still on screen. Growing the maps cannot do that.
        //
        // The cost is that a removed row leaves its key behind. That is harmless: the key resolves to a
        // field that owns no messages, so it never matches and never wins attribution.
        //
        // Insertion order matters and is preserved here. The summary attributes a message to the FIRST
        // key that owns it, so a message shared by several rows links to the earliest-registered one -
        // model order. Re-adding an existing key does not move it; only Remove would, because
        // Dictionary reuses freed slots and a re-added entry can then enumerate ahead of untouched ones.
        foreach (var employee in WageEntryData.Employees)
        {
            var key = $"{Member}-{employee.RowKey}";

            _fieldIds[key] = ElementIdFor(employee);
            _rowFields[key] = new FieldIdentifier(employee, Member);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet() => RegisterAllRowFields();

    private bool ValidateEmployees()
    {
        _messageStore.Clear();

        // Re-register before validating: a row added since the last render must be linkable too.
        RegisterAllRowFields();

        var allValid = true;

        foreach (var employee in WageEntryData.Employees)
        {
            // ONLY the wage. Names and SSN are display-only in this grid - they are edited through the
            // Add/Edit modal, which validates them on save. Validating the whole object here reported
            // errors the user cannot act on from this screen, and SSN in particular is stored
            // unformatted while its rule demands ###-##-####, so every row failed it.
            var results = new List<ValidationResult>();
            var wages = new ValidationContext(employee) { MemberName = nameof(EmployeeWageEntryData.QuarterlyWages) };
            Validator.TryValidateProperty(employee.QuarterlyWages, wages, results);

            foreach (var result in results)
            {
                // Registration happens in RegisterAllRowFields, for every row - not here, and not
                // only for the ones that fail.
                var field = new FieldIdentifier(employee, nameof(EmployeeWageEntryData.QuarterlyWages));
                var message = result.ErrorMessage ?? string.Empty;

                if (!_editContext.GetValidationMessages(field).Contains(message))
                {
                    _messageStore.Add(field, message);
                }

                allValid = false;
            }
        }

        return allValid;
    }

    private List<EmployeeWageEntryData> GetFilteredEmployees()
    {
        var activeEmployees = WageEntryData.Employees.Where(e =>
        {
            return e.SaveForNextQuarter;
        });

        if (!string.IsNullOrWhiteSpace(_searchText))
        {
            var searchLower = _searchText.ToLower();
            activeEmployees = activeEmployees.Where(e =>
            {
                return e.LastName.ToLower().Contains(searchLower) ||
                                e.FirstName.ToLower().Contains(searchLower) ||
                                e.SSN.Contains(searchLower);
            });
        }

        return ApplySortHistory(activeEmployees);
    }

    /// <summary>
    /// Applies the sort history so the most recent sort is primary and
    /// earlier sorts break ties (stable multi-column sorting).
    /// </summary>
    private List<EmployeeWageEntryData> ApplySortHistory(IEnumerable<EmployeeWageEntryData> source)
    {
        IOrderedEnumerable<EmployeeWageEntryData>? ordered = null;

        foreach (var (column, ascending) in _sortHistory)
        {
            var key = GetSortKey(column);

            ordered = ordered is null
                ? (ascending ? source.OrderBy(key) : source.OrderByDescending(key))
                : (ascending ? ordered.ThenBy(key) : ordered.ThenByDescending(key));
        }

        return ordered?.ToList() ?? source.ToList();
    }

    private static Func<EmployeeWageEntryData, IComparable?> GetSortKey(string column)
    {
        return column switch
        {
            "firstName" => e => { return e.FirstName; }
            ,
            "ssn" => e => { return e.SSN; }
            ,
            "quarterlyWages" => e => { return e.QuarterlyWages; }
            ,
            _ => e => { return e.LastName; }
            ,
        };
    }

    private void HandleSearchInput(ChangeEventArgs e)
    {
        _searchText = e.Value?.ToString() ?? string.Empty;
    }

    private void Sort(string column)
    {
        var existingIndex = _sortHistory.FindIndex(s =>
        {
            return s.Column == column;
        });

        // Clicking the current primary column toggles direction so
        // any other column becomes primary, ascending.
        var ascending = existingIndex != 0 || !_sortHistory[0].Ascending;

        if (existingIndex >= 0)
        {
            _sortHistory.RemoveAt(existingIndex);
        }

        _sortHistory.Insert(0, (column, ascending));

        _sortColumn = column;
        _sortAscending = ascending;
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

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }

    private string FormatSsn(EmployeeWageEntryData employee)
    {
        return (_showAllSsns || _visibleSsns.Contains(employee.SSN)) ? SsnHelper.FormatSSN(employee.SSN) : SsnHelper.MaskSSN(employee.SSN);
    }

    private void ToggleAllSsnVisibility()
    {
        _showAllSsns = !_showAllSsns;
        _visibleSsns.Clear();
    }

    private void ToggleSsnVisibility(string ssn)
    {
        if (!_visibleSsns.Remove(ssn))
        {
            _visibleSsns.Add(ssn);
        }
    }

    private bool IsSsnVisible(string ssn)
    {
        return _showAllSsns || _visibleSsns.Contains(ssn);
    }
    private string GetVisibilityIcon(string ssn)
    {
        return IsSsnVisible(ssn)
            ? Assets["icons/visibility-on.svg"]
            : Assets["icons/visibility-off.svg"];
    }

    private string GetVisibilityAlt(string ssn)
    {
        return IsSsnVisible(ssn) ? "Hide SSN" : "Show SSN";
    }

    private void HandleWageValueChanged(EmployeeWageEntryData employee, decimal? newValue)
    {
        employee.QuarterlyWages = newValue ?? 0m;
    }

    private void HandleEdit(EmployeeWageEntryData employee)
    {
        _isEditMode = true;
        _modalEmployee = employee;
        _originalSsn = employee.SSN;
        _isEditEmployeeModalOpen = true;
    }

    private void HandleDelete(EmployeeWageEntryData employee)
    {
        _deleteEmployee = employee;
        _isDeleteEmployeeModalOpen = true;
    }

    private async Task HandleDeleteModalClose()
    {
        _isDeleteEmployeeModalOpen = false;
        _deleteEmployee = null;
    }

    private async Task HandleDeleteConfirm()
    {
        if (_deleteEmployee != null)
        {
            _deleteEmployee.SaveForNextQuarter = false;
        }

        _isDeleteEmployeeModalOpen = false;
        _deleteEmployee = null;
    }

    //private void HandleSaveWithoutWagesClose()
    //{
    //    _isSaveWithoutWagesModalOpen = false;
    //    _zeroWagesTcs?.TrySetResult(true);
    //}

    //private void HandleDoNotSaveEmployees()
    //{
    //    foreach (var employee in WageEntryData.Employees.Where(e =>
    //    {
    //        return e.QuarterlyWages == 0m && e.SaveForNextQuarter;
    //    }))
    //    {
    //        employee.SaveForNextQuarter = false;
    //    }

    //    _isSaveWithoutWagesModalOpen = false;
    //    _zeroWagesTcs?.TrySetResult(true);
    //}

    private async Task OpenAddModel()
    {
        _isEditMode = false;
        _originalSsn = null;
        _modalEmployee = new EmployeeWageEntryData { SaveForNextQuarter = true };
        _isEditEmployeeModalOpen = true;
    }

    private void HandleModalClose()
    {
        _isEditEmployeeModalOpen = false;
    }

    private void HandleModalSave(EmployeeWageEntryData employee)
    {
        if (_isEditMode)
        {
            //Match by the original SSN captured when the eidt modal was opened,
            //since the user may have changed the SSN in the modal.
            var normalizedOriginalSsn = _originalSsn?.Replace("-", "") ?? string.Empty;
            var existingEmployee = WageEntryData.Employees.FirstOrDefault(e =>
            {
                return (e.SSN?.Replace("-", "") ?? string.Empty) == normalizedOriginalSsn;
            });

            if (existingEmployee != null)
            {
                existingEmployee.LastName = employee.LastName;
                existingEmployee.FirstName = employee.FirstName;
                existingEmployee.SSN = employee.SSN;
            }
        }
        else
        {
            var normalizedSSN = employee.SSN?.Replace("-", "") ?? string.Empty;
            var softDeleted = WageEntryData.Employees.FirstOrDefault(e =>
            {
                return (e.SSN?.Replace("-", "") ?? string.Empty) == normalizedSSN && !e.SaveForNextQuarter;
            });

            if (softDeleted != null)
            {
                softDeleted.LastName = employee.LastName;
                softDeleted.FirstName = employee.FirstName;
                softDeleted.SaveForNextQuarter = true;
            }
            else
            {
                WageEntryData.Employees.Add(employee);
            }
        }

        _isEditEmployeeModalOpen = false;
    }

    private List<string> GetOtherEmployeeSSNs()
    {
        var normalizedOriginalSsn = _isEditMode
            ? (_originalSsn?.Replace("-", "") ?? string.Empty)
            : string.Empty;

        return WageEntryData.Employees
            .Where(e =>
            {
                var normalizedSSN = e.SSN?.Replace("-", "") ?? string.Empty;
                return normalizedSSN != normalizedOriginalSsn && e.SaveForNextQuarter;
            })
            .Select(e =>
            {
                return e.SSN;
            })
            .Where(ssn =>
            {
                return !string.IsNullOrEmpty(ssn);
            })
            .ToList();
    }
}
