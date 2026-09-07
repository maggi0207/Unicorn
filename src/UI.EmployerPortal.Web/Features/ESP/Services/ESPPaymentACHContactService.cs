using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;


namespace UI.EmployerPortal.Web.Features.ESP.Services;
/// <summary>
/// 
/// </summary>
public interface IESPPaymentACHContactService
{
    ///<summary>Saves Web contact information</summary>
    Task<string?> SaveWebContact(ESPContactModel model, int secureUserSK, int employersk);
    ///<summary>Saves ESP Web contact information</summary>
    Task<string?> SaveESPWebContact(ESPContactModel model, int secureUserSK, int employersk);
    /// <summary>
    /// 
    /// </summary>
    /// <param name="secureUserSK"></param>
    /// <param name="contactTypeCodeSK"></param>
    /// <returns></returns>

    Task<ESPContactModel?> GetESPWebContact(int secureUserSK, int contactTypeCodeSK);
}
internal class ESPPaymentACHContactService : IESPPaymentACHContactService
{
    private readonly IESPService _espService;
    private readonly IAsyncRetryPolicy<ESPPaymentACHContactService> _retryPolicy;
    public ESPPaymentACHContactService(

        IESPService espService,
        IAsyncRetryPolicy<ESPPaymentACHContactService> retryPolicy)
    {
        _espService = espService;
        _retryPolicy = retryPolicy;
    }

    public async Task<string?> SaveWebContact(ESPContactModel model, int secureUserSK, int employersk)
    {
        var wciProxy = new SaveESPWebContactRequest
        {
            SecureUserSK = secureUserSK,
            ContactTypeCodeSK = 4,//ESP
            ContactName = model.ContactName,
            PhoneNumber = model.PhoneNumber,
            PhoneNumberExtension = model.PhoneExt,
            EmailAddress = model.Email,
            InternationalPhoneNumberFlag = model.InternationalFlag,
            WebContactSK = model.WebContactInformationsk

        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.SaveWebContactAsync(wciProxy);
        });

        return response?.WebContact?.ToString();
    }
    public async Task<string?> SaveESPWebContact(ESPContactModel model, int secureUserSK, int employersk)
    {
        var wciProxy = new SaveESPWebContactRequest
        {
            SecureUserSK = secureUserSK,
            ContactTypeCodeSK = 4,//ESP
            ContactName = model.ContactName,
            PhoneNumber = model.PhoneNumber,
            PhoneNumberExtension = model.PhoneExt,
            EmailAddress = model.Email,
            InternationalPhoneNumberFlag = model.InternationalFlag,
            WebContactSK = model.WebContactInformationsk

        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.SaveWebContactAsync(wciProxy);
        });

        return response?.WebContact?.ToString();
    }
    public async Task<ESPContactModel?> GetESPWebContact(int secureUserSK, int contactTypeCodeSK)
    {
        var request = new ObtainESPWebContactRequest
        {
            ContactTypeCodeSK = contactTypeCodeSK,
            SecureUserSK = secureUserSK
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.ObtainWebContactAsync(request);
        });

        return response?.WebContactInformation == null ? null : MapESPcontacttoModel(response.WebContactInformation);
        ;
    }


    private static ESPContactModel MapESPcontacttoModel(WebContactInformationProxy proxy)
    {
        return new ESPContactModel
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
}
