using System.ComponentModel.DataAnnotations;
using UI.EmployerPortal.Web.Features.ManageAccount.Models;

namespace Test.UI.EmployerPortal.Web.Component.Services;

/// <summary>
/// Unit tests for <see cref="AccountDetailsModel"/> validation logic.
/// Covers the IValidatableObject.Validate method (conditional FEIN/LegalName reason requirements)
/// and the RequiredIfAttribute custom validation attribute.
/// </summary>
public class AccountDetailsModelTests
{
    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Creates a valid model with all required fields populated and original values matching current values.
    /// </summary>
    private static AccountDetailsModel MakeValidModel()
    {
        return new AccountDetailsModel
        {
            FEIN = "12-3456789",
            LegalName = "Acme Corp",
            TradeName = "Acme",
            PhoneNumber = "555-123-4567",
            Extension = "101",
            CountryCode = "+1",
            EmailAddress = "test@example.com",
            OriginalFEIN = "12-3456789",
            OriginalLegalName = "Acme Corp"
        };
    }

    /// <summary>
    /// Runs IValidatableObject.Validate on the model and returns all results as a list.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>A list of validation results.</returns>
    private static List<ValidationResult> RunValidate(AccountDetailsModel model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        foreach (ValidationResult result in model.Validate(context))
        {
            results.Add(result);
        }
        return results;
    }

    /// <summary>
    /// Runs full DataAnnotations validation (Required, EmailAddress, RequiredIf, and IValidatableObject).
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <returns>A list of validation results.</returns>
    private static List<ValidationResult> RunFullValidation(AccountDetailsModel model)
    {
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();
        Validator.TryValidateObject(model, context, results, validateAllProperties: true);
        return results;
    }

    /// <summary>
    /// Returns true if any validation result in the list has a member name matching the specified name.
    /// </summary>
    /// <param name="results">The list of validation results to search.</param>
    /// <param name="memberName">The member name to look for.</param>
    /// <returns>True if any result contains the member name.</returns>
    private static bool HasMemberError(List<ValidationResult> results, string memberName)
    {
        foreach (ValidationResult result in results)
        {
            foreach (string name in result.MemberNames)
            {
                if (name == memberName)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>
    /// Returns true if any validation result in the list has an error message containing the specified text.
    /// </summary>
    /// <param name="results">The list of validation results to search.</param>
    /// <param name="text">The text to search for in error messages.</param>
    /// <returns>True if any error message contains the text.</returns>
    private static bool HasErrorContaining(List<ValidationResult> results, string text)
    {
        foreach (ValidationResult result in results)
        {
            if (result.ErrorMessage != null && result.ErrorMessage.Contains(text))
            {
                return true;
            }
        }
        return false;
    }

    // ── IValidatableObject.Validate: FEIN reason ─────────────────────────────

    /// <summary>No validation error when FEIN has not changed.</summary>
    [Fact]
    public void Validate_No_Error_When_FEIN_Unchanged()
    {
        var model = MakeValidModel();

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    /// <summary>Validation error when FEIN is changed but no reason is selected.</summary>
    [Fact]
    public void Validate_Error_When_FEIN_Changed_Without_Reason()
    {
        var model = MakeValidModel();
        model.FEIN = "98-7654321";

        List<ValidationResult> results = RunValidate(model);

        Assert.Single(results);
        Assert.True(HasMemberError(results, "ReasonForFeinChange"));
        Assert.True(HasErrorContaining(results, "FEIN"));
    }

    /// <summary>No error when FEIN is changed and a reason is selected.</summary>
    [Fact]
    public void Validate_No_Error_When_FEIN_Changed_With_Reason()
    {
        var model = MakeValidModel();
        model.FEIN = "98-7654321";
        model.ReasonForFeinChange = "1";

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    /// <summary>FEIN comparison ignores dashes — same digits with different formatting is not a change.</summary>
    [Fact]
    public void Validate_No_Error_When_FEIN_Differs_Only_In_Dash_Formatting()
    {
        var model = MakeValidModel();
        model.FEIN = "123456789";          // no dash
        model.OriginalFEIN = "12-3456789"; // with dash

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    /// <summary>FEIN comparison detects actual digit changes even when dashes differ.</summary>
    [Fact]
    public void Validate_Error_When_FEIN_Digits_Differ_Despite_Dash_Variations()
    {
        var model = MakeValidModel();
        model.FEIN = "11-1111111";
        model.OriginalFEIN = "12-3456789";

        List<ValidationResult> results = RunValidate(model);

        Assert.Single(results);
        Assert.True(HasMemberError(results, "ReasonForFeinChange"));
    }

    // ── IValidatableObject.Validate: Legal Name reason ───────────────────────

    /// <summary>No validation error when Legal Name has not changed.</summary>
    [Fact]
    public void Validate_No_Error_When_LegalName_Unchanged()
    {
        var model = MakeValidModel();

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    /// <summary>Validation error when Legal Name is changed but no reason is selected.</summary>
    [Fact]
    public void Validate_Error_When_LegalName_Changed_Without_Reason()
    {
        var model = MakeValidModel();
        model.LegalName = "New Corp Name";

        List<ValidationResult> results = RunValidate(model);

        Assert.Single(results);
        Assert.True(HasMemberError(results, "ReasonForLegalNameChange"));
        Assert.True(HasErrorContaining(results, "Legal Name"));
    }

    /// <summary>No error when Legal Name is changed and a reason is selected.</summary>
    [Fact]
    public void Validate_No_Error_When_LegalName_Changed_With_Reason()
    {
        var model = MakeValidModel();
        model.LegalName = "New Corp Name";
        model.ReasonForLegalNameChange = "2";

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    // ── Both FEIN and Legal Name changed ─────────────────────────────────────

    /// <summary>Two validation errors when both FEIN and Legal Name are changed without reasons.</summary>
    [Fact]
    public void Validate_Two_Errors_When_Both_FEIN_And_LegalName_Changed_Without_Reasons()
    {
        var model = MakeValidModel();
        model.FEIN = "98-7654321";
        model.LegalName = "New Corp Name";

        List<ValidationResult> results = RunValidate(model);

        Assert.Equal(2, results.Count);
        Assert.True(HasMemberError(results, "ReasonForFeinChange"));
        Assert.True(HasMemberError(results, "ReasonForLegalNameChange"));
    }

    /// <summary>No errors when both fields are changed and both reasons are provided.</summary>
    [Fact]
    public void Validate_No_Errors_When_Both_Changed_With_Reasons()
    {
        var model = MakeValidModel();
        model.FEIN = "98-7654321";
        model.ReasonForFeinChange = "1";
        model.LegalName = "New Corp Name";
        model.ReasonForLegalNameChange = "1";

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }

    // ── RequiredIfAttribute ──────────────────────────────────────────────────

    /// <summary>FeinChangeReasonExplanation is required when ReasonForFeinChange is "5" (Other).</summary>
    [Fact]
    public void FullValidation_Error_When_FEIN_Reason_Is_Other_Without_Explanation()
    {
        var model = MakeValidModel();
        model.ReasonForFeinChange = "5";
        model.FeinChangeReasonExplanation = null;

        List<ValidationResult> results = RunFullValidation(model);

        Assert.True(HasErrorContaining(results, "Explanation for FEIN Change"));
    }

    /// <summary>No explanation error when ReasonForFeinChange is "5" and explanation is provided.</summary>
    [Fact]
    public void FullValidation_No_Error_When_FEIN_Reason_Is_Other_With_Explanation()
    {
        var model = MakeValidModel();
        model.ReasonForFeinChange = "5";
        model.FeinChangeReasonExplanation = "We merged with another company.";

        List<ValidationResult> results = RunFullValidation(model);

        Assert.False(HasErrorContaining(results, "Explanation for FEIN Change"));
    }

    /// <summary>No explanation error when ReasonForFeinChange is not "5".</summary>
    [Fact]
    public void FullValidation_No_Explanation_Error_When_FEIN_Reason_Is_Not_Other()
    {
        var model = MakeValidModel();
        model.ReasonForFeinChange = "1";
        model.FeinChangeReasonExplanation = null;

        List<ValidationResult> results = RunFullValidation(model);

        Assert.False(HasErrorContaining(results, "Explanation for FEIN Change"));
    }

    /// <summary>LegalNameChangeExplanation is required when ReasonForLegalNameChange is "5" (Other).</summary>
    [Fact]
    public void FullValidation_Error_When_LegalName_Reason_Is_Other_Without_Explanation()
    {
        var model = MakeValidModel();
        model.ReasonForLegalNameChange = "5";
        model.LegalNameChangeExplanation = null;

        List<ValidationResult> results = RunFullValidation(model);

        Assert.True(HasErrorContaining(results, "Explanation for Legal Name Change"));
    }

    /// <summary>No explanation error when ReasonForLegalNameChange is "5" and explanation is provided.</summary>
    [Fact]
    public void FullValidation_No_Error_When_LegalName_Reason_Is_Other_With_Explanation()
    {
        var model = MakeValidModel();
        model.ReasonForLegalNameChange = "5";
        model.LegalNameChangeExplanation = "Court ordered name change.";

        List<ValidationResult> results = RunFullValidation(model);

        Assert.False(HasErrorContaining(results, "Explanation for Legal Name Change"));
    }

    // ── Edge cases ───────────────────────────────────────────────────────────

    /// <summary>Whitespace-only reason is treated as empty and triggers validation error.</summary>
    [Fact]
    public void Validate_Error_When_FEIN_Changed_With_Whitespace_Only_Reason()
    {
        var model = MakeValidModel();
        model.FEIN = "98-7654321";
        model.ReasonForFeinChange = "   ";

        List<ValidationResult> results = RunValidate(model);

        Assert.Single(results);
        Assert.True(HasMemberError(results, "ReasonForFeinChange"));
    }

    /// <summary>Null FEIN values are handled gracefully and treated as empty string.</summary>
    [Fact]
    public void Validate_No_Error_When_Both_FEIN_And_OriginalFEIN_Are_Null()
    {
        var model = MakeValidModel();
        model.FEIN = null!;
        model.OriginalFEIN = null!;

        List<ValidationResult> results = RunValidate(model);

        // No FEIN-related validation error from Validate (Required is separate)
        Assert.False(HasMemberError(results, "ReasonForFeinChange"));
    }

    /// <summary>Model defaults have matching original values so no change is detected.</summary>
    [Fact]
    public void Validate_No_Errors_On_Default_Model()
    {
        var model = new AccountDetailsModel();

        List<ValidationResult> results = RunValidate(model);

        Assert.Empty(results);
    }
}
