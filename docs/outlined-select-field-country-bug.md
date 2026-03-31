# Bug: OutlinedSelectField — Country Value Not Displaying / Disappearing

**Component:** `OutlinedSelectField.razor`
**Affected Scenarios:** Business Information page — Country dropdown in Mailing Address and Physical Location sections
**Fixed in commit:** `25fd460`

---

## Symptoms

Three distinct failures, all on the **Country** `<select>` field:

| # | Scenario | Observed Behaviour |
|---|---|---|
| 1 | User opens the page and selects a country | Selected value immediately disappears; dropdown reverts to blank or previous selection |
| 2 | User checks "Physical Location is the same as the Business address" | Country value is not copied from mailing address into the physical location dropdown |
| 3 | User clicks "Edit Address" on the Address Correction page and returns to Business Information | Country dropdown appears blank even though the model has the previously entered value |

State field (also an `OutlinedSelectField`) was not affected because it uses `@bind-Value` auto-binding, which goes through a slightly different code path.

---

## Root Cause

### Background — how Blazor sets `<select>` values

Blazor must set `element.value` as a **DOM property** (not an HTML `value` attribute) to visually update a `<select>` element's selected option after the initial render. Using `element.setAttribute('value', 'Canada')` has no effect on the browser's current selection state; only `element.value = 'Canada'` (property assignment) works. Blazor's `@bind` directive on `<select>` uses the property-assignment path internally.

### The async timing race

The component used `@bind="_currentValue" @bind:after="OnAfterBind"` where `OnAfterBind` was an `async Task`:

```csharp
// Previous broken implementation
private async Task OnAfterBind()
{
    _isUserChanging = true;       // too late — a render may have already fired
    Value = _currentValue;
    await ValueChanged.InvokeAsync(Value);  // potential yield point
    EditContext.NotifyFieldChanged(...);
    _isUserChanging = false;
}
```

Blazor's internal binder calls `StateHasChanged()` as part of the `@bind` pipeline — **before** `@bind:after` executes. This queues a render. If that render fires before `OnAfterBind` sets `_isUserChanging = true`, or between the `await` and the parent model update (`Address.Country = newValue`), the following sequence occurs:

```
1. User selects "Canada"
2. @bind sets _currentValue = "Canada"
3. Blazor StateHasChanged() queued internally by binder
4. [render fires] → OnParametersSet runs
5.   _isUserChanging == false → _currentValue = Value ?? "" = "United States"  ← stale!
6.   element.value = "United States"  ← selection visually reverts
7. OnAfterBind resumes:
8.   Value = _currentValue = "United States"  ← wrong value propagated to parent
```

The `_isUserChanging` guard (introduced as a mitigation) was itself subject to a timing window — there was no safe point to set the flag before Blazor's internal binder logic ran.

The same race explains **scenario 2 and 3**: during a programmatic update (parent sets `Address.Country` and calls `StateHasChanged`), if any intermediate render fired with a stale `Value`, `OnParametersSet` would reset `_currentValue` and the `@bind` would push that stale value back to the DOM.

---

## Approaches Tried (and Why They Failed)

### Attempt 1 — `value="@(Value ?? "")"` + `selected="@(Value == option.Value)"` on options

```razor
<select value="@(Value ?? "")" @onchange="OnChange">
    <option value="@opt.Value" selected="@(Value == opt.Value)">@opt.Text</option>
```

**Problem:** Sets `value` as an HTML *attribute*, not a DOM *property*. The browser ignores attribute patches on `<select>` for selection control after initial render. Programmatic updates (copy, edit-return) did not visually update the dropdown.

### Attempt 2 — `@bind="_currentValue"` + `@bind:after="OnAfterBind"` (async) + `_isUserChanging` flag

```csharp
private async Task OnAfterBind()
{
    _isUserChanging = true;
    Value = _currentValue;
    await ValueChanged.InvokeAsync(Value);
    EditContext.NotifyFieldChanged(...);
    _isUserChanging = false;
}

protected override void OnParametersSet()
{
    if (!_isUserChanging)   // guard
        _currentValue = Value ?? "";
}
```

**Problem:** Blazor's binder calls `StateHasChanged()` *before* `@bind:after` runs. The guard was set too late. Any render fired in the window between `@bind` completing and `_isUserChanging = true` would still reset `_currentValue`.

### Attempt 3 — `value="@_currentValue"` + `@onchange="OnChange"` + `_currentValue` backing field

```csharp
private async Task OnChange(ChangeEventArgs e)
{
    _currentValue = e.Value?.ToString() ?? "";   // sync first
    Value = _currentValue;
    await ValueChanged.InvokeAsync(Value);
    ...
}
```

**Problem:** `value="@_currentValue"` uses Blazor's attribute path (not property-assignment path) for `<select>`, so programmatic updates still did not update the visual selection reliably.

---

## Fix

Bind to a **C# property** (`BoundValue`) instead of using `@bind:after`. The property setter runs **synchronously** — it updates `_currentValue`, propagates to the parent model, and notifies `EditContext` all before any render can observe the state.

```razor
<select @bind="BoundValue" @onblur="HandleBlur" ...>
```

```csharp
private string _currentValue = "";

/// <summary>
/// Property bound via @bind. Setter runs all notifications synchronously —
/// no async gap in which OnParametersSet could reset _currentValue with stale data.
/// </summary>
private string BoundValue
{
    get => _currentValue;
    set
    {
        if (_currentValue == value) return;
        _currentValue = value;
        Value = value;
        _ = ValueChanged.InvokeAsync(value);   // sync for our callbacks
        if (EditContext is not null && ResolvedExpression is not null)
            EditContext.NotifyFieldChanged(FieldIdentifier.Create(ResolvedExpression));
    }
}

protected override void OnParametersSet()
{
    _currentValue = Value ?? "";   // safe — parent model is always up-to-date by render time
    // ... EditContext wiring
}
```

### Why this works for all three scenarios

| Scenario | Flow |
|---|---|
| **User selects "Canada"** | `@bind` calls `BoundValue.set("Canada")` → `_currentValue = "Canada"`, `Address.Country = "Canada"` — all sync, no yield. By the time any render fires, both are "Canada". `OnParametersSet` sets `_currentValue = "Canada"`. ✓ |
| **Copy (same-as-mailing checkbox)** | Parent sets `dest.Country = "Canada"`, calls `StateHasChanged()`. Re-render: `OnParametersSet` sets `_currentValue = "Canada"`. `@bind` drives `element.value = "Canada"` via DOM property. ✓ |
| **Edit-return from Address Correction** | Component initializes fresh. `OnParametersSet` sets `_currentValue = "United States"` from restored model. `@bind` renders `element.value = "United States"`. ✓ |

### Key technical notes

- `@bind` on `<select>` in Blazor calls `element.value = value` (DOM property), **not** `element.setAttribute('value', value)`. This is required for the browser to update visual selection after initial render.
- `ValueChanged.InvokeAsync(value)` is fire-and-forget (`_ =`) intentionally. All `ValueChanged` callbacks in this project (`OnCountryChanged`, auto-generated `@bind-Value` setters) are synchronous, so the returned `Task` is always already completed.
- `_isUserChanging` and `OnAfterBind` were removed entirely — they are no longer needed.

---

## Files Changed

| File | Change |
|---|---|
| `src/UI.EmployerPortal.Razor.SharedComponents/Inputs/OutlinedSelectField.razor` | Replace `@bind:after` + async `OnAfterBind` + `_isUserChanging` with synchronous `BoundValue` property setter |
| `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/BusinessInformation.razor` | Restore `@key` on physical location `AddressField` so it recreates correctly when `_physicalSameAsMailing` toggles |

---

## Commit History

| Commit | Description |
|---|---|
| `c1527a1` | Initial fix attempt — `@bind` + `@bind:after` + `_currentValue` backing field |
| `70fe775` | Second attempt — switched to `value=` + `@onchange` to avoid `@bind:after` async |
| `4519ce7` | Third attempt — restored `@bind:after` with `_isUserChanging` guard |
| `25fd460` | **Final fix** — synchronous `BoundValue` property setter, no `@bind:after` |
