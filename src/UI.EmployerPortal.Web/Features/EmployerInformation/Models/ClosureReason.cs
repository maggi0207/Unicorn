using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerInformation.Models;

/// <summary>
/// Options for status change closure reason
/// </summary>
public enum ClosureReason
{
    /// <summary>
    /// Out of Business
    /// </summary>
    [Display(Name = "Out of Business")]
    OutOfBusiness = 1,

    /// <summary>
    /// Cancelled
    /// </summary>
    [Display(Name = "Cancelled")]
    Cancelled = 2,

    /// <summary>
    /// Duplicate Account
    /// </summary>
    [Display(Name = "Duplicate Account")]
    DuplicateAccount = 3,

    /// <summary>
    /// Transferred
    /// </summary>
    [Display(Name = "Transferred")]
    Transferred = 4,

    /// <summary>
    /// No longer meets UI Criteria
    /// </summary>
    [Display(Name = "No longer meets UI Criteria")]
    NoLongerMeetsUICriteria = 5,

    /// <summary>
    /// No longer operating in Wisconsin
    /// </summary>
    [Display(Name = "No longer operating in Wisconsin")]
    NoLongerOperatingInWisconsin = 6,

    /// <summary>
    /// Reorganized
    /// </summary>
    [Display(Name = "Reorganized")]
    Reorganized = 7,

    /// <summary>
    /// Independent Contractors
    /// </summary>
    [Display(Name = "Independent Contractors")]
    IndependentContractors = 9,

    /// <summary>
    /// Loasing employees from PEO
    /// </summary>
    [Display(Name = "Leasing employees from PEO")]
    LeasingEmployeesFromPEO = 10,

    /// <summary>
    /// Finance Conversion
    /// </summary>
    [Display(Name = "Finance Conversion")]
    FinanceConversion = 11,

    /// <summary>
    /// Zero payroll reports filed
    /// </summary>
    [Display(Name = "Zero payroll reports filed")]
    ZeroPayrollReportsFiled = 12,

    /// <summary>
    /// Does not meet UI criteria
    /// </summary>
    [Display(Name = "Does not meet UI criteria")]
    DoesNotMeetUICriteria = 13,

    /// <summary>
    /// Continuing without employees
    /// </summary>
    [Display(Name = "Continuing without employees")]
    ContinuingWithoutEmployees = 14,

    /// <summary>
    /// Deceased
    /// </summary>
    [Display(Name = "Deceased")]
    Deceased = 15,

    /// <summary>
    /// Reimbursable no longer meets UI criteria
    /// </summary>
    [Display(Name = "Reimbursable no longer meets UI criteria")]
    ReimbursableNoLongerMeetsUICriteria = 16,

    /// <summary>
    /// Member of Group
    /// </summary>
    [Display(Name = "Member of Group")]
    MemberOfGroup = 17,

    /// <summary>
    /// Dissolved Group
    /// </summary>
    [Display(Name = "Dissolved Group")]
    DissolvedGroup = 18,

    /// <summary>
    /// Fiscal agent electing to be employer
    /// </summary>
    [Display(Name = "Fiscal agent electing to be employer")]
    FiscalAgentElectingToBeEmployer = 19,
}
