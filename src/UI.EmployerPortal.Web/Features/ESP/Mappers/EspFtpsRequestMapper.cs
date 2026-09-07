using System.Text.RegularExpressions;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.ESP.Mappers;

/// <summary>
/// Methods for mapping the ESP FTPS request models to the corresponding WCF contracts
/// </summary>
public static class EspFtpsRequestMapper
{
    /// <summary>
    /// Map the <see cref="EspFtpsRegistrationModel"/> to the <see cref="FileContactRequest"/> contract
    /// </summary>
    /// <param name="model">The model to map</param>
    /// <returns>A <see cref="FileContactRequest"/> instance</returns>
    public static FileContactRequest ToFileContactRequest(this EspFtpsRegistrationModel model)
    {
        // Parse and validate phone/fax numbers (returns empty strings in invalid)
        var (fileUploadPhoneAreaCode, fileUploadPhoneNumber) = ParsePhoneNumber(model.UploadContactPhoneNumber);
        var (recordsQuestionsPhoneAreaCode, recordsQuestionsPhoneNumber) = ParsePhoneNumber(model.QuestionsContactPhoneNumber);
        var (faxAreaCode, faxNumber) = ParsePhoneNumber(model.FaxNumber);
        var countryAddressFormatCode = CountryAddressFormatCode.TryGetValue(model.Country ?? string.Empty, out var c) ? c : 1;

        var request = new FileContactRequest()
        {
            BusinessDetails = new BusinessDetailsProxy()
            {
                BusinessName = model.BusinessName,
                CityName = model.City,
                Country = model.Country,
                CountryAddressFormatCodeSK = countryAddressFormatCode,
                FaxAAreaCode = faxAreaCode,
                FaxNumber = faxNumber,
                Line2Address = model.AddressLine1,
                Line1Address = model.AddressLine2,
                Line3Address = string.Empty,
                Line4Address = string.Empty,
                State = model.State,
                StateCodeSK = ContactInformationService.GetStateCodeSKByStateAbbreviation(model.State),
            },
            PrimaryContact = new ContactDetailProxy()
            {
                ContactName = model.UploadContactName,
                EmailAddress = model.UploadContactEmail,
                PhoneAreaCode = fileUploadPhoneAreaCode,
                PhoneNumber = fileUploadPhoneNumber,
                PhoneExtension = model.UploadContactExtension,
            },
            SameAsPrimaryContactFlag = model.IsSameAsFileUploadContact,
            SecondaryContact = model.IsSameAsFileUploadContact
                ? new ContactDetailProxy()
                {
                    ContactName = model.UploadContactName,
                    EmailAddress = model.UploadContactEmail,
                    PhoneAreaCode = fileUploadPhoneAreaCode,
                    PhoneNumber = fileUploadPhoneNumber,
                    PhoneExtension = model.UploadContactExtension,
                }
                : new ContactDetailProxy()
                {
                    ContactName = model.QuestionsContactName,
                    EmailAddress = model.QuestionsContactEmail,
                    PhoneAreaCode = recordsQuestionsPhoneAreaCode,
                    PhoneNumber = recordsQuestionsPhoneNumber,
                    PhoneExtension = model.QuestionsContactExtension,
                },
        };

        // Set zip/postal code according to country
        switch (countryAddressFormatCode)
        {
            case 1: // U.S.
                request.BusinessDetails.ZipCode = model.ZipCode;
                request.BusinessDetails.ZipCodeExtension = model.ZipExt;
                break;
            case 2: // Canada
                request.BusinessDetails.CanadianPostalCode = UpperAlphanumericOnly(model.CanadianPostalCode);
                break;
            default:
                // leave both empty for international
                break;
        }
        return request;
    }

    /// <summary>
    /// Map the <see cref="FileContactProxy"/> to <see cref="EspFtpsRegistrationModel"/>
    /// </summary>
    /// <param name="contact"></param>
    /// <returns>An instance of <see cref="EspFtpsRegistrationModel"/></returns>
    /// <exception cref="ArgumentNullException">The contact was null</exception>
    public static EspFtpsRegistrationModel ToRegistrationModel(this FileContactProxy contact)
    {
        ArgumentNullException.ThrowIfNull(contact);
        var model = new EspFtpsRegistrationModel()
        {
            BusinessName = contact.BusinessName,
            Country = CountrySelectOptions.TryGetValue(contact.CountryAddressFormatSK ?? 0, out var country) ? country : contact.CountryAddressFormat,
            AddressLine1 = contact.Line2Address,
            AddressLine2 = contact.Line1Address,
            City = contact.CityName,
            State = contact.StateAbbreviation,
            ZipCode = contact.ZipCode,
            ZipExt = contact.ZipExtensionCode,
            FaxNumber = FormatPhoneNumber(contact.FaxAreaCode, contact.FaxNumber),
            CanadianPostalCode = contact.CanadianPostalCode,
            UploadContactName = contact.PrimaryContactName,
            UploadContactPhoneNumber = FormatPhoneNumber(contact.PrimaryContactPhoneAreaCode, contact.PrimaryContactPhoneNumber),
            UploadContactExtension = contact.PrimaryContactPhoneExtension,
            UploadContactEmail = contact.EmailAddress,
            UploadContactConfirmEmail = contact.EmailAddress,
            IsSameAsFileUploadContact = !string.IsNullOrWhiteSpace(contact.EmailAddress) && contact.EmailAddress == contact.SecondaryEmailAddress,
            QuestionsContactName = contact.SecondaryContactName,
            QuestionsContactPhoneNumber = FormatPhoneNumber(contact.SecondaryContactPhoneAreaCode, contact.SecondaryContactPhoneNumber),
            QuestionsContactExtension = contact.SecondaryContactPhoneExtension,
            QuestionsContactEmail = contact.SecondaryEmailAddress,
            QuestionsContactConfirmEmail = contact.SecondaryEmailAddress,
        };

        return model;
    }

    /// <summary>
    /// Country address format codes from Address.Countries select options
    /// </summary>
    private static readonly Dictionary<string, int> CountryAddressFormatCode = new()
    {
        { "United States", 1 },
        { "Canada", 2 },
        { "Other International", 3 },
    };

    private static readonly Dictionary<int, string> CountrySelectOptions = new()
    {
        { 1, "United States" },
        { 2, "Canada" },
        { 3, "Other International" },
    };

    /// <summary>
    /// Splits a formatted phone number into a 3-digit area code and 7-digit number.
    /// Returns empty strings if the input does not contain a valid US phone number.
    /// </summary>
    /// <remarks>
    /// Handles common formats: "(402) 555-1234", "402-555-1234", "402.555.1234", "1-402-555-1234", "+1 402 555 1234".
    /// </remarks>
    public static (string AreaCode, string Number) ParsePhoneNumber(string? input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            return (AreaCode: string.Empty, Number: string.Empty);
        }

        var digits = Regex.Replace(input, @"\D", ""); // strip everything but digits

        // drop leading US country code if present
        if (digits.Length == 11 && digits[0] == '1')
        {
            digits = digits[1..];
        }

        if (digits.Length != 10)
        {
            return (AreaCode: string.Empty, Number: string.Empty);
        }

        return (AreaCode: digits[..3], Number: digits[3..]);
    }

    /// <summary>
    /// Format a phone number, given an area code and phone number local part
    /// </summary>
    /// <param name="areaCode"></param>
    /// <param name="phoneNumber"></param>
    /// <returns>A phone number formatted as (xxx) xxx-xxxx or an empty string if the
    /// inputs do not have the required number of digits to represent and area code
    /// and local phone number.</returns>
    private static string FormatPhoneNumber(string areaCode, string phoneNumber)
    {
        var digitsAreaCode = Regex.Replace(areaCode ?? string.Empty, @"\D", "");
        var digitsPhoneNumber = Regex.Replace(phoneNumber ?? string.Empty, @"\D", "");

        return digitsAreaCode.Length != 3
            ? string.Empty
            : digitsPhoneNumber.Length != 7
            ? string.Empty
            : $"{digitsAreaCode}-{digitsPhoneNumber[..3]}-{digitsPhoneNumber[3..]}";
    }

    private static string UpperAlphanumericOnly(string? input)
    {
        return string.IsNullOrEmpty(input)
            ? string.Empty
            : Regex.Replace(input.ToUpperInvariant(), "[^a-zA-Z0-9]", "");
    }
}
