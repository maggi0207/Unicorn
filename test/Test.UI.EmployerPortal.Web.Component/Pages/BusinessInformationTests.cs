using Bunit;
using FakeItEasy;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

/// <summary>
/// Component tests for BusinessInformation.
/// Validation is triggered via Validate() (called by the wizard), not a submit button.
/// </summary>
public class BusinessInformationTests : BunitContext
{
    private readonly IAddressValidationWrapper _fakeValidator;

    /// <summary>Registers required services before each test.</summary>
    public BusinessInformationTests()
    {
        _fakeValidator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        Services.AddSingleton(_fakeValidator);
        Services.AddSingleton<RegistrationStateService>();
        Services.AddSingleton<AddressValidationCoordinator>();
    }

    // ── Render ────────────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Page_Title()
    {
        var cut = Render<BusinessInformation>();
        Assert.Equal("Business Information", cut.Find("h1.page-title").TextContent.Trim());
    }

    [Fact]
    public void Renders_Subtitle()
    {
        var cut = Render<BusinessInformation>();
        Assert.Contains("All fields are required unless noted", cut.Find("p.page-subtitle").TextContent);
    }

    [Fact]
    public void Renders_Business_Mailing_Address_Section_Header()
    {
        var cut = Render<BusinessInformation>();
        Assert.Contains("Business Mailing Address", cut.Markup);
    }

    [Fact]
    public void Renders_Physical_Location_1_Section_Header()
    {
        var cut = Render<BusinessInformation>();
        Assert.Contains("Physical Location 1", cut.Markup);
    }

    // ── Physical Locations ────────────────────────────────────────────────────

    [Fact]
    public void Add_Another_Physical_Location_Button_Visible_Initially()
    {
        var cut = Render<BusinessInformation>();
        Assert.NotEmpty(cut.FindAll(".bi-add-location"));
    }

    [Fact]
    public void No_Remove_Button_On_First_Physical_Location()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".bi-remove-location"));
    }

    [Fact]
    public void Only_One_Physical_Location_Rendered_Initially()
    {
        var cut = Render<BusinessInformation>();
        Assert.Contains("Physical Location 1", cut.Markup);
        Assert.DoesNotContain("Physical Location 2", cut.Markup);
    }

    [Fact]
    public void Add_Button_Appends_Second_Physical_Location()
    {
        var cut = Render<BusinessInformation>();
        cut.Find(".bi-add-location").Click();
        Assert.Contains("Physical Location 2", cut.Markup);
    }

    [Fact]
    public void Remove_Button_Appears_After_Adding_Second_Location()
    {
        var cut = Render<BusinessInformation>();
        cut.Find(".bi-add-location").Click();
        Assert.NotEmpty(cut.FindAll(".bi-remove-location"));
    }

    [Fact]
    public void Add_Button_Hidden_When_At_Max_Three_Locations()
    {
        var cut = Render<BusinessInformation>();
        cut.Find(".bi-add-location").Click();
        cut.Find(".bi-add-location").Click();
        Assert.Empty(cut.FindAll(".bi-add-location"));
    }

    [Fact]
    public void Clicking_Remove_Decreases_Location_Count()
    {
        var cut = Render<BusinessInformation>();
        cut.Find(".bi-add-location").Click();
        Assert.Contains("Physical Location 2", cut.Markup);
        cut.Find(".bi-remove-location").Click();
        Assert.DoesNotContain("Physical Location 2", cut.Markup);
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public void No_Error_Banner_Before_Validate()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public async Task Validate_With_Empty_Form_Shows_Error_Banner()
    {
        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public async Task Error_Banner_Contains_Missing_Information_Text()
    {
        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.Contains("Missing information", cut.Find(".notification-banner--error").TextContent);
    }

    [Fact]
    public async Task Validate_With_Empty_Form_Returns_False()
    {
        var cut = Render<BusinessInformation>();
        var result = await cut.InvokeAsync(cut.Instance.Validate);
        Assert.False(result);
    }

    [Fact]
    public void No_Field_Errors_Before_Any_Interaction()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Field_Error_Shown_For_Touched_Required_Field_Without_Validate()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").Input(string.Empty);
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void No_Global_Error_Banner_When_Only_Field_Touched_Without_Validate()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").Input(string.Empty);
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Untouched_Fields_Have_No_Error_When_Another_Field_Is_Touched()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").Input(string.Empty);
        Assert.DoesNotContain("FEIN is required", cut.Markup);
    }

    [Fact]
    public async Task Validate_With_Empty_Form_Shows_Address_Field_Errors()
    {
        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Blur_On_Empty_Required_Field_Shows_Field_Error()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").TriggerEvent("onblur", new FocusEventArgs());
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Blur_Does_Not_Show_Global_Error_Banner()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").TriggerEvent("onblur", new FocusEventArgs());
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public async Task Validate_Does_Not_Call_Address_Service_When_Form_Is_Invalid()
    {
        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._)).MustNotHaveHappened();
    }
}
