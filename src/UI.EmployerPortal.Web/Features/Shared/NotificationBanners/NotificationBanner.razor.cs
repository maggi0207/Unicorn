namespace UI.EmployerPortal.Razor.SharedComponents.NotificationBanners;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

//Example usage:

//<NotificationBanner
//    NotificationType = "NotificationType.Alert"
//    Title="Alert notification"
//    Message="Message for the alert notification">
//</NotificationBanner>

//<NotificationBanner
//    NotificationType = "NotificationType.Warning"
//    Title="Warning notification - expanded"
//    Message="Message for the alert notification"
//    DueDate="@_dt"
//    Action="Take Action"
//    ActionLink="/employer-registration"
//    DueDateState="DueDateState.Upcoming"
//    DismissButton="true">
//</NotificationBanner>

/// <summary>
///
/// </summary>
public partial class NotificationBanner : IAsyncDisposable
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    [Inject] private NavigationManager Nav { get; set; } = default!;

    // JS module reference for focus helpers (validation.js)
    private IJSObjectReference? _module;

    /// <summary>
    /// Whether this banner announces itself as an alert. Defaults to true. Set false when the page
    /// already owns a persistend live region carrying the same text, so the message is not announced twice.
    /// </summary>
    [Parameter]
    public bool Announce { get; set; } = true;

    // Reference to the MultiLine banner div so we can auto-focus it on first render
    private ElementReference _bannerElement;

    /// <summary>
    /// The type of notification
    /// </summary>
    [Parameter, Required(ErrorMessage = "NotificationType is required")]
    public NotificationType NotificationType { get; set; }
    /// <summary>
    /// Title - Optional
    /// </summary>
    [Parameter]
    public string? Title { get; set; }
    /// <summary>
    /// Message - Optional. Use GetFormattedMessage() as the message should be truncated after 50 characters
    /// </summary>
    [Parameter]
    public string? Message { get; set; }
    /// <summary>
    /// Due Date - Optional. Right justified
    /// </summary>
    [Parameter]
    public DateTime? DueDate { get; set; }
    /// <summary>
    /// Action button text.  Optional but requires ActionLink if used.
    /// Right justified
    /// </summary>
    [Parameter]
    public string? Action { get; set; }
    /// <summary>
    /// HTML link for the Action button. Optional but required if Action is used and no ActionCallback is passed.
    /// Right justified
    /// </summary>
    [Parameter]
    public string? ActionLink { get; set; }
    /// <summary>
    /// Callback for the Action button. Optional but required if Action is used and no ActionLink is passed.
    /// </summary>
    [Parameter]
    public EventCallback ActionCallback { get; set; }
    /// <summary>
    /// DismissButton to close the form. Optional.
    /// Right justified.
    /// </summary>
    [Parameter]
    public bool DismissButton { get; set; }
    /// <summary>
    /// Determines the icon displayed before the Due Date
    /// </summary>
    [Parameter]
    public DueDateState DueDateState { get; set; } = DueDateState.None;
    /// <summary>
    /// Hides the form when set to false.
    /// </summary>
    [Parameter]
    public bool Visible { get; set; } = true;

    /// <summary>
    ///
    /// </summary>
    [Parameter]
    public MessageStyle MessageStyle { get; set; } = MessageStyle.SingleLine;

    /// <summary>
    /// /
    /// </summary>
    [Parameter]
    public List<string> Messages { get; set; } = new();

    /// <summary>
    /// Obsolete: No longer used for truncation. Kept for backwards compatibility only.
    /// Messages are now displayed in full and wrap within the banner.
    /// </summary>
    [Parameter]
    public int MessageLimit { get; set; } = int.MaxValue;

    /// <summary>
    /// Maps property names to their corresponding input HTML ids for anchor link navigation.
    /// </summary>
    [Parameter]
    public List<string> MessageFieldIds { get; set; } = new();

    /// <summary>
    /// Maps message index to their corresponding input HTML dataset attribute ids for anchor link navigation.
    /// </summary>
    [Parameter]
    public List<string> MessageFieldDataIds { get; set; } = new();

    /// <summary>
    /// Optional Child Content
    /// </summary>
    [Parameter]
    public RenderFragment? ChildContent { get; set; } = null;

    //private IJSObjectReference? _module;
    // _module and _bannerElement declared above

    private string GetFormattedTitle()
    {
        return String.IsNullOrWhiteSpace(Title) ? String.Empty : Title.TrimEnd().EndsWith(":") ? String.Empty : $"{Title}";
    }

    /// <summary>
    /// GetDisplayTitle
    /// </summary>
    /// <returns></returns>
    public string GetDisplayTitle()
    {
        return !String.IsNullOrWhiteSpace(Title)
            ? Title.TrimEnd()
            : NotificationType == NotificationType.Alert && MessageStyle == MessageStyle.MultiLine
            ? "Please fix the following errors:"
            : String.Empty;
    }

    private string GetFormattedMessage()
    {
        return !String.IsNullOrWhiteSpace(Message) ? Message : String.Empty;
    }

    private bool IsRightAlignedContent()
    {
        return DismissButton
            || HasAction()
            || DueDate.HasValue;
    }

    private void Hide()
    {
        Visible = false;
    }

    private bool HasAction()
    {
        return !String.IsNullOrWhiteSpace(Action) &&
            (!String.IsNullOrWhiteSpace(ActionLink) || ActionCallback.HasDelegate);
    }

    private void PerformAction()
    {
        if (ActionLink != null)
        {
            Nav.NavigateTo(ActionLink);
        }
        if (ActionCallback.HasDelegate)
        {
            ActionCallback.InvokeAsync();
        }
    }

    private string GetDueDateText()
    {
        return DueDate.HasValue ? $"Due: {DueDate.Value:MM/dd/yyyy}" : String.Empty;
    }

    private MarkupString GetNotificationIcon()
    {
        return NotificationType switch
        {
            NotificationType.Alert => GetAlertNotificationIcon(),
            NotificationType.Warning => GetWarningNotificationIcon(),
            NotificationType.Information => GetInformationNotificationIcon(),
            NotificationType.Confirmation => GetConfirmationNotificationIcon(),
            _ => new MarkupString(String.Empty),
        };
    }

    private MarkupString GetDueDateIcon()
    {
        return DueDateState switch
        {
            DueDateState.Overdue => GetOverdueIcon(),
            DueDateState.Upcoming => GetOverdueIcon(),
            DueDateState.Achieved => GetCheckIcon(),
            DueDateState.None => new MarkupString(String.Empty),
            _ => new MarkupString(String.Empty),
        };
    }

    private string GetCssClass()
    {
        return NotificationType switch
        {
            NotificationType.Alert => MessageStyle == MessageStyle.SingleLine ? "nb-alert" : "nb-alert-multiline",
            NotificationType.Warning => MessageStyle == MessageStyle.SingleLine ? "nb-warning" : "nb-warning-multiline",
            NotificationType.Information => MessageStyle == MessageStyle.SingleLine ? "nb-information" : "nb-information-multiline",
            NotificationType.Confirmation => MessageStyle == MessageStyle.SingleLine ? "nb-confirmation" : "nb-confirmation-multiline",
            _ => String.Empty,
        };

    }

    private MarkupString GetAlertNotificationIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/alert-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetWarningNotificationIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/warning-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetInformationNotificationIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/information-notification.svg";
        var altText = "Information";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetConfirmationNotificationIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/confirmation-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetDismissIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/dismiss-icon.svg";
        var altText = "Dismiss";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetOverdueIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/overdue-icon.svg";
        var altText = "Overdue";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetCheckIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/check-icon.svg";
        var altText = "Achieved";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetRoundBulletPointIcon()
    {
        var icon = "_content/UI.EmployerPortal.Razor.SharedComponents/icons/round-bullet-point.svg";
        var altText = "BulletPoint";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private string GetFieldId(int index)
    {
        return index < MessageFieldIds.Count ? MessageFieldIds[index] : string.Empty;
    }

    /// <summary>
    /// On first render, auto-focuses the banner when it is used as a validation
    /// error summary (Alert + MultiLine with messages).
    /// </summary>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (NotificationType == NotificationType.Alert
        && MessageStyle == MessageStyle.MultiLine
        && Messages.Count > 0)
        {
            _module = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/validation.js");

            await _module.InvokeVoidAsync("focusElementRef", _bannerElement);
        }
    }

    /// <summary>
    /// Scrolls to and focuses the target field via the imported JS module.
    /// </summary>
    private string GetFieldDataId(int index)
    {
        return index < MessageFieldDataIds.Count ? MessageFieldDataIds[index] : string.Empty;
    }

    private async Task FocusFieldAsync(string inputId)
    {
        if (!string.IsNullOrWhiteSpace(inputId))
        {
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./js/validation.js");

            await _module.InvokeVoidAsync("focusElement", inputId);
        }
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            try
            {
                await _module.DisposeAsync();
            }
            catch (JSDisconnectedException)
            {
                // Circuit already disconnected (page refresh / navigation)
            }
            catch (ObjectDisposedException)
            {
                // JS runtime already disposed
            }
        }
    }

    private async Task FocusFieldByDatasetIdAsync(string datasetId)
    {
        if (!string.IsNullOrWhiteSpace(datasetId))
        {
            await JSRuntime.InvokeVoidAsync("focusDatasetIdElement", datasetId);
        }
    }
}
