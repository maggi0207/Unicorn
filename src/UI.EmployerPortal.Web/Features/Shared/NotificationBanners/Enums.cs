namespace UI.EmployerPortal.Web.Features.Shared.NotificationBanners;

/// <summary>
/// The type of Notification
/// </summary>
public enum NotificationType
{
    /// <summary>
    /// Red Warning box
    /// </summary>
    Alert,
    /// <summary>
    /// Yellow Warning box
    /// </summary>
    Warning,
    /// <summary>
    /// Blue Information Box
    /// </summary>
    Information,
    /// <summary>
    /// Green Confirmation Box
    /// </summary>
    Confirmation
}

/// <summary>
/// Determines the icon displayed before the due date
/// </summary>
public enum DueDateState
{
    /// <summary>
    /// Error icon
    /// </summary>
    Upcoming,
    /// <summary>
    /// Warning icon
    /// </summary>
    Overdue,
    /// <summary>
    /// Check icon
    /// </summary>
    Achieved,
    /// <summary>
    /// No icon
    /// </summary>
    None
}
