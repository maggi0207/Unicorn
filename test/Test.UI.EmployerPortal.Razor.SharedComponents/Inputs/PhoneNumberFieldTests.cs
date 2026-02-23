using Bunit;
using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using Xunit;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Inputs;

public class PhoneNumberFieldTests : TestContext
{
    // ── Rendering ────────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Default_Label_PhoneNumber()
    {
        var cut = RenderComponent<PhoneNumberField>();
        Assert.Equal("Phone Number", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Renders_Custom_Label()
    {
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.Label, "Work Phone"));
        Assert.Equal("Work Phone", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Input_Type_Is_Tel()
    {
        var cut = RenderComponent<PhoneNumberField>();
        Assert.Equal("tel", cut.Find("input").GetAttribute("type"));
    }

    [Fact]
    public void MaxLength_Attribute_Is_12()
    {
        var cut = RenderComponent<PhoneNumberField>();
        Assert.Equal("12", cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void Shows_Format_Hint_999_999_9999()
    {
        var cut = RenderComponent<PhoneNumberField>();
        Assert.Contains("999-999-9999", cut.Markup);
    }

    [Fact]
    public void Is_Disabled_When_Disabled_True()
    {
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.Disabled, true));
        Assert.NotNull(cut.Find("input[disabled]"));
    }

    [Fact]
    public void Is_Not_Disabled_By_Default()
    {
        var cut = RenderComponent<PhoneNumberField>();
        Assert.Null(cut.Find("input").GetAttribute("disabled"));
    }

    // ── Formatting logic ─────────────────────────────────────────────────────

    [Theory]
    [InlineData("1234567890", "123-456-7890")]   // 10 digits → full format
    [InlineData("1234567",    "123-456-7")]       // 7 digits  → 3-3-N (> 6 uses full split)
    [InlineData("1234",       "123-4")]            // 4 digits  → prefix + partial (> 3)
    [InlineData("123",        "123")]             // 3 digits  → no dash
    [InlineData("12",         "12")]              // 2 digits  → no dash
    [InlineData("",           "")]                // empty     → empty
    public async Task Formats_Digits_Correctly(string rawInput, string expected)
    {
        string? captured = null;
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.ValueChanged, v => captured = v));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = rawInput });

        Assert.Equal(expected, captured);
    }

    [Fact]
    public async Task Strips_Non_Digit_Characters()
    {
        string? captured = null;
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.ValueChanged, v => captured = v));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "(123) 456-7890" });

        Assert.Equal("123-456-7890", captured);
    }

    [Fact]
    public async Task Strips_Letters_And_Symbols()
    {
        string? captured = null;
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.ValueChanged, v => captured = v));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "abc1d2e3f4g5h6i7j8k9l0" });

        Assert.Equal("123-456-7890", captured);
    }

    [Fact]
    public async Task Truncates_At_10_Digits()
    {
        string? captured = null;
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.ValueChanged, v => captured = v));

        // 14 digits — only first 10 should be used
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "12345678901234" });

        Assert.Equal("123-456-7890", captured);
    }

    [Fact]
    public async Task ValueChanged_Callback_Is_Invoked_Once()
    {
        var callCount = 0;
        var cut = RenderComponent<PhoneNumberField>(p => p
            .Add(x => x.ValueChanged, _ => callCount++));

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "5556667777" });

        Assert.Equal(1, callCount);
    }
}
