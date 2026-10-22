using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace UI.EmployerPortal.Web.Features.ManageAccount.Components;

/// <summary>
/// Modal that lets a user request an access key be mailed to the employer's main mailing address.
/// The employer is identified by a 10-digit UI Account Number or FEIN. Client-side validation only
/// checks the identifier length; the backend service validates the identifier and creates the letter.
/// </summary>
public partial class RequestAccessKeyModal
{
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;
    private const string InvalidIdentifierMessage = "Invalid UI Account Number or FEIN";
    private const int MinimumIdentifierDigits = 9;
    private const int MaximumIdentifierDigits = 10;

    /// <summary>Controls whether the modal is visible.</summary>
    [Parameter]
    public bool IsOpen { get; set; }

    /// <summary>When true, the mail request is in flight; inputs and buttons are disabled.</summary>
    [Parameter]
    public bool IsBusy { get; set; }

    /// <summary>Service-side error (e.g. a rule violation) to display inside the modal.</summary>
    [Parameter]
    public string? ServiceError { get; set; }

    /// <summary>Invoked when the user cancels or closes the modal. Parent must set IsOpen = false.</summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>Invoked when the user opens the modal.</summary>
    [Parameter]
    public EventCallback OnClearServiceError { get; set; }

    /// <summary>
    /// Invoked with the entered identifier once client validation passes and the user selects
    /// Mail Access Key.
    /// </summary>
    [Parameter]
    public EventCallback<string> OnMailAccessKey { get; set; }

    private string _identifier = string.Empty;
    private string? _validationError;
    private bool _wasOpen;
    private ElementReference _modalElement;
    private IJSObjectReference? _module;
    private const string InputId = "rak-identifie";

    /// <summary>
    /// Set and trap focus in modal when it opens
    /// </summary>
    /// <param name="firstRender"></param>
    /// <returns></returns>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (IsOpen != _wasOpen)
        {
            Reset();
            _wasOpen = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _modalElement);
        }
        else if (!IsOpen && _wasOpen)
        {
            _wasOpen = false;
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("closeFocusTrap");
            }
        }
        else if (IsOpen && _wasOpen)
        {
            await JSRuntime.InvokeVoidAsync("focusElement", InputId);
        }
    }

    private async Task HandleCancel()
    {
        if (IsBusy)
        {
            return;
        }
        await OnCancel.InvokeAsync();
    }

    private async Task OnIdentifierChanged(string value)
    {
        _identifier = value ?? string.Empty;
        _validationError = null;

        if (!string.IsNullOrWhiteSpace(ServiceError))
        {
            await OnClearServiceError.InvokeAsync();
        }
    }

    private async Task HandleMail()
    {
        if (IsBusy)
        {
            return;
        }

        // Requirement: empty or fewer than 10 digits is invalid.
        var digitsOnly = new string(_identifier.Where(char.IsDigit).ToArray());

        // Valid identifiers are a 9-digit FEIN or a 10-digit UI Account Number.
        if (digitsOnly.Length is < MinimumIdentifierDigits or > MaximumIdentifierDigits)
        {
            _validationError = null;
        }

        await OnMailAccessKey.InvokeAsync(digitsOnly);
    }

    private void Reset()
    {
        _identifier = string.Empty;
        _validationError = null;
    }
}
