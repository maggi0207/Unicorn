using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Pages;

/// <summary>
/// Code-behind for the employer registration wizard page.
/// Hosts all registration steps and delegates validation to each step component.
/// </summary>
public partial class EmployerRegistrationSteps
{
    [Inject] private NavigationManager Nav { get; set; } = default!;
    [Inject] private RegistrationStateService RegistrationState { get; set; } = default!;

    private int _currentStep = 1;

    private BusinessInformation? _businessInformationRef;
    private BusinessContact?     _businessContactRef;

    /// <summary>Wizard step definitions matching the full employer registration flow.</summary>
    private readonly List<WizardStep> _wizardSteps = new()
    {
        new() { StepNumber = 1, Icon = "/icons/employer_reg_step_1.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 2, Icon = "/icons/employer_reg_step_2.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 3, Icon = "/icons/employer_reg_step_3.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 4, Icon = "/icons/employer_reg_step_4.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 5, Icon = "/icons/employer_reg_step_5.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 6, Icon = "/icons/employer_reg_step_6.svg", ActionButtonText = "CONTINUE" },
        new() { StepNumber = 7, Icon = "/icons/employer_reg_step_7.svg", ActionButtonText = "SUBMIT"   },
    };

    /// <summary>Restores the current step from state (e.g., after returning from the Address Correction page).</summary>
    protected override void OnInitialized()
    {
        if (RegistrationState.CurrentStep > 0)
        {
            _currentStep = RegistrationState.CurrentStep;
            RegistrationState.CurrentStep = 0;
        }
    }

    /// <summary>Delegates validation to the active step component and advances the wizard if valid.</summary>
    private async Task HandleActionClick()
    {
        bool isValid;

        if (_currentStep == 3)
            isValid = await (_businessInformationRef?.Validate() ?? Task.FromResult(false));
        else if (_currentStep == 4)
            isValid = await (_businessContactRef?.Validate() ?? Task.FromResult(false));
        else
            isValid = true;

        if (!isValid) return;

        if (_currentStep == _wizardSteps.Count)
        {
            await HandleSubmit();
            return;
        }

        _currentStep++;
    }

    /// <summary>Handles Back click on the first step by navigating to the welcome page.</summary>
    private async Task HandleBackClick()
    {
        if (_currentStep == 1)
            Nav.NavigateTo("/employer-registration/employer-registration-welcome");
    }

    /// <summary>Submits the completed registration via WCF service calls.</summary>
    private async Task HandleSubmit()
    {
        // WCF Service calls to save and register
    }
}
