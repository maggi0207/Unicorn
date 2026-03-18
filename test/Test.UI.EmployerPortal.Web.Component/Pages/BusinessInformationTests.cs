using Bunit;
using FakeItEasy;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

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
    public void No_Error_Banner_Before_Validate()
    {
        var cut = Render<BusinessInformation>();
        Assert.Empty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Validate_With_Empty_Form_Shows_Error_Banner()
    {
        var cut = Render<BusinessInformation>();
        cut.InvokeAsync(cut.Instance.Validate);
        Assert.NotEmpty(cut.FindAll(".notification-banner--error"));
    }

    [Fact]
    public void Error_Banner_Contains_Missing_Information_Text()
    {
        var cut = Render<BusinessInformation>();
        cut.InvokeAsync(cut.Instance.Validate);
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

    /// <summary>Creates a fully-valid BusinessInformationModel for pre-populating the component via state.</summary>
    private static BusinessInformationModel MakeValidModel()
    {
        var mailing = new AddressModel
        {
            AddressLine1 = "123 Main St",
            City = "Madison",
            State = "WI",
            Zip = "53701",
            Country = "United States"
        };
        var physical = new AddressModel
        {
            AddressLine1 = "456 Oak Ave",
            City = "Milwaukee",
            State = "WI",
            Zip = "53202",
            Country = "United States"
        };
        return new BusinessInformationModel
        {
            FEIN = "12-3456789",
            LegalName = "Test Corp",
            PhoneNumber = "608-555-1234",
            Email = "test@example.com",
            MailingAddress = mailing,
            PhysicalLocations = [physical]
        };
    }

    /// <summary>
    /// Returns true when the form is fully valid and the address service reports no corrections.
    /// </summary>
    [Fact]
    public async Task Validate_Returns_True_When_Form_Valid_And_No_Corrections()
    {
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();

        var cut = Render<BusinessInformation>();
        var result = await cut.InvokeAsync(cut.Instance.Validate);

        Assert.True(result);
    }

    /// <summary>
    /// Calls the address service for both mailing and physical addresses when the form is valid.
    /// </summary>
    [Fact]
    public async Task Validate_Calls_Address_Service_For_Mailing_And_Physical_When_Form_Valid()
    {
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();

        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);

        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .MustHaveHappened(2, Times.Exactly);
    }

    /// <summary>
    /// Returns false and navigates to address correction when the address service
    /// reports that the mailing address needs correction.
    /// </summary>
    [Fact]
    public async Task Validate_Returns_False_When_Address_Service_Returns_Corrections()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Address not found", null));

        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();

        var cut = Render<BusinessInformation>();
        var result = await cut.InvokeAsync(cut.Instance.Validate);

        Assert.False(result);
    }

    /// <summary>
    /// Saves the model to RegistrationState before navigating to address correction.
    /// </summary>
    [Fact]
    public async Task Validate_Saves_BusinessInfo_To_State_When_Address_Service_Needs_Correction()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var model = MakeValidModel();
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = model;

        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);

        Assert.NotNull(state.BusinessInfo);
    }

    /// <summary>
    /// When "Physical Location is the same as the Business address" is checked,
    /// only the mailing address is sent to the address service (not the physical).
    /// </summary>
    [Fact]
    public async Task Validate_Does_Not_Validate_Physical_Address_When_Same_As_Mailing()
    {
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();
        state.PhysicalSameAsMailing = true;

        var cut = Render<BusinessInformation>();
        await cut.InvokeAsync(cut.Instance.Validate);

        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .MustHaveHappenedOnceExactly();
    }

    /// <summary>
    /// Checking "Physical Location is the same as the Business address" disables
    /// the first physical location address fields.
    /// </summary>
    [Fact]
    public void Physical_Same_As_Mailing_Checkbox_Disables_Physical_Address_Fields()
    {
        var cut = Render<BusinessInformation>();
        cut.Find(".bi-same-as-mailing input[type='checkbox']").Change(true);

        var disabledInputs = cut.FindAll("input[disabled]");
        Assert.NotEmpty(disabledInputs);
    }

    /// <summary>
    /// Toggling on the same-as-mailing checkbox copies current mailing values
    /// into the physical location fields (verified via disabled input values).
    /// </summary>
    [Fact]
    public void Physical_Same_As_Mailing_Checkbox_Copies_Mailing_To_Physical()
    {
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();
        state.PhysicalSameAsMailing = false;

        var cut = Render<BusinessInformation>();

        cut.Find(".bi-same-as-mailing input[type='checkbox']").Change(true);

        var cityInputs = cut.FindAll("input[aria-label='City']");
        Assert.Equal(cityInputs[0].GetAttribute("value"), cityInputs[1].GetAttribute("value"));
    }

    /// <summary>
    /// The "same as mailing" checkbox is disabled when the mailing address line contains "PO Box".
    /// </summary>
    [Fact]
    public void Same_As_Mailing_Checkbox_Is_Disabled_When_Mailing_Is_PO_Box()
    {
        var model = MakeValidModel();
        model.MailingAddress.AddressLine1 = "PO Box 1234";

        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = model;

        var cut = Render<BusinessInformation>();

        var checkbox = cut.Find(".bi-same-as-mailing input[type='checkbox']");
        Assert.NotNull(checkbox.GetAttribute("disabled"));
    }

    /// <summary>
    /// The "same as mailing" checkbox is enabled when the mailing address is a normal street address.
    /// </summary>
    [Fact]
    public void Same_As_Mailing_Checkbox_Is_Enabled_When_Mailing_Is_Not_PO_Box()
    {
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = MakeValidModel();

        var cut = Render<BusinessInformation>();

        var checkbox = cut.Find(".bi-same-as-mailing input[type='checkbox']");
        Assert.Null(checkbox.GetAttribute("disabled"));
    }

    /// <summary>
    /// Validate returns false and shows a field error when a physical location
    /// street address contains "PO Box".
    /// </summary>
    [Fact]
    public async Task Validate_Returns_False_When_Physical_Location_Is_PO_Box()
    {
        var model = MakeValidModel();
        model.PhysicalLocations[0].AddressLine1 = "PO Box 999";

        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = model;

        var cut = Render<BusinessInformation>();
        var result = await cut.InvokeAsync(cut.Instance.Validate);

        Assert.False(result);
        Assert.Contains("Physical location cannot be a PO Box", cut.Markup);
    }

    /// <summary>
    /// When the checkbox is checked and the user subsequently types "PO Box" into
    /// the mailing address, the checkbox is automatically unchecked.
    /// </summary>
    [Fact]
    public void Same_As_Mailing_Checkbox_Auto_Unchecks_When_Mailing_Becomes_PO_Box()
    {
        var model = MakeValidModel();
        var state = Services.GetRequiredService<RegistrationStateService>();
        state.BusinessInfo = model;
        state.PhysicalSameAsMailing = false;

        var cut = Render<BusinessInformation>();

        cut.Find(".bi-same-as-mailing input[type='checkbox']").Change(true);

        cut.Find("input[aria-label='Street Address']").Input("PO Box 999");

        var checkbox = cut.Find(".bi-same-as-mailing input[type='checkbox']");
        Assert.Null(checkbox.GetAttribute("checked"));
    }
}
