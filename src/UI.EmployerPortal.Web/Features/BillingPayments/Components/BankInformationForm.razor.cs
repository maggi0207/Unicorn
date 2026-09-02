using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Razor.SharedComponents.Inputs;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Components;

/// <summary>
/// Code-behind for the Bank Information.
/// </summary>
public partial class BankInformationForm
{
    [Inject] private IBankAccountOrchestrator BankAccountOrchestrator { get; set; } = default!;
    /// <summary>
    /// On Save
    /// </summary>
    [Parameter] public EventCallback<SavedBankAccount> OnSaved { get; set; }
    /// <summary>
    /// On Back
    /// </summary>
    [Parameter] public EventCallback OnBack { get; set; }
    /// <summary>
    /// On Cancel
    /// </summary>
    [Parameter] public EventCallback OnCancel { get; set; }

    /// <summary>
    /// When non-zero, the form is in edit mode and pre-populates from the existing account.
    /// </summary>
    [Parameter] public int BankAccountSk { get; set; }

    /// <summary>Show hide back button</summary>
    [Parameter] public bool Showback { get; set; } = false;

    private BankAccountModel _model = new();
    private EditContext _editContext = default!;
    private bool IsEditMode => BankAccountSk > 0;
    private bool _loadFailed;
    private bool _showValidation;
    private bool _showValidationSummary;
    private List<string> _validationErrors = [];
    private readonly List<string> _validationFieldIds = [];
    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private bool _isSaving;
    private bool _helpDrawerOpen;
    private bool _iatDrawerOpen;
    private bool _showIatVerificationModal;
    private string? _saveError;
    private bool _iAccept;
    private bool _showIAcceptError;
    private IReadOnlyList<PendingPayment> _pendingPayments = [];
    private string _pendingSortColumn = "settlementDate";
    private bool _pendingSortAscending = true;

    private readonly List<SelectOption> _accountTypeOptions =
    [
        new SelectOption { Value = "Checking", Text = "Checking" },
        new SelectOption { Value = "Savings",  Text = "Savings"  }
    ];

    private List<SelectOption> _countryOptions = [];
    private List<SelectOption> _usStateOptions = [];
    private List<SelectOption> _canadaProvinceOptions = [];

    private int _usaCodeSk;
    private int _canadaCodeSk;

    private bool IsUsa => _usaCodeSk != 0 && _model.IatCountryCode == _usaCodeSk;
    private bool IsCanada => _canadaCodeSk != 0 && _model.IatCountryCode == _canadaCodeSk;

    /// <summary>OnInitialized</summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(_model);
        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            if (_showValidationSummary)
            {
                _editContext.Validate();
                _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();
                _validationFieldIds.Clear();

                foreach (var message in _validationErrors)
                {
                    _validationFieldIds.Add(GetFieldIdForMessage(message));
                }
                _showValidationSummary = _validationErrors.Any();
            }
            StateHasChanged();
        };
    }
    /// <inheritdoc/>
    protected override async Task OnInitializedAsync()
    {
        var countryTask = BankAccountOrchestrator.GetCountryCodesAsync();
        var usStateTask = BankAccountOrchestrator.GetUSStateCodesAsync();
        var canadaTask = BankAccountOrchestrator.GetCanadianStateCodesAsync();
        var editTask = IsEditMode
            ? BankAccountOrchestrator.GetBankAccountForEditAsync(BankAccountSk)
            : Task.FromResult<BankAccountModel?>(null);
        var pendingTask = IsEditMode
            ? BankAccountOrchestrator.GetPendingPaymentsAsync(BankAccountSk)
            : Task.FromResult<IReadOnlyList<PendingPayment>>([]);

        await Task.WhenAll(countryTask, usStateTask, canadaTask, editTask, pendingTask);

        var countries = await countryTask;
        _countryOptions = countries.Select(c =>
        {
            return new SelectOption { Value = c.Value, Text = c.Text };
        }).ToList();
        _usStateOptions = [.. await usStateTask];
        _canadaProvinceOptions = [.. await canadaTask];

        _usaCodeSk = int.TryParse(
            countries.FirstOrDefault(c =>
            {
                return c.ShortCode.Equals("US", StringComparison.OrdinalIgnoreCase);
            })?.Value,
            out var usaSk) ? usaSk : 0;

        _canadaCodeSk = int.TryParse(
            countries.FirstOrDefault(c =>
            {
                return c.ShortCode.Equals("CA", StringComparison.OrdinalIgnoreCase);
            })?.Value,
            out var canadaSk) ? canadaSk : 0;

        if (IsEditMode)
        {
            _pendingPayments = await pendingTask;

            var existing = await editTask;
            if (existing is null)
            {
                _loadFailed = true;
                return;
            }

            existing.IatCountryIsUsa = existing.IatCountryCode == _usaCodeSk;
            existing.IatCountryIsCanada = existing.IatCountryCode == _canadaCodeSk;

            if (!existing.IatCountryIsUsa)
            {
                existing.IatStateCode = 0;
            }
            if (!existing.IatCountryIsCanada)
            {
                existing.IatProvinceCode = 0;
            }

            _model = existing;
            _editContext = new EditContext(_model);
            _editContext.OnFieldChanged += (_, e) =>
            {
                _touchedFields.Add(e.FieldIdentifier);
                if (_showValidationSummary)
                {
                    _editContext.Validate();
                    _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();
                    _validationFieldIds.Clear();

                    foreach (var message in _validationErrors)
                    {
                        _validationFieldIds.Add(GetFieldIdForMessage(message));
                    }
                    _showValidationSummary = _validationErrors.Any();
                }
                StateHasChanged();
            };
        }
    }

    private async Task HandleRoutingBlur()
    {
        if (_model.RoutingNumber?.Length == 9)
        {
            _model.BankName = await BankAccountOrchestrator.LookupBankNameAsync(_model.RoutingNumber);
        }
    }

    private void HandleIatChanged(bool value)
    {
        _model.IsInternational = value;

        if (!value)
        {
            _model.IatCountryCode = 0;
            _model.IatCountryIsUsa = false;
            _model.IatCountryIsCanada = false;
            _model.IatStreetAddress = null;
            _model.IatCity = null;
            _model.IatPostalCode = null;
            _model.IatStateCode = 0;
            _model.IatProvinceCode = 0;
        }
    }

    private void HandleCountryChanged(string value)
    {
        _model.IatCountryCode = int.TryParse(value, out var code) ? code : 0;
        _model.IatCountryIsUsa = _model.IatCountryCode == _usaCodeSk;
        _model.IatCountryIsCanada = _model.IatCountryCode == _canadaCodeSk;
        _model.IatStateCode = 0;
        _model.IatProvinceCode = 0;
    }

    private void HandleStateChanged(string value)
    {
        _model.IatStateCode = int.TryParse(value, out var code) ? code : 0;
    }

    private void HandleProvinceChanged(string value)
    {
        _model.IatProvinceCode = int.TryParse(value, out var code) ? code : 0;
    }

    private void HandleOpenHelp()
    {
        _helpDrawerOpen = true;
    }

    private void HandleCloseHelp()
    {
        _helpDrawerOpen = false;
    }

    private void HandleOpenIat()
    {
        _iatDrawerOpen = true;
    }

    private void HandleCloseIat()
    {
        _iatDrawerOpen = false;
    }

    private async Task HandleValidSubmit()
    {
        _showValidation = true;
        _showValidationSummary = false;

        if (IsEditMode && _pendingPayments.Count > 0 && !_iAccept)
        {
            _showIAcceptError = true;
            return;
        }

        _showIAcceptError = false;

        if (_model.IsInternational)
        {
            _showIatVerificationModal = true;
            return;
        }

        await SaveAsync();
    }

    private void HandleIatVerificationCancel()
    {
        _showIatVerificationModal = false;
        HandleIatChanged(false);
    }

    private async Task HandleIatVerificationConfirm()
    {
        _showIatVerificationModal = false;
        await SaveAsync();
    }

    private void OnFormInvalid()
    {
        _showValidation = true;
        _showValidationSummary = true;
        _validationErrors = _editContext.GetValidationMessages().Distinct().ToList();
        _validationFieldIds.Clear();

        foreach (var message in _validationErrors)
        {
            _validationFieldIds.Add(GetFieldIdForMessage(message));
        }
    }

    private static string GetFieldIdForMessage(string message)
    {
        return message switch
        {
            var m when m.Contains("Nickname", StringComparison.OrdinalIgnoreCase) => "bank-nickname",
            var m when m.Contains("Routing Number", StringComparison.OrdinalIgnoreCase) => "bank-routing",
            var m when m.Contains("Account Number", StringComparison.OrdinalIgnoreCase) => "bank-account-number",
            var m when m.Contains("Re-enter", StringComparison.OrdinalIgnoreCase) => "bank-confirm-account",
            var m when m.Contains("match", StringComparison.OrdinalIgnoreCase) => "bank-confirm-account",
            var m when m.Contains("Account Type", StringComparison.OrdinalIgnoreCase) => "bank-account-type",
            var m when m.Contains("Country", StringComparison.OrdinalIgnoreCase) => "iat-country",
            var m when m.Contains("Street Address", StringComparison.OrdinalIgnoreCase) => "iat-street",
            var m when m.Contains("City", StringComparison.OrdinalIgnoreCase) => "iat-city",
            var m when m.Contains("Postal Code", StringComparison.OrdinalIgnoreCase) => "iat-postal",
            _ => string.Empty
        };
    }

    private async Task SaveAsync()
    {
        _isSaving = true;
        _saveError = null;

        var result = IsEditMode
            ? await BankAccountOrchestrator.EditBankAccountAsync(_model)
            : await BankAccountOrchestrator.AddBankAccountAsync(_model);

        _isSaving = false;

        if (!result.Success)
        {
            _saveError = result.ErrorMessage;
            return;
        }

        var saved = new SavedBankAccount
        {
            Nickname = _model.Nickname ?? string.Empty,
            RoutingNumber = _model.RoutingNumber ?? string.Empty,
            MaskedAccountNumber = MaskAccountNumber(_model.AccountNumber),
            BankName = _model.BankName ?? string.Empty,
            AccountType = _model.AccountType ?? string.Empty
        };

        await OnSaved.InvokeAsync(saved);
    }

    private void HandleBack()
    {
        _ = OnBack.InvokeAsync();
    }

    private static string MaskAccountNumber(string? accountNumber)
    {
        return string.IsNullOrWhiteSpace(accountNumber) || accountNumber.Length < 4
            ? accountNumber ?? string.Empty
            : $"*******{accountNumber[^4..]}";
    }

    private IEnumerable<PendingPayment> GetSortedPendingPayments()
    {
        return _pendingSortColumn switch
        {
            "confirmationNumber" => SortPendingBy(p =>
            {
                return p.ConfirmationNumber;
            }),
            "amount" => SortPendingBy(p =>
            {
                return p.Amount;
            }),
            _ => SortPendingBy(p =>
            {
                return p.SettlementDate;
            }),
        };
    }

    private IOrderedEnumerable<PendingPayment> SortPendingBy<TKey>(Func<PendingPayment, TKey> keySelector)
    {
        return _pendingSortAscending
            ? _pendingPayments.OrderBy(keySelector)
            : _pendingPayments.OrderByDescending(keySelector);
    }

    private void SortPending(string column)
    {
        if (_pendingSortColumn == column)
        {
            _pendingSortAscending = !_pendingSortAscending;
        }
        else
        {
            _pendingSortColumn = column;
            _pendingSortAscending = true;
        }
    }

    private MarkupString GetPendingSortIcon(string column)
    {
        string path;
        string altText;

        if (_pendingSortColumn == column)
        {
            path = _pendingSortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _pendingSortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetPendingAriaSort(string column)
    {
        return _pendingSortColumn != column ? null : _pendingSortAscending ? "ascending" : "descending";
    }

    private void HandlePendingHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            SortPending(column);
        }
    }

}

