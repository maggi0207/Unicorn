using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

/// <summary>
/// Qualified Settlement Fund Model
/// </summary>
public class QualifiedSettlementFundModel
{
    /// <summary>
    /// PaymentsForServices
    /// </summary>
    public bool? PaymentsForServices { get; set; }

    /// <summary>
    /// EntityLeganName
    /// </summary>
    [Required(ErrorMessage = "Legal name is required")]
    public string? EntityLegalName { get; set; }

    /// <summary>
    /// FederalIdNumber
    /// </summary>
    [Required(ErrorMessage = "Federal ID Number is required")]
    public string? FederalIdNumber { get; set; }

    /// <summary>
    /// WisconsinUiAccountNumber
    /// </summary>
    public string? WisconsinUiAccountNumber { get; set; }

    /// <summary>
    /// ServiceAddress
    /// </summary>
    public QsfServiceAddressModel ServiceAddress { get; set; } = new();

    /// <summary>
    /// FederalIdNumber
    /// </summary>
    public string? PaymentReason { get; set; }

    /// <summary>
    /// UploadedFileName
    /// </summary>
    public string? FilePath { get; set; }

    /// <summary>
    /// WillProvideDocumentationLater
    /// </summary>
    public bool WillProvideDocumentationLater { get; set; }
}

/// <summary>
/// Qsf Service Address Model
/// </summary>
public class QsfServiceAddressModel
{
    /// <summary>
    /// AddressLine1
    /// </summary>
    [Required(ErrorMessage = "Address Line 1 is required")]
    public string? AddressLine1 { get; set; }

    /// <summary>
    /// AddressLine2
    /// </summary>
    public string? AddressLine2 { get; set; }

    /// <summary>
    /// City
    /// </summary>
    [Required(ErrorMessage = "City is required")]
    public string? City { get; set; }

    /// <summary>
    /// State
    /// </summary>
    public string? State { get; set; } = string.Empty;

    /// <summary>
    /// ZipCode
    /// </summary>
    [Required(ErrorMessage = "Zip Code is required")]
    [RegularExpression(@"^\d{5}$", ErrorMessage = "Zip Code must be 5 digits")]
    public string? ZipCode { get; set; }

    /// <summary>
    /// ZipExtension
    /// </summary>
    public string? ZipExtension { get; set; }
}
