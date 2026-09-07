using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using CancelEFTPaymentResponse = UI.EmployerPortal.Generated.ServiceClients.ESPService.CancelEFTPaymentResponse;
using LoadEFTPaymentResponse = UI.EmployerPortal.Generated.ServiceClients.ESPService.LoadEFTPaymentResponse;
using RuleViolationProxy = UI.EmployerPortal.Generated.ServiceClients.ESPService.RuleViolationProxy;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// Loads and manages ESP tax report file uploads pending EFT payment, and the EFT payments made against them.
/// </summary>
public interface IEspPaymentService
{
    /// <summary>
    /// Returns the tax report file uploads that have passed review and are awaiting EFT payment initiation.
    /// </summary>
    Task<PendingTaxFilePaymentsResult> GetPendingTaxFilePaymentsAsync();

    /// <summary>
    /// Cancels an EFT payment.
    /// </summary>
    /// <param name="eftPaymentSK">Surrogate key of the payment to cancel.</param>
    /// <param name="secureUserSK">Surrogate key of the requesting user.</param>
    Task<CancelEFTPaymentResponse> CancelEspPaymentAsync(int eftPaymentSK, int secureUserSK);

    /// <summary>
    /// Creates or edits an EFT payment for a tax file upload.
    /// </summary>
    /// <param name="eFTPaymentRequest">The payment details to save.</param>
    Task<ESPPaymentResponse> SaveEspPaymentAsync(ESPPaymentRequest eFTPaymentRequest);

    /// <summary>
    /// Loads a single EFT payment.
    /// </summary>
    /// <param name="eftPaymentSk">Surrogate key of the payment to load.</param>
    /// <param name="secureUserSk">Surrogate key of the requesting user.</param>
    Task<LoadEFTPaymentResponse?> LoadEspPaymentAsync(int eftPaymentSk, int secureUserSk);

    /// <summary>
    /// Cancels processing of a tax report file upload.
    /// </summary>
    /// <param name="fileUploadDetailSk">Surrogate key of the file upload to cancel.</param>
    Task<(bool Success, string? ErrorMessage)> CancelTaxFileUploadAsync(int fileUploadDetailSk);
}

/// <summary>Default implementation of <see cref="IEspPaymentService"/>.</summary>
internal sealed class EspPaymentService : IEspPaymentService
{
    private readonly IUserAccountService _userAccountService;
    private readonly IESPService _espService;
    private readonly ISessionManager _sessionManager;

    /// <summary>Initializes a new instance of <see cref="EspPaymentService"/>.</summary>
    public EspPaymentService(
        IUserAccountService userAccountService,
        IESPService espService,
        ISessionManager sessionManager)
    {
        _userAccountService = userAccountService;
        _espService = espService;
        _sessionManager = sessionManager;
    }

    /// <inheritdoc/>
    public async Task<PendingTaxFilePaymentsResult> GetPendingTaxFilePaymentsAsync()
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        try
        {
            var response = await _espService.LoadTaxUploadsForPendingPaymentAsync(new ObtainFileUploadByUserRequest
            {
                SecureUserSk = secureUserSk
            });

            if (response?.RuleViolations is { Length: > 0 })
            {
                var errorMessage = string.Join(" ", response.RuleViolations.Select(v =>
                {
                    return $"{v.RuleViolation}.";
                }));

                return new PendingTaxFilePaymentsResult { ErrorMessage = errorMessage };
            }

            var payments = response?.Files is null
            ? []
            : (IReadOnlyList<PendingTaxFilePayment>) response.Files
            .Select(f =>
            {
                return new PendingTaxFilePayment
                {
                    FileUploadDetailSk = f.FileUploadDetailSK ?? 0,
                    UploadDate = f.UploadDate ?? default,
                    FileName = f.FormattedFileName ?? f.OriginalFileName ?? string.Empty,
                    ConfirmationId = f.ConfirmationID ?? string.Empty,
                    ReportsWithoutErrors = f.ReportsWithoutErrorsCount ?? 0,
                    PaymentAmount = f.PaymentAmount ?? 0m
                };
            })
            .ToList();

            return new PendingTaxFilePaymentsResult { Payments = payments };
        }
        catch (FaultException)
        {
            return new PendingTaxFilePaymentsResult
            {
                ErrorMessage = "The service reported an error processing this request. Please try again later."
            };
        }
        catch (CommunicationException)
        {
            return new PendingTaxFilePaymentsResult
            {
                ErrorMessage = "Service is temporarily unavailable. Please try again."
            };
        }
        catch (Exception)
        {
            return new PendingTaxFilePaymentsResult
            {
                ErrorMessage = "An unexpected error occurred. Please try again."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<CancelEFTPaymentResponse> CancelEspPaymentAsync(int eftPaymentSK, int secureUserSK)
    {
        try
        {
            var paymentRequest = new UI.EmployerPortal.Generated.ServiceClients.ESPService.CancelESPPaymentRequest
            {
                EFTPaymentSK = eftPaymentSK,
                SecureUserSK = secureUserSK
            };

            return await _espService.CancelESPPaymentAsync(paymentRequest);
        }
        catch (FaultException)
        {
            return new CancelEFTPaymentResponse();
        }
        catch (CommunicationException)
        {
            return new CancelEFTPaymentResponse();
        }
    }

    /// <inheritdoc/>
    public async Task<ESPPaymentResponse> SaveEspPaymentAsync(ESPPaymentRequest eFTPaymentRequest)
    {
        try
        {
            return eFTPaymentRequest.EFTPaymentSK > 0
                ? await EditEspPaymentAsync(eFTPaymentRequest)
                : await CreateEspPaymentAsync(eFTPaymentRequest);
        }
        catch (FaultException)
        {
            return new ESPPaymentResponse
            {
                ErrorMessage = "The service reported an error processing this request. Please try again later."
            };
        }
        catch (CommunicationException)
        {
            return new ESPPaymentResponse
            {
                ErrorMessage = "Service is temporarily unavailable. Please try again."
            };
        }
    }

    /// <inheritdoc/>
    public async Task<LoadEFTPaymentResponse?> LoadEspPaymentAsync(int eftPaymentSk, int secureUserSk)
    {
        try
        {
            var request = new ObtainEESPPaymentRequest
            {
                SecureUserSK = secureUserSk,
                EFTPaymentSK = eftPaymentSk
            };

            return await _espService.LoadESPPaymentAsync(request);
        }
        catch (FaultException)
        {
            return null;
        }
        catch (CommunicationException)
        {
            return null;
        }
    }

    /// <inheritdoc/>
    public async Task<(bool Success, string? ErrorMessage)> CancelTaxFileUploadAsync(int fileUploadDetailSk)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        try
        {
            var request = new UI.EmployerPortal.Generated.ServiceClients.ESPService.CancelTaxFileUploadRequest
            {
                FileUploadDetailSK = fileUploadDetailSk,
                SecureUserSK = secureUserSk
            };

            var response = await _espService.CancelTaxFileUploadAsync(request);

            if (response?.Value == true)
            {
                return (true, null);
            }

            var errorMessage = "Unable to cancel processing for this file.";
            if (response?.RuleViolations is { Length: > 0 })
            {
                errorMessage += " " + string.Join(" ", response.RuleViolations.Select(v =>
                {
                    return $"{v.RuleViolation}.";
                }));
            }

            return (false, errorMessage);
        }
        catch (FaultException)
        {
            return (false, "The service reported an error processing this request. Please try again later.");
        }
        catch (CommunicationException)
        {
            return (false, "Service is temporarily unavailable. Please try again.");
        }
    }

    private async Task<ESPPaymentResponse> EditEspPaymentAsync(ESPPaymentRequest eFTPaymentRequest)
    {
        var paymentRequest = new EditEESPPaymentRequest
        {
            EFTPaymentSK = eFTPaymentRequest.EFTPaymentSK,
            BankAccountSK = eFTPaymentRequest.BankAccountSK,
            FileUploadDetailSK = eFTPaymentRequest.FileUploadDetailSK,
            PaymentAmount = eFTPaymentRequest.PaymentAmount,
            PaymentSettlementDate = eFTPaymentRequest.PaymentSettlementDate,
            PaymentType = eFTPaymentRequest.PaymentType,
            SecureUserSK = eFTPaymentRequest.SecureUserSK,
            SentDate = DateTime.Now
        };

        var result = await _espService.EditESPPaymentAsync(paymentRequest);

        return BuildSaveResponse(result?.ConfirmationID, result?.EFTPaymentSK, result?.RuleViolations);
    }

    private async Task<ESPPaymentResponse> CreateEspPaymentAsync(ESPPaymentRequest eFTPaymentRequest)
    {
        var paymentRequest = new CreateESPPaymentRequest
        {
            BankAccountSK = eFTPaymentRequest.BankAccountSK,
            FileUploadDetailSK = eFTPaymentRequest.FileUploadDetailSK,
            PaymentAmount = eFTPaymentRequest.PaymentAmount,
            PaymentSettlementDate = eFTPaymentRequest.PaymentSettlementDate,
            PaymentType = eFTPaymentRequest.PaymentType,
            SecureUserSK = eFTPaymentRequest.SecureUserSK,
            SentDate = DateTime.Now
        };

        var result = await _espService.CreateESPPaymentForUploadAsync(paymentRequest);

        return BuildSaveResponse(result?.ConfirmationID, result?.EFTPaymentSK, result?.RuleViolations);
    }

    private static ESPPaymentResponse BuildSaveResponse(string? confirmationId, int? eftPaymentSk, RuleViolationProxy[]? ruleViolations)
    {
        if (!string.IsNullOrWhiteSpace(confirmationId))
        {
            return new ESPPaymentResponse
            {
                ConfirmationId = confirmationId,
                EFTPaymentSK = eftPaymentSk ?? 0,
                IsAuthorized = true,
                TransactionDateTime = DateTime.Now
            };
        }

        var errorMessage = "An error occured while processing your payment.";

        if (ruleViolations is not null)
        {
            errorMessage += " " + string.Join(" ", ruleViolations.Select(v => $"{v.RuleViolation}."));
        }

        return new ESPPaymentResponse { ErrorMessage = errorMessage };
    }
}
