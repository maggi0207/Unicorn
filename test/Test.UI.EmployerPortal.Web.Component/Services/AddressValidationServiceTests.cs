using FakeItEasy;
using System.ServiceModel;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;

namespace Test.UI.EmployerPortal.Web.Component.Services;

/// <summary>
/// Unit tests for <see cref="AddressValidationService"/>.
/// Verifies WCF request mapping, response parsing, and exception handling.
/// </summary>
public class AddressValidationServiceTests
{
    private readonly GeneratedClient.IAddressValidationService _fakeClient;
    private readonly AddressValidationService _sut;

    /// <summary>Initializes the fake WCF client and the service under test.</summary>
    public AddressValidationServiceTests()
    {
        _fakeClient = A.Fake<GeneratedClient.IAddressValidationService>();
        _sut = new AddressValidationService(_fakeClient);
    }

    /// <summary>Verifies the WCF request type is always set to Employer.</summary>
    [Fact]
    public async Task ValidateAsync_Sets_AddressRequestType_To_Employer()
    {
        GeneratedClient.AddressProxy? captured = null;
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .ReturnsLazily(call =>
            {
                captured = call.GetArgument<GeneratedClient.AddressProxy>(0);
                return Task.FromResult(new GeneratedClient.ValidateAddressResponse());
            });

        await _sut.ValidateAsync(new AddressModel());

        Assert.Equal(GeneratedClient.AddressRequestTypeEnum.Employer, captured!.AddressRequestType);
    }

    /// <summary>Verifies all AddressModel fields are mapped into the WCF request.</summary>
    [Fact]
    public async Task ValidateAsync_Maps_All_AddressModel_Fields_To_Request()
    {
        GeneratedClient.AddressProxy? captured = null;
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .ReturnsLazily(call =>
            {
                captured = call.GetArgument<GeneratedClient.AddressProxy>(0);
                return Task.FromResult(new GeneratedClient.ValidateAddressResponse());
            });

        var address = new AddressModel
        {
            AddressLine1 = "123 Main St",
            AddressLine2 = "Suite 4",
            City = "Madison",
            State = "WI",
            Zip = "53701",
            Extension = "1234",
            Country = "United States"
        };

        await _sut.ValidateAsync(address);

        Assert.Equal("123 Main St", captured!.LineOneAddress);
        Assert.Equal("Suite 4", captured.LineTwoAddress);
        Assert.Equal("Madison", captured.CityName);
        Assert.Equal("WI", captured.StateCode);
        Assert.Equal("53701", captured.ZipCode);
        Assert.Equal("1234", captured.ZipCodeExtension);
    }

    /// <summary>Verifies country display names are mapped to ISO codes, defaulting to US for unknown values.</summary>
    [Theory]
    [InlineData("United States", "US")]
    [InlineData("Canada", "CA")]
    [InlineData("Mexico", "MX")]
    [InlineData("Unknown Country", "US")]
    [InlineData(null, "US")]
    public async Task ValidateAsync_Maps_Country_Display_Name_To_ISO_Code(string? country, string expectedCode)
    {
        GeneratedClient.AddressProxy? captured = null;
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .ReturnsLazily(call =>
            {
                captured = call.GetArgument<GeneratedClient.AddressProxy>(0);
                return Task.FromResult(new GeneratedClient.ValidateAddressResponse());
            });

        await _sut.ValidateAsync(new AddressModel { Country = country });

        Assert.Equal(expectedCode, captured!.CountryCode);
    }

    /// <summary>Verifies IsValid is true when ErrorMessageOne is null.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_IsValid_True_When_ErrorMessageOne_Is_Null()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse { ErrorMessageOne = null }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.True(result.IsValid);
    }

    /// <summary>Verifies IsValid is true when ErrorMessageOne is empty string.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_IsValid_True_When_ErrorMessageOne_Is_Empty()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse { ErrorMessageOne = "" }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.True(result.IsValid);
    }

    /// <summary>Verifies IsValid is false when ErrorMessageOne contains an error message.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_IsValid_False_When_ErrorMessageOne_Is_Populated()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse { ErrorMessageOne = "Address not found" }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.False(result.IsValid);
    }

    /// <summary>Verifies the error message is taken from ErrorMessageOne when the address is invalid.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_ErrorMessageOne_When_Address_Is_Invalid()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                ErrorMessageOne = "Address not found",
                ErrorMessageTwo = "Secondary error"
            }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Equal("Address not found", result.ErrorMessage);
    }

    /// <summary>Verifies ErrorMessage is null when the address is valid.</summary>
    [Fact]
    public async Task ValidateAsync_ErrorMessage_Is_Null_When_Address_Is_Valid()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse { ErrorMessageOne = null }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Null(result.ErrorMessage);
    }

    /// <summary>Verifies CorrectedAddress is null when OutputAddress is null.</summary>
    [Fact]
    public async Task ValidateAsync_CorrectedAddress_Is_Null_When_OutputAddress_Is_Null()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse { OutputAddress = null }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Null(result.CorrectedAddress);
    }

    /// <summary>Verifies all OutputAddress fields are mapped to a CorrectedAddress model.</summary>
    [Fact]
    public async Task ValidateAsync_Maps_OutputAddress_Fields_To_CorrectedAddress()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy
                {
                    LineOneAddress = "123 MAIN ST",
                    LineTwoAddress = "STE 4",
                    CityName = "MADISON",
                    StateCode = "WI",
                    ZipCode = "53701",
                    ZipCodeExtension = "1234"
                }
            }));

        var result = await _sut.ValidateAsync(new AddressModel { Country = "United States" });

        Assert.NotNull(result.CorrectedAddress);
        Assert.Equal("123 MAIN ST", result.CorrectedAddress!.AddressLine1);
        Assert.Equal("STE 4", result.CorrectedAddress.AddressLine2);
        Assert.Equal("MADISON", result.CorrectedAddress.City);
        Assert.Equal("WI", result.CorrectedAddress.State);
        Assert.Equal("53701", result.CorrectedAddress.Zip);
        Assert.Equal("1234", result.CorrectedAddress.Extension);
    }

    /// <summary>
    /// Verifies the service swaps street lines when LineOneAddress is empty
    /// (a known edge case in the DWD service).
    /// </summary>
    [Fact]
    public async Task ValidateAsync_Swaps_Street_Lines_When_LineOneAddress_Is_Empty()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy
                {
                    LineOneAddress = "",
                    LineTwoAddress = "123 MAIN ST"
                }
            }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Equal("123 MAIN ST", result.CorrectedAddress!.AddressLine1);
        Assert.Null(result.CorrectedAddress.AddressLine2);
    }

    /// <summary>Verifies the street line swap also occurs when LineOneAddress is whitespace.</summary>
    [Fact]
    public async Task ValidateAsync_Swaps_Street_Lines_When_LineOneAddress_Is_Whitespace()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy
                {
                    LineOneAddress = "   ",
                    LineTwoAddress = "456 OAK AVE"
                }
            }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Equal("456 OAK AVE", result.CorrectedAddress!.AddressLine1);
        Assert.Null(result.CorrectedAddress.AddressLine2);
    }

    /// <summary>Verifies AddressLine2 is preserved when LineOneAddress is populated.</summary>
    [Fact]
    public async Task ValidateAsync_Preserves_LineTwoAddress_When_LineOneAddress_Is_Present()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy
                {
                    LineOneAddress = "123 MAIN ST",
                    LineTwoAddress = "APT 2"
                }
            }));

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Equal("123 MAIN ST", result.CorrectedAddress!.AddressLine1);
        Assert.Equal("APT 2", result.CorrectedAddress.AddressLine2);
    }

    /// <summary>
    /// Verifies CorrectedAddress.Country falls back to the input address country
    /// when OutputAddress.CountryCode is null (a known quirk of the DWD service).
    /// </summary>
    [Fact]
    public async Task ValidateAsync_Falls_Back_To_Input_Country_When_OutputAddress_CountryCode_Is_Null()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy { CountryCode = null }
            }));

        var result = await _sut.ValidateAsync(new AddressModel { Country = "Canada" });

        Assert.Equal("Canada", result.CorrectedAddress!.Country);
    }

    /// <summary>Verifies CorrectedAddress.Country uses the service-returned CountryCode when present.</summary>
    [Fact]
    public async Task ValidateAsync_Uses_OutputAddress_CountryCode_When_Present()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Returns(Task.FromResult(new GeneratedClient.ValidateAddressResponse
            {
                OutputAddress = new GeneratedClient.AddressProxy { CountryCode = "CA" }
            }));

        var result = await _sut.ValidateAsync(new AddressModel { Country = "United States" });

        Assert.Equal("CA", result.CorrectedAddress!.Country);
    }

    /// <summary>Verifies a WCF CommunicationException is caught and returns a failure result.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_Failure_On_CommunicationException()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Throws<CommunicationException>();

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.CorrectedAddress);
    }

    /// <summary>Verifies an unexpected exception is caught and returns a failure result.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_Failure_On_Unexpected_Exception()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Throws<InvalidOperationException>();

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
        Assert.Null(result.CorrectedAddress);
    }

    /// <summary>Verifies the failure error message is the expected user-friendly string.</summary>
    [Fact]
    public async Task ValidateAsync_Returns_Unavailable_Message_On_Exception()
    {
        A.CallTo(() => _fakeClient.ValidateAddressAsync(A<GeneratedClient.AddressProxy>._))
            .Throws<CommunicationException>();

        var result = await _sut.ValidateAsync(new AddressModel());

        Assert.Contains("unavailable", result.ErrorMessage, StringComparison.OrdinalIgnoreCase);
    }
}
