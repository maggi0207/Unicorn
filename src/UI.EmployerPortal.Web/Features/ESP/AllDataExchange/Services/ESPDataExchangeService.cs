
using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ESP.AllDataExchange.Services;

/// <summary>
/// Feature service for Data Exchange Files Uploaded also download,
/// <see cref="IESPService"/> WCF client.
/// </summary>
public interface IESPDataExchangeService
{

    /// <summary>
    /// Retrieves the list of files uploaded associated with the current user.
    /// </summary>
    Task<List<DataExchangeResponseModel>> GetDataExchangeResponseFilesAsync();
    /// <summary>
    /// Download files uploaded associated with the current user.
    /// </summary>
    Task<DownloadESPDXFileResponse> DownloadESPDXFileAsync(DownloadESPDXFileRequestModel request);


}

/// <inheritdoc />
internal class ESPDataExchangeService : IESPDataExchangeService
{
    private readonly IESPService _espService;
    private readonly IUserAccountService _userAccountService;
    private readonly IAsyncRetryPolicy<ESPDataExchangeService> _retryPolicy;

    /// <summary>Initializes a new instance of the <see cref="ESPDataExchangeService"/> class.</summary>
    public ESPDataExchangeService(
        IESPService espService,
        IUserAccountService userAccountService,
        IAsyncRetryPolicy<ESPDataExchangeService> retryPolicy)
    {
        _espService = espService;
        _userAccountService = userAccountService;
        _retryPolicy = retryPolicy;
    }


    /// <inheritdoc />
    public async Task<List<DataExchangeResponseModel>> GetDataExchangeResponseFilesAsync()
    {

        var secureUserSk = _userAccountService.GetUserSKClaim();

        var request = new ObtainFileUploadByUserRequest
        {
            SecureUserSk = secureUserSk
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.LoadESPDXResponseDataAsync(request);
        });


        var result = new List<DataExchangeResponseModel>(response.Files.Length);

        foreach (var file in response.Files)
        {
            result.Add(new DataExchangeResponseModel
            {
                RequestFileName = file.FormattedFileName ?? string.Empty,
                UploadDateTime = file.UploadDate ?? DateTime.MinValue,
                RecordCount = file.FileRecordCount ?? 0,
                ConfirmationID = file.ConfirmationID ?? string.Empty,
                ResponseFileName = file.FormattedFileName ?? string.Empty,
                FileUploadSK = file.FileUploadDetailSK ?? 0
            });
        }

        return result;
    }

    public async Task<DownloadESPDXFileResponse> DownloadESPDXFileAsync(DownloadESPDXFileRequestModel request)
    {
        var secureUserSk = _userAccountService.GetUserSKClaim();

        var req = new DownloaadESPDXFileRequest
        {
            FileUploadDetailSK = request.FileUploadDetailSK,
            SecureUserSK = secureUserSk

        };
        var response = await _espService.DownloadESPDXFileAsync(req);
        return response;
    }

}
