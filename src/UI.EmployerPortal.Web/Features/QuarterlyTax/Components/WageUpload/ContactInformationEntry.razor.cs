
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.WageUpload;
/// <summary>
/// Codebehind for Contact Entry Page
/// </summary>
public partial class ContactInformationEntry
{
    /// <summary>
    /// Gets or sets the primary data model for the form
    /// </summary>
    [Parameter]
    public required ContactModel Model { get; set; }

    /// <summary>
    /// Reporting Quarter
    /// </summary>
    [Parameter]
    public string ReportingQuarter { get; set; } = string.Empty;

    /// <summary>
    /// Created this for data exchange file upload
    /// </summary>
    [Parameter]
    public bool ShowHeader { get; set; } = true;

    /// <summary>
    /// Due date
    /// </summary>
    [Parameter]
    public DateTime? DueDate { get; set; }

    [Inject]
    private IContactInformationService ContactInformationService { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    /// <summary>
    /// Tracks if the form has been attempted to be submitted
    /// </summary>
    private EditContext _editContext = default!;
    private ValidationMessageStore _manualMessageStore = default!;
    private bool _formSubmitted = false;

    /// <summary>Tracks whether the current form state has any validation errors.</summary>
    private bool _hasValidationErrors = false;

    /// <summary>Tracks which fields have been interacted with so errors show on blur.</summary>
    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    /// <summary>Shared 10-digit phone format used by both Upload and Record phone fields.</summary>
    private readonly System.Text.RegularExpressions.Regex _phoneRegex = new(@"^\d{3}-\d{3}-\d{4}$");

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(ContactModel.BusinessName)] = "bus-name",
        ["AddressLine1"] = "AddressLine1",
        ["City"] = "City",
        ["Province"] = "Province",
        ["State"] = "State",
        ["Zip"] = "Zip",
        ["PostalCode"] = "PostalCode",
        [nameof(ContactModel.ContactName)] = "upload-contact-name",
        [nameof(ContactModel.UploadPhoneNumber)] = "upload-phone",
        [nameof(ContactModel.UploadEmail)] = "upload-email",
        [nameof(ContactModel.ConfirmUploadEmail)] = "upload-confirm-email",
        [nameof(ContactModel.RecordContactName)] = "record-contact-name",
        [nameof(ContactModel.RecordPhone)] = "record-phone",
        [nameof(ContactModel.RecordEmail)] = "record-email",
        [nameof(ContactModel.ConfirmationRecordEmail)] = "record-confirm-email"
    };

    /// <inheritdoc />

    protected override async Task OnInitializedAsync()
    {
        _editContext = new EditContext(Model);
        _manualMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            SyncRecordFieldIfNeeded(e.FieldIdentifier);

            if (e.FieldIdentifier.FieldName == nameof(ContactModel.UploadPhoneNumber))
            {
                ValidatePhoneNumberField(e.FieldIdentifier, Model.UploadPhoneNumber);
            }
            else if (e.FieldIdentifier.FieldName == nameof(ContactModel.RecordPhone))
            {
                if (!Model.SameAsFileUpload)
                {
                    ValidatePhoneNumberField(e.FieldIdentifier, Model.RecordPhone);
                }
            }
            else if (ReferenceEquals(e.FieldIdentifier.Model, Model.MailingAddress))
            {
                ValidateAddressFieldOnChange(e.FieldIdentifier);
            }
            else
            {
                ValidateRecordsFieldOnChange(e.FieldIdentifier);
            }

            _hasValidationErrors = _editContext.GetValidationMessages().Any();
            StateHasChanged();
        };
    }

    /// <summary>
    /// Validates the contact information form.
    /// </summary>
    public bool IsValid()
    {
        _formSubmitted = true;
        _manualMessageStore.Clear();

        //nested addressmodel - leverage it's own required attributes
        var addressContext = new ValidationContext(Model.MailingAddress);
        var addressErrors = new List<ValidationResult>();
        Validator.TryValidateObject(Model.MailingAddress, addressContext, addressErrors, validateAllProperties: true);

        // Guard against duplicate (field, message) pairs before adding to the store.
        var addedAddressMessages = new HashSet<(string FieldName, string Message)>();
        foreach (var error in addressErrors)
        {
            foreach (var memberName in error.MemberNames)
            {
                var key = (memberName, error.ErrorMessage!);
                if (addedAddressMessages.Add(key))
                {
                    _manualMessageStore.Add(new FieldIdentifier(Model.MailingAddress, memberName), error.ErrorMessage!);
                }
            }
        }

        if (Model.MailingAddress.Country == "United States" &&
                !string.IsNullOrWhiteSpace(Model.MailingAddress.Zip) &&
                !System.Text.RegularExpressions.Regex.IsMatch(Model.MailingAddress.Zip!, @"^\d{5}$"))
        {
            _manualMessageStore.Add(new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.Zip)), "Zip Code is not a valid format.");
        }

        if (Model.MailingAddress.Country == "Canada" &&
                !string.IsNullOrWhiteSpace(Model.MailingAddress.PostalCode) &&
                !System.Text.RegularExpressions.Regex.IsMatch(Model.MailingAddress.PostalCode!, @"^[A-Za-z]\d[A-Za-z] \d[A-Za-z]\d$"))
        {
            _manualMessageStore.Add(
                new FieldIdentifier(Model.MailingAddress, nameof(AddressModel.PostalCode)),
                "Postal Code format is incorrect. Please enter a valid postal code in the format ANA NAN where \"A\" represents a letter and \"N\" represents a digit.");
        }

        ValidatePhoneNumberField(_editContext.Field(nameof(ContactModel.UploadPhoneNumber)), Model.UploadPhoneNumber);

        if (!Model.SameAsFileUpload)
        {
            ValidatePhoneNumberField(_editContext.Field(nameof(ContactModel.RecordPhone)), Model.RecordPhone);
            ValidateRecordFieldsForSubmit();
        }

        _editContext.Validate();

        _editContext.NotifyValidationStateChanged();
        var isValid = !_editContext.GetValidationMessages().Any();
        _hasValidationErrors = !isValid;
        StateHasChanged();
        return isValid;
    }

    private FieldIdentifier ResolveField(string fieldName)
    {
        return fieldName switch
        {
            nameof(AddressModel.AddressLine1)
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

    private void HandleUploadEmailBlur()
    {
        var emailAttr = new EmailAddressAttribute();

        var uploadField = _editContext.Field(nameof(ContactModel.UploadEmail));
        var confirmUploadEmail = _editContext.Field(nameof(ContactModel.ConfirmUploadEmail));

        _manualMessageStore.Clear(uploadField);
        _manualMessageStore.Clear(confirmUploadEmail);

        if (!string.IsNullOrWhiteSpace(Model.UploadEmail) && !emailAttr.IsValid(Model.UploadEmail))
        {
            _manualMessageStore.Add(uploadField, "Enter a valid email address.");
        }
        if (!string.IsNullOrWhiteSpace(Model.ConfirmUploadEmail) && !emailAttr.IsValid(Model.ConfirmUploadEmail))
        {
            _manualMessageStore.Add(confirmUploadEmail, "Enter a valid confirm email address.");
        }
        if (emailAttr.IsValid(Model.UploadEmail) && emailAttr.IsValid(Model.ConfirmUploadEmail)
            && !string.Equals(Model.UploadEmail, Model.ConfirmUploadEmail, StringComparison.OrdinalIgnoreCase))
        {
            _manualMessageStore.Add(confirmUploadEmail, "Email addresses do not match.");
        }
        _editContext.NotifyValidationStateChanged();
    }

    private void HandleRecordEmailBlur()
    {
        var emailAttr = new EmailAddressAttribute();

        var recordEmail = _editContext.Field(nameof(ContactModel.RecordEmail));
        var confirmRecordEmail = _editContext.Field(nameof(ContactModel.ConfirmationRecordEmail));

        _manualMessageStore.Clear(recordEmail);
        _manualMessageStore.Clear(confirmRecordEmail);

        if (string.IsNullOrWhiteSpace(Model.RecordEmail))
        {
            _manualMessageStore.Add(recordEmail, "Record Email Address is required.");
        }
        else if (!string.IsNullOrWhiteSpace(Model.RecordEmail) && !emailAttr.IsValid(Model.RecordEmail))
        {
            _manualMessageStore.Add(recordEmail, "\"Enter a valid record email address.");
        }
        if (string.IsNullOrWhiteSpace(Model.ConfirmationRecordEmail))
        {
            _manualMessageStore.Add(confirmRecordEmail, "Confirm Record Email Address is required.");
        }
        else if (!string.IsNullOrWhiteSpace(Model.ConfirmationRecordEmail) && !emailAttr.IsValid(Model.ConfirmationRecordEmail))
        {
            _manualMessageStore.Add(confirmRecordEmail, "Enter a valid record email address.");
        }
        if (emailAttr.IsValid(Model.RecordEmail) && emailAttr.IsValid(Model.ConfirmationRecordEmail)
            && !string.Equals(Model.RecordEmail, Model.ConfirmationRecordEmail, StringComparison.OrdinalIgnoreCase))
        {
            _manualMessageStore.Add(confirmRecordEmail, "Record email addresses do not match.");
        }
        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Validates a phone number field (required + 10-digit format) as a single source of
    /// truth, used both on live field-change and at submit time, for Upload and Record phone.
    /// Ensures exactly one message is ever shown for these fields.
    /// </summary>
    private void ValidatePhoneNumberField(FieldIdentifier field, string? value)
    {
        _manualMessageStore.Clear(field);

        if (!string.IsNullOrWhiteSpace(value) && !_phoneRegex.IsMatch(value))
        {
            _manualMessageStore.Add(field, "Enter a valid 10-digit phone number.");
        }

        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Clears any existing message for a MailingAddress field as soon as it changes,
    /// so a stale "required" message disappears immediately as the user types.
    /// &lt;DataAnnotationsValidator/&gt; already re-validates MailingAddress fields
    /// natively on change (via its own internal store), so this method only clears
    /// our manual store's entry — it must never re-add one, or the message renders
    /// twice (once from each store).
    /// </summary>
    private void ValidateAddressFieldOnChange(FieldIdentifier field)
    {
        _manualMessageStore.Clear(field);
        _editContext.NotifyValidationStateChanged();
    }

    private void ValidateRecordsFieldOnChange(FieldIdentifier field)
    {
        if (Model.SameAsFileUpload)
        {
            return;
        }

        // RecordPhone is handled separately via ValidatePhoneNumberField (required + format).
        var recordsFields = new Dictionary<string, Func<string?>>
        {
            [nameof(ContactModel.RecordContactName)] = () =>
            {
                return Model.RecordContactName;
            },
            [nameof(ContactModel.RecordEmail)] = () =>
            {
                return Model.RecordEmail;
            },
            [nameof(ContactModel.ConfirmationRecordEmail)] = () =>
            {
                return Model.ConfirmationRecordEmail;
            }
        };

        if (!recordsFields.TryGetValue(field.FieldName, out var getValue))
        {
            return;
        }

        _manualMessageStore.Clear(field);

        if (string.IsNullOrWhiteSpace(getValue()))
        {
            var label = field.FieldName switch
            {
                nameof(ContactModel.RecordContactName) => "Record Contact Name",
                nameof(ContactModel.RecordEmail) => "Record Email Address",
                nameof(ContactModel.ConfirmationRecordEmail) => "Confirm Record Email Address",
                _ => field.FieldName
            };

            _manualMessageStore.Add(field, $"{label} is required.");
        }

        _editContext.NotifyValidationStateChanged();
    }

    /// <summary>
    /// Comprehensive safety-net validation for Record Contact Name / Email / Confirm Email
    /// at submit time — ensures correctness even for fields the user never touched or blurred.
    /// RecordPhone is validated separately via ValidatePhoneNumberField.
    /// </summary>
    private void ValidateRecordFieldsForSubmit()
    {
        var emailAttr = new EmailAddressAttribute();

        if (string.IsNullOrWhiteSpace(Model.RecordContactName))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.RecordContactName)), "Record Contact Name is required.");
        }

        if (string.IsNullOrWhiteSpace(Model.RecordEmail))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.RecordEmail)), "Record Email Address is required.");
        }
        else if (!emailAttr.IsValid(Model.RecordEmail))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.RecordEmail)), "Enter a valid record email address.");
        }

        if (string.IsNullOrWhiteSpace(Model.ConfirmationRecordEmail))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.ConfirmationRecordEmail)), "Confirm Record Email Address is required.");
        }
        else if (!emailAttr.IsValid(Model.ConfirmationRecordEmail))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.ConfirmationRecordEmail)), "Enter a valid record email address.");
        }

        if (emailAttr.IsValid(Model.RecordEmail) && emailAttr.IsValid(Model.ConfirmationRecordEmail)
            && !string.Equals(Model.RecordEmail, Model.ConfirmationRecordEmail, StringComparison.OrdinalIgnoreCase))
        {
            _manualMessageStore.Add(_editContext.Field(nameof(ContactModel.ConfirmationRecordEmail)), "Record email addresses do not match.");
        }
    }

    /// <summary> Returns true when a field's errors should be visible — after form submission or after the user has interacted with that field./// </summary>
    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
    }

    /// <summary> Returns true when an address section's errors should be visible —  after form submission or after the user has changed any field in that address. /// </summary>
    private bool IsAddressVisible(AddressModel address)
    {
        return _formSubmitted || _touchedFields.Any(f =>
        {
            return ReferenceEquals(f.Model, address);
        });
    }

    /// <summary>Keeps the Records contact fields in sync with the Upload contact fields while "Same as File Upload Contact" is checked.</summary>
    private void SyncRecordFieldIfNeeded(FieldIdentifier field)
    {
        if (!Model.SameAsFileUpload)
        {
            return;
        }

        switch (field.FieldName)
        {
            case nameof(ContactModel.ContactName):
                Model.RecordContactName = Model.ContactName;
                break;
            case nameof(ContactModel.UploadPhoneNumber):
                Model.RecordPhone = Model.UploadPhoneNumber;
                break;
            case nameof(ContactModel.UploadExt):
                Model.RecordExt = Model.UploadExt;
                break;
            case nameof(ContactModel.UploadEmail):
                Model.RecordEmail = Model.UploadEmail;
                break;
            case nameof(ContactModel.ConfirmUploadEmail):
                Model.ConfirmationRecordEmail = Model.ConfirmUploadEmail;
                break;
        }
    }

    /// <summary>Handles the "Same as File Upload Contact" checkbox.When checked, copies upload contact values into the records contact fields./// </summary>
    private void OnCheckboxChanged(bool isChecked)
    {
        Model.SameAsFileUpload = isChecked;
        var recordFieldNames = new[]
        {
            nameof(ContactModel.RecordContactName),
            nameof(ContactModel.RecordPhone),
            nameof(ContactModel.RecordEmail),
            nameof(ContactModel.ConfirmationRecordEmail),
        };

        if (isChecked)
        {
            Model.RecordContactName = Model.ContactName;
            Model.RecordPhone = Model.UploadPhoneNumber;
            Model.RecordExt = Model.UploadExt;
            Model.RecordEmail = Model.UploadEmail;
            Model.ConfirmationRecordEmail = Model.ConfirmUploadEmail;

            foreach (var fieldName in recordFieldNames)
            {
                _manualMessageStore.Clear(_editContext.Field(fieldName));
            }
            _editContext.NotifyValidationStateChanged();
        }
        else
        {
            Model.RecordContactName = string.Empty;
            Model.RecordPhone = string.Empty;
            Model.RecordExt = string.Empty;
            Model.RecordEmail = string.Empty;
            Model.ConfirmationRecordEmail = string.Empty;

            foreach (var fieldname in recordFieldNames)
            {
                var field = _editContext.Field(fieldname);
                _touchedFields.Add(field);

                if (fieldname == nameof(ContactModel.RecordPhone))
                {
                    ValidatePhoneNumberField(field, Model.RecordPhone);
                }
                else
                {
                    ValidateRecordsFieldOnChange(field);
                }
            }
        }

        if (_formSubmitted)
        {
            IsValid();
        }
        else
        {
            StateHasChanged();
        }
    }
}
