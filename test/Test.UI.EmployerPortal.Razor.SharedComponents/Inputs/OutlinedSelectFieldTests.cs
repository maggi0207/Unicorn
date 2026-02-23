using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using Xunit;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Inputs;

public class OutlinedSelectFieldTests : BunitContext
{
    private static List<SelectOption> ThreeOptions =>
    [
        new() { Value = "US", Text = "United States" },
        new() { Value = "CA", Text = "Canada" },
        new() { Value = "MX", Text = "Mexico" },
    ];

    // ── Rendering ────────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Label()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Label, "Country")
            .Add(x => x.Options, ThreeOptions));

        Assert.Equal("Country", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Renders_All_Options()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions));

        Assert.Equal(3, cut.FindAll("option").Count);
    }

    [Fact]
    public void Renders_Placeholder_As_First_Option()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Placeholder, "-- Select --"));

        var options = cut.FindAll("option");
        Assert.Equal(4, options.Count);                         // placeholder + 3
        Assert.Equal("-- Select --", options[0].TextContent.Trim());
    }

    [Fact]
    public void No_Placeholder_When_Not_Set()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions));

        Assert.Equal(3, cut.FindAll("option").Count);
    }

    [Fact]
    public void Reflects_Provided_Value_On_Select_Element()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Value, "CA"));

        Assert.Equal("CA", cut.Find("select").GetAttribute("value"));
    }

    [Fact]
    public void Empty_Value_When_No_Value_Provided()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions));

        Assert.Equal("", cut.Find("select").GetAttribute("value"));
    }

    [Fact]
    public void Is_Disabled_When_Disabled_True()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Disabled, true));

        Assert.NotNull(cut.Find("select[disabled]"));
    }

    [Fact]
    public void Not_Disabled_By_Default()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions));

        Assert.Null(cut.Find("select").GetAttribute("disabled"));
    }

    // ── Error styling ─────────────────────────────────────────────────────────

    [Fact]
    public void No_Error_Class_When_Visible_False()
    {
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Visible, false));

        Assert.DoesNotContain("master-select-field--error", cut.Markup);
        Assert.DoesNotContain("master-select--error", cut.Markup);
    }

    [Fact]
    public void No_Error_Class_Without_EditContext()
    {
        // No cascaded EditContext — HasError must be false even with Visible=true
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Visible, true));

        Assert.DoesNotContain("master-select-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_Class_When_Field_Has_Validation_Error()
    {
        var model    = new TestModel();
        var editCtx  = new EditContext(model);

        // Manually inject a validation message into the EditContext
        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField!), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.Visible, true)
            .Add(x => x.For, () => model.RequiredField!)
            .AddCascadingValue(editCtx));

        Assert.Contains("master-select-field--error", cut.Markup);
    }

    // ── Events ────────────────────────────────────────────────────────────────

    [Fact]
    public async Task Invokes_ValueChanged_On_Selection()
    {
        string? captured = null;
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.ValueChanged, v => captured = v));

        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "CA" });

        Assert.Equal("CA", captured);
    }

    [Fact]
    public async Task ValueChanged_Invoked_Once_Per_Change()
    {
        var callCount = 0;
        var cut = Render<OutlinedSelectField>(p => p
            .Add(x => x.Options, ThreeOptions)
            .Add(x => x.ValueChanged, _ => callCount++));

        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "US" });
        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "CA" });

        Assert.Equal(2, callCount);
    }

    // ── Helper ────────────────────────────────────────────────────────────────

    private class TestModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string? RequiredField { get; set; }
    }
}
