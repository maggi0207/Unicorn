using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Abstraction over the SOAP address validation service.
/// Allows the real WCF implementation to be swapped for a stub in tests.
/// </summary>
public interface IAddressValidationWrapper
{
    /// <summary>
    /// Validates the given address against the address validation service.
    /// Returns <see cref="AddressValidationResult.IsValid"/> = true when the address is deliverable.
    /// </summary>
    Task<AddressValidationResult> ValidateAsync(AddressModel address);
}

/// <summary>
/// Result returned by <see cref="IAddressValidationWrapper.ValidateAsync"/>.
/// </summary>
/// <param name="IsValid">True when the address passed validation (no error from the service).</param>
/// <param name="ErrorMessage">Service error message when <paramref name="IsValid"/> is false; otherwise null.</param>
/// <param name="CorrectedAddress">Standardized address returned by the service, or null if the service could not find a match.</param>
public record AddressValidationResult(bool IsValid, string? ErrorMessage, AddressModel? CorrectedAddress);
