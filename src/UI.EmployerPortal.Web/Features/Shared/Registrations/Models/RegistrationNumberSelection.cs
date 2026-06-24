using UI.EmployerPortal.Web.Features.Shared.Session.Models;

namespace UI.EmployerPortal.Web.Features.Shared.Registrations.Models;

/// <summary>
/// Session Model for storing the selection whether employer has registration number
/// Used to persist the selected employer selection across page navigations.
/// </summary>
public sealed record RegistrationNumberSelection : ISessionModel
{
    /// <summary>
    /// Gets or sets the selected employer registration selection. 
    /// </summary>
    public EmployerRegistrationInfo? HasRegistrationNumber { get; init; }
}
