using Bunit;
using FakeItEasy;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration;
using UI.EmployerPortal.Web.Services;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

public class BusinessContactTests : BunitContext
{
    private readonly IAddressValidationWrapper _fakeValidator;

    public BusinessContactTests()
    {
        _fakeValidator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(Task.FromResult(new AddressValidationResult(true, null, null)));

        Services.AddSingleton(_fakeValidator);
    }

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
        Assert.Contains("Is the Business Contact Address different from the Business Mailing Address?", cut.Find(".bc-question-text").TextContent);
    }

    [Fact]
    public void Renders_Yes_And_No_Radio_Buttons()
    {
        var cut = Render<BusinessContact>();
        var radios = cut.FindAll("input[type='radio'][name='isDifferentAddress']");
        Assert.Equal(2, radios.Count);
    }

    [Fact]
    public void Renders_Back_Save_Quit_And_Continue_Buttons()
    {
        var cut = Render<BusinessContact>();
        Assert.NotNull(cut.Find("button.btn--secondary"));
        Assert.NotNull(cut.Find("button.btn--tertiary"));
        Assert.NotNull(cut.Find("button.btn--primary"));
    }

    [Fact]
    public void No_Error_Banner_Before_Submit()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".bi-error-banner"));
    }

    [Fact]
    public void Contact_Address_Section_Hidden_Initially()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".bi-section-header"));
    }

    [Fact]
    public void No_Radio_Error_Before_Submit()
    {
        var cut = Render<BusinessContact>();
        Assert.Empty(cut.FindAll(".bc-radio-error"));
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

    [Fact]
    public void Continue_With_Empty_Fields_Shows_Error_Banner()
    {
        var cut = Render<BusinessContact>();
        cut.Find("button.btn--primary").Click();
        Assert.NotEmpty(cut.FindAll(".bi-error-banner"));
    }

    [Fact]
    public void Continue_Without_Radio_Selection_Shows_Radio_Error()
    {
        var cut = Render<BusinessContact>();
        cut.Find("button.btn--primary").Click();
        Assert.NotEmpty(cut.FindAll(".bc-radio-error"));
    }

    [Fact]
    public void Back_Button_Navigates_To_Business_Information()
    {
        var cut = Render<BusinessContact>();
        cut.Find("button.btn--secondary").Click();
        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.Contains("employer-registration/business-information", nav.Uri);
    }

    [Fact]
    public void Save_And_Quit_Navigates_To_Dashboard()
    {
        var cut = Render<BusinessContact>();
        cut.Find("button.btn--tertiary").Click();
        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.Contains("dashboard", nav.Uri);
    }
}
