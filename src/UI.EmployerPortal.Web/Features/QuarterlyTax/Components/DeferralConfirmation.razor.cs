using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;

/// <summary>
/// Confirmation component for First Quarter Deferral election.
/// Based on the ReportConfirmation layout but tailored for the deferral flow,
/// which has unique policy requirements in the "What's Next" section and
/// does not require Payment Options, Upload Wage File, or email subscription features.
/// </summary>

public partial class DeferralConfirmation : ComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Gets or sets Reporting Quarter to display in Confirmation page.
    /// </summary>

    [Parameter] public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets Due Date to display in Confirmation page.
    /// </summary>

    [Parameter] public DateTime? DueDate { get; set; }

    /// <summary>
    /// Gets or sets Effective Date in Confirmation page.
    /// </summary>

    [Parameter] public DateTime? EffectiveDate { get; set; }

    /// <summary>
    /// Gets or sets Confirmation Number received from submit response.
    /// </summary>

    [Parameter] public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// Callback invoked when "File Another Report" is clicked.
    /// </summary>

    [Parameter] public EventCallback OnFileAnotherReportClicked { get; set; }

    /// <summary>
    /// Set focus on the h1 heading when the page loads for accessibility so that users are aware of the success status
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JS.InvokeVoidAsync("focusElement", "confirmation-heading");
        }
    }
    /// <summary>
    /// Invokes window.print to save the page.
    /// </summary>

    public async Task HandleSavePageClicked()
    {
        await JS.InvokeVoidAsync("printElement", ".confirmation-page");
    }
    private async Task HandleFileAnotherReportClicked()
    {
        await OnFileAnotherReportClicked.InvokeAsync();
    }
}
