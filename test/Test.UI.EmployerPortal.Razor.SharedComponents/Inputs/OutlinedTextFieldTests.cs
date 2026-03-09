using Bunit;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Inputs;

public class OutlinedTextFieldTests : BunitContext
{
    // ── Rendering ──────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Label()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Label, "Legal Name");
        });

        Assert.Equal("Legal Name", cut.Find("label").TextContent.Trim());
    }

    [Fact]
    public void Default_Input_Type_Is_Text()
    {
        var cut = Render<OutlinedTextField>();
        Assert.Equal("text", cut.Find("input").GetAttribute("type"));
    }

    [Fact]
    public void Renders_Custom_Input_Type()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Type, "email");
        });

        Assert.Equal("email", cut.Find("input").GetAttribute("type"));
    }

    [Fact]
    public void Reflects_Provided_Value()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Value, "Test Corp");
        });

        Assert.Equal("Test Corp", cut.Find("input").GetAttribute("value"));
    }

    [Fact]
    public void Shows_Format_Hint_When_FormatText_Set()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.FormatText, "Format: name@example.com");
        });

        Assert.Contains("Format: name@example.com", cut.Markup);
    }

    [Fact]
    public void No_Format_Hint_When_FormatText_Not_Set()
    {
        var cut = Render<OutlinedTextField>();
        Assert.DoesNotContain("master-hint", cut.Markup);
    }

    [Fact]
    public void MaxLength_Attribute_Set_When_Provided()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.MaxLength, 50);
        });

        Assert.Equal("50", cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void No_MaxLength_Attribute_When_Not_Set()
    {
        var cut = Render<OutlinedTextField>();
        Assert.Null(cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void Is_Disabled_When_Disabled_True()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Disabled, true);
        });

        Assert.NotNull(cut.Find("input[disabled]"));
    }

    [Fact]
    public void Not_Disabled_By_Default()
    {
        var cut = Render<OutlinedTextField>();
        Assert.Null(cut.Find("input").GetAttribute("disabled"));
    }

    // ── Error visibility ───────────────────────────────────────────────────

    [Fact]
    public void No_Error_Class_When_Not_Visible_And_Not_Touched()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, false);
        });

        Assert.DoesNotContain("master-text-field--error", cut.Markup);
        Assert.DoesNotContain("master-input--error", cut.Markup);
    }

    [Fact]
    public void No_Error_Class_Without_EditContext()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, true);
        });

        Assert.DoesNotContain("master-text-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_Class_When_Visible_And_Field_Has_Error()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Contains("master-text-field--error", cut.Markup);
        Assert.Contains("master-input--error", cut.Markup);
    }

    [Fact]
    public void Error_Label_Class_Applied_When_HasError()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Contains("master-label--error", cut.Markup);
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

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.DoesNotContain("master-text-field--error", cut.Markup);
    }

    [Fact]
    public void Shows_Error_After_Blur_Without_Visible()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        cut.Find("input").TriggerEvent("onblur", new FocusEventArgs());

        Assert.Contains("master-text-field--error", cut.Markup);
    }

    [Fact]
    public async Task Shows_Error_When_ValidationRequested_Fires()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, false);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        // Run on the component dispatcher so bUnit processes the StateHasChanged call
        await cut.InvokeAsync(() => editCtx.Validate());

        Assert.Contains("master-text-field--error", cut.Markup);
    }

    [Fact]
    public void OnBlur_Callback_Invoked_On_Blur()
    {
        var called = false;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.OnBlur, () => { called = true; });
        });

        cut.Find("input").TriggerEvent("onblur", new FocusEventArgs());

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

        var cut = Render<OutlinedTextField>(p =>
        {
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

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        Assert.Contains("Value is required.", cut.Find(".master-error").TextContent);
    }

    [Fact]
    public void No_Error_Div_When_No_Error()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Visible, false);
        });

        Assert.Empty(cut.FindAll(".master-error"));
    }

    // ── ARIA attributes ────────────────────────────────────────────────────

    [Fact]
    public void Input_Has_AriaLabel_Matching_Label()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Label, "Email Address");
        });

        Assert.Equal("Email Address", cut.Find("input").GetAttribute("aria-label"));
    }

    [Fact]
    public void AriaDescribedBy_Includes_HintId_When_FormatText_Set()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Id, "test-field");
            p.Add(x => x.FormatText, "name@example.com");
        });

        var describedBy = cut.Find("input").GetAttribute("aria-describedby");
        Assert.Contains("test-field-hint", describedBy);
    }

    [Fact]
    public void AriaDescribedBy_Includes_ErrorId_When_HasError()
    {
        var model   = new TestModel();
        var editCtx = new EditContext(model);

        var store = new ValidationMessageStore(editCtx);
        store.Add(FieldIdentifier.Create(() => model.RequiredField), "Value is required.");
        editCtx.NotifyValidationStateChanged();

        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Id, "test-field");
            p.Add(x => x.Visible, true);
            p.Add(x => x.For, () => model.RequiredField);
            p.AddCascadingValue(editCtx);
        });

        var describedBy = cut.Find("input").GetAttribute("aria-describedby");
        Assert.Contains("test-field-error", describedBy);
    }

    // ── ValueChanged callbacks ─────────────────────────────────────────────

    [Fact]
    public async Task Invokes_ValueChanged_On_Input()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Hello" });

        Assert.Equal("Hello", captured);
    }

    [Fact]
    public async Task ValueChanged_Passes_Current_Input_Value()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Wisconsin DWD" });

        Assert.Equal("Wisconsin DWD", captured);
    }

    [Fact]
    public async Task Empty_Input_Passes_Empty_String()
    {
        string? captured = "previous";
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "" });

        Assert.Equal("", captured);
    }

    // ── Mask — maxlength ───────────────────────────────────────────────────

    [Fact]
    public void Mask_Sets_MaxLength_To_Full_Mask_Length()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-000-00");
        });

        Assert.Equal("11", cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void Mask_Overrides_Explicit_MaxLength()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "00/00/0000");
            p.Add(x => x.MaxLength, 5);
        });

        // Mask "00/00/0000" has length 10 — should win over MaxLength=5
        Assert.Equal("10", cut.Find("input").GetAttribute("maxlength"));
    }

    [Fact]
    public void No_Mask_Still_Uses_MaxLength()
    {
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.MaxLength, 12);
        });

        Assert.Equal("12", cut.Find("input").GetAttribute("maxlength"));
    }

    // ── Mask — digit placeholder (0) ───────────────────────────────────────

    [Fact]
    public async Task Mask_Digit_Formats_With_Separator()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-000-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "123456789" });

        Assert.Equal("1234-567-89", captured);
    }

    [Fact]
    public async Task Mask_Digit_Partial_Input_No_Trailing_Separator()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-000-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "1234" });

        Assert.Equal("1234", captured);
    }

    [Fact]
    public async Task Mask_Digit_Partial_Crosses_First_Separator()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-000-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "12345" });

        Assert.Equal("1234-5", captured);
    }

    [Fact]
    public async Task Mask_Digit_Rejects_Letters()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "1a2b" });

        // Letters skipped — only digits kept
        Assert.Equal("12", captured);
    }

    [Fact]
    public async Task Mask_Digit_User_Typed_Literal_Consumed()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "00-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        // User typed the dash themselves — should not be doubled
        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "12-34" });

        Assert.Equal("12-34", captured);
    }

    [Fact]
    public async Task Mask_Digit_Empty_Input_Returns_Empty()
    {
        string? captured = "previous";
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-000-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "" });

        Assert.Equal("", captured);
    }

    // ── Mask — letter placeholder (A) ──────────────────────────────────────

    [Fact]
    public async Task Mask_Letter_Accepts_Letters()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "AA-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "AB12" });

        Assert.Equal("AB-12", captured);
    }

    [Fact]
    public async Task Mask_Letter_Rejects_Digits()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "AA");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "1A2B" });

        // Digits skipped — only letters kept
        Assert.Equal("AB", captured);
    }

    [Fact]
    public async Task Mask_Letter_Preserves_Case()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "AAAA");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "aBcD" });

        Assert.Equal("aBcD", captured);
    }

    // ── Mask — alphanumeric placeholder (*) ────────────────────────────────

    [Fact]
    public async Task Mask_Alphanumeric_Accepts_Letters_And_Digits()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "**-**");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "A1B2" });

        Assert.Equal("A1-B2", captured);
    }

    [Fact]
    public async Task Mask_Alphanumeric_Rejects_Special_Characters()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "****");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "A!1@" });

        // Special chars skipped — only alphanumeric kept
        Assert.Equal("A1", captured);
    }

    // ── Mask — date formats ────────────────────────────────────────────────

    [Fact]
    public async Task Mask_Date_MmSlashDdSlashYyyy()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "00/00/0000");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "03092026" });

        Assert.Equal("03/09/2026", captured);
    }

    [Fact]
    public async Task Mask_Date_YyyyDashMmDashDd()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "0000-00-00");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "20260309" });

        Assert.Equal("2026-03-09", captured);
    }

    [Fact]
    public async Task Mask_Date_Partial_No_Trailing_Slash()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.Mask, "00/00/0000");
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "03" });

        Assert.Equal("03", captured);
    }

    // ── Mask — no regression on non-masked fields ──────────────────────────

    [Fact]
    public async Task No_Mask_InputMode_Numeric_Still_Strips_Non_Digits()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.InputMode, "numeric");
            p.Add(x => x.MaxLength, 5);
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "ab12cd3" });

        Assert.Equal("123", captured);
    }

    [Fact]
    public async Task No_Mask_Plain_Text_Passes_Through_Unchanged()
    {
        string? captured = null;
        var cut = Render<OutlinedTextField>(p =>
        {
            p.Add(x => x.ValueChanged, v => { captured = v; });
        });

        await cut.Find("input").InputAsync(new ChangeEventArgs { Value = "Hello World!" });

        Assert.Equal("Hello World!", captured);
    }

    private class TestModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string? RequiredField { get; set; }
    }
}
