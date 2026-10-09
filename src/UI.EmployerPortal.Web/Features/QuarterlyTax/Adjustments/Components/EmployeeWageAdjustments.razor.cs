using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;

/// <summary>
/// Step 2 of the Wage Report Adjustment wizard — Employee Wage Adjustments.
/// </summary>
public partial class EmployeeWageAdjustments
{
    private const string SsnLengthErrorMessage =
        "SSN must be 9 digits, or an M00 number of \"M\" followed by 8 digits.";

    /// <summary>
    /// Gets or sets the employees adjustment model.
    /// </summary>
    [Parameter]
    public WageAdjustmentEmployeesModel EmployeesData { get; set; } = default!;

    /// <summary>
    /// Gets or sets the reporting quarter display string.
    /// </summary>
    [Parameter]
    public string ReportingQuarter { get; set; } = string.Empty;

    private const string AdjustedWagesRequiredMessage = "Adjusted wages are required.";
    private EditContext _editContext = default!;
    private ValidationMessageStore _validationMessageStore = default!;
    private bool _isAddModalOpen;
    private bool _isRemoveModalOpen;
    private bool _showErrors;
    private WageAdjustmentEmployeeData? _employeeToRemove;
    private readonly HashSet<Guid> _visibleSsns = [];
    private bool _showAllSsns;
    private readonly Dictionary<Guid, bool> _correctedSsnFocused = [];
    private readonly Dictionary<Guid, bool> _correctedSsnVisible = [];
    private readonly Dictionary<string, string> _fieldIds = [];
    // Maps each summary key onto the row field that owns its messages - which is where every
    // message lands, whether written here or by DataAnnotationsValidator.
    private readonly Dictionary<string, FieldIdentifier> _rowFields = [];
    private readonly HashSet<Guid> _invalidSsnErrors = [];
    private string _sortColumn = "lastName";
    private bool _sortAscending = true;
    private int _currentPage = 1;
    private int _pageSize = 10;
    private int TotalPages => (int) Math.Ceiling(EmployeesData.AdjustmentEmployees.Count / (double) _pageSize);
    private IEnumerable<WageAdjustmentEmployeeData> PagedEmployees
    => SortedEmployees.Skip((_currentPage - 1) * _pageSize).Take(_pageSize);
    private string PaginationLabel
    {
        get
        {
            var total = EmployeesData.AdjustmentEmployees.Count;
            var start = total == 0 ? 0 : ((_currentPage - 1) * _pageSize) + 1;
            var end = Math.Min(_currentPage * _pageSize, total);
            return $"{start}-{end} of {total}";
        }
    }
    private List<SelectOption> AdjustmentReasonOptions
  => EmployeesData.AdjustmentReasons
  .Select(r =>
  {
      return new SelectOption { Value = r.CodeSK.ToString(), Text = r.ReasonText ?? string.Empty };
  })
  .ToList();

    private IEnumerable<WageAdjustmentEmployeeData> SortedEmployees => _sortColumn switch
    {
        "lastName" => SortBy(e =>
        {
            return e.OriginalLastName;
        }),
        "firstName" => SortBy(e =>
        {
            return e.OriginalFirstName;
        }),
        "ssn" => SortBy(e =>
        {
            return e.OriginalSSN;
        }),
        "quarterlyWages" => SortBy(e =>
        {
            return e.OriginalQuarterlyWages;
        }),
        "reason" => SortBy(e =>
        {
            return EmployeesData.AdjustmentReasons.FirstOrDefault(r =>
            {
                return r.CodeSK == e.AdjustmentReasonCodeSK;
            })?.ReasonText ?? string.Empty;
        }),
        _ => SortBy(e =>
        {
            return e.OriginalLastName;
        })
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _editContext = new EditContext(EmployeesData);
        _validationMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += ClearManualMessagesFor;
    }

    /// <summary>
    /// Drops the messages THIS Component wrote for a field as soon as the user changes it.
    /// Oly the changed field is cleared. Clearning the whole store would take unrelated rows' errors with it.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void ClearManualMessagesFor(object? sender, FieldChangedEventArgs e)
    {
        _validationMessageStore.Clear(e.FieldIdentifier);
        _editContext.NotifyValidationStateChanged();
    }


    /// <summary>
    /// Validates the step and shows errors if invalid.
    /// </summary>
    public async Task<bool> IsValid()
    {
        var hasErrors = false;
        var edited = false;
        _invalidSsnErrors.Clear();
        _validationMessageStore.Clear();
        _fieldIds.Clear();
        _rowFields.Clear();
        RegisterAllRowFields();
        foreach (var employee in EmployeesData.AdjustmentEmployees)
        {
            var hasSsnChanged = !string.IsNullOrWhiteSpace(employee.CorrectedSSN);
            var hasFirstNameChanged = !string.IsNullOrWhiteSpace(employee.CorrectedFirstName);
            var hasLastNameChanged = !string.IsNullOrWhiteSpace(employee.CorrectedLastName);
            var hasWagesChanged = employee.AdjustedQuarterlyWages.HasValue;
            if (hasSsnChanged || hasFirstNameChanged || hasLastNameChanged || hasWagesChanged)
            {
                edited = true;
            }

            // Validation 0: a corrected name must satisfy the same length and character rules as a
            // name entered anywhere else. Nothing checked this before, so a 200-character name or one
            // containing characters the mainframe cannot store was accepted here and rejected on
            // Wage Entry.
            var lastNameProblem = EmployeeFieldRules.DescribeNameProblem(employee.CorrectedLastName, "Last name");
            if (lastNameProblem is not null)
            {
                AddEmployeeError(employee, $"corrected-lastname-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.CorrectedLastName), lastNameProblem);
                hasErrors = true;
            }

            var firstNameProblem = EmployeeFieldRules.DescribeNameProblem(employee.CorrectedFirstName, "First name");
            if (firstNameProblem is not null)
            {
                AddEmployeeError(employee, $"corrected-firstname-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.CorrectedFirstName), firstNameProblem);
                hasErrors = true;
            }

            // Validation 0b: the $100M ceiling. The [Range] attribute produces this on field change,
            // but a value restored from a saved adjustment is never "changed", so check it here too.
            if (employee.AdjustedQuarterlyWages > (decimal) EmployeeFieldRules.MaxQuarterlyWages)
            {
                AddEmployeeError(employee, $"corrected-wages-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages),
                    EmployeeFieldRules.WagesTooLargeMessage);
                hasErrors = true;
            }

            // Validation 1: Cannot change all 3 fields at once
            if (hasSsnChanged && hasFirstNameChanged && hasLastNameChanged)
            {
                AddEmployeeError(employee, $"corrected-lastname-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.CorrectedLastName),
                    "Cannot change Last Name, First Name and SSN together.");
                hasErrors = true;
            }

            // Validation 2: Corrected SSN must be exactly 9 characters when provided
            // (9 digits, or an M00 number of "M" plus 8 digits).
            if (hasSsnChanged && SsnHelper.Normalize(employee.CorrectedSSN).Length != SsnHelper.IdentifierLength)
            {
                _invalidSsnErrors.Add(employee.Id);
                AddEmployeeError(employee, $"corrected-ssn-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.CorrectedSSN),
                    SsnLengthErrorMessage);
                hasErrors = true;
            }

            // Validation 3: Reason is required when Employee Quarterly Wages are edited
            if (hasWagesChanged && (employee.AdjustmentReasonCodeSK is null or 0))
            {
                AddEmployeeError(employee, $"corrected-reason-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation),
                    "Adjustment reason required when wage is entered");
                hasErrors = true;
            }

            // Validation 4: Wage is required when Employee Quarterly Wages Reason is selected
            if (employee.AdjustmentReasonCodeSK.HasValue && !hasWagesChanged)
            {
                AddEmployeeError(employee, $"corrected-wages-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages),
                    "Adjustment wage required when reason is selected");
                hasErrors = true;
            }

            // Validation 5: An adjusted wage identical to the reported wage is not an adjustment.
            // Submitting it produces a zero-variance detail the service rejects, so block it here
            // instead. A blank field remains valid.
            if (IsAdjustedWageUnchanged(employee))
            {
                AddEmployeeError(employee, $"corrected-wages-{employee.Id}",
                    nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages),
                    AdjustedWagesRequiredMessage);
                hasErrors = true;
            }
        }
        if (!edited)
        {
            _validationMessageStore.Add(new FieldIdentifier(EmployeesData, String.Empty), "There are no adjustments to process.");
            hasErrors = true;
        }
        if (hasErrors)
        {
            _showErrors = true;
        }
        _editContext.NotifyValidationStateChanged();
        return !hasErrors;
    }

    /// <summary>
    /// Registers the field that owns each row's messages, so <see cref="ResolveField" /> can map the
    /// summary's key onto it. Every per-row message lands here - this method's own, and everything
    /// <c>DataAnnotationsValidator</c> produces on <see cref="WageAdjustmentEmployeeData" />.
    /// </summary>
    private void RegisterRowFields(WageAdjustmentEmployeeData employee)
    {
        Register($"corrected-lastname-{employee.Id}", nameof(WageAdjustmentEmployeeData.CorrectedLastName));
        Register($"corrected-firstname-{employee.Id}", nameof(WageAdjustmentEmployeeData.CorrectedFirstName));
        Register($"corrected-ssn-{employee.Id}", nameof(WageAdjustmentEmployeeData.CorrectedSSN));
        Register($"corrected-wages-{employee.Id}", nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages));
        Register($"corrected-reason-{employee.Id}", nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation));

        void Register(string key, string propertyName)
        {
            // Both maps, for EVERY field, regardless of whether anything is currently wrong with it.
            // The key is also the input's element id, so the summary can link to it.
            _fieldIds[key] = key;
            _rowFields[key] = new FieldIdentifier(employee, propertyName);
        }
    }

    /// <summary>
    /// Registers every row's fields so the summary can attribute ANY message to a key.
    ///
    /// WHY HERE and not only in <see cref="IsValid" />: that method registers a key only when one of
    /// its own rules fires, but <c>DataAnnotationsValidator</c> produces messages independently, on
    /// every field change. Those arrive with no key registered, so they listed as plain text while
    /// the hand-written errors beside them were links. It also went stale - editing a field after a
    /// failed Continue created messages the last IsValid run had never seen.
    ///
    /// Registration is cheap, idempotent and independent of validity, so doing it for every row on
    /// every parameter set keeps the maps in step with what is actually on screen.
    /// </summary>
    private void RegisterAllRowFields()
    {
        foreach (var employee in EmployeesData.AdjustmentEmployees)
        {
            RegisterRowFields(employee);
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet() => RegisterAllRowFields();

    /// <summary>
    /// Records one error twice, following the idiom already used throughout <see cref="IsValid" />:
    /// against the summary's key so it is listed and linkable, and against the employee's own field so
    /// the input renders it inline.
    /// </summary>
    private void AddEmployeeError(WageAdjustmentEmployeeData employee, string fieldKey, string propertyName, string message)
    {
        // One write. ResolveField maps fieldKey onto this identifier for the summary, and the input
        // reads the same identifier for its inline error.
        var field = new FieldIdentifier(employee, propertyName);
        if (!_editContext.GetValidationMessages(field).Contains(message))
        {
            _validationMessageStore.Add(field, message);
        }
        _fieldIds[fieldKey] = fieldKey;
        _rowFields[fieldKey] = field;
    }

    /// <summary>
    /// Maps a summary key onto the row field that owns its messages, falling back to the root model
    /// for anything not row-scoped.
    /// </summary>
    private FieldIdentifier ResolveField(string key) =>
        _rowFields.TryGetValue(key, out var rowField) ? rowField : _editContext.Field(key);

    private void OpenAddEmployeesModal()
    {
        _isAddModalOpen = true;
        StateHasChanged();
    }

    private Task HandleAddModalClose()
    {
        _isAddModalOpen = false;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleEmployeeAdded(PreviouslyReportedEmployee employee)
    {
        if (EmployeesData.AdjustmentEmployees.Count == 0)
        {
            var keyFieldId = new FieldIdentifier(EmployeesData, String.Empty);
            _validationMessageStore.Clear(keyFieldId);
        }
        var alreadyAdded = EmployeesData.AdjustmentEmployees.Any(e =>
        {
            return e.OriginalSSN == employee.SSN && e.Order == employee.Order;
        });

        if (!alreadyAdded)
        {
            EmployeesData.AdjustmentEmployees.Add(new WageAdjustmentEmployeeData
            {
                OriginalLastName = employee.LastName,
                OriginalFirstName = employee.FirstName,
                OriginalSSN = employee.SSN,
                OriginalQuarterlyWages = employee.QuarterlyWages,
                Order = employee.Order,
            });
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void HandleRemoveEmployee(WageAdjustmentEmployeeData employee)
    {
        _employeeToRemove = employee;
        _isRemoveModalOpen = true;
        StateHasChanged();
    }

    private void HandleRemoveEmployeeFromAddModel(PreviouslyReportedEmployee employee)
    {
        var match = EmployeesData.AdjustmentEmployees.FirstOrDefault(e =>
        {
            return e.OriginalSSN == employee.SSN
                    && e.OriginalLastName == employee.LastName
                    && e.OriginalFirstName == employee.FirstName;
        });
        if (match is not null)
        {
            EmployeesData.AdjustmentEmployees.Remove(match);
        }
        StateHasChanged();
    }

    private Task HandleRemoveModalClose()
    {
        _isRemoveModalOpen = false;
        _employeeToRemove = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private Task HandleRemoveConfirmed()
    {
        if (_employeeToRemove is not null)
        {
            EmployeesData.AdjustmentEmployees.Remove(_employeeToRemove);
            _employeeToRemove = null;
        }
        _isRemoveModalOpen = false;
        if (_currentPage > TotalPages && TotalPages > 0)
        {
            _currentPage = TotalPages;
        }
        StateHasChanged();
        return Task.CompletedTask;
    }

    private void ToggleAllSsnVisibility()
    {
        _showAllSsns = !_showAllSsns;
        if (_showAllSsns)
        {
            foreach (var emp in EmployeesData.AdjustmentEmployees)
            {
                _visibleSsns.Add(emp.Id);
                _correctedSsnVisible[emp.Id] = true;
            }
        }
        else
        {
            _visibleSsns.Clear();
            foreach (var emp in EmployeesData.AdjustmentEmployees)
            {
                _correctedSsnVisible[emp.Id] = false;
            }
        }
    }

    private void ToggleSsnVisibility(Guid employeeId)
    {
        if (!_visibleSsns.Remove(employeeId))
        {
            _visibleSsns.Add(employeeId);
        }
    }

    private void HandleCorrectedLastNameChanged(WageAdjustmentEmployeeData employee, string? value)
    {
        employee.CorrectedLastName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void HandleCorrectedFirstNameChanged(WageAdjustmentEmployeeData employee, string? value)
    {
        employee.CorrectedFirstName = string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private void HandleCorrectedSsnChanged(WageAdjustmentEmployeeData employee, string? value)
    {
        employee.CorrectedSSN = string.IsNullOrWhiteSpace(value) ? null : value;

    }

    private void HandleAdjustedWageOnBlur(WageAdjustmentEmployeeData employee)
    {
        // If wages were removed, clear the selected reason and its errors
        if (!employee.AdjustedQuarterlyWages.HasValue)
        {
            employee.AdjustmentReasonCodeSK = null;
            var reasonFieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation));
            var reasonKeyFieldId = new FieldIdentifier(EmployeesData, $"corrected-reason-{employee.Id}");
            _validationMessageStore.Clear(reasonFieldId);
            _validationMessageStore.Clear(reasonKeyFieldId);
        }

        var fieldKey = $"corrected-wages-{employee.Id}";
        var msg = $"Adjustment wage required when reason is selected";
        var keyFieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages));
        var feilIdbyKey = new FieldIdentifier(EmployeesData, fieldKey);

        _validationMessageStore.Clear(keyFieldId);
        _validationMessageStore.Clear(feilIdbyKey);

        if (employee.AdjustmentReasonCodeSK.HasValue && (employee.AdjustedQuarterlyWages is null))
        {
            _validationMessageStore.Add(new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages)), msg);
            _fieldIds[fieldKey] = $"corrected-wages-{employee.Id}";
        }
        else if (IsAdjustedWageUnchanged(employee))
        {
            _validationMessageStore.Add(new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages)), AdjustedWagesRequiredMessage);
            _fieldIds[fieldKey] = $"corrected-wages-{employee.Id}";
        }
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// True when an adjusted wage was entered but matches the wage already reported,
    /// which is not a real adjustment. A blank field is not flagged.
    /// </summary>
    private static bool IsAdjustedWageUnchanged(WageAdjustmentEmployeeData employee)
    {
        return employee.AdjustedQuarterlyWages.HasValue
            && employee.AdjustedQuarterlyWages.Value == employee.OriginalQuarterlyWages;
    }

    private void HandleAdjustedWageReasonOnBlur(WageAdjustmentEmployeeData employee)
    {
        var fieldKey = $"corrected-reason-{employee.Id}";
        var msg = "Adjustment reason required when wage is entered";
        var fieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation));
        var keyFieldId = new FieldIdentifier(EmployeesData, fieldKey);

        _validationMessageStore.Clear(fieldId);
        _validationMessageStore.Clear(keyFieldId);

        if (employee.AdjustedQuarterlyWages.HasValue && (employee.AdjustmentReasonCodeSK is null or 0))
        {
            _validationMessageStore.Add(new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation)), msg);
            _fieldIds[fieldKey] = $"corrected-reason-{employee.Id}";
        }
        _editContext.NotifyValidationStateChanged();
    }

    private void HandleCorrectedSsnFocusedChanged(WageAdjustmentEmployeeData employee, bool value)
    {
        var fieldKey = $"corrected-ssn-{employee.Id}";
        var msg = SsnLengthErrorMessage;
        var fieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.CorrectedSSN));
        var keyFieldId = new FieldIdentifier(EmployeesData, fieldKey);

        _correctedSsnFocused[employee.Id] = value;
        _validationMessageStore.Clear(fieldId);
        _validationMessageStore.Clear(keyFieldId);

        if (string.IsNullOrWhiteSpace(employee.CorrectedSSN))
        {
            _invalidSsnErrors.Remove(employee.Id);
        }
        else
        {
            if (SsnHelper.Normalize(employee.CorrectedSSN).Length != SsnHelper.IdentifierLength)
            {
                _validationMessageStore.Add(new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.CorrectedSSN)), msg);
                _fieldIds[fieldKey] = $"corrected-ssn-{employee.Id}";
                _invalidSsnErrors.Add(employee.Id);
            }
            else
            {
                _invalidSsnErrors.Remove(employee.Id);
            }
        }
        _editContext.NotifyValidationStateChanged();
    }

    private void HandleCorrectedSsnVisibleChanged(Guid id, bool value)
    {
        _correctedSsnVisible[id] = value;
    }

    private void HandleReasonChanged(WageAdjustmentEmployeeData employee, string? value)
    {
        var fieldKey = $"corrected-reason-{employee.Id}";
        var msg = "Adjustment reason required when wage is entered";
        var fieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation));
        var keyFieldId = new FieldIdentifier(EmployeesData, fieldKey);

        _validationMessageStore.Clear(fieldId);
        _validationMessageStore.Clear(keyFieldId);

        employee.AdjustmentReasonCodeSK = int.TryParse(value, out var parsed) ? parsed : null;
        _validationMessageStore.Clear(fieldId);
        if (employee.AdjustedQuarterlyWages.HasValue && (employee.AdjustmentReasonCodeSK is null or 0))
        {
            _validationMessageStore.Add(new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation)), msg);
        }
        _editContext.NotifyValidationStateChanged();
    }

    private IOrderedEnumerable<WageAdjustmentEmployeeData> SortBy<TKey>(Func<WageAdjustmentEmployeeData, TKey> keySelector)
    {
        return _sortAscending
        ? EmployeesData.AdjustmentEmployees.OrderBy(keySelector)
        : EmployeesData.AdjustmentEmployees.OrderByDescending(keySelector);
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

    private void HandleAdjustedWageChanged(WageAdjustmentEmployeeData employee, decimal? value)
    {
        employee.AdjustedQuarterlyWages = value;
        if (!value.HasValue)
        {
            employee.AdjustmentReasonCodeSK = null;
            var reasonFieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustmentReasonValidation));
            var reasonKeyFieldId = new FieldIdentifier(EmployeesData, $"corrected-reason-{employee.Id}");
            _validationMessageStore.Clear(reasonFieldId);
            _validationMessageStore.Clear(reasonKeyFieldId);

            var wageFieldId = new FieldIdentifier(employee, nameof(WageAdjustmentEmployeeData.AdjustedQuarterlyWages));
            var wageKeyFieldId = new FieldIdentifier(EmployeesData, $"corrected-wages-{employee.Id}");
            _validationMessageStore.Clear(wageFieldId);
            _validationMessageStore.Clear(wageKeyFieldId);

            _editContext.NotifyValidationStateChanged();
        }
    }
}
