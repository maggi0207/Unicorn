namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
/// Represents a selectable option used in dropdowns or select components.
/// Contains a display text and its corresponding value.
/// </summary>
public class SelectOption
{
    /// <summary>
    /// Gets or sets the underlying value of the option.
    /// This is typically submitted to the backend when selected.
    /// </summary>
    public string Value { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the display text shown to the user in the UI.
    /// </summary>
    public string Text { get; set; } = string.Empty;
}
