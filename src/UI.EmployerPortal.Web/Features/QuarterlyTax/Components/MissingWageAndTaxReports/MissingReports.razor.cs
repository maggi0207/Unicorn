using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.MissingWageAndTaxReports;

/// <summary>
/// MissingReportsMissing
/// </summary>
public partial class MissingReports
{
    /// <summary>
    /// Report Data
    /// </summary>
    [Parameter]
    public MissingWageAndTaxReportsData ReportData { get; set; } = default!;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private bool _hasActiveAudit;
    private bool _hasMissingReports;
    private bool _hasPendingReports;
    private bool _showPendingErrorrs;
    private readonly EditContext _pendingEditContext = new(new object());
    private ValidationMessageStore _pendingMessageStore = default!;
    private readonly Dictionary<string, string> _pendingFieldIds = new()
    {
        [string.Empty] = "pending-reports-table"
    };

    private IEnumerable<MissingReportModel> _gridData = [];
    private string _sortColumn = "reportName";
    private bool _sortAscending = true;
    private const string AuditWarning = "Active Audit in Progress: ";
    private const string AuditText = "Your account is currently in an active audit. Some reports may not be available to take action on.";
    private const string MissingReportsWarning = "Missing Reports: ";
    private const string MissingReportsText = "Reports should be filed in chronological order for each calendar year. Failure to do so could create incorrect tax contribution calculations and impact your tax liability.";
    private const string CssLate = "custom-cell-late";
    private const string CssDueSoon = "custom-cell-due-soon";
    private const string CssCurrentReportNeeded = "custom-cell-report-needed";

    private bool _showOutOfOrderModal;
    private MissingReportModel? _pendingReport;
    /// <inheritdoc/>
    protected override void OnParametersSet()
    {
        _gridData = ReportData.MissingReports;
        _hasActiveAudit = ReportData.HasActiveAudits;
        _hasPendingReports = ReportData.PendingReports.Count > 0;
        _pendingMessageStore = new ValidationMessageStore(_pendingEditContext);
        _hasMissingReports = ReportData.MissingReports.GroupBy(x =>
        {
            return x.Year;
        }).Any(y =>
        {
            return y.Count() > 1;
        });
    }

    private bool IsFileReportDisabled(MissingReportModel report)
    {
        return _hasPendingReports ||
            (_hasActiveAudit &&
             report.DueDate.HasValue &&
             report.DueDate.Value < DateTime.Today &&
             IsReportInAuditPeriod(report.Quarter, report.Year, ReportData.AuditActualPeriodStartDate, ReportData.AuditActualPeriodEndDate));
    }

    private static bool IsReportInAuditPeriod(int reportQuarter, int reportYear, DateTime? auditStartDate, DateTime? auditDate)
    {
        if (!auditStartDate.HasValue || !auditDate.HasValue)
        {
            return true;
        }

        var auditStartYear = auditStartDate.Value.Year;
        if (reportYear < auditStartYear)
        {
            return false;
        }

        var currentQuarter = ((auditDate.Value.Month - 1) / 3) + 1;
        var endQuarter = currentQuarter == 1 ? 4 : currentQuarter - 1;
        var endYear = currentQuarter == 1 ? auditDate.Value.Year - 1 : auditDate.Value.Year;

        return reportYear < endYear || (reportYear == endYear && reportQuarter <= endQuarter);
    }


    private string GetStatusCss(MissingReportModel report)
    {
        if (!String.IsNullOrWhiteSpace(report.Status))
        {
            if (report.Status.StartsWith("Late", StringComparison.OrdinalIgnoreCase))
            {
                return CssLate;
            }
            if (report.Status.StartsWith("Due Soon", StringComparison.OrdinalIgnoreCase))
            {
                return CssDueSoon;
            }
            if (report.Status.StartsWith("Current Report Needed", StringComparison.OrdinalIgnoreCase))
            {
                return CssCurrentReportNeeded;
            }
        }
        return String.Empty;
    }

    private MarkupString GetReportIcon(MissingReportModel report)
    {
        MarkupString iconMarkup;
        iconMarkup = report switch
        {
            { Status: string s } when s.StartsWith("Late", StringComparison.OrdinalIgnoreCase) => GetLateReportIcon(),
            { Status: string s } when s.StartsWith("Due Soon", StringComparison.OrdinalIgnoreCase) => GetDueSoonIcon(),
            { Status: string s } when s.StartsWith("Current Report Needed", StringComparison.OrdinalIgnoreCase) => GetCurrentReportNeededIcon(),
            _ => new MarkupString()
        };

        return iconMarkup;
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

        _gridData = PerformSort(column, _sortAscending, _gridData);
    }

    private IEnumerable<MissingReportModel> PerformSort(string columnName, bool sortAscending, IEnumerable<MissingReportModel> reports)
    {
        if (columnName == "quarter")
        {
            return SortByQuarter(sortAscending, reports);
        }

        Func<MissingReportModel, Object> orderByFunc;

        orderByFunc = columnName switch
        {
            "quarter" => item =>
            {
                return item.Year;
            }
            ,
            "reportName" => item =>
            {
                return String.IsNullOrEmpty(item.ReportName) ? String.Empty : item.ReportName;
            }
            ,
            "statusDueDate" => item =>
            {
                return item.DueDate ?? DateTime.MinValue;
            }
            ,
            _ => item =>
            {
                return item.Quarter;
            }
        };

        var sorted = sortAscending
            ? reports.OrderBy(orderByFunc)
            : reports.OrderByDescending(orderByFunc);

        return sorted;
    }

    private static IEnumerable<MissingReportModel> SortByQuarter(bool sortAscending, IEnumerable<MissingReportModel> reports)
    {
        return sortAscending
            ? reports.OrderBy(OrderByYear).ThenBy(OrderByQuarter)
            : reports.OrderByDescending(OrderByYear).ThenByDescending(OrderByQuarter);
    }

    private static int OrderByYear(MissingReportModel item)
    {
        return item.Year;
    }

    private static int OrderByQuarter(MissingReportModel item)
    {
        return item.Quarter;
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

    private MarkupString GetActiveAuditIcon()
    {
        return GetImageAndAltText("images/reports/active-audit-icon.svg", "");
    }

    private MarkupString GetMissingReportsIcon()
    {
        return GetImageAndAltText("images/reports/missing-reports-icon.svg", "");
    }

    private MarkupString GetLateReportIcon()
    {
        return GetImageAndAltText("images/reports/late-report-icon.svg", "");
    }

    private MarkupString GetDueSoonIcon()
    {
        return GetImageAndAltText("images/reports/due-soon-icon.svg", "");
    }

    private MarkupString GetCurrentReportNeededIcon()
    {
        return GetImageAndAltText("images/reports/current-report-needed-icon.svg", "");
    }

    private MarkupString GetImageAndAltText(string path, string altText)
    {
        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private async Task HandleFileReport(MissingReportModel report)
    {
        if (IsReportOutOfOrder(report))
        {
            _pendingReport = report;
            _showOutOfOrderModal = true;
            return;
        }

        await ProceedWithFileReport(report);
    }

    private bool IsReportOutOfOrder(MissingReportModel report)
    {
        return ReportData.MissingReports.Any(r =>
        {
            return r.Year == report.Year && r.Quarter < report.Quarter;
        });
    }

    private async Task HandleOutOfOrderConfirm()
    {
        _showOutOfOrderModal = false;
        if (_pendingReport is not null)
        {
            await ProceedWithFileReport(_pendingReport);
        }
    }

    private void HandleOutOfOrderCancel()
    {
        _showOutOfOrderModal = false;
        _pendingReport = null;
    }

    private async Task ProceedWithFileReport(MissingReportModel report)
    {
        await QuarterlyReportOrchestrator.SaveMissingReportToSessionAsync(report);
        NavigationManager.NavigateTo("quarterly-tax/select-report", true);
    }

    private static string GetReportNameByFilingSelectionType(int? reportingSelectionCodeSK)
    {
        return reportingSelectionCodeSK switch
        {
            1 => "Tax & Wage Entry",
            2 => "Tax & Wage Upload ",
            3 => "Tax Report Only",
            4 => "Zero Payroll this Quarter",
            5 => "Wage Upload",
            6 => "Wage Entry",
            _ => "Tax & Wage Report",
        };
    }

    private async Task HandleContinuePendingReport(WageTaxFilingProxy pending, MissingReportModel? missingReport)
    {
        _pendingMessageStore.Clear();
        _showPendingErrorrs = false;

        var report = missingReport ?? new MissingReportModel()
        {
            Quarter = pending.Quarter ?? missingReport!.Quarter,
            Year = pending.Year ?? missingReport!.Year,
            FormattedQuarterYear = $"Q{pending.Quarter} {pending.Year}",
            ReportName = GetReportNameByFilingSelectionType(pending.ReportingSelectionCodeSK),
        };

        report.PendingWageTaxFilingSK = pending.WageTaxFilingSK;
        report.PendingFilingTypeCodeSK = pending.FilingTypeCodeSK;
        report.PendingReportingSelectionCodeSK = pending.ReportingSelectionCodeSK;

        if (pending.FilingTypeCodeSK != null)
        {
            var filingMethod = await TaxAndWageEntryService.GetAvailableTaxAndReportFilingMethod((TaxWageFilingType) pending.ReportingSelectionCodeSK!);

            if (filingMethod is null)
            {
                _pendingMessageStore.Add(_pendingEditContext.Field(string.Empty), "Cannot find Filing Method.");
                _showPendingErrorrs = true;
                _pendingEditContext.NotifyValidationStateChanged();
                return;
            }
        }

        await QuarterlyReportOrchestrator.SaveMissingReportToSessionAsync(report);

        var url = pending.ReportingSelectionCodeSK switch
        {
            1 => "quarterly-tax/tax-and-wage-entry-report",
            2 => "quarterly-tax/tax-report-only?source=tax-wage-upload",
            3 => "quarterly-tax/tax-report-only",
            6 => "quarterly-tax/wage-entry-report",
            _ => "quarterly-tax/select-report"
        };

        NavigationManager.NavigateTo(url, true);
    }
}
