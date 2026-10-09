using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;

/// <summary>
/// Side panel drawer displaying International ACH Transaction (IAT) important information.
/// </summary>
public partial class IatInfoDrawer
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    /// <summary>
    /// Controls whether the drawer is visible.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }

    /// <summary>
    /// Invoked when the user closes the drawer.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

    private ElementReference _modalRef;
    private bool _wasVisible;
    private IJSObjectReference? _module;

    /// <summary>
    /// Calls JS to set and trap focus in modal
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !_wasVisible)
        {
            _wasVisible = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _modalRef);
        }
        else if (!IsOpen && _wasVisible)
        {
            _wasVisible = false;
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("closeFocusTrap");
            }
        }
    }

    private void HandleClose()
    {
        _ = OnClose.InvokeAsync();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            HandleClose();
        }
    }
}
