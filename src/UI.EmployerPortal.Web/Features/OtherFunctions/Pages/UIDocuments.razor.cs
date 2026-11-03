using System.Globalization;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Razor.SharedComponents.Validation;
using UI.EmployerPortal.Web.Features.OtherFunctions.Models;
using UI.EmployerPortal.Web.Features.Shared.Accounts.Models;
using UI.EmployerPortal.Web.Features.Shared.OtherFunctions.Services;
using ISessionManager = UI.EmployerPortal.Web.Features.Shared.Session.Managers.ISessionManager;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Pages;

/// <summary>
/// UI Documents Page
/// </summary>
public partial class UIDocuments
{
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;

    [Inject]
    private IUIDocumentService UIDocumentService { get; set; } = default!;

    [Inject]
    private ISessionManager SessionManager { get; set; } = default!;

    private UIDocumentSearchModel Model { get; set; } = new();

    private EditContext? _editContext;

    private ValidationMessageStore? _messageStore;

    private CustomValidator? _customValidator;

    private bool _formSubmitted;

    private bool _showValidationSummary;

    private readonly HashSet<FieldIdentifier> _touchedFields = new();
    private readonly Dictionary<string, string> _validationFieldIds = new()
    {
    { nameof(UIDocumentSearchModel.FromDate), "from-date"},
    { nameof(UIDocumentSearchModel.ToDate), "to-date"}
          };

    private bool _isLoading = false;

    private List<UIDocument> _allDocuments = [];

    private List<UIDocument> _documentToDisplay = [];

    private readonly string _minimumFromDate = DateOnly.FromDateTime(DateTime.Today.AddYears(-3)).ToString("yyyy-MM-dd");

    private readonly string _maximumDate = DateOnly.FromDateTime(DateTime.Today).ToString("yyyy-MM-dd");

    private string _sortColumn = "date";

    private bool _sortAscending = false;

    private bool _searchExecutedSuccessfully;

    private bool _showBanner;

    private string _bannerMessage = string.Empty;

    [Inject]
    private IJSRuntime JS { get; set; } = default!;

    /// <summary>
    /// Optional description fragment from query. When set, opens the newest matching row via <see cref="HandleView"/>.
    /// Normal <c>/ui-documents</c> visits omit this and are unchanged.
    /// </summary>
    [SupplyParameterFromQuery(Name = "autoOpen")]
    public string? AutoOpen { get; set; }

    private IEnumerable<UIDocument> SortedDocuments => GetSortedDocuments();

    ///<inheritdoc/>
    protected override async Task OnAuthorizedInitAsync()
    {
        var today = DateTime.Today;
        var sixMonthAgo = today.AddMonths(-6);

        Model = new UIDocumentSearchModel
        {
            FromDate = sixMonthAgo.ToString("yyyy-MM-dd"),
            ToDate = today.ToString("yyyy-MM-dd")
        };

        _editContext = new EditContext(Model);
        _messageStore = new ValidationMessageStore(_editContext);

        _editContext.OnFieldChanged += (_, e) =>
        {
            _touchedFields.Add(e.FieldIdentifier);
            _messageStore?.Clear(e.FieldIdentifier);
            _showValidationSummary = false;
            _editContext.NotifyValidationStateChanged();
            StateHasChanged();
        };

        await LoadInitialCorrespondence();
        await TryAutoOpenFromQueryAsync();
    }

    /// <summary>
    /// Opens the newest correspondence whose description contains <see cref="AutoOpen"/>.
    /// No-op when the query is missing (default UI Documents behavior).
    /// </summary>
    private async Task TryAutoOpenFromQueryAsync()
    {
        if (string.IsNullOrWhiteSpace(AutoOpen) || _documentToDisplay.Count == 0)
        {
            return;
        }

        var match = _documentToDisplay
            .Where(d => d.Description.Contains(AutoOpen, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(d => d.Date)
            .FirstOrDefault();

        if (match is null)
        {
            return;
        }

        await HandleView(match);
    }

    private async Task LoadInitialCorrespondence()
    {
        var fromDate = DateTime.Today.AddMonths(-6);
        var toDate = DateTime.Today;
        var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();
        var employerSK = selectedEmployer?.EmployerAccount?.Id ?? 0;

        _allDocuments = await UIDocumentService.GetCorrespondenceAsync(employerSK, fromDate, toDate);

        _documentToDisplay = _allDocuments;
    }

    private bool IsVisible(Expression<Func<string?>> @for)
    {
        return _formSubmitted || _touchedFields.Contains(FieldIdentifier.Create(@for));
    }

    private async Task HandleSearch()
    {
        _formSubmitted = true;

        if (_editContext == null || _messageStore == null)
        {
            return;
        }
        _messageStore.Clear();
        _editContext.Validate();

        ValidateDateRange();

        var isValid = !_editContext.GetValidationMessages().Any();

        _showValidationSummary = !isValid;

        if (!isValid)
        {
            _searchExecutedSuccessfully = false;
            _documentToDisplay = [];
            _editContext.NotifyValidationStateChanged();
            return;
        }

        var fromDate = DateTime.Parse(Model.FromDate!, CultureInfo.InvariantCulture);
        var toDate = DateTime.Parse(Model.ToDate!, CultureInfo.InvariantCulture);

        var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();
        var employerSK = selectedEmployer?.EmployerAccount?.Id ?? 0;
        _allDocuments = await UIDocumentService.GetCorrespondenceAsync(employerSK, fromDate, toDate);
        _documentToDisplay = _allDocuments;

        _searchExecutedSuccessfully = true;
        _editContext.NotifyValidationStateChanged();

        await Task.CompletedTask;
    }

    private void ValidateDateRange()
    {
        if (_editContext == null || _messageStore == null)
        {
            return;
        }

        var fromField = new FieldIdentifier(Model, nameof(Model.FromDate));
        var toField = new FieldIdentifier(Model, nameof(Model.ToDate));

        if (string.IsNullOrWhiteSpace(Model.FromDate))
        {
            _messageStore.Add(fromField, "From Date is required");
        }

        if (string.IsNullOrWhiteSpace(Model.ToDate))
        {
            _messageStore.Add(toField, "To Date is required");
        }

        if (string.IsNullOrWhiteSpace(Model.FromDate) || string.IsNullOrWhiteSpace(Model.ToDate))
        {
            return;
        }

        var fromDateValid = DateTime.TryParse(Model.FromDate, out var fromDate);
        var toDateValid = DateTime.TryParse(Model.ToDate, out var toDate);

        if (!fromDateValid)
        {
            _messageStore.Add(fromField, "Enter a valid From Date");
        }


        if (!toDateValid)
        {
            _messageStore.Add(toField, "Enter a valid To Date");
        }

        if (!fromDateValid || !toDateValid)
        {
            return;
        }

        if (fromDate.Date > toDate.Date)
        {
            _messageStore.Add(fromField, "From Date cannot be greater than To Date");
        }

        if (fromDate.Date < DateTime.Today.AddYears(-3).Date)
        {
            _messageStore.Add(fromField, "From Date cannot be more than 3 years ago");
        }
    }

    private void OnInvalid()
    {
        _formSubmitted = true;
        _showValidationSummary = true;
        StateHasChanged();
    }

    private IEnumerable<UIDocument> GetSortedDocuments()
    {
        return _sortColumn switch
        {
            "form" => SortBy(x =>
            {
                return x.Form;
            }),
            "description" => SortBy(x =>
            {
                return x.Description;
            }),
            "date" => SortBy(x =>
            {
                return x.Date;
            }),
            _ => SortBy(x =>
            {
                return x.Date;
            })
        };
    }

    private IOrderedEnumerable<UIDocument> SortBy<TKey>(Func<UIDocument, TKey> keySelector)
    {
        return _sortAscending
            ? _documentToDisplay.OrderBy(keySelector)
            : _documentToDisplay.OrderByDescending(keySelector);
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
        var path = _sortColumn == column
            ? _sortAscending ? "images/sort/sort-icon-asc.svg" : "images/sort/sort-icon-desc.svg"
            : "images/sort/sort-icon.svg";

        return new MarkupString($"<img src='{Assets[path]}' class='sort-icon' alt='' />");
    }

    private string? GetAriaSort(string column)
    {
        return _sortColumn != column ? null : _sortAscending ? "ascending" : "descending";
    }

    private async Task HandleView(UIDocument document)
    {
        _isLoading = true;
        _showBanner = false;
        try
        {
            var deadline = DateTime.UtcNow.AddMinutes(2);
            var pollInterval = TimeSpan.FromSeconds(5);
            var selectedEmployer = await SessionManager.GetAsync<SelectedEmployerAccount>();

            if (selectedEmployer?.EmployerAccount?.Id == null)
            {
                return;
            }
            var employerSK = selectedEmployer?.EmployerAccount?.Id ?? 0;

            while (DateTime.UtcNow < deadline)
            {
                try
                {

                    var pdfUrl = await UIDocumentService.GenerateCorrespondencePdfAsync(employerSK, document.GenerationSK);

                    if (!string.IsNullOrEmpty(pdfUrl) && File.Exists(pdfUrl))
                    {
                        await using var fileStream = File.OpenRead(pdfUrl);
                        using var streamRef = new DotNetStreamReference(stream: fileStream);
                        await JS.InvokeVoidAsync("openPdfInNewTab", streamRef);
                        return;
                    }
                }

                catch (Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }
                var remaining = deadline - DateTime.UtcNow;
                var delay = remaining < pollInterval ? remaining : pollInterval;
                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay);
                }
            }

            _bannerMessage = "Request timed out while generating the document. Please try again.";
            _showBanner = true;
        }
        finally
        {
            _isLoading = false;
            await InvokeAsync(StateHasChanged);
        }

    }

    private static string FormatDate(DateTime date)
    {
        return date.TimeOfDay.TotalSeconds > 0 ? date.ToString("M/d/yyyy h:mm:ss tt", CultureInfo.InvariantCulture)
            : date.ToString("MM/dd/yyyy", CultureInfo.InvariantCulture);

    }
}
