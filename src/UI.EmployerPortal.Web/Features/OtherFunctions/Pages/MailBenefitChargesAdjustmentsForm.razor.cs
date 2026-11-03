using System.Net;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using UI.EmployerPortal.Generated.ServiceClients.PortalUtilityService;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Services;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// Code behind for Mail Benefit Charges Adjustments Form page
/// </summary>
public partial class MailBenefitChargesAdjustmentsForm
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    private MailReserveFundBalanceStatementsModel Model { get; set; } = new();

    private bool ShowHistoryTable => _historyItems.Any();
    private string? _sortColumn;

    private bool _sortAscending = true;

    private List<ReserveFundMailHistoryItem> _historyItems = [];

    private bool _showBanner;

    private string _bannerMessage = string.Empty;

    private bool _lastSavedValue;


    /// <summary>
    /// Loading spinner
    /// </summary>
    private bool _isLoading = true;

    private bool _isSaving;

    private bool IsSaveDisabled => _isSaving || Model.DoNotMailReserveFundBalanceStatements != _lastSavedValue;

    [Inject]
    private IPortalUtilityService PortalUtilityService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    [Inject]
    private IUserAccountService UserAccountService { get; set; } = default!;

    private const string DO_NOT_RECEIVE_TEXT = "You don't receive Benefits Charges and Adjustments by mail. To start receiving them by mail, uncheck the box below and select Save.";
    private const string RECEIVE_TEXT = "You currently receive Benefits Charges and Adjustments by mail.To stop receiving them by mail, check the box below and select Save.";
    private string _instructionText = DO_NOT_RECEIVE_TEXT;

    private void SetInstructionText(bool doNotMail)
    {
        _instructionText = doNotMail ? DO_NOT_RECEIVE_TEXT : RECEIVE_TEXT;
    }

    ///<inheritdoc />
    protected override async Task OnAuthorizedInitAsync()
    {
        _isLoading = true;
        _showBanner = false;
        _isSaving = false;

        var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();

        var employerSk = selectedEmployer?.EmployerAccount?.Id ?? 0;
        var secureUserSk = UserAccountService.GetUserSKClaim();

        await BindCurrentFlagValueAndLoadHistoryTable(employerSk, secureUserSk);
        _isLoading = false;
    }


    private async Task HandleSave()
    {
        if (_isSaving || Model.DoNotMailReserveFundBalanceStatements == !_lastSavedValue)
        {
            return;
        }
        _isSaving = true;
        _isLoading = true;
        try
        {
            var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();
            var employerSk = selectedEmployer?.EmployerAccount?.Id ?? 0;
            var secureUserSk = UserAccountService.GetUserSKClaim();

            var backenedFlag = !Model.DoNotMailReserveFundBalanceStatements;
            var response = await PortalUtilityService.SaveReserveFundStatementMailFlagAsync(employerSk, secureUserSk, backenedFlag);

            _bannerMessage = WebUtility.HtmlDecode(response?.Message ?? string.Empty);
            _showBanner = !string.IsNullOrWhiteSpace(_bannerMessage);

            await BindCurrentFlagValueAndLoadHistoryTable(employerSk, secureUserSk);
        }
        finally
        {
            _isSaving = false;
            _isLoading = false;
        }
    }

    private async Task BindCurrentFlagValueAndLoadHistoryTable(int employerSk, int secureUserSk)
    {
        var currentSelectionValue = await PortalUtilityService.ReserveFundStatementMailFlagAsync(employerSk, secureUserSk);

        if (currentSelectionValue != null)
        {
            Model.DoNotMailReserveFundBalanceStatements = !currentSelectionValue.Value;
            _lastSavedValue = currentSelectionValue.Value;
        }

        var histories = await PortalUtilityService.ObtainEmployerRelatedInformationObtainedAsync(employerSk, secureUserSk);

        if (histories?.ActivityLogComments != null)
        {
            PopulateHistoryTable(histories.ActivityLogComments);
        }

        SetInstructionText(Model.DoNotMailReserveFundBalanceStatements);
    }

    private void PopulateHistoryTable(ActivityLogCommentProxy[] proxies)
    {
        _historyItems = [.. proxies.Select(p =>
        {
            return new ReserveFundMailHistoryItem
            {
                UpdatedBy = p.UpdatedBy ?? "System",
                UpdateDate = p.UpdatedDate ?? DateTime.Now,
                Action = p.ActivityLogCommentText ?? string.Empty
            };
        }).OrderByDescending(p => { return p.UpdateDate; })];
    }

    private void HandleCheckboxChanged(ChangeEventArgs e)
    {
        if (bool.TryParse(e.Value?.ToString(), out var isChecked))
        {
            Model.DoNotMailReserveFundBalanceStatements = isChecked;
            _showBanner = false;
            StateHasChanged();
        }
    }

    private void HandleBack()
    {
        NavigationManager.NavigateTo("other-functions/benefit-charges");
    }

    private IEnumerable<ReserveFundMailHistoryItem> GetSortedHistory()
    {
        return _sortColumn switch
        {
            "updatedBy" => SortBy(x =>
            {
                return x.UpdatedBy;
            }),
            "updatedDate" => SortBy(x =>
            {
                return x.UpdateDate;
            }),
            "action" => SortBy(x =>
            {
                return x.Action;
            }),
            _ => _historyItems
        };
    }

    private IOrderedEnumerable<ReserveFundMailHistoryItem> SortBy<TKey>(Func<ReserveFundMailHistoryItem, TKey> keySelector)
    {
        return _sortAscending ? _historyItems.OrderBy(keySelector) : _historyItems.OrderByDescending(keySelector);
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

    private sealed class MailReserveFundBalanceStatementsModel
    {
        public bool DoNotMailReserveFundBalanceStatements { get; set; }
    }

    private sealed class ReserveFundMailHistoryItem
    {
        public string UpdatedBy { get; set; } = string.Empty;

        public DateTime UpdateDate { get; set; }

        public string Action { get; set; } = string.Empty;
    }

}
