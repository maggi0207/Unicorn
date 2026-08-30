# Antigravity Coding Rules — UI Employer Portal

> **IMPORTANT**: Antigravity must read and follow ALL rules in this file before modifying any code in this repository.

---

## 1. Environment & Repository Limitations

- **Incomplete Clone:** The AI's local repository is an incomplete copy of the original application.
- **Do Not Build:** The AI must NEVER attempt to run `dotnet build`, `dotnet run`, or any compilation commands. The local copy will fail to build.
- **Ask for Missing Files:** If a file is referenced but missing from the local repository, the AI must not guess its contents. It must explicitly ask the user to provide the file or a screenshot from their original application.
- **Focus on File Edits:** Analyse code statically. Rely entirely on the user to compile, run, and test the application.

---

## 2. General C# Style

- **No lambda expressions** — use explicit `foreach` loops and `if` statements instead of LINQ lambdas (`.Where(x => ...)`, `.Select(x => ...)`, `.Any(x => ...)`, `.RemoveAll(x => ...)` etc.)
- **No `var` with implicit type from lambdas** — declare types explicitly where possible
- **Use `out var`** instead of `out int` / `out string` etc. for `TryParse` calls (IDE0007 compliance)

---

## 3. XML Documentation Comments

- **Every new `public` or `private` property, method, and class must have an XML `<summary>` comment**
- **Every new parameter on a method must have an `<param>` XML comment**
- **Do NOT remove existing XML comments** unless explicitly asked

Example:
```csharp
/// <summary>
/// Saves the address form model to the backend service.
/// </summary>
/// <param name="model">The address form model submitted by the user.</param>
/// <param name="employerSK">The employer surrogate key.</param>
/// <returns>A tuple with success flag and error message.</returns>
public async Task<(bool success, string error)> SaveAddressAsync(AddressFormModel model, int employerSK)
```

---

## 4. Blazor / Razor Components

- **Avoid `@bind-*` attributes for complex/dropdown state** — use direct model property binding instead
- **Use scoped CSS classes** (e.g., `.del-modal-*`) — do NOT use Bootstrap utility class chains mixed with inline `style=` attributes for component-specific UI
- **Do NOT use inline `style=` attributes** for layout or colors — put all styles in the component's `.razor.css` file

---

## 5. Address Form (ManageAddresses Feature)

- **Address type dropdown (`AvailableAddressTypeOptions`)** — hides already-used address types from the dropdown, EXCEPT `AdditionalPhysicalLocationSK = 20` which allows multiple entries
- **Address type is always disabled in edit mode** — `IsAddressTypeDisabled => _isEditMode`
- **Edit = Delete old + Insert new** — when saving in edit mode (and `AddressTypeCodeSK != 11`), always call `DeleteAddressAsync` on the old address first, then `SaveAddressAsync`
- **Main Business Mailing Address (`AddressTypeCodeSK = 11`) is always the first row** — regardless of sort column or direction, it is always pinned at the top of the addresses table
- **Main Business Mailing Address (`AddressTypeCodeSK = 11`)** — never deleted during edit; the backend upserts it automatically

---

## 6. Validation & Error Banners

- **String properties drive dropdown validation** — `AddressTypeString`, `CountryString`, `StateString`, `ProvinceString` are validated in `AddressFormModel.Validate()`, which also assigns the parsed integer SK values back to the model
- **Banner field IDs must map to actual HTML element IDs** — use a `switch` to map property names to correct HTML `id` values (e.g., `AddressTypeString` → `"AddressType"`, `StateString` → `"StateCodeSK"`)
- **Validation banner shows errors in UI layout order** — collect errors in the same top-to-bottom order as the fields appear on screen

---

## 7. Delete Modal

- Use scoped `.del-modal-*` CSS classes (defined in `ManageAddresses.razor.css`)
- Do NOT use Bootstrap `.modal`, `.modal-dialog`, `.modal-content`, etc. for this modal
- Modal width: **480px**, title font-size: **17px**, body font-size: **14px**

---

## 8. How to Add New Rules

Add new rules to this file whenever:
- A coding pattern is established during a review/fix session
- A user explicitly states a preference for how code should be written
- A bug is fixed that reveals a recurring anti-pattern to avoid

---

## 9. IDE Warning Compliance

- **IDE0044 (Make field readonly):** Any private field that is assigned only in the constructor or at declaration (e.g., `private readonly List<string> _errors = new();`) must be marked as `readonly`.
- **IDE0042 (Variable declaration can be deconstructed):** When a method returns a tuple, deconstruct it directly into variables (e.g., `var (success, error) = await ...`) instead of assigning the tuple to a single variable.
- **IDE1006 (Naming rule violation):** Method parameters must use `camelCase` formatting, even in generic methods or delegates. `private static readonly` fields must use `PascalCase` without a `_` prefix (e.g., `PostalCodePattern` not `_postalCodePattern`). Instance fields in non-static classes may keep the `_` prefix.
- **IDE0046 ('if' statement can be simplified):** If an `if` statement can be simplified into a single return expression (e.g., returning a boolean condition directly instead of `if (x) return true; else return false;`), simplify it to satisfy the IDE rule, bypassing the "always use brackets for if statements" rule when applicable.
- **IDE0028 (Simplify collection initialization):** Use C# 12 collection expressions (`[]` instead of `new()` or `new List<T>()`) to initialize lists, arrays, and sets.
- **IDE0007 (Use 'var' instead of explicit type):** Use `var` for local variable declarations when the type is apparent or assigned from expressions (e.g., `var startYear = dateFirstPaidWages.Value.Year;`, `var entry = new YearQuartersPaidWages(...)`).
- **IDE0048 (Add parentheses for clarity):** Add explicit parentheses around arithmetic, logical, or division sub-expressions for operator precedence clarity (e.g., `var startQuarter = ((month - 1) / 3) + 1;`).
