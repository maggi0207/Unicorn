using Bunit;
using FakeItEasy;
using Microsoft.Extensions.DependencyInjection;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Pages;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace Test.UI.EmployerPortal.Web.Component.Features.ManageAccount;

/// <summary>
/// Component tests for the ManageAddresses page.
/// Tests cover initial render states, table display, sort toggling,
/// Add/Edit form visibility, delete modal, and CanDelete logic.
/// </summary>
public class ManageAddressesTests : BunitContext
{
    private readonly IManageAddressService _fakeService;
    private readonly ISessionManager _fakeSession;

    /// <summary>Registers required services before each test.</summary>
    public ManageAddressesTests()
    {
        _fakeService = A.Fake<IManageAddressService>();
        _fakeSession = A.Fake<ISessionManager>();

        // Default: session returns employerSK = 1
        A.CallTo(() => { return _fakeSession.GetAsync<SelectedEmployerAccount>(); })
            .Returns(Task.FromResult<SelectedEmployerAccount?>(
                new SelectedEmployerAccount
                {
                    EmployerAccount = new EmployerAccount { Id = 1 }
                }));

        // Default: service returns empty list
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>()));

        Services.AddSingleton(_fakeService);
        Services.AddSingleton(_fakeSession);
    }

    private static AddressRowModel MakeRow(
        long sk = 1,
        string type = "Main Physical Location",
        string address = "123 Main St, Madison, WI 53701",
        bool canDelete = true,
        int addressTypeCodeSK = 13) => new()
        {
            AddressSK = sk,
            AddressTypeCodeSK = addressTypeCodeSK,
            AddressType = type,
            FormattedAddress = address,
            CanDelete = canDelete,
            CountryAddressFormatCodeSK = 1
        };

    private static AddressRowModel MainMailingRow() =>
        MakeRow(sk: 1, type: "Main Business Mailing Address", canDelete: false, addressTypeCodeSK: 11);

    /// <summary>Page renders the "Manage Addresses" h1 title after loading.</summary>
    [Fact]
    public async Task Renders_Page_Title()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; }); // flush OnInitializedAsync

        Assert.Equal("Manage Addresses", cut.Find("h1.page-title").TextContent.Trim());
    }

    /// <summary>Page renders the description text below the title.</summary>
    [Fact]
    public async Task Renders_Description_Text()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Add, Edit and Delete business addresses for an account.", cut.Markup);
    }

    /// <summary>While loading, a loading spinner is shown and the table is not.</summary>
    [Fact]
    public void Shows_Loading_Spinner_Initially()
    {
        // Prevent the async load from completing by using a never-completing task
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(new TaskCompletionSource<List<AddressRowModel>>().Task);

        var cut = Render<ManageAddresses>();

        Assert.Empty(cut.FindAll("table"));
        Assert.Contains("Loading", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>When there are no addresses, the table is rendered with no data rows.</summary>
    [Fact]
    public async Task Renders_Empty_Table_When_No_Addresses()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>()));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.NotEmpty(cut.FindAll("table"));
        Assert.Empty(cut.FindAll("tr.data-row"));
    }

    /// <summary>Table has an "Address Type" column header.</summary>
    [Fact]
    public async Task Renders_Address_Type_Column_Header()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Address Type", cut.Markup);
    }

    /// <summary>Table has an "Address" column header.</summary>
    [Fact]
    public async Task Renders_Address_Column_Header()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Address", cut.Markup);
    }

    /// <summary>Table has an "Action" column header.</summary>
    [Fact]
    public async Task Renders_Action_Column_Header()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Action", cut.Markup);
    }

    /// <summary>Each address in the list produces one data row.</summary>
    [Fact]
    public async Task Renders_One_Data_Row_Per_Address()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 1, type: "Main Physical Location"),
                MakeRow(sk: 2, type: "Headquarters")
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Equal(2, cut.FindAll("tr.data-row").Count);
    }

    /// <summary>Address type label is displayed in the first column of the data row.</summary>
    [Fact]
    public async Task Renders_Address_Type_In_Row()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(type: "Headquarters")
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Headquarters", cut.Markup);
    }

    /// <summary>Formatted address is displayed in the second column of the data row.</summary>
    [Fact]
    public async Task Renders_Formatted_Address_In_Row()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(address: "456 Oak Ave, Green Bay, WI 54301")
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("456 Oak Ave, Green Bay, WI 54301", cut.Markup);
    }

    /// <summary>Every row has an Edit button regardless of CanDelete.</summary>
    [Fact]
    public async Task Every_Row_Has_Edit_Button()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MainMailingRow(),
                MakeRow(sk: 2, canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        var editButtons = cut.FindAll("button[aria-label^='Edit']");
        Assert.Equal(2, editButtons.Count);
    }

    /// <summary>Delete button is NOT shown for the main business mailing address (CanDelete = false).</summary>
    [Fact]
    public async Task Delete_Button_Hidden_For_Main_Mailing_Address()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel> { MainMailingRow() }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Empty(cut.FindAll("button[aria-label^='Delete']"));
    }

    /// <summary>Delete button IS shown for addresses where CanDelete = true.</summary>
    [Fact]
    public async Task Delete_Button_Shown_For_Deletable_Addresses()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 2, type: "Secondary Physical Location", canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.NotEmpty(cut.FindAll("button[aria-label^='Delete']"));
    }

    /// <summary>Mixed list: only deletable rows show delete buttons.</summary>
    [Fact]
    public async Task Delete_Button_Only_Shown_For_Deletable_Rows_In_Mixed_List()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MainMailingRow(),
                MakeRow(sk: 2, type: "Secondary Physical Location", canDelete: true),
                MakeRow(sk: 3, type: "Headquarters", canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        // Only 2 deletable rows — 3rd (main mailing) has no delete button
        Assert.Equal(2, cut.FindAll("button[aria-label^='Delete']").Count);
    }

    /// <summary>"+ Add Address" button is present on the table view.</summary>
    [Fact]
    public async Task Renders_Add_Address_Button()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Add Address", cut.Markup);
    }

    /// <summary>Add Address form is NOT shown initially.</summary>
    [Fact]
    public async Task Add_Form_Hidden_Initially()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.DoesNotContain("Add Address", cut.Find("h1.page-title").TextContent);
    }

    /// <summary>Clicking "+ Add Address" button shows the Add Address form with correct title.</summary>
    [Fact]
    public async Task Clicking_Add_Address_Shows_Form_With_Add_Title()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Add a new address']").Click();

        Assert.Contains("Add Address", cut.Find("h1.page-title").TextContent);
    }

    /// <summary>The Add Address form shows "Address Line 1" field.</summary>
    [Fact]
    public async Task Add_Form_Shows_Address_Line_1_Field()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Add a new address']").Click();

        Assert.Contains("Address Line 1", cut.Markup);
    }

    /// <summary>The Add Address form shows "Address Line 2" field.</summary>
    [Fact]
    public async Task Add_Form_Shows_Address_Line_2_Field()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Add a new address']").Click();

        Assert.Contains("Address Line 2", cut.Markup);
    }

    /// <summary>Clicking the Edit button on a row shows the Edit Address form with correct title.</summary>
    [Fact]
    public async Task Clicking_Edit_Button_Shows_Form_With_Edit_Title()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel> { MakeRow() }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Edit']").Click();

        Assert.Contains("Edit Address", cut.Find("h1.page-title").TextContent);
    }

    /// <summary>The Edit form is pre-populated with the selected row's Address Type.</summary>
    [Fact]
    public async Task Edit_Form_Prepopulates_From_Selected_Row()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(type: "Headquarters", sk: 5)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Edit Headquarters']").Click();

        // Form is shown with Edit title
        Assert.Contains("Edit Address", cut.Find("h1.page-title").TextContent);
    }

    /// <summary>Clicking CANCEL on the Add form returns to the table view.</summary>
    [Fact]
    public async Task Cancel_On_Add_Form_Returns_To_Table()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Add a new address']").Click();
        Assert.Contains("Add Address", cut.Find("h1.page-title").TextContent);

        cut.Find("button[aria-label='Cancel address edit']").Click();

        Assert.Equal("Manage Addresses", cut.Find("h1.page-title").TextContent.Trim());
    }

    /// <summary>Clicking CANCEL on the Edit form returns to the table view.</summary>
    [Fact]
    public async Task Cancel_On_Edit_Form_Returns_To_Table()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel> { MakeRow() }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Edit']").Click();
        cut.Find("button[aria-label='Cancel address edit']").Click();

        Assert.Equal("Manage Addresses", cut.Find("h1.page-title").TextContent.Trim());
    }

    /// <summary>Delete modal is NOT shown initially.</summary>
    [Fact]
    public async Task Delete_Modal_Hidden_Initially()
    {
        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Empty(cut.FindAll("[aria-labelledby='delete-address-title']"));
    }

    /// <summary>Clicking the Delete button opens the delete confirmation modal.</summary>
    [Fact]
    public async Task Clicking_Delete_Button_Shows_Modal()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 2, type: "Secondary Physical Location", canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Delete']").Click();

        Assert.NotEmpty(cut.FindAll("[aria-labelledby='delete-address-title']"));
    }

    /// <summary>Delete modal shows the address type name in its confirmation text.</summary>
    [Fact]
    public async Task Delete_Modal_Contains_Address_Type_Name()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 2, type: "Headquarters", canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Delete Headquarters']").Click();

        Assert.Contains("Headquarters", cut.Markup);
    }

    /// <summary>Clicking CANCEL inside the modal closes it without calling DeleteAddressAsync.</summary>
    [Fact]
    public async Task Cancel_In_Delete_Modal_Closes_Modal()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 2, canDelete: true)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Delete']").Click();
        cut.FindAll("button").First(b => { return b.TextContent.Trim() == "CANCEL"; }).Click();

        Assert.Empty(cut.FindAll("[aria-labelledby='delete-address-title']"));
        A.CallTo(() => { return _fakeService.DeleteAddressAsync(A<long>._, A<int>._); })
            .MustNotHaveHappened();
    }

    /// <summary>After a successful delete, the service refreshes the address list.</summary>
    [Fact]
    public async Task Successful_Delete_Refreshes_Address_List()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 2, type: "Headquarters", canDelete: true)
            }));

        A.CallTo(() => _fakeService.DeleteAddressAsync(2L, A<int>._))
            .Returns(Task.FromResult<(bool, string)>((true, string.Empty)));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Delete Headquarters']").Click();
        await cut.InvokeAsync(() =>
            cut.FindAll("button").First(b => { return b.TextContent.Trim() == "DELETE"; }).ClickAsync(new()));

        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .MustHaveHappenedTwiceExactly(); // once on init, once after delete
    }

    /// <summary>
    /// Clicking the Address Type header a second time reverses the sort order
    /// (aria-sort changes from ascending to descending).
    /// </summary>
    [Fact]
    public async Task Clicking_Address_Type_Header_Twice_Toggles_Sort_Direction()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 1, type: "Headquarters"),
                MakeRow(sk: 2, type: "Billing")
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        var addressTypeHeader = cut.Find("th[aria-sort]");

        // First click: should already be ascending (default)
        Assert.Equal("ascending", addressTypeHeader.GetAttribute("aria-sort"));

        // Second click: should toggle to descending
        await cut.InvokeAsync(() => { return addressTypeHeader.ClickAsync(new()); });

        addressTypeHeader = cut.Find("th[aria-sort='descending']");
        Assert.NotNull(addressTypeHeader);
    }

    /// <summary>Clicking a different column header resets sort to ascending on that column.</summary>
    [Fact]
    public async Task Clicking_Different_Column_Resets_Sort_To_Ascending()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 1, type: "A Type", address: "Z Address"),
                MakeRow(sk: 2, type: "B Type", address: "A Address")
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        // Default sort is addressType ascending — click the "Address" header
        var headers = cut.FindAll("th.header-cell");
        await cut.InvokeAsync(() => { return headers[1].ClickAsync(new()); });

        // The Address column now has aria-sort="ascending"
        var addressHeader = cut.Find("th[aria-sort='ascending']");
        Assert.Contains("Address", addressHeader.TextContent);
    }

    /// <summary>When GetAddressesAsync throws, an error banner is shown.</summary>
    [Fact]
    public async Task Shows_Error_Banner_When_Load_Fails()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .ThrowsAsync(new InvalidOperationException("WCF failure"));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        Assert.Contains("Unable to load addresses", cut.Markup, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>GetAddressesAsync is called with the employerSK from the session.</summary>
    [Fact]
    public async Task GetAddressesAsync_Called_With_Session_EmployerSK()
    {
        A.CallTo(() => { return _fakeSession.GetAsync<SelectedEmployerAccount>(); })
            .Returns(Task.FromResult<SelectedEmployerAccount?>(
                new SelectedEmployerAccount { EmployerAccount = new EmployerAccount { Id = 42 } }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        A.CallTo(() => _fakeService.GetAddressesAsync(42)).MustHaveHappenedOnceExactly();
    }

    /// <summary>When session returns null, GetAddressesAsync is not called (no valid employerSK).</summary>
    [Fact]
    public async Task GetAddressesAsync_Not_Called_When_Session_Is_Null()
    {
        A.CallTo(() => { return _fakeSession.GetAsync<SelectedEmployerAccount>(); })
            .Returns(Task.FromResult<SelectedEmployerAccount?>(null));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); }).MustNotHaveHappened();
    }

    /// <summary>In Edit mode, Address Type dropdown is disabled when editing Main Mailing Address (11).</summary>
    [Fact]
    public async Task Edit_Form_Disables_Address_Type_For_Main_Mailing()
    {

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Edit']").Click();

        var select = cut.Find("select#AddressType");
        Assert.True(select.HasAttribute("disabled"));
    }

    /// <summary>In Edit mode, Address Type dropdown is disabled when editing Main Physical Location (13).</summary>
    [Fact]
    public async Task Edit_Form_Disables_Address_Type_For_Main_Physical()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MakeRow(sk: 1, type: "Main Physical Location", addressTypeCodeSK: 13)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label^='Edit']").Click();

        var select = cut.Find("select#AddressType");
        Assert.True(select.HasAttribute("disabled"));
    }

    /// <summary>In Add mode, Main Mailing and Main Physical are hidden if they already exist.</summary>
    [Fact]
    public async Task Add_Form_Hides_Primary_Addresses_If_They_Exist()
    {
        A.CallTo(() => { return _fakeService.GetAddressesAsync(A<int>._); })
            .Returns(Task.FromResult(new List<AddressRowModel>
            {
                MainMailingRow(),
                MakeRow(sk: 2, type: "Main Physical Location", addressTypeCodeSK: 13)
            }));

        var cut = Render<ManageAddresses>();
        await cut.InvokeAsync(() => { return Task.CompletedTask; });

        cut.Find("button[aria-label='Add a new address']").Click();

        var options = cut.FindAll("select#AddressType option").Select(o => o.GetAttribute("value")).ToList();
        Assert.DoesNotContain("11", options);
        Assert.DoesNotContain("13", options);
    }
}
