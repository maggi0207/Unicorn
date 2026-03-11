using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Scoped service that carries transient navigation state between registration wizard pages.
/// Holds the list of address corrections to resolve and tracks which one is currently displayed.
/// </summary>
public class RegistrationStateService
{
    /// <summary>The model saved before navigating to address correction, so it can be restored on edit.</summary>
    public BusinessInformationModel? BusinessInfo { get; set; }

    /// <summary>Address corrections collected after service validation on BusinessInformation submit.</summary>
    public List<AddressCorrectionItem> AddressCorrections { get; set; } = new();

    /// <summary>Index of the correction currently being shown on the AddressCorrection page.</summary>
    public int CorrectionIndex { get; set; }

    /// <summary>
    /// True when the user checked "Physical Location is the same as the Business address" on the
    /// Business Information page. Tells AddressCorrection to mirror any mailing correction to
    /// PhysicalLocations[0] as well.
    /// </summary>
    public bool PhysicalSameAsMailing { get; set; }

    /// <summary>
    /// The wizard step to restore when navigating back to the EmployerRegistrationSteps page
    /// (e.g., after completing the Address Correction flow). Zero means no restoration needed.
    /// </summary>
    public int CurrentStep { get; set; }
}

/// <summary>
/// Represents one address that needs user review on the Address Correction page.
/// </summary>
/// <param name="Label">Display name shown as the section header, e.g. "Business Mailing Address".</param>
/// <param name="Original">The address the user entered. Updated in-place when the user accepts the suggestion.</param>
/// <param name="Suggested">The standardized address returned by the validation service, or null if the service found no match.</param>
/// <param name="ErrorMessage">Optional error message returned by the validation service, shown as a bullet in the warning banner.</param>
public record AddressCorrectionItem(
    string Label,
    AddressModel Original,
    AddressModel? Suggested,
    string? ErrorMessage = null
);
