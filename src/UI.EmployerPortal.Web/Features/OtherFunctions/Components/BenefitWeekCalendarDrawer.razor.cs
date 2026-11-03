using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using UI.EmployerPortal.Web.Features.Shared.OtherFunctions.Services;

namespace UI.EmployerPortal.Web.Features.OtherFunctions.Components;

/// <summary>
/// Side panel drawer displaying the Unemployment Insurance Benefit Week and Statement Calendar.
/// </summary>
public partial class BenefitWeekCalendarDrawer : ComponentBase
{
    private bool _isLoading;
    private int _displayYear;
    private BenefitCalendarData _calendarData = new([], []);
    private bool _previousIsOpen;
    private bool _wasOpen;
    private ElementReference _drawerRef;
    private IJSObjectReference? _module;

    /// <summary>Controls whether the drawer is visible.</summary>
    [Parameter] public bool IsOpen { get; set; }

    /// <summary>Invoked when the user closes the drawer.</summary>
    [Parameter] public EventCallback OnClose { get; set; }

    [Inject]
    private IBenefitCalendarService BenefitCalendarService { get; set; } = default!;
    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;


    /// <inheritdoc />
    protected override async Task OnParametersSetAsync()
    {
        if (IsOpen && !_previousIsOpen)
        {
            _isLoading = true;
            _displayYear = await BenefitCalendarService.GetCurrentYearAsync();
            await LoadCalendarAsync(_displayYear);
        }
        _previousIsOpen = IsOpen;
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        // Wait for the calendar to finish loading or else calendar buttons won't be in the tab order
        if (IsOpen && !_wasOpen && !_isLoading)
        {
            _wasOpen = true;
            _module ??= await JSRuntime.InvokeAsync<IJSObjectReference>("import", "./js/FilterDrawer.js");
            await _module.InvokeVoidAsync("openFocusTrap", _drawerRef);
        }
        else if (!IsOpen && _wasOpen)
        {
            _wasOpen = false;
            if (_module is not null)
            {
                await _module.InvokeVoidAsync("closeFocusTrap");
            }
        }
    }

    private async Task LoadCalendarAsync(int year)
    {
        _isLoading = true;
        try
        {
            _calendarData = await BenefitCalendarService.LoadCalendarAsync(year);
        }
        finally
        {
            _isLoading = false;
        }
    }

    private async Task PreviousYear()
    {
        _displayYear--;
        await LoadCalendarAsync(_displayYear);
    }

    private async Task NextYear()
    {
        _displayYear++;
        await LoadCalendarAsync(_displayYear);
    }

    private void HandleClose()
    {
        _ = OnClose.InvokeAsync();
    }

    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Escape")
        {
            HandleClose();
        }
    }

    /// <summary>
    /// Returns calendar weeks that overlap with the given month of the display year.
    /// A week overlaps a month when its Sunday-Saturday span intersects any day in that month.
    /// </summary>
    private IEnumerable<CalendarWeekEntry> GetWeeksForMonth(int month)
    {
        var firstDay = new DateTime(_displayYear, month, 1);
        var lastDay = new DateTime(_displayYear, month, DateTime.DaysInMonth(_displayYear, month));
        return _calendarData.CalendarWeeks
        .Where(w =>
        {
            return w.WeekStartDate <= lastDay && w.WeekEndDate >= firstDay;
        });
    }

    /// <summary>
    /// Enumerates all 7 dates (Sun–Sat) in a calendar week.
    /// </summary>
    private static IEnumerable<DateTime> GetWeekDates(CalendarWeekEntry week)
    {
        for (var d = week.WeekStartDate; d <= week.WeekEndDate; d = d.AddDays(1))
        {
            yield return d;
        }
    }

    private string GetDateCellClass(DateTime date, int currentMonth)
    {
        var classes = new List<string> { "date-cell" };
        if (date.Month != currentMonth)
        {
            classes.Add("other-month");
        }

        if (_calendarData.ShadedDates.Contains(date.Date))
        {
            classes.Add("shaded");
        }

        return string.Join(" ", classes);
    }

    private static string GetMonthName(int month)
    {
        return new DateTime(2000, month, 1).ToString("MMMM");
    }
}
