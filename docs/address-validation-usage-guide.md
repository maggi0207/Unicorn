# Address Validation & Correction — Developer Usage Guide

This guide explains how to wire up address validation and the Address Correction page in any new wizard step that uses the `AddressField` component.

---

## How It Works — Overview

```
Wizard step (e.g. BusinessInformation)
  └─► calls AddressValidationCoordinator.ValidateAndRedirectAsync(...)
        └─► AddressCorrectionHelper.CollectCorrectionsAsync(...)
              └─► IAddressValidationWrapper.ValidateAsync(address)   [WCF service]
        └─► if corrections exist → saves state → navigates to /address-correction
        └─► if no corrections  → returns true → wizard advances normally

AddressCorrection page
  └─► user reviews each flagged address, picks "use corrected" or "use as entered"
  └─► clicks CONTINUE → applies choices to the original AddressModel objects in-place
  └─► sets CurrentStep = PostCorrectionStep → navigates back to the wizard
```

---

## Step 1 — Register the services

Both services must be registered once in `Program.cs` (already done):

```csharp
builder.Services.AddScoped<AddressValidationCoordinator>();
builder.Services.AddScoped<IAddressValidationWrapper, AddressValidationService>();
```

---

## Step 2 — Inject the coordinator into your wizard step

```razor
@inject AddressValidationCoordinator AddressCoordinator
@inject RegistrationStateService RegistrationState
```

---

## Step 3 — Call `ValidateAndRedirectAsync` inside `Validate()`

Call this **after** local DataAnnotations validation passes. Pass:

| Parameter | What to provide |
|---|---|
| `addresses` | A list of `(label, AddressModel)` tuples — one per address field on the page |
| `editStep` | The wizard step index of **this** page (so EDIT ADDRESS comes back here) |
| `postCorrectionStep` | The wizard step to advance to after corrections are resolved |
| `onBeforeNavigate` | A callback that saves your page model to `RegistrationState` before navigating away |

```csharp
public async Task<bool> Validate()
{
    formSubmitted = true;
    var isValid = editContext.Validate() && ValidateNestedAddresses();

    if (!isValid)
    {
        hasValidationErrors = true;
        return false;
    }

    return await AddressCoordinator.ValidateAndRedirectAsync(
        addresses: [
            ("Business Mailing Address", Model.MailingAddress),
            ("Physical Location 1",      Model.PhysicalLocations[0])
        ],
        editStep:            3,   // step index of this page
        postCorrectionStep:  4,   // step to resume after correction
        onBeforeNavigate:    () => RegistrationState.MyPageModel = Model
    );
}
```

**Return value:** `true` = all addresses are fine, wizard may advance. `false` = corrections needed (navigation already triggered).

---

## Step 4 — Restore model state on return

When the user comes back from Address Correction via EDIT ADDRESS, the component re-initialises. Restore the saved model in `OnInitialized`:

```csharp
protected override void OnInitialized()
{
    if (RegistrationState.MyPageModel is not null)
        Model = RegistrationState.MyPageModel;

    editContext = new EditContext(Model);
    // ... re-attach validators
}
```

---

## Step 5 — Handle "Same as Mailing" mirroring (optional)

If your page has a "same as mailing" checkbox, set the flag on `RegistrationStateService` so the Address Correction page can mirror any accepted mailing correction to the physical address automatically:

```csharp
RegistrationState.PhysicalSameAsMailing = physicalSameAsMailing;
```

The Address Correction page reads this flag and copies the corrected mailing address into `RegistrationState.BusinessInfo.PhysicalLocations[0]` when the user accepts the suggestion.

---

## Address Correction Page — What it does automatically

You do **not** need to modify the `AddressCorrection.razor` page. It:

- Displays one section per item in `RegistrationState.AddressCorrections`
- Shows two radio buttons ("Use corrected" / "Use as entered") when a suggestion exists
- Shows a confirmation checkbox when no suggestion exists (service could not standardize)
- Applies choices to the original `AddressModel` objects **in-place** on CONTINUE — so the models your page holds are automatically updated
- Navigates back to the wizard at `RegistrationState.PostCorrectionStep`
- Navigates back to your page at `RegistrationState.EditStep` when the user clicks EDIT ADDRESS

---

## Address Labels

Use consistent labels so the Address Correction page headings are clear:

| Address | Recommended label |
|---|---|
| Business mailing address | `"Business Mailing Address"` |
| First physical location | `"Physical Location 1"` |
| Additional physical locations | `"Physical Location 2"`, `"Physical Location 3"` |
| Business contact address | `"Business Contact Address"` |

---

## Skipping an address

Pass only the addresses that are visible and filled in. For example, skip the physical address when the "same as mailing" checkbox is checked:

```csharp
var addressesToValidate = new List<(string, AddressModel)>
{
    ("Business Mailing Address", Model.MailingAddress)
};

if (!physicalSameAsMailing)
    addressesToValidate.Add(("Physical Location 1", Model.PhysicalLocations[0]));

return await AddressCoordinator.ValidateAndRedirectAsync(
    addressesToValidate, editStep: 3, postCorrectionStep: 4,
    onBeforeNavigate: () => RegistrationState.BusinessInfo = Model);
```

---

## Key types at a glance

| Type | Location | Purpose |
|---|---|---|
| `IAddressValidationWrapper` | `Services/` | Interface over the WCF client — inject this in tests |
| `AddressValidationService` | `Services/` | Real WCF implementation |
| `AddressValidationCoordinator` | `Services/` | Orchestrates validation + navigation — inject this in wizard steps |
| `AddressCorrectionHelper` | `Services/` | Static helper; collects correction items from validation results |
| `AddressCorrectionItem` | `Services/RegistrationStateService.cs` | Record holding one flagged address (label, original, suggested, error) |
| `RegistrationStateService` | `Services/` | Scoped state bag shared between wizard pages |
| `AddressValidationResult` | `Services/IAddressValidationWrapper.cs` | Record returned by `ValidateAsync` |
