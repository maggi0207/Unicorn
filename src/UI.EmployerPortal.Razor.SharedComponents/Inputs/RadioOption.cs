namespace UI.EmployerPortal.Razor.SharedComponents.Inputs;

/// <summary>
/// Represents a single option in a <see cref="RadioGroup{TValue}"/> component,
/// pairing a typed value with a display label.
/// </summary>
/// <typeparam name="TValue">The type of the radio option value.</typeparam>
public class RadioOption<TValue>
{
    /// <summary>
    /// The underlying value submitted when this option is selected.
    /// </summary>
    public TValue? Value { get; set; }

    /// <summary>
    /// The human-readable label rendered next to the radio button.
    /// </summary>
    public string Label { get; set; } = string.Empty;
}
