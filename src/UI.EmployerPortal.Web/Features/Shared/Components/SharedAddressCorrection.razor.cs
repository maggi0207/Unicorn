using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.Shared.Components;

/// <summary>
/// A reusable component that displays address validation warnings and allows the user
/// to select between an originally entered address and a suggested corrected address.
/// </summary>
public partial class SharedAddressCorrection : ComponentBase
{
    /// <summary>
    /// The list of address corrections to display to the user.
    /// </summary>
    [Parameter, EditorRequired]
    public List<AddressCorrectionItem> Corrections { get; set; } = new();

    /// <summary>
    /// Event triggered when the user clicks the "Edit Address" button.
    /// Typically used by the parent to navigate back to the original entry form.
    /// </summary>
    [Parameter]
    public EventCallback OnEditAddress { get; set; }

    /// <summary>
    /// Event triggered when the user successfully resolves all address issues and clicks Continue.
    /// Passes back the list of corrections. The parent should typically apply the user's choices.
    /// </summary>
    [Parameter]
    public EventCallback<List<AddressCorrectionItem>> OnContinue { get; set; }

    /// <summary>Indices of items that were unresolved when CONTINUE was clicked.</summary>
    protected HashSet<int> ItemErrors = new();
    
    /// <summary>Per-item radio selection: "corrected" or "entered". Only for items with a suggestion.</summary>
    protected Dictionary<int, string> Selections = new();
    
    /// <summary>Indices of no-suggestion items accepted via checkbox.</summary>
    protected HashSet<int> AcceptedAsEntered = new();

    /// <summary>Constant representing the choice to use the corrected address.</summary>
    protected const string SelectionCorrected = "corrected";
    
    /// <summary>Constant representing the choice to use the original entered address.</summary>
    protected const string SelectionEntered = "entered";

    /// <summary>
    /// Initializes the component. Automatically proceeds if no actionable corrections exist.
    /// </summary>
    protected override void OnInitialized()
    {
        if (!Corrections.Exists(ac => !ac.IsOnlyCapitalizationOrZipExtensionChange))
        {
            ApplyAndNotify();
        }
    }

    /// <summary>Returns the lowercase type name for use in body text and radio labels.</summary>
    protected static string GetTypeName(string label) => label.ToLower();

    /// <summary>Returns the label as-is for section headers.</summary>
    protected static string GetSectionTitle(string label) => label;

    /// <summary>Returns the current radio selection for item at the specified index, or null.</summary>
    protected string? GetSelection(int index) => Selections.TryGetValue(index, out var v) ? v : null;

    /// <summary>Sets the radio selection for a specific correction item.</summary>
    protected void SetSelection(int index, string value)
    {
        Selections[index] = value;
        ItemErrors.Remove(index);
    }

    /// <summary>Marks the user's selection to use the corrected address.</summary>
    protected void SetSelectionCorrected(int index) => SetSelection(index, SelectionCorrected);
    
    /// <summary>Marks the user's selection to use the entered address.</summary>
    protected void SetSelectionEntered(int index) => SetSelection(index, SelectionEntered);

    /// <summary>Toggles the "use as entered" checkbox for a no-suggestion item.</summary>
    protected void ToggleAcceptAsEntered(int index, ChangeEventArgs e)
    {
        if (e.Value is true)
        {
            AcceptedAsEntered.Add(index);
            ItemErrors.Remove(index);
        }
        else
        {
            AcceptedAsEntered.Remove(index);
        }
    }

    /// <summary>
    /// Invokes the OnEditAddress callback to allow the parent to handle manual edits.
    /// </summary>
    protected async Task HandleEditAddress()
    {
        if (OnEditAddress.HasDelegate)
        {
            await OnEditAddress.InvokeAsync();
        }
    }

    /// <summary>
    /// Validates that all items are resolved. If so, applies corrections and invokes OnContinue.
    /// </summary>
    protected void HandleContinue()
    {
        ItemErrors.Clear();

        for (var i = 0; i < Corrections.Count; i++)
        {
            var item = Corrections[i];
            if (!item.IsOnlyCapitalizationOrZipExtensionChange)
            {
                var resolved = item.Suggested is not null
                    ? Selections.ContainsKey(i)
                    : AcceptedAsEntered.Contains(i);

                if (!resolved)
                    ItemErrors.Add(i);
            }
        }

        if (ItemErrors.Count > 0)
            return;

        ApplyAndNotify();
    }

    /// <summary>
    /// Applies the accepted suggestions to the original address objects and notifies the parent.
    /// </summary>
    private void ApplyAndNotify()
    {
        for (var i = 0; i < Corrections.Count; i++)
        {
            var item = Corrections[i];
            if (item.Suggested is not null
                && (item.IsOnlyCapitalizationOrZipExtensionChange
                    || (Selections.TryGetValue(i, out var choice)
                    && choice == SelectionCorrected)))
            {
                ApplyCorrection(item.Original, item.Suggested);
            }
        }

        if (OnContinue.HasDelegate)
        {
            OnContinue.InvokeAsync(Corrections);
        }
    }

    /// <summary>
    /// Copies all address fields from the source model into the target model.
    /// </summary>
    public static void ApplyCorrection(AddressModel target, AddressModel source)
    {
        target.AddressLine1 = source.AddressLine1;
        target.AddressLine2 = source.AddressLine2;
        target.City = source.City;
        target.State = source.State;
        target.Zip = source.Zip;
        target.Extension = source.Extension;
        target.Country = source.Country;
    }

    /// <summary>Formats the street address lines into a single string.</summary>
    protected static string FormatLine1(AddressModel a)
    {
        var parts = new[] { a.AddressLine1, a.AddressLine2 }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join(" ", parts).ToUpperInvariant();
    }

    /// <summary>Formats the city, state, and zip code into a single string.</summary>
    protected static string FormatLine2(AddressModel a)
    {
        var zip = string.IsNullOrWhiteSpace(a.Extension)
            ? a.Zip
            : $"{a.Zip}-{a.Extension}";
        return $"{a.City?.ToUpperInvariant()}, {a.State} {zip}".Trim();
    }
}
