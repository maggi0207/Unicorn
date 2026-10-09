using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;


namespace UI.EmployerPortal.Web.Features.ESP.Services;
/// <summary>
/// 
/// </summary>
public interface IESPPaymentACHContactService
{
    ///<summary>Saves ESP Web contact information</summary>
    Task<(bool success, string error)> SaveESPWebContact(ESPContactModel model, int secureUserSK);
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

    public async Task<(bool success, string error)> SaveESPWebContact(ESPContactModel model, int secureUserSK)
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
        return response?.RuleViolations is { Length: > 0 } violations
        ? new ESPContactModel
        {
            RuleViolations = ToMessages(violations.Select(v =>
            {
                return v.RuleViolation;
            }))
        }
          : response?.WebContactInformation == null ? (ESPContactModel?) null : MapESPcontacttoModel(response.WebContactInformation);
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
    private static List<string> ToMessages(IEnumerable<string?> ruleViolations)
    {
        return ruleViolations
            .Select(v =>
            {
                return v ?? string.Empty;
            })
            .Where(m =>
            {
                return !string.IsNullOrWhiteSpace(m);
            })
            .ToList();
    }
}
