using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UI.EmployerPortal.Web.Features.ESP.Components;

/// <summary>
/// Confirmation modal shown before removing an ESP client relationship.
/// </summary>
public partial class RemoveClientModal
{
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;
    /// <summary>
    /// Whether the modal is currently displayed.
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// Whether a removal request is in flight.
    /// </summary>
    [Parameter]
    public bool IsBusy { get; set; }

    /// <summary>
    /// Error message to display when a removal attempt fails.
    /// </summary>
    [Parameter]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// The client account name shown in the confirmation text.
    /// </summary>
    [Parameter]
    public string ClientDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Invoked when the user cancels the removal.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Invoked when the user confirms the removal.
    /// </summary>
    [Parameter]
    public EventCallback OnConfirm { get; set; }
    private ElementReference _removeModalRef;
    private IJSObjectReference? _module;
    private bool _wasOpen;

    /// <summary>
    /// Set and trap focus in modal when it opens
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen && !_wasOpen)
        {
            _wasOpen = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _removeModalRef);
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

    private async Task HandleCancel()
    {
        if (IsBusy)
        {
            return;
        }
        await OnCancel.InvokeAsync();
    }

    private async Task HandleConfirm()
    {
        await OnConfirm.InvokeAsync();
    }
}
