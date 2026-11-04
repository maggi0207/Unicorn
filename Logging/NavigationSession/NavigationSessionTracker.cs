using System.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Options;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Logging.NavigationSession;

internal class NavigationSessionTracker : INavigationSessionTracker
{
    private const string ApplicationName = "Employer Portal";
    private static readonly string Platform = Environment.OSVersion.VersionString;
    private static readonly string ServerName = Environment.MachineName;
    private static readonly NavigationSessionSk InvalidNavigationSessionSk = new(-1);
    private readonly NavigationSessionOptions _options;
    private readonly string _connectionString;
    private readonly ILogger<NavigationSessionTracker> _logger;
    private readonly ISessionManager _sessionManager;
    private readonly IUserAccountService _userAccountService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public NavigationSessionTracker(IOptions<NavigationSessionOptions> options
        , ISessionManager sessionManager
        , IUserAccountService userAccountService
        , IHttpContextAccessor httpContextAccessor
        , ILogger<NavigationSessionTracker> logger)
    {
        _options = options.Value;
        _connectionString = _options.ConnectionString;
        _sessionManager = sessionManager;
        _userAccountService = userAccountService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    private async Task<bool> HasExistingNavigationSessionAsync()
    {
        var navigationSessionSk = await GetNavigationSessionSkAsync();
        return navigationSessionSk is not null && navigationSessionSk.Value > 0;
    }

    public async Task<bool> StartNavigationSessionIfRequiredAsync(DateTime startedAt)
    {
        if (await HasExistingNavigationSessionAsync())
        {
            return false;
        }
        _logger.LogNavigationSessionTrackerVersion("REAL");

        var userAgent = ResolveUserAgent();
        var ipAddress = ResolveIpAddress();
        var secureUserSk = _userAccountService.GetUserSKClaim();
        var username = _userAccountService.GetUserID() ?? string.Empty;
        var sessionId = ResolveSessionId();
        var request = new NavigationSessionRequest
        {
            IpAddress = ipAddress,
            UserAgent = userAgent,
            SecureUserSk = secureUserSk,
            UserName = username,
            StartedAt = startedAt,
            SessionId = sessionId
        };

        var navigationSessionSk = await StartNewNavigationSessionAsync(request);
        await SetNavigationSessionSkAsync(navigationSessionSk);
        return true;
    }

    public async Task LogNavigationSessionActivity(Func<Task> action, string actionDescription, DateTime occuredAt)
    {
        var pageType = await GetCurrentPageTypeAsync() ?? PageType.GuestAccessDashboard;
        await LogNavigationSessionActivity(pageType, actionDescription, occuredAt);
        await action();
    }

    public async Task LogNavigationSessionActivity(Action action, string actionDescription, DateTime occuredAt)
    {
        var pageType = await GetCurrentPageTypeAsync() ?? PageType.GuestAccessDashboard;
        await LogNavigationSessionActivity(pageType, actionDescription, occuredAt);
        action();
    }

    public async Task<EventCallback> LogNavigationSessionActivity(EventCallback callback, string actionDescription, DateTime occuredAt)
    {
        var pageType = await GetCurrentPageTypeAsync() ?? PageType.GuestAccessDashboard;
        await LogNavigationSessionActivity(pageType, actionDescription, occuredAt);
        await callback.InvokeAsync();
        return callback;
    }

    public async Task LogNavigationSessionActivity(PageType pageType, string actionDescription, DateTime occuredAt)
    {
        await SetCurrentPageTypeAsync(pageType);
        var username = _userAccountService.GetUserID() ?? string.Empty;
        var employerSk = await GetSelectedEmployerSk();
        var navigationSessionSk = await GetNavigationSessionSkAsync();
        var activity = new NavigationSessionActivity
        {
            ActionDescription = actionDescription,
            OccurredAt = occuredAt,
            PageTypeSk = pageType,
            UserName = username,
            EmployerSk = employerSk,
            NavigationSessionSk = navigationSessionSk
        };

        await LogNavigationSessionActivity(activity);
    }

    private async Task SetCurrentPageTypeAsync(PageType pageType)
    {
        await _sessionManager.SetAsync(pageType);
    }

    private async Task<PageType?> GetCurrentPageTypeAsync()
    {
        return await _sessionManager.GetAsync<PageType>();
    }

    private async Task LogNavigationSessionActivity(NavigationSessionActivity activity)
    {
        if (activity.NavigationSessionSk == InvalidNavigationSessionSk)
        {
            _logger.LogNavigationSessionActivityNotLogged(activity.PageTypeSk, activity.ActionDescription);
            return;
        }

        try
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandType = CommandType.StoredProcedure;

            command.CommandText = "STESCHEMA.NAVIGATION_LOG.NL_SAVE";

            var logSkParameter = new OracleParameter("vNVGT_LOG_SK", OracleDbType.Int64)
            {
                Direction = ParameterDirection.InputOutput,
                Value = DBNull.Value
            };
            command.Parameters.Add(logSkParameter);

            var updateTimestampParameter = new OracleParameter("vUPDT_TMS", OracleDbType.TimeStamp)
            {
                Direction = ParameterDirection.InputOutput,
                Value = activity.OccurredAt
            };
            command.Parameters.Add(updateTimestampParameter);

            command.Parameters.Add
            (
                "vWEB_PAGE_CD_SK",
                OracleDbType.Int64,
                activity.PageTypeSk.Value,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vNVGT_SES_SK",
                OracleDbType.Int64,
                activity.NavigationSessionSk.Value,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vACTN_DSC",
                OracleDbType.Varchar2,
                activity.ActionDescription,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCRE_TMS",
                OracleDbType.TimeStamp,
                activity.OccurredAt,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vUPDT_PROC_NAM",
                OracleDbType.Varchar2,
                ApplicationName,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vUPDT_USER_NAM",
                OracleDbType.Varchar2,
                activity.UserName,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vACTV_YR",
                OracleDbType.Int64,
                activity.ActivityYear,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vACTV_QTR_NUM",
                OracleDbType.Int64,
                activity.ActivityQuarter,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCMN_CLNT_SK",
                OracleDbType.Int64,
                activity.EmployerSk,
                ParameterDirection.Input
            );

            await command.ExecuteNonQueryAsync();
        }
        catch (Exception e)
        {
            _logger.LogNavigationSessionDatabaseException(e);
        }
    }

    private async Task<NavigationSessionSk> StartNewNavigationSessionAsync(NavigationSessionRequest request)
    {
        try
        {
            await using var connection = new OracleConnection(_connectionString);
            await connection.OpenAsync();
            await using var command = connection.CreateCommand();
            command.BindByName = true;
            command.CommandType = CommandType.StoredProcedure;

            command.CommandText = "STESCHEMA.NAVIGATION_SESSION.NS_SAVE";

            var sessionSkParameter = new OracleParameter("vNVGT_SES_SK", OracleDbType.Int64)
            {
                Direction = ParameterDirection.InputOutput,
                Value = DBNull.Value
            };
            command.Parameters.Add(sessionSkParameter);

            var updateTimestampParameter = new OracleParameter("vUPDT_TMS", OracleDbType.TimeStamp)
            {
                Direction = ParameterDirection.InputOutput,
                Value = request.StartedAt
            };
            command.Parameters.Add(updateTimestampParameter);

            command.Parameters.Add
            (
                "vSES_ID",
                OracleDbType.Varchar2,
                request.SessionId,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vINET_PRTCL_ADR",
                OracleDbType.Varchar2,
                request.IpAddress,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCLNT_BRWSR_NAM",
                OracleDbType.Varchar2,
                string.Empty,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCLNT_BRWSR_VER_DSC",
                OracleDbType.Varchar2,
                string.Empty,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vAD_USER_NAM",
                OracleDbType.Varchar2,
                request.UserName,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vWEB_SRVR_NAM",
                OracleDbType.Varchar2,
                ServerName,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCRE_TMS",
                OracleDbType.TimeStamp,
                request.StartedAt,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vPLTFM_DSC",
                OracleDbType.Varchar2,
                Platform,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vFPRNT_KEY_TXT",
                OracleDbType.Varchar2,
                string.Empty,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vCMN_CLNT_SECR_USER_SK",
                OracleDbType.Int64,
                request.SecureUserSk,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vUSER_AGNT_TXT",
                OracleDbType.Varchar2,
                request.UserAgent,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vUPDT_PROC_NAM",
                OracleDbType.Varchar2,
                ApplicationName,
                ParameterDirection.Input
            );

            command.Parameters.Add
            (
                "vUPDT_USER_NAM",
                OracleDbType.Varchar2,
                request.UserName,
                ParameterDirection.Input
            );

            await command.ExecuteNonQueryAsync();

            var navigationSessionSk = new NavigationSessionSk(((OracleDecimal) sessionSkParameter.Value).ToInt64());
            return navigationSessionSk;
        }
        catch (Exception e)
        {
            _logger.LogNavigationSessionDatabaseException(e);
            return InvalidNavigationSessionSk;
        }
    }

    private async Task<int?> GetSelectedEmployerSk()
    {
        var selectedAccount = await _sessionManager.GetAsync<SelectedEmployerAccount>();
        return selectedAccount?.EmployerAccount?.Id;
    }

    private async Task<NavigationSessionSk> GetNavigationSessionSkAsync()
    {
        var navigationSessionSk = await _sessionManager.GetAsync<NavigationSessionSk>();
        return navigationSessionSk ?? InvalidNavigationSessionSk;
    }

    private async Task SetNavigationSessionSkAsync(NavigationSessionSk navigationSessionSk)
    {
        await _sessionManager.ClearAsync<NavigationSessionSk>();
        await _sessionManager.SetAsync(navigationSessionSk);
    }

    private string ResolveIpAddress()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        return (httpContext is not null)
            ? httpContext.Request
                .Headers["X-Forwarded-For"]
                .FirstOrDefault()
              ?? httpContext.Connection
                ?.RemoteIpAddress
                ?.ToString()
              ?? "unavailable"
            : string.Empty;
    }

    private string ResolveUserAgent()
    {
        return _httpContextAccessor
            .HttpContext
            ?.Request
            ?.Headers.UserAgent.ToString() ?? string.Empty;
    }

    private string ResolveSessionId()
    {
        return "unavailable";
    }
}
