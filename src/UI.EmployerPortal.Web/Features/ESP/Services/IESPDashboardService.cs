using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Services;

/// <summary>
/// ESP Dahboard Service
/// </summary>
internal interface IESPDashboardService
{
    Task<WageUploadFileValidationResponse> ValidateWageReportUploadAsync(string fileContent, string fileName, string fileExtension);
    Task<FileUploadResponse> UploadWageReportAsync(WageFileUploadRequest request);
    Task<int?> GetEmployerSkAsync();
    Task<EmployerAccount?> GetSelectedEmployerAccountAsync();


}
