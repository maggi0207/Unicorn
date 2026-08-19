using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.EFTPaymentService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Services;

/// <summary>
/// IEFTPaymentService
/// </summary>
public interface IEftPaymentService
{
    /// <summary>
    /// GetEftPaymentDatesAsync
    /// </summary>
    /// <returns></returns>
    Task<EftPaymentDatesResult> GetEftPaymentDatesAsync();

    /// <summary>
    /// Cancel Payment
    /// </summary>
    /// <param name="eftPaymentSK"></param>
    /// <param name="secureUserSK"></param>
    /// <param name="employerSK"></param>
    /// <returns></returns>
    Task<CancelEFTPaymentResponse> CancelEftPaymentAsync(int eftPaymentSK, int secureUserSK, int employerSK);

    /// <summary>
    /// Save Payment
    /// </summary>
    /// <param name="eFTPaymentRequest"></param>
    Task<EftPaymentResponse> SaveEftPaymentAsync(EftPaymentRequest eFTPaymentRequest);

    /// <summary>
    /// Returns all US state codes from the WCF <c>GetUSStateCodes</c> operation.
    /// Each item contains <c>CodeSK</c>, <c>LongDescription</c> (full state name),
    /// and <c>ShortDescription</c> (two-letter abbreviation, e.g. "WI").
    /// </summary>
    Task<IReadOnlyList<CodeLookupItem>> GetUSStateCodesAsync();

    /// <summary>
    /// Returns all country codes from the WCF <c>GetCountryCodes</c> operation.
    /// Each item contains <c>CodeSK</c>, <c>LongDescription</c> (country name),
    /// and <c>ShortDescription</c> (two-letter ISO code, e.g. "US").
    /// </summary>
    Task<IReadOnlyList<CodeLookupItem>> GetCountryCodesAsync();

    /// <summary>
    /// Returns all card and ACH payment history for the given employer
    /// from the WCF <c>LoadCardEFTPaymentHistory</c> operation.
    /// </summary>
    Task<LoadCardEFTPaymentActivityResponse> LoadCardEftPaymentHistoryAsync(int employerSk, int secureUserSk);

    /// <summary>
    /// Returns the full detail for a single EFT payment from the WCF <c>LoadEFTPayment</c> operation.
    /// Returns null on service failure.
    /// </summary>
    Task<LoadEFTPaymentResponse?> LoadEftPaymentAsync(int eftPaymentSk, int secureUserSk, int employerSk);

    /// <summary>
    /// Returns the audit/activity history for a single EFT payment from the WCF <c>LoadEFTPaymentHistory</c> operation.
    /// Returns null on service failure.
    /// </summary>
    Task<LoadEFTPaymentHistoryResponse?> LoadEftPaymentHistoryAsync(int eftPaymentSk, int secureUserSk, int employerSk);
}

/// <summary>EftPaymentService</summary>
public sealed class EftPaymentService : IEftPaymentService
{
    private readonly IEFTPaymentService _wcfclient;
    /// <summary>
    /// EftPaymentService
    /// </summary>
    /// <param name="wcfclient"></param>
    public EftPaymentService(IEFTPaymentService wcfclient)
    {
        _wcfclient = wcfclient;
    }
    /// <summary>
    /// GetEftPaymentDatesAsync
    /// </summary>
    /// <returns></returns>
    public async Task<EftPaymentDatesResult> GetEftPaymentDatesAsync()
    {
        var response = await _wcfclient.GetEFTPaymentDatesAsync();
        var holidays = (response.BankHolidays ?? [])
                        .Select(DateOnly.FromDateTime)
                        .ToList();
        var firstAvailable = response.FirstAvailableSettlementDate?.Value is { } dt
                             ? DateOnly.FromDateTime(dt)
                             : DateOnly.FromDateTime(DateTime.Today).AddDays(1);

        return new EftPaymentDatesResult
        {
            BankHolidays = holidays,
            FirstAvailableSettlementDate = firstAvailable
        };
    }

    /// <summary>
    /// Cancel Payment
    /// </summary>
    /// <param name="eftPaymentSK"></param>
    /// <param name="secureUserSK"></param>
    /// <param name="employerSK"></param>
    /// <returns></returns>
    public async Task<CancelEFTPaymentResponse> CancelEftPaymentAsync(int eftPaymentSK, int secureUserSK, int employerSK)
    {
        return await _wcfclient.CancelEFTPaymentAsync(eftPaymentSK, secureUserSK, employerSK);
    }

    /// <summary>
    /// Save Payment
    /// </summary>
    /// <param name="eFTPaymentRequest"></param>
    /// <returns></returns>
    public async Task<EftPaymentResponse> SaveEftPaymentAsync(EftPaymentRequest eFTPaymentRequest)
    {
        var response = new EftPaymentResponse();
        response.ErrorMessage = "An error occured while processing your payment.";
        if (eFTPaymentRequest.EFTPaymentSK > 0)
        {
            var paymentRequest = new EditEFTPaymentRequest()
            {
                EFTPaymentSK = eFTPaymentRequest.EFTPaymentSK,
                BankAccountSK = eFTPaymentRequest.BankAccountSK,
                FileUploadDetailSK = eFTPaymentRequest.FileUploadDetailSK,
                PaymentAmount = eFTPaymentRequest.PaymentAmount,
                PaymentSettlementDate = eFTPaymentRequest.PaymentSettlementDate,
                PaymentType = eFTPaymentRequest.PaymentType,
                SecureUserSK = eFTPaymentRequest.SecureUserSK,
                SentDate = DateTime.Now,
                EmployerSK = eFTPaymentRequest.EmployerSK
            };
            var result = await _wcfclient.EditEFTPaymentAsync(paymentRequest);
            if (result != null)
            {
                if (!string.IsNullOrWhiteSpace(result.ConfirmationID))
                {
                    response.ConfirmationId = result.ConfirmationID;
                    response.EFTPaymentSK = result.EFTPaymentSK;
                    response.IsAuthorized = true;
                    response.TransactionDateTime = DateTime.Now;
                }
                else if (result.RuleViolations != null)
                {
                    foreach (var violation in result.RuleViolations)
                    {
                        response.ErrorMessage += violation.RuleViolation.ToString() + ". ";
                    }
                }
            }
        }
        else
        {
            var paymentRequest = new CreateEFTPaymentRequest()
            {
                BankAccountSK = eFTPaymentRequest.BankAccountSK,
                FileUploadDetailSK = eFTPaymentRequest.FileUploadDetailSK,
                PaymentAmount = eFTPaymentRequest.PaymentAmount,
                PaymentSettlementDate = eFTPaymentRequest.PaymentSettlementDate,
                PaymentType = eFTPaymentRequest.PaymentType,
                SecureUserSK = eFTPaymentRequest.SecureUserSK,
                SentDate = DateTime.Now,
                EmployerSK = eFTPaymentRequest.EmployerSK
            };
            var result = await _wcfclient.CreateEFTPaymentAsync(paymentRequest);
            if (result != null)
            {
                if (!string.IsNullOrWhiteSpace(result.ConfirmationID))
                {
                    response.ConfirmationId = result.ConfirmationID;
                    response.EFTPaymentSK = result.EFTPaymentSK;
                    response.IsAuthorized = true;
                    response.TransactionDateTime = DateTime.Now;
                }
                else if (result.RuleViolations != null)
                {
                    foreach (var violation in result.RuleViolations)
                    {
                        response.ErrorMessage += violation.RuleViolation.ToString() + ". ";
                    }
                }
            }
        }
        return response;
    }

    /// <summary>
    /// Get US state codes from the WCF <c>GetUSStateCodes</c> operation.
    /// Each item contains <c>CodeSK</c>, <c>LongDescription</c> (full state name),
    /// and <c>ShortDescription</c> (two-letter abbreviation, e.g. "WI").
    /// </summary>
    public async Task<IReadOnlyList<CodeLookupItem>> GetUSStateCodesAsync()
    {
        try
        {
            var items = await _wcfclient.GetUSStateCodesAsync();

            return items is null || items.Length == 0
                ? []
                : (IReadOnlyList<CodeLookupItem>) items
                .Select(s =>
                {
                    return new CodeLookupItem
                    {
                        CodeSK = s.CodeSK,
                        LongDescription = s.LongDescription ?? string.Empty,
                        ShortDescription = s.ShortDescription ?? string.Empty
                    };
                })
                .OrderBy(s =>
                {
                    return s.LongDescription;
                })
                .ToList();
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    /// <summary>
    /// Get country codes from the WCF <c>GetCountryCodes</c> operation.
    /// Each item contains <c>CodeSK</c>, <c>LongDescription</c> (country name),
    /// and <c>ShortDescription</c> (two-letter ISO code, e.g. "US").
    /// </summary>
    public async Task<IReadOnlyList<CodeLookupItem>> GetCountryCodesAsync()
    {
        try
        {
            var items = await _wcfclient.GetCountryCodesAsync();

            return items is null || items.Length == 0
                ? []
                : (IReadOnlyList<CodeLookupItem>) items
                .Select(c =>
                {
                    return new CodeLookupItem
                    {
                        CodeSK = c.CodeSK,
                        LongDescription = c.LongDescription ?? string.Empty,
                        ShortDescription = c.ShortDescription ?? string.Empty
                    };
                })
                .OrderBy(c =>
                {
                    return c.LongDescription;
                })
                .ToList();
        }
        catch (CommunicationException)
        {
            return [];
        }
    }

    /// <inheritdoc />
    public async Task<LoadCardEFTPaymentActivityResponse> LoadCardEftPaymentHistoryAsync(int employerSk, int secureUserSk)
    {
        return await _wcfclient.LoadCardEFTPaymentHistoryAsync(new SimpleEmployerRequest
        {
            EmployerSK = employerSk,
            SecureUserSK = secureUserSk
        });
    }

    /// <inheritdoc />
    public async Task<LoadEFTPaymentResponse?> LoadEftPaymentAsync(int eftPaymentSk, int secureUserSk, int employerSk)
    {
        try
        {
            return await _wcfclient.LoadEFTPaymentAsync(eftPaymentSk, secureUserSk, employerSk);
        }
        catch (CommunicationException)
        {
            return null;
        }
    }

    /// <inheritdoc />
    public async Task<LoadEFTPaymentHistoryResponse?> LoadEftPaymentHistoryAsync(int eftPaymentSk, int secureUserSk, int employerSk)
    {
        try
        {
            return await _wcfclient.LoadEFTPaymentHistoryAsync(eftPaymentSk, secureUserSk, employerSk);
        }
        catch (CommunicationException)
        {
            return null;
        }
    }
}
