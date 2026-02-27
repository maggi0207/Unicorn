using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Web.Features.EmployerRegistration;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

public class BusinessInformationTests : BunitContext
{
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

    [Fact]
    public void Renders_Back_Save_Quit_And_Continue_Buttons()
    {
        var cut = Render<BusinessInformation>();
        Assert.NotNull(cut.Find("button.btn--secondary"));
        Assert.NotNull(cut.Find("button.btn--tertiary"));
        Assert.NotNull(cut.Find("button[type='submit'].btn--primary"));
    }

    [Fact]
    public void Continue_Button_Is_Submit_Type()
    {
        var cut = Render<BusinessInformation>();
        Assert.NotNull(cut.Find("button[type='submit']"));
    }

    [Fact]
    public void Back_Button_Navigates_To_Ownership()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("button.btn--secondary").Click();
        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.Contains("employer-registration/ownership", nav.Uri);
    }

    [Fact]
    public void Save_And_Quit_Navigates_To_Dashboard()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("button.btn--tertiary").Click();
        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.Contains("dashboard", nav.Uri);
    }

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

    [Fact]
    public void No_Error_Banner_Before_Submit()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Continue_With_Empty_Form_Shows_Error_Banner()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("button[type='submit']").Click();
        Assert.NotEmpty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Error_Banner_Contains_Missing_Information_Text()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("button[type='submit']").Click();
        Assert.Contains("Missing information", cut.Find(".notification-banner--error").TextContent);
    }

    [Fact]
    public void No_Field_Errors_Before_Any_Interaction()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".field-error"));
    }

    [Fact]
    public void Field_Error_Shown_For_Touched_Required_Field_Without_Submit()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").Input(string.Empty);
        Assert.NotEmpty(cut.FindAll(".field-error"));
    }

    [Fact]
    public void No_Global_Error_Banner_When_Only_Field_Touched_Without_Submit()
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
    public void Continue_With_Empty_Form_Shows_Address_Field_Errors()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("button[type='submit']").Click();
        Assert.NotEmpty(cut.FindAll(".field-error"));
    }

    [Fact]
    public void Blur_On_Empty_Required_Field_Shows_Field_Error()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").TriggerEvent("onfocusout", new FocusEventArgs());
        Assert.NotEmpty(cut.FindAll(".field-error"));
    }

    [Fact]
    public void Blur_Does_Not_Show_Global_Error_Banner()
    {
        var cut = Render<BusinessInformation>();
        cut.Find("input[aria-label='Legal Name']").TriggerEvent("onfocusout", new FocusEventArgs());
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }
}
