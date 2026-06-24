using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

///<summary>Services responsible for retrieving and saving file contact information</summary>
public interface IContactInformationService
{
    ///<summary>Retrieves file contact informationfor the secure user </summary>
    Task<ContactModel?> ObtainFileContact(int secureUserSK);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="secureUserSK"></param>
    /// <param name="employerSK"></param>
    /// <param name="contactTypeCodeSK"></param>
    /// <returns></returns>
    Task<ACHContactModel?> GetEmployerWebContact(int secureUserSK, int employerSK, int contactTypeCodeSK);
    ///<summary>Saves Web contact information</summary>
    Task<string?> SaveWebContact(ACHContactModel model, int secureUserSK, int employersk);

    ///<summary>Saves file contact information</summary>
    Task<CreateFileContactResponse> SaveFileContact(ContactModel model);
}
/// <summary> Service responsible for retrieving and saving contact information for Quarterly Tax File upload process </summary>
internal class ContactInformationService : IContactInformationService
{
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IAsyncRetryPolicy<ContactInformationService> _retryPolicy;
    private readonly IUserAccountService _userAccountService;

    /// <summary> Initializes a new instance of the service class </summary>
    public ContactInformationService(
        IPortalUtilityService portalUtilityService,
        IAsyncRetryPolicy<ContactInformationService> retryPolicy,
        IUserAccountService userAccountService)
    {
        _portalUtilityService = portalUtilityService;
        _retryPolicy = retryPolicy;
        _userAccountService = userAccountService;
    }

    /// <inheritdoc/>
    public async Task<ContactModel?> ObtainFileContact(int secureUserSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainFileContactAsync(secureUserSK);
        });

        return response?.FileContactProxy == null ? (ContactModel?) null : MapProxyToModel(response.FileContactProxy);
    }
    public async Task<ACHContactModel?> GetEmployerWebContact(int secureUserSK, int employerSK, int contactTypeCodeSK)
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.GetEmployerWebContactAsync(secureUserSK, employerSK, contactTypeCodeSK);
        });

        return response?.WebContactInformation == null ? (ACHContactModel?) null : MapcontacttoModel(response.WebContactInformation);
        ;
    }

    private static int? MapCountryToCode(string? country)
    {
        return country switch
        {
            "United States" => 1,
            "Canada" => 2,
            "Other International" => 3,
            _ => 1
        };
    }
    private static string MapCodeToCountry(int? code)
    {
        return code switch
        {
            1 => "United States",
            2 => "Canada",
            3 => "Other International",
            _ => string.Empty
        };
    }

    ///<inheritdoc/>
    public async Task<CreateFileContactResponse> SaveFileContact(ContactModel model)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();
        var uploadPhoneParsed = TryParsePhone(model.UploadPhoneNumber, out var uploadPhoneAreaCode, out var uploadPhoneNumber);
        var recordPhoneParsed = TryParsePhone(model.RecordPhone, out var recordPhoneAreaCode, out var recordPhoneNumber);
        var faxParsed = TryParsePhone(model.FaxNumber, out var faxAreaCode, out var faxNumber);

        var request = new FileContactRequest
        {
            SecureUserSK = secureUserSK,
            ContactTypeCodeSK = 1,
            EffectiveDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddYears(10),

            BusinessDetails = new BusinessDetailsProxy
            {
                BusinessName = model.BusinessName,
                Line1Address = model.MailingAddress.AddressLine2,
                Line2Address = model.MailingAddress.AddressLine1,
                CityName = model.MailingAddress.City,
                State = (model.MailingAddress.Country?.ToLower() ?? "") == "canada"
                    ? model.MailingAddress.Province
                    : model.MailingAddress.State,
                StateCodeSK = GetStateCodeSKByStateAbbreviation(
                    (model.MailingAddress.Country?.ToLower() ?? "") == "canada"
                    ? model.MailingAddress.Province
                    : model.MailingAddress.State),
                ZipCode = model.MailingAddress.Zip,
                ZipCodeExtension = model.MailingAddress.Extension,
                CanadianPostalCode = (model.MailingAddress.PostalCode ?? "").Replace(" ", ""),
                FaxNumber = faxParsed ? faxNumber : string.Empty,
                FaxAAreaCode = faxParsed ? faxAreaCode : string.Empty,
                CountryAddressFormatCodeSK = MapCountryToCode(model.MailingAddress.Country) ?? 1
            },
            PrimaryContact = new ContactDetailProxy
            {
                ContactName = model.ContactName,
                PhoneNumber = uploadPhoneParsed ? uploadPhoneNumber : string.Empty,
                PhoneAreaCode = uploadPhoneParsed ? uploadPhoneAreaCode : string.Empty,
                PhoneExtension = model.UploadExt,
                EmailAddress = model.UploadEmail,
            },
            SecondaryContact = new ContactDetailProxy
            {
                ContactName = model.RecordContactName,
                PhoneNumber = recordPhoneParsed ? recordPhoneNumber : string.Empty,
                PhoneAreaCode = recordPhoneParsed ? recordPhoneAreaCode : string.Empty,
                PhoneExtension = model.RecordExt,
                EmailAddress = model.RecordEmail
            }
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.SaveFileContactAsync(request);
        });

        return response;
    }

    private bool TryParsePhone(string input, out string? areaCode, out string? phoneNumber)
    {
        areaCode = null;
        phoneNumber = null;

        if (string.IsNullOrWhiteSpace(input))
        {
            return false;
        }

        // Keep only digits
        var digits = new string(input.Where(char.IsDigit).ToArray());

        // Validate length
        if (digits.Length != 10)
        {
            return false;
        }

        // Extract parts
        areaCode = digits[..3];
        phoneNumber = digits.Substring(3, 7);

        return true;
    }

    private static string? FormatPhone(string? areaCode, string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(areaCode) || string.IsNullOrWhiteSpace(phoneNumber))
        {
            return null;
        }

        // Keep only digits (in case of bad input)
        var areaDigits = new string(areaCode.Where(char.IsDigit).ToArray());
        var phoneDigits = new string(phoneNumber.Where(char.IsDigit).ToArray());

        // Validate lengths
        if (areaDigits.Length != 3 || phoneDigits.Length != 7)
        {
            return null;
        }

        // Format: XXX-XXX-XXXX
        return $"{areaDigits}-{phoneDigits[..3]}-{phoneDigits.Substring(3, 4)}";
    }


    private static ContactModel MapProxyToModel(FileContactProxy proxy)
    {
        return new ContactModel
        {
            BusinessName = proxy.BusinessName,
            FaxNumber = FormatPhone(proxy.FaxAreaCode, proxy.FaxNumber) ?? string.Empty,

            MailingAddress = new AddressModel
            {
                AddressLine1 = proxy.Line2Address,
                AddressLine2 = proxy.Line1Address,
                Country = MapCodeToCountry(proxy.CountryAddressFormatSK),
                City = proxy.CityName,
                State = proxy.StateAbbreviation,
                Province = proxy.StateAbbreviation,
                Zip = proxy.ZipCode,
                PostalCode = (proxy.CanadianPostalCode ?? "").Length > 3 ? proxy.CanadianPostalCode!.Insert(3, " ") : proxy.CanadianPostalCode,
                Extension = proxy.ZipExtensionCode
            },

            ContactName = proxy.PrimaryContactName ?? string.Empty,
            UploadPhoneNumber = FormatPhone(proxy.PrimaryContactPhoneAreaCode, proxy.PrimaryContactPhoneNumber) ?? string.Empty,
            UploadExt = proxy.PrimaryContactPhoneExtension,
            UploadEmail = proxy.EmailAddress,
            ConfirmUploadEmail = proxy.EmailAddress,

            RecordContactName = proxy.SecondaryContactName ?? string.Empty,
            RecordPhone = FormatPhone(proxy.SecondaryContactPhoneAreaCode, proxy.SecondaryContactPhoneNumber) ?? string.Empty,
            RecordExt = proxy.SecondaryContactPhoneExtension,
            RecordEmail = proxy.SecondaryEmailAddress,
            ConfirmationRecordEmail = proxy.SecondaryEmailAddress
        };
    }

    private static readonly Dictionary<string, int> StateAbbreviationToStateCodeSK = new()
    {
        ["AL"] = 1,
        ["AK"] = 2,
        ["AS"] = 3,
        ["AZ"] = 4,
        ["AR"] = 5,
        ["CA"] = 6,
        ["CO"] = 7,
        ["CT"] = 8,
        ["DE"] = 9,
        ["DC"] = 10,
        ["FM"] = 11,
        ["FL"] = 12,
        ["GA"] = 13,
        ["GU"] = 14,
        ["HI"] = 15,
        ["ID"] = 16,
        ["IL"] = 17,
        ["IN"] = 18,
        ["IA"] = 19,
        ["KS"] = 20,
        ["KY"] = 21,
        ["LA"] = 22,
        ["ME"] = 23,
        ["MH"] = 24,
        ["MD"] = 25,
        ["MA"] = 26,
        ["MI"] = 27,
        ["MN"] = 28,
        ["MS"] = 29,
        ["MO"] = 30,
        ["MT"] = 31,
        ["NE"] = 32,
        ["NV"] = 33,
        ["NH"] = 34,
        ["NJ"] = 35,
        ["NM"] = 36,
        ["NY"] = 37,
        ["NC"] = 38,
        ["ND"] = 39,
        ["MP"] = 40,
        ["OH"] = 41,
        ["OK"] = 42,
        ["OR"] = 43,
        ["PW"] = 44,
        ["PA"] = 45,
        ["PR"] = 46,
        ["RI"] = 47,
        ["SC"] = 48,
        ["SD"] = 49,
        ["TN"] = 50,
        ["TX"] = 51,
        ["UT"] = 52,
        ["VT"] = 53,
        ["VI"] = 54,
        ["VA"] = 55,
        ["WA"] = 56,
        ["WV"] = 57,
        ["WI"] = 58,
        ["WY"] = 59,
        ["AB"] = 60,
        ["BC"] = 61,
        ["MB"] = 62,
        ["NB"] = 63,
        ["NL"] = 64,
        ["NT"] = 65,
        ["NS"] = 66,
        ["NU"] = 67,
        ["ON"] = 68,
        ["PE"] = 69,
        ["QC"] = 70,
        ["SK"] = 71,
        ["YT"] = 72,
        ["AA"] = 73,
        ["AE"] = 74,
        ["AP"] = 75,
    };

    private static int GetStateCodeSKByStateAbbreviation(string? stateAbbreviation)
    {
        return stateAbbreviation != null
            ? StateAbbreviationToStateCodeSK.TryGetValue(stateAbbreviation, out var code) ? code : 0
            : 0;
    }
    private static ACHContactModel MapcontacttoModel(WebContactInformationProxy proxy)
    {
        return new ACHContactModel
        {
            ContactName = proxy.ContactName,
            Email = proxy.EmailAddress,
            PhoneNumberFormat = proxy.PhoneNumberFormatType,
            PhoneNumber = proxy.PhoneNumber,
            ConfirmEmail = proxy.EmailAddress,
            PhoneExt = proxy.PhoneNumberExtension,
            WebContactInformationsk = (int) (proxy.WebContactInformationSK ?? 0)
        };
    }
    public async Task<string?> SaveWebContact(ACHContactModel model, int secureUserSK, int employersk)
    {
        var wciProxy = new WebContactRequest
        {
            SecureUserSK = secureUserSK,
            EmployerSK = employersk,
            ContactTypeCodeSK = 4,
            ContactName = model.ContactName,
            PhoneNumber = model.PhoneNumber,
            PhoneNumberExtension = model.PhoneExt,
            EmailAddress = model.Email,
            InternationalPhoneNumberFlag = model.InternationalFlag,
            WebContactSK = model.WebContactInformationsk

        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.SaveWebContactAsync(wciProxy);
        });

        return response?.WebContact.ToString();
    }

}


