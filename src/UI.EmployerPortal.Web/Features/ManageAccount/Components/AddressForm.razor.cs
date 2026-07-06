using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs.Models;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

public partial class AddressForm : ComponentBase
{
    [Parameter, EditorRequired]
    public AddressFormModel FormModel { get; set; } = default!;

    [Parameter, EditorRequired]
    public EditContext EditContext { get; set; } = default!;

    [Parameter]
    public bool IsSaving { get; set; }

    [Parameter]
    public bool FormSubmitted { get; set; }

    [Parameter]
    public List<string> FormErrors { get; set; } = new();

    [Parameter]
    public EventCallback OnSubmit { get; set; }

    [Parameter]
    public EventCallback OnCancel { get; set; }

    // Select options
    [Parameter] public List<SelectOption> AddressTypeOptions { get; set; } = new();
    [Parameter] public List<SelectOption> CountryOptions { get; set; } = new();
    [Parameter] public List<SelectOption> StateOptions { get; set; } = new();
    [Parameter] public List<SelectOption> ProvinceOptions { get; set; } = new();

    // Bound values for selects
    [Parameter] public string? AddressTypeValue { get; set; }
    [Parameter] public EventCallback<string?> AddressTypeValueChanged { get; set; }

    [Parameter] public string? CountryValue { get; set; }
    [Parameter] public EventCallback<string?> CountryValueChanged { get; set; }

    [Parameter] public string? StateValue { get; set; }
    [Parameter] public EventCallback<string?> StateValueChanged { get; set; }

    [Parameter] public string? ProvinceValue { get; set; }
    [Parameter] public EventCallback<string?> ProvinceValueChanged { get; set; }
}
