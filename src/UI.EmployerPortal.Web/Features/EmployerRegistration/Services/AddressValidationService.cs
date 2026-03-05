using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public class AddressValidationService : IAddressValidationWrapper
{
    private readonly GeneratedClient.IAddressValidationService _client;

    public AddressValidationService(GeneratedClient.IAddressValidationService client)
    {
        _client = client;
    }

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

        // ErrorMessageOne is set when the address could not be validated
        var isValid = string.IsNullOrEmpty(response.ErrorMessageOne);

        var errorMessage = isValid
            ? null
            : response.ErrorMessageOne ?? response.ErrorMessageTwo;

        AddressModel? correctedAddress = null;
        if (response.OutputAddress is not null)
        {
            // The service sometimes returns the street in LineTwoAddress when LineOneAddress is empty
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
                Country      = response.OutputAddress.CountryCode ?? address.Country
            };
        }

        return new AddressValidationResult(isValid, errorMessage, correctedAddress);
    }
}
