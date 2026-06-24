namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// CorporateOfficerServicesData
/// </summary>
public class CorporateOfficerServicesModel
{
    /// <summary>
    /// Do corporate officers perform services or plan to perform services in Wisconsin?
    /// </summary>
    public bool? OfficersPerformServices { get; set; }

    /// <summary>
    /// Approximate date officers will be paid (must be a future date)
    /// </summary>
    public DateOnly? ApproximatePayDate { get; set; }

    /// <summary>
    /// Explanation if officers are not expected to be paid
    /// </summary>
    public string NoPayExplanation { get; set; } = string.Empty;
}
