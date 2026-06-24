using System.Net;
using Microsoft.AspNetCore.Components;

namespace UI.EmployerPortal.Web.Features.Shared.Layout.Components;

/// <summary>
/// BoldDropdown component
/// </summary>
public partial class BoldDropdown<TItem>
{
    /// <summary>
    /// Parameter to get the object that has the items
    /// </summary>
    [Parameter]
    public List<TItem> Items { get; set; } = [];

    /// <summary>
    /// Parameter to get the default dropdown text
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Select an option";

    /// <summary>
    /// Parameter to get the default dropdown text
    /// </summary>
    [Parameter] public string DefaultOptionLabel { get; set; } = "Select...";

    /// <summary>
    /// Parameter to get the left label for an item
    /// </summary>
    [Parameter, EditorRequired]
    public Func<TItem, string> FirstLabel { get; set; } = default!;

    /// <summary>
    /// Parameter to get the secondary (right, bold) label for an item
    /// </summary>
    [Parameter]
    public Func<TItem, string?>? SecondLabel { get; set; }

    /// <summary>
    /// Parameter to get Separator string rendered between the two labels. Defaults to 4 spaces
    /// </summary>
    [Parameter]
    public int SpaceLength { get; set; } = 4;

    /// <summary>
    /// Parameter to get Max characters shown in the selected value truncating rest with with ellipsis
    /// Set to 0 disable truncation entirely. Defaults to 35.
    /// </summary>
    [Parameter]
    public int MaxDisplayLength { get; set; } = 35;

    /// <summary>
    /// Parameter for the ellipsis string appended when truncating. Defaults to "…"
    /// </summary>
    [Parameter]
    public string Ellipsis { get; set; } = "…";

    /// <summary>
    /// Fired when item is selected
    /// </summary>
    [Parameter]
    public EventCallback<TItem?> OnChange { get; set; }

    /// <summary>
    /// Fired when item is selected send the selected text
    /// </summary>
    [Parameter]
    public EventCallback<string?> OnChangeText { get; set; }

    /// <summary>
    /// Fired when the user picks the default "Select..." option
    /// </summary>
    [Parameter]
    public EventCallback OnCleared { get; set; }

    /// <summary>
    /// Current Selected Item Value
    /// </summary>
    [Parameter]
    public TItem? Value { get; set; }

    private TItem? _selectedItem;
    private bool _isOpen = false;

    ///
    protected override void OnParametersSet()
    {
        _selectedItem = Value;
    }
    /// <inheritdoc/>
    private void Toggle()
    {
        _isOpen = !_isOpen;
    }

    /// <inheritdoc/>
    private async Task Select(TItem item)
    {
        _selectedItem = item;
        _isOpen = false;
        await OnChangeText.InvokeAsync(GetFullLabel(item));
        await OnChange.InvokeAsync(item);
    }

    /// <inheritdoc/>
    private async Task SelectDefault()
    {
        _selectedItem = default;
        _isOpen = false;
        await OnChangeText.InvokeAsync(null);
        await OnChange.InvokeAsync(default);
        await OnCleared.InvokeAsync();
    }

    /// <inheritdoc/>
    private void OnFocusOut()
    {
        _isOpen = false;
    }

    /// <inheritdoc/>
    private string GetFullLabel(TItem item)
    {
        var first = FirstLabel(item);
        var second = SecondLabel?.Invoke(item);
        return string.IsNullOrEmpty(second) ? first : $"{first.PadRight(first.Length + SpaceLength)}{second}";
    }

    /// <summary>
    /// display the dropdown item value with ellipsis for longer text
    /// </summary>
    private string GetTruncatedLabel(TItem item)
    {
        var first = FirstLabel(item);
        var second = SecondLabel?.Invoke(item) ?? string.Empty;

        if (MaxDisplayLength <= 0)
        {
            return BuildBoldHtml(first, second);
        }

        var fullLength = string.IsNullOrEmpty(second)
            ? first.Length
            : first.Length + SpaceLength + second.Length;

        if (fullLength <= MaxDisplayLength)
        {
            return BuildBoldHtml(first, second);
        }

        if (first.Length >= MaxDisplayLength)
        {
            return BuildBoldHtml(first[..MaxDisplayLength] + Ellipsis, string.Empty);
        }

        var remaining = MaxDisplayLength - first.Length - SpaceLength;
        if (remaining <= 0)
        {
            return BuildBoldHtml(first + Ellipsis, string.Empty);
        }

        var truncatedSecond = second.Length > remaining
            ? second[..remaining] + Ellipsis
            : second;

        return BuildBoldHtml(first, truncatedSecond);
    }

    /// <summary>
    /// make the text - first + bold second HTML string
    /// </summary>
    private string BuildBoldHtml(string first, string second)
    {
        var encodedFirst = WebUtility.HtmlEncode(first);

        if (string.IsNullOrEmpty(second))
        {
            return encodedFirst;
        }

        var htmlSep = string.Concat(Enumerable.Repeat("&nbsp;", SpaceLength));
        var encodedSecond = WebUtility.HtmlEncode(second);
        return $"{encodedFirst}{htmlSep}<strong>{encodedSecond}</strong>";
    }
}
