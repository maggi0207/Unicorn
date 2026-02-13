# Project: Wisconsin DWD Employer Portal

## Overview
ASP.NET Core Blazor Web Application (.NET 8) for the Wisconsin Department of Workforce Development (DWD) Employer Portal. Multi-step employer registration wizard with interactive server-side rendering.

## Tech Stack
- **Framework:** .NET 8, Blazor Web App
- **Render Mode:** `@rendermode InteractiveServer` (all interactive pages)
- **Validation:** DataAnnotations + CustomValidator (for nested objects)
- **CSS:** Scoped CSS isolation (`.razor.css` files), no CSS framework beyond base Bootstrap

## Project Structure
```
UI.EmployerPortal.Web/
├── Components/              # Root layout, shared components
│   ├── Layout/             # MainLayout, NavMenu
│   ├── Pages/              # Default Blazor pages (Home, Error)
│   └── Shared/             # Shared UI components
├── Features/               # Feature-based organization
│   └── EmployerRegistration/
│       ├── Components/     # Reusable field components
│       │   ├── OutlinedTextField.razor
│       │   ├── OutlinedSelectField.razor
│       │   ├── AddressField.razor
│       │   ├── PhoneNumberField.razor
│       │   ├── FEINField.razor
│       │   ├── FieldError.razor
│       │   ├── CustomValidator.razor
│       │   └── SelectOption.cs
│       ├── Models/         # Data models with DataAnnotations
│       └── *.razor         # Feature pages (wizard steps)
└── wwwroot/                # Static assets
```

## Code Conventions

### Blazor Components
- **All code in `@code {}` block** — NO `.razor.cs` code-behind files
- Reusable components accept `[Parameter]` props and use `EventCallback<T>` for two-way binding
- Validation uses `[CascadingParameter] EditContext` + `Expression<Func<string>> For` pattern
- Components notify EditContext of changes via `EditContext.NotifyFieldChanged(FieldIdentifier.Create(For))`

### CSS Naming
- `master-` prefix: shared input component styles (master-text-field, master-input, master-select)
- `bi-` prefix: BusinessInformation page-specific styles
- BEM-style modifiers: `master-input--error`, `master-label--error`
- Page wrapper class: `page_wrapper` > `bi-page-content`

### Form Validation Pattern
- `EditForm` with `EditContext` (not `Model` directly)
- `DataAnnotationsValidator` for top-level properties
- `CustomValidator` with `ValidationMessageStore` for nested objects (AddressModel)
- Manual validation via `Validator.TryValidateObject()` in `HandleContinue()`
- `formSubmitted` flag gates error visibility in UI
- `hasValidationErrors` flag controls top error banner

### Layout
- 2-column grid: `330px 330px`, `24px` gap = `684px` total width
- Zip code row: special layout with zip field + dash + extension field

## Registration Wizard Steps
1. Account Type
2. Ownership
3. **Business Information** (current focus)
4. Address Correction
5. (more steps TBD)

Route pattern: `/employer-registration/{step-name}`

## Key Models
- `BusinessInformationModel` — FEIN, LegalName, TradeName, Phone, Email + MailingAddress + PhysicalLocations
- `AddressModel` — Country, AddressLine1/2, City, State, Zip, Extension (reusable for mailing & physical)
- `SelectOption` — simple { Value, Text } for dropdowns

## Important Patterns
- **Auto-formatting inputs:** PhoneNumberField (999-999-9999) and FEINField (99-9999999) strip non-digits, limit length, and insert dashes automatically
- **Dynamic physical locations:** Max 2, "Add Another" button hides at max, first location cannot be removed
- **Error clearing on typing:** Components notify EditContext on input/change, CustomValidator clears per-field errors on OnFieldChanged

## Build & Run
```bash
dotnet build
dotnet run
```
Default URLs: https://localhost:7275, http://localhost:5259

## User Preferences
- Keep code in @code{} block, never use .razor.cs code-behind
- Prefer creating reusable components over inline duplication
- Service/API integration is deferred — focus on UI and validation only
