namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

/// <summary>
/// The limits that apply to employee identifiers, wages and counts wherever they are captured.
///
/// WHY THIS EXISTS: the same employee is entered on Wage Entry, on Add Additional Employees, on Wage
/// Adjustment by Quarter and on Wage Adjustments across Multiple Quarters, through four separate
/// models. The rules had drifted — Wage Entry enforced a 64-character limit and a character set while
/// the three adjustment screens enforced none of it, so the identical value was accepted on one
/// screen and rejected on another. Keeping the limits and the wording in one place is what stops that
/// recurring.
///
/// The constraints come from downstream systems rather than from the UI: the mainframe Benefits
/// system mishandles non-ASCII characters in names, and SUITES does not process wages of $100M or
/// more — they are accepted into intake and then stall. Rejecting them at entry is deliberate.
/// </summary>
public static class EmployeeFieldRules
{
    /// <summary>Maximum characters accepted in a first or last name.</summary>
    public const int NameMaxLength = 64;

    /// <summary>
    /// Characters accepted in a first or last name: ASCII letters, digits, spaces, dashes and
    /// apostrophes.
    ///
    /// The space is a deliberate widening of the rule as originally specified, which listed only
    /// letters, numbers, dashes and apostrophes. Without it, ordinary names such as "Van Der Berg"
    /// and "Mary Ann" are rejected — and the adjustment screens, which are where someone goes to
    /// CORRECT a name, would refuse the correction. The constraint being honoured is that the
    /// downstream mainframe Benefits system mishandles non-ASCII characters; a space is ASCII and
    /// causes it no trouble.
    ///
    /// A name of nothing but spaces is still rejected, by <c>[Required]</c> — it trims before
    /// testing for empty, so whitespace never satisfies it.
    /// </summary>
    public const string NamePattern = @"^[a-zA-Z0-9\-' ]+$";

    /// <summary>
    /// Highest quarterly wage accepted for a single employee: $99,999,999.99, i.e. under $100M.
    /// </summary>
    public const double MaxQuarterlyWages = 99_999_999.99;

    /// <summary>
    /// Highest monthly employee count accepted on a tax report.
    ///
    /// Mirrors the <c>Max</c> already set on the count inputs. Having it on the model as well is what
    /// catches a value that never passed through the input — a resumed draft, or a report restored
    /// from a saved filing.
    /// </summary>
    public const int MaxEmployeeCount = 999_999;

    /// <summary>Message for a name that is too long. <c>{0}</c> is the field label.</summary>
    public const string NameTooLongMessage = "{0} cannot exceed 64 characters.";

    /// <summary>Message for a name containing disallowed characters. <c>{0}</c> is the field label.</summary>
    public const string NameCharactersMessage =
        "{0} may only contain letters, numbers, spaces, dashes and apostrophes.";

    // Pre-formatted per field. Validation attributes need compile-time constants, so they cannot
    // call string.Format on the templates above; without these the same sentence gets retyped in six
    // attributes across three models, which is how the rules drifted apart in the first place.

    /// <summary>Last name too long, for use in a validation attribute.</summary>
    public const string LastNameTooLongMessage = "Last name cannot exceed 64 characters.";

    /// <summary>First name too long, for use in a validation attribute.</summary>
    public const string FirstNameTooLongMessage = "First name cannot exceed 64 characters.";

    /// <summary>Last name has disallowed characters, for use in a validation attribute.</summary>
    public const string LastNameCharactersMessage =
        "Last name may only contain letters, numbers, spaces, dashes and apostrophes.";

    /// <summary>First name has disallowed characters, for use in a validation attribute.</summary>
    public const string FirstNameCharactersMessage =
        "First name may only contain letters, numbers, spaces, dashes and apostrophes.";

    /// <summary>Message for a wage at or above the $100M ceiling.</summary>
    public const string WagesTooLargeMessage = "Quarterly wages must be less than $100,000,000.";

    /// <summary>
    /// Message for a negative wage. Separate from <see cref="WagesTooLargeMessage" /> because
    /// "must be less than $100,000,000" reads absurdly against -50.
    /// </summary>
    public const string WagesNegativeMessage = "Quarterly wages cannot be negative.";

    /// <summary>Message for an employee count outside the accepted range.</summary>
    public const string EmployeeCountOutOfRangeMessage =
        "Employee count must be between 0 and 999,999.";

    /// <summary>
    /// Message for a wage that could not be read as a number. Names the field and says what is
    /// expected — "Please enter a valid amount." left the user guessing which of several amounts on
    /// the row was wrong and what "valid" meant.
    /// </summary>
    public const string WagesNotANumberMessage =
        "Quarterly wages must be a dollar amount, for example 1234.56.";

    /// <summary>
    /// True when <paramref name="name" /> satisfies the length and character rules. Empty is treated
    /// as valid — a missing value is a Required concern, reported separately so the user is not told
    /// two different things about one blank field.
    /// </summary>
    public static bool IsValidName(string? name) =>
        string.IsNullOrWhiteSpace(name)
        || (name.Length <= NameMaxLength
            && System.Text.RegularExpressions.Regex.IsMatch(name, NamePattern));

    /// <summary>
    /// The specific problem with <paramref name="name" />, or null when it is acceptable.
    /// <paramref name="label" /> names the field in the message, e.g. "Last name".
    /// </summary>
    public static string? DescribeNameProblem(string? name, string label)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            return null;
        }

        // Length first, so a name that is both too long and full of bad characters reports one
        // problem at a time rather than two.
        return name.Length > NameMaxLength
            ? string.Format(NameTooLongMessage, label)
            : System.Text.RegularExpressions.Regex.IsMatch(name, NamePattern)
                ? null
                : string.Format(NameCharactersMessage, label);
    }
}
