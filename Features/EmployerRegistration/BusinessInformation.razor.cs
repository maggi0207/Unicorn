using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration;

/// <summary>
/// Code-behind for the Business Information page (Step 3).
/// Uses EditForm + DataAnnotationsValidator for validation.
/// </summary>
public partial class BusinessInformation
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private BusinessInformationModel Model = new();
    private EditContext editContext = default!;

    /// <summary>
    /// Track whether the form has been submitted
    /// </summary>
    private bool formSubmitted = false;

    protected List<SelectOption> Countries { get; set; } = new()
    {
        new SelectOption { Value = "United States", Text = "United States" },
        new SelectOption { Value = "Canada", Text = "Canada" },
        new SelectOption { Value = "Mexico", Text = "Mexico" }
    };

    protected List<SelectOption> States { get; set; } = new()
    {
        new SelectOption { Value = "AL", Text = "Alabama" },
        new SelectOption { Value = "AK", Text = "Alaska" },
        new SelectOption { Value = "AZ", Text = "Arizona" },
        new SelectOption { Value = "AR", Text = "Arkansas" },
        new SelectOption { Value = "CA", Text = "California" },
        new SelectOption { Value = "CO", Text = "Colorado" },
        new SelectOption { Value = "CT", Text = "Connecticut" },
        new SelectOption { Value = "DE", Text = "Delaware" },
        new SelectOption { Value = "FL", Text = "Florida" },
        new SelectOption { Value = "GA", Text = "Georgia" },
        new SelectOption { Value = "HI", Text = "Hawaii" },
        new SelectOption { Value = "ID", Text = "Idaho" },
        new SelectOption { Value = "IL", Text = "Illinois" },
        new SelectOption { Value = "IN", Text = "Indiana" },
        new SelectOption { Value = "IA", Text = "Iowa" },
        new SelectOption { Value = "KS", Text = "Kansas" },
        new SelectOption { Value = "KY", Text = "Kentucky" },
        new SelectOption { Value = "LA", Text = "Louisiana" },
        new SelectOption { Value = "ME", Text = "Maine" },
        new SelectOption { Value = "MD", Text = "Maryland" },
        new SelectOption { Value = "MA", Text = "Massachusetts" },
        new SelectOption { Value = "MI", Text = "Michigan" },
        new SelectOption { Value = "MN", Text = "Minnesota" },
        new SelectOption { Value = "MS", Text = "Mississippi" },
        new SelectOption { Value = "MO", Text = "Missouri" },
        new SelectOption { Value = "MT", Text = "Montana" },
        new SelectOption { Value = "NE", Text = "Nebraska" },
        new SelectOption { Value = "NV", Text = "Nevada" },
        new SelectOption { Value = "NH", Text = "New Hampshire" },
        new SelectOption { Value = "NJ", Text = "New Jersey" },
        new SelectOption { Value = "NM", Text = "New Mexico" },
        new SelectOption { Value = "NY", Text = "New York" },
        new SelectOption { Value = "NC", Text = "North Carolina" },
        new SelectOption { Value = "ND", Text = "North Dakota" },
        new SelectOption { Value = "OH", Text = "Ohio" },
        new SelectOption { Value = "OK", Text = "Oklahoma" },
        new SelectOption { Value = "OR", Text = "Oregon" },
        new SelectOption { Value = "PA", Text = "Pennsylvania" },
        new SelectOption { Value = "RI", Text = "Rhode Island" },
        new SelectOption { Value = "SC", Text = "South Carolina" },
        new SelectOption { Value = "SD", Text = "South Dakota" },
        new SelectOption { Value = "TN", Text = "Tennessee" },
        new SelectOption { Value = "TX", Text = "Texas" },
        new SelectOption { Value = "UT", Text = "Utah" },
        new SelectOption { Value = "VT", Text = "Vermont" },
        new SelectOption { Value = "VA", Text = "Virginia" },
        new SelectOption { Value = "WA", Text = "Washington" },
        new SelectOption { Value = "WV", Text = "West Virginia" },
        new SelectOption { Value = "WI", Text = "Wisconsin" },
        new SelectOption { Value = "WY", Text = "Wyoming" }
    };

    protected override void OnInitialized()
    {
        editContext = new EditContext(Model);

        // Track validation state as user interacts
        editContext.OnFieldChanged += (_, __) =>
        {
            StateHasChanged();
        };
    }

    private void GoBack()
    {
        Nav.NavigateTo("/ownership");
    }

    private void HandleSaveQuit()
    {
        Nav.NavigateTo("/dashboard");
    }

    private void GoNext()
    {
        Nav.NavigateTo("/address-correction");
    }

    private void OnInvalid()
    {
        // Set flag to show errors only after submission attempt
        formSubmitted = true;
        // Forces UI to refresh and show validation messages
        StateHasChanged();
    }

    private void AddPhysicalLocation()
    {
        // TODO: Add additional physical location support
    }
}
