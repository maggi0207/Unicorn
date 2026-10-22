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
    /// OnCancel
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// OnContinue
    /// </summary>
    [Parameter]
    public EventCallback OnContinue { get; set; }

    private async Task HandleCancel()
    {
        await OnCancel.InvokeAsync();
    }

    private async Task HandleContinue()
    {
        await OnContinue.InvokeAsync();
    }
}
