using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerInformation.Models;

/// <summary>
/// Options for status change activation reason
/// </summary>
public enum ActivationReason
{
    /// <summary>
    /// Again has employees
    /// </summary>
    [Display(Name = "Again has employees")]
    AgainHasEmployees = 1,

    /// <summary>
    /// Expect future employment
    /// </summary>
    [Display(Name = "Expect future employment")]
    ExpectFutureEmployment = 2,

    /// <summary>
    /// No employment expected, but dispute account closure
    /// </summary>
    [Display(Name = "No employment expected, but dispute account closure")]
    NoEmploymentExpectedDisputeAccountClosure = 3,

    /// <summary>
    /// Closed in error
    /// </summary>
    [Display(Name = "Closed in error")]
    ClosedInError = 4,

    /// <summary>
    /// Employment found by Audit Investigation
    /// </summary>
    [Display(Name = "Employment found by Audit Investigation")]
    EmploymentFoundByAuditInvestigation = 5,
}
