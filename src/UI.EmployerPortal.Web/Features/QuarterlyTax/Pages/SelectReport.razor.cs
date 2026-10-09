using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Auth;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Models;
using UI.EmployerPortal.Web.Features.Shared.QuarterlyTax.Services;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Pages;

/// <summary>
/// Dashboad page to selct the reporting method
/// </summary>
public partial class SelectReport
{
    // ── Injected services ──────────────────────────────────────────────────────
    [Inject]
    private ITaxAndWageEntryService TaxAndWageEntryService { get; set; } = default!;
    [Inject]
    private IQuarterlyReportOrchestrator QuarterlyReportOrchestrator { get; set; } = default!;
    [Inject]
    private NavigationManager NavigationManager { get; set; } = default!;
    [Inject]
    private IPageAuthorizationService PageAuthorizationService { get; set; } = default!;
    [Inject]
    private IJSRuntime JSRuntime { get; set; } = default!;


    // ── Private state ──────────────────────────────────────────────────────────
    private MissingReportModel? _missingReportModel;
    private bool _hasReportContext;
    private bool _hasPendingReport;
    private string _pendingReportUrl = string.Empty;
    private string _reportingQuarterDisplay = string.Empty;

    private List<QuarterlyReportSelectionMethod> _eligibleMethods = new();
    private List<QuarterlyReportSelectionMethod> _unavailableMethods = new();

    private int? _selectedMethod;
    private int _selectedIndex = 0;
    private bool _isDrawerOpen;
    private bool _showMissingPriorQuartersModal;
    private bool _isLoading = true;
    private bool _showValidationSummary;
    private int? _errorMethodCodeSK;
    private readonly EditContext _editContext = new(new object());
    private ValidationMessageStore _messageStore = default!;
    private readonly Dictionary<string, string> _validationFieldIds = new()
    {
        [string.Empty] = "reporting-card-0",
    };

    // ── Computed ───────────────────────────────────────────────────────────────
    private bool CanContinue => _hasReportContext && _selectedMethod.HasValue;

    private readonly IReadOnlyDictionary<string, (string LongDescription, int SortOrder)>
        _taxFilingMethodsDefinitions = new Dictionary<string, (string, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Tax Report with Employee Wage Entry"] = ("Online entry of wage detail with automated tax calculations.", 1),
            ["Tax Report with Employee Wage Upload"] = ("Online entry of tax report with a wage file transfer/upload. You must have an original, new file.", 2),
            ["Tax Report Only"] = ("Online entry of tax report without wage detail. Wage report must be submitted using another electronic media.", 3),
            ["Zero Payroll this Quarter Report"] = ("No wages paid this quarter.", 4)
        };

    private readonly IReadOnlyDictionary<string, (string LongDescription, int SortOrder)>
        _wageFilingMethodsDefinitions = new Dictionary<string, (string, int)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Wage Entry"] = ("Online entry of wage detail for the quarter.", 1),
            ["Wage Upload"] = ("Upload a wage file for the quarter.", 2)
        };

    private bool _isWageReport;

    /// <summary>
    ///
    /// </summary>
    /// <returns></returns>
    protected override async Task OnAuthorizedInitAsync()
    {
    }
    /// <inheritdoc/>
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!firstRender)
        {
            return;
        }

        _missingReportModel = await QuarterlyReportOrchestrator.GetMissingReportFromSessionAsync();
        _hasReportContext = _missingReportModel is not null;
        _hasPendingReport = _missingReportModel?.PendingWageTaxFilingSK.HasValue == true;

        if (_hasReportContext)
        {
            _isWageReport = string.Equals(_missingReportModel!.ReportName, "Wage Report", StringComparison.OrdinalIgnoreCase);
            _reportingQuarterDisplay = BuildQuarterDisplay(_missingReportModel!);

            if (_hasPendingReport)
            {
                _pendingReportUrl = _missingReportModel!.PendingReportingSelectionCodeSK switch
                {
                    1 => "quarterly-tax/tax-and-wage-entry-report",
                    2 => "quarterly-tax/tax-report-only?source=tax-wage-upload",
                    3 => "quarterly-tax/tax-report-only",
                    4 => "quarterly-tax/zero-payroll-report",
                    6 => "quarterly-tax/wage-entry-report",
                    _ => "quarterly-tax/select-report"
                };
            }
            else
            {
                await LoadFilingMethodsAsync(_missingReportModel!);
            }
        }
        else
        {
            // No report context — render all four methods as disabled placeholders
            _eligibleMethods = BuildDisabledPlaceholders();
        }

        _messageStore = new ValidationMessageStore(_editContext);
        _isLoading = false;
        StateHasChanged();
    }

    private IReadOnlyDictionary<string, (string LongDescription, int SortOrder)> ActiveFilingMethodDefinitions
        => _isWageReport ? _wageFilingMethodsDefinitions : _taxFilingMethodsDefinitions;

    private List<QuarterlyReportSelectionMethod> BuildDisabledPlaceholders()
    {
        return ActiveFilingMethodDefinitions.OrderBy(kv =>
        {
            return kv.Value.SortOrder;
        })
            .Select(kv =>
            {
                return new QuarterlyReportSelectionMethod
                {
                    CodeSK = 0,
                    ShortDescription = kv.Key,
                    LongDescription = kv.Value.LongDescription,
                    IsEligible = false,
                };
            }).ToList();
    }

    // ── Data loading ───────────────────────────────────────────────────────────
    private async Task LoadFilingMethodsAsync(MissingReportModel missingReportModel)
    {
        var rawMethods = await TaxAndWageEntryService.GetAvailableFilingMethods(missingReportModel);

        var rawByDescription = rawMethods.ToDictionary(m =>
        {
            return m.ShortDescription ?? string.Empty;
        },
        m =>
        {
            return m;
        },
        StringComparer.OrdinalIgnoreCase);

        var options = ActiveFilingMethodDefinitions.OrderBy(kv =>
        {
            return kv.Value.SortOrder;
        })
        .Select(kv =>
        {
            rawByDescription.TryGetValue(kv.Key, out var raw);

            return new QuarterlyReportSelectionMethod
            {
                CodeSK = raw?.CodeSK ?? 0,
                ShortDescription = kv.Key,
                LongDescription = kv.Value.LongDescription,
                IsEligible = raw?.IsEligible ?? false,
                EligibleReasons = raw?.EligibleReasons?.ToList() ?? new List<string>()
            };

        })
        .ToList();


        _eligibleMethods = options.Where(m =>
        {
            return m.IsEligible;
        }).ToList();
        _unavailableMethods = [.. options.Where(m =>
        {
            return !m.IsEligible;
        })];
    }


    private void NavigateToMissingReports()
    {
        NavigationManager.NavigateTo("quarterly-tax/missing-reports");
    }


    // ── Interaction handlers ───────────────────────────────────────────────────
    private void SelectMethod(int index)
    {
        _selectedIndex = index;
        _validationFieldIds[string.Empty] = $"reporting-card-{index}";
        if (!_hasReportContext)
        {
            _selectedMethod = null;
            return;
        }
        else
        {
            _selectedMethod = _eligibleMethods[index].CodeSK;
        }
    }

    private async Task HandleNextCard()
    {
        if (_eligibleMethods.Count == 0)
        {
            return;
        }
        // Move down or wrap to beginning
        SelectMethod((_selectedIndex + 1) % _eligibleMethods.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task HandlePreviousCard()
    {
        if (_eligibleMethods.Count == 0)
        {
            return;
        }
        // Move up or wrap to end
        SelectMethod((_selectedIndex - 1 + _eligibleMethods.Count) % _eligibleMethods.Count);
        await FocusOnCard(_selectedIndex);
    }

    private async Task FocusOnCard(int index)
    {
        await JSRuntime.InvokeVoidAsync("focusElement", $"reporting-card-{index}");
    }

    private async Task<bool> IsFirstTimeFilerAsync()
    {
        var previouslyFiledReports = await TaxAndWageEntryService.GetPreviouslyFiledReportsAsync();
        if (previouslyFiledReports.Any())
        {
            return false;
        }

        var missingReportsData = await TaxAndWageEntryService.GetMissingWageAndTaxReports();
        var hasPriorMissingReports = missingReportsData.MissingReports.Any(r =>
            r.Year < _missingReportModel!.Year ||
            (r.Year == _missingReportModel!.Year && r.Quarter < _missingReportModel!.Quarter));
        return !hasPriorMissingReports;
    }

    private async Task<bool> HasMissingPriorQuartersAsync()
    {
        if (_missingReportModel is null)
        {
            return false;
        }

        if (_missingReportModel.Quarter > 1)
        {
            if (await IsFirstTimeFilerAsync())
            {
                return false;
            }

            var wageRportFoundInPriorQuarters = false;
            for (var i = 1; i < _missingReportModel.Quarter; i++)
            {
                var wageReport = await TaxAndWageEntryService.GetWageReportByQuarterYear(_missingReportModel.Year, i);
                if (wageReport != null && wageReport.WageReportSK != null)
                {
                    wageRportFoundInPriorQuarters = true;
                    break;
                }
            }

            return !wageRportFoundInPriorQuarters;
        }

        return false;
    }

    private void HandleConfirmMissingPriorQuartersModal()
    {
        _showMissingPriorQuartersModal = false;
        NavigationManager.NavigateTo("quarterly-tax/tax-report-only", true);
    }

    private void HandleCancelMissingPriorQuartersModal()
    {
        _showMissingPriorQuartersModal = false;
    }

    private async Task HandleContinue()
    {
        _messageStore.Clear();
        _showValidationSummary = false;
        _errorMethodCodeSK = null;

        var selectedMethodName = _eligibleMethods.FirstOrDefault(m =>
        {
            return m.CodeSK == _selectedMethod;
        })?.ShortDescription;

        if (_selectedMethod is not null && _missingReportModel is not null)
        {
            var filingMethod = await TaxAndWageEntryService.GetAvailableFilingMethod(_missingReportModel, (TaxWageFilingType) _selectedMethod.Value);
            if (filingMethod is null || !filingMethod.IsEligible)
            {
                var reasons = filingMethod?.EligibleReasons ?? ["This filing method is no longer available."];
                if (!reasons.Any())
                {
                    reasons = ["This filing method is no longer available."];
                }
                foreach (var reason in reasons)
                {
                    _messageStore.Add(_editContext.Field(string.Empty), reason);
                }
                _showValidationSummary = true;
                _errorMethodCodeSK = _selectedMethod;
                _editContext.NotifyValidationStateChanged();
                return;
            }
        }


        if (_isWageReport)
        {
            switch (_selectedMethod)
            {
                case 6:
                    NavigationManager.NavigateTo("quarterly-tax/wage-entry-report", true);
                    break;
                case 5:
                    NavigationManager.NavigateTo("quarterly-tax/wage-upload", true);
                    break;
                default:
                    NavigationManager.NavigateTo("not-found", true);
                    break;
            }
            return;
        }
        if (_selectedMethod == 3 && await HasMissingPriorQuartersAsync())
        {
            _showMissingPriorQuartersModal = true;
            return;
        }
        switch (_selectedMethod)
        {
            case 1:
                NavigationManager.NavigateTo("quarterly-tax/tax-and-wage-entry-report", true);
                break;
            case 2:
                NavigationManager.NavigateTo("quarterly-tax/tax-report-only?source=tax-wage-upload", true);
                break;
            case 3:
                NavigationManager.NavigateTo("quarterly-tax/tax-report-only", true);
                break;
            case 4:
                NavigationManager.NavigateTo("quarterly-tax/zero-payroll-tax-report", true);
                break;
            default:
                NavigationManager.NavigateTo("not-found", true);
                break;
        }

    }

    private void HandleContinuePendingReport()
    {
        if (!string.IsNullOrEmpty(_pendingReportUrl))
        {
            NavigationManager.NavigateTo(_pendingReportUrl);
        }
    }

    private void OpenDrawer()
    {
        _isDrawerOpen = true;
    }

    private void CloseDrawer()
    {
        _isDrawerOpen = false;
    }


    // ── Helpers ────────────────────────────────────────────────────────────────
    private static string BuildQuarterDisplay(MissingReportModel model)
    {
        // Expected format: "Q1 2024 Tax & Wage Report"
        return $"{model.FormattedQuarterYear} {model.ReportName}";
    }
}
