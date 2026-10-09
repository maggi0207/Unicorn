using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Components;

/// <summary>
/// Step 3 of the Wage Report Adjustment wizard — Verification.
/// </summary>
public partial class AdjustmentVerification
{
    /// <summary>
    /// Gets or sets the reporting quarter display string.
    /// </summary>
    [Parameter]
    public string ReportingQuarter { get; set; } = string.Empty;
    /// <summary>
    /// Gets or sets the list of employees with adjustments to verify.
    /// </summary>
    [Parameter]
    public List<WageAdjustmentEmployeeData> AdjustmentEmployees { get; set; } = [];
    /// <summary>
    /// Gets or sets the available adjustment reasons for display.
    /// </summary>
    [Parameter]
    public List<WageAdjustmentReasonOption> AdjustmentReasons { get; set; } = [];

    /// <summary>
    /// Gets or sets callback invoked when the user clicks Edit on Employees.
    /// </summary>
    [Parameter]
    public EventCallback OnEditEmployees { get; set; }

    private bool _employeesExpanded = true;
    private readonly HashSet<Guid> _visibleSsns = [];
    private bool _showAllSsns;
    private readonly HashSet<Guid> _visibleCorrectedSsns = [];


    private void ToggleEmployeesExpanded()
    {
        _employeesExpanded = !_employeesExpanded;
    }

    private Task HandleEditEmployees()
    {
        return OnEditEmployees.InvokeAsync();
    }

    private static string GetReasonText(int? codeSK, List<WageAdjustmentReasonOption> reasons)
    {
        return codeSK is null or 0
            ? string.Empty
            : reasons.FirstOrDefault(r =>
        {
            return r.CodeSK == codeSK;
        })?.ReasonText ?? string.Empty;
    }

    private void ToggleAllSsnVisibility()
    {
        _showAllSsns = !_showAllSsns;
        if (_showAllSsns)
        {
            foreach (var emp in AdjustmentEmployees)
            {
                _visibleSsns.Add(emp.Id);
                _visibleCorrectedSsns.Add(emp.Id);
            }
        }
        else
        {
            _visibleSsns.Clear();
            _visibleCorrectedSsns.Clear();
        }
    }

    private void ToggleSsnVisibility(Guid employeeId)
    {
        if (!_visibleSsns.Remove(employeeId))
        {
            _visibleSsns.Add(employeeId);
        }
    }
    private void ToggleSsnCorrectedVisibility(Guid employeeId)
    {
        if (!_visibleCorrectedSsns.Remove(employeeId))
        {
            _visibleCorrectedSsns.Add(employeeId);
        }
    }
}
