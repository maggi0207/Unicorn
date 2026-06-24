namespace UI.EmployerPortal.Web.Features.Shared.NotificationBanners;

using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;

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
public partial class NotificationBanner
{
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
    /// HTML link for the Action button.  Optional but requires Action if used.
    /// Right justified
    /// </summary>
    [Parameter]
    public string? ActionLink { get; set; }
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

    private string GetFormattedMessage()
    {
        return !String.IsNullOrWhiteSpace(Message)
            ? Message.Length <= 50 ? Message : string.Concat(Message.AsSpan(0, 50), "...")
            : String.Empty;
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
        return !String.IsNullOrWhiteSpace(ActionLink) && !String.IsNullOrWhiteSpace(Action);
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
            NotificationType.Alert => "nb-alert",
            NotificationType.Warning => "nb-warning",
            NotificationType.Information => "nb-information",
            NotificationType.Confirmation => "nb-confirmation",
            _ => String.Empty,
        };

    }

    private MarkupString GetAlertNotificationIcon()
    {
        var icon = "/icons/alert-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetWarningNotificationIcon()
    {
        var icon = "/icons/warning-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetInformationNotificationIcon()
    {
        var icon = "/icons/information-notification.svg";
        var altText = "Information";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetConfirmationNotificationIcon()
    {
        var icon = "/icons/confirmation-notification.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetDismissIcon()
    {
        var icon = "/icons/dismiss-icon.svg";
        var altText = "Alert";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetOverdueIcon()
    {
        var icon = "/icons/overdue-icon.svg";
        var altText = "Overdue";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }

    private MarkupString GetCheckIcon()
    {
        var icon = "/icons/check-icon.svg";
        var altText = "Achieved";

        return new MarkupString($"<img src='{icon}' class='sort-icon' alt='{altText}' />");
    }
}
