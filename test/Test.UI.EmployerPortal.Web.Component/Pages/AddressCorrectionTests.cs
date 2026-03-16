using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Pages;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

namespace Test.UI.EmployerPortal.Web.Component.Pages;

/// <summary>
/// Component tests for the <see cref="AddressCorrection"/> page.
/// Covers rendering, validation, radio/checkbox interaction, navigation, and correction application.
/// </summary>
public class AddressCorrectionTests : BunitContext
{
    private readonly RegistrationStateService _stateService;

    /// <summary>Registers the state service before each test.</summary>
    public AddressCorrectionTests()
    {
        _stateService = new RegistrationStateService();
        Services.AddSingleton(_stateService);
    }

    /// <summary>Creates an original entered address model.</summary>
    private static AddressModel MakeOriginal()
        => new AddressModel
        {
            AddressLine1 = "123 Main St",
            City = "Madison",
            State = "WI",
            Zip = "53701",
            Country = "United States"
        };

    /// <summary>Creates a standardized suggested address model (differs from original).</summary>
    private static AddressModel MakeSuggested()
        => new AddressModel
        {
            AddressLine1 = "123 MAIN ST",
            City = "MADISON",
            State = "WI",
            Zip = "53701",
            Extension = "1234",
            Country = "United States"
        };

    /// <summary>Creates a correction item that has a suggestion.</summary>
    private static AddressCorrectionItem ItemWithSuggestion(string label = "Mailing Address")
        => new AddressCorrectionItem(label, MakeOriginal(), MakeSuggested());

    /// <summary>Creates a correction item that has no suggestion (service could not standardize).</summary>
    private static AddressCorrectionItem ItemWithoutSuggestion(string label = "Mailing Address")
        => new AddressCorrectionItem(label, MakeOriginal(), null, "Address not found");

    /// <summary>Verifies the page title reads "Address Correction".</summary>
    [Fact]
    public void Renders_Page_Title()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.Equal("Address Correction", cut.Find("h1.page-title").TextContent.Trim());
    }

    /// <summary>Renders one warning banner per correction item.</summary>
    [Fact]
    public void Renders_Warning_Banner_For_Each_Correction_Item()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion(), ItemWithSuggestion("Physical Address")];
        var cut = Render<AddressCorrection>();

        Assert.Equal(2, cut.FindAll(".ac-warning").Count);
    }

    /// <summary>The warning banner body text contains the lowercase item label.</summary>
    [Fact]
    public void Warning_Banner_Contains_Item_Label_Text()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion("Business Mailing Address")];
        var cut = Render<AddressCorrection>();

        Assert.Contains("business mailing address", cut.Find(".ac-warning").TextContent.ToLower());
    }

    /// <summary>Error message text from the service is rendered inside the warning banner.</summary>
    [Fact]
    public void Renders_ErrorMessage_In_Warning_Banner_When_Present()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.Contains("Address not found", cut.Find(".ac-warning-errors").TextContent);
    }

    /// <summary>No error list element is rendered when the item has no error message.</summary>
    [Fact]
    public void Does_Not_Render_Error_List_When_No_ErrorMessage()
    {
        _stateService.AddressCorrections =
            [new AddressCorrectionItem("Mailing Address", MakeOriginal(), MakeSuggested(), null)];
        var cut = Render<AddressCorrection>();

        Assert.Empty(cut.FindAll(".ac-warning-errors"));
    }

    /// <summary>The entered address box shows the original address in upper-case.</summary>
    [Fact]
    public void Renders_Entered_Address_Box_With_Upper_Cased_Street()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();

        var boxes = cut.FindAll(".ac-address-box");
        Assert.Contains("123 MAIN ST", boxes[0].TextContent);
    }

    /// <summary>Two radio buttons are rendered when a suggestion exists.</summary>
    [Fact]
    public void Renders_Two_Radio_Buttons_When_Suggestion_Exists()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.Equal(2, cut.FindAll("input[type='radio']").Count);
    }

    /// <summary>No radio buttons are rendered when there is no suggestion.</summary>
    [Fact]
    public void No_Radio_Buttons_When_No_Suggestion()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.Empty(cut.FindAll("input[type='radio']"));
    }

    /// <summary>A checkbox is rendered when there is no suggestion.</summary>
    [Fact]
    public void Renders_Checkbox_When_No_Suggestion()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.NotEmpty(cut.FindAll("input[type='checkbox']"));
    }

    /// <summary>No required-options banner is visible before CONTINUE is clicked.</summary>
    [Fact]
    public void No_Required_Banner_Before_Continue_Clicked()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();

        Assert.Empty(cut.FindAll(".ac-required-banner"));
    }

    /// <summary>Required-options banner appears when CONTINUE is clicked without a radio selection.</summary>
    [Fact]
    public void Continue_Without_Selection_Shows_Required_Banner()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click();

        Assert.NotEmpty(cut.FindAll(".ac-required-banner"));
    }

    /// <summary>Required-options banner appears when CONTINUE is clicked without checking the checkbox.</summary>
    [Fact]
    public void Continue_Without_Checkbox_Checked_Shows_Required_Banner()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click();

        Assert.NotEmpty(cut.FindAll(".ac-required-banner"));
    }

    /// <summary>Field-level error message appears for an unresolved item with a suggestion.</summary>
    [Fact]
    public void Continue_Without_Selection_Shows_Field_Error()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click();

        Assert.NotEmpty(cut.FindAll(".ac-field-error"));
    }

    /// <summary>Required-options banner contains an anchor link to the unresolved correction block.</summary>
    [Fact]
    public void Required_Banner_Links_To_Unresolved_Item()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion("Mailing Address")];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click();

        Assert.Contains("ac-item-0", cut.Find(".ac-required-banner").InnerHtml);
    }

    /// <summary>Selecting the "use corrected" radio clears the field-level error for that item.</summary>
    [Fact]
    public void Selecting_Corrected_Radio_Clears_Field_Error()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click(); // trigger error

        cut.FindAll("input[type='radio']")[0].Change(true); // "Use corrected address"

        Assert.Empty(cut.FindAll(".ac-field-error"));
    }

    /// <summary>Selecting the "use as entered" radio clears the field-level error for that item.</summary>
    [Fact]
    public void Selecting_Entered_Radio_Clears_Field_Error()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click(); // trigger error

        cut.FindAll("input[type='radio']")[1].Change(true); // "Use as entered"

        Assert.Empty(cut.FindAll(".ac-field-error"));
    }

    /// <summary>Checking the "use as entered" checkbox clears the required-options banner.</summary>
    [Fact]
    public void Checking_Checkbox_Clears_Required_Banner()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--primary").Click();

        cut.Find("input[type='checkbox']").Change(true);

        Assert.Empty(cut.FindAll(".ac-required-banner"));
    }

    /// <summary>Unchecking the checkbox after it was checked re-enables the error on the next CONTINUE.</summary>
    [Fact]
    public void Unchecking_Checkbox_Allows_Error_On_Next_Continue()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        var cut = Render<AddressCorrection>();
        cut.Find("input[type='checkbox']").Change(true);   // check
        cut.Find("input[type='checkbox']").Change(false);  // uncheck
        cut.Find(".btn--primary").Click();

        Assert.NotEmpty(cut.FindAll(".ac-required-banner"));
    }

    /// <summary>Clicking CONTINUE after resolving all items navigates to the registration steps page.</summary>
    [Fact]
    public void Continue_With_All_Resolved_Navigates_To_Steps()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        _stateService.PostCorrectionStep = 4;
        var cut = Render<AddressCorrection>();
        cut.FindAll("input[type='radio']")[0].Change(true); // select "corrected"
        cut.Find(".btn--primary").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/employer-registration/steps", nav.Uri);
    }

    /// <summary>Clicking CONTINUE after checking the no-suggestion checkbox navigates to steps.</summary>
    [Fact]
    public void Continue_With_No_Suggestion_Accepted_Navigates_To_Steps()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        _stateService.PostCorrectionStep = 4;
        var cut = Render<AddressCorrection>();
        cut.Find("input[type='checkbox']").Change(true);
        cut.Find(".btn--primary").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/employer-registration/steps", nav.Uri);
    }

    /// <summary>CONTINUE does not navigate when items remain unresolved.</summary>
    [Fact]
    public void Continue_Does_Not_Navigate_When_Items_Unresolved()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        var nav = Services.GetRequiredService<NavigationManager>();
        var initialUri = nav.Uri;
        var cut = Render<AddressCorrection>();

        cut.Find(".btn--primary").Click();

        Assert.Equal(initialUri, nav.Uri);
    }

    /// <summary>CONTINUE stores PostCorrectionStep in CurrentStep before navigating.</summary>
    [Fact]
    public void Continue_Sets_CurrentStep_To_PostCorrectionStep()
    {
        _stateService.AddressCorrections = [ItemWithoutSuggestion()];
        _stateService.PostCorrectionStep = 5;
        var cut = Render<AddressCorrection>();
        cut.Find("input[type='checkbox']").Change(true);
        cut.Find(".btn--primary").Click();

        Assert.Equal(5, _stateService.CurrentStep);
    }

    /// <summary>Clicking EDIT ADDRESS navigates to the registration steps page.</summary>
    [Fact]
    public void Edit_Address_Navigates_To_Registration_Steps()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        _stateService.EditStep = 3;
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--secondary").Click();

        var nav = Services.GetRequiredService<NavigationManager>();
        Assert.EndsWith("/employer-registration/steps", nav.Uri);
    }

    /// <summary>Clicking EDIT ADDRESS stores EditStep in CurrentStep.</summary>
    [Fact]
    public void Edit_Address_Sets_CurrentStep_To_EditStep()
    {
        _stateService.AddressCorrections = [ItemWithSuggestion()];
        _stateService.EditStep = 3;
        var cut = Render<AddressCorrection>();
        cut.Find(".btn--secondary").Click();

        Assert.Equal(3, _stateService.CurrentStep);
    }

    /// <summary>
    /// When the user chooses "use corrected", the suggested address fields are copied
    /// into the original address model in-place.
    /// </summary>
    [Fact]
    public void Continue_With_Corrected_Choice_Applies_Suggestion_To_Original_Address()
    {
        var original = MakeOriginal();
        var suggested = MakeSuggested();
        _stateService.AddressCorrections = [new AddressCorrectionItem("Mailing Address", original, suggested)];
        var cut = Render<AddressCorrection>();

        cut.FindAll("input[type='radio']")[0].Change(true); // "Use corrected"
        cut.Find(".btn--primary").Click();

        Assert.Equal(suggested.AddressLine1, original.AddressLine1);
        Assert.Equal(suggested.Extension, original.Extension);
    }

    /// <summary>
    /// When the user chooses "use as entered", the original address model is not modified.
    /// </summary>
    [Fact]
    public void Continue_With_Entered_Choice_Does_Not_Modify_Original_Address()
    {
        var original = MakeOriginal();
        var savedLine1 = original.AddressLine1;
        _stateService.AddressCorrections = [new AddressCorrectionItem("Mailing Address", original, MakeSuggested())];
        var cut = Render<AddressCorrection>();

        cut.FindAll("input[type='radio']")[1].Change(true); // "Use as entered"
        cut.Find(".btn--primary").Click();

        Assert.Equal(savedLine1, original.AddressLine1);
        Assert.Null(original.Extension); // extension should not be copied
    }

    /// <summary>
    /// When PhysicalSameAsMailing is true and the mailing correction is accepted,
    /// the correction is also applied to PhysicalLocations[0].
    /// </summary>
    [Fact]
    public void Continue_Mirrors_Mailing_Correction_To_Physical_When_Flag_Set()
    {
        var mailingAddress = MakeOriginal();
        var physicalAddress = new AddressModel
        {
            AddressLine1 = mailingAddress.AddressLine1,
            City = mailingAddress.City,
            State = mailingAddress.State,
            Zip = mailingAddress.Zip,
            Country = mailingAddress.Country
        };

        var businessInfo = new BusinessInformationModel { MailingAddress = mailingAddress };
        businessInfo.PhysicalLocations[0] = physicalAddress;

        _stateService.PhysicalSameAsMailing = true;
        _stateService.BusinessInfo = businessInfo;
        _stateService.AddressCorrections =
            [new AddressCorrectionItem("Mailing Address", mailingAddress, MakeSuggested())];

        var cut = Render<AddressCorrection>();
        cut.FindAll("input[type='radio']")[0].Change(true); // accept corrected
        cut.Find(".btn--primary").Click();

        Assert.Equal("123 MAIN ST", physicalAddress.AddressLine1);
        Assert.Equal("1234", physicalAddress.Extension);
    }

    /// <summary>
    /// When PhysicalSameAsMailing is false, mailing correction is NOT mirrored to physical.
    /// </summary>
    [Fact]
    public void Continue_Does_Not_Mirror_Correction_When_Flag_Is_False()
    {
        var mailingAddress = MakeOriginal();
        var physicalAddress = new AddressModel { AddressLine1 = "456 Oak Ave", City = "Milwaukee", State = "WI", Zip = "53202", Country = "United States" };

        var businessInfo = new BusinessInformationModel { MailingAddress = mailingAddress };
        businessInfo.PhysicalLocations[0] = physicalAddress;

        _stateService.PhysicalSameAsMailing = false;
        _stateService.BusinessInfo = businessInfo;
        _stateService.AddressCorrections =
            [new AddressCorrectionItem("Mailing Address", mailingAddress, MakeSuggested())];

        var cut = Render<AddressCorrection>();
        cut.FindAll("input[type='radio']")[0].Change(true); // accept corrected mailing
        cut.Find(".btn--primary").Click();

        Assert.Equal("456 Oak Ave", physicalAddress.AddressLine1);
    }
}
