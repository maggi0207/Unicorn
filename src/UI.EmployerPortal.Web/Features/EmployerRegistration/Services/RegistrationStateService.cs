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
}

/// <summary>
/// Represents one address that needs user review on the Address Correction page.
/// </summary>
/// <param name="Label">Display name shown as the section header, e.g. "Business Mailing Address".</param>
/// <param name="Original">The address the user entered. Updated in-place when the user accepts the suggestion.</param>
/// <param name="Suggested">The standardized address returned by the validation service, or null if the service found no match.</param>
public record AddressCorrectionItem(
    string Label,
    AddressModel Original,
    AddressModel? Suggested
);
