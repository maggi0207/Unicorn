using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public interface IAddressValidationWrapper
{
    Task<AddressValidationResult> ValidateAsync(AddressModel address);
}

public record AddressValidationResult(bool IsValid, string? ErrorMessage, AddressModel? CorrectedAddress);
