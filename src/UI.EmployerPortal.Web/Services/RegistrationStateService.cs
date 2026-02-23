using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public class RegistrationStateService
{
    public List<AddressCorrectionItem> AddressCorrections { get; set; } = new();
}

public record AddressCorrectionItem(
    string Label,
    AddressModel Original,
    AddressModel? Suggested
);
