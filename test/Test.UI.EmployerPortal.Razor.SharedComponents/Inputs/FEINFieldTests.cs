using Bunit;
using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Inputs;

public class FEINFieldTests : BunitContext
{
    [Fact]
    public void Renders_Default_Label_FEIN()
    {
        var cut = Render<FEINField>();
        Assert.Equal("FEIN", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Renders_Custom_Label()
    {
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.Label, "Federal EIN");
        });
        Assert.Equal("Federal EIN", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Input_Type_Is_Text()
    {
        var cut = Render<FEINField>();
        Assert.Equal("text", cut.Find("input").GetAttribute("type"));
    }

    [Fact]
    public void MaxLength_Attribute_Is_10()
    {
        var cut = Render<FEINField>();
        Assert.Equal("10", cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void Shows_Format_Hint_99_9999999()
    {
        var cut = Render<FEINField>();
        Assert.Contains("99-9999999", cut.Markup);
    }

    [Fact]
    public void Reflects_Provided_Value()
    {
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.Value, "12-3456789");
        });
        Assert.Equal("12-3456789", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void Is_Disabled_When_Disabled_True()
    {
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.Disabled, true);
        });
        Assert.NotNull(cut.Find("input[disabled]"));
    }

    [Fact]
    public void Is_Not_Disabled_By_Default()
    {
        var cut = Render<FEINField>();
        Assert.Null(cut.Find("input").GetAttribute("disabled"));
    }

    [Theory]
    [InlineData("123456789", "12-3456789")]
    [InlineData("1234",      "12-34")]
    [InlineData("12",        "12")]
    [InlineData("1",         "1")]
    [InlineData("",          "")]
    public async Task Formats_Digits_Correctly(string rawInput, string expected)
    {
        string? captured = null;
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = rawInput });

        Assert.Equal(expected, captured);
    }

    [Fact]
    public async Task Strips_Non_Digit_Characters()
    {
        string? captured = null;
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "12-3456789" });

        Assert.Equal("12-3456789", captured);
    }

    [Fact]
    public async Task Strips_Letters_And_Symbols()
    {
        string? captured = null;
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "ab1c2de3f4g5h6i7j8k9" });

        Assert.Equal("12-3456789", captured);
    }

    [Fact]
    public async Task Truncates_At_9_Digits()
    {
        string? captured = null;
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "123456789012" });

        Assert.Equal("12-3456789", captured);
    }

    [Fact]
    public async Task ValueChanged_Callback_Is_Invoked_Once()
    {
        var callCount = 0;
        var cut = Render<FEINField>(p =>
        {
            p.Add(x => x.ValueChanged, _ => { callCount++; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "123456789" });

        Assert.Equal(1, callCount);
    }
}
