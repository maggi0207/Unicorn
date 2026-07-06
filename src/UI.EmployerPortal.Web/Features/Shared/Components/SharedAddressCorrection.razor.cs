using Microsoft.AspNetCore.Components;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.Shared.Components;

public partial class SharedAddressCorrection : ComponentBase
{
    [Parameter, EditorRequired]
    public List<AddressCorrectionItem> Corrections { get; set; } = new();

    [Parameter]
    public EventCallback OnEditAddress { get; set; }

    [Parameter]
    public EventCallback<List<AddressCorrectionItem>> OnContinue { get; set; }

    protected HashSet<int> ItemErrors = new();
    protected Dictionary<int, string> Selections = new();
    protected HashSet<int> AcceptedAsEntered = new();

    protected const string SelectionCorrected = "corrected";
    protected const string SelectionEntered = "entered";

    protected override void OnInitialized()
    {
        if (!Corrections.Exists(ac => !ac.IsOnlyCapitalizationOrZipExtensionChange))
        {
            ApplyAndNotify();
        }
    }

    protected static string GetTypeName(string label) => label.ToLower();

    protected static string GetSectionTitle(string label) => label;

    protected string? GetSelection(int index) => Selections.TryGetValue(index, out var v) ? v : null;

    protected void SetSelection(int index, string value)
    {
        Selections[index] = value;
        ItemErrors.Remove(index);
    }

    protected void SetSelectionCorrected(int index) => SetSelection(index, SelectionCorrected);
    protected void SetSelectionEntered(int index) => SetSelection(index, SelectionEntered);

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

    protected async Task HandleEditAddress()
    {
        if (OnEditAddress.HasDelegate)
        {
            await OnEditAddress.InvokeAsync();
        }
    }

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

    protected static string FormatLine1(AddressModel a)
    {
        var parts = new[] { a.AddressLine1, a.AddressLine2 }
            .Where(s => !string.IsNullOrWhiteSpace(s));
        return string.Join(" ", parts).ToUpperInvariant();
    }

    protected static string FormatLine2(AddressModel a)
    {
        var zip = string.IsNullOrWhiteSpace(a.Extension)
            ? a.Zip
            : $"{a.Zip}-{a.Extension}";
        return $"{a.City?.ToUpperInvariant()}, {a.State} {zip}".Trim();
    }
}
