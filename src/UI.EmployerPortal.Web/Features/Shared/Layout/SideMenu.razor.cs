using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Routing;
using UI.EmployerPortal.Web.Auth;

namespace UI.EmployerPortal.Web.Features.Shared.Layout;

/// <summary>
/// SideMenu
/// </summary>
public partial class SideMenu : IDisposable
{
    private bool _isMinimized = false;
    private bool _isGuest = false;
    private readonly HashSet<string> _expandedMenus = [];

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private ILayoutOrchestator LayoutOrchestator { get; set; } = default!;
    [Inject]
    private IAuthorizationService AuthorizationService { get; set; } = default!;
    [Inject]
    private AuthenticationStateProvider AuthStateProvider { get; set; } = default!;
    /// <summary>
    /// OnInitialized
    /// </summary>
    protected override async Task OnInitializedAsync()
    {
        NavigationManager.LocationChanged += OnLocationChanged;
    }

    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _isGuest = await LayoutOrchestator.IsGuestAccountAsync();
            _menuItems = _isGuest ? GuestMenuItems : await BuildAuthorizedMenuItemsAsync(MenuItems);
            if (ShouldCollapseMenu(GetUrlPath()))
            {
                _isMinimized = true;
            }
            ExpandActiveMenus();
            StateHasChanged();
        }
    }

    private async Task<List<MenuItem>> BuildAuthorizedMenuItemsAsync(List<MenuItem> template)
    {
        var authState = await AuthStateProvider.GetAuthenticationStateAsync();
        var user = authState.User;
        var result = new List<MenuItem>();
        foreach (var item in template)
        {
            var isVisible = item.IsVisible;
            if (isVisible && item.PolicyName is not null)
            {
                var authResult = await AuthorizationService.AuthorizeAsync(user, null, item.PolicyName);
                isVisible = authResult.Succeeded;
            }
            var authorizedSubmenus = new List<SubMenuItem>();
            foreach (var sub in item.Submenus)
            {
                if (sub.PolicyName is null)
                {
                    authorizedSubmenus.Add(sub);
                }
                else
                {
                    var subResult = await AuthorizationService.AuthorizeAsync(user, null, sub.PolicyName);
                    if (subResult.Succeeded)
                    {
                        authorizedSubmenus.Add(sub);
                    }
                }
            }
            result.Add(new MenuItem
            {
                Id = item.Id,
                Title = item.Title,
                Icon = item.Icon,
                Url = item.Url,
                MinimizedUrl = item.MinimizedUrl,
                LandingUrl = item.LandingUrl,
                HasSubmenu = item.HasSubmenu,
                IsVisible = isVisible,
                PolicyName = item.PolicyName,
                Submenus = authorizedSubmenus
            });
        }
        return result;
    }
    private void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        if (ShouldCollapseMenu(GetUrlPath()))
        {
            _isMinimized = true;
            _expandedMenus.Clear();
        }
        ExpandActiveMenus();
        InvokeAsync(StateHasChanged);
    }

    private static bool ShouldCollapseMenu(string path)
    {
        return
           path.StartsWith("quarterly-tax/tax-and-wage-entry-report", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("quarterly-tax/tax-report-only", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("quarterly-tax/wage-entry-report", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("quarterly-tax/zero-payroll-tax-report", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("tax-wage-report-adjustments/add-new-employees", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("tax-wage-report-adjustments/tax-report-adjustment", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("tax-wage-report-adjustments/wage-adjustment-by-quarter", StringComparison.OrdinalIgnoreCase) ||
           path.StartsWith("tax-wage-report-adjustments/wage-adjustment-multiple-quarter", StringComparison.OrdinalIgnoreCase);
    }


    /// <summary>
    /// Dispose
    /// </summary>
    public void Dispose()
    {
        NavigationManager.LocationChanged -= OnLocationChanged;
    }

    private static readonly List<MenuItem> MenuItems =
    [
        new MenuItem
        {
            Id = "quarterly-tax",
            Title = "Quarterly Tax & Wage Reporting",
            Icon = "images/dashboard/quarterly-tax.svg",
            Url = "quarterly-tax",
            MinimizedUrl = "quarterly-tax/missing-reports",
            LandingUrl = "quarterly-tax/missing-reports",
            HasSubmenu = true,
            IsVisible = true,
            PolicyName = AuthorizationPolicies.RequiresAnyQuarterlyTaxAndWageReportsPermission,
            Submenus =
            [
                new()
                {
                    Title = "Missing Reports",
                    Url = "quarterly-tax/missing-reports",
                    PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_EnterQuarterlyTaxAndWageReportsPermission,
                    AdditionalUrls = [
                        "quarterly-tax/select-report",
                        "quarterly-tax/wage-entry-report",
                        "quarterly-tax/zero-payroll-report",
                        "quarterly-tax/tax-report-only",
                    ] },
                new() { Title = "Wage Report File Upload", Url = "quarterly-tax/wage-upload", PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_EnterQuarterlyTaxAndWageReportsPermission },
                new()
                {
                    Title = "Tax & Wage Report Adjustments",
                    Url = "tax-wage-report-adjustments/adjustments",
                    PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_EnterTaxAndWageReportAdjustmentsPermission,
                    AdditionalUrls = [
                        "tax-wage-report-adjustments/tax-report-adjustment",
                        "tax-wage-report-adjustments/wage-adjustment-by-quarter",
                        "tax-wage-report-adjustments/add-new-employees",
                        "tax-wage-report-adjustments/wage-adjustment-multiple-quarter",
                        ]},
                new() { Title = "Previously Filed Reports", Url = "quarterly-tax/previously-filed", PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_ViewAndPrintTaxandWageReportsPermission },
                new() { Title = "First Quarter Deferral", Url = "quarterly-tax/first-quarter-deferral", PolicyName = AuthorizationPolicies.RequiresAllQuarterlyTaxAndWageReportsPermission },
                new() { Title = "Validation Test Environment", Url = "quarterly-tax/validation-test", PolicyName = AuthorizationPolicies.RequiresAllQuarterlyTaxAndWageReportsPermission },
                new() { Title = "View File Upload Summary", Url = "quarterly-tax/upload-summary", PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_ViewAndPrintTaxandWageReportsPermission },
                new() { Title = "Wage Upload Error Messages", Url = "quarterly-tax/wage-report-file-upload-error-list", PolicyName = AuthorizationPolicies.RequiresQuarterlyTaxAndWageReports_ViewAndPrintTaxandWageReportsPermission }
                ]
        },
        new MenuItem
        {
            Id = "billing-payments",
            Title = "Billing & Payments",
            Icon = "images/dashboard/billing-payments.svg",
            Url = "billing-payments",
            MinimizedUrl = "billing-payments/billing",
            LandingUrl = "billing-payments/billing",
            HasSubmenu = true,
            IsVisible = true,
            PolicyName =  AuthorizationPolicies.RequiresAnyPaymentsPermission,
            Submenus =
            [
                new() { Title = "Billing", Url = "billing-payments/billing" },
                new() { Title = "Payment Options", Url = "billing-payments/payment-options" },
                new() { Title = "Payment History", Url = "billing-payments/payment-history" },
                new() { Title = "Manage Contact", Url = "billing-payments/manage-contact" },
                new() { Title = "Manage Bank Accounts", Url = "billing-payments/manage-bank-accounts" },
            ]
        },
        new MenuItem
        {
            Id = "ui-documents",
            Title = "UI Documents",
            Icon = "images/dashboard/ui-documents.svg",
            Url = "ui-documents",
            HasSubmenu = false,
            IsVisible = true,
            PolicyName =  AuthorizationPolicies.RequiresAnyUIAccountInformationPermission,
        },
        new MenuItem
        {
            Id = "manage-account",
            Title = "View & Manage Account",
            Icon = "images/dashboard/manage-account.svg",
            Url = "manage-account",
            MinimizedUrl = "manage-account/account-users",
            LandingUrl = "manage-account/account-users",
            HasSubmenu = true,
            IsVisible = true,
            PolicyName =  AuthorizationPolicies.RequiresManagerRole,
            Submenus =
            [
                new() { Title = "View Rate Summary", Url = "manage-account/rate-summary" },
                new() { Title = "UI Account Details", Url = "manage-account/account-details" },
                new() { Title = "Manage Addresses", Url = "manage-account/manage-addresses" },
                new() { Title = "UI Account Users", Url = "manage-account/account-users", AdditionalUrls=["manage-account"] },
                new() { Title = "Associated ESP Accounts", Url = "manage-account/esp-accounts" },
                new() { Title = "My UI Account Details", Url = "manage-account/ui-account" }
            ]
        },
        new MenuItem
        {
            Id = "other-functions",
            Title = "Other Functions",
            Icon = "images/dashboard/other-functions.svg",
            Url = "other-functions",
            MinimizedUrl = "other-functions/select-other-functions",
            LandingUrl = "other-functions/select-other-functions",
            HasSubmenu = true,
            IsVisible = true,
            PolicyName =  AuthorizationPolicies.RequiresAnyOtherTaxPermissionsPermission,
            Submenus =
            [
                new() { Title  = "Other Functions", Url ="other-functions/select-other-functions",PolicyName = AuthorizationPolicies.RequiresOtherTaxPermissions_EmployerAbstractRecertificationRequest940CPermission  },
                new() { Title = "Taxable Reserve Fund Transactions", Url = "other-functions/benefit-charges", PolicyName = AuthorizationPolicies.RequiresOtherTaxPermissions_DownloadBenefitsChargesOrReserveFundTransactionsPermission },
                new() { Title = "Request 940-C Recertification Form", Url = "other-functions/940c", PolicyName = AuthorizationPolicies.RequiresOtherTaxPermissions_EmployerAbstractRecertificationRequest940CPermission },
                new() { Title = "Manage Email Subscriptions", Url = "other-functions/email-subscriptions",PolicyName =  AuthorizationPolicies.RequiresAnyOtherTaxPermissionsPermission },
                new() { Title = "Audit File Upload", Url = "other-functions/upload-audit" , PolicyName = AuthorizationPolicies.RequiresAnyOtherTaxPermissionsPermission }
            ]
        }
    ];

    //new MenuItem { Id = "eforms", Title = "eForms", Icon = "images/dashboard/eforms.svg", Url = "eforms", HasSubmenu = false, IsVisible = false },
    //new MenuItem { Id = "secure-messages", Title = "Secure Messages", Icon = "images/dashboard/secure-messages.svg", Url = "secure-messages", HasSubmenu = false, IsVisible = false },

    //new MenuItem { Id = "levies", Title = "Levies", Icon = "images/dashboard/levies.svg", Url = "levies", HasSubmenu = true, IsVisible = false, Submenus = new List<SubMenuItem>
    //{
    //    new() { Title = "Active Levies", Url = "levies/active" },
    //    new() { Title = "Levy Payments", Url = "levies/payments" }
    //}},
    //new MenuItem { Id = "appeals", Title = "Appeals", Icon = "images/dashboard/appeals.svg", Url = "appeals", HasSubmenu = true, IsVisible = false, Submenus = new List<SubMenuItem>
    //{
    //    new() { Title = "File an Appeal", Url = "appeals/file" },
    //    new() { Title = "Upload Hearing Documentation", Url = "appeals/upload-docs" },
    //    new() { Title = "Update Hearing Contact Info", Url = "appeals/contact-info" }
    //}},

    private static readonly List<MenuItem> GuestMenuItems =
    [
        new MenuItem
        {
            Id = "quarterly-tax",
            Title = "Quarterly Tax & Wage Reporting",
            Icon = "images/dashboard/quarterly-tax.svg",
            Url = "quarterly-tax",
            MinimizedUrl = "quarterly-tax/missing-reports",
            LandingUrl = "quarterly-tax/missing-reports",
            HasSubmenu = true,
            IsVisible = true,
            Submenus =
            [
                new() { Title = "Missing Reports", Url = "quarterly-tax/missing-reports" },
                new() { Title = "Wage Report File Upload", Url = "quarterly-tax/wage-upload" },
                new() { Title = "Validation Test Environment", Url = "quarterly-tax/validation-test" },
                new() { Title = "Wage Upload Error Messages", Url = "quarterly-tax/wage-report-file-upload-error-list" }
            ]
        },
        new MenuItem
        {
            Id = "other-functions",
            Title = "Other Functions",
            Icon = "images/dashboard/other-functions.svg",
            Url = "other-functions",
            MinimizedUrl = "other-functions",
            LandingUrl = "other-functions/benefit-charges",
            HasSubmenu = true,
            IsVisible = true,
            Submenus =
            [
                new() { Title = "Download Benefit Charges", Url = "other-functions/benefit-charges" },
                new() { Title = "Request 940C Recertification", Url = "other-functions/940c" },
                new() { Title = "Manage Email Subscriptions", Url = "other-functions/email-subscriptions" },
                new() { Title = "Upload Audit Files", Url = "other-functions/upload-audit" }
            ]
        }
    ];

    private List<MenuItem> _menuItems = MenuItems;

    private void ExpandActiveMenus()
    {
        foreach (var item in _menuItems)
        {
            if (item.HasSubmenu && IsMenuActive(item))
            {
                _expandedMenus.Add(item.Id);
            }
        }
    }

    private void ToggleMenu()
    {
        _isMinimized = !_isMinimized;
        if (_isMinimized)
        {
            _expandedMenus.Clear();
        }
        else
        {
            ExpandActiveMenus();
        }
    }
    private void ToggleExpand(MenuItem item)
    {
        if (_expandedMenus.Contains(item.Id))
        {
            _expandedMenus.Remove(item.Id);
        }
        else
        {
            _expandedMenus.Add(item.Id);
        }
    }
    private void OnMenuClick(MenuItem item)
    {
        if (_isMinimized)
        {
            NavigationManager.NavigateTo(item.MinimizedUrl ?? item.Url, true);
            return;
        }

        if (item.HasSubmenu)
        {
            if (_expandedMenus.Contains(item.Id))
            {
                _expandedMenus.Remove(item.Id);
            }
            else
            {
                _expandedMenus.Add(item.Id);
            }
            if (!string.IsNullOrEmpty(item.LandingUrl))
            {
                NavigationManager.NavigateTo(item.LandingUrl, true);
            }
        }
        else
        {
            NavigationManager.NavigateTo(item.Url, true);
        }
    }

    private bool IsMenuActive(MenuItem item)
    {
        var currentUri = NavigationManager.Uri;
        var relativePath = NavigationManager.ToBaseRelativePath(currentUri).Split('?')[0].TrimEnd('/').ToLower();
        var itemPath = item.Url.TrimStart('/').ToLower();

        if (item.HasSubmenu)
        {
            // Only highlight if one of the submenus matches the current URL
            return item.Submenus.Any(IsSubmenuActive);
        }

        // For items without submenus, highlight if URL matches or starts with the item URL
        return relativePath == itemPath || relativePath.StartsWith(itemPath + "/");
    }

    private bool IsSubmenuActive(SubMenuItem submenu)
    {
        var relativePath = GetUrlPath();
        var mainUrl = submenu.Url.TrimStart('/').ToLower();

        if (relativePath.Equals(mainUrl, StringComparison.OrdinalIgnoreCase) || 
            relativePath.StartsWith(mainUrl + "/", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        foreach (var url in submenu.AdditionalUrls)
        {
            var additionalUrl = url.TrimStart('/').ToLower();
            
            // Exact match
            if (relativePath.Equals(additionalUrl, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (additionalUrl.Contains('/') && relativePath.StartsWith(additionalUrl + "/", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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

    private class MenuItem
    {
        public string Id { get; set; } = "";
        public string Title { get; set; } = "";
        public string Icon { get; set; } = "";
        public string Url { get; set; } = "";
        public string? MinimizedUrl { get; set; }
        public string? LandingUrl { get; set; }
        public bool HasSubmenu { get; set; }
        public List<SubMenuItem> Submenus { get; set; } = new List<SubMenuItem>();
        public bool IsVisible { get; set; }
        public string? PolicyName { get; set; }
    }

    private class SubMenuItem
    {
        public string Title { get; set; } = "";
        public string Url { get; set; } = "";
        public List<string> AdditionalUrls { get; set; } = [];
        public string? PolicyName { get; set; }
    }
}
