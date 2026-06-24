using UI.EmployerPortal.Web.Features.Shared.Registrations.Models;
using UI.EmployerPortal.Web.Features.Shared.Session.Managers;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration;

/// <summary>
/// Interface for landing orchestration operations
/// Manages account selection and session storage.
/// </summary>
public interface IEmployerRegistrationOrchestrator
{
    /// <summary>
    /// Stores whether the Employer has an existing registartion number and prepares for navigation. 
    /// </summary>    
    /// <param name="hasRegistrationNumber">True if employer has a registration number, false otherwise.</param>
    /// <returns></returns>
    Task SelectRegistrationChoiceAsync(EmployerRegistrationInfo hasRegistrationNumber);
}

/// <summary>
/// Orchestrator for the landing page.
/// Handle account selection and session management.
/// </summary>
public class EmployerRegistrationOrchestrator : IEmployerRegistrationOrchestrator
{
    private readonly ISessionManager _sessionManager;

    /// <summary>
    /// Initialize a new instance of the <see cref="EmployerRegistrationOrchestrator"/> class. 
    /// </summary>
    /// <param name="sessionManager">The session manager for storing session data.</param>
    public EmployerRegistrationOrchestrator(ISessionManager sessionManager)
    {
        _sessionManager = sessionManager;
    }

    ///<inheritdoc />
    public async Task SelectRegistrationChoiceAsync(EmployerRegistrationInfo hasRegistrationNumber)
    {
        await _sessionManager.SetAsync(new RegistrationNumberSelection { HasRegistrationNumber = hasRegistrationNumber });
    }

}
