using System.Collections.Concurrent;
using UI.EmployerPortal.Generated.ServiceClients.UploadService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.FileUpload.Models;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.Shared.FileUpload.Services;

/// <summary>
/// Defines the contract for uploading files via the WCF upload service.
/// </summary>
internal interface IUploadServices
{
    /// <summary>
    /// Uploads the file at the specified path via the WCF service and returns
    /// a list of <see cref="FileUploadService"/> results, including any rule violations.
    /// </summary>
    /// <param name="fullpath">Cloud file path of the file to upload.</param>
    /// <returns>List of upload results; multiple entries indicate rule violations.</returns>
    Task<List<FileUploadService>> LoadFileWcf(string fullpath);
}

/// <summary>
/// Concrete implementation of <see cref="IUploadServices"/> that calls the
/// WCF <c>UploadWebRegistrationFileAsync</c> endpoint and maps the response
/// to <see cref="FileUploadService"/> models.
/// </summary>
internal class UploadServices : IUploadServices
{
    private readonly IAsyncRetryPolicy<UserAccountService> _retryPolicy;
    private readonly IUserAccountService _userAccountService;
    private readonly IUploadService _fileService;

    /// <summary>
    /// Initializes a new instance of <see cref="UploadServices"/> with the
    /// required retry policy, user account service, and WCF upload client.
    /// </summary>
    public UploadServices(
        IAsyncRetryPolicy<UserAccountService> retryPolicy,
        IUserAccountService userAccountService,
        IUploadService uploadService)
    {
        _retryPolicy = retryPolicy;
        _userAccountService = userAccountService;
        _fileService = uploadService;
    }

    /// <inheritdoc />
    public async Task<List<FileUploadService>> LoadFileWcf(string fullpath)
    {
        var assosiatedfileload = new ConcurrentBag<FileUploadService>();
        var loadfile = await _retryPolicy.ExecuteAsync(() =>
        {
            return _fileService.UploadWebRegistrationFileAsync(
                new WebUserFileUploadProxy()
                {
                    SecureUserSK = _userAccountService.GetUserSKClaim(),
                    WebUserFileStatusCodeSK = 2,
                    WebUserFileTypeCodeSK = 3,
                    CloudFilePath = fullpath,
                    ConfirmationNumber = "7174167",
                    UploadDate = DateTime.Now
                });
        });

        if (loadfile == null || loadfile.EmployerSK == 0 || loadfile.WebUserFileUploadSK == 0)
        {
            return [.. assosiatedfileload];
        }
        else
        {
            assosiatedfileload.Add(new FileUploadService()
            {
                EmployerSK = loadfile.EmployerSK.ToString(),
                WebUserFileUploadSK = loadfile.WebUserFileUploadSK.ToString(),
                SurveyNumber = loadfile.SurveyNumber.ToString()
            });
        }

        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(60));

        await Parallel.ForEachAsync(
            source: loadfile.RuleViolations,
            parallelOptions: new ParallelOptions
            {
                MaxDegreeOfParallelism = 32,
                CancellationToken = cts.Token
            },
            async (item, token) =>
            {
                assosiatedfileload.Add(new FileUploadService()
                {
                    RuleId = item.RuleID,
                    RuleViolation = item.RuleViolation,
                });
            });

        return [.. assosiatedfileload];
    }
}
