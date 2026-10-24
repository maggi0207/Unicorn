using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

/// <summary>
/// NoPermissionsSelectedModal
/// </summary>
public partial class NoPermissionsSelectedModal
{
    /// <summary>
    /// IsOpen
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// True while the removal is in flight. Replaces the body with a spinner and hides the
    /// buttons, so CONTINUE cannot be pressed twice.
    /// </summary>
    [Parameter]
    public bool IsBusy { get; set; }

    /// <summary>
    /// Shown in place of the warning text when the removal fails, so the failure is reported
    /// where the user pressed the button.
    /// </summary>
    [Parameter]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// OnCancel
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// OnContinue
    /// </summary>
    [Parameter]
    public EventCallback OnConfirm { get; set; }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }

    private async Task HandleConfirm()
    {
        await OnConfirm.InvokeAsync();
    }
}
