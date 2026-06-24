using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using EmailSubscriptionResponse = UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.EmailSubscriptionResponse;

namespace UI.EmployerPortal.Web.Features.Shared.OtherFunctions.Services;


/// <summary>
/// Services responsible for retrieving and saving email subscriptions
/// </summary>
public interface IEmailSubscriptionService
{
    /// <summary>
    /// Retrieves email subscription for secure user
    /// </summary>
    /// <returns></returns>
    Task<EmailSubscription?> GetEmailSubscriptions();

    /// <summary>
    /// Saves email subscriptions
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task<EmailSubscriptionResponse> SaveEmailSubscriptions(EmailSubscription model);

    /// <summary>
    /// Services responsible for retreiving and saving email subscriptions
    /// </summary>
    internal class EmailSubscriptionService : IEmailSubscriptionService
    {
        private readonly IPortalUtilityService _portalUtilityService;

        private readonly IAsyncRetryPolicy<EmailSubscriptionService> _retryPolicy;

        private readonly IUserAccountService _userAccountService;

        /// <summary>
        /// Initializes service
        /// </summary>
        /// <param name="portalUtilityService"></param>
        /// <param name="retryPolicy"></param>
        /// <param name="userAccountService"></param>
        public EmailSubscriptionService(IPortalUtilityService portalUtilityService, IAsyncRetryPolicy<EmailSubscriptionService>
            retryPolicy, IUserAccountService userAccountService)
        {
            _portalUtilityService = portalUtilityService;
            _retryPolicy = retryPolicy;
            _userAccountService = userAccountService;
        }

        public async Task<EmailSubscription?> GetEmailSubscriptions()
        {
            var secureUserSk = _userAccountService.GetUserSKClaim();

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _portalUtilityService.LoadEmailSubscriptionsAsync(secureUserSk);
            });

            return response?.EmailSubscription == null ? (EmailSubscription?) null : MapProxyToModel(response.EmailSubscription);
        }

        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task<EmailSubscriptionResponse> SaveEmailSubscriptions(EmailSubscription model)
        {
            var secureUserSK = _userAccountService.GetUserSKClaim();

            var proxy = new SubscriptionEmailProxy
            {
                SecureUserSK = secureUserSK,

                EmailAddress = model.EmailAddress,
                TimeToFile = model.TimeToFileUiTaxAndWageReport,
                UINews = model.UnemploymentInsuranceNews
            };

            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _portalUtilityService.SaveSubscriptionEmailAsync(proxy);
            });

            return response;
        }

        /// <summary>
        /// Maps proxy to model
        /// </summary>
        /// <param name="proxy"></param>
        /// <returns></returns>
        private static EmailSubscription MapProxyToModel(SubscriptionEmailProxy proxy)
        {
            return new EmailSubscription
            {
                EmailAddress = proxy.EmailAddress,
                ConfirmEmailAddress = proxy.EmailAddress,

                TimeToFileUiTaxAndWageReport = proxy.TimeToFile,
                UnemploymentInsuranceNews = proxy.UINews
            };

        }

    }

}
