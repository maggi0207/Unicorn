using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

/// <summary>
/// RemoveUserModal
/// </summary>
public partial class RemoveUserModal
{
    /// <summary>
    /// IsOpen
    /// </summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>
    /// IsBusy
    /// </summary>
    [Parameter]
    public bool IsBusy { get; set; }

    /// <summary>
    /// ErrorMessage
    /// </summary>
    [Parameter]
    public string? ErrorMessage { get; set; }

    /// <summary>
    /// ErrorMessage
    /// </summary>
    [Parameter]
    public string UserDisplayName { get; set; } = string.Empty;

    /// <summary>
    /// OnCancel
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// OnConfirm
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
