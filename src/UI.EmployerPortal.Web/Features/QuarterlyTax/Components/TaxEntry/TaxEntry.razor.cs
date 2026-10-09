using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.QuarterlyTax.Models;

namespace UI.EmployerPortal.Web.Features.QuarterlyTax.Components.TaxEntry;

/// <summary>
/// Tax entry component for capturing quarterly tax report data.
/// </summary>
public partial class TaxEntry : ComponentBase
{
    private bool _grossWagesTouched;
    private bool _exclusionExceedsWages;
    private ValidationMessageStore _messageStore = default!;

    private bool IsOutOfBalance => !TaxDataEntry.IsAuditSource && _grossWagesTouched &&
        TaxDataEntry.PreviouslyReportedGrossWages.HasValue &&
        TaxDataEntry.TotalGrossCoveredWages != TaxDataEntry.PreviouslyReportedGrossWages.Value;

    private async Task HandleExclusionChanged()
    {
        Recalculate();
        await InvokeAsync(StateHasChanged);
        if (OnExclusionUpdated.HasDelegate)
        {
            await OnExclusionUpdated.InvokeAsync(TaxDataEntry.ExclusionOverride.EffectiveAmount);
        }
    }

    private EditContext _editContext = default!;
    private bool _showErrors;

    private readonly Dictionary<string, string> _fieldIds = new()
    {
        [nameof(TaxEntryModel.EmployeeCountMonth1)] = "employee-count-month1",
        [nameof(TaxEntryModel.EmployeeCountMonth2)] = "employee-count-month2",
        [nameof(TaxEntryModel.EmployeeCountMonth3)] = "employee-count-month3",
        [nameof(TaxEntryModel.TotalGrossCoveredWages)] = "total-gross-covered-wages",
        [nameof(TaxEntryModel.ExclusionOverride)] = "exclusion-amount",
        [nameof(TaxEntryModel.GrossWageDiscrepancyExplanation)] = "gross-wage-discrepancy",
    };

    /// <summary>
       /// intialization
       /// </summary>
    protected override void OnInitialized()
    {
        _editContext = new EditContext(TaxDataEntry);
        _messageStore = new ValidationMessageStore(_editContext);
        _editContext.OnFieldChanged += OnFieldChanged;
        _showErrors = false;
        _grossWagesTouched = !string.IsNullOrWhiteSpace(TaxDataEntry?.GrossWageDiscrepancyExplanation)
        || (TaxDataEntry?.PreviouslyReportedGrossWages.HasValue == true
      && TaxDataEntry.TotalGrossCoveredWages != TaxDataEntry.PreviouslyReportedGrossWages.Value);
    }

    private void OnFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        StateHasChanged();
    }

    /// <summary>
    /// Recalculate
    /// </summary>
    protected override async Task OnParametersSetAsync()
    {
        SetMonthsFromQuarter();
        Recalculate();
        if (!_grossWagesTouched)
        {
            _grossWagesTouched = !string.IsNullOrWhiteSpace(TaxDataEntry?.GrossWageDiscrepancyExplanation)
                || (TaxDataEntry?.PreviouslyReportedGrossWages.HasValue == true
&& TaxDataEntry.TotalGrossCoveredWages != TaxDataEntry.PreviouslyReportedGrossWages.Value);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    public async Task<bool> IsValid()
    {
        _grossWagesTouched = true;

        if (_editContext.Validate())
        {
            return true;
        }

        _showErrors = true;
        return false;
    }

    private void SetMonthsFromQuarter()
    {
        if (TaxDataEntry is null || TaxDataEntry.Quarter is < 1 or > 4)
        {
            return;
        }

        var startMonth = ((TaxDataEntry.Quarter - 1) * 3) + 1;

        TaxDataEntry.MonthName1 = new DateTime(2000, startMonth, 1).ToString("MMMM");
        TaxDataEntry.MonthName2 = new DateTime(2000, startMonth + 1, 1).ToString("MMMM");
        TaxDataEntry.MonthName3 = new DateTime(2000, startMonth + 2, 1).ToString("MMMM");
    }

    private void Recalculate()
    {
        if (!IsReadonly)
        {
            TaxDataEntry.DefinedTaxablePayroll =
                TaxDataEntry.TotalGrossCoveredWages - TaxDataEntry.ExclusionOverride.EffectiveAmount;
            TaxDataEntry.TaxAssessed =
             (TaxDataEntry.DefinedTaxablePayroll * TaxDataEntry.TaxRate);
        }
    }

    private Task HandleGrossWagesChanged(decimal? newValue)
    {
        _grossWagesTouched = true;
        TaxDataEntry.TotalGrossCoveredWages = newValue ?? 0m;
        if (TaxDataEntry.PreviouslyReportedGrossWages.HasValue &&
        TaxDataEntry.TotalGrossCoveredWages == TaxDataEntry.PreviouslyReportedGrossWages.Value)
        {
            TaxDataEntry.GrossWageDiscrepancyExplanation = null;
        }
        _exclusionExceedsWages = TaxDataEntry.ExclusionOverride.EffectiveAmount > TaxDataEntry.TotalGrossCoveredWages;
        Recalculate();
        StateHasChanged();
        return Task.CompletedTask;
    }

    private async Task HandleDirectExclusionChanged(decimal? newValue)
    {
        TaxDataEntry.ExclusionOverride.OverrideAmount = newValue ?? 0m;
        TaxDataEntry.ExclusionOverride.IsOverride = true;

        _messageStore.Clear();
        _exclusionExceedsWages = TaxDataEntry.ExclusionOverride.EffectiveAmount > TaxDataEntry.TotalGrossCoveredWages;

        Recalculate();
        StateHasChanged();
        if (OnExclusionUpdated.HasDelegate)
        {
            await OnExclusionUpdated.InvokeAsync(TaxDataEntry.ExclusionOverride.EffectiveAmount);
        }
    }

    private void HandleGrossWagesBlur()
    {
        _grossWagesTouched = true;
    }
}


/// <summary>
///
/// </summary>
public class EmployeeCountChangedArgs
{
    /// <summary>
    /// Month
    /// </summary>
    public int? Month { get; set; }

    /// <summary>
    /// New Count
    /// </summary>
    public int? NewCount { get; set; }
}
