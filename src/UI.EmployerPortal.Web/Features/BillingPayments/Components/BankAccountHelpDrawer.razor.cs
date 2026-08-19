using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;

/// <summary>
/// Side panel drawer displaying Bank Account Help content.
/// </summary>
public partial class BankAccountHelpDrawer
{
    /// <summary>
    /// Controls whether the drawer is visible.
    /// </summary>
    [Parameter] public bool IsOpen { get; set; }

    /// <summary>
    /// Invoked when the user closes the drawer.
    /// </summary>
    [Parameter] public EventCallback OnClose { get; set; }

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
