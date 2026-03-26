namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Identifies the type of data captured in a <see cref="SurveyResponse"/>.
/// Values correspond to back-end survey response item surrogate keys.
/// </summary>
public enum SurveyResponseItem
{
    /// <summary>Business legal name.</summary>
    BUS_LGL_NAM,

    /// <summary>Trade name (DBA).</summary>
    TRD_NAM,

    /// <summary>Employer email address.</summary>
    ER_EMAIL_ADR,

    /// <summary>Federal Employer Identification Number.</summary>
    FEIN_NUM,
}
