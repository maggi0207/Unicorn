using System.ServiceModel;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Wraps the generated WCF <see cref="GeneratedClient.IAddressValidationService"/> client
/// and maps request/response types to the application's <see cref="AddressModel"/>.
/// </summary>
public class AddressValidationService : IAddressValidationWrapper
{
    private readonly GeneratedClient.IAddressValidationService _client;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="client"></param>
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
            // UIEP-1921: The backend FinalistFacade expects the street address on LineTwoAddress
            // and suite/apt on LineOneAddress (it flips them internally). Match that convention.
            LineOneAddress = address.AddressLine2 ?? string.Empty,
            LineTwoAddress = address.AddressLine1,
            CityName = address.City,
            StateCode = address.State,
            ZipCode = address.Zip,
            ZipCodeExtension = address.Extension ?? string.Empty,
            // Service requires ISO country code ("US"), not the display name ("United States")
            CountryCode = ToCountryCode(address.Country)
        };

        GeneratedClient.ValidateAddressResponse response;
        try
        {
            response = await _client.ValidateAddressAsync(request);
        }
        catch (CommunicationException)
        {
            // WCF communication failure (network error, SOAP fault, serialization failure).
            // Treat as unverifiable — let the user proceed without a suggestion.
            return new AddressValidationResult(false, "Address validation is temporarily unavailable. Please try again.", null);
        }
        catch (Exception)
        {
            // Unexpected failure — fail safe so the page does not crash.
            return new AddressValidationResult(false, "Address validation is temporarily unavailable. Please try again.", null);
        }

        // ErrorMessageOne is populated when the address could not be validated;
        // ReturnCode is not reliable (observed as null for both valid and invalid responses).
        var isValid = string.IsNullOrEmpty(response.ErrorMessageOne);

        var errorMessage = isValid
            ? null
            : response.ErrorMessageOne ?? response.ErrorMessageTwo;

        AddressModel? correctedAddress = null;
        if (response.OutputAddress is not null)
        {
            // UIEP-1921: The backend returns street on LineTwoAddress and suite/apt on
            // LineOneAddress (same swapped convention we send). Map back to the UI model:
            // AddressLine1 = street address (from backend LineTwoAddress)
            // AddressLine2 = suite/apt      (from backend LineOneAddress)
            var line1 = response.OutputAddress.LineTwoAddress;
            var line2 = string.IsNullOrWhiteSpace(response.OutputAddress.LineOneAddress)
                ? null
                : response.OutputAddress.LineOneAddress;

            correctedAddress = new AddressModel
            {
                AddressLine1 = line1,
                AddressLine2 = line2,
                City = response.OutputAddress.CityName,
                State = response.OutputAddress.StateCode,
                Zip = response.OutputAddress.ZipCode,
                Extension = response.OutputAddress.ZipCodeExtension,
                Country = response.OutputAddress.CountryCode ?? address.Country,
                CountyName = response.OutputAddress.CountyName
            };
        }

        return new AddressValidationResult(isValid, errorMessage, correctedAddress);
    }

    /// <summary>
    /// Maps the AddressModel country display name to the ISO code expected by the WCF service.
    /// Defaults to "US" when the value is null or unrecognised.
    /// </summary>
    private static string ToCountryCode(string? country)
    {
        return country switch
        {
            "United States" => "US",
            "Canada" => "CA",
            "Mexico" => "MX",
            _ => "US",
        };
    }
}
