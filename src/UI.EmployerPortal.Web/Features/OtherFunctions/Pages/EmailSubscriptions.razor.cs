using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Razor.SharedComponents.Validation;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.Shared.OtherFunctions.Services;


namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// Email subscriptions page
/// </summary>
public partial class EmailSubscriptions
{
    /// <summary>
    /// Model
    /// </summary>
    [Parameter]
    public EmailSubscription Model { get; set; } = new();


    /// <summary>
    /// Navigation Manager
    /// </summary>
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;


    [Inject]
    private IEmailSubscriptionService EmailSubscriptionService { get; set; } = default!;

    /// <summary>
    /// Tracks if the form has been attempted to be submitted
    /// </summary>
    private EditContext? _editContext;

    /// <summary>Reference to CustomValidator for displaying nested object errors.</summary>

    private CustomValidator? _customValidator;

    /// <summary>Tracks whether the form has been submitted at least once.</summary>

    private bool _formSubmitted = false;

    /// <summary>Tracks whether the current form state has any validation errors.</summary>

    private bool _hasValidationErrors = false;
    /// <summary>Tracks which fields have been interacted with so errors show on blur.</summary>

    private readonly HashSet<FieldIdentifier> _touchedFields = new();

    private bool _showValidationSummary;

    private readonly Dictionary<string, string> _validationFieldIds = new()
    {
        {nameof(EmailSubscription.EmailAddress), "record-email" },
        {nameof(EmailSubscription.ConfirmEmailAddress), "record-confirmemail" }

    };


    ///<inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        Model = await EmailSubscriptionService.GetEmailSubscriptions() ?? new EmailSubscription();
        _editContext = new EditContext(Model);

        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);

            if (e.FieldIdentifier.FieldName == nameof(EmailSubscription.EmailAddress))
            {
                var confirmEmailField = new FieldIdentifier(Model, nameof(EmailSubscription.ConfirmEmailAddress));
                _touchedFields.Add(confirmEmailField);
                _editContext.NotifyFieldChanged(confirmEmailField);
            }
            _hasValidationErrors = _editContext.GetValidationMessages().Any();
            StateHasChanged();
        };
    }

    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
    }
    private void HandleContinue()
    {
        _formSubmitted = true;
        _hasValidationErrors = false;
        StateHasChanged();
    }

    /// <summary>Called by EditForm when top-level validation fails.</summary>

    private void OnInvalid()
    {
        _formSubmitted = true;
        _hasValidationErrors = true;
        StateHasChanged();
    }

    private async Task HandleSave()
    {
        _formSubmitted = true;

        var isValid = _editContext!.Validate();

        _showValidationSummary = !isValid;
        _hasValidationErrors = !isValid;

        if (!isValid)
        {
            StateHasChanged();
            return;
        }

        await EmailSubscriptionService.SaveEmailSubscriptions(Model);

        NavigationManager.NavigateTo("manage-email-subscription-confirmation");
    }

    private async Task HandleCancel()
    {
        NavigationManager.NavigateTo("other-functions/select-other-functions");

        await Task.CompletedTask;
    }



}
