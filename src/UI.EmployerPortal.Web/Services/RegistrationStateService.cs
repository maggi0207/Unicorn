using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

namespace UI.EmployerPortal.Web.Services;

public class RegistrationStateService
{
    /// <summary>The model saved before navigating to address correction, so it can be restored on edit.</summary>
    public BusinessInformationModel? BusinessInfo { get; set; }

    /// <summary>Address corrections collected after service validation on BusinessInformation submit.</summary>
    public List<AddressCorrectionItem> AddressCorrections { get; set; } = new();

    /// <summary>Index of the correction currently being shown on the AddressCorrection page.</summary>
    public int CorrectionIndex { get; set; }
}

public record AddressCorrectionItem(
    string Label,
    AddressModel Original,
    AddressModel? Suggested
);
