using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.Shared.Layout;

/// <summary>
/// MainLayout
/// </summary>
public partial class MainLayout : IDisposable
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private LinkGenerator LinkGenerator { get; set; } = default!;

    //[Inject]
    //private AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    /// <summary>
    /// Explicitely decide if menu is visible. This is currently used when page is not found. 
    /// </summary>
    [CascadingParameter(Name = "HideMenu")]
    public bool HideMenu { get; set; } = false;

    private string AuthenticationInformation { get; set; } = string.Empty;
    private bool _isAuthenticated = false;
    private string _logoutUrl = string.Empty;
    private bool IsLandingPage
    {
        get
        {
            var path = GetUrlPath();
            return path.Equals("landing-page");
        }
    }

    private bool IsDashboardPage
    {
        get
        {
            var path = GetUrlPath();
            return path.Equals("guest-dashboard") || path.Equals("employer-dashboard");
        }
    }

    private bool ShowSideMenu
    {
        get
        {
            var path = GetUrlPath();

            return !(HideMenu ||
                      path == "" ||
                      path.Equals("landing-page") ||
                      path.Equals("employer-dashboard") ||
                      path.Equals("guest-dashboard") ||
                      path.StartsWith("employer-registration", StringComparison.OrdinalIgnoreCase) ||
                      path.Equals("pages/accessaccount", StringComparison.OrdinalIgnoreCase));
        }
    }
    private bool ShowTopHeader
    {
        get
        {
            var path = GetUrlPath();
            return path != "";
        }
    }

    private bool ShowSubHeader
    {
        get
        {
            var path = GetUrlPath();

            return !(path == "" ||
                path.StartsWith("employer-registration", StringComparison.OrdinalIgnoreCase));
        }
    }

    private bool ShowEmployerNavigation
    {
        get
        {
            var path = GetUrlPath();

            return !(path == "" || path.Equals("landing-page"));
        }
    }

    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        var baseUri = new Uri(NavigationManager.BaseUri);
        var pathBase = new PathString(baseUri.AbsolutePath.TrimEnd('/'));
        _logoutUrl = LinkGenerator.GetPathByName("Logout", pathBase: pathBase) ?? string.Empty;

        if (UserAccountService.IsAuthenticated())
        {
            _isAuthenticated = true;
            GetAuthenticationInformationAsync();
        }
        else
        {
            _isAuthenticated = false;
        }

        NavigationManager.LocationChanged += OnLocationChanged;
        StateHasChanged();
    }

    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        InvokeAsync(StateHasChanged);
    }

    private string GetUrlPath()
    {
        var relativePath = NavigationManager.ToBaseRelativePath(NavigationManager.Uri).ToLower();

        if (relativePath.Contains("#"))
        {
            relativePath = relativePath.Split('#')[0];
        }

        return relativePath.Split('?')[0].TrimEnd('/');
    }

    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private void GetAuthenticationInformationAsync()
    {
        AuthenticationInformation = $"{UserAccountService.GetFirstName()} {UserAccountService.GetLastName()}, {UserAccountService.GetUserID()}";
    }
}
