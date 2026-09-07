using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.ESP.Mappers;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// Service responsible for submitting Employer Service Provider FTPS registrations.
/// </summary>
public interface IEspFtpsRegistrationService
{
    /// <summary>
    /// Submits a new Employer Service Provider FTPS registration with a retry policy.
    /// </summary>
    /// <param name="model">The collected FTPS registration details.</param>
    /// <returns>The service response, including any rule violations.</returns>
    Task<CreateFileContactResponse> SaveFileContactAsync(EspFtpsRegistrationModel model);

    /// <summary>
    /// Submits an existing FTP user reference with a retry policy.
    /// </summary>
    Task<SaveFTPUserResponse> SaveFtpUserAsync();

    /// <summary>
    /// Looks up the FTP User record for the current user.
    /// </summary>
    /// <returns>An instance of <see cref="EspFtpsRegistrationModel"/> or null if there is no FTP contact for the current user.</returns>
    Task<EspFtpsRegistrationModel?> GetFtpUserForCurrentUser();
}

/// <inheritdoc />
internal class EspFtpsRegistrationService : IEspFtpsRegistrationService
{
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<EspFtpsRegistrationService> _retryPolicy;

    /// <summary>Initializes a new instance of the <see cref="EspFtpsRegistrationService"/> class.</summary>
    public EspFtpsRegistrationService(
        IPortalUtilityService portalUtilityService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<EspFtpsRegistrationService> retryPolicy)
    {
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }

    /// <inheritdoc />
    public async Task<EspFtpsRegistrationModel?> GetFtpUserForCurrentUser()
    {
        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.ObtainFileContactByTypeAsync(
                secureUserSK: _userAccountService.GetUserSKClaim(),
                contactType: (int) FileContactCode.FTP);

        });
        return response?.FileContactProxy?.ToRegistrationModel();
    }

    /// <inheritdoc />
    public async Task<CreateFileContactResponse> SaveFileContactAsync(EspFtpsRegistrationModel model)
    {
        var contract = model.ToFileContactRequest();
        contract.SecureUserSK = _userAccountService.GetUserSKClaim();
        contract.ContactTypeCodeSK = (int) FileContactCode.FTP;

        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.SaveFileContactAsync(contract);
        });
    }

    public async Task<SaveFTPUserResponse> SaveFtpUserAsync()
    {
        var contract = new SaveFtpUserRequest()
        {
            SecureUserSK = _userAccountService.GetUserSKClaim(),
            DataExchangeFilesFlag = true,
            TaxReportFilesFlag = true,
            WageReportFilesFlag = true
        };

        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.SaveFTPUserAsync(contract);
        });
    }
}

/// <summary>
/// File Contact Codes from DB table CNTC_TYP_CD
/// </summary>
internal enum FileContactCode
{
    FileUploadPrimary = 1,
    FileUploadSecondary = 2,
    FTP = 3,
    EFT = 4,
    WageNet = 5
}
