using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Components;

/// <summary>
/// Code-behind for the Audit File Upload contact information form. Owns its own EditContext
/// and validation, matching the self-contained pattern used elsewhere (see
/// SelectedMethodAndFileUpload). Exposes <see cref="IsValid"/> so the hosting page can
/// validate before submitting, and <see cref="GetCurrentErrors"/> so the hosting page can
/// pull this form's current errors into its own single, page-level summary instead of this
/// component rendering its own banner.
/// </summary>
public partial class AuditContactInformation
{
    /// <summary>
    /// The contact data model bound to the form.
    /// </summary>
    [Parameter]
    public required AuditContactModel Model { get; set; }

    /// <summary>
    /// Raised whenever this form's validation state may have changed after a submit attempt,
    /// so the hosting page can refresh its own combined summary.
    /// </summary>
    [Parameter]
    public EventCallback OnValidationChanged { get; set; }

    private EditContext _editContext = default!;
    private ValidationMessageStore _manualMessageStore = default!;
    private bool _formSubmitted;
    private bool _hasValidationErrors;

    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    /// <summary>Shared 10-digit phone format used by both Upload and Record phone fields.</summary>
    private readonly Regex _phoneRegex = new(@"^\d{3}-\d{3}-\d{4}$");

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(AuditContactModel.BusinessName)] = "audit-bus-name",
        ["AddressLine1"] = "audit-AddressLine1",
        ["AddressLine3"] = "audit-AddressLine3",
        ["AddressLine4"] = "audit-AddressLine4",
        ["City"] = "audit-City",
        ["State"] = "audit-State",
        ["Province"] = "audit-Province",
        ["Zip"] = "audit-Zip",
        ["PostalCode"] = "audit-PostalCode",
        [nameof(AuditContactModel.ContactFirstName)] = "audit-contact-first-name",
        [nameof(AuditContactModel.ContactLastName)] = "audit-contact-last-name",
        [nameof(AuditContactModel.PhoneNumber)] = "audit-phone",
        [nameof(AuditContactModel.Email)] = "audit-email",
        [nameof(AuditContactModel.ConfirmEmail)] = "audit-confirm-email"
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
        _manualMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            if (e.FieldIdentifier.FieldName == nameof(AuditContactModel.PhoneNumber))
            {
                ValidatePhoneNumberField(e.FieldIdentifier, Model.PhoneNumber);
            }
            if (_formSubmitted)
            {
                RunValidation();
                _ = OnValidationChanged.InvokeAsync();
            }
            else
            {
                _hasValidationErrors = _editContext.GetValidationMessages().Any();
            }
            StateHasChanged();
        };
    }

    /// <summary>
    /// Validates the form. Returns true when there are no validation errors.
    /// </summary>
    public bool IsValid()
    {
        _formSubmitted = true;
        var isValid = RunValidation();
        StateHasChanged();
        return isValid;
    }

    /// <summary>
    /// Returns this form's current validation messages paired with their link target HTML id
    /// (or null when no field/link applies), for the hosting page to fold into its own summary.
    /// </summary>
    public IReadOnlyList<(string Message, string? TargetId)> GetCurrentErrors()
    {
        var results = new List<(string Message, string? TargetId)>();

        foreach (var message in _editContext.GetValidationMessages().Distinct())
        {
            var fieldName = _fieldIds.Keys.FirstOrDefault(key =>
            {
                var field = ResolveField(key);
                return _editContext.GetValidationMessages(field).Contains(message);
            });

            var targetId = fieldName is not null && _fieldIds.TryGetValue(fieldName, out var id)
                ? id
                : null;

            results.Add((message, targetId));
        }

        return results;
    }

    /// <summary>
    /// Clears and rebuilds all validation messages from the current field values.
    /// </summary>
    private bool RunValidation()
    {
        _manualMessageStore.Clear();

        // Validate the nested address using its own data annotations.
        var addressContext = new ValidationContext(Model.MailingAddress);
        var addressErrors = new List<ValidationResult>();
        Validator.TryValidateObject(Model.MailingAddress, addressContext, addressErrors, validateAllProperties: true);
        foreach (var error in addressErrors)
        {
            foreach (var memberName in error.MemberNames)
            {
                _manualMessageStore.Add(new FieldIdentifier(Model.MailingAddress, memberName), error.ErrorMessage!);
            }
        }

        if (Model.MailingAddress.Country == "United States" &&
            !string.IsNullOrWhiteSpace(Model.MailingAddress.Zip) &&
            !Regex.IsMatch(Model.MailingAddress.Zip!, @"^\d{5}$"))
        {
            _manualMessageStore.Add(new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.Zip)), "Zip Code is not a valid format.");
        }

        if (Model.MailingAddress.Country == "Canada" &&
            !string.IsNullOrWhiteSpace(Model.MailingAddress.PostalCode) &&
            !Regex.IsMatch(Model.MailingAddress.PostalCode!, @"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$"))
        {
            _manualMessageStore.Add(
                new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.PostalCode)),
                "Postal Code format is incorrect. Please enter a valid postal code in the format ANA NAN where \"A\" represents a letter and \"N\" represents a digit.");
        }

        _editContext.Validate();

        var phoneField = _editContext.Field(nameof(AuditContactModel.PhoneNumber));

        ValidatePhoneNumberField(phoneField, Model.PhoneNumber);

        foreach (var error in Model.Validate(new ValidationContext(Model)))
        {
            foreach (var memberName in error.MemberNames)
            {
                _manualMessageStore.Add(_editContext.Field(memberName), error.ErrorMessage!);
            }
        }

        _editContext.NotifyValidationStateChanged();
        var isValid = !_editContext.GetValidationMessages().Any();
        _hasValidationErrors = !isValid;
        return isValid;
    }

    /// <summary>
    /// Validates a phone number field (required + 10-digit format) as a single source of
    /// truth, used both on live field-change and at submit time, for Upload and Record phone.
    /// Ensures exactly one message is ever shown for these fields.
    /// </summary>
    private void ValidatePhoneNumberField(FieldIdentifier field, string? value)
    {
        _manualMessageStore.Clear(field);

        if (string.IsNullOrWhiteSpace(value))
        {
            _manualMessageStore.Add(field, "Phone Number is required.");
        }
        else if (!_phoneRegex.IsMatch(value))
        {
            _manualMessageStore.Add(field, "Enter a valid 10-digit phone number.");
        }

        _editContext.NotifyValidationStateChanged();
    }

    private FieldIdentifier ResolveField(string fieldName)
    {
        return fieldName switch
        {
            nameof(AddressModel.AddressLine1)
            or nameof(AddressModel.AddressLine3)
            or nameof(AddressModel.AddressLine4)
            or nameof(AddressModel.City)
            or nameof(AddressModel.State)
            or nameof(AddressModel.Province)
            or nameof(AddressModel.Zip)
            or nameof(AddressModel.PostalCode)
            or nameof(AddressModel.Extension)
                => new FieldIdentifier(Model.MailingAddress, fieldName),
            _ => _editContext.Field(fieldName)
        };
    }

    private void HandleEmailBlur()
    {
        var emailAttr = new EmailAddressAttribute();

        var emailField = _editContext.Field(nameof(AuditContactModel.Email));
        var confirmEmailField = _editContext.Field(nameof(AuditContactModel.ConfirmEmail));

        _manualMessageStore.Clear(emailField);
        _manualMessageStore.Clear(confirmEmailField);

        if (!string.IsNullOrWhiteSpace(Model.Email) && !emailAttr.IsValid(Model.Email))
        {
            _manualMessageStore.Add(emailField, "Enter a valid email address.");
        }
        if (!string.IsNullOrWhiteSpace(Model.ConfirmEmail) && !emailAttr.IsValid(Model.ConfirmEmail))
        {
            _manualMessageStore.Add(confirmEmailField, "Enter a valid confirm email address.");
        }
        if (emailAttr.IsValid(Model.Email) && emailAttr.IsValid(Model.ConfirmEmail)
            && !string.Equals(Model.Email, Model.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
        {
            _manualMessageStore.Add(confirmEmailField, "Email addresses do not match.");
        }

        _editContext.NotifyValidationStateChanged();

        if (_formSubmitted)
        {
            _ = OnValidationChanged.InvokeAsync();
        }
    }

    /// <summary>
    /// Errors for a field become visible after submit or after the user interacts with it.
    /// </summary>
    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
    }

    /// <summary>
    /// Address errors become visible after submit or after the user changes any address field.
    /// </summary>
    private bool IsAddressVisible(AddressModel address)
    {
        return _formSubmitted || _touchedFields.Any(f =>
        {
            return ReferenceEquals(f.Model, address);
        });
    }
}
