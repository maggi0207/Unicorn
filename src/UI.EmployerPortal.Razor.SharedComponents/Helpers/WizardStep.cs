namespace UI.EmployerPortal.Razor.SharedComponents.Helpers;

/// <summary>
/// Represents a single step in the wizard component with optional per-step button text overrides.
/// </summary>
public class WizardStep
{
    /// <summary>The step number (1-based).</summary>
    public int StepNumber { get; set; }

    /// <summary>Optional icon path (e.g., "/icons/step1.svg"). When null, the step number is shown.</summary>
    public string? Icon { get; set; }

    /// <summary>Override for the Back button text on this step. Default: "Back".</summary>
    public string BackButtonText { get; set; } = "Back";

    /// <summary>Override for the Cancel button text on this step. Default: "Cancel".</summary>
    public string CancelButtonText { get; set; } = "Cancel";

    /// <summary>Controls whether the Cancel button border is visible on this step. Default: false.</summary>
    public bool ShowCancelButtonBorder { get; set; } = false;

    /// <summary>Override for the Save and Quit button text on this step. Default: "Save &amp; Quit".</summary>
    public string SaveAndQuitButtonText { get; set; } = "Save & Quit";

    /// <summary>Controls whether the Save and Quit button border is visible on this step. Default: false.</summary>
    public bool ShowSaveAndQuitButtonBorder { get; set; } = false;

    /// <summary>Override for the primary action button text on this step. Default: "Continue".</summary>
    public string ActionButtonText { get; set; } = "Continue";
}
