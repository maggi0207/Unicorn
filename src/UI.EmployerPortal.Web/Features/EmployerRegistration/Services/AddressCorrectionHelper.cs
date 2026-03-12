using UI.EmployerPortal.Razor.SharedComponents.Helpers;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Helper for running service-side address validation and building the list of corrections
/// to present on the Address Correction page. Shared by any wizard step that validates addresses.
/// </summary>
public static class AddressCorrectionHelper
{
    /// <summary>
    /// Validates each address in <paramref name="addresses"/> against the service.
    /// Returns a correction item for each address that is invalid or whose service-suggested
    /// standardization differs from the entered value.
    /// </summary>
    /// <param name="validator">The address validation service wrapper.</param>
    /// <param name="addresses">Pairs of display label and address model to validate.</param>
    public static async Task<List<AddressCorrectionItem>> CollectCorrectionsAsync(
        IAddressValidationWrapper validator,
        IEnumerable<(string Label, AddressModel Address)> addresses)
    {
        var corrections = new List<AddressCorrectionItem>();

        foreach (var (label, address) in addresses)
        {
            var result = await validator.ValidateAsync(address);

            var needsCorrection = !result.IsValid
                || (result.CorrectedAddress is not null
                    && !AddressHelper.AddressesAreEqual(address, result.CorrectedAddress));

            if (needsCorrection)
                corrections.Add(new AddressCorrectionItem(label, address, result.CorrectedAddress, result.ErrorMessage));
        }

        return corrections;
    }
}
