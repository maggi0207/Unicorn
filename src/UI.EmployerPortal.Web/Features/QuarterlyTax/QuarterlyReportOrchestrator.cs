
using UI.EmployerPortal.Web.Features.QuarterlyTax.Adjustments.Models;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Models;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax;

/// <summary>
/// Interface for quarterly report orchestration operations
/// </summary>
public interface IQuarterlyReportOrchestrator
{
    /// <summary>
    /// Retrieves the selected missing report from the current selected account in session storage.
    /// </summary>
    /// <returns></returns>
    Task<MissingReportModel?> GetMissingReportFromSessionAsync();

    /// <summary>
    /// Check if logged in user has associated employers
    /// </summary>
    /// <returns></returns>
    Task<bool> UserHasAssociatedEmployers();

    /// <summary>
    /// Check if logged in user has selected employer to work on.
    /// </summary>
    /// <returns></returns>
    Task<bool> UserHasSelectedEmployer();

    /// <summary>
    /// Stores the selected missing report on the current SelectedAccount in session storage.
    /// </summary>
    /// <param name="model"></param>
    /// <returns></returns>
    Task SaveMissingReportToSessionAsync(MissingReportModel model);

    /// <summary>
    /// Clears the selected missing report from session storage.
    /// </summary>
    Task ClearMissingReportFromSessionAsync();

    /// <summary>
    /// GetPendingAdjustmentReportFromSessionAsync
    /// </summary>
    /// <returns></returns>
    Task<PendingAdjustmentReportModel?> GetPendingAdjustmentReportFromSessionAsync();

    /// <summary>
    /// Stores the selected pending adjustment in session storage.
    /// </summary>
    Task SavePendingAdjustmentToSessionAsync(PendingAdjustmentReportModel model);

    /// <summary>
    /// Clears the selected pending adjustment from session storage.
    /// </summary>
    Task ClearPendingAdjustmentFromSessionAsync();


}

/// <summary>
/// Orchestrator the the quarterly tax and wage entry report. 
/// </summary>
public class QuarterlyReportOrchestrator : IQuarterlyReportOrchestrator
{
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initialize new instance of the <see cref="QuarterlyReportOrchestrator"/> class.
    /// </summary>
    /// <param name="sessionManager">The session manager for storing session data</param>
    public QuarterlyReportOrchestrator(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    /// <inheritdoc />
    public async Task SaveMissingReportToSessionAsync(MissingReportModel model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectedMissingReport = model;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    /// <inheritdoc />
    public async Task<bool> UserHasAssociatedEmployers()
    {
        var sessionAllEmployerAccounts = await _sessionManager.GetAsync<SessionAllEmployerAccounts>();
        return sessionAllEmployerAccounts != null && sessionAllEmployerAccounts?.EmployerAccounts?.Count > 0;
    }

    /// <inheritdoc />
    public async Task<bool> UserHasSelectedEmployer()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer != null;
    }

    /// <inheritdoc />
    public async Task<MissingReportModel?> GetMissingReportFromSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer?.SelectedMissingReport;
    }

    /// <inheritdoc />
    public async Task<PendingAdjustmentReportModel?> GetPendingAdjustmentReportFromSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedEmployer?.SelectedPendingAdjustment;
    }

    /// <inheritdoc />
    public async Task SavePendingAdjustmentToSessionAsync(PendingAdjustmentReportModel model)
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectedPendingAdjustment = model;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    /// <inheritdoc />
    public async Task ClearPendingAdjustmentFromSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectedPendingAdjustment = null;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }

    /// <inheritdoc />
    public async Task ClearMissingReportFromSessionAsync()
    {
        var selectedEmployer = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        if (selectedEmployer != null)
        {
            selectedEmployer.SelectedMissingReport = null;
            await _sessionManager.SetAsync(selectedEmployer);
        }
    }
}
