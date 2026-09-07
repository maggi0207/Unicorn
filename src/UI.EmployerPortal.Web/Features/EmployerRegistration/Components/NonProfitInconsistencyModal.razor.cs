namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

/// <summary>
/// Modal shown when a user selects Non-Profit (other) in Step 6 and answers Yes to the
/// 501(c)(3) IRS application question, which is inconsistent with their Step 1 answer.
/// Offers two options: return to Step 1 to correct the answer, or stay on Step 6 and
/// change their answer to No.
/// </summary>
public partial class NonProfitInconsistencyModal
{
    /// <summary>
    /// Tracks which radio option the user has selected in the modal.
    /// </summary>
    public enum ModalOption
    {
        /// <summary>Navigate back to Step 1 to correct the non-profit answer.</summary>
        ReturnToStep1,

        /// <summary>Stay on Step 6 and change the 501(c)(3) answer to No.</summary>
        StayOnStep6
    }

    /// <summary>
    /// Minimal form model required by EditForm / RadioGroup.
    /// </summary>
    private class ModalFormModel
    {
        public ModalOption? SelectedOption { get; set; } = ModalOption.ReturnToStep1;
    }

    private static readonly IReadOnlyList<RadioOption<ModalOption?>> ModalOptions = new[]
    {
        new RadioOption<ModalOption?> { Value = ModalOption.ReturnToStep1, Label = "Option 1: Return to Step 1 and correct your answer." },
        new RadioOption<ModalOption?> { Value = ModalOption.StayOnStep6,   Label = "Option 2: Stay on step 6 and change your answer to no." },
    };

    /// <summary>Controls the visibility of the modal.</summary>
    [Parameter] public bool IsVisible { get; set; }

    /// <summary>Raised when the user chooses Option 1 (Return to Step 1) and clicks Continue.</summary>
    [Parameter] public EventCallback OnReturnToStep1 { get; set; }

    /// <summary>Raised when the user chooses Option 2 (Stay on Step 6), clicks Continue, or presses Escape.</summary>
    [Parameter] public EventCallback OnStayOnStep6 { get; set; }

    private readonly ModalFormModel _formModel = new();
    private ElementReference _modalRef;
    private bool _wasVisible;

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Move focus into modal on open so screen readers announce it immediately
        if (IsVisible && !_wasVisible)
        {
            _wasVisible = true;
            await _modalRef.FocusAsync();
        }
        else if (!IsVisible && _wasVisible)
        {
            _wasVisible = false;
            // Reset to default selection for the next time the modal opens
            _formModel.SelectedOption = ModalOption.ReturnToStep1;
        }
    }

    private async Task HandleContinue()
    {
        if (_formModel.SelectedOption == ModalOption.ReturnToStep1)
        {
            await OnReturnToStep1.InvokeAsync();
        }
        else
        {
            await OnStayOnStep6.InvokeAsync();
        }
    }

    private async Task HandleKeyDown(KeyboardEventArgs e)
    {
        // Escape key dismissal — treat as "Stay on Step 6"
        if (e.Key == "Escape")
        {
            await OnStayOnStep6.InvokeAsync();
        }
    }
}
