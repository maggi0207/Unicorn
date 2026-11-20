using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Models;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.AuditFileUpload.Components;

/// <summary>
/// Form shown when no employer is selected, allowing the user to identify their UI account
/// by entering a UI Account Number and FEIN before proceeding with an audit file upload.
/// </summary>
public partial class AuditFileAccountLookup : IDisposable
{
    /// <summary>
    /// Raised when the user submits valid credentials; carries the entered
    /// UI Account Number and FEIN so the host page can perform the employer lookup.
    /// </summary>
    [Parameter]
    public EventCallback<(string UIAccountNumber, string FEIN)> OnContinue { get; set; }

    /// <summary>
    /// Raised when the user clicks CANCEL.
    /// </summary>
    [Parameter]
    public EventCallback OnCancel { get; set; }

    /// <summary>
    /// Errors returned by the employer lookup service, displayed above the form fields.
    /// </summary>
    [Parameter]
    public IReadOnlyList<string> ExternalErrors { get; set; } = [];

    /// <summary>
    /// InitialUIAccountNumber
    /// </summary>
    [Parameter]
    public string? InitialUIAccountNumber { get; set; }

    /// <summary>
    /// InitialFein
    /// </summary>
    [Parameter]
    public string? InitialFein { get; set; }

    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private readonly AuditAccountLookupModel _model = new();
    private EditContext _editContext = default!;
    private IReadOnlyList<string> _appliedExternalErrors = [];
    private List<string> _externalErrors = [];
    private bool _formSubmitted;
    private bool _hasErrors;

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(AuditAccountLookupModel.UIAccountNumber)] = "audit-lookup-ui-account",
        [nameof(AuditAccountLookupModel.FEIN)] = "audit-lookup-fein"
    };

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _model.UIAccountNumber = InitialUIAccountNumber ?? string.Empty;
        _model.FEIN = InitialFein ?? string.Empty;

        _editContext = new EditContext(_model);
        _editContext.OnFieldChanged += HandleFieldChanged;
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        if (ExternalErrors.SequenceEqual(_appliedExternalErrors))
        {
            return;
        }
        _appliedExternalErrors = ExternalErrors;
        _externalErrors = ExternalErrors.ToList();
    }



    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        _externalErrors.Clear();
        _appliedExternalErrors = [];
        _editContext.NotifyValidationStateChanged();
        StateHasChanged();
    }

    private async Task HandleContinue()
    {
        _externalErrors.Clear();
        _formSubmitted = true;
        var isValid = _editContext.Validate();
        _hasErrors = !isValid;

        if (!isValid)
        {
            return;
        }

        await OnContinue.InvokeAsync((_model.UIAccountNumber, _model.FEIN));
    }

    private async Task HandleCancel()
    {
        NavigationManager.NavigateTo("landing-page");
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _editContext.OnFieldChanged -= HandleFieldChanged;
    }
}
