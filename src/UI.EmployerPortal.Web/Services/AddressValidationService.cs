using UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public class AddressValidationService : IAddressValidationService
{
    public async Task<AddressValidationResult> ValidateAsync(AddressModel address)
    {
        var client = new AddressValidationServiceClient();
        try
        {
            var request = new AddressProxy
            {
                AddressRequestType = AddressRequestTypeEnum.Employer,
                LineOneAddress     = address.AddressLine1,
                LineTwoAddress     = address.AddressLine2,
                CityName           = address.City,
                StateCode          = address.State,
                ZipCode            = address.Zip,
                ZipCodeExtension   = address.Extension,
                CountryCode        = address.Country
            };

            var response = await client.ValidateAddressAsync(request);

            var isValid = string.IsNullOrEmpty(response.ReturnCode)
                       || response.ReturnCode == "0";

            var errorMessage = isValid
                ? null
                : response.ErrorMessageOne ?? response.ErrorMessageTwo;

            var correctedAddress = response.OutputAddress is null ? null : new AddressModel
            {
                AddressLine1 = response.OutputAddress.LineOneAddress,
                AddressLine2 = response.OutputAddress.LineTwoAddress,
                City         = response.OutputAddress.CityName,
                State        = response.OutputAddress.StateCode,
                Zip          = response.OutputAddress.ZipCode,
                Extension    = response.OutputAddress.ZipCodeExtension,
                Country      = response.OutputAddress.CountryCode
            };

            return new AddressValidationResult(isValid, errorMessage, correctedAddress);
        }
        finally
        {
            await client.CloseAsync();
        }
    }
}
