using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Inputs;

public class OutlinedSelectFieldTests : BunitContext
{
    private static List<SelectOption> ThreeOptions =>
    [
        new() { Value = "US", Text = "United States" },
        new() { Value = "CA", Text = "Canada" },
        new() { Value = "MX", Text = "Mexico" },
    ];

    // ── Rendering ──────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Label()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Label, "Country");
            p.Add(x => x.Options, ThreeOptions);
        });

        Assert.Equal("Country", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Renders_All_Options()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
        });

        Assert.Equal(3, cut.FindAll("option").Count);
    }

    [Fact]
    public void Renders_Placeholder_As_First_Option()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Placeholder, "-- Select --");
        });

        var options = cut.FindAll("option");
        Assert.Equal(4, options.Count);
        Assert.Equal("-- Select --", options[0].TextContent.Trim());
    }

    [Fact]
    public void No_Placeholder_When_Not_Set()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
        });

        Assert.Equal(3, cut.FindAll("option").Count);
    }

    [Fact]
    public void Reflects_Provided_Value_On_Select_Element()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Value, "CA");
        });

        Assert.Equal("CA", cut.Find("select").GetAttribute("value"));
    }

    [Fact]
    public void Empty_Value_When_No_Value_Provided()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
        });

        Assert.Equal("", cut.Find("select").GetAttribute("value"));
    }

    [Fact]
    public void Is_Disabled_When_Disabled_True()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Disabled, true);
        });

        Assert.NotNull(cut.Find("select[disabled]"));
    }

    [Fact]
    public void Not_Disabled_By_Default()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
        });

        Assert.Null(cut.Find("select").GetAttribute("disabled"));
    }

    // ── Error visibility ───────────────────────────────────────────────────

    [Fact]
    public void No_Error_Class_When_Not_Visible_And_Not_Touched()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, false);
        });

        Assert.DoesNotContain("master-select-field--error", cut.Markup);
        Assert.DoesNotContain("master-select--error", cut.Markup);
    }

    [Fact]
    public void No_Error_Class_Without_EditContext()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, true);
        });

        Assert.DoesNotContain("master-select-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_Class_When_Visible_And_Field_Has_Error()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Contains("master-select-field--error", cut.Markup);
    }

    // ── Touch / blur behaviour ─────────────────────────────────────────────

    [Fact]
    public void No_Error_Before_Blur_When_Visible_False()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.DoesNotContain("master-select-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_After_Blur_Without_Visible()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        cut.Find("select").TriggerEvent("onblur", new FocusEventArgs());

        Assert.Contains("master-select-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_When_ValidationRequested_Fires()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        editCtx.Validate(); // raises OnValidationRequested → sets _touched = true

        Assert.Contains("master-select-field--error", cut.Markup);
    }

    [Fact]
    public void OnBlur_Callback_Invoked_On_Blur()
    {
        var called = false;
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.OnBlur, () => { called = true; });
        });

        cut.Find("select").TriggerEvent("onblur", new FocusEventArgs());

        Assert.True(called);
    }

    // ── Error icon and message ─────────────────────────────────────────────

    [Fact]
    public void Shows_SVG_Error_Icon_When_HasError()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.NotNull(cut.Find(".master-error svg"));
    }

    [Fact]
    public void Shows_Error_Message_Text_When_HasError()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Contains("Value is required.", cut.Find(".master-error").TextContent);
    }

    [Fact]
    public void No_Error_Div_When_No_Error()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, false);
        });

        Assert.Empty(cut.FindAll(".master-error"));
    }

    // ── ARIA attributes ────────────────────────────────────────────────────

    [Fact]
    public void AriaRequired_True_When_Required_Parameter_Set()
    {
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Required, true);
        });

        Assert.Equal("True", cut.Find("select").GetAttribute("aria-required"));
    }

    [Fact]
    public void AriaRequired_True_When_Field_Has_Required_Attribute()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Equal("True", cut.Find("select").GetAttribute("aria-required"));
    }

    [Fact]
    public void AriaDescribedBy_Includes_ErrorId_When_HasError()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Id, "test-field");
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        var describedBy = cut.Find("select").GetAttribute("aria-describedby");
        Assert.Contains("test-field-error", describedBy);
    }

    // ── ValueChanged callbacks ─────────────────────────────────────────────

    [Fact]
    public async Task Invokes_ValueChanged_On_Selection()
    {
        string? captured = null;
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "CA" });

        Assert.Equal("CA", captured);
    }

    [Fact]
    public async Task ValueChanged_Invoked_Once_Per_Change()
    {
        var callCount = 0;
        var cut = Render<OutlinedSelectField>(p =>
        {
            p.Add(x => x.Options, ThreeOptions);
            p.Add(x => x.ValueChanged, _ => { callCount++; });
        });

        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "US" });
        await cut.Find("select").ChangeAsync(new ChangeEventArgs { Value = "CA" });

        Assert.Equal(2, callCount);
    }

    private class TestModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string? RequiredField { get; set; }
    }
}
