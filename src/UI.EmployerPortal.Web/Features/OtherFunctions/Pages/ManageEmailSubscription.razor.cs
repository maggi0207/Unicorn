using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.Shared.OtherFunctions.Services;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// Manage Email Subscription Confirmation page 
/// </summary>
public partial class ManageEmailSubscription
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IEmailSubscriptionService EmailSubscriptionService { get; set; } = default!;


    private bool _expanded = true;

    /// <summary>
    /// Loading spinner
    /// </summary>
    private bool _isLoading = true;


    /// <summary>
    /// Model
    /// </summary>
    protected EmailSubscription Model { get; set; } = new();

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        _isLoading = true;
        Model = await EmailSubscriptionService.GetEmailSubscriptions() ?? new EmailSubscription();
        _isLoading = false;
    }

    private async Task HandleEdit()
    {
        NavigationManager.NavigateTo("other-functions/email-subscriptions");
        Model = await EmailSubscriptionService.GetEmailSubscriptions() ?? new EmailSubscription();
    }

    private void ToggleExpanded()
    {
        _expanded = !_expanded;
    }

    private async Task HandleEmployerHome()
    {
        NavigationManager.NavigateTo("employer-dashboard");

        await Task.CompletedTask;
    }

}
