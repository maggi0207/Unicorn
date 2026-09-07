using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// Feature service for Employer Service Provider account operations: registration,
/// and maintaining the ESP's client relationships. Wraps the generated
/// <see cref="IESPService"/> WCF client.
/// </summary>
public interface IEspAccountService
{
    /// <summary>
    /// Submits a new Employer Service Provider registration.
    /// </summary>
    /// <param name="model">The collected registration details.</param>
    /// <returns>The service response, including any rule violations.</returns>
    Task<ESPResponse> RegisterEspAsync(EspRegistrationModel model);

    /// <summary>
    /// Retrieves the current ESP's registration details (as entered during registration)
    /// mapped into an <see cref="EspRegistrationModel"/> for viewing and editing.
    /// </summary>
    /// <returns>The populated model, or <c>null</c> when no ESP details are available.</returns>
    Task<EspRegistrationModel?> ObtainEspDetailsAsync();

    /// <summary>
    /// Updates the current ESP's registration details with the user's modifications.
    /// </summary>
    /// <param name="model">The modified ESP details.</param>
    /// <returns>The service response, including any rule violations.</returns>
    Task<ESPResponse> UpdateEspDetailsAsync(EspRegistrationModel model);

    /// <summary>
    /// Retrieves the list of client accounts associated with the current ESP.
    /// </summary>
    Task<List<EspClientModel>> GetClientsAsync();

    /// <summary>
    /// Removes the ESP's relationship with a single client account.
    /// </summary>
    /// <param name="commonClientSK">The client employer's common client surrogate key.</param>
    /// <returns>A tuple indicating success and, on failure, the rule-violation messages.</returns>
    Task<(bool success, string message)> RemoveClientAsync(int commonClientSK);

    /// <summary>
    /// Requests worker access to a client account using an access key and UI account number.
    /// </summary>
    /// <param name="accessKey">The client-supplied access key.</param>
    /// <param name="uiAccountNumber">The associated UI account number (format 000000-000-0).</param>
    /// <returns>
    /// <c>success</c> — true when the relationship was created; <c>legalName</c> — the client's
    /// legal name for the confirmation message; <c>ruleViolations</c> — any violations on failure.
    /// </returns>
    Task<(bool success, IReadOnlyList<string> ruleViolations)> AddClientAsync(string accessKey, string uiAccountNumber);
}

/// <inheritdoc />
internal class EspAccountService : IEspAccountService
{
    private readonly IESPService _espService;
    private readonly IEmployerAccountService _employerAccountService;
    private readonly IEmployerAccessProvider _employerAccessProvider;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<EspAccountService> _retryPolicy;

    /// <summary>Initializes a new instance of the <see cref="EspAccountService"/> class.</summary>
    public EspAccountService(
        IESPService espService,
        IEmployerAccountService employerAccountService,
        IEmployerAccessProvider employerAccessProvider,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<EspAccountService> retryPolicy)
    {
        _espService = espService;
        _employerAccountService = employerAccountService;
        _employerAccessProvider = employerAccessProvider;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc />
    public async Task<ESPResponse> RegisterEspAsync(EspRegistrationModel model)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        var phoneParsed = TryParsePhone(model.PhoneNumber, out var phoneAreaCode, out var phoneNumber);
        var faxParsed = TryParsePhone(model.FaxNumber, out var faxAreaCode, out var faxNumber);

        var address = model.MailingAddress;
        var isCanada = string.Equals(address.Country, "Canada", StringComparison.OrdinalIgnoreCase);

        var request = new RegisterNewESPRequest
        {
            BusinessName = model.LegalName,
            ContactName = $"{model.FirstName} {model.LastName}".Trim(),
            ElectronicAddress = model.Email,
            EmployerServiceProviderAccessCode = string.Empty,
            FEIN = DigitsOnly(model.Fein),
            FaxNumber = faxParsed ? faxNumber : string.Empty,
            FaxNumberAreaCode = faxParsed ? faxAreaCode : string.Empty,
            FaxNumberExtension = string.Empty,
            InternetAddress = model.Website ?? string.Empty,
            PhoneNumber = phoneParsed ? phoneNumber : string.Empty,
            PhoneNumberAreaCode = phoneParsed ? phoneAreaCode : string.Empty,
            PhoneNumberExtension = string.Empty,
            SecureUserSk = secureUserSk,
            SidesBrokerNumberText = model.SidesNumber ?? string.Empty,
            // Address.
            AddressLine1 = address.AddressLine1,
            AddressLine2 = !string.IsNullOrWhiteSpace(address.AddressLine2) ? address.AddressLine2 : " ",
            AddressLine3 = address.AddressLine3 ?? string.Empty,
            AddressLine4 = address.AddressLine4 ?? string.Empty,
            City = address.City,
            State = isCanada ? 0 : ContactInformationService.GetStateCodeSKByStateAbbreviation(address.State),
            Province = isCanada ? ContactInformationService.GetStateCodeSKByStateAbbreviation(address.Province) : 0,
            ZipCode = address.Zip ?? string.Empty,
            ZipExt = address.Extension ?? string.Empty,
            PostalCode = (address.PostalCode ?? string.Empty).Replace(" ", ""),
            County = string.Empty,
            CountryFormat = MapCountryToCode(address.Country)
        };

        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.RegisterNewESPAsync(request);
        });
    }

    /// <inheritdoc />
    public async Task<EspRegistrationModel?> ObtainEspDetailsAsync()
    {
        var espSk = _userAccountService.GetEspUserSK();
        if (espSk is null or 0)
        {
            return null;
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ObtainESPDetailsAsync(espSk.Value);
        });

        var proxy = response?.EmployerServiceProviderProxies?.FirstOrDefault();
        return proxy is null ? null : MapProxyToModel(proxy);
    }

    /// <inheritdoc />
    public async Task<ESPResponse> UpdateEspDetailsAsync(EspRegistrationModel model)
    {
        var espSk = _userAccountService.GetEspUserSK();

        var phoneParsed = TryParsePhone(model.PhoneNumber, out var phoneAreaCode, out var phoneNumber);
        var faxParsed = TryParsePhone(model.FaxNumber, out var faxAreaCode, out var faxNumber);

        var address = model.MailingAddress;
        var isCanada = string.Equals(address.Country, "Canada", StringComparison.OrdinalIgnoreCase);

        var request = new UpdateESPDetailsRequest
        {
            EmployerServiceProviderSK = espSk ?? 0,
            BusinessName = model.LegalName,
            ContactName = $"{model.FirstName} {model.LastName}".Trim(),
            Email = model.Email,
            FEIN = DigitsOnly(model.Fein),
            FaxNumber = faxParsed ? faxNumber : string.Empty,
            FaxAreaCode = faxParsed ? faxAreaCode : string.Empty,
            FaxExt = string.Empty,
            Website = model.Website ?? string.Empty,
            PhoneNumber = phoneParsed ? phoneNumber : string.Empty,
            PhoneAreaCode = phoneParsed ? phoneAreaCode : string.Empty,
            PhoneExt = string.Empty,
            SidesBrokeNumber = model.SidesNumber ?? string.Empty,
            // Address.
            AddressLine1 = address.AddressLine1,
            AddressLine2 = !string.IsNullOrWhiteSpace(address.AddressLine2) ? address.AddressLine2 : " ",
            AddressLine3 = address.AddressLine3 ?? string.Empty,
            AddressLine4 = address.AddressLine4 ?? string.Empty,
            City = address.City,
            State = isCanada ? 0 : ContactInformationService.GetStateCodeSKByStateAbbreviation(address.State),
            Province = isCanada ? ContactInformationService.GetStateCodeSKByStateAbbreviation(address.Province) : 0,
            ZipCode = address.Zip ?? string.Empty,
            ZipExt = address.Extension ?? string.Empty,
            PostalCode = (address.PostalCode ?? string.Empty).Replace(" ", ""),
            CountryFormat = MapCountryToCode(address.Country)
        };

        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.UpdateESPDetailsAsync(request);
        });
    }

    /// <summary>
    /// Maps a service <see cref="EmployerServiceProviderProxy"/> into the UI
    /// <see cref="EspRegistrationModel"/> used by the registration/profile flow.
    /// </summary>
    private static EspRegistrationModel MapProxyToModel(EmployerServiceProviderProxy proxy)
    {
        var (firstName, lastName) = SplitContactName(proxy.ContactName);
        var country = MapCodeToCountry(proxy.CountryFormat);
        var isCanada = string.Equals(country, "Canada", StringComparison.OrdinalIgnoreCase);

        return new EspRegistrationModel
        {
            LegalName = proxy.BusinessName,
            Fein = FormatFein(proxy.FEIN),
            PhoneNumber = FormatPhone(proxy.PhoneAreaCode, proxy.PhoneNumber),
            FaxNumber = FormatPhone(proxy.FaxAreaCode, proxy.FaxNumber),
            Website = proxy.Website,
            Email = proxy.Email,
            ConfirmEmail = proxy.Email,
            SidesNumber = proxy.SidesBrokeNumber,
            FirstName = firstName,
            LastName = lastName,
            MailingAddress = new AddressModel
            {
                Country = country,
                AddressLine1 = proxy.AddressLine1,
                AddressLine2 = string.IsNullOrWhiteSpace(proxy.AddressLine2) ? null : proxy.AddressLine2.Trim(),
                AddressLine3 = proxy.AddressLine3,
                AddressLine4 = proxy.AddressLine4,
                City = proxy.City,
                State = isCanada ? null : GetStateAbbreviationByStateCodeSK(proxy.State),
                Province = isCanada ? GetStateAbbreviationByStateCodeSK(proxy.Province) : null,
                Zip = proxy.ZipCode,
                Extension = proxy.ZipExt,
                PostalCode = proxy.PostalCode
            }
        };
    }

    /// <summary>Splits a full contact name into a first name and the remaining last name.</summary>
    private static (string firstName, string lastName) SplitContactName(string? contactName)
    {
        if (string.IsNullOrWhiteSpace(contactName))
        {
            return (string.Empty, string.Empty);
        }

        var parts = contactName.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
        return parts.Length == 2 ? (parts[0], parts[1]) : (parts[0], string.Empty);
    }

    /// <summary>Rebuilds a formatted phone number (999-999-9999) from a stored area code and number.</summary>
    private static string FormatPhone(string? areaCode, string? number)
    {
        var digits = DigitsOnly(areaCode) + DigitsOnly(number);
        return digits.Length == 10
            ? $"{digits[..3]}-{digits.Substring(3, 3)}-{digits.Substring(6, 4)}"
            : string.Empty;
    }

    /// <summary>Formats a 9-digit FEIN as 99-9999999; returns the original value when it is not 9 digits.</summary>
    private static string FormatFein(string? fein)
    {
        var digits = DigitsOnly(fein);
        return digits.Length == 9 ? $"{digits[..2]}-{digits.Substring(2, 7)}" : fein ?? string.Empty;
    }

    /// <inheritdoc />
    public async Task<List<EspClientModel>> GetClientsAsync()
    {
        var espSk = _userAccountService.GetEspUserSK();
        if (espSk is null or 0)
        {
            return [];
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.GetESPClientListAsync(espSk.Value);
        });

        if (response?.Clients == null || response.Clients.Length == 0)
        {
            return [];
        }

        var result = new List<EspClientModel>(response.Clients.Length);
        foreach (var proxy in response.Clients)
        {
            result.Add(new EspClientModel
            {
                AccountName = proxy.EmployerDetails.LegalName ?? string.Empty,
                UIAccountNumber = proxy.EmployerDetails.UIAccountNumber ?? string.Empty,
                CommonClientSK = proxy.EmployerDetails.CommonClientSK
            });
        }

        return result;
    }

    /// <inheritdoc />
    public async Task<(bool success, string message)> RemoveClientAsync(int commonClientSK)
    {
        var espSk = _userAccountService.GetEspUserSK();
        var userSK = _userAccountService.GetUserSKClaim();
        if (espSk is null or 0)
        {
            return (false, "Unable to determine the current Employer Service Provider.");
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.RemoveESPRelationshipAsync(new RemoveESPClientRequest
            {
                EmployerSK = commonClientSK,
                EmployerServiceProviderSK = espSk.Value,
                SecureUserSk = userSK
            });
        });

        if (response?.RuleViolations == null || response.RuleViolations.Length == 0)
        {
            return (true, string.Empty);
        }

        var errors = string.Join(" ", response.RuleViolations.Select(v =>
        {
            return v.RuleViolation;
        }));
        return (false, errors);
    }

    /// <inheritdoc />
    public async Task<(bool success, IReadOnlyList<string> ruleViolations)> AddClientAsync(
        string accessKey, string uiAccountNumber)
    {
        var espSk = _userAccountService.GetEspUserSK();
        if (espSk is null or 0)
        {
            return (false, ["Unable to determine the current Employer Service Provider."]);
        }

        var existingClients = await GetClientsAsync();
        var normalizedAccountNumber = NormalizeAccountNumber(uiAccountNumber);

        if (existingClients.Any(c =>
        {
            return NormalizeAccountNumber(c.UIAccountNumber) == normalizedAccountNumber;
        }))
        {
            return (false, ["This client has already been added."]);
        }

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.AddNewESPClientRelationshipAsync(new AddESPClientRequest
            {
                AccessKey = accessKey,
                EmployerAccountNumber = uiAccountNumber,
                EmployerServiceProviderSK = espSk.Value,
                SecureUserSk = _userAccountService.GetUserSKClaim()
            });
        });

        if (response?.RuleViolations is { Length: > 0 })
        {
            var violations = response.RuleViolations.Select(v =>
            {
                return v.RuleViolation;
            }).ToList();
            return (false, violations);
        }
        else
        {
            await _employerAccessProvider.ResetEmployerAccess();
            await _employerAccountService.GetEmployerAccounts(true);
        }

        return (true, []);
    }

    private static string NormalizeAccountNumber(string? accountNumber)
    {
        return string.Concat((accountNumber ?? string.Empty).Where(char.IsLetterOrDigit));
    }

    private static string DigitsOnly(string? value)
    {
        return value == null ? string.Empty : new string(value.Where(char.IsDigit).ToArray());
    }

    /// <summary>
    /// Splits a formatted phone number into a 3-digit area code and 7-digit number.
    /// Returns false when the input does not contain exactly 10 digits.
    /// </summary>
    private static bool TryParsePhone(string? input, out string? areaCode, out string? phoneNumber)
    {
        areaCode = null;
        phoneNumber = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        var digits = new string(input.Where(char.IsDigit).ToArray());
        if (digits.Length != 10)
        {
            return false;
        }

        areaCode = digits[..3];
        phoneNumber = digits.Substring(3, 7);
        return true;
    }

    /// <summary>Reverse lookup of <see cref="ContactInformationService.StateAbbreviationToStateCodeSK"/>: state code SK to abbreviation.</summary>
    private static readonly Dictionary<int, string> StateCodeSKToStateAbbreviation =
        ContactInformationService.StateAbbreviationToStateCodeSK.ToDictionary(
            kvp =>
            {
                return kvp.Value;
            },
            kvp =>
            {
                return kvp.Key;
            });

    private static string? GetStateAbbreviationByStateCodeSK(int stateCodeSK)
    {
        return StateCodeSKToStateAbbreviation.TryGetValue(stateCodeSK, out var abbreviation) ? abbreviation : null;
    }

    /// <summary>Maps a country name to its SUITES country address-format code (US=1, Canada=2, Other International=3).</summary>
    private static int MapCountryToCode(string? country)
    {
        return country switch
        {
            "United States" => 1,
            "Canada" => 2,
            "Other International" => 3,
            _ => 1
        };
    }

    /// <summary>Maps a SUITES country address-format code back to its country name (1=US, 2=Canada, 3=Other International).</summary>
    private static string MapCodeToCountry(int code)
    {
        return code switch
        {
            1 => "United States",
            2 => "Canada",
            3 => "Other International",
            _ => "United States"
        };
    }



}

