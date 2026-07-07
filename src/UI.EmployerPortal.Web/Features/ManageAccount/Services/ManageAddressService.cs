using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Implements address management operations for the Manage Addresses page
/// using the AccountMaintenanceService WCF proxy.
/// </summary>
internal class ManageAddressService : IManageAddressService
{
    private readonly IAsyncRetryPolicy<ManageAddressService> _retryPolicy;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly IUserAccountService _userAccountService;

    // CountryAddressFormatCodeSK constants (from ContactInformationService.MapCountryToCode):
    //   1 = United States, 2 = Canada, 3 = Other International
    private const int CountryUS = 1;
    private const int CountryCanada = 2;
    private const int CountryOtherIntl = 3;

    // The Main Business Mailing Address has AddressCodeSK = 1 (primary address, not deletable per Figma)
    private const int MainBusinessMailingAddressCodeSK = 1;

    /// <summary>
    /// Initializes a new instance of the <see cref="ManageAddressService"/> class.
    /// </summary>
    public ManageAddressService(
        IAsyncRetryPolicy<ManageAddressService> retryPolicy,
        IAccountMaintenanceService accountMaintenanceService,
        IUserAccountService userAccountService)
    {
        _retryPolicy = retryPolicy;
        _accountMaintenanceService = accountMaintenanceService;
        _userAccountService = userAccountService;
    }

    /// <inheritdoc/>
    public async Task<List<AddressRowModel>> GetAddressesAsync(int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.GetEmployerAddressesAsync(employerSK, secureUserSK);
        });

        if (response?.StreetAddresses == null)
        {
            return [];
        }

        var result = new List<AddressRowModel>();
        foreach (var address in response.StreetAddresses)
        {
            if (address.IsActive)
            {
                result.Add(MapToRowModel(address));
            }
        }
        return result;
    }

    /// <inheritdoc/>
    public async Task<(bool success, string error)> SaveAddressAsync(AddressFormModel model, int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        bool saved;

        switch (model.CountryAddressFormatCodeSK)
        {
            case CountryCanada:
                var canadaRequest = new PortalAddressRequestCanada
                {
                    EmployerSK = employerSK,
                    SecureUserSK = secureUserSK,
                    AddressTypeCodeSK = model.AddressTypeCodeSK,
                    // Backend expects UI Line 1 in LineTwoAddress and UI Line 2 in LineOneAddress — swap on save
                    LineOneAddress = model.LineTwoAddress ?? string.Empty,
                    LineTwoAddress = model.LineOneAddress,
                    CityName = model.CityName ?? string.Empty,
                    CountyName = model.CountyName ?? string.Empty,
                    ProvinceCodeSK = model.ProvinceCodeSK ?? 0,
                    CanadianPostalCode = (model.CanadianPostalCode ?? string.Empty).Replace(" ", "")
                };
                var canadaResponse = await _retryPolicy.ExecuteAsync(() =>
                {
                    return _accountMaintenanceService.SaveEmployerAddressCanadaAsync(canadaRequest);
                });
                saved = canadaResponse?.Value ?? false;
                if (!saved && canadaResponse?.RuleViolations?.Length > 0)
                {
                    return (false, canadaResponse.RuleViolations[0].RuleViolation);
                }
                break;

            case CountryOtherIntl:
                var intlRequest = new PortalAddressRequestOtherInternational
                {
                    EmployerSK = employerSK,
                    SecureUserSK = secureUserSK,
                    AddressTypeCodeSK = model.AddressTypeCodeSK,
                    // Backend expects UI Line 1 in LineTwoAddress and UI Line 2 in LineOneAddress — swap on save
                    LineOneAddress = model.LineTwoAddress ?? string.Empty,
                    LineTwoAddress = model.LineOneAddress,
                    LineThreeAddress = model.LineThreeAddress ?? string.Empty,
                    LineFourAddress = model.LineFourAddress ?? string.Empty
                };
                var intlResponse = await _retryPolicy.ExecuteAsync(() =>
                {
                    return _accountMaintenanceService.SaveEmployerAddressOtherInternationalAsync(intlRequest);
                });
                saved = intlResponse?.Value ?? false;
                if (!saved && intlResponse?.RuleViolations?.Length > 0)
                {
                    return (false, intlResponse.RuleViolations[0].RuleViolation);
                }
                break;

            default:
                var usRequest = new PortalAddressRequestUnitedStates
                {
                    EmployerSK = employerSK,
                    SecureUserSK = secureUserSK,
                    AddressTypeCodeSK = model.AddressTypeCodeSK,
                    // Backend expects UI Line 1 in LineTwoAddress and UI Line 2 in LineOneAddress — swap on save
                    LineOneAddress = model.LineTwoAddress ?? string.Empty,
                    LineTwoAddress = model.LineOneAddress,
                    CityName = model.CityName ?? string.Empty,
                    CountyName = model.CountyName ?? string.Empty,
                    StateCodeSK = model.StateCodeSK ?? 0,
                    ZipCode = model.ZipCode ?? string.Empty,
                    ZipExtension = model.ZipExtension ?? string.Empty
                };
                var usResponse = await _retryPolicy.ExecuteAsync(() =>
                {
                    return _accountMaintenanceService.SaveEmployerAddressUnitedStatesAsync(usRequest);
                });
                saved = usResponse?.Value ?? false;
                if (!saved && usResponse?.RuleViolations?.Length > 0)
                {
                    return (false, usResponse.RuleViolations[0].RuleViolation);
                }
                break;
        }

        if (saved)
        {
            return (true, string.Empty);
        }
        return (false, "Unable to save the address. Please try again.");
    }

    /// <inheritdoc/>
    public async Task<(bool success, string error)> DeleteAddressAsync(long addressSK, int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.RemoveEmployerAddressAsync(
                (int)addressSK,
                employerSK,
                secureUserSK);
        });

        if (response?.Value ?? false)
        {
            return (true, string.Empty);
        }
        return (false, "Unable to delete the address. Please try again.");
    }

    private static AddressRowModel MapToRowModel(StreetAddressProxy proxy)
    {
        var typeCodeSK = proxy.AddressCodeSK ?? 0;
        var addressType = proxy.AddressCode ?? proxy.ShortDescription ?? string.Empty;

        var isMainBusinessMailing = typeCodeSK == MainBusinessMailingAddressCodeSK ||
                                    addressType.Contains("Main Business Mailing", StringComparison.OrdinalIgnoreCase) ||
                                    (proxy.ShortDescription ?? string.Empty).Contains("Main Business Mailing", StringComparison.OrdinalIgnoreCase);

        return new AddressRowModel
        {
            AddressSK = proxy.AddressSK ?? 0,
            AddressTypeCodeSK = typeCodeSK,
            AddressType = addressType,
            FormattedAddress = BuildFormattedAddress(proxy),
            CanDelete = !isMainBusinessMailing,
            CountryAddressFormatCodeSK = proxy.CountryAddressFormatCodeSK ?? CountryUS,
            // Backend stores UI Line 1 in LineTwoAddress and UI Line 2 in LineOneAddress — swap on load
            LineOneAddress = proxy.LineTwoAddress ?? string.Empty,
            LineTwoAddress = proxy.LineOneAddress,
            CityName = proxy.CityName,
            StateCode = proxy.StateCode,
            StateCodeSK = proxy.StateCodeSK,
            ZipCode = proxy.ZipCode,
            ZipExtensionCode = proxy.ZipExtensionCode,
            CanadianPostalCode = proxy.CanadianPostalCode,
            LineThreeAddress = proxy.LineThreeInternationalAddress,
            LineFourAddress = proxy.LineFourAddress,
            CountyName = proxy.CountyName
        };
    }

    /// <summary>
    /// Builds a single-line formatted address string for table display.
    /// </summary>
    private static string BuildFormattedAddress(StreetAddressProxy proxy)
    {
        var countryCode = proxy.CountryAddressFormatCodeSK ?? CountryUS;

        if (countryCode == CountryCanada)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(proxy.LineOneAddress))
            {
                parts.Add(proxy.LineOneAddress);
            }
            if (!string.IsNullOrWhiteSpace(proxy.LineTwoAddress))
            {
                parts.Add(proxy.LineTwoAddress);
            }
            if (!string.IsNullOrWhiteSpace(proxy.CityName))
            {
                parts.Add(proxy.CityName);
            }

            var provincePart = new List<string>();
            if (!string.IsNullOrWhiteSpace(proxy.StateCode))
            {
                provincePart.Add(proxy.StateCode);
            }
            if (!string.IsNullOrWhiteSpace(proxy.CanadianPostalCode))
            {
                provincePart.Add(proxy.CanadianPostalCode);
            }

            var provinceJoined = string.Join(" ", provincePart);
            if (!string.IsNullOrWhiteSpace(provinceJoined))
            {
                parts.Add(provinceJoined);
            }

            return string.Join(", ", parts);
        }

        if (countryCode == CountryOtherIntl)
        {
            var parts = new List<string>();

            if (!string.IsNullOrWhiteSpace(proxy.LineOneAddress))
            {
                parts.Add(proxy.LineOneAddress);
            }
            if (!string.IsNullOrWhiteSpace(proxy.LineTwoAddress))
            {
                parts.Add(proxy.LineTwoAddress);
            }
            if (!string.IsNullOrWhiteSpace(proxy.LineThreeInternationalAddress))
            {
                parts.Add(proxy.LineThreeInternationalAddress);
            }
            if (!string.IsNullOrWhiteSpace(proxy.LineFourAddress))
            {
                parts.Add(proxy.LineFourAddress);
            }

            return string.Join(", ", parts);
        }

        // United States
        var streetParts = new List<string>();

        if (!string.IsNullOrWhiteSpace(proxy.LineOneAddress))
        {
            streetParts.Add(proxy.LineOneAddress);
        }
        if (!string.IsNullOrWhiteSpace(proxy.LineTwoAddress))
        {
            streetParts.Add(proxy.LineTwoAddress);
        }

        var cityState = new List<string>();
        if (!string.IsNullOrWhiteSpace(proxy.CityName))
        {
            cityState.Add(proxy.CityName + ",");
        }
        if (!string.IsNullOrWhiteSpace(proxy.StateCode))
        {
            cityState.Add(proxy.StateCode);
        }

        var cityStatePart = string.Join(" ", cityState);

        var zip = proxy.ZipCode ?? string.Empty;
        if (!string.IsNullOrWhiteSpace(proxy.ZipExtensionCode))
        {
            zip += $"-{proxy.ZipExtensionCode}";
        }

        var allParts = new List<string>(streetParts);
        if (!string.IsNullOrWhiteSpace(cityStatePart))
        {
            allParts.Add(cityStatePart);
        }
        if (!string.IsNullOrWhiteSpace(zip))
        {
            allParts.Add(zip);
        }

        return string.Join(", ", allParts);
    }
}
