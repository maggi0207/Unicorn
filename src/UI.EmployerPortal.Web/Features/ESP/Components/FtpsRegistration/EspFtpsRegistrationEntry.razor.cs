using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Razor.SharedComponents.Model;
using UI.EmployerPortal.Web.Features.ESP.Models;
using UI.EmployerPortal.Web.Features.ESP.Pages.FtpsRegistration;
using UI.EmployerPortal.Web.Features.ESP.Services;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using UI.EmployerPortal.Web.Logging;

namespace UI.EmployerPortal.Web.Features.ESP.Components.FtpsRegistration;

/// <summary>
/// Code-behind for the Employer Service Provider FTPS registration entry form (page 1).
/// </summary>
public partial class EspFtpsRegistrationEntry
{
    [Inject] private IEspFtpsRegistrationService EspFtpsRegistrationService { get; set; } = default!;
    [Inject] private IUserAccountService UserAccountService { get; set; } = default!;
    [Inject] private ILogger<EspFtpsRegistration> Logger { get; set; } = default!;

    /// <summary>The registration model bound to the form.</summary>
    [Parameter]
    public required EspFtpsRegistrationModel Model { get; set; }

    /// <summary>The parent's loading state</summary>
    [Parameter] public bool IsLoading { get; set; }

    /// <summary>Callback to change parent's model state</summary>
    [Parameter] public EventCallback<EspFtpsRegistrationModel> ModelChanged { get; set; }

    /// <summary>Callback to change parent's loading state</summary>
    [Parameter] public EventCallback<bool> IsLoadingChanged { get; set; }

    private EditContext _editContext = default!;
    private ValidationMessageStore _manualMessageStore = default!;
    private bool _formSubmitted;
    private bool _hasValidationErrors;

    /// <summary>US states and territories (Canadian provinces excluded).</summary>
    private static readonly HashSet<string> CanadianProvinceCodes =
        ["AB", "BC", "MB", "NB", "NL", "NT", "NS", "NU", "ON", "PE", "QC", "SK", "YT"];

    private static readonly List<SelectOption> States =
        AddressModel.States.Where(s =>
        {
            return !CanadianProvinceCodes.Contains(s.Value);
        }).ToList();

    private static readonly List<SelectOption> Provinces =
        AddressModel.States.Where(s =>
        {
            return CanadianProvinceCodes.Contains(s.Value);
        }).ToList();

    // Choose state v.s provinces by country - fallback to U.S. states is intentional
    private List<SelectOption> StateOrProvinceOptions => (Model.Country ?? string.Empty) == "Canada" ? Provinces : States;

    // Strong types for country select values
    private const string UnitedStates = "United States";
    private const string Canada = "Canada";
    private const string OtherInternational = "Other International";

    // FTPS registration only supports domestic addresses, so exclude the international option.
    private static readonly List<SelectOption> CountryOptions =
        [.. AddressModel.Countries.Where(c => c.Value != OtherInternational)];

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(EspFtpsRegistrationModel.BusinessName)] = "esp-ftps-business-name",
        [nameof(EspFtpsRegistrationModel.Country)] = "esp-ftps-country",
        [nameof(EspFtpsRegistrationModel.AddressLine1)] = "esp-ftps-address-line1",
        [nameof(EspFtpsRegistrationModel.AddressLine2)] = "esp-ftps-address-line2",
        [nameof(EspFtpsRegistrationModel.City)] = "esp-ftps-city",
        [nameof(EspFtpsRegistrationModel.State)] = "esp-ftps-state",
        [nameof(EspFtpsRegistrationModel.ZipCode)] = "esp-ftps-zip",
        [nameof(EspFtpsRegistrationModel.ZipExt)] = "esp-ftps-zip-ext",
        [nameof(EspFtpsRegistrationModel.CanadianPostalCode)] = "esp-ftps-ca-postal-code",
        [nameof(EspFtpsRegistrationModel.FaxNumber)] = "esp-ftps-fax",
        [nameof(EspFtpsRegistrationModel.UploadContactName)] = "esp-ftps-upload-name",
        [nameof(EspFtpsRegistrationModel.UploadContactPhoneNumber)] = "esp-ftps-upload-phone",
        [nameof(EspFtpsRegistrationModel.UploadContactExtension)] = "esp-ftps-upload-extension",
        [nameof(EspFtpsRegistrationModel.UploadContactEmail)] = "esp-ftps-upload-email",
        [nameof(EspFtpsRegistrationModel.UploadContactConfirmEmail)] = "esp-ftps-upload-confirm-email",
        [nameof(EspFtpsRegistrationModel.QuestionsContactName)] = "esp-ftps-questions-name",
        [nameof(EspFtpsRegistrationModel.QuestionsContactPhoneNumber)] = "esp-ftps-questions-phone",
        [nameof(EspFtpsRegistrationModel.QuestionsContactExtension)] = "esp-ftps-questions-extension",
        [nameof(EspFtpsRegistrationModel.QuestionsContactEmail)] = "esp-ftps-questions-email",
        [nameof(EspFtpsRegistrationModel.QuestionsContactConfirmEmail)] = "esp-ftps-questions-confirm-email",
    };

    /// <inheritdoc />
    protected override async Task OnInitializedAsync()
    {
        // Always bind the EditContext to the current model so DataAnnotationsValidator
        // has its required cascading value, even for the new-registrant path where no
        // existing registration is loaded below.
        InitializeEditContext();

        if (string.IsNullOrWhiteSpace(Model.BusinessName))
        {
            await SetLoading(true);
            try
            {
                var existing = await EspFtpsRegistrationService.GetFtpUserForCurrentUser();
                if (existing is not null)
                {
                    Model = existing;
                    await ModelChanged.InvokeAsync(Model);
                    InitializeEditContext(); // rebind EditContext to the newly-loaded model
                }
            }
            catch (Exception ex)
            {
                LogErrorGettingFtpUserForCurrentUser(Logger, UserAccountService.GetUserSKClaim(), ex);
            }
            finally
            {
                await SetLoading(false);
                StateHasChanged();
            }
        }
    }

    private void InitializeEditContext()
    {
        _editContext = new EditContext(Model);
        _manualMessageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += (_, _) =>
        {
            _hasValidationErrors = _editContext.GetValidationMessages().Any();
            StateHasChanged();
        };
    }

    /// <summary>
    /// Copy the file upload contact to the records questions contact if same
    /// </summary>
    private void HandleIsSameAsFileUploadContact(bool value)
    {
        Model.IsSameAsFileUploadContact = value;
        if (value)
        {
            Model.QuestionsContactName = Model.UploadContactName;
            Model.QuestionsContactPhoneNumber = Model.UploadContactPhoneNumber;
            Model.QuestionsContactExtension = Model.UploadContactExtension;
            Model.QuestionsContactEmail = Model.UploadContactEmail;
            Model.QuestionsContactConfirmEmail = Model.UploadContactConfirmEmail;
        }
        else
        {
            Model.QuestionsContactName = string.Empty;
            Model.QuestionsContactPhoneNumber = string.Empty;
            Model.QuestionsContactExtension = string.Empty;
            Model.QuestionsContactEmail = string.Empty;
            Model.QuestionsContactConfirmEmail = string.Empty;
        }

        StateHasChanged();
    }

    private void HandleCountryChange()
    {
        Model.State = string.Empty;
        Model.ZipCode = string.Empty;
        Model.ZipExt = string.Empty;
        Model.CanadianPostalCode = string.Empty;
        _editContext.Validate();
        var isValid = !_editContext.GetValidationMessages().Any();
        _hasValidationErrors = !isValid;
        StateHasChanged();
    }

    /// <summary>Validates the form and returns true when no validation errors remain.</summary>
    public bool IsValid()
    {
        _formSubmitted = true;
        _manualMessageStore.Clear();

        _editContext.Validate();

        var isValid = !_editContext.GetValidationMessages().Any();
        _hasValidationErrors = !isValid;
        StateHasChanged();
        return isValid;
    }

    private async Task SetLoading(bool value)
    {
        IsLoading = value;
        await IsLoadingChanged.InvokeAsync(value);
    }

    [LoggerMessage(
        EventId = LogEventIds.ObtainFileContactByTypeServiceException,
        Level = LogLevel.Error,
        Message = "Failed to determine whether there is an FTP user for {SecureUserSK}.")]
    private static partial void LogErrorGettingFtpUserForCurrentUser(ILogger logger, int secureUserSK, Exception ex);
}
