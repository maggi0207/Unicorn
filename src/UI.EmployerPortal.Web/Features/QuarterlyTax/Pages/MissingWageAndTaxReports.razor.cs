using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// MissingWageAndTaxReports
/// </summary>
public partial class MissingWageAndTaxReports
{
    private MissingWageAndTaxReportsData _reportData = default!;

    private bool _userHasAssociatedEmployers = false;

    private bool _userHasSelectedEmployer = false;

    private bool _isLoading = true;

    private ElementReference _headingRef;

    private bool _hasFocused = false;

    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;

    /// <summary>
    /// OnInitializedAsync
    /// </summary>
    /// <returns></returns>
    protected override async Task OnAuthorizedInitAsync()
    {
        //defensive coding
        await QuarterlyReportOrchestrator.ClearMissingReportFromSessionAsync();

        _reportData = await TaxAndWageEntryService.GetMissingWageAndTaxReports();
        //default sort.
        if (_reportData != null && _reportData.MissingReports != null && _reportData.MissingReports.Count > 0)
        {
            _reportData.MissingReports = [.. _reportData.MissingReports.OrderBy(mr =>
            {
                return mr.Year;
            }).ThenBy(mr =>
            {
                return mr.Quarter;
            })];
        }
        _userHasAssociatedEmployers = await QuarterlyReportOrchestrator.UserHasAssociatedEmployers();
        _userHasSelectedEmployer = await QuarterlyReportOrchestrator.UserHasSelectedEmployer();

        if (_userHasSelectedEmployer)
        {
            var pendingResponse = await TaxAndWageEntryService.GetPendingTaxWagesForEmployerAsync();
            _reportData?.PendingReports =
                pendingResponse != null &&
                pendingResponse!.WageTaxFilingCollection != null &&
                pendingResponse!.WageTaxFilingCollection.Length > 0
                ? [.. pendingResponse.WageTaxFilingCollection]
                : [];
        }
        _isLoading = false;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!_isLoading && !_hasFocused)
        {
            _hasFocused = true;
            await _headingRef.FocusAsync();
        }
    }
}
