using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components;


/// <summary>
/// Collapsible Tax Details component for displaying tax information in verification and review pages.
/// </summary>
public partial class TaxDetails
{
    /// <summary>
    /// Gets or sets the tax report data to display
    /// </summary>
    [Parameter]
    public TaxDetailsModel? ReportData { get; set; }

    /// <summary>
    /// Event callback invoked when the Edit link is clicked.
    /// </summary>
    [Parameter]
    public EventCallback OnEdit { get; set; }

    /// <summary>
    /// Gets Or Sets Confirmation Number
    /// If null or empty, nothing is rendered. Other Usages are unaffected.
    /// </summary>
    [Parameter]
    public string? ConfirmationNumber { get; set; }

    /// <summary>
    /// Gets Or Sets whether the Edit button is shown. Defaults to true.
    /// Set to false on read-only pages. Other Usages are unaffected.
    /// </summary>
    [Parameter]
    public bool ShowEdit { get; set; } = true;

    /// <summary>
    /// Gets or sets whether the component is initially expanded.
    /// </summary>
    [Parameter]
    public bool IsExpanded { get; set; } = true;

    /// <summary>
    /// Captures additional attributes (like style, class, id) to apply to the root element.
    /// </summary>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object>? AdditionalAttributes { get; set; }

    private bool _isExpanded;

    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override void OnInitialized()
    {
        _isExpanded = IsExpanded;
    }

    private void ToggleExpand()
    {
        _isExpanded = !_isExpanded;
    }

    private async Task HandleEditClick()
    {
        await OnEdit.InvokeAsync();
    }
}


