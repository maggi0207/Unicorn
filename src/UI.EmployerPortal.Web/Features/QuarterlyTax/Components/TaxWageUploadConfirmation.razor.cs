using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;
/// <summary>
/// Tax and Wage upload confirmation page after Tax Entry
/// </summary>
public partial class TaxWageUploadConfirmation : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// Gets or sets Reporting Quarter to Display in Confirmation page
    /// </summary>
    [Parameter] public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Due Date to Display in Confirmation page
    /// </summary>
    [Parameter] public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets Effective Date in Confirmation page
    /// </summary>
    [Parameter] public DateTime? EffectiveDate { get; set; }

    // --- Confirmation ---
    /// <summary>
    /// Gets or sets Confirmation Number received from submit response
    /// </summary>
    [Parameter] public string ConfirmationNumber { get; set; } = string.Empty;

    private void HandleContinueToWageUpload()
    {
        NavigationManager.NavigateTo("quarterly-tax/wage-upload");
    }

    private async Task HandleSavePageClicked()
    {
        await JS.InvokeVoidAsync("printElement", ".confirmation-page");
    }
}
