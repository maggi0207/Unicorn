using UI.EmployerPortal.Generated.ServiceClients.AccountMaintenanceService;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Services;

/// <summary>
/// Provides integration with the AccountMaintenanceService WCF proxy
/// for fetching and updating employer account details.
/// Uses <see cref="IAccountMaintenanceService.UpdateEmployerInformationAsync"/>
/// to persist information changes.
/// </summary>
internal class AccountDetailsService : IAccountDetailsService
{
    private readonly IAsyncRetryPolicy<AccountDetailsService> _retryPolicy;
    private readonly IAccountMaintenanceService _accountMaintenanceService;
    private readonly IUserAccountService _userAccountService;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountDetailsService"/> class.
    /// </summary>
    /// <param name="retryPolicy">The retry policy for resilient WCF calls.</param>
    /// <param name="accountMaintenanceService">The WCF proxy for account maintenance operations.</param>
    /// <param name="userAccountService">Provides the authenticated user's secure user SK.</param>
    public AccountDetailsService(
        IAsyncRetryPolicy<AccountDetailsService> retryPolicy,
        IAccountMaintenanceService accountMaintenanceService,
        IUserAccountService userAccountService)
    {
        _retryPolicy = retryPolicy;
        _accountMaintenanceService = accountMaintenanceService;
        _userAccountService = userAccountService;
    }

    /// <summary>
    /// Retrieves the current account details for the specified employer
    /// by calling <see cref="IAccountMaintenanceService.GetPortalEmployerProxyAsync"/>.
    /// </summary>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A populated <see cref="AccountDetailsModel"/>.</returns>
    public async Task<AccountDetailsModel> GetAccountDetailsAsync(int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.GetPortalEmployerProxyAsync(employerSK, secureUserSK);
        });

        if (response?.EmployerProxy == null)
        {
            return new AccountDetailsModel();
        }

        var employer = response.EmployerProxy;

        var rawPhone = employer.PhoneNumber ?? string.Empty;
        var phoneSplit = rawPhone.Split(new[] { 'x', 'X' }, 2);
        var phoneStr = phoneSplit[0];
        var extStr = phoneSplit.Length > 1 ? phoneSplit[1] : string.Empty;

        var phoneDigits = new string(phoneStr.Where(char.IsDigit).ToArray());
        if (phoneDigits.Length > 10) phoneDigits = phoneDigits[..10];

        var formattedPhone = phoneDigits.Length switch
        {
            > 6 => $"{phoneDigits[..3]}-{phoneDigits[3..6]}-{phoneDigits[6..]}",
            > 3 => $"{phoneDigits[..3]}-{phoneDigits[3..]}",
            _ => phoneDigits
        };

        var extDigits = new string(extStr.Where(char.IsDigit).ToArray());

        var feinStr = employer.FEIN ?? string.Empty;
        var feinDigits = new string(feinStr.Where(char.IsDigit).ToArray());
        if (feinDigits.Length > 9) feinDigits = feinDigits[..9];
        var formattedFein = feinDigits.Length > 2 ? $"{feinDigits[..2]}-{feinDigits[2..]}" : feinDigits;

        return new AccountDetailsModel
        {
            FEIN = formattedFein,
            LegalName = employer.LegalName ?? string.Empty,
            TradeName = employer.TradeName,
            PhoneNumber = formattedPhone,
            Extension = extDigits,
            CountryCode = string.IsNullOrWhiteSpace(employer.InternationalPhoneCode) ? null : $"+{employer.InternationalPhoneCode.TrimStart('+')}",
            EmailAddress = string.Empty
        };
    }

    /// <summary>
    /// Updates the employer information by mapping the <see cref="AccountDetailsModel"/>
    /// to an <see cref="EmployerUpdate"/> and calling
    /// <see cref="IAccountMaintenanceService.UpdateEmployerInformationAsync"/>.
    /// </summary>
    /// <param name="model">The model containing the updated account information.</param>
    /// <param name="employerSK">The surrogate key of the employer.</param>
    /// <returns>A tuple indicating success and any error messages from rule violations.</returns>
    public async Task<(bool success, string error)> UpdateAccountDetailsAsync(AccountDetailsModel model, int employerSK)
    {
        var secureUserSK = _userAccountService.GetUserSKClaim();

        // The EmployerUpdate WCF object requires PhoneAreaCode and PhoneLocalNumber
        // as separate fields. Strip all non-digits from the formatted phone number
        // (e.g. "(675) 555-5555" → "6755555555") then split: first 3 = area code,
        // remaining 7 = local number. This is the same pattern used in ContactInformationService.
        var phoneDigits = new string((model.PhoneNumber ?? "").Where(char.IsDigit).ToArray());
        var phoneAreaCode    = phoneDigits.Length == 10 ? phoneDigits[..3]            : string.Empty;
        var phoneLocalNumber = phoneDigits.Length == 10 ? phoneDigits.Substring(3, 7) : phoneDigits;

        var request = new EmployerInformationUpdateRequest
        {
            SecureUserSK = secureUserSK,
            EmployerUpdate = new EmployerUpdate
            {
                EmployerSK = employerSK,
                FEIN = model.FEIN,
                LegalName = model.LegalName,
                TradeName = string.IsNullOrWhiteSpace(model.TradeName) ? null : model.TradeName,
                EmailAddress = model.EmailAddress,
                PhoneAreaCode = phoneAreaCode,
                PhoneLocalNumber = phoneLocalNumber,
                PhoneExtension = string.IsNullOrWhiteSpace(model.Extension) ? null : model.Extension,
                PhoneInternationalCode = string.IsNullOrWhiteSpace(model.CountryCode) ? null : model.CountryCode.TrimStart('+'),
                FeinChangeReasonCodeSK = int.TryParse(model.ReasonForFeinChange, out var feinReasonSK) ? feinReasonSK : null,
                FeinChangeReasonExplanation = string.IsNullOrWhiteSpace(model.FeinChangeReasonExplanation) ? null : model.FeinChangeReasonExplanation,
                LegalNameChangeReasonCodeSK = int.TryParse(model.ReasonForLegalNameChange, out var legalReasonSK) ? legalReasonSK : null,
                LegalNameChangeExplanation = string.IsNullOrWhiteSpace(model.LegalNameChangeExplanation) ? null : model.LegalNameChangeExplanation
            }
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _accountMaintenanceService.UpdateEmployerInformationAsync(request);
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
}
