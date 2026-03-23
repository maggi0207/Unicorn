namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Represents the business category selected by the employer during the
/// Unemployment Insurance Subjectivity step of registration.
/// </summary>
public enum BusinessCategory
{
    /// <summary>No category has been selected yet.</summary>
    Unknown = 0,

    /// <summary>A standard for-profit commercial business.</summary>
    Commercial,

    /// <summary>Domestic household employer (e.g. in a private home).</summary>
    Domestic,

    /// <summary>Agricultural or farming operation.</summary>
    Agricultural,

    /// <summary>Non-profit organization with a 501(c)(3) ruling from the IRS.</summary>
    NonProfit_501c3,

    /// <summary>Non-profit organization without a 501(c)(3) ruling.</summary>
    NonProfit_Other,
}
