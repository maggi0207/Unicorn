using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Code-behind for the Previously Filed Report Details page.
/// Renders TaxDetails and WageDetails for the selected quarter and year.
/// </summary>
public partial class PreviouslyFiledReportDetails
{
    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;

    /// <summary>
    /// Quarter
    /// </summary>
    [SupplyParameterFromQuery(Name = "quarter")]
    public int Quarter { get; set; }

    /// <summary>
    /// YEar
    /// </summary>
    [SupplyParameterFromQuery(Name = "year")]
    public int Year { get; set; }

    private PreviouslyFiledReportDetailsModel? _details;
    private bool _isLoading = true;
    private string QuarterYear => $"Q{Quarter} {Year}";

    private WageDetails? _wageDetails;
    /// <inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _details = await TaxAndWageEntryService.GetPreviouslyFiledReportDetailsAsync(Quarter, Year);
        _isLoading = false;
    }

    private async Task HandlePrint()
    {
        if (_wageDetails is null)
        {
            await JS.InvokeVoidAsync("printElement", ".pfr-details-page");
            return;
        }

        await _wageDetails.PrintAsync(() => JS.InvokeVoidAsync("printElement", ".pfr-details-page").AsTask());
    }
}
