using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// GeneratedAccessKey
/// </summary>
public partial class GeneratedAccessKey
{
    [Inject]
    private IGenerateAccessKeyFlow Flow { get; set; } = default!;

    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject]
    private NavigationManager Navigation { get; set; } = default!;

    private bool _showCopiedBanner = false;

    private string KeyTypeLabel => Flow.SelectedKeyType == "manager" ? "Manager" : "Worker";

    /// <inheritdoc/>
    protected override void OnInitialized()
    {
        if (string.IsNullOrWhiteSpace(Flow.GeneratedKey))
        {
            Navigation.NavigateTo("manage-account/account-users");
        }
    }

    private async Task CopyKeyToClipboardAsync()
    {
        if (string.IsNullOrWhiteSpace(Flow.GeneratedKey))
        {
            return;
        }

        await JSRuntime.InvokeVoidAsync("navigator.clipboard.writeText", Flow.GeneratedKey);
        _showCopiedBanner = true;
    }

    private void HandleBack()
    {
        Navigation.NavigateTo("manage-account/account-users/generate-key");
    }

    private void HandleDone()
    {
        Flow.Reset();
        Navigation.NavigateTo("manage-account/account-users");
    }
}
