using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;
using FileUploadResponse = UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService.FileUploadResponse;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;
using WageUploadFileValidationResponse = UI.EmployerPortal.Generated.ServiceClients.TaxWageReportingService.WageUploadFileValidationResponse;

namespace UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

/// <summary>
/// Inteface for wage file upload service
/// </summary>
public interface IWageFileUploadService
{
    /// <summary>
    /// Validate wage upload file.
    /// </summary>
    /// <param name="fileContent"></param>
    /// <param name="fileName"></param>
    /// <param name="fileExtension"></param>
    /// <returns></returns>
    Task<WageUploadFileValidationResponse> ValidateWageUploadFile(string fileContent, string fileName, string fileExtension);

    /// <summary>
    /// Submit an original wage report file
    /// </summary>
    /// <param name="filePath"></param>
    /// <param name="wageFileStatusCode"></param>
    /// <param name="wageFileTypeCode"></param>
    /// <param name="isTestEnv"></param>
    /// <returns></returns>
    Task<FileUploadResponse> SubmitWageFileUploadAsync(string filePath, WageFileStatusCode wageFileStatusCode, WageFileTypeCode wageFileTypeCode, bool isTestEnv);
}

/// <inheritdoc />
internal class WageFileUploadService : IWageFileUploadService
{
    private readonly ITaxWageReportingService _taxWageReportingService;
    private readonly IPortalUtilityService _portalUtilityService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<WageFileUploadService> _retryPolicy;
    private readonly ISessionManager _sessionManager;

    public WageFileUploadService(
           ITaxWageReportingService taxWageReportingService,
           IPortalUtilityService portalUtilityService,
           IUserAccountService userAccountService,
           IAsyncRetryPolicy<WageFileUploadService> retryPolicy,
           ISessionManager sessionManager)
    {
        _taxWageReportingService = taxWageReportingService;
        _portalUtilityService = portalUtilityService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
        _sessionManager = sessionManager;
    }

    public async Task<WageUploadFileValidationResponse> ValidateWageUploadFile(string fileContent, string fileName, string fileExtension)
    {
        return await _retryPolicy.ExecuteAsync(() =>
        {
            return _taxWageReportingService.ValidateWageUtilityFileAsync(fileContent, fileName, fileExtension);
        });
    }

    public async Task<FileUploadResponse> SubmitWageFileUploadAsync(
        string filePath,
        WageFileStatusCode wageFileStatusCode,
        WageFileTypeCode wageFileTypeCode, bool isTestEnv)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        var request = new WageFileUploadRequest()
        {
            FilePath = filePath,
            CommonClientSK = selectedEmployer!.EmployerAccount!.Id,
            SecureUserSk = _userAccountService.GetUserSKClaim(),
            WebUserFileStatusCodeSK = (int) wageFileStatusCode,
            WebUserFileTypeCodeSK = (int) wageFileTypeCode,
            IsTestFile = isTestEnv
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _portalUtilityService.CreateWageFileUploadAsync(request);
        });

        return response;
    }
}
