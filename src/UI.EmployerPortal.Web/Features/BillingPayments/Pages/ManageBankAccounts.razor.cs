using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Server.ProtectedBrowserStorage;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.BillingPayments.Models;

namespace UI.EmployerPortal.Web.Features.BillingPayments.Pages;

/// <summary>
/// Code-behind for the Manage Bank Accounts page.
/// Controls navigation between the account list, add form, and save confirmation states.
/// </summary>
public partial class ManageBankAccounts
{
    private const string AchOriginUrl = "billing-payments/bank-account-payment-ach?source=flow";
    private const string PendingPaymentErrorMessage = "You can't delete a bank account with a pending payment. Review your pending payment(s) on Payment History.";

    [Inject] private IBankAccountOrchestrator BankAccountOrchestrator { get; set; } = default!;
    [Inject] private NavigationManager NavigationManager { get; set; } = default!;

    /// <summary>
    /// When true, a successful save redirects back to the ACH payment page instead of
    /// showing the on-page confirmation screen. Set via ?showback=true on the URL.
    /// </summary>
    [SupplyParameterFromQuery(Name = "showback")]
    public bool Showback { get; set; } = false;

    /// <summary>
    /// When true, a successful save redirects back to the ACH payment page instead of
    /// showing the on-page confirmation screen. Set via ?showback=true on the URL.
    /// </summary>
    [SupplyParameterFromQuery(Name = "action")]
    public string? Action { get; set; }

    [Inject] private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject] private ProtectedSessionStorage SessionStorage { get; set; } = default!;
    private BankAccountPageState _pageState = BankAccountPageState.List;
    private SavedBankAccount? _savedAccount;
    private int _editAccountSk;
    private IReadOnlyList<SavedBankAccount> _accounts = [];
    private bool _isLoading;
    private string? _loadError;
    private string _sortColumn = "nickname";
    private bool _sortAscending = true;
    private bool _canGoBack = true;

    private bool _showRemoveModal = false;
    private SavedBankAccount? _accountToRemove;

    private bool _isPendingPaymentError => string.Equals(_loadError, PendingPaymentErrorMessage, StringComparison.OrdinalIgnoreCase);

    /// <inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        await LoadAccountsAsync();

        if (string.Equals(Action, "add", StringComparison.OrdinalIgnoreCase))
        {
            HandleAddAccount();
        }
        else if (string.Equals(Action, "edit", StringComparison.OrdinalIgnoreCase))
        {
            var result = await SessionStorage.GetAsync<int>("bankAccountEditIntent");
            if (result.Success)
            {
                HandleEditAccount(result.Value);
                await SessionStorage.DeleteAsync("bankAccountEditIntent");
            }
        }
    }

    private async Task LoadAccountsAsync()
    {
        _isLoading = true;
        _loadError = null;

        try
        {
            _accounts = await BankAccountOrchestrator.GetExistingAccountsAsync();
            if (_accounts.Count == 0)
            {
                _pageState = BankAccountPageState.AddForm;
                _canGoBack = false;
                //NavigationManager.NavigateTo("billing-payments/bank-account-payment-ach?source=flow");
                //return;

            }


        }
        catch (Exception)
        {
            _loadError = "Unable to load bank accounts. Please try again.";
        }
        finally
        {
            _isLoading = false;
        }
    }

    private void HandleAddAccount()
    {
        _editAccountSk = 0;
        _pageState = BankAccountPageState.AddForm;
        _canGoBack = true;
    }

    private void HandleEditAccount(int bankAccountSk)
    {
        _editAccountSk = bankAccountSk;
        _pageState = BankAccountPageState.EditForm;
        _canGoBack = true;
    }

    private void HandleSaved(SavedBankAccount account)
    {
        _savedAccount = account;

        if (Showback)
        {
            NavigationManager.NavigateTo(AchOriginUrl);
            return;
        }
        _pageState = BankAccountPageState.Confirmation;
    }

    private async Task HandleBackToList()
    {
        // Arrived here via showback with no accounts on file, so the add form was shown
        // automatically and there's no list to fall back to - return to the ACH page instead.
        if (Showback)
        {
            NavigationManager.NavigateTo(AchOriginUrl);
            return;
        }

        _savedAccount = null;
        _editAccountSk = 0;
        _pageState = BankAccountPageState.List;
        await LoadAccountsAsync();
    }

    private void HandleContinue()
    {
        NavigationManager.NavigateTo("billing-payments");
    }

    private IEnumerable<SavedBankAccount> GetSortedAccounts()
    {
        return _sortColumn switch
        {
            "nickname" => SortBy(a =>
            {
                return a.Nickname;
            }),
            "bankName" => SortBy(a =>
            {
                return a.BankName;
            }),
            "accountType" => SortBy(a =>
            {
                return a.AccountType;
            }),
            "accountNumber" => SortBy(a =>
            {
                return a.MaskedAccountNumber;
            }),
            "routingNumber" => SortBy(a =>
            {
                return a.RoutingNumber;
            }),
            _ => SortBy(a =>
            {
                return a.Nickname;
            }),
        };
    }

    private IOrderedEnumerable<SavedBankAccount> SortBy<TKey>(Func<SavedBankAccount, TKey> keySelector)
    {
        return _sortAscending
            ? _accounts.OrderBy(keySelector)
            : _accounts.OrderByDescending(keySelector);
    }

    private void Sort(string column)
    {
        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = column;
            _sortAscending = true;
        }
    }

    private MarkupString GetSortIcon(string column)
    {
        string path;
        string altText;

        if (_sortColumn == column)
        {
            path = _sortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg";
            altText = _sortAscending ? "Sorted ascending" : "Sorted descending";
        }
        else
        {
            path = "images/sort/sort-icon.svg";
            altText = "Not sorted";
        }

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='{altText}' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private void HandleHeaderKeyDown(KeyboardEventArgs e, string column)
    {
        if (e.Key is "Enter" or " ")
        {
            Sort(column);
        }
    }

    private void OpenRemoveModal(SavedBankAccount account)
    {
        _accountToRemove = account;
        _showRemoveModal = true;
    }

    private void CancelRemove()
    {
        _showRemoveModal = false;
        _accountToRemove = null;
    }

    private async Task ConfirmRemove()
    {
        if (_accountToRemove != null)
        {
            try
            {
                var result = await BankAccountOrchestrator.InactivateBankAccountAsync(_accountToRemove.BankAccountSk);

                if (result.Success)
                {
                    await LoadAccountsAsync();
                }
                else
                {
                    _loadError = result.ErrorMessage ?? "Unable to remove bank account. Please try again.";
                }
            }
            catch (Exception)
            {
                _loadError = "Unable to delete bank account. Please try again.";
            }
        }
        _showRemoveModal = false;
        _accountToRemove = null;
    }

    private async Task HandleClearError()
    {
        _loadError = null;
        await LoadAccountsAsync();
    }

    private void HandleGoToPaymentHistory()
    {
        NavigationManager.NavigateTo("billing-payments/payment-history");
    }
}

/// <summary>
/// Represents the current display state of the Manage Bank Accounts page.
/// </summary>
internal enum BankAccountPageState
{
    /// <summary>
    /// The account list view with the Add Account button.
    /// </summary>
    List,

    /// <summary>
    /// The blank Bank Information form for adding a new account.
    /// </summary>
    AddForm,

    /// <summary>
    /// The pre-populated Bank Information form for editing an existing account.
    /// </summary>
    EditForm,

    /// <summary>
    /// The save confirmation screen shown after a successful add or edit.
    /// </summary>
    Confirmation
}
