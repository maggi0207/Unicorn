using UI.EmployerPortal.Generated.ServiceClients.ESPService;
using UI.EmployerPortal.Web.Features.Shared.Layout;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Startup.ResiliencyProtocols;

namespace UI.EmployerPortal.Web.Features.ESP.Services;


/// <summary>
/// Service interface for submitting and validating data exchange file uploads via ESPService.
/// </summary>
public interface IDataExchangeFileUploadService
{

    /// <summary>
    /// Submits a data exchange file for processing.
    /// </summary>
    Task<DataExchangeTaxFileSubmitResult> SubmitDataExchangeFileUploadAsync(
        string filePath,
        int secureUserSK,
        TaxFileUploadStatusCode taxFileStatusCode,
        int recordCount,
        bool isTestEnv = false);
}

/// <summary>
/// // <summary>
/// Result returned by SubmitDataExchangeTaxFileUploadAsync.
/// </summary>
/// </summary>
public class DataExchangeTaxFileSubmitResult
{
    /// <summary>
    /// 
    /// </summary>
    public bool Success { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public string? ConfirmationNumber { get; set; }
    /// <summary>
    /// 
    /// </summary>
    public RuleViolationProxy[]? RuleViolations { get; set; }
}

/// <summary>
/// For Data Exchange File Upload confirmation page
/// </summary>
public class DataExchangeConfirmationResult
{
    /// <summary>
    /// Success message
    /// </summary>
    public bool Success { get; set; }

    /// <summary>
    /// Confirmation number
    /// </summary>
    public string ConfirmationNumber { get; set; } = string.Empty;

    /// <summary>
    /// File Name
    /// </summary>
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// Upload Date/Time
    /// </summary>
    public DateTime? UploadDate { get; set; }

    /// <summary>
    /// Record Count
    /// </summary>
    public int RecordCount { get; set; }

}


/// <summary>
/// Implementation of ITaxFileUploadService.
/// Sources the employer identifier from either the regular employer-dashboard session
/// (SelectedEmployerAccount) or the ESP client-selection service (IESPDashboardService),
/// depending on whether the current use. 
/// </summary>
internal class DataExchangeFileUploadService : IDataExchangeFileUploadService
{
    private readonly IESPService _espService;
    private readonly IAsyncRetryPolicy<DataExchangeFileUploadService> _retryPolicy;
    private readonly ISessionManager _sessionManager;
    private readonly ILayoutOrchestator _layoutOrchestator;
    private readonly IESPDashboardService _espDashboardService;

    public DataExchangeFileUploadService(
        IESPService espService,
        IAsyncRetryPolicy<DataExchangeFileUploadService> retryPolicy,
        ISessionManager sessionManager,
        ILayoutOrchestator layoutOrchestator,
        IESPDashboardService espDashboardService)
    {
        _espService = espService;
        _retryPolicy = retryPolicy;
        _sessionManager = sessionManager;
        _layoutOrchestator = layoutOrchestator;
        _espDashboardService = espDashboardService;
    }

    /// <inheritdoc />
    public async Task<DataExchangeTaxFileSubmitResult> SubmitDataExchangeFileUploadAsync(
        string filePath,
        int secureUserSK,
        TaxFileUploadStatusCode taxFileStatusCode,
        int recordCount,
        bool isTestEnv = false)
    {
        var request = new ESPDXFileRequest
        {
            FilePath = filePath,
            SecureUserSk = secureUserSK,
            WebUserFileStatusCodeSK = (int) taxFileStatusCode,
            WebUserFileTypeCodeSK = 1,
            ContentLength = recordCount,
            IsTestFile = isTestEnv
        };

        var response = await _retryPolicy.ExecuteAsync(() =>
        {
            return _espService.UploadESPDXFileAsync(request);
        });

        return new DataExchangeTaxFileSubmitResult
        {
            Success = response?.RuleViolations.Length == 0,
            ConfirmationNumber = response?.ConfirmationNumber ?? string.Empty,
            RuleViolations = response?.RuleViolations
        };
    }

}
