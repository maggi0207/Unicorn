using System.Collections.Concurrent;
using UI.EmployerPortal.Generated.ServiceClients.UploadService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Services;
/// <summary>
///
/// </summary>
internal interface IUploadServices
{
    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    Task<List<FileUploadService>> LoadFileWcf(string fullpath, string confnum, int surveyResponsesk);

}
internal class UploadServices : IUploadServices
{
    private readonly IAsyncRetryPolicy<UserAccountService> _retryPolicy;
    private readonly IUserAccountService _userAccountService;
    private readonly IUploadService _fileService;

    public UploadServices(
        IAsyncRetryPolicy<UserAccountService> retryPolicy,
        IUserAccountService userAccountService,
        IUploadService uploadService)
    {

        _retryPolicy = retryPolicy;
        _userAccountService = userAccountService;
        _fileService = uploadService;
    }

    public async Task<List<FileUploadService>> LoadFileWcf(string fullpath, string confnum, int surveyResponsesk)
    {
        var assosiatedfileload = new ConcurrentBag<FileUploadService>();
        var loadfile = await _retryPolicy.ExecuteAsync(() =>
        {
            return _fileService.UploadWebRegistrationFileAsync(
                new WebUserFileUploadProxy()
                {
                    SecureUserSK = _userAccountService.GetUserSKClaim(),
                    WebUserFileStatusCodeSK = 2,//read only varable top of file
                    WebUserFileTypeCodeSK = 3,//read only varable 
                    CloudFilePath = fullpath, //app setting
                    ConfirmationNumber = confnum,
                    SurveyResponseSK = surveyResponsesk,
                    CommonClientSK = null,
                    UploadDate = DateTime.Now,
                });
        });
        if (loadfile == null || loadfile.WebUserFileUploadSK == 0)
        {
            if (loadfile != null && loadfile.RuleViolations != null)
            {

                await Parallel.ForEachAsync(
                    source: loadfile.RuleViolations,
                    async (item, token) =>
                    {
                        assosiatedfileload.Add(new FileUploadService()
                        {
                            RuleId = item.RuleID,
                            RuleViolation = item.RuleViolation,
                        });
                    });
            }
        }
        else
        {
            assosiatedfileload.Add(new FileUploadService()
            {
                EmployerSK = loadfile.EmployerSK.ToString(),
                WebUserFileUploadSK = loadfile.WebUserFileUploadSK.ToString(),
                SurveyNumber = loadfile.SurveyNumber?.ToString() ?? string.Empty
            });
        }

        return [.. assosiatedfileload];
    }
}

