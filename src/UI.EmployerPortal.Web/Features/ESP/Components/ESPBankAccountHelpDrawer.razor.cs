using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace UI.EmployerPortal.Web.Features.ESP.Components;

/// <summary>
/// Side panel drawer displaying Bank Account Help content.
/// </summary>
public partial class ESPBankAccountHelpDrawer
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
    private ElementReference _modalElement;
    private IJSObjectReference? _module;
    private bool _wasOpen;

    /// <summary>
    /// Call JS functions to move focus and trap focus in dialog
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !_wasOpen)
        {
            _wasOpen = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _modalElement);
        }
        else if (!IsOpen && _wasOpen)
        {
            _wasOpen = false;
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
