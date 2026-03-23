using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Form model for the Unemployment Insurance Subjectivity step of employer registration.
/// Captures all answers needed to determine whether the employer is subject to Wisconsin UI law.
/// </summary>
public class SubjectivityModel
{
    /// <summary>The employer's business category (Commercial, Domestic, Agricultural, or Non-Profit).</summary>
    [Required(ErrorMessage = "Business category is required.")]
    public BusinessCategory? BusinessCategory { get; set; }

    /// <summary>Whether the employer has applied for 501(c)(3) status with the IRS.</summary>
    public bool? HasAppliedFor501c3Status { get; set; }

    /// <summary>Financial institution used for the business checking account (name + address).</summary>
    public AddressModel FinancialInstitution { get; set; } = new();

    /// <summary>Whether the employer has employees who work in states other than Wisconsin.</summary>
    public bool? EmployeesInOtherStates { get; set; }

    /// <summary>Whether the employer has a Federal Unemployment Tax (FUTA) liability in any state other than Wisconsin.</summary>
    public bool? FederalTaxLiability { get; set; }

    /// <summary>Whether the employer has a federal tax liability outside of Wisconsin.</summary>
    public bool? HasFederalTaxLiabilityOutsideWisconsin { get; set; }

    /// <summary>Whether the employer has paid wages meeting the quarterly threshold (employee-based test).</summary>
    public bool? PaidWagesInAQuarterEmployees { get; set; }

    /// <summary>Quarter and year (q/yyyy) in which the employer first paid qualifying wages under the employee test.</summary>
    public string? QuarterYearFirstPaidEmployees { get; set; }

    /// <summary>Whether the employer has had at least one employee working in 20 different calendar weeks.</summary>
    public bool? Employee20Weeks { get; set; }

    /// <summary>Date on which the employer's 20th qualifying calendar week ended.</summary>
    public DateTime? DateWeek20Ended { get; set; }

    /// <summary>Whether the employer has had at least 4 employees on the same day in 20 different calendar weeks (Non-Profit 501c3 test).</summary>
    public bool? Employee20WeeksFourOrMore { get; set; }

    /// <summary>Whether the employer expects to pay wages meeting the quarterly threshold in the future.</summary>
    public bool? ExpectToPayWagesInAQuarter { get; set; }

    /// <summary>When the employer expects to begin paying qualifying wages (e.g. "Within 30 Days").</summary>
    public string? WhenExpectToPayWagesInAQuarter { get; set; }

    /// <summary>Whether the employer has paid wages meeting the quarterly threshold (tax-based test).</summary>
    public bool? PaidWagesInAQuarterTaxes { get; set; }

    /// <summary>Quarter and year (q/yyyy) in which the employer first paid qualifying wages under the tax test.</summary>
    public string? QuarterYearFirstPaidTaxes { get; set; }

    /// <summary>Gross quarterly wage entries used to verify the employer meets wage minimums.</summary>
    public List<WageEntryModel> Wages { get; set; } = new();
}
