namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

/// <summary>
///  Represents a single radio option
/// </summary>
/// <typeparam name="T"></typeparam>
/// Type of value associated with the radio option
public class RadioOption<T>
{
    /// <summary>
    /// gets or sets the value associated with the radio button
    /// </summary>
    public T Value { get; set; } = default!;
    /// <summary>
    /// Gets or sets the display Label shown to the user
    /// </summary>
    public string Label { get; set; } = String.Empty;
}
