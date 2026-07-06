using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// Manage Addresses page — allows employers to view, add, edit and delete their addresses.
/// </summary>
public partial class ManageAddresses
{
    [Inject] private IManageAddressService ManageAddressService { get; set; } = default!;
    [Inject] private ISessionManager SessionManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    private int _employerSK;
    private List<AddressRowModel> _addresses = [];
    private bool _isLoading = true;
    private bool _loadError = false;
    private string? _successMessage;
    private string? _errorMessage;
    private string _bannerKey = Guid.NewGuid().ToString();

    private bool _showForm = false;
    private bool _isEditMode = false;
    private bool _isSaving = false;
    private bool _formSubmitted = false;
    private List<string> _formErrors = [];

    private AddressFormModel _formModel = new();
    private EditContext? _editContext;

    // String-bound select helpers (OutlinedSelectField binds to string)
    private string _countryValue = "1";
    private string _addressTypeValue = string.Empty;
    private string _stateValue = string.Empty;
    private string _provinceValue = string.Empty;

    private bool _showDeleteModal = false;
    private bool _isDeleting = false;
    private string? _deleteError;
    private AddressRowModel? _addressToDelete;

    private string _sortColumn = "addressType";
    private bool _sortAscending = true;

    private IEnumerable<AddressRowModel> SortedAddresses
    {
        get
        {
            return _sortColumn switch
            {
                "address" => SortBy(_addresses, r => { return r.FormattedAddress; }),
                _ => SortBy(_addresses, r => { return r.AddressType; })
            };
        }
    }

    private IOrderedEnumerable<AddressRowModel> SortBy<TKey>(
        List<AddressRowModel> source, Func<AddressRowModel, TKey> keySelector)
    {
        return _sortAscending
            ? source.OrderBy(keySelector)
            : source.OrderByDescending(keySelector);
    }

    private void Sort(string column)
    {
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
    }

    private MarkupString GetSortIcon(string column)
    {
        string path;
        string altText;

        if (_sortColumn == column)
        {
            path = _sortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _sortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }

    private readonly List<SelectOption> _countryOptions =
    [
        new SelectOption { Value = "1", Text = "United States" },
        new SelectOption { Value = "2", Text = "Canada" },
        new SelectOption { Value = "3", Text = "Other International" }
    ];

    /// <summary>
    /// Address type options — AddressTypeCodeSK values.
    /// TODO: Confirm actual SKs with backend team.
    /// </summary>
    private readonly List<SelectOption> _addressTypeOptions =
    [
        new SelectOption { Value = "1",  Text = "Main Business Mailing Address" },
        new SelectOption { Value = "2",  Text = "Main Physical Location" },
        new SelectOption { Value = "3",  Text = "Secondary Physical Location" },
        new SelectOption { Value = "4",  Text = "Additional Physical Location" },
        new SelectOption { Value = "5",  Text = "Headquarters" },
        new SelectOption { Value = "6",  Text = "Business Records" }
    ];

    private readonly List<SelectOption> _stateOptions =
    [
        new SelectOption { Value = "1",  Text = "AL" }, new SelectOption { Value = "2",  Text = "AK" },
        new SelectOption { Value = "4",  Text = "AZ" }, new SelectOption { Value = "5",  Text = "AR" },
        new SelectOption { Value = "6",  Text = "CA" }, new SelectOption { Value = "7",  Text = "CO" },
        new SelectOption { Value = "8",  Text = "CT" }, new SelectOption { Value = "9",  Text = "DE" },
        new SelectOption { Value = "10", Text = "DC" }, new SelectOption { Value = "12", Text = "FL" },
        new SelectOption { Value = "13", Text = "GA" }, new SelectOption { Value = "15", Text = "HI" },
        new SelectOption { Value = "16", Text = "ID" }, new SelectOption { Value = "17", Text = "IL" },
        new SelectOption { Value = "18", Text = "IN" }, new SelectOption { Value = "19", Text = "IA" },
        new SelectOption { Value = "20", Text = "KS" }, new SelectOption { Value = "21", Text = "KY" },
        new SelectOption { Value = "22", Text = "LA" }, new SelectOption { Value = "23", Text = "ME" },
        new SelectOption { Value = "25", Text = "MD" }, new SelectOption { Value = "26", Text = "MA" },
        new SelectOption { Value = "27", Text = "MI" }, new SelectOption { Value = "28", Text = "MN" },
        new SelectOption { Value = "29", Text = "MS" }, new SelectOption { Value = "30", Text = "MO" },
        new SelectOption { Value = "31", Text = "MT" }, new SelectOption { Value = "32", Text = "NE" },
        new SelectOption { Value = "33", Text = "NV" }, new SelectOption { Value = "34", Text = "NH" },
        new SelectOption { Value = "35", Text = "NJ" }, new SelectOption { Value = "36", Text = "NM" },
        new SelectOption { Value = "37", Text = "NY" }, new SelectOption { Value = "38", Text = "NC" },
        new SelectOption { Value = "39", Text = "ND" }, new SelectOption { Value = "41", Text = "OH" },
        new SelectOption { Value = "42", Text = "OK" }, new SelectOption { Value = "43", Text = "OR" },
        new SelectOption { Value = "45", Text = "PA" }, new SelectOption { Value = "47", Text = "RI" },
        new SelectOption { Value = "48", Text = "SC" }, new SelectOption { Value = "49", Text = "SD" },
        new SelectOption { Value = "50", Text = "TN" }, new SelectOption { Value = "51", Text = "TX" },
        new SelectOption { Value = "52", Text = "UT" }, new SelectOption { Value = "53", Text = "VT" },
        new SelectOption { Value = "55", Text = "VA" }, new SelectOption { Value = "56", Text = "WA" },
        new SelectOption { Value = "57", Text = "WV" }, new SelectOption { Value = "58", Text = "WI" },
        new SelectOption { Value = "59", Text = "WY" }
    ];

    private readonly List<SelectOption> _provinceOptions =
    [
        new SelectOption { Value = "60", Text = "AB" }, new SelectOption { Value = "61", Text = "BC" },
        new SelectOption { Value = "62", Text = "MB" }, new SelectOption { Value = "63", Text = "NB" },
        new SelectOption { Value = "64", Text = "NL" }, new SelectOption { Value = "65", Text = "NT" },
        new SelectOption { Value = "66", Text = "NS" }, new SelectOption { Value = "67", Text = "NU" },
        new SelectOption { Value = "68", Text = "ON" }, new SelectOption { Value = "69", Text = "PE" },
        new SelectOption { Value = "70", Text = "QC" }, new SelectOption { Value = "71", Text = "SK" },
        new SelectOption { Value = "72", Text = "YT" }
    ];

    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        try
        {
            var session = await SessionManager.GetAsync<SelectedEmployerAccount>();
            _employerSK = session?.EmployerAccount?.Id ?? 0;

            if (_employerSK > 0)
            {
                _addresses = await ManageAddressService.GetAddressesAsync(_employerSK);
            }
        }
        catch
        {
            _loadError = true;
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void HandleAddClick()
    {
        _isEditMode = false;
        _formModel = new AddressFormModel { CountryAddressFormatCodeSK = 1 };
        _countryValue = "1";
        _addressTypeValue = string.Empty;
        _stateValue = string.Empty;
        _provinceValue = string.Empty;
        _formErrors = [];
        _formSubmitted = false;
        _editContext = new EditContext(_formModel);
        _showForm = true;
        _errorMessage = null;
        _successMessage = null;
    }

    private void HandleEditClick(AddressRowModel row)
    {
        _isEditMode = true;
        _formModel = new AddressFormModel
        {
            AddressSK = row.AddressSK,
            AddressTypeCodeSK = row.AddressTypeCodeSK,
            CountryAddressFormatCodeSK = row.CountryAddressFormatCodeSK,
            LineOneAddress = row.LineOneAddress,
            LineTwoAddress = row.LineTwoAddress,
            CityName = row.CityName,
            StateCodeSK = row.StateCodeSK,
            ZipCode = row.ZipCode,
            ZipExtension = row.ZipExtensionCode,
            CanadianPostalCode = row.CanadianPostalCode,
            ProvinceCodeSK = row.StateCodeSK,
            LineThreeAddress = row.LineThreeAddress,
            LineFourAddress = row.LineFourAddress,
            CountyName = row.CountyName
        };

        _countryValue = row.CountryAddressFormatCodeSK.ToString();
        _addressTypeValue = row.AddressTypeCodeSK.ToString();

        if (row.StateCodeSK.HasValue)
        {
            _stateValue = row.StateCodeSK.Value.ToString();
            _provinceValue = row.StateCodeSK.Value.ToString();
        }
        else
        {
            _stateValue = string.Empty;
            _provinceValue = string.Empty;
        }
        _formErrors = [];
        _formSubmitted = false;
        _editContext = new EditContext(_formModel);
        _showForm = true;
        _errorMessage = null;
        _successMessage = null;
    }

    private void HandleDeleteClick(AddressRowModel row)
    {
        _addressToDelete = row;
        _deleteError = null;
        _showDeleteModal = true;
    }

    private async Task HandleFormSubmitAsync()
    {
        _formSubmitted = true;
        _formErrors = [];

        // Sync string-bound selects back to the model before validation
        _formModel.CountryAddressFormatCodeSK = 1;
        if (int.TryParse(_countryValue, out var c))
        {
            _formModel.CountryAddressFormatCodeSK = c;
        }

        _formModel.AddressTypeCodeSK = 0;
        if (int.TryParse(_addressTypeValue, out var at))
        {
            _formModel.AddressTypeCodeSK = at;
        }

        _formModel.StateCodeSK = null;
        if (int.TryParse(_stateValue, out var st))
        {
            _formModel.StateCodeSK = st;
        }

        _formModel.ProvinceCodeSK = null;
        if (int.TryParse(_provinceValue, out var pr))
        {
            _formModel.ProvinceCodeSK = pr;
        }

        if (!_editContext!.Validate())
        {
            _formErrors = _editContext.GetValidationMessages().ToList();
            return;
        }

        _isSaving = true;
        StateHasChanged();

        try
        {
            var (success, error) = await ManageAddressService.SaveAddressAsync(_formModel, _employerSK);

            if (success)
            {
                _addresses = await ManageAddressService.GetAddressesAsync(_employerSK);

                if (_isEditMode)
                {
                    _successMessage = "Address updated successfully.";
                }
                else
                {
                    _successMessage = "Address added successfully.";
                }

                _bannerKey = Guid.NewGuid().ToString();
                _showForm = false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    _formErrors = ["Unable to save address. Please try again."];
                }
                else
                {
                    _formErrors = [error];
                }
            }
        }
        catch
        {
            _formErrors = ["An unexpected error occurred. Please try again."];
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void HandleFormCancel()
    {
        _showForm = false;
        _formErrors = [];
    }

    private void CloseDeleteModal()
    {
        if (_isDeleting)
        {
            return;
        }
        _showDeleteModal = false;
        _deleteError = null;
    }

    private async Task ConfirmDeleteAsync()
    {
        if (_addressToDelete == null)
        {
            return;
        }

        _isDeleting = true;
        _deleteError = null;

        try
        {
            var (success, error) = await ManageAddressService.DeleteAddressAsync(
                _addressToDelete.AddressSK, _employerSK);

            if (success)
            {
                _addresses = await ManageAddressService.GetAddressesAsync(_employerSK);
                _successMessage = $"{_addressToDelete.AddressType} address deleted successfully.";
                _bannerKey = Guid.NewGuid().ToString();
                _showDeleteModal = false;
            }
            else
            {
                if (string.IsNullOrWhiteSpace(error))
                {
                    _deleteError = "Unable to delete the address. Please try again.";
                }
                else
                {
                    _deleteError = error;
                }
            }
        }
        catch
        {
            _deleteError = "An unexpected error occurred. Please try again.";
        }
        finally
        {
            _isDeleting = false;
        }
    }

    private void HandleCancel()
    {
        NavigationManager.NavigateTo("manage-account/account-details");
    }
}
