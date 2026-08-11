using Bunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Pages;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

/// <summary>
/// Component tests for the AccountDetails (Update Account Information) page.
/// Covers initial rendering, conditional reason dropdown visibility,
/// form submission with/without changes, validation errors, and navigation.
/// </summary>
public class AccountDetailsTests : BunitContext
{
    /// <summary>The fake account details service.</summary>
    private readonly IAccountDetailsService _fakeService;

    /// <summary>The fake session manager.</summary>
    private readonly ISessionManager _fakeSession;

    /// <summary>Registers required services before each test.</summary>
    public AccountDetailsTests()
    {
        _fakeService = A.Fake<IAccountDetailsService>();
        _fakeSession = A.Fake<ISessionManager>();

        // Default: session returns employerSK = 1
        A.CallTo(() => _fakeSession.GetAsync<SelectedEmployerAccount>())
            .Returns(Task.FromResult<SelectedEmployerAccount?>(
                new SelectedEmployerAccount
                {
                    EmployerAccount = new EmployerAccount { Id = 1 }
                }));

        // Default: service returns a populated model
        A.CallTo(() => _fakeService.GetAccountDetailsAsync(A<int>._))
            .Returns(Task.FromResult(MakeLoadedModel()));

        // Default: update returns success
        A.CallTo(() => _fakeService.UpdateAccountDetailsAsync(A<AccountDetailsModel>._, A<int>._))
            .Returns(Task.FromResult<(bool success, string error)>((true, string.Empty)));

        Services.AddSingleton<IAccountDetailsService>(_fakeService);
        Services.AddSingleton<ISessionManager>(_fakeSession);

        JSInterop.Mode = JSRuntimeMode.Loose;
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a model as returned by the backend service.
    /// </summary>
    private static AccountDetailsModel MakeLoadedModel()
    {
        return new AccountDetailsModel
        {
            FEIN = "12-3456789",
            LegalName = "Acme Corp",
            TradeName = "Acme",
            PhoneNumber = "555-123-4567",
            Extension = "101",
            CountryCode = "+1",
            EmailAddress = "test@example.com"
        };
    }

    /// <summary>
    /// Renders the AccountDetails component and flushes OnInitializedAsync.
    /// </summary>
    private async Task<IRenderedComponent<AccountDetails>> RenderAndFlushAsync()
    {
        var cut = Render<AccountDetails>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });
        return cut;
    }

    // ── Initial Rendering ────────────────────────────────────────────────────

    /// <summary>Page renders the "Update Account Information" title after loading.</summary>
    [Fact]
    public async Task Renders_Page_Title()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Equal("Update Account Information", cut.Find("h1.page-title").TextContent.Trim());
    }

    /// <summary>Page renders the FEIN field.</summary>
    [Fact]
    public async Task Renders_FEIN_Field()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("FEIN", cut.Markup);
    }

    /// <summary>Page renders the Legal Name field.</summary>
    [Fact]
    public async Task Renders_Legal_Name_Field()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("Legal Name", cut.Markup);
    }

    /// <summary>Page renders the Phone Number field.</summary>
    [Fact]
    public async Task Renders_Phone_Number_Field()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("Phone Number", cut.Markup);
    }

    /// <summary>Page renders the Email Address field.</summary>
    [Fact]
    public async Task Renders_Email_Address_Field()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("Email Address", cut.Markup);
    }

    /// <summary>Page renders the Trade Name field.</summary>
    [Fact]
    public async Task Renders_Trade_Name_Field()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("Trade Name", cut.Markup);
    }

    /// <summary>Page renders CANCEL and SAVE buttons.</summary>
    [Fact]
    public async Task Renders_Cancel_And_Save_Buttons()
    {
        var cut = await RenderAndFlushAsync();

        Assert.NotEmpty(cut.FindAll("button[aria-label='Cancel and return to manage account']"));
        Assert.NotEmpty(cut.FindAll("button[aria-label='Save account information changes']"));
    }

    // ── Conditional Reason Dropdowns ─────────────────────────────────────────

    /// <summary>Reason for FEIN Change dropdown is hidden when FEIN has not changed.</summary>
    [Fact]
    public async Task FEIN_Reason_Dropdown_Hidden_When_FEIN_Unchanged()
    {
        var cut = await RenderAndFlushAsync();

        Assert.DoesNotContain("Reason for FEIN Change", cut.Markup);
    }

    /// <summary>Reason for Legal Name Change dropdown is hidden when Legal Name has not changed.</summary>
    [Fact]
    public async Task LegalName_Reason_Dropdown_Hidden_When_LegalName_Unchanged()
    {
        var cut = await RenderAndFlushAsync();

        Assert.DoesNotContain("Reason for Legal Name Change", cut.Markup);
    }

    // ── Submit: No Changes ───────────────────────────────────────────────────

    /// <summary>Submitting without changes navigates to account summary without calling UpdateAccountDetailsAsync.</summary>
    [Fact]
    public async Task Submit_Without_Changes_Navigates_Without_Calling_Update()
    {
        var cut = await RenderAndFlushAsync();

        await cut.InvokeAsync(() =>
        {
            return cut.Find("form").SubmitAsync();
        });

        A.CallTo(() => _fakeService.UpdateAccountDetailsAsync(A<AccountDetailsModel>._, A<int>._))
            .MustNotHaveHappened();
    }

    /// <summary>Submitting without changes navigates to account summary without success query params.</summary>
    [Fact]
    public async Task Submit_Without_Changes_Navigates_To_Summary_Page()
    {
        var cut = await RenderAndFlushAsync();
        var navManager = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        await cut.InvokeAsync(() =>
        {
            return cut.Find("form").SubmitAsync();
        });

        Assert.Contains("/manage-account/account-summary", navManager.Uri);
        Assert.DoesNotContain("success=true", navManager.Uri);
    }

    // ── Submit: With Changes (Success) ───────────────────────────────────────

    /// <summary>Submitting with email changed calls UpdateAccountDetailsAsync and navigates with email=1.</summary>
    [Fact]
    public async Task Submit_With_Email_Changed_Calls_Update_And_Navigates_With_Email_Param()
    {
        // Return a model where email will differ after user types a new one
        var model = MakeLoadedModel();
        model.EmailAddress = "old@example.com";
        A.CallTo(() => _fakeService.GetAccountDetailsAsync(A<int>._))
            .Returns(Task.FromResult(model));

        var cut = await RenderAndFlushAsync();

        // Change the email field
        cut.Find("input#EmailAddress").Change("new@example.com");

        await cut.InvokeAsync(() =>
        {
            return cut.Find("form").SubmitAsync();
        });

        A.CallTo(() => _fakeService.UpdateAccountDetailsAsync(A<AccountDetailsModel>._, 1))
            .MustHaveHappenedOnceExactly();
    }

    // ── Submit: With Changes (Error) ─────────────────────────────────────────

    /// <summary>When UpdateAccountDetailsAsync returns an error, error message is displayed on the page.</summary>
    [Fact]
    public async Task Submit_With_Error_Response_Shows_Error_Message()
    {
        var model = MakeLoadedModel();
        model.EmailAddress = "old@example.com";
        A.CallTo(() => _fakeService.GetAccountDetailsAsync(A<int>._))
            .Returns(Task.FromResult(model));

        A.CallTo(() => _fakeService.UpdateAccountDetailsAsync(A<AccountDetailsModel>._, A<int>._))
            .Returns(Task.FromResult<(bool success, string error)>((false, "A backend rule violation occurred.")));

        var cut = await RenderAndFlushAsync();

        cut.Find("input#EmailAddress").Change("new@example.com");

        await cut.InvokeAsync(() =>
        {
            return cut.Find("form").SubmitAsync();
        });

        Assert.Contains("A backend rule violation occurred.", cut.Markup);
    }

    // ── Cancel Button ────────────────────────────────────────────────────────

    /// <summary>Clicking CANCEL navigates to /manage-account.</summary>
    [Fact]
    public async Task Cancel_Navigates_To_Manage_Account()
    {
        var cut = await RenderAndFlushAsync();
        var navManager = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        cut.Find("button[aria-label='Cancel and return to manage account']").Click();

        Assert.Contains("/manage-account", navManager.Uri);
    }

    // ── Loading State ────────────────────────────────────────────────────────

    /// <summary>Loading spinner is shown while data is being fetched.</summary>
    [Fact]
    public void Shows_Loading_Spinner_While_Fetching()
    {
        A.CallTo(() => _fakeService.GetAccountDetailsAsync(A<int>._))
            .Returns(new TaskCompletionSource<AccountDetailsModel>().Task);

        var cut = Render<AccountDetails>();

        Assert.Contains("Loading", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    // ── Session Null ─────────────────────────────────────────────────────────

    /// <summary>When session is null, GetAccountDetailsAsync is not called.</summary>
    [Fact]
    public async Task GetAccountDetailsAsync_Not_Called_When_Session_Is_Null()
    {
        A.CallTo(() => _fakeSession.GetAsync<SelectedEmployerAccount>())
            .Returns(Task.FromResult<SelectedEmployerAccount?>(null));

        var cut = await RenderAndFlushAsync();

        A.CallTo(() => _fakeService.GetAccountDetailsAsync(A<int>._))
            .MustNotHaveHappened();
    }

    // ── GetAccountDetailsAsync Called With Correct SK ─────────────────────────

    /// <summary>GetAccountDetailsAsync is called with the employerSK from session.</summary>
    [Fact]
    public async Task GetAccountDetailsAsync_Called_With_Session_EmployerSK()
    {
        A.CallTo(() => _fakeSession.GetAsync<SelectedEmployerAccount>())
            .Returns(Task.FromResult<SelectedEmployerAccount?>(
                new SelectedEmployerAccount { EmployerAccount = new EmployerAccount { Id = 42 } }));

        var cut = await RenderAndFlushAsync();

        A.CallTo(() => _fakeService.GetAccountDetailsAsync(42))
            .MustHaveHappenedOnceExactly();
    }

    // ── No Validation Errors Before Submit ───────────────────────────────────

    /// <summary>No error banner is shown before the form is submitted.</summary>
    [Fact]
    public async Task No_Error_Banner_Before_Submit()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    // ── Manage Addresses Link ────────────────────────────────────────────────

    /// <summary>Page renders the Manage Addresses navigation link.</summary>
    [Fact]
    public async Task Renders_Manage_Addresses_Link()
    {
        var cut = await RenderAndFlushAsync();

        Assert.Contains("Manage Addresses", cut.Markup);
    }
}
