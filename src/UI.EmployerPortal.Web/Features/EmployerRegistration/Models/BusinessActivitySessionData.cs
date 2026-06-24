namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Business Activity session data for persisting form state across page reloads or navigation.
/// </summary>
public class BusinessActivitySessionData
{
    /// <summary>
    /// Gets or sets the persisted date the business started or was acquired.
    /// </summary>
    public DateTime? DateBusinessStarted { get; set; }

    /// <summary>
    /// Gets or sets the persisted date the employer first had paid employees working in Wisconsin.
    /// </summary>
    public DateTime? DateFirstPaidEmployeesInWI { get; set; }

    /// <summary>
    /// Gets or sets the persisted date the employer first paid wages for work performed in Wisconsin.
    /// </summary>
    public DateTime? DateFirstPaidWagesInWI { get; set; }

    /// <summary>
    /// Gets or sets the persisted principal business activity type.
    /// </summary>
    public PrincipalBusinessActivityType PrincipalBusinessActivity { get; set; }

    /// <summary>
    /// Gets or sets the persisted description of the primary business activity.
    /// </summary>
    public string? PrimaryBusinessActivityDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the Wisconsin-specific business activity
    /// is persisted as the same as the primary business activity.
    /// </summary>
    public bool SameAsPrimaryBusinessActivity { get; set; }

    /// <summary>
    /// Gets or sets the persisted description of the Wisconsin-specific business activity.
    /// </summary>
    public string? WisconsinSpecificBusinessActivity { get; set; }

    ///<summary>
    ///Does the entity supply temporary workers to clients?
    /// </summary>
    public bool? SuppliesTemporaryWorkers { get; set; }

    ///<summary>
    ///Does the entity provide leased employees under one account?
    /// </summary>
    public bool? ProvidesEmployeeLeasing { get; set; }

    ///<summary>
    ///Explanantion when Employer Service is selected
    /// </summary>
    public string? EmployerServiceExplanantion { get; set; }

    ///<summary>
    ///Own employees/client employees or a combination of both
    /// </summary>
    public string? EmployeeType { get; set; }

    ///<summary>
    ///No.of employees currently performing services in Wisconsin
    /// </summary>
    public string? EmployeeCount { get; set; }

    ///<summary>
    ///Services performed by employee in Wisconsin
    /// </summary>
    public string? ServicesDescription { get; set; }
}
