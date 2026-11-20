using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerInformation.Models;

/// <summary>
/// Options for status change closure reason
/// THIS IS A DUPLICATE OF A DWD DB TABLE, DO NOT CHANGE UNLESS THE TABLE IS ALSO CHANGING
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
    /// Transferred or Sold Business
    /// </summary>
    [Display(Name = "Transferred or Sold Business")]
    TransferredOrSoldBusiness = 4,

    /// <summary>
    /// No Longer Meets UI Criteria
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
    [Display(Name = "Leasing employees from Professional Employer Organization")]
    LeasingEmployeesFromPEO = 10,

    /// <summary>
    /// Finance Conversion
    /// </summary>
    [Display(Name = "Finance Conversion")]
    FinanceConversion = 11,

    /// <summary>
    /// Zero Payroll Reports Filed
    /// </summary>
    [Display(Name = "Zero payroll reports filed")]
    ZeroPayrollReportsFiled = 12,

    /// <summary>
    /// Does not meet UI Criteria
    /// </summary>
    [Display(Name = "Does not meet UI Criteria")]
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
    /// Reimbursable no longer meets UI Criteria
    /// </summary>
    [Display(Name = "Reimbursable no longer meets UI Criteria")]
    ReimbursableNoLongerMeetsUICriteria = 16,

    /// <summary>
    /// Member of group
    /// </summary>
    [Display(Name = "Member of group")]
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
