using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.ESP.Components;

/// <summary>
/// Confirmation modal shown before removing an ESP client relationship.
/// </summary>
public partial class RemoveClientModal
{
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
