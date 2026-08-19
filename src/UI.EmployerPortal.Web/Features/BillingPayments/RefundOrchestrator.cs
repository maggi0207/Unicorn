using System.ServiceModel;
using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.BillingPayments;

/// <summary>
/// Orchestrates the Request a Refund flow: eligibility, prefilled payee/address details,
/// and submission of the refund request.
/// </summary>
public interface IRefundOrchestrator
{
    /// <summary>
    /// Returns whether the currently selected employer is eligible to request a refund.
    /// </summary>
    Task<bool> IsEligibleForRefundAsync();

    /// <summary>
    /// Builds a refund request model pre-populated with the employer's payee name,
    /// mailing address, and the given refund amount.
    /// </summary>
    Task<RefundRequestModel> BuildRefundRequestAsync(decimal refundAmount);

    /// <summary>
    /// Submits the refund request and returns the result.
    /// </summary>
    Task<RefundSubmissionResult> SubmitRefundRequestAsync(RefundRequestModel model);

    /// <summary>
    /// Stores the refund request in session so it can be read by the confirmation page.
    /// </summary>
    Task SaveRefundRequestToSessionAsync(RefundRequestModel model);

    /// <summary>
    /// Retrieves the refund request previously stored in session.
    /// </summary>
    Task<RefundRequestModel?> GetRefundRequestFromSessionAsync();
}

/// <summary>
/// Default implementation of <see cref="IRefundOrchestrator"/>.
/// </summary>
internal class RefundOrchestrator : IRefundOrchestrator
{
    private readonly ISessionManager _sessionManager;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<RefundOrchestrator> _retryPolicy;

    /// <summary>
    /// Initializes a new instance of <see cref="RefundOrchestrator"/>.
    /// </summary>
    public RefundOrchestrator(
        ISessionManager sessionManager,
        IAccountMaintenanceService accountMaintenanceService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<RefundOrchestrator> retryPolicy)
    {
        _sessionManager = sessionManager;
        _accountMaintenanceService = accountMaintenanceService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc/>
    public async Task<bool> IsEligibleForRefundAsync()
    {
        // Every employer is shown the Request a Refund option; RequestRefundAsync enforces
        // eligibility server-side and surfaces a rule violation on submit if not eligible.
        await Task.CompletedTask;
        return true;
    }

    /// <inheritdoc/>
    public async Task<RefundRequestModel> BuildRefundRequestAsync(decimal refundAmount)
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var employer = selected?.EmployerAccount;

        return new RefundRequestModel
        {
            RefundAmount = refundAmount,
            PayeeName = employer?.LegalName ?? string.Empty,
            RefundAddress = BuildAddress(employer)
        };
    }

    /// <inheritdoc/>
    public async Task<RefundSubmissionResult> SubmitRefundRequestAsync(RefundRequestModel model)
    {
        var employerSk = await GetEmployerSkAsync();
        if (employerSk is null)
        {
            return new RefundSubmissionResult(false, "No employer account selected");
        }

        var secureUserSk = _userAccountService.GetUserSKClaim();

        try
        {
            var response = await _retryPolicy.ExecuteAsync(() =>
            {
                return _accountMaintenanceService.RequestRefundAsync(
                    employerSk.Value, model.EmailAddress, model.AdditionalInformation ?? string.Empty, secureUserSk);
            });

            return response?.RuleViolations is { Length: > 0 }
                ? new RefundSubmissionResult(false, response.RuleViolations[0].RuleViolation)
                : new RefundSubmissionResult(true, null);
        }
        catch (CommunicationException)
        {
            return new RefundSubmissionResult(false, "Service is temporarily unavailable. Please try again.");
        }
        catch (Exception)
        {
            return new RefundSubmissionResult(false, "An unexpected error occurred. Please try again.");
        }
    }

    /// <inheritdoc/>
    public async Task SaveRefundRequestToSessionAsync(RefundRequestModel model)
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selected != null)
        {
            selected.SelectedRefundRequest = model;
            await _sessionManager.SetAsync(selected);
        }
    }

    /// <inheritdoc/>
    public async Task<RefundRequestModel?> GetRefundRequestFromSessionAsync()
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selected?.SelectedRefundRequest;
    }

    private async Task<int?> GetEmployerSkAsync()
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selected?.EmployerAccount?.Id;
    }

    private static string BuildAddress(EmployerAccount? employer)
    {
        if (employer is null)
        {
            return string.Empty;
        }

        var lines = new List<string>();
        if (!string.IsNullOrWhiteSpace(employer.AddressLine1))
        {
            lines.Add(employer.AddressLine1);
        }
        if (!string.IsNullOrWhiteSpace(employer.AddressLine2))
        {
            lines.Add(employer.AddressLine2);
        }

        var cityState = string.Join(" ", new[] { employer.City, employer.State }.Where(s => !string.IsNullOrWhiteSpace(s)));
        var cityStateZip = cityState;
        if (!string.IsNullOrWhiteSpace(employer.Zip))
        {
            cityStateZip = string.IsNullOrWhiteSpace(cityState) ? employer.Zip : $"{cityState} {employer.Zip}";
        }

        if (!string.IsNullOrWhiteSpace(cityStateZip))
        {
            lines.Add(cityStateZip);
        }

        return string.Join("\n", lines);
    }
}
