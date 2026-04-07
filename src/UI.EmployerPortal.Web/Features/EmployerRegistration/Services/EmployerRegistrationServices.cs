using System.Collections.Concurrent;
using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.EmployerRegistrationService;
using UI.EmployerPortal.Generated.ServiceClients.PortalCorrespondenceService;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;


internal interface IEmployerRegistrationServices
{
    Task<List<RegisterEmployer>> GetRegisterEmployer();

    Task<PortalContinueRegistrationResponse> ContinueRegistration(
        string fein,
        string surveyNumberText);

    Task<CorrespondencePDFResponse> GeneratePDFIDForRegistration(PortalCorrespondenceIDRequest pcidr);
}
/// <summary>
/// Get Rigister Wcf call
/// </summary>
internal class EmployerRegistrationServices : IEmployerRegistrationServices
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IAsyncRetryPolicy<UserAccountService> _retryPolicy;
    private readonly IUserAccountService _userAccountService;
    private readonly IEmployerRegistrationService _employerRegistrationService;
    private readonly IPortalCorrespondenceService _portalCorrespondenceService;

    public EmployerRegistrationServices(
      IHttpContextAccessor httpContextAccessor,
       IAsyncRetryPolicy<UserAccountService> retryPolicy,
        IUserAccountService userAccountService,
      IEmployerRegistrationService portalRegistrationService,
      IPortalCorrespondenceService portalCorrespondenceService)
    {
        _httpContextAccessor = httpContextAccessor;
        _retryPolicy = retryPolicy;
        _userAccountService = userAccountService;
        _employerRegistrationService = portalRegistrationService;
        _portalCorrespondenceService = portalCorrespondenceService;
    }
    public async Task<List<RegisterEmployer>> GetRegisterEmployer()
    {
        var associatedregemp = new ConcurrentBag<RegisterEmployer>();
        var registeredemployee = await _retryPolicy.ExecuteAsync(() =>
        {
            return _employerRegistrationService.RegisterEmployerAsync(
                new RequestForEmployerRegistration()
                {
                    SecureUserSK = _userAccountService.GetUserSKClaim(),
                    SurveyResponseSK = _userAccountService.GetUserSKClaim()
                });
        });

        //return registeredemployee == null ?  [.. associatedregemp] : (List<RegisterEmployer>) [.. associatedregemp];
        if (registeredemployee == null)
        {
            return [.. associatedregemp];
        }
        else
        {
            associatedregemp.Add(new RegisterEmployer()
            {
                UIAccountNumber = registeredemployee.UIAccountNumber,
                SuitesAccountNumber = registeredemployee.SUITESAccountNumber
            });

        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        await Parallel.ForEachAsync(
            source: registeredemployee.RuleViolations,
            parallelOptions: new ParallelOptions
            {
                MaxDegreeOfParallelism = 32,
                CancellationToken = cts.Token
            },
            async (item, token) =>
            {
                associatedregemp.Add(new RegisterEmployer()
                {
                    RuleId = item.RuleID,
                    RuleViolation = item.RuleViolation,


                });

            });
        return [.. associatedregemp];
    }

    public async Task<PortalContinueRegistrationResponse> ContinueRegistration(string fein, string surveyNumberText)
    {
        try
        {
            return await _retryPolicy.ExecuteAsync(() =>
            {
                return _employerRegistrationService.ContinueRegistrationAsync(
                  new ContinueRegistrationRequest()
                  {
                      FEIN = fein,
                      SecureUserSK = _userAccountService.GetUserSKClaim(),
                      SurveyNumberText = surveyNumberText
                  });
            });
        }
        catch (CommunicationException)
        {
            //WCF COmmunication Failure
            return new PortalContinueRegistrationResponse
            {
                Message = "Service is temporarily unavailable. Please try again."
            };
        }
        catch (Exception)
        {
            //Unexpected Error
            return new PortalContinueRegistrationResponse
            {
                Message = "An unexpected error occured. Please try again."
            };
        }

    }

    public async Task<CorrespondencePDFResponse> GeneratePDFIDForRegistration(PortalCorrespondenceIDRequest pcidr)
    {
        try
        {
            var secureUserSK = _userAccountService.GetUserSKClaim();

            pcidr.SecureUserSK = secureUserSK;

            return await _retryPolicy.ExecuteAsync(() =>
            {
                return _portalCorrespondenceService.GeneratePDFIDForRegistrationAsync(pcidr);
            });
        }
        catch (CommunicationException)
        {
            //WCF COmmunication Failure
            //return CorrespondencePDFResponse
            //{
            //    Message = "Service is temporarily unavailable. Please try again."
            //};
            //return "";
            throw;
        }
        catch (Exception)
        {
            //Unexpected Error
            //return new CorrespondencePDFResponse
            //{
            //    Message = "An unexpected error occured. Please try again."
            //};
            throw;
        }

    }
}
