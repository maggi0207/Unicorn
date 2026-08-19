using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Models;

/// <summary>
/// Form model for adding a bank account.
/// Validates required fields and cross-field rules via <see cref="IValidatableObject"/>.
/// </summary>
public class BankAccountModel : IValidatableObject
{
    /// <summary>
    /// Surrogate key of the account being edited. Zero means this is a new account.
    /// </summary>
    public int BankAccountSk { get; set; }

    /// <summary>
    /// User-defined label for this bank account. Must be unique across the employer's accounts.
    /// </summary>
    [Required(ErrorMessage = "Bank Account Nickname is required")]
    [MaxLength(50, ErrorMessage = "Bank Account Nickname cannot exceed 50 characters")]
    public string? Nickname { get; set; }

    /// <summary>
    /// ABA routing number. Must be exactly 9 digits.
    /// </summary>
    [Required(ErrorMessage = "Bank Routing Number is required")]
    [RegularExpression(@"^\d{9}$", ErrorMessage = "Bank Routing Number must be exactly 9 digits")]
    public string? RoutingNumber { get; set; }

    /// <summary>
    /// Bank account number. Must be digits only and up to 17 characters, unless it equals
    /// <see cref="OriginalMaskedAccountNumber"/> (meaning the user did not change it during edit).
    /// </summary>
    [Required(ErrorMessage = "Bank Account Number is required")]
    public string? AccountNumber { get; set; }

    /// <summary>
    /// Confirmation entry for <see cref="AccountNumber"/>. Must match exactly.
    /// </summary>
    [Required(ErrorMessage = "Re-enter Account Number is required")]
    public string? ConfirmAccountNumber { get; set; }

    /// <summary>
    /// The masked account number pre-populated when editing an existing account (e.g. *******9177).
    /// When <see cref="AccountNumber"/> still equals this value the user has not changed the number,
    /// so digit-format validation is skipped and null is sent to the backend on save.
    /// </summary>
    public string? OriginalMaskedAccountNumber { get; set; }

    /// <summary>
    /// Bank name populated from the routing number lookup. Read-only on the form.
    /// </summary>
    public string? BankName { get; set; }

    /// <summary>
    /// Account type — Checking or Savings.
    /// </summary>
    [Required(ErrorMessage = "Account Type is required")]
    public string? AccountType { get; set; }

    /// <summary>
    /// Indicates the account is funded by a transfer from a financial institution outside the U.S. (IAT).
    /// When true, the IAT address fields are required.
    /// </summary>
    public bool IsInternational { get; set; }

    /// <summary>
    /// Database CodeSK for the selected country from the EFT payment service.
    /// Required when <see cref="IsInternational"/> is true.
    /// </summary>
    public int IatCountryCode { get; set; }

    /// <summary>
    /// Helper method to get the string value of the IatCountryCode
    /// </summary>
    public string? IatCountryCodeValue => IatCountryCode == 0 ? null : IatCountryCode.ToString();

    /// <summary>
    /// Indicates the selected country is United States. Set by the form when country changes.
    /// Used for conditional State field validation.
    /// </summary>
    public bool IatCountryIsUsa { get; set; }

    /// <summary>
    /// Indicates the selected country is Canada. Set by the form when country changes.
    /// Used for conditional Province field validation.
    /// </summary>
    public bool IatCountryIsCanada { get; set; }

    /// <summary>
    /// Street address of the foreign financial institution.
    /// Required when <see cref="IsInternational"/> is true.
    /// </summary>
    public string? IatStreetAddress { get; set; }

    /// <summary>
    /// City of the foreign financial institution.
    /// Required when <see cref="IsInternational"/> is true.
    /// </summary>
    public string? IatCity { get; set; }

    /// <summary>
    /// Postal code of the foreign financial institution.
    /// Required when <see cref="IatCountryCode"/> is United States (840) or Canada (124).
    /// </summary>
    public string? IatPostalCode { get; set; }

    /// <summary>
    /// US state CodeSK for the foreign financial institution address.
    /// Required when <see cref="IatCountryCode"/> is United States (840).
    /// </summary>
    public int IatStateCode { get; set; }

    /// <summary>
    /// US state CodeSK for the foreign financial institution address.
    /// Required when <see cref="IatCountryCode"/> is United States (840).
    /// </summary>
    public string? IatStateValue => IatStateCode == 0 ? null : IatStateCode.ToString();

    /// <summary>
    /// Canadian province CodeSK for the foreign financial institution address.
    /// Required when <see cref="IatCountryCode"/> is Canada (124).
    /// </summary>
    public int IatProvinceCode { get; set; }

    /// <summary>
    /// US state CodeSK for the foreign financial institution address.
    /// Required when <see cref="IatCountryCode"/> is United States (840).
    /// </summary>
    public string? IatProvinceValue => IatProvinceCode == 0 ? null : IatProvinceCode.ToString();

    /// <inheritdoc/>
    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var accountNumberUnchanged = !string.IsNullOrWhiteSpace(OriginalMaskedAccountNumber)
            && AccountNumber == OriginalMaskedAccountNumber;

        if (!accountNumberUnchanged && !string.IsNullOrWhiteSpace(AccountNumber))
        {
            if (AccountNumber.Length > 17)
            {
                yield return new ValidationResult(
                    "Bank Account Number cannot exceed 17 digits",
                    [nameof(AccountNumber)]);
            }
            else if (!System.Text.RegularExpressions.Regex.IsMatch(AccountNumber, @"^\d+$"))
            {
                yield return new ValidationResult(
                    "Bank Account Number must contain digits only",
                    [nameof(AccountNumber)]);
            }
        }

        if (!string.IsNullOrWhiteSpace(AccountNumber) &&
            !string.IsNullOrWhiteSpace(ConfirmAccountNumber) &&
            AccountNumber != ConfirmAccountNumber)
        {
            yield return new ValidationResult(
                "Account numbers do not match",
                [nameof(ConfirmAccountNumber)]);
        }

        if (IsInternational)
        {
            if (IatCountryCode == 0)
            {
                yield return new ValidationResult("Country is required", [nameof(IatCountryCodeValue)]);
            }

            if (string.IsNullOrWhiteSpace(IatStreetAddress))
            {
                yield return new ValidationResult("Street Address is required", [nameof(IatStreetAddress)]);
            }

            if (string.IsNullOrWhiteSpace(IatCity))
            {
                yield return new ValidationResult("City is required", [nameof(IatCity)]);
            }

            if (IatCountryIsUsa && IatStateCode == 0)
            {
                yield return new ValidationResult("State is required", [nameof(IatStateValue)]);
            }

            if (IatCountryIsCanada && IatProvinceCode == 0)
            {
                yield return new ValidationResult("Province is required", [nameof(IatProvinceValue)]);
            }

            if ((IatCountryIsUsa || IatCountryIsCanada) && string.IsNullOrWhiteSpace(IatPostalCode))
            {
                yield return new ValidationResult("Postal Code is required", [nameof(IatPostalCode)]);
            }
        }
    }
}
