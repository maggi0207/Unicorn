using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.ESP.Models;

namespace UI.EmployerPortal.Web.Features.ESP.Components;

/// <summary>
/// Code-behind for the Employer Service Provider registration entry form (page 1).
/// </summary>
public partial class EspRegistrationEntry
{
    /// <summary>The registration model bound to the form.</summary>
    [Parameter]
    public required EspRegistrationModel Model { get; set; }

    private EditContext _editContext = default!;
    private ValidationMessageStore _manualMessageStore = default!;
    private bool _formSubmitted;
    private bool _hasValidationErrors;

    /// <summary>Tracks which fields have been interacted with so errors show on blur.</summary>
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(EspRegistrationModel.LegalName)] = "esp-legal-name",
        [nameof(EspRegistrationModel.Fein)] = "esp-fein",
        [nameof(EspRegistrationModel.PhoneNumber)] = "esp-phone",
        [nameof(EspRegistrationModel.FaxNumber)] = "esp-fax",
        [nameof(EspRegistrationModel.Website)] = "esp-website",
        [nameof(EspRegistrationModel.Email)] = "esp-email",
        [nameof(EspRegistrationModel.ConfirmEmail)] = "esp-confirm-email",
        [nameof(EspRegistrationModel.SidesNumber)] = "esp-sides",
        [nameof(AddressModel.Country)] = "esp-Country",
        [nameof(AddressModel.AddressLine1)] = "esp-AddressLine1",
        [nameof(AddressModel.AddressLine3)] = "esp-AddressLine3",
        [nameof(AddressModel.AddressLine4)] = "esp-AddressLine4",
        [nameof(AddressModel.City)] = "esp-City",
        [nameof(AddressModel.State)] = "esp-State",
        [nameof(AddressModel.Province)] = "esp-Province",
        [nameof(AddressModel.Zip)] = "esp-Zip",
        [nameof(AddressModel.PostalCode)] = "esp-PostalCode",
        [nameof(EspRegistrationModel.FirstName)] = "esp-first-name",
        [nameof(EspRegistrationModel.LastName)] = "esp-last-name",
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _editContext = new EditContext(Model);
        _manualMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            _hasValidationErrors = _editContext.GetValidationMessages().Any();
            StateHasChanged();
        };
    }

    /// <summary>Validates the form and returns true when no validation errors remain.</summary>
    public bool IsValid()
    {
        _formSubmitted = true;
        _manualMessageStore.Clear();

        var feinResult = FEINField.ValidateFEIN(Model.Fein);
        if (!feinResult.IsValid)
        {
            _manualMessageStore.Add(_editContext.Field(nameof(EspRegistrationModel.Fein)), feinResult.ErrorMessage);
        }

        // The nested AddressModel isn't reached by DataAnnotationsValidator, so validate it
        // explicitly using its own country-aware required attributes.
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

        if (Model.MailingAddress.Country == "United States"
            && !string.IsNullOrWhiteSpace(Model.MailingAddress.Zip)
            && !System.Text.RegularExpressions.Regex.IsMatch(Model.MailingAddress.Zip!, @"^\d{5}$"))
        {
            _manualMessageStore.Add(
                new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.Zip)),
                "Zip Code is not a valid format.");
        }

        if (Model.MailingAddress.Country == "Canada"
            && !string.IsNullOrWhiteSpace(Model.MailingAddress.PostalCode)
            && !System.Text.RegularExpressions.Regex.IsMatch(Model.MailingAddress.PostalCode!, @"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$"))
        {
            _manualMessageStore.Add(
                new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.PostalCode)),
                "Postal Code format is incorrect. Please enter a valid postal code in the format ANA NAN where \"A\" represents a letter and \"N\" represents a digit.");
        }

        _editContext.Validate();

        var isValid = !_editContext.GetValidationMessages().Any();
        _hasValidationErrors = !isValid;
        StateHasChanged();
        return isValid;
    }

    /// <summary>
    /// Resolves a property name to the <see cref="FieldIdentifier"/> that owns its validation
    /// messages, routing address fields to the nested <see cref="EspRegistrationModel.MailingAddress"/>.
    /// </summary>
    private FieldIdentifier ResolveField(string fieldName)
    {
        return fieldName switch
        {
            nameof(AddressModel.Country)
            or nameof(AddressModel.AddressLine1)
            or nameof(AddressModel.AddressLine2)
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

    /// <summary>
    /// Returns true when an address section's errors should be visible — after form submission
    /// or after the user has changed any field in that address.
    /// </summary>
    private bool IsAddressVisible(AddressModel address)
    {
        return _formSubmitted || _touchedFields.Any(f =>
        {
            return ReferenceEquals(f.Model, address);
        });
    }

    /// <summary>Validates email format and that the two email fields match, on blur.</summary>
    private void HandleEmailBlur()
    {
        var emailAttr = new EmailAddressAttribute();

        var emailField = _editContext.Field(nameof(EspRegistrationModel.Email));
        var confirmField = _editContext.Field(nameof(EspRegistrationModel.ConfirmEmail));

        _manualMessageStore.Clear(emailField);
        _manualMessageStore.Clear(confirmField);

        if (!string.IsNullOrWhiteSpace(Model.Email) && !emailAttr.IsValid(Model.Email))
        {
            _manualMessageStore.Add(emailField, "Enter a valid email address.");
        }

        if (!string.IsNullOrWhiteSpace(Model.ConfirmEmail) && !emailAttr.IsValid(Model.ConfirmEmail))
        {
            _manualMessageStore.Add(confirmField, "Enter a valid email address.");
        }

        if (emailAttr.IsValid(Model.Email) && emailAttr.IsValid(Model.ConfirmEmail)
            && !string.Equals(Model.Email, Model.ConfirmEmail, StringComparison.OrdinalIgnoreCase))
        {
            _manualMessageStore.Add(confirmField, "Email addresses do not match.");
        }

        _editContext.NotifyValidationStateChanged();
    }
}
