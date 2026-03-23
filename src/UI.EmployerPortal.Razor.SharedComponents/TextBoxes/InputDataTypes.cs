namespace UI.EmployerPortal.Razor.SharedComponents.TextBoxes;

/// <summary>
/// Specifies the data type for a <see cref="NumericTextBox"/> input, controlling
/// formatting and display behaviour.
/// </summary>
public enum InputDataTypes
{
    /// <summary>Plain numeric input with no special formatting.</summary>
    None,

    /// <summary>Currency input formatted with two decimal places.</summary>
    Currency,

    /// <summary>Percentage input formatted with a percent suffix.</summary>
    Percentage,
}
