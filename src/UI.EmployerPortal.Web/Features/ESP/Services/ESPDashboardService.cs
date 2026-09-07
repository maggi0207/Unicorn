using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

internal class ESPDashboardService : IESPDashboardService
{
    private readonly IESPService _espService;
    private readonly ISessionManager _sessionManager;

    public ESPDashboardService(IESPService espService, ISessionManager sessionManager)
    {
        _espService = espService;
        _sessionManager = sessionManager;

    }

    public async Task<WageUploadFileValidationResponse> ValidateWageReportUploadAsync(string fileContent, string fileName, string fileExtension)
    {
        var validationResponse = await _espService.ValidateWageReportUploadAsync(fileContent, fileName, fileExtension);
        return validationResponse;
    }

    public async Task<FileUploadResponse> UploadWageReportAsync(WageFileUploadRequest request)
    {
        var fileUploadResponse = await _espService.UploadWageReportAsync(request);
        return fileUploadResponse;
    }

    public async Task<int?> GetEmployerSkAsync()
    {
        var selected = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selected?.EmployerAccount?.Id;
    }

    public async Task<EmployerAccount?> GetSelectedEmployerAccountAsync()
    {
        var selectedAccount = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedAccount?.EmployerAccount;
    }

}
