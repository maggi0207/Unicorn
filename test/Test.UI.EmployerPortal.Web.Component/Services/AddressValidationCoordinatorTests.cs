using Bunit;
using FakeItEasy;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

namespace Test.UI.EmployerPortal.Web.Component.Services;

/// <summary>
/// Unit tests for <see cref="AddressValidationCoordinator"/>.
/// Uses <see cref="BunitContext"/> to obtain a working <see cref="NavigationManager"/>.
/// </summary>
public class AddressValidationCoordinatorTests : BunitContext
{
    private readonly IAddressValidationWrapper _fakeValidator;
    private readonly RegistrationStateService _stateService;
    private readonly AddressValidationCoordinator _coordinator;
    private readonly NavigationManager _nav;

    /// <summary>Initializes fakes and creates the coordinator under test.</summary>
    public AddressValidationCoordinatorTests()
    {
        _fakeValidator = A.Fake<IAddressValidationWrapper>();
        _stateService = new RegistrationStateService();
        _nav = Services.GetRequiredService<NavigationManager>();
        _coordinator = new AddressValidationCoordinator(_fakeValidator, _stateService, _nav);
    }

    /// <summary>Creates a labeled address entry for passing to the coordinator.</summary>
    private static (string Label, AddressModel Address) MakeEntry(string label = "Mailing Address")
        => (label, new AddressModel
        {
            AddressLine1 = "123 Main St",
            City = "Madison",
            State = "WI",
            Zip = "53701",
            Country = "United States"
        });

    /// <summary>Returns true when the validator reports all addresses as valid with no suggestions.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Returns_True_When_No_Corrections_Needed()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        var result = await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.True(result);
    }

    /// <summary>State corrections list remains empty when no corrections are needed.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Does_Not_Populate_State_When_No_Corrections()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.Empty(_stateService.AddressCorrections);
    }

    /// <summary>The NavigationManager URI is unchanged when no corrections are needed.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Does_Not_Navigate_When_No_Corrections()
    {
        var initialUri = _nav.Uri;
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.Equal(initialUri, _nav.Uri);
    }

    /// <summary>Returns false when at least one address requires correction.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Returns_False_When_Corrections_Needed()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var result = await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.False(result);
    }

    /// <summary>The corrections list in state is populated when corrections are needed.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Populates_AddressCorrections_In_State()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Not found", null));

        await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry("Mailing Address")], editStep: 3, postCorrectionStep: 4);

        Assert.Single(_stateService.AddressCorrections);
    }

    /// <summary>Multiple addresses that need correction all appear in state.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Populates_All_Corrections_From_Multiple_Addresses()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry("Mailing Address"), MakeEntry("Physical Address")],
            editStep: 3, postCorrectionStep: 4);

        Assert.Equal(2, _stateService.AddressCorrections.Count);
    }

    /// <summary>CorrectionIndex is always reset to 0 before navigation.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Resets_CorrectionIndex_To_Zero()
    {
        _stateService.CorrectionIndex = 5;
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.Equal(0, _stateService.CorrectionIndex);
    }

    /// <summary>EditStep is stored in state before navigation so AddressCorrection can redirect back.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Stores_EditStep_In_State()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 7, postCorrectionStep: 8);

        Assert.Equal(7, _stateService.EditStep);
    }

    /// <summary>PostCorrectionStep is stored in state so AddressCorrection knows where to advance.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Stores_PostCorrectionStep_In_State()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 3, postCorrectionStep: 9);

        Assert.Equal(9, _stateService.PostCorrectionStep);
    }

    /// <summary>Navigates to the address-correction page when corrections are needed.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Navigates_To_AddressCorrection_When_Corrections_Needed()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        await _coordinator.ValidateAndRedirectAsync([MakeEntry()], editStep: 3, postCorrectionStep: 4);

        Assert.EndsWith("/employer-registration/address-correction", _nav.Uri);
    }

    /// <summary>The optional onBeforeNavigate callback is invoked when corrections are needed.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Invokes_OnBeforeNavigate_When_Corrections_Needed()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var callbackInvoked = false;
        await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry()], editStep: 3, postCorrectionStep: 4,
            onBeforeNavigate: () => callbackInvoked = true);

        Assert.True(callbackInvoked);
    }

    /// <summary>The onBeforeNavigate callback is NOT invoked when all addresses pass validation.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Does_Not_Invoke_OnBeforeNavigate_When_No_Corrections()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(true, null, null));

        var callbackInvoked = false;
        await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry()], editStep: 3, postCorrectionStep: 4,
            onBeforeNavigate: () => callbackInvoked = true);

        Assert.False(callbackInvoked);
    }

    /// <summary>The callback is invoked before navigation happens.</summary>
    [Fact]
    public async Task ValidateAndRedirectAsync_Invokes_OnBeforeNavigate_Before_Navigation()
    {
        A.CallTo(() => _fakeValidator.ValidateAsync(A<AddressModel>._))
            .Returns(new AddressValidationResult(false, "Error", null));

        var callbackUri = "";
        await _coordinator.ValidateAndRedirectAsync(
            [MakeEntry()], editStep: 3, postCorrectionStep: 4,
            onBeforeNavigate: () => callbackUri = _nav.Uri);

        Assert.DoesNotContain("address-correction", callbackUri);
    }
}
