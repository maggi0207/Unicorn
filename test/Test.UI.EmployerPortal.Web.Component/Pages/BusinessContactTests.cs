using Bunit;
using FakeItEasy;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

/// <summary>
/// Component tests for BusinessContact.
/// Validation is triggered via Validate() (called by the wizard), not a submit button.
/// </summary>
public class BusinessContactTests : BunitContext
{
    private readonly IAddressValidationWrapper _fakeValidator;

    /// <summary>Registers required services before each test.</summary>
    public BusinessContactTests()
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
        var cut = Render<BusinessContact>();
        Assert.Equal("Business Contact", cut.Find("h1.page-title").TextContent.Trim());
    }

    [Fact]
    public void Renders_Subtitle()
    {
        var cut = Render<BusinessContact>();
        Assert.Contains("All fields are required unless noted", cut.Find("p.page-subtitle").TextContent);
    }

    [Fact]
    public void Renders_First_Name_Field()
    {
        var cut = Render<BusinessContact>();
        Assert.Contains("First Name", cut.Markup);
    }

    [Fact]
    public void Renders_Last_Name_Field()
    {
        var cut = Render<BusinessContact>();
        Assert.Contains("Last Name", cut.Markup);
    }

    [Fact]
    public void Renders_Title_Optional_Field()
    {
        var cut = Render<BusinessContact>();
        Assert.Contains("Title (Optional)", cut.Markup);
    }

    [Fact]
    public void Renders_Address_Question_Text()
    {
        var cut = Render<BusinessContact>();
        Assert.Contains(
            "Is the Business Contact Address different from the Business Mailing Address?",
            cut.Find(".bc-question-text").TextContent);
    }

    [Fact]
    public void Renders_Yes_And_No_Radio_Buttons()
    {
        var cut = Render<BusinessContact>();
        var radios = cut.FindAll("input[type='radio'][name='isDifferentAddress']");
        Assert.Equal(2, radios.Count);
    }

    // ── Contact Address Visibility ────────────────────────────────────────────

    [Fact]
    public void Contact_Address_Section_Hidden_Initially()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".bi-section-header"));
    }

    [Fact]
    public void Selecting_Yes_Shows_Contact_Address_Section()
    {
        var cut = Render<BusinessContact>();
        cut.FindAll("input[type='radio']")[0].Change(new ChangeEventArgs());
        Assert.NotEmpty(cut.FindAll(".bi-section-header"));
    }

    [Fact]
    public void Selecting_No_Hides_Contact_Address_Section()
    {
        var cut = Render<BusinessContact>();
        cut.FindAll("input[type='radio']")[0].Change(new ChangeEventArgs());
        Assert.NotEmpty(cut.FindAll(".bi-section-header"));
        cut.FindAll("input[type='radio']")[1].Change(new ChangeEventArgs());
        Assert.Empty(cut.FindAll(".bi-section-header"));
    }

    // ── Validation ────────────────────────────────────────────────────────────

    [Fact]
    public void No_Error_Banner_Before_Validate()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void No_Radio_Error_Before_Validate()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".bc-radio-error"));
    }

    [Fact]
    public async Task Validate_With_Empty_Fields_Shows_Error_Banner()
    {
        var cut = Render<BusinessContact>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public async Task Validate_Without_Radio_Selection_Shows_Radio_Error()
    {
        var cut = Render<BusinessContact>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".bc-radio-error"));
    }

    [Fact]
    public async Task Validate_With_Empty_Form_Returns_False()
    {
        var cut = Render<BusinessContact>();
        var result = await cut.InvokeAsync(cut.Instance.Validate);
        Assert.False(result);
    }

    [Fact]
    public void No_Field_Errors_Before_Any_Interaction()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Field_Error_Shown_For_Touched_Required_Field_Without_Validate()
    {
        var cut = Render<BusinessContact>();
        cut.Find("input[aria-label='First Name']").Input(string.Empty);
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void No_Global_Error_Banner_When_Only_Field_Touched_Without_Validate()
    {
        var cut = Render<BusinessContact>();
        cut.Find("input[aria-label='First Name']").Input(string.Empty);
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Untouched_Fields_Have_No_Error_When_Another_Field_Is_Touched()
    {
        var cut = Render<BusinessContact>();
        cut.Find("input[aria-label='First Name']").Input(string.Empty);
        Assert.DoesNotContain("Last Name is required", cut.Markup);
    }

    [Fact]
    public async Task Validate_With_Empty_Fields_Shows_Field_Errors()
    {
        var cut = Render<BusinessContact>();
        await cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Blur_On_Empty_Required_Field_Shows_Field_Error()
    {
        var cut = Render<BusinessContact>();
        cut.Find("input[aria-label='First Name']").TriggerEvent("onblur", new FocusEventArgs());
        Assert.NotEmpty(cut.FindAll(".master-error"));
    }

    [Fact]
    public void Blur_Does_Not_Show_Global_Error_Banner()
    {
        var cut = Render<BusinessContact>();
        cut.Find("input[aria-label='First Name']").TriggerEvent("onblur", new FocusEventArgs());
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public async Task Validate_Does_Not_Call_Address_Service_When_Form_Is_Invalid()
    {
        var cut = Render<BusinessContact>();
        await cut.InvokeAsync(cut.Instance.Validate);
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._)).MustNotHaveHappened();
    }
}
