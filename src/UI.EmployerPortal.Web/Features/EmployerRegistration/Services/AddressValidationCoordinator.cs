using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Scoped service that orchestrates the full address validation and correction flow.
/// Any wizard step can inject this service to validate one or more addresses against
/// the service and automatically redirect to the Address Correction page when needed.
/// </summary>
public class AddressValidationCoordinator
{
    private readonly IAddressValidationWrapper _validator;
    private readonly RegistrationStateService _state;
    private readonly NavigationManager _nav;

    /// <summary>Initialises the coordinator with its required dependencies.</summary>
    public AddressValidationCoordinator(
        IAddressValidationWrapper validator,
        RegistrationStateService state,
        NavigationManager nav)
    {
        _validator = validator;
        _state     = state;
        _nav       = nav;
    }

    /// <summary>
    /// Validates the supplied addresses against the service.
    /// <para>
    /// Returns <c>true</c> when all addresses are valid or unchanged — the caller may advance the wizard.
    /// </para>
    /// <para>
    /// Returns <c>false</c> when one or more addresses need user review. Corrections are stored in
    /// <see cref="RegistrationStateService"/> and the browser is redirected to
    /// <c>/employer-registration/address-correction</c> automatically.
    /// </para>
    /// </summary>
    /// <param name="addresses">Pairs of display label and address model to validate.</param>
    /// <param name="editStep">Wizard step to return to when the user clicks EDIT ADDRESS.</param>
    /// <param name="postCorrectionStep">Wizard step to advance to after all corrections are resolved.</param>
    /// <param name="onBeforeNavigate">
    /// Optional callback invoked just before navigation — use it to save model state into
    /// <see cref="RegistrationStateService"/> so the component can restore it on return.
    /// </param>
    public async Task<bool> ValidateAndRedirectAsync(
        IEnumerable<(string Label, AddressModel Address)> addresses,
        int editStep,
        int postCorrectionStep,
        Action? onBeforeNavigate = null)
    {
        var corrections = await AddressCorrectionHelper.CollectCorrectionsAsync(_validator, addresses);

        if (!corrections.Any())
            return true;

        onBeforeNavigate?.Invoke();

        _state.AddressCorrections  = corrections;
        _state.CorrectionIndex     = 0;
        _state.EditStep            = editStep;
        _state.PostCorrectionStep  = postCorrectionStep;

        _nav.NavigateTo("/employer-registration/address-correction");
        return false;
    }
}
