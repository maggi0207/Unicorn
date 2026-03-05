using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Wraps the generated WCF <see cref="GeneratedClient.IAddressValidationService"/> client
/// and maps request/response types to the application's <see cref="AddressModel"/>.
/// </summary>
public class AddressValidationService : IAddressValidationWrapper
{
    private readonly GeneratedClient.IAddressValidationService _client;

    public AddressValidationService(GeneratedClient.IAddressValidationService client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<AddressValidationResult> ValidateAsync(AddressModel address)
    {
        var request = new GeneratedClient.AddressProxy
        {
            AddressRequestType = GeneratedClient.AddressRequestTypeEnum.Employer,
            LineOneAddress     = address.AddressLine1,
            LineTwoAddress     = address.AddressLine2,
            CityName           = address.City,
            StateCode          = address.State,
            ZipCode            = address.Zip,
            ZipCodeExtension   = address.Extension,
            CountryCode        = address.Country
        };

        var response = await _client.ValidateAddressAsync(request);

        // ErrorMessageOne is populated when the address could not be validated;
        // ReturnCode is not reliable (observed as null for both valid and invalid responses).
        var isValid = string.IsNullOrEmpty(response.ErrorMessageOne);

        var errorMessage = isValid
            ? null
            : response.ErrorMessageOne ?? response.ErrorMessageTwo;

        AddressModel? correctedAddress = null;
        if (response.OutputAddress is not null)
        {
            // The service occasionally returns the street in LineTwoAddress when LineOneAddress is empty.
            var line1 = string.IsNullOrWhiteSpace(response.OutputAddress.LineOneAddress)
                ? response.OutputAddress.LineTwoAddress
                : response.OutputAddress.LineOneAddress;
            var line2 = string.IsNullOrWhiteSpace(response.OutputAddress.LineOneAddress)
                ? null
                : response.OutputAddress.LineTwoAddress;

            correctedAddress = new AddressModel
            {
                AddressLine1 = line1,
                AddressLine2 = line2,
                City         = response.OutputAddress.CityName,
                State        = response.OutputAddress.StateCode,
                Zip          = response.OutputAddress.ZipCode,
                Extension    = response.OutputAddress.ZipCodeExtension,
                // Service returns null CountryCode in OutputAddress; fall back to the input value.
                Country      = response.OutputAddress.CountryCode ?? address.Country
            };
        }

        return new AddressValidationResult(isValid, errorMessage, correctedAddress);
    }
}
