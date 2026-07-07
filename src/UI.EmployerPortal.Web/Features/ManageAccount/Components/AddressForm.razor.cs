using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

/// <summary>
/// A reusable Blazor component for entering and editing address information.
/// Encapsulates the UI form fields, validation, and layout for addresses.
/// </summary>
public partial class AddressForm : ComponentBase
{
    /// <summary>
    /// The address model containing the data bound to the form fields.
    /// </summary>
    [Parameter, EditorRequired]
    public AddressFormModel FormModel { get; set; } = default!;

    /// <summary>
    /// The EditContext used by the EditForm to track validation state and changes.
    /// </summary>
    [Parameter, EditorRequired]
    public EditContext EditContext { get; set; } = default!;

    /// <summary>
    /// Indicates whether the form is currently in a saving state.
    /// When true, disables the submit button and changes its text.
    /// </summary>
    [Parameter]
    public bool IsSaving { get; set; }

    /// <summary>
    /// Indicates whether the user has attempted to submit the form.
    /// Useful for displaying conditional validation feedback.
    /// </summary>
    [Parameter]
    public bool FormSubmitted { get; set; }

    /// <summary>
    /// A list of form-level validation errors or API errors to display in a banner.
    /// </summary>
    [Parameter]
    public List<string> FormErrors { get; set; } = new();

    /// <summary>
    /// Event triggered when the user successfully submits the valid form.
    /// </summary>
    [Parameter]
    public EventCallback OnSubmit { get; set; }

    /// <summary>
    /// Event triggered when the user clicks the cancel button.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Select options for the Address Type dropdown.
    /// </summary>
    [Parameter] public List<SelectOption> AddressTypeOptions { get; set; } = new();
    
    /// <summary>
    /// Select options for the Country dropdown.
    /// </summary>
    [Parameter] public List<SelectOption> CountryOptions { get; set; } = new();
    
    /// <summary>
    /// Select options for the State dropdown (US addresses).
    /// </summary>
    [Parameter] public List<SelectOption> StateOptions { get; set; } = new();
    
    /// <summary>
    /// Select options for the Province dropdown (Canadian addresses).
    /// </summary>
    [Parameter] public List<SelectOption> ProvinceOptions { get; set; } = new();

    private string? _addressTypeValue;
    /// <summary>
    /// The currently selected Address Type value (bound to string).
    /// </summary>
    [Parameter]
    public string? AddressTypeValue
    {
        get => _addressTypeValue;
        set
        {
            if (_addressTypeValue != value)
            {
                _addressTypeValue = value;
                AddressTypeValueChanged.InvokeAsync(value);
            }
        }
    }
    
    /// <summary>
    /// Event callback for when the Address Type value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> AddressTypeValueChanged { get; set; }

    private string? _countryValue;
    /// <summary>
    /// The currently selected Country value (bound to string).
    /// </summary>
    [Parameter]
    public string? CountryValue
    {
        get => _countryValue;
        set
        {
            if (_countryValue != value)
            {
                _countryValue = value;
                CountryValueChanged.InvokeAsync(value);
            }
        }
    }
    
    /// <summary>
    /// Event callback for when the Country value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> CountryValueChanged { get; set; }

    private string? _stateValue;
    /// <summary>
    /// The currently selected State value (bound to string).
    /// </summary>
    [Parameter]
    public string? StateValue
    {
        get => _stateValue;
        set
        {
            if (_stateValue != value)
            {
                _stateValue = value;
                StateValueChanged.InvokeAsync(value);
            }
        }
    }
    
    /// <summary>
    /// Event callback for when the State value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> StateValueChanged { get; set; }

    private string? _provinceValue;
    /// <summary>
    /// The currently selected Province value (bound to string).
    /// </summary>
    [Parameter]
    public string? ProvinceValue
    {
        get => _provinceValue;
        set
        {
            if (_provinceValue != value)
            {
                _provinceValue = value;
                ProvinceValueChanged.InvokeAsync(value);
            }
        }
    }
    
    /// <summary>
    /// Event callback for when the Province value changes.
    /// </summary>
    [Parameter] public EventCallback<string?> ProvinceValueChanged { get; set; }
}
