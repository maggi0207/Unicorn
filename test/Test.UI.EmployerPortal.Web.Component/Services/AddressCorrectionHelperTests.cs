using FakeItEasy;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace Test.UI.EmployerPortal.Web.Component.Services;

/// <summary>
/// Unit tests for <see cref="AddressCorrectionHelper"/>.
/// Verifies the correction-item collection logic for all validation outcomes.
/// </summary>
public class AddressCorrectionHelperTests
{
    /// <summary>Creates a valid-looking address model for use in test setups.</summary>
    private static AddressModel MakeAddress(
        string line1 = "123 Main St",
        string city = "Madison",
        string state = "WI",
        string zip = "53701",
        string? extension = null)
        => new AddressModel
        {
            AddressLine1 = line1,
            City = city,
            State = state,
            Zip = zip,
            Extension = extension,
            Country = "United States"
        };

    /// <summary>When all addresses pass validation with no suggestion, the result is empty.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Empty_When_Address_Is_Valid_With_No_Suggestion()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", MakeAddress())]);

        Assert.Empty(result);
    }

    /// <summary>
    /// When the service returns a suggestion that is identical to the entered address
    /// (case-insensitive), no correction item is produced.
    /// </summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Empty_When_Suggestion_Equals_Entered()
    {
        var address = MakeAddress();
        var identicalSuggestion = MakeAddress();

        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, identicalSuggestion));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Empty(result);
    }

    /// <summary>Case-insensitive comparison: upper-cased suggestion equals entered address.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Empty_When_Suggestion_Differs_Only_In_Case()
    {
        var address = MakeAddress(line1: "123 main st", city: "madison");
        var upperSuggestion = MakeAddress(line1: "123 MAIN ST", city: "MADISON");

        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, upperSuggestion));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Empty(result);
    }

    /// <summary>An invalid address (IsValid=false) always produces a correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Correction_When_Address_Is_Invalid()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Address not found", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", MakeAddress())]);

        Assert.Single(result);
    }

    /// <summary>A valid address whose suggestion differs from the entered value produces a correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Correction_When_Suggestion_Differs_From_Entered()
    {
        var address = MakeAddress();
        var differentSuggestion = MakeAddress(extension: "1234");

        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, differentSuggestion));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Single(result);
    }

    /// <summary>The label from the input tuple is preserved on the correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Preserves_Label_In_Correction_Item()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Business Mailing Address", MakeAddress())]);

        Assert.Equal("Business Mailing Address", result[0].Label);
    }

    /// <summary>The service error message is preserved on the correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Preserves_ErrorMessage_In_Correction_Item()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Address not found", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", MakeAddress())]);

        Assert.Equal("Address not found", result[0].ErrorMessage);
    }

    /// <summary>The original entered address is preserved on the correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Preserves_Original_Address_In_Correction_Item()
    {
        var address = MakeAddress();
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Same(address, result[0].Original);
    }

    /// <summary>The suggested address returned by the service is preserved on the correction item.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Preserves_CorrectedAddress_In_Correction_Item()
    {
        var address = MakeAddress();
        var suggestion = MakeAddress(zip: "53701", extension: "9999");
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, suggestion));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Same(suggestion, result[0].Suggested);
    }

    /// <summary>ErrorMessage is null on the correction item when the service returns no error text.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Null_ErrorMessage_When_Service_Has_No_Error()
    {
        var address = MakeAddress();
        var suggestion = MakeAddress(extension: "0001");
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, suggestion));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator, [("Mailing Address", address)]);

        Assert.Null(result[0].ErrorMessage);
    }

    /// <summary>Only the addresses that need correction produce items; valid ones are excluded.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Only_Corrections_From_Mixed_Addresses()
    {
        var validAddress = MakeAddress();
        var invalidAddress = MakeAddress(line1: "999 Unknown St");

        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(validAddress))
            .Returns(new AddressValidationResult(true, null, null));
        A.CallTo(() => validator.ValidateAsync(invalidAddress))
            .Returns(new AddressValidationResult(false, "Not found", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator,
            [("Mailing Address", validAddress), ("Physical Address", invalidAddress)]);

        Assert.Single(result);
        Assert.Equal("Physical Address", result[0].Label);
    }

    /// <summary>When all addresses need correction, all are included in the result.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_All_When_All_Addresses_Need_Correction()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator,
            [("Address 1", MakeAddress()), ("Address 2", MakeAddress(line1: "456 Oak Ave"))]);

        Assert.Equal(2, result.Count);
    }

    /// <summary>An empty input produces an empty result without calling the validator.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Returns_Empty_For_Empty_Input()
    {
        var validator = A.Fake<IAddressValidationWrapper>();

        var result = await AddressCorrectionHelper.CollectCorrectionsAsync(validator, []);

        Assert.Empty(result);
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._)).MustNotHaveHappened();
    }

    /// <summary>The validator is called exactly once per address in the input list.</summary>
    [Fact]
    public async Task CollectCorrectionsAsync_Calls_Validator_Once_Per_Address()
    {
        var validator = A.Fake<IAddressValidationWrapper>();
        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        await AddressCorrectionHelper.CollectCorrectionsAsync(
            validator,
            [("A", MakeAddress()), ("B", MakeAddress(line1: "456 Oak Ave")), ("C", MakeAddress(city: "Milwaukee"))]);

        A.CallTo(() => validator.ValidateAsync(A<AddressModel>._)).MustHaveHappened(3, Times.Exactly);
    }
}
