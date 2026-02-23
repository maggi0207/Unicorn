using Bunit;
using UI.EmployerPortal.Razor.SharedComponents.Address;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using Xunit;

namespace Test.UI.EmployerPortal.Razor.SharedComponents.Address;

public class AddressFieldTests : BunitContext
{
    private static AddressModel ValidAddress() => new()
    {
        Country      = "United States",
        AddressLine1 = "123 Main St",
        City         = "Madison",
        State        = "WI",
        Zip          = "53703"
    };

    // ── Rendering ─────────────────────────────────────────────────────────────

    [Fact]
    public void Renders_Country_Dropdown_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("Country", cut.Markup);
    }

    [Fact]
    public void Renders_AddressLine1_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("Address Line 1", cut.Markup);
    }

    [Fact]
    public void Renders_AddressLine2_Optional_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("Address Line 2 (Optional)", cut.Markup);
    }

    [Fact]
    public void Renders_City_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("City", cut.Markup);
    }

    [Fact]
    public void Renders_State_Dropdown_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("State", cut.Markup);
    }

    [Fact]
    public void Renders_ZipCode_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("Zip Code", cut.Markup);
    }

    [Fact]
    public void Renders_Extension_Optional_Label()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Contains("+4 (Optional)", cut.Markup);
    }

    [Fact]
    public void Renders_Two_Select_Dropdowns_Country_And_State()
    {
        var cut = Render<AddressField>(p => p
            .Add(x => x.Address, ValidAddress()));

        Assert.Equal(2, cut.FindAll("select").Count);
    }

    // ── AddressModel static data ──────────────────────────────────────────────

    [Fact]
    public void Countries_List_Has_Three_Options()
    {
        Assert.Equal(3, AddressModel.Countries.Count);
    }

    [Fact]
    public void Countries_List_Contains_United_States()
    {
        Assert.Contains(AddressModel.Countries, c => c.Value == "United States");
    }

    [Fact]
    public void Countries_List_Contains_Canada()
    {
        Assert.Contains(AddressModel.Countries, c => c.Value == "Canada");
    }

    [Fact]
    public void Countries_List_Contains_Mexico()
    {
        Assert.Contains(AddressModel.Countries, c => c.Value == "Mexico");
    }

    [Fact]
    public void States_List_Has_75_Entries()
    {
        Assert.Equal(75, AddressModel.States.Count);
    }

    [Fact]
    public void States_List_Contains_Wisconsin()
    {
        var wi = AddressModel.States.FirstOrDefault(s => s.Value == "WI");
        Assert.NotNull(wi);
        Assert.Equal("Wisconsin", wi!.Text);
    }

    [Fact]
    public void States_List_Contains_US_States()
    {
        Assert.Contains(AddressModel.States, s => s.Value == "CA");
        Assert.Contains(AddressModel.States, s => s.Value == "NY");
        Assert.Contains(AddressModel.States, s => s.Value == "TX");
    }

    [Fact]
    public void States_List_Contains_Canadian_Provinces()
    {
        Assert.Contains(AddressModel.States, s => s.Value == "ON");
        Assert.Contains(AddressModel.States, s => s.Value == "BC");
        Assert.Contains(AddressModel.States, s => s.Value == "QC");
    }

    [Fact]
    public void States_List_Contains_APO_FPO_Codes()
    {
        Assert.Contains(AddressModel.States, s => s.Value == "AA");
        Assert.Contains(AddressModel.States, s => s.Value == "AE");
        Assert.Contains(AddressModel.States, s => s.Value == "AP");
    }

    // ── Default values ────────────────────────────────────────────────────────

    [Fact]
    public void New_AddressModel_Defaults_Country_To_UnitedStates()
    {
        var address = new AddressModel();
        Assert.Equal("United States", address.Country);
    }

    [Fact]
    public void New_AddressModel_Has_Null_Required_Fields_Except_Country()
    {
        var address = new AddressModel();
        Assert.Null(address.AddressLine1);
        Assert.Null(address.City);
        Assert.Null(address.State);
        Assert.Null(address.Zip);
    }
}
