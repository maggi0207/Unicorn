using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.SelectReport;
/// <summary>
/// Slide-in drawer component  to display unavailable Report methods along with the reasons
/// </summary>

public partial class UnavailableMethodsDrawer
{
    /// <summary>
    /// Controls drawer visibility.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }

    /// <summary>
    /// List of methods that are NOT eligible.
    /// </summary>
    [Parameter] public IReadOnlyList<QuarterlyReportSelectionMethod> UnavailableMethods { get; set; } = Array.Empty<QuarterlyReportSelectionMethod>();

    /// <summary>
    /// Raised when the user closes the drawer.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    private async Task HandleClose()
    {
        await OnClose.InvokeAsync();
    }

}
