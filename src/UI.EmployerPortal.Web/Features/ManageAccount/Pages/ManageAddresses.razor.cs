using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;
using UI.EmployerPortal.Generated.ServiceClients.AccountSummaryService;
using UI.EmployerPortal.Web.Features.Dashboard;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Pages;

/// <summary>
/// Manage Addresses page — allows employers to view, add, edit and delete their addresses.
/// </summary>
public partial class ManageAddresses
{
    [Inject] private IManageAddressService ManageAddressService { get; set; } = default!;
    [Inject] private ISessionManager SessionManager { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;
    [Inject] private IAddressValidationWrapper AddressValidator { get; set; } = default!;
    [Inject] private IAccountSummaryService AccountSummaryService { get; set; } = default!;
    [Inject] private IDashboardOrchestrator DashboardOrchestrator { get; set; } = default!;

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

    /// <summary>
    /// Tracks the property names associated with validation errors, allowing the error banner to navigate to fields on click.
    /// </summary>
    private List<string> _formFieldIds = [];

    private AddressFormModel _formModel = new();
    private EditContext? _editContext;

    // Address Correction State
    private bool _showCorrection = false;
    private List<UI.EmployerPortal.Web.Features.EmployerRegistration.Services.AddressCorrectionItem> _corrections = new();

    private bool _showDeleteModal = false;
    private bool _isDeleting = false;
    private string? _deleteError;
    private AddressRowModel? _addressToDelete;

    private string _sortColumn = "addressType";
    private bool _sortAscending = true;

    /// <summary>
    /// Indicates whether the current employer has a Power of Attorney on file.
    /// When true, editing the Main Business Mailing address is blocked.
    /// </summary>
    private bool _hasPOA = false;

    /// <summary>
    /// Controls visibility of the POA restriction modal shown when an employer
    /// with a POA on file attempts to edit the Main Business Mailing address.
    /// </summary>
    private bool _showPoaModal = false;

    /// <summary>
    /// The address type SK for Main Business Mailing — always pinned as the first row in the table.
    /// </summary>
    private const int MainBusinessMailingSK = 11;

    private IEnumerable<AddressRowModel> SortedAddresses
    {
        get
        {
            // Separate Main Business Mailing from the rest
            AddressRowModel? mailingRow = null;
            var otherRows = new List<AddressRowModel>();

            foreach (var address in _addresses)
            {
                if (address.AddressTypeCodeSK == MainBusinessMailingSK)
                {
                    mailingRow = address;
                }
                else
                {
                    otherRows.Add(address);
                }
            }

            // Sort the non-mailing rows by the selected column
            if (_sortColumn == "address")
            {
                otherRows.Sort(CompareByAddress);
            }
            else
            {
                otherRows.Sort(CompareByAddressType);
            }

            // Prepend mailing row (if present) so it is always first
            var result = new List<AddressRowModel>();
            if (mailingRow != null)
            {
                result.Add(mailingRow);
            }
            foreach (var row in otherRows)
            {
                result.Add(row);
            }

            return result;
        }
    }

    /// <summary>
    /// Compares two address rows by their formatted address for sorting.
    /// </summary>
    private int CompareByAddress(AddressRowModel x, AddressRowModel y)
    {
        var result = string.Compare(x.FormattedAddress, y.FormattedAddress, StringComparison.OrdinalIgnoreCase);
        if (!_sortAscending)
        {
            result = -result;
        }
        return result;
    }

    /// <summary>
    /// Compares two address rows by their address type for sorting.
    /// </summary>
    private int CompareByAddressType(AddressRowModel x, AddressRowModel y)
    {
        var result = string.Compare(x.AddressType, y.AddressType, StringComparison.OrdinalIgnoreCase);
        if (!_sortAscending)
        {
            result = -result;
        }
        return result;
    }

    /// <summary>
    /// Address type is always disabled in edit mode — the type cannot be changed once an address is saved.
    /// </summary>
    private bool IsAddressTypeDisabled => _isEditMode;

    /// <summary>
    /// Address type SK for Additional Physical Location — the only type that allows multiple entries.
    /// All other address types are unique and hidden from the dropdown once added.
    /// </summary>
    private const int AdditionalPhysicalLocationSK = 20;

    private List<SelectOption> AvailableAddressTypeOptions
    {
        get
        {
            var options = _allAddressTypeOptions.ToList();

            // Collect all address type SKs already in use by a different address row
            var usedTypes = new HashSet<int>();
            foreach (var address in _addresses)
            {
                if (address.AddressSK != _formModel.AddressSK)
                {
                    usedTypes.Add(address.AddressTypeCodeSK);
                }
            }

            // Remove any already-used type EXCEPT Additional Physical Location (20),
            // which is allowed to have multiple entries
            var toRemove = new List<SelectOption>();
            foreach (var option in options)
            {
                if (int.TryParse(option.Value, out var sk) &&
                    sk != AdditionalPhysicalLocationSK &&
                    usedTypes.Contains(sk))
                {
                    toRemove.Add(option);
                }
            }
            foreach (var option in toRemove)
            {
                options.Remove(option);
            }

            return options;
        }
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
    private readonly List<SelectOption> _allAddressTypeOptions =
    [
        new SelectOption { Value = "11", Text = "Main Business Mailing Address" },
        new SelectOption { Value = "13", Text = "Main Physical Location" },
        new SelectOption { Value = "19", Text = "Secondary Physical Location" },
        new SelectOption { Value = "20", Text = "Additional Physical Location" },
        new SelectOption { Value = "7",  Text = "Headquarters" },
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

                // Fetch the full employer record to check the POA indicator
                var employer = await DashboardOrchestrator.GetSelectedEmployerAccountAsync();
                if (employer != null)
                {
                    var request = new EmployerRequest
                    {
                        EmployerSK = employer.Id,
                        SecureUserSK = 0
                    };
                    var fullEmployer = await AccountSummaryService.GetEmployerAsync(request);
                    if (fullEmployer != null && fullEmployer.Employer != null)
                    {
                        _hasPOA = fullEmployer.Employer.PoaIndicator == true;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading addresses: {ex}");
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
        _formErrors = [];
        _formFieldIds = [];
        _formSubmitted = false;
        _editContext = new EditContext(_formModel);
        _showForm = true;
        _showCorrection = false;
        _errorMessage = null;
        _successMessage = null;
    }

    private void HandleEditClick(AddressRowModel row)
    {
        // If the employer has a POA on file, block editing the Main Business Mailing address
        // and show the POA information modal instead.
        if (row.AddressTypeCodeSK == MainBusinessMailingSK && _hasPOA)
        {
            _showPoaModal = true;
            return;
        }

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
            CountyName = row.CountyName,
            CountryString = row.CountryAddressFormatCodeSK.ToString(),
            AddressTypeString = row.AddressTypeCodeSK.ToString(),
            StateString = row.StateCodeSK?.ToString(),
            ProvinceString = row.StateCodeSK?.ToString()
        };

        _formErrors = [];
        _formFieldIds = [];
        _formSubmitted = false;
        _editContext = new EditContext(_formModel);
        _showForm = true;
        _showCorrection = false;
        _errorMessage = null;
        _successMessage = null;
    }

    /// <summary>
    /// Closes the POA restriction modal.
    /// </summary>
    private void ClosePoaModal()
    {
        _showPoaModal = false;
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
        _formFieldIds = [];

        if (!_editContext!.Validate())
        {
            var properties = new[]
            {
                nameof(AddressFormModel.AddressTypeString),
                nameof(AddressFormModel.CountryString),
                nameof(AddressFormModel.LineOneAddress),
                nameof(AddressFormModel.LineTwoAddress),
                nameof(AddressFormModel.CityName),
                nameof(AddressFormModel.StateString),
                nameof(AddressFormModel.ProvinceString),
                nameof(AddressFormModel.ZipCode),
                nameof(AddressFormModel.CanadianPostalCode),
                nameof(AddressFormModel.LineThreeAddress),
                nameof(AddressFormModel.LineFourAddress)
            };

            foreach (var prop in properties)
            {
                var fi = new FieldIdentifier(_formModel, prop);
                foreach (var error in _editContext.GetValidationMessages(fi))
                {
                    _formErrors.Add(error);
                    
                    var fieldId = prop switch
                    {
                        nameof(AddressFormModel.AddressTypeString) => "AddressType",
                        nameof(AddressFormModel.CountryString) => "AddressCountry",
                        nameof(AddressFormModel.StateString) => "StateCodeSK",
                        nameof(AddressFormModel.ProvinceString) => "ProvinceCodeSK",
                        _ => prop
                    };
                    _formFieldIds.Add(fieldId);
                }
            }
            return;
        }


        _isSaving = true;
        StateHasChanged();

        try
        {
            // Map form model to AddressModel for validation
            var addressModel = new AddressModel
            {
                AddressLine1 = _formModel.LineOneAddress,
                AddressLine2 = _formModel.LineTwoAddress,
                City = _formModel.CityName,
                State = _stateOptions.FirstOrDefault(s => { return s.Value == _formModel.StateString; })?.Text,
                Zip = _formModel.ZipCode,
                Extension = _formModel.ZipExtension,
                Country = _countryOptions.FirstOrDefault(c => { return c.Value == _formModel.CountryString; })?.Text
            };

            var label = _allAddressTypeOptions.FirstOrDefault(a => { return a.Value == _formModel.AddressTypeString; })?.Text ?? "Address";

            var corrections = await AddressCorrectionHelper.CollectCorrectionsAsync(AddressValidator, [(label, addressModel)]);

            if (corrections.Any())
            {
                _corrections = corrections;
                _showCorrection = true;
                _showForm = false;
                return;
            }

            await ProceedToSaveAsync();
        }
        catch
        {
            _formErrors = ["An unexpected error occurred during validation. Please try again."];
        }
        finally
        {
            _isSaving = false;
        }
    }

    private void HandleCorrectionEdit()
    {
        _showCorrection = false;
        _showForm = true;
    }

    private async Task HandleCorrectionContinue(List<UI.EmployerPortal.Web.Features.EmployerRegistration.Services.AddressCorrectionItem> finalizedCorrections)
    {
        _isSaving = true;
        StateHasChanged();

        try
        {
            // Map the finalized correction back to the form model
            if (finalizedCorrections.FirstOrDefault() is { } item)
            {
                _formModel.LineOneAddress = item.Original.AddressLine1 ?? string.Empty;
                _formModel.LineTwoAddress = item.Original.AddressLine2;
                _formModel.CityName = item.Original.City ?? string.Empty;
                _formModel.ZipCode = item.Original.Zip ?? string.Empty;
                _formModel.ZipExtension = item.Original.Extension;
                // State and Country drop-downs are assumed correct or we'd map them back by text matching.
            }

            await ProceedToSaveAsync();
        }
        finally
        {
            _isSaving = false;
            _showCorrection = false;
        }
    }

    private async Task ProceedToSaveAsync()
    {
        try
        {
            if (_isEditMode && _formModel.AddressSK.HasValue && _formModel.AddressTypeCodeSK != 11)
            {
                var (removeSuccess, removeError) = await ManageAddressService.DeleteAddressAsync(_formModel.AddressSK.Value, _employerSK);
                if (!removeSuccess)
                {
                    _formErrors = [string.IsNullOrWhiteSpace(removeError) ? "Unable to update address. Please try again." : removeError];
                    _showForm = true;
                    return;
                }
            }

            var (success, error) = await ManageAddressService.SaveAddressAsync(_formModel, _employerSK);

            if (success)
            {
                _addresses = await ManageAddressService.GetAddressesAsync(_employerSK);

                _successMessage = _isEditMode ? "Address updated successfully." : "Address added successfully.";

                _bannerKey = Guid.NewGuid().ToString();
                _showForm = false;
            }
            else
            {
                _formErrors = [string.IsNullOrWhiteSpace(error) ? "Unable to save address. Please try again." : error];
                _showForm = true; // Show form again to display error
            }
        }
        catch
        {
            _formErrors = ["An unexpected error occurred. Please try again."];
            _showForm = true;
        }
    }

    private void HandleFormCancel()
    {
        _showForm = false;
        _formErrors = [];
        _formFieldIds = [];
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
                _deleteError = string.IsNullOrWhiteSpace(error) 
                    ? "Unable to delete the address. Please try again." 
                    : error;
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
