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

        return response.StreetAddresses
            .Where(a => a.IsActive)
            .Select(MapToRowModel)
            .ToList();
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
                    LineOneAddress = model.LineOneAddress,
                    LineTwoAddress = model.LineTwoAddress ?? string.Empty,
                    CityName = model.CityName ?? string.Empty,
                    CountyName = model.CountyName ?? string.Empty,
                    ProvinceCodeSK = model.ProvinceCodeSK ?? 0,
                    CanadianPostalCode = (model.CanadianPostalCode ?? string.Empty).Replace(" ", "")
                };
                var canadaResponse = await _retryPolicy.ExecuteAsync(() =>
                    _accountMaintenanceService.SaveEmployerAddressCanadaAsync(canadaRequest));
                saved = canadaResponse?.Value ?? false;
                break;

            case CountryOtherIntl:
                var intlRequest = new PortalAddressRequestOtherInternational
                {
                    EmployerSK = employerSK,
                    SecureUserSK = secureUserSK,
                    AddressTypeCodeSK = model.AddressTypeCodeSK,
                    LineOneAddress = model.LineOneAddress,
                    LineTwoAddress = model.LineTwoAddress ?? string.Empty,
                    LineThreeAddress = model.LineThreeAddress ?? string.Empty,
                    LineFourAddress = model.LineFourAddress ?? string.Empty
                };
                var intlResponse = await _retryPolicy.ExecuteAsync(() =>
                    _accountMaintenanceService.SaveEmployerAddressOtherInternationalAsync(intlRequest));
                saved = intlResponse?.Value ?? false;
                break;

            default: // United States
                var usRequest = new PortalAddressRequestUnitedStates
                {
                    EmployerSK = employerSK,
                    SecureUserSK = secureUserSK,
                    AddressTypeCodeSK = model.AddressTypeCodeSK,
                    LineOneAddress = model.LineOneAddress,
                    LineTwoAddress = model.LineTwoAddress ?? string.Empty,
                    CityName = model.CityName ?? string.Empty,
                    CountyName = model.CountyName ?? string.Empty,
                    StateCodeSK = model.StateCodeSK ?? 0,
                    ZipCode = model.ZipCode ?? string.Empty,
                    ZipExtension = model.ZipExtension ?? string.Empty
                };
                var usResponse = await _retryPolicy.ExecuteAsync(() =>
                    _accountMaintenanceService.SaveEmployerAddressUnitedStatesAsync(usRequest));
                saved = usResponse?.Value ?? false;
                break;
        }

        return saved
            ? (true, string.Empty)
            : (false, "Unable to save the address. Please try again.");
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

        return (response?.Value ?? false)
            ? (true, string.Empty)
            : (false, "Unable to delete the address. Please try again.");
    }

    // ── Private helpers ─────────────────────────────────────────────────────

    private static AddressRowModel MapToRowModel(StreetAddressProxy proxy)
    {
        var typeCodeSK = proxy.AddressCodeSK ?? 0;

        return new AddressRowModel
        {
            AddressSK = proxy.AddressSK ?? 0,
            AddressTypeCodeSK = typeCodeSK,
            AddressType = proxy.AddressCode ?? proxy.ShortDescription ?? string.Empty,
            FormattedAddress = BuildFormattedAddress(proxy),
            CanDelete = typeCodeSK != MainBusinessMailingAddressCodeSK,
            CountryAddressFormatCodeSK = proxy.CountryAddressFormatCodeSK ?? CountryUS,
            LineOneAddress = proxy.LineOneAddress ?? string.Empty,
            LineTwoAddress = proxy.LineTwoAddress,
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
            // e.g. "124 O Street, Ottawa, ON K1A 0A9"
            var parts = new List<string>();
            if (!string.IsNullOrWhiteSpace(proxy.LineOneAddress)) parts.Add(proxy.LineOneAddress);
            if (!string.IsNullOrWhiteSpace(proxy.LineTwoAddress)) parts.Add(proxy.LineTwoAddress);
            if (!string.IsNullOrWhiteSpace(proxy.CityName)) parts.Add(proxy.CityName);
            var provincePart = string.Join(" ",
                new[] { proxy.StateCode, proxy.CanadianPostalCode }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));
            if (!string.IsNullOrWhiteSpace(provincePart)) parts.Add(provincePart);
            return string.Join(", ", parts);
        }

        if (countryCode == CountryOtherIntl)
        {
            var parts = new[]
            {
                proxy.LineOneAddress, proxy.LineTwoAddress,
                proxy.LineThreeInternationalAddress, proxy.LineFourAddress
            }.Where(s => !string.IsNullOrWhiteSpace(s));
            return string.Join(", ", parts);
        }

        // United States: "5321 Westminster Pl, Milwaukee, WI 53201-4302"
        {
            var streetParts = new List<string>();
            if (!string.IsNullOrWhiteSpace(proxy.LineOneAddress)) streetParts.Add(proxy.LineOneAddress);
            if (!string.IsNullOrWhiteSpace(proxy.LineTwoAddress)) streetParts.Add(proxy.LineTwoAddress);

            var cityStatePart = string.Join(" ",
                new[] { proxy.CityName + ",", proxy.StateCode }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            var zip = proxy.ZipCode;
            if (!string.IsNullOrWhiteSpace(proxy.ZipExtensionCode))
                zip += $"-{proxy.ZipExtensionCode}";

            var allParts = streetParts
                .Concat(new[] { cityStatePart, zip }
                    .Where(s => !string.IsNullOrWhiteSpace(s)));

            return string.Join(", ", allParts);
        }
    }
}
