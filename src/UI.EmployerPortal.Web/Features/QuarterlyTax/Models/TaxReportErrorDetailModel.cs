namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// Represents an individual error detail displayed in the Error Detail Report tab.
/// </summary>
public class TaxReportErrorDetailModel
{
    /// <summary>
    /// Gets or sets the record number.
    /// </summary>
    public int RecordNumber { get; set; }

    /// <summary>
    /// Gets or sets the legal name.
    /// </summary>
    public string LegalName { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the FEIN.
    /// </summary>
    public string FEIN { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the UI Account Number.
    /// </summary>
    public string UIAccountNumber { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the quarter and year.
    /// </summary>
    public string QuarterYear { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets whether first quarter deferral was elected.
    /// </summary>
    public string ElectedFirstQuarterDeferral { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the gross wages.
    /// </summary>
    public decimal GrossWages { get; set; }

    /// <summary>
    /// Gets or sets the exclusions amount.
    /// </summary>
    public decimal Exclusions { get; set; }

    /// <summary>
    /// Gets or sets the taxable payroll.
    /// </summary>
    public decimal TaxablePayroll { get; set; }

    /// <summary>
    /// Gets or sets the Month 1 employee count.
    /// </summary>
    public int Month1EmployeeCount { get; set; }

    /// <summary>
    /// Gets or sets the Month 2 employee count.
    /// </summary>
    public int Month2EmployeeCount { get; set; }

    /// <summary>
    /// Gets or sets the Month 3 employee count.
    /// </summary>
    public int Month3EmployeeCount { get; set; }

    /// <summary>
    /// Gets or sets the amount paid.
    /// </summary>
    public decimal AmountPaid { get; set; }

    /// <summary>
    /// Gets or sets the error message.
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the corrective action.
    /// </summary>
    public string CorrectiveAction { get; set; } = string.Empty;


}

