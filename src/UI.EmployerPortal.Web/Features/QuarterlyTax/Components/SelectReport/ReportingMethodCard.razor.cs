using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.SelectReport;

/// <summary>
/// Component to Display Reporing Method Option
/// </summary>

public partial class ReportingMethodCard
{
    /// <summary>
    /// Html id for the card element
    /// </summary>
    [Parameter] public string? Id { get; set; }
    /// <summary>
    /// Tile of the Card
    /// </summary>
    [Parameter] public string Title { get; set; } = string.Empty;
    /// <summary>
    /// A breif Description of the Card
    /// </summary>
    [Parameter] public string Description { get; set; } = string.Empty;
    /// <summary>
    /// Custom Description Content
    /// </summary>
    [Parameter] public RenderFragment? CustomDescription { get; set; }
    /// <summary>
    /// Is User selceted this paticular card
    /// </summary>
    [Parameter] public bool IsSelected { get; set; }
    /// <summary>
    /// Is Current card Disabled
    /// </summary>
    [Parameter] public bool IsDisabled { get; set; }
    /// <summary>
    /// Whether the card is in an error state
    /// </summary>
    [Parameter] public bool IsError { get; set; }
    /// <summary>
    /// Call back event on Selection of the card
    /// </summary>
    [Parameter] public EventCallback OnSelected { get; set; }
    /// <summary>
    /// Call back event on down arrow press to change Selection of the cards
    /// </summary>
    [Parameter] public EventCallback OnNextCardSelected { get; set; }
    /// <summary>
    /// Call back event on up arrow press to change Selection of the cards
    /// </summary>
    [Parameter] public EventCallback OnPreviousCardSelected { get; set; }

    /// <summary>
    /// New control bold title only where needed
    /// </summary>
    [Parameter] public bool BoldTitle { get; set; }

    /// <summary>
    /// IsFocusable
    /// </summary>
    [Parameter] public bool IsFocusable { get; set; }

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// Prevent the spacebar from scolling on this element
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await JSRuntime.InvokeVoidAsync("preventSpacebarScrolling", Id);
        }
    }

    private async Task HandleClick()
    {
        if (!IsDisabled)
        {
            await OnSelected.InvokeAsync();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        if (!IsDisabled && (e.Key == "Enter" || e.Key == " "))
        {
            await OnSelected.InvokeAsync();
        }
        else if (e.Key == "ArrowDown")
        {
            await OnNextCardSelected.InvokeAsync();
        }
        else if (e.Key == "ArrowUp")
        {
            await OnPreviousCardSelected.InvokeAsync();
        }
    }
}
