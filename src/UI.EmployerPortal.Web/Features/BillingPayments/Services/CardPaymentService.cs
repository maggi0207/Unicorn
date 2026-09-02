using System.Globalization;
using System.ServiceModel;
using System.Text;
using System.Text.Json;

//using System.Web;
using Com.Alacriti.Checkout.Api;
using log4net;
//using Com.Alacriti.Checkout.Model;
using Microsoft.Extensions.Caching.Memory;
using UI.EmployerPortal.Generated.ServiceClients.CardPaymentService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Dashboard;
//using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// Integrates with US Bank eBill Orbipay (Alacriti) for hosted card payment forms.
/// This service handles session creation and payment confirmation following Orbipay's
/// hosted payment workflow, mirroring the reference VB.NET CardPaymentPost.aspx.vb implementation.
/// </summary>
public interface ICardPaymentService
{
    /// <summary>
    /// Creates an Orbipay hosted form session for card payment entry.
    /// Returns HTML markup to embed the Orbipay form in the page.
    /// </summary>
    Task<OrbipaySessionResult> CreateHostedFormSessionAsync(
        decimal amount,
        string customerAccountReference,
        string contactName,
        string email,
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string zip,
        string country,
        bool isUsAddress);

    /// <summary>
    /// Confirms payment after Orbipay hosted form submission.
    /// Called when user completes the form and posts back to the application.
    /// Mirrors the reference CardPaymentPost workflow: extract token, call Alacriti API,
    /// save result to database.
    /// </summary>
    Task<OrbipayConfirmationResult> ConfirmPaymentAsync(
        string token,
        string digiSign,
        string customerAccountReference,
        int secureUserSk,
        object payload,
        string customerReference,
        OrbipayPaymentConfirmationRequest request);

    /// <summary>
    /// GetCardProfileAsync
    /// </summary>
    /// <param name="secureUserSK"></param>
    /// <param name="commonClientSk"></param>
    /// <returns></returns>
    Task<CardPaymentProfileModel> GetCardProfileAsync(int secureUserSK, int commonClientSk);

    /// <summary>
    /// GetCardProfileAsync
    /// </summary>
    /// <param name="secureUserSK"></param>
    /// <param name="commonClientSk"></param>
    /// <param name="cardPaymentReg"></param>
    /// <returns></returns>
    Task<int> SaveCardProfileAsync(int secureUserSK, int commonClientSk, CardPaymentProfileModel cardPaymentReg);
}

/// <summary>
/// Implementation of Orbipay hosted payment form integration.
/// Follows the US Bank eBill system workflow for card payments.
/// </summary>
internal sealed partial class CardPaymentService : ICardPaymentService
{
    private const string EbillConfigCacheKey = "BillingPayments:Orbipay:EbillConfiguration";
    private static readonly TimeSpan EbillConfigCacheDuration = TimeSpan.FromMinutes(15);
    private static readonly SemaphoreSlim EbillConfigLock = new(1, 1);

    private readonly ILogger<CardPaymentService> _logger;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ICardPaymentSystem _cardPaymentSystem;
    private static readonly Lock CheckoutInitLock = new();
    //private static bool CheckoutInitialized;
    private readonly IMemoryCache _memoryCache;
    private readonly string _idempotentRequestKey = "";
    private static readonly ILog Log = LogManager.GetLogger(typeof(CardPaymentService));
    private readonly IUserAccountService _userAccountService;
    private readonly IDashboardOrchestrator _dashboardOrchestrator;

    public CardPaymentService(
        ILogger<CardPaymentService> logger,
        ICardPaymentSystem cardPaymentSystem,
        IUserAccountService userAccountService,
        IDashboardOrchestrator dashboardOrchestrator,
        IMemoryCache memoryCache,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _cardPaymentSystem = cardPaymentSystem;
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _idempotentRequestKey = Guid.NewGuid().ToString().Replace("-", "");
        //EnsureCheckoutInitialized();
        _userAccountService = userAccountService;
        _dashboardOrchestrator = dashboardOrchestrator;

        //var employer = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
        //_employer = employer;
        //var userSk = UserAccountService.GetUserSKClaim();
        //var empSk = employer?.Id ?? 0;
    }

    /// <summary>
    /// Initializes Orbipay checkout SDK configuration one time per app domain.
    /// </summary>
    //private void EnsureCheckoutInitialized()
    //{
    //    if (CheckoutInitialized)
    //    {
    //        return;
    //    }

    //    lock (CheckoutInitLock)
    //    {
    //        if (CheckoutInitialized)
    //        {
    //            return;
    //        }

    //        var path = _config["Orbipay:CheckoutConfigPath"];
    //        if (string.IsNullOrWhiteSpace(path))
    //        {
    //            return;
    //        }

    //        if (!Path.IsPathRooted(path))
    //        {
    //            path = Path.Combine(AppContext.BaseDirectory, path);
    //        }

    //        if (!File.Exists(path))
    //        {
    //            return;
    //        }

    //        Checkout.initProperties(path);
    //        CheckoutInitialized = true;
    //    }
    //}

    private async Task<dynamic?> GetEbillConfigurationCachedAsync(CancellationToken cancellationToken = default)
    {
        if (_memoryCache.TryGetValue(EbillConfigCacheKey, out var cached) && cached is not null)
        {
            return cached;
        }

        await EbillConfigLock.WaitAsync(cancellationToken);
        try
        {
            if (_memoryCache.TryGetValue(EbillConfigCacheKey, out cached) && cached is not null)
            {
                return cached;
            }

            var config = await _cardPaymentSystem.GetEBillConfigurationAsync();
            if (config is null)
            {
                return null;
            }

            _memoryCache.Set(
                EbillConfigCacheKey,
                config,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = EbillConfigCacheDuration,
                    SlidingExpiration = TimeSpan.FromMinutes(5)
                });

            return config;
        }
        finally
        {
            EbillConfigLock.Release();
        }
    }

    public async Task<OrbipaySessionResult> CreateHostedFormSessionAsync(
        decimal amount,
        string customerAccountReference,
        string contactName,
        string email,
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string zip,
        string country,
        bool isUsAddress)
    {
        try
        {
            var ebillConfig = await GetEbillConfigurationCachedAsync();
            if (ebillConfig is null)
            {
                LogOrbipayConfigMissing(_logger);
                return new OrbipaySessionResult
                {
                    Success = false,
                    ErrorMessage = "Payment provider configuration is unavailable. Please contact support."
                };
            }

            string hostedFormUrl = ebillConfig.HostedFormURL ?? string.Empty;
            string clientKey = ebillConfig.TaxClientKey ?? string.Empty;
            const string Locale = "en";

            if (string.IsNullOrWhiteSpace(hostedFormUrl) || string.IsNullOrWhiteSpace(clientKey))
            {
                LogOrbipayConfigIncomplete(_logger);
                return new OrbipaySessionResult
                {
                    Success = false,
                    ErrorMessage = "Payment provider is not properly configured."
                };
            }

            var nameParts = (contactName ?? string.Empty).Split(' ', 2);
            var firstName = nameParts.Length > 0 ? nameParts[0] : string.Empty;
            var lastName = nameParts.Length > 1 ? nameParts[1] : string.Empty;

            var orbipayCountryCode = NormalizeOrbipayCountryCode(country);
            var html = BuildOrbipayFormMarkup(
                hostedFormUrl,
                "orbipay-checkout-form",
                "orbipay-checkout-script",
                clientKey,
                customerAccountReference,
                firstName,
                lastName,
                email,
                addressLine1,
                addressLine2,
                city,
                state,
                zip,
                orbipayCountryCode,
                amount,
                Locale,
                isUsAddress);

            LogHostedFormSessionCreated(_logger, amount);

            return new OrbipaySessionResult
            {
                Success = true,
                HostedFormHtml = html
            };
        }
        catch (Exception ex)
        {
            LogErrorCreatingHostedFormSession(_logger, ex);
            return new OrbipaySessionResult
            {
                Success = false,
                ErrorMessage = "An error occurred while preparing the payment form. Please try again."
            };
        }
    }

    public async Task<OrbipayConfirmationResult> ConfirmPaymentAsync(
        string token,
        string digiSign,
        string customerAccountReference,
        int secureUserSK,
        object payload,
        string customerReference,
        OrbipayPaymentConfirmationRequest request)
    {
        try
        {
            var payLoadObject = JsonSerializer.Serialize(payload);
            var tokenPrefix = token?.Length > 8 ? token[..8] + "..." : "(short)";
            var fullToken = $"token={token}&digisign={digiSign}&customer_account_reference={customerAccountReference}&customer_reference={customerReference}";
            Log.Info($"[STEP 1] ConfirmPaymentAsync invoked |" +
                $"CustomerAccountReference={customerAccountReference}, " +
                $"digiSign={digiSign}, " +
                $"payload={payLoadObject}, " +
                $"customerReference={customerReference}, " +
                $"Amount={request.Amount:F2}, " +
                $"FullToken={fullToken}");
            await WritePaymentLogAsync(
                $"[STEP 1] ConfirmPaymentAsync invoked |" +
                $"CustomerAccountReference={customerAccountReference}, " +
                $"Amount={request.Amount:F2}, " +
                $"TokenPrefix={tokenPrefix}"
                );

            Log.Info($"[STEP 2] Fetching ebill configuration from cache / WDV service");
            await WritePaymentLogAsync($"[STEP 2] Fetching ebill configuration from cache / WDV service");
            var ebillConfig = await _cardPaymentSystem.GetEBillConfigurationAsync();
            if (ebillConfig is null)
            {
                Log.Error($"[STEP 2] FAILED. eBill configuration null");
                await WritePaymentLogAsync($"[STEP 2] FAILED. eBill configuration null");
                LogOrbipayCredentialsIncomplete(_logger);
                return new OrbipayConfirmationResult
                {
                    Success = false,
                    ErrorDescription = "Payment provider credentials are incomplete."
                };
            }
            Log.Info($"[STEP 2] OK. eBill configuration retrived");
            await WritePaymentLogAsync($"[STEP 2] OK. eBill configuration retrived");

            Log.Info($"[STEP 3] Getting Card payment registration");
            await WritePaymentLogAsync($"[STEP 3] Getting Card payment registration");

            var employer = await _dashboardOrchestrator.GetSelectedEmployerAccountAsync();
            var userSk = _userAccountService.GetUserSKClaim();
            var empSk = employer?.Id ?? 0;

            var cardPaymentReg = await _cardPaymentSystem.ObtainPortalRegistrationAsync(userSk, empSk);
            //var cardPaymentReg = await _cardPaymentSystem.ObtainPortalRegistrationAsync(10021377, 7174161);
            if (cardPaymentReg is null)
            {
                Log.Error($"[STEP 3] FAILED. Card payment registration");
                await WritePaymentLogAsync($"[STEP 3] FAILED. Card payment registration");
                LogOrbipayCredentialsIncomplete(_logger);
                return new OrbipayConfirmationResult
                {
                    Success = false,
                    ErrorDescription = "Payment provider credentials are incomplete."
                };
            }
            var cardRegistrationSK = cardPaymentReg.RegistrationSK;


            Log.Info($"[STEP 4] Validating Orbipay credentials");
            await WritePaymentLogAsync($"[STEP 4] Validating Orbipay credentials");
            var clientKey = ebillConfig.TaxClientKey ?? string.Empty;
            var signatureKey = ebillConfig.TaxSecretKey ?? string.Empty;
            var clientApiKey = ebillConfig.TaxAPIKey ?? string.Empty;
            var clientPrivateKey = ebillConfig.TaxPrivateKey ?? string.Empty;
            var hwfPublicKey = ebillConfig.PublicKey ?? string.Empty;
            var liveMode = ebillConfig.LiveMode ? "true" : "false";
            customerAccountReference = cardPaymentReg.RegistrationSK.ToString();

            if (string.IsNullOrWhiteSpace(clientKey) ||
                string.IsNullOrWhiteSpace(signatureKey) ||
                string.IsNullOrWhiteSpace(clientApiKey) ||
                string.IsNullOrWhiteSpace(clientPrivateKey) ||
                string.IsNullOrWhiteSpace(hwfPublicKey))
            {
                Log.Error($"[STEP 4] Failed: One or more Orbipay credentials are missing or empty");
                await WritePaymentLogAsync($"[STEP 4] Failed: One or more Orbipay credentials are missing or empty");
                LogOrbipayCredentialsIncomplete(_logger);
                return new OrbipayConfirmationResult
                {
                    Success = false,
                    ErrorDescription = "Payment provider credentials are incomplete."
                };
            }

            Log.Info($"[STEP 4] OK: Credentials validated | LiveMode={liveMode}");
            await WritePaymentLogAsync($"[STEP 4] OK: Credentials validated | LiveMode={liveMode}");
            Log.Info($"[STEP 5] Building custom fields and InvocationContext");
            await WritePaymentLogAsync($"[STEP 5] Building custom fields and InvocationContext");
            var customFields = BuildCustomFields(request, ebillConfig);
            var invocationContext = new InvocationContext(clientApiKey, clientPrivateKey, hwfPublicKey, _idempotentRequestKey);
            //var invocationContext = new InvocationContext(clientApiKey, clientPrivateKey, hwfPublicKey);

            var customFieldsLog = string.Join(", ", customFields.Select(kv =>
            {
                return $"{kv.Key}={kv.Value}";
            }));
            //var invocationContextLog = string.Join(", ", invocationContext.GetType()
            //                        .GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance).Select(p =>
            //                        {
            //                            return $"{p.Name}-{p.GetValue(invocationContext)}";
            //                        }));

            var invocationContextLog = $"InvocationContext | " +
            $"clientApiKey={clientApiKey}, " +
                $"clientPrivateKey={clientPrivateKey}, " +
                $"hwfPublicKey={hwfPublicKey}, " +
                $"_idempotentRequestKey={_idempotentRequestKey}";

            //Log.Info($"[STEP 6] Calling Orbipay payment.Confirm() | " +
            //        $"CustomerAccountReference={customerAccountReference}, \n Amount={request.Amount:F2}," +
            //        $"\n Token={token}, \n DigiSign={digiSign}, \n ClientKey={clientKey}, " +
            //        $"\n SignatureKey={signatureKey}, \n ClientApiKey={clientApiKey}, \n ClientKey={clientKey}, " +
            //        $"\n CustomFields={customFieldsLog}, \n InvocationContext={invocationContextLog}, " +
            //        $"\n LiveMode={liveMode}"
            //        );
            //await WritePaymentLogAsync($"[STEP 6] Calling Orbipay payment.Confirm() | " +
            //        $"CustomerAccountReference={customerAccountReference}, \n Amount={request.Amount:F2}," +
            //        $"\n Token={token}, \n DigiSign={digiSign}, \n ClientKey={clientKey}, " +
            //        $"\n SignatureKey={signatureKey}, \n ClientApiKey={clientApiKey}, \n ClientKey={clientKey}, " +
            //        $"\n CustomFields={customFieldsLog}, \n InvocationContext={invocationContextLog}, " +
            //        $"\n LiveMode={liveMode}"
            //        );

            // call new payment method
            //var payment = new Com.Alacriti.Checkout.Api.Payment(customerAccountReference, request.Amount.ToString("F2", CultureInfo.InvariantCulture))
            //    .withToken(token, digiSign)
            //    .forClient(clientKey, signatureKey, clientApiKey)
            //    .withCustomFields(customFields)
            //    .confirm(invocationContext, liveMode);

            //Log.Info($"[STEP 7] Orbipay response received | " +
            //        $"IsNull={payment is null}, " +
            //        $"HasError={payment?.Error is not null}");
            //await WritePaymentLogAsync($"[STEP 7] Orbipay response received | " +
            //        $"IsNull={payment is null}, " +
            //        $"HasError={payment?.Error is not null}");

            //if (payment is null)
            //{
            //    Log.Error($"[STEP 7 Orbipay returned a null payment object");
            //    await WritePaymentLogAsync($"[STEP 7 Orbipay returned a null payment object");
            //    return new OrbipayConfirmationResult
            //    {
            //        Success = false,
            //        ErrorDescription = "Card payment failed"
            //    };
            //}

            //if (payment.Error is null)
            //{
            //    Log.Info(
            //        $"[STEP 8] Payment Success | " +
            //        $"ConfirmationNumber={payment.ConfirmationNumber}, " +
            //        $"RawAmount={payment.Amount}, " +
            //        $"PaymentMethod={payment.PaymentMethod}, " +
            //        $"Feeamount={payment.Fee?.Feeamount ?? "(none)"}, " +
            //        $"PaymentDate={payment.PaymentDate}");
            //    await WritePaymentLogAsync(
            //        $"[STEP 8] Payment Success | " +
            //        $"ConfirmationNumber={payment.ConfirmationNumber}, " +
            //        $"RawAmount={payment.Amount}, " +
            //        $"PaymentMethod={payment.PaymentMethod}, " +
            //        $"Feeamount={payment.Fee?.Feeamount ?? "(none)"}, " +
            //        $"PaymentDate={payment.PaymentDate}");
            //}
            //if (payment is not null && payment.Error is null)
            //{
            //var convenienceFee = 0m;
            //if (!string.IsNullOrWhiteSpace(payment.Fee?.Feeamount))
            //{
            //    _ = decimal.TryParse(payment.Fee.Feeamount, NumberStyles.Number, CultureInfo.InvariantCulture, out convenienceFee);
            //}

            //var amount = request.Amount;
            //if (!string.IsNullOrWhiteSpace(payment.Amount))
            //{
            //    _ = decimal.TryParse(payment.Amount, NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
            //}

            ////DateTime? paymentDate = null;
            ////if (!string.IsNullOrWhiteSpace(payment.PaymentDate) &&
            ////    DateTime.TryParse(payment.PaymentDate, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
            ////{
            ////    paymentDate = parsedDate;
            ////}

            //DateTime? paymentDate = null;
            //var paymentDateText = Convert.ToString(payment.PaymentDate, CultureInfo.InvariantCulture);
            //if (!string.IsNullOrWhiteSpace(paymentDateText) &&
            //    DateTime.TryParse(paymentDateText, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var parsedDate))
            //{
            //    paymentDate = parsedDate;
            //}

            //var lastFour = payment.FundingAccount?.AccountNumber;
            //if (!string.IsNullOrWhiteSpace(lastFour) && lastFour.Length > 4)
            //{
            //    lastFour = lastFour[^4..];
            //}

            //Log.Info(
            //    $"[STEP 8] Parsed | " +
            //    $"ConvenienceFee={convenienceFee}, " +
            //    $"LastFour={lastFour}, " +
            //    $"PaymentDate={paymentDate:0}");
            //await WritePaymentLogAsync(
            //    $"[STEP 8] Parsed | " +
            //    $"ConvenienceFee={convenienceFee}, " +
            //    $"LastFour={lastFour}, " +
            //    $"PaymentDate={paymentDate:0}");

            //Log.Info(
            //    $"[STEP 9] Persisting payment to SUITES | " +
            //    $"ConfirmationNumber={payment.ConfirmationNumber}");
            //await WritePaymentLogAsync(
            //    $"[STEP 9] Persisting payment to SUITES | " +
            //    $"ConfirmationNumber={payment.ConfirmationNumber}");

            //save payment method
            //await SaveERPortalPaymentAsync(payment, request, lastFour, convenienceFee);

            //var req = new StreamReader(Request.InputStream, Request.ContentEncoding);
            //string tokenstring = HttpUtility.UrlDecode(req.ReadToEnd());
            var requestJson = JsonSerializer.Serialize(request);
            // var tempToken = !string.IsNullOrWhiteSpace(token) ? token : "";
            Log.Info($"SaveCardPaymentAsync | " +
                        $"token={fullToken}, Request={requestJson}, SecureUserSk={secureUserSK}, cardRegistrationSK={cardRegistrationSK}");
            await WritePaymentLogAsync(
                $"SaveCardPaymentAsync | " +
                $"Token={fullToken}, " +
                $"Request={requestJson}, " +
                $"CardRegistrationSK={cardRegistrationSK}, " +
                $"SecureUserSk={secureUserSK}");

            var paymentResponse = await SaveCardPaymentAsync(fullToken, request, secureUserSK, cardRegistrationSK);
            //LogPaymentConfirmed(_logger, payment.ConfirmationNumber, amount);
            await WritePaymentLogAsync(
                $"PaymentResponse: {paymentResponse}");
            //Log.Info(
            //    $"[STEP 9] OK: payment saved to SUITES | " +
            //    $"ConfirmationNumber={payment.ConfirmationNumber}");
            //await WritePaymentLogAsync(
            //    $"[STEP 9] OK: payment saved to SUITES | " +
            //    $"ConfirmationNumber={payment.ConfirmationNumber}");
            if (paymentResponse != null)
            {
                var errorMessage = paymentResponse.RuleViolations?.Length > 0 ? paymentResponse.RuleViolations[0].RuleViolation : "";
                var confirmationNumber = string.IsNullOrWhiteSpace(paymentResponse.ConfirmationNumber) ? null : paymentResponse.ConfirmationNumber;
                await WritePaymentLogAsync(
                            $"paymentResponse | " +
                            $"ConfirmationNumber={confirmationNumber}, " +
                            $"RuleViolation={errorMessage}, " +
                            $"PhoneNumber={paymentResponse?.CollectionsPhoneNumber}");

                return paymentResponse?.RuleViolations?.Length > 0
                    ? new OrbipayConfirmationResult
                    {
                        Success = false,
                        ErrorDescription = paymentResponse.RuleViolations[0].RuleViolation,
                        ErrorField = paymentResponse.ErrorDescription,
                        ErrorCode = paymentResponse.ErrorDescription,
                        PhoneNumber = string.IsNullOrWhiteSpace(paymentResponse.CollectionsPhoneNumber) ? null : paymentResponse.CollectionsPhoneNumber,
                        DisplayError = paymentResponse.DisplayError ? "true" : "false"
                    }
                    : new OrbipayConfirmationResult
                    {
                        Success = true,
                        ConfirmationNumber = paymentResponse?.ConfirmationNumber,
                        Amount = request.Amount,
                        PaymentMethod = "cc",
                        LastFourDigits = "9999",
                        ConvenienceFee = 0m,
                        PaymentDate = DateTime.UtcNow
                    };
            }
            //}

            //var errors = payment?.Error?.ToList() ?? [];
            //var errorMessage = string.Join(" ", errors.Select(e =>
            //{
            //    return e.Message;
            //}).Where(m =>
            //{
            //    return !string.IsNullOrWhiteSpace(m);
            //}));
            //var errorField = string.Join(" ", errors.Select(e =>
            //{
            //    return e.Field;
            //}).Where(f =>
            //{
            //    return !string.IsNullOrWhiteSpace(f);
            //}));
            //var errorCode = string.Join(" ", errors.Select(e =>
            //{
            //    return e.Code;
            //}).Where(c =>
            //{
            //    return !string.IsNullOrWhiteSpace(c);
            //}));

            //Log.Error(
            //        $"[STEP 10] payment DECLINED by Orbipay | " +
            //        $"ErrorCode={errorCode}, ErrorField={errorField}, ErrorMessage={errorMessage}");
            //await WritePaymentLogAsync(
            //        $"[STEP 10] payment DECLINED by Orbipay | " +
            //        $"ErrorCode={errorCode}, ErrorField={errorField}, ErrorMessage={errorMessage}");

            //var displayErrorCodes = (string?) ebillConfig.DisplayErrorCodes ?? string.Empty;
            //var displayError = errors.Any(e =>
            //{
            //    return !string.IsNullOrWhiteSpace(e.Code) &&
            //                    e.Code != "0" &&
            //                    displayErrorCodes.Contains(e.Code, StringComparison.OrdinalIgnoreCase);
            //});

            //var phoneNumber = (string?) ebillConfig.EmployerCollectionsPhoneNumber ?? string.Empty;

            //LogPaymentError(_logger, errorMessage, errorCode);

            //return new OrbipayConfirmationResult
            //{
            //    Success = false,
            //    ErrorDescription = string.IsNullOrWhiteSpace(errorMessage) ? "Card payment failed." : errorMessage,
            //    ErrorField = errorField,
            //    ErrorCode = errorCode,
            //    PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber,
            //    DisplayError = displayError ? "true" : "false"
            //};
            return new OrbipayConfirmationResult
            {
                Success = false,
                ErrorDescription = "Card payment failed.",
                ErrorField = "",
                ErrorCode = "",
                PhoneNumber = "",
                DisplayError = "true"
            };
        }
        catch (CommunicationException ex)
        {
            Log.Error($"[ERROR] CommunicationException while contacting Orbipay | {ex.GetType().Name}: {ex.Message}", ex);
            await WritePaymentLogAsync($"[ERROR] CommunicationException while contacting Orbipay | {ex.GetType().Name}: {ex.Message} | InnerException: {ex.InnerException} | StackTrace: {ex.StackTrace}");

            LogCommunicationErrorWithAlacritiApi(_logger, ex);
            return new OrbipayConfirmationResult
            {
                Success = false,
                ErrorDescription = "Unable to reach the payment provider. Please try again."
            };
        }
        catch (Exception ex)
        {
            Log.Error($"[ERROR] Unexpected exception in ConfirmPaymentAsync | {ex.GetType().Name}: {ex.Message}", ex);
            await WritePaymentLogAsync($"[ERROR] Unexpected exception in ConfirmPaymentAsync | {ex.GetType().Name}: {ex.Message} | InnerException: {ex.InnerException} | StackTrace: {ex.StackTrace}");
            LogErrorConfirmingOrbipayPayment(_logger, ex);
            return new OrbipayConfirmationResult
            {
                Success = false,
                ErrorDescription = "An error occurred while processing your payment."
            };
        }
    }

    private async Task SaveERPortalPaymentAsync(Com.Alacriti.Checkout.Model.Payment payment,
                                                OrbipayPaymentConfirmationRequest request,
                                                string? lastFourDigits,
                                                decimal convenienceFee)
    {
        try
        {
            var paymentProxy = new eBillAccountProxy
            {
                ConfirmationNumber = payment.ConfirmationNumber,
                LastFourAccountNumber = lastFourDigits,
                PaymentMethod = payment.PaymentMethod,
                CardType = payment.PaymentMethod,
                Amount = request.Amount
            };
            //paymentProxy.RegistrationSk = request.RegistrationSk;
            if (convenienceFee != 0m)
            {
                paymentProxy.Fee = convenienceFee;
            }
            var paymentJson = payment.ToJson();
            await _cardPaymentSystem.SaveERPortalPaymentAsync(request.RegistrationSk, paymentProxy, paymentJson, request.IsVoluntary);
            LogPaymentSavedToSuites(_logger, payment.ConfirmationNumber);
        }
        catch (Exception ex)
        {
            LogErrorSavingPaymentToSuites(_logger, payment.ConfirmationNumber, ex);
        }
    }

    private async Task<CardPaymentResponse> SaveCardPaymentAsync(string tokenString,
                                                OrbipayPaymentConfirmationRequest request,
                                                int secureUserSK,
                                                int cardRegisrationSK = 0)
    {
        var cardPaymentResponse = new CardPaymentResponse();
        try
        {
            var paymentRequest = new CardPaymentRequest
            {
                CardRegisrationSK = cardRegisrationSK,
                EmployerSK = request.EmployerSk,
                PaymentAmount = Convert.ToString(request.Amount),
                SecureUserSK = secureUserSK,
                TokenRequestString = tokenString,
                VoluntaryPayment = request.IsVoluntary

            };
            return await _cardPaymentSystem.SaveEmployerPortalCardPaymentAsync(paymentRequest);
        }
        catch (Exception ex)
        {
            LogErrorSavingPaymentToSuites(_logger, "SaveCardPaymentAsync", ex);
        }
        return cardPaymentResponse;
    }

    /// <summary>Builds the Orbipay hosted form HTML markup.</summary>
    private static string BuildOrbipayFormMarkup(
        string hostedFormUrl,
        string formId,
        string scriptId,
        string clientKey,
        string customerAccountReference,
        string firstName,
        string lastName,
        string email,
        string addressLine1,
        string? addressLine2,
        string city,
        string? state,
        string zip,
        string country,
        decimal amount,
        string locale,
        bool isUsAddress)
    {
        var sb = new StringBuilder();
        var q = '"';

        sb.AppendLine($"<button id={q}orbipay-checkout-button{q} type={q}button{q} style={q}display:none;{q}>Pay</button>");

        sb.AppendLine($"<form id={q}{formId}{q} action={q}javascript:void(0){q} method={q}POST{q}>");
        sb.AppendLine($"<script id={q}{scriptId}{q} src={q}{hostedFormUrl}{q}");
        sb.AppendLine($"data-prevent_posting={q}true{q}");
        sb.AppendLine($"data-client_key={q}{HtmlEncode(clientKey)}{q}");
        sb.AppendLine($"data-api_event={q}create_payment{q}");
        sb.AppendLine($"data-customer_account_reference={q}{HtmlEncode(customerAccountReference)}{q}");
        sb.AppendLine($"data-payment_option={q}card{q}");
        sb.AppendLine($"data-payment_option_readonly={q}true{q}");
        sb.AppendLine($"data-amount={q}{amount.ToString("F2", CultureInfo.InvariantCulture)}{q}");
        sb.AppendLine($"data-customer_first_name={q}{HtmlEncode(firstName)}{q}");
        sb.AppendLine($"data-customer_last_name={q}{HtmlEncode(lastName)}{q}");
        sb.AppendLine($"data-customer_name={q}{HtmlEncode($"{firstName} {lastName}".Trim())}{q}");
        sb.AppendLine($"data-customer_email={q}{HtmlEncode(email)}{q}");
        sb.AppendLine($"data-customer_address_line1={q}{HtmlEncode(addressLine1)}{q}");
        sb.AppendLine($"data-customer_address_line2={q}{HtmlEncode(addressLine2 ?? string.Empty)}{q}");
        sb.AppendLine($"data-customer_city={q}{HtmlEncode(city)}{q}");
        sb.AppendLine($"data-customer_state={q}{HtmlEncode(state ?? string.Empty)}{q}");
        sb.AppendLine($"data-customer_country={q}{HtmlEncode(country)}{q}");
        sb.AppendLine($"data-locale={q}{HtmlEncode(locale)}{q}");

        // Orbipay requires data-customer_zip_code1 for US addresses and
        // data-customer_postal_code for non-US countries.
        // Reference: Orbipay integration docs + CardPaymentVerification.aspx.vb lines 104-107.
        if (isUsAddress)
        {
            sb.AppendLine($"data-customer_zip_code1={q}{HtmlEncode(zip)}{q}>");
        }
        else
        {
            sb.AppendLine($"data-customer_postal_code={q}{HtmlEncode(zip)}{q}>");
        }

        sb.AppendLine("</script>");
        sb.AppendLine("</form>");

        return sb.ToString();
    }

    /// <summary>Builds custom fields for Orbipay payment (max 64 chars per field).</summary>
    private static Dictionary<string, string> BuildCustomFields(OrbipayPaymentConfirmationRequest request, dynamic ebillConfig)
    {
        return new Dictionary<string, string>
        {
            { "cdf001", Truncate(request.EmployerLegalName, 64) },
            { "cdf002", "Employer Portal" },
            { "cdf003", request.UIAccountNumber },
            { "cdf004", request.EmployerAccountNumber },
            { "cdf005", request.IsVoluntary ? "Voluntary" : "Employer" },
            { "cdf006", Truncate((string?)ebillConfig.EmployerCollectionsWebsite ?? string.Empty, 64) },
            { "cdf007", Truncate((string?)ebillConfig.EmployerCollectionsPhoneNumber ?? string.Empty, 64) },
            { "cdf008", Truncate(request.EmployerAccountNumber, 5) }
        };
    }

    public async Task<CardPaymentProfileModel> GetCardProfileAsync(int secureUserSK, int commonClientSk)
    {
        var cardModal = new CardPaymentProfileModel();
        try
        {
            var cardPaymentReg = await _cardPaymentSystem.ObtainPortalRegistrationAsync(secureUserSK, commonClientSk);
            if (cardPaymentReg is null)
            {
                LogOrbipayCredentialsIncomplete(_logger);
                return cardModal;
            }
            cardModal.RegistrationSK = cardPaymentReg.RegistrationSK;
            cardModal.AccountNumber = cardPaymentReg.AccountNumber;
            cardModal.AccountType = cardPaymentReg.AccountType.ToString();
            cardModal.AddressLine1 = cardPaymentReg.AddressLine1;
            cardModal.AddressLine2 = cardPaymentReg.AddressLine2;
            cardModal.City = cardPaymentReg.City;
            cardModal.Company = cardPaymentReg.Company;
            cardModal.Country = cardPaymentReg.Country;
            cardModal.CustomerId = cardPaymentReg.CustomerID;
            cardModal.Email = cardPaymentReg.Email;
            cardModal.FirstName = cardPaymentReg.FirstName;
            cardModal.LastName = cardPaymentReg.LastName;
            cardModal.NonUSPostalCode = cardPaymentReg.NonUSPostalCode;
            cardModal.PhoneNumber = cardPaymentReg.Phone;
            cardModal.State = cardPaymentReg.State;
            cardModal.RegistrationFound = cardPaymentReg.RegistrationFound;
            cardModal.ISOCountryCode3 = cardPaymentReg.ISOCountryCode3;
            cardModal.ZipCode = cardPaymentReg.Zip;
            return cardModal;
        }
        catch
        {
            return cardModal;
        }
    }

    public async Task<int> SaveCardProfileAsync(int secureUserSK, int commonClientSk, CardPaymentProfileModel cardPaymentReg)
    {
        var cardModal = new eBillRegistrationProxy();
        try
        {
            if (Enum.TryParse<eBillRegistrationProxy.DebtorAccountType>(cardPaymentReg.AccountType, out var result))
            {
                cardModal.AccountType = result;
            }
            cardModal.RegistrationSK = cardPaymentReg.RegistrationSK;
            cardModal.AccountNumber = cardPaymentReg.AccountNumber;
            cardModal.AddressLine1 = cardPaymentReg.AddressLine1;
            cardModal.AddressLine2 = cardPaymentReg.AddressLine2;
            cardModal.City = cardPaymentReg.City;
            cardModal.Company = cardPaymentReg.Company;
            cardModal.Country = cardPaymentReg.Country;
            cardModal.CustomerID = cardPaymentReg.CustomerId;
            cardModal.Email = cardPaymentReg.Email;
            cardModal.FirstName = cardPaymentReg.FirstName;
            cardModal.LastName = cardPaymentReg.LastName;
            cardModal.NonUSPostalCode = cardPaymentReg.NonUSPostalCode;
            cardModal.Phone = cardPaymentReg.PhoneNumber;
            cardModal.State = cardPaymentReg.State;
            cardModal.RegistrationFound = cardPaymentReg.RegistrationFound;
            cardModal.ISOCountryCode3 = cardPaymentReg.ISOCountryCode3;
            cardModal.Zip = cardPaymentReg.ZipCode;

            var resultSk = await _cardPaymentSystem.SavePortalRegistrationAsync(cardModal, secureUserSK, commonClientSk);
            return resultSk.RegistrationSK;
        }
        catch (Exception ex)
        {
            LogErrorSavingPaymentToSuites(_logger, "SaveCardProfileAsync", ex);
        }
        return 0;
    }

    private static string Truncate(string? value, int maxLength)
    {
        return string.IsNullOrEmpty(value)
            ? string.Empty
            : value.Length > maxLength
                ? value[..maxLength]
                : value;
    }

    private static string HtmlEncode(string? value)
    {
        return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
    }

    /// <summary>
    /// Normalizes the selected country value to the 3-letter Orbipay country code.
    /// </summary>
    private static string NormalizeOrbipayCountryCode(string? country)
    {
        return string.IsNullOrWhiteSpace(country)
            ? "USA"
            : country.Equals("UNITED STATES", StringComparison.OrdinalIgnoreCase) ||
            country.Equals("UNITED STATES OF AMERICA", StringComparison.OrdinalIgnoreCase) ||
            country.Equals("USA", StringComparison.OrdinalIgnoreCase)
            ? "USA"
            : country;
    }

    private static async Task WritePaymentLogAsync(string message)
    {
        const string LogFile = @"\\WWWMAD0D7933\vol1\AppLogs\UI\Tax\EmployerPortal\payment_responses.txt";
        try
        {
            var timestamp = DateTimeOffset.Now.ToString("yyyy-MM-dd HH:mm:sss.fff zzz", CultureInfo.InvariantCulture);
            var line = $"[{timestamp}] {message}{Environment.NewLine}";
            await File.AppendAllTextAsync(LogFile, line);
        }
        catch
        {
            throw;
        }
    }
    #region LoggerMessage Delegates

    [LoggerMessage(
        EventId = 1001,
        Level = LogLevel.Error,
        Message = "Orbipay configuration section not found in appsettings")]
    private static partial void LogOrbipayConfigMissing(ILogger logger);

    [LoggerMessage(
        EventId = 1002,
        Level = LogLevel.Error,
        Message = "Orbipay HostedFormUrl or ClientKey is missing")]
    private static partial void LogOrbipayConfigIncomplete(ILogger logger);

    [LoggerMessage(
        EventId = 1003,
        Level = LogLevel.Information,
        Message = "Orbipay hosted form session created for amount {Amount}")]
    private static partial void LogHostedFormSessionCreated(ILogger logger, decimal amount);

    [LoggerMessage(
        EventId = 1004,
        Level = LogLevel.Error,
        Message = "Error creating Orbipay hosted form session")]
    private static partial void LogErrorCreatingHostedFormSession(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1005,
        Level = LogLevel.Error,
        Message = "Orbipay credentials incomplete")]
    private static partial void LogOrbipayCredentialsIncomplete(ILogger logger);

    [LoggerMessage(
        EventId = 1006,
        Level = LogLevel.Information,
        Message = "Payment confirmed: ConfirmationNumber={ConfirmationNumber}, Amount={Amount}")]
    private static partial void LogPaymentConfirmed(ILogger logger, string confirmationNumber, decimal? amount);

    [LoggerMessage(
        EventId = 1007,
        Level = LogLevel.Error,
        Message = "Communication error with Alacriti payment API")]
    private static partial void LogCommunicationErrorWithAlacritiApi(ILogger logger, Exception ex);

    [LoggerMessage(
        EventId = 1008,
        Level = LogLevel.Error,
        Message = "Error confirming Orbipay payment")]
    private static partial void LogErrorConfirmingOrbipayPayment(ILogger logger, Exception ex);

    [LoggerMessage(
       EventId = 1009,
       Level = LogLevel.Warning,
       Message = "Orbipay payment error: {ErrorMessage} (codes: {ErrorCodes})")]
    private static partial void LogPaymentError(ILogger logger, string errorMessage, string errorCodes);

    [LoggerMessage(
        EventId = 1010,
        Level = LogLevel.Information,
        Message = "Payment saved to SUITES: ConfirmationNumber={ConfirmationNumber}")]
    private static partial void LogPaymentSavedToSuites(ILogger logger, string confirmationNumber);

    [LoggerMessage(
        EventId = 1011,
        Level = LogLevel.Error,
        Message = "Failed to save payment {ConfirmationNumber} to SUITES. Manual reconciliation required.")]
    private static partial void LogErrorSavingPaymentToSuites(ILogger logger, string confirmationNumber, Exception ex);

    #endregion
}
