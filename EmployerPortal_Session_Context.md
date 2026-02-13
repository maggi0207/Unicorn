# Employer Portal — Business Information Step — Full Session Context

> Use this file at the start of a new Claude Code session to resume work on the project.
> It contains the complete project state, all files, patterns, conventions, and decisions made.

---

## 1. Project Overview

**Organization:** Wisconsin Department of Workforce Development (DWD)
**Project:** Employer Portal Modernization
**What we're building:** Business Information page (Step 3 of employer registration wizard)
**Status:** UI complete, builds with 0 code errors. Not yet tested in browser. Git push pending for refactored code.

**Technology Stack:**
- .NET 8 Blazor Web App (InteractiveServer render mode)
- C# with code-behind pattern
- Feature-based folder architecture
- EditForm + EditContext + DataAnnotationsValidator for validation
- Scoped CSS (.razor.css files)
- Future: WCF/web service integration (deferred — UI only for now)

**GitHub:** `https://github.com/maggi0207/UI.EmployerPortal.Web`
**Git user:** maggi0207
**Local path:** `D:\UI.EmployerPortal.Web`
**SDK:** .NET 8.0.417 (also has 10.0.102 installed — project targets net8.0)
**IDE:** Visual Studio 2022/2026

---

## 2. Project Structure

```
D:\UI.EmployerPortal.Web\
├── Components\
│   ├── App.razor                          # Root HTML shell
│   ├── Routes.razor                       # Router config
│   ├── _Imports.razor                     # Global component imports
│   ├── Layout\
│   │   ├── MainLayout.razor               # Default layout (sidebar + content)
│   │   ├── MainLayout.razor.css
│   │   ├── NavMenu.razor
│   │   └── NavMenu.razor.css
│   └── Pages\
│       ├── Home.razor                     # Default home page
│       ├── Counter.razor                  # (scaffold)
│       ├── Error.razor                    # Error page
│       └── Weather.razor                  # (scaffold)
│
├── Features\
│   ├── _Imports.razor                     # Feature-level imports (RenderMode, Components, Models)
│   └── EmployerRegistration\
│       ├── BusinessInformation.razor      # ★ Main page (Step 3)
│       ├── BusinessInformation.razor.cs   # ★ Code-behind
│       ├── BusinessInformation.razor.css  # ★ Scoped CSS
│       ├── Components\
│       │   ├── OutlinedTextField.razor     # Text input with floating label
│       │   ├── OutlinedTextField.razor.css
│       │   ├── OutlinedSelectField.razor   # Select dropdown with floating label
│       │   ├── OutlinedSelectField.razor.css
│       │   ├── FieldError.razor            # Inline validation error
│       │   ├── FieldError.razor.css
│       │   └── SelectOption.cs             # Data model for dropdown options
│       └── Models\
│           └── BusinessInformationModel.cs # Form model with DataAnnotations
│
├── wwwroot\
│   ├── app.css                            # Global styles (font: Verdana)
│   └── ...
│
├── test file\                             # ★ READ-ONLY reference from team's Step 1
│   ├── Components\OutlinedTextField.razor + .css
│   ├── Model\EmployerRegOrchestrator.cs + RegistrationNumberSelection.cs
│   └── Pages\EmployerRegEntryPoint.razor + .css + Orchestrator
│
├── Program.cs                             # App startup (InteractiveServer configured)
├── UI.EmployerPortal.Web.csproj           # .NET 8 project (excludes test file\ from build)
├── UI.EmployerPortal.Web.sln
└── .gitignore
```

---

## 3. Key Patterns & Conventions (from team's reference code)

### Component Pattern
- Components use `@code {}` inline (NOT code-behind)
- Pages use code-behind (`.razor` + `.razor.cs` + `.razor.css`)
- CSS class prefix: `master-` for shared input components (e.g., `master-text-field`, `master-input`)

### Validation Pattern
- `EditForm` + `EditContext` + `DataAnnotationsValidator` (NOT manual validation)
- `formSubmitted` boolean flag controls when errors appear
- Errors only show after form submission attempt (not on load)
- `Visible` parameter on components controls error display
- `For` expression parameter (`Expression<Func<string>>`) identifies bound field for validation
- `FieldError` component shows inline per-field errors (no summary box)
- `OnValidSubmit` → navigate forward; `OnInvalidSubmit` → set `formSubmitted = true`

### Page Layout Pattern
- `page_wrapper` class for centering: `display: flex; justify-content: center;`
- Content div inside with fixed width (684px for 2-column grid)
- Grid: `330px 330px` columns, `24px` gap

### Navigation
- Back → `/ownership`
- Save & Quit → `/dashboard`
- Continue → `/address-correction`

### Namespace Convention
- Pages: `UI.EmployerPortal.Web.Features.EmployerRegistration`
- Components: `UI.EmployerPortal.Web.Features.EmployerRegistration.Components`
- Models: `UI.EmployerPortal.Web.Features.EmployerRegistration.Models`

---

## 4. Design Specifications (from Figma)

- **Font:** Verdana throughout (not Helvetica)
- **Primary color:** #003663 (dark navy blue — buttons, section headers, borders)
- **Error color:** #C62828 (red — validation errors)
- **Link color:** #0D6EFD (blue — "Add Another Physical Location")
- **Input height:** 56px
- **Input border:** 1px solid #adadad (normal), #003663 (focus), #C62828 (error)
- **Floating label:** position absolute, top: -8px, left: 12px, background white, font 12px
- **Grid:** 2 columns × 330px each, 24px gap = 684px total
- **Buttons:** Primary (filled #003663), Secondary (outlined #003663), Tertiary (text-only)
- **Responsive:** Single column below 575px

---

## 5. All Current Files — Complete Source

### 5.1 `UI.EmployerPortal.Web.csproj`

```xml
<Project Sdk="Microsoft.NET.Sdk.Web">

  <PropertyGroup>
    <TargetFramework>net8.0</TargetFramework>
    <Nullable>enable</Nullable>
    <ImplicitUsings>enable</ImplicitUsings>
  </PropertyGroup>

  <ItemGroup>
    <Compile Remove="test file\**" />
    <Content Remove="test file\**" />
    <None Remove="test file\**" />
  </ItemGroup>

</Project>
```

### 5.2 `Program.cs`

```csharp
using UI.EmployerPortal.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
```

### 5.3 `Components\_Imports.razor`

```razor
@using System.Net.Http
@using System.Net.Http.Json
@using Microsoft.AspNetCore.Components.Forms
@using Microsoft.AspNetCore.Components.Routing
@using Microsoft.AspNetCore.Components.Web
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web.Virtualization
@using Microsoft.JSInterop
@using UI.EmployerPortal.Web
@using UI.EmployerPortal.Web.Components
```

### 5.4 `Features\_Imports.razor`

```razor
@using static Microsoft.AspNetCore.Components.Web.RenderMode
@using Microsoft.AspNetCore.Components.Web
@using Microsoft.AspNetCore.Components.Forms

@using UI.EmployerPortal.Web.Features.EmployerRegistration
@using UI.EmployerPortal.Web.Features.EmployerRegistration.Components
@using UI.EmployerPortal.Web.Features.EmployerRegistration.Models
```

### 5.5 `Components\App.razor`

```html
<!DOCTYPE html>
<html lang="en">

<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <base href="/" />
    <link rel="stylesheet" href="bootstrap/bootstrap.min.css" />
    <link rel="stylesheet" href="app.css" />
    <link rel="stylesheet" href="UI.EmployerPortal.Web.styles.css" />
    <link rel="icon" type="image/png" href="favicon.png" />
    <HeadOutlet />
</head>

<body>
    <Routes />
    <script src="_framework/blazor.web.js"></script>
</body>

</html>
```

### 5.6 `Components\Routes.razor`

```razor
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
        <FocusOnNavigate RouteData="routeData" Selector="h1" />
    </Found>
</Router>
```

### 5.7 `wwwroot\app.css` (global styles — font set to Verdana)

```css
html, body {
    font-family: Verdana, Geneva, sans-serif;
    font-size: 16px;
    color: #000000;
}

a, .btn-link {
    color: #006bb7;
}

.btn-primary {
    color: #fff;
    background-color: #1b6ec2;
    border-color: #1861ac;
}

.btn:focus, .btn:active:focus, .btn-link.nav-link:focus, .form-control:focus, .form-check-input:focus {
  box-shadow: 0 0 0 0.1rem white, 0 0 0 0.25rem #258cfb;
}

.content {
    padding-top: 1.1rem;
}

h1:focus {
    outline: none;
}

.valid.modified:not([type=checkbox]) {
    outline: 1px solid #26b050;
}

.invalid {
    outline: 1px solid #e50000;
}

.validation-message {
    color: #e50000;
}

.blazor-error-boundary {
    background: url(data:image/svg+xml;base64,...) no-repeat 1rem/1.8rem, #b32121;
    padding: 1rem 1rem 1rem 3.7rem;
    color: white;
}

    .blazor-error-boundary::after {
        content: "An error has occurred."
    }

.darker-border-checkbox.form-check-input {
    border-color: #929292;
}
```

---

### 5.8 `Features\EmployerRegistration\Components\OutlinedTextField.razor`

```razor
@using Microsoft.AspNetCore.Components.Forms
@using System.Linq.Expressions
<!-- Text field container with error state class -->
<div class="master-text-field @(HasError ? "master-text-field--error" : "")"
     aria-label="@Label">

    <!-- Label with error color -->
    <label for="@Id"
           class="@(HasError ? "master-label--error" : "")">
        @Label
    </label>

    <!-- Input field with error styling -->
    <input id="@Id"
           class="master-input @(HasError ? "master-input--error" : "")"
           type="@Type"
           value="@Value"
           @oninput="OnInput"
           disabled="@Disabled"
           aria-label="@Label"
           aria-invalid="@HasError" />

    <!-- Format/Hint Text -->
    @if (!string.IsNullOrWhiteSpace(FormatText))
    {
    <div class="master-hint">
        @FormatText
    </div>
    }
</div>
@code {
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public string Type { get; set; } = "text";
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Visible { get; set; }
    [Parameter] public string? FormatText { get; set; }
    [Parameter] public Expression<Func<string>>? For { get; set; }
    [CascadingParameter] private EditContext? EditContext { get; set; }
    private bool HasError =>
        Visible &&
        EditContext is not null &&
        For is not null &&
        EditContext.GetValidationMessages(FieldIdentifier.Create(For)).Any();
    private async Task OnInput(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
}
```

### 5.9 `Features\EmployerRegistration\Components\OutlinedTextField.razor.css`

```css
.master-text-field {
    position: relative;
    width: 100%;
    max-width: 330px;
}
/* Input */
.master-input {
    width: 100%;
    height: 56px;
    padding-left: 16px;
    padding-right: 14px;
    font-size: 16px;
    font-family: inherit;
    background-color: #ffffff;
    border: 1px solid #adadad;
    border-radius: 4px;
    outline: none;
    box-sizing: border-box;
}
    /* Focus state */
    .master-input:focus {
        border-color: #003663;
    }
/* Label on border */
.master-text-field label {
    position: absolute;
    top: -8px;
    left: 12px;
    padding: 0 6px;
    background: #ffffff;
    font-size: 12px;
    color: #333;
    font-family: Verdana;
    font-weight: 400;
    font-size: 12px;
    line-height: 150%;
    letter-spacing: 0%;
}
/* Disabled */
.master-input:disabled {
    background-color: #f5f5f5;
    color: #777;
}

/* Format/ Hint Text*/
.master-hint {
    margin: 6px;
    font-size: 12px;
    color: #333;
}

/* Error state styles */
.master-input--error {
    border-color: #C62828 !important;
}
.master-label--error {
    color: #C62828 !important;
}
.master-text-field--error {
    border-color: #C62828 !important;
}
    .master-text-field--error label {
        color: #C62828;
    }
```

### 5.10 `Features\EmployerRegistration\Components\OutlinedSelectField.razor`

```razor
@using Microsoft.AspNetCore.Components.Forms
@using System.Linq.Expressions
<!-- Select field container with error state class -->
<div class="master-select-field @(HasError ? "master-select-field--error" : "")"
     aria-label="@Label">

    <!-- Label with error color -->
    <label for="@Id"
           class="@(HasError ? "master-label--error" : "")">
        @Label
    </label>

    <!-- Select dropdown with error styling -->
    <select id="@Id"
            class="master-select @(HasError ? "master-select--error" : "")"
            value="@Value"
            @onchange="OnChange"
            disabled="@Disabled"
            aria-label="@Label"
            aria-invalid="@HasError">
        @if (!string.IsNullOrEmpty(Placeholder))
        {
            <option value="" disabled>@Placeholder</option>
        }
        @foreach (var option in Options)
        {
            <option value="@option.Value">@option.Text</option>
        }
    </select>

    <!-- Dropdown arrow -->
    <div class="master-select-arrow">
        <svg width="12" height="8" viewBox="0 0 12 8" fill="none">
            <path d="M1 1L6 7L11 1" stroke="#003663" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
    </div>
</div>
@code {
    [Parameter] public string? Label { get; set; }
    [Parameter] public string Id { get; set; } = Guid.NewGuid().ToString();
    [Parameter] public string? Value { get; set; }
    [Parameter] public EventCallback<string> ValueChanged { get; set; }
    [Parameter] public List<SelectOption> Options { get; set; } = new();
    [Parameter] public string? Placeholder { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool Visible { get; set; }
    [Parameter] public Expression<Func<string>>? For { get; set; }
    [CascadingParameter] private EditContext? EditContext { get; set; }
    private bool HasError =>
        Visible &&
        EditContext is not null &&
        For is not null &&
        EditContext.GetValidationMessages(FieldIdentifier.Create(For)).Any();
    private async Task OnChange(ChangeEventArgs e)
    {
        Value = e.Value?.ToString();
        await ValueChanged.InvokeAsync(Value);
    }
}
```

### 5.11 `Features\EmployerRegistration\Components\OutlinedSelectField.razor.css`

```css
.master-select-field {
    position: relative;
    width: 100%;
    max-width: 330px;
}
/* Select */
.master-select {
    width: 100%;
    height: 56px;
    padding-left: 16px;
    padding-right: 40px;
    font-size: 16px;
    font-family: inherit;
    background-color: #ffffff;
    border: 1px solid #adadad;
    border-radius: 4px;
    outline: none;
    box-sizing: border-box;
    appearance: none;
    -webkit-appearance: none;
    cursor: pointer;
}
    /* Focus state */
    .master-select:focus {
        border-color: #003663;
    }
/* Label on border */
.master-select-field label {
    position: absolute;
    top: -8px;
    left: 12px;
    padding: 0 6px;
    background: #ffffff;
    font-size: 12px;
    color: #333;
    font-family: Verdana;
    font-weight: 400;
    font-size: 12px;
    line-height: 150%;
    letter-spacing: 0%;
    z-index: 1;
}
/* Disabled */
.master-select:disabled {
    background-color: #f5f5f5;
    color: #777;
}
/* Arrow icon */
.master-select-arrow {
    position: absolute;
    right: 14px;
    top: 50%;
    transform: translateY(-50%);
    pointer-events: none;
    display: flex;
    align-items: center;
}

/* Error state styles */
.master-select--error {
    border-color: #C62828 !important;
}
.master-label--error {
    color: #C62828 !important;
}
.master-select-field--error {
    border-color: #C62828 !important;
}
    .master-select-field--error label {
        color: #C62828;
    }
```

### 5.12 `Features\EmployerRegistration\Components\FieldError.razor`

```razor
@using Microsoft.AspNetCore.Components.Forms
@using System.Linq.Expressions
<!-- Inline field error message -->
@if (Visible && EditContext is not null && For is not null)
{
    @foreach (var message in EditContext.GetValidationMessages(FieldIdentifier.Create(For)))
    {
        <div class="field-error">@message</div>
    }
}
@code {
    [Parameter] public Expression<Func<string>>? For { get; set; }
    [Parameter] public bool Visible { get; set; }
    [CascadingParameter] private EditContext? EditContext { get; set; }
}
```

### 5.13 `Features\EmployerRegistration\Components\FieldError.razor.css`

```css
.field-error {
    font-family: Verdana, sans-serif;
    font-size: 12px;
    color: #C62828;
    margin-top: 4px;
    padding-left: 12px;
}
```

### 5.14 `Features\EmployerRegistration\Components\SelectOption.cs`

```csharp
namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Components;

public class SelectOption
{
    public string Value { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
```

### 5.15 `Features\EmployerRegistration\Models\BusinessInformationModel.cs`

```csharp
using System.ComponentModel.DataAnnotations;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

public class BusinessInformationModel
{
    // Business Details
    [Required(ErrorMessage = "FEIN is required.")]
    public string? FEIN { get; set; }

    [Required(ErrorMessage = "Legal Name is required.")]
    public string? LegalName { get; set; }

    public string? TradeName { get; set; }

    [Required(ErrorMessage = "Phone Number is required.")]
    public string? PhoneNumber { get; set; }

    [Required(ErrorMessage = "Email Address is required.")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
    public string? Email { get; set; }

    // Mailing Address
    [Required(ErrorMessage = "Country is required.")]
    public string? MailingCountry { get; set; } = "United States";

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? MailingAddressLine1 { get; set; }

    public string? MailingAddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? MailingCity { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public string? MailingState { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    public string? MailingZip { get; set; }

    public string? MailingExtension { get; set; }

    // Physical Location 1
    [Required(ErrorMessage = "Country is required.")]
    public string? PhysicalCountry { get; set; } = "United States";

    [Required(ErrorMessage = "Address Line 1 is required.")]
    public string? PhysicalAddressLine1 { get; set; }

    public string? PhysicalAddressLine2 { get; set; }

    [Required(ErrorMessage = "City is required.")]
    public string? PhysicalCity { get; set; }

    [Required(ErrorMessage = "State is required.")]
    public string? PhysicalState { get; set; }

    [Required(ErrorMessage = "Zip Code is required.")]
    public string? PhysicalZip { get; set; }

    public string? PhysicalExtension { get; set; }
}
```

### 5.16 `Features\EmployerRegistration\BusinessInformation.razor`

```razor
@page "/business-information"
@rendermode InteractiveServer
@using Microsoft.AspNetCore.Components.Forms
@using UI.EmployerPortal.Web.Features.EmployerRegistration.Components
@using UI.EmployerPortal.Web.Features.EmployerRegistration.Models
<PageTitle>Business Information</PageTitle>

<div class="page_wrapper">
    <div class="bi-page-content">

        <h1 class="page-title">Business Information</h1>
        <p class="page-subtitle">All fields are required unless noted</p>

        <EditForm EditContext="@editContext" OnValidSubmit="GoNext" OnInvalidSubmit="OnInvalid" FormName="BusinessInformation">
            <DataAnnotationsValidator />

            @* Section 1: Business Details *@
            <div class="bi-fields-grid">
                <div class="bi-field">
                    <OutlinedTextField Label="FEIN"
                                       @bind-Value="Model.FEIN"
                                       For="() => Model.FEIN"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.FEIN)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Legal Name"
                                       @bind-Value="Model.LegalName"
                                       For="() => Model.LegalName"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.LegalName)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Trade Name (Optional)"
                                       @bind-Value="Model.TradeName" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Phone Number"
                                       Type="tel"
                                       @bind-Value="Model.PhoneNumber"
                                       For="() => Model.PhoneNumber"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.PhoneNumber)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Email Address"
                                       Type="email"
                                       @bind-Value="Model.Email"
                                       For="() => Model.Email"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.Email)" Visible="formSubmitted" />
                </div>
            </div>

            @* Section 2: Business Mailing Address *@
            <div class="bi-section-header">
                <span>Business Mailing Address</span>
            </div>

            <div class="bi-fields-grid">
                <div class="bi-field">
                    <OutlinedSelectField Label="Country"
                                         @bind-Value="Model.MailingCountry"
                                         Options="Countries"
                                         For="() => Model.MailingCountry"
                                         Visible="formSubmitted" />
                    <FieldError For="@(() => Model.MailingCountry)" Visible="formSubmitted" />
                </div>

                <div class="bi-field"></div>

                <div class="bi-field">
                    <OutlinedTextField Label="Address Line 1"
                                       @bind-Value="Model.MailingAddressLine1"
                                       For="() => Model.MailingAddressLine1"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.MailingAddressLine1)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Address Line 2 (Optional)"
                                       @bind-Value="Model.MailingAddressLine2" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="City"
                                       @bind-Value="Model.MailingCity"
                                       For="() => Model.MailingCity"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.MailingCity)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedSelectField Label="State"
                                         @bind-Value="Model.MailingState"
                                         Options="States"
                                         For="() => Model.MailingState"
                                         Visible="formSubmitted" />
                    <FieldError For="@(() => Model.MailingState)" Visible="formSubmitted" />
                </div>

                <div class="bi-zip-row">
                    <div class="bi-zip-field">
                        <OutlinedTextField Label="Zip Code"
                                           @bind-Value="Model.MailingZip"
                                           For="() => Model.MailingZip"
                                           Visible="formSubmitted" />
                        <FieldError For="@(() => Model.MailingZip)" Visible="formSubmitted" />
                    </div>
                    <span class="bi-zip-dash">—</span>
                    <div class="bi-ext-field">
                        <OutlinedTextField Label="Ext. (Optional)"
                                           @bind-Value="Model.MailingExtension" />
                    </div>
                </div>
            </div>

            @* Section 3: Physical Location 1 *@
            <div class="bi-section-header">
                <span>Physical Location 1</span>
            </div>

            <div class="bi-fields-grid">
                <div class="bi-field">
                    <OutlinedSelectField Label="Country"
                                         @bind-Value="Model.PhysicalCountry"
                                         Options="Countries"
                                         For="() => Model.PhysicalCountry"
                                         Visible="formSubmitted" />
                    <FieldError For="@(() => Model.PhysicalCountry)" Visible="formSubmitted" />
                </div>

                <div class="bi-field"></div>

                <div class="bi-field">
                    <OutlinedTextField Label="Address Line 1"
                                       @bind-Value="Model.PhysicalAddressLine1"
                                       For="() => Model.PhysicalAddressLine1"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.PhysicalAddressLine1)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="Address Line 2 (Optional)"
                                       @bind-Value="Model.PhysicalAddressLine2" />
                </div>

                <div class="bi-field">
                    <OutlinedTextField Label="City"
                                       @bind-Value="Model.PhysicalCity"
                                       For="() => Model.PhysicalCity"
                                       Visible="formSubmitted" />
                    <FieldError For="@(() => Model.PhysicalCity)" Visible="formSubmitted" />
                </div>

                <div class="bi-field">
                    <OutlinedSelectField Label="State"
                                         @bind-Value="Model.PhysicalState"
                                         Options="States"
                                         For="() => Model.PhysicalState"
                                         Visible="formSubmitted" />
                    <FieldError For="@(() => Model.PhysicalState)" Visible="formSubmitted" />
                </div>

                <div class="bi-zip-row">
                    <div class="bi-zip-field">
                        <OutlinedTextField Label="Zip Code"
                                           @bind-Value="Model.PhysicalZip"
                                           For="() => Model.PhysicalZip"
                                           Visible="formSubmitted" />
                        <FieldError For="@(() => Model.PhysicalZip)" Visible="formSubmitted" />
                    </div>
                    <span class="bi-zip-dash">—</span>
                    <div class="bi-ext-field">
                        <OutlinedTextField Label="Ext. (Optional)"
                                           @bind-Value="Model.PhysicalExtension" />
                    </div>
                </div>
            </div>

            @* Add Another Physical Location *@
            <button type="button" class="bi-add-location" @onclick="AddPhysicalLocation">
                <svg width="16" height="16" viewBox="0 0 16 16" fill="none">
                    <path d="M8 3V13M3 8H13" stroke="#0D6EFD" stroke-width="2" stroke-linecap="round" />
                </svg>
                Add Another Physical Location
            </button>

            @* Action Buttons *@
            <div class="bi-button-row">
                <div class="bi-button-row-left">
                    <button type="button" class="btn btn--secondary" @onclick="GoBack">
                        <svg width="8" height="13" viewBox="0 0 8 13" fill="none">
                            <path d="M7 1L1 6.5L7 12" stroke="#003663" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                        BACK
                    </button>
                </div>
                <div class="bi-button-row-right">
                    <button type="button" class="btn btn--tertiary" @onclick="HandleSaveQuit">
                        SAVE &amp; QUIT
                    </button>
                    <button type="submit" class="btn btn--primary">
                        CONTINUE
                        <svg width="8" height="13" viewBox="0 0 8 13" fill="none">
                            <path d="M1 1L7 6.5L1 12" stroke="white" stroke-width="2" stroke-linecap="round" stroke-linejoin="round" />
                        </svg>
                    </button>
                </div>
            </div>
        </EditForm>

    </div>
</div>
```

### 5.17 `Features\EmployerRegistration\BusinessInformation.razor.cs`

```csharp
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Components;
using UI.EmployerPortal.Web.Features.EmployerRegistration.Models;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration;

public partial class BusinessInformation
{
    [Inject] private NavigationManager Nav { get; set; } = default!;

    private BusinessInformationModel Model = new();
    private EditContext editContext = default!;

    private bool formSubmitted = false;

    protected List<SelectOption> Countries { get; set; } = new()
    {
        new SelectOption { Value = "United States", Text = "United States" },
        new SelectOption { Value = "Canada", Text = "Canada" },
        new SelectOption { Value = "Mexico", Text = "Mexico" }
    };

    protected List<SelectOption> States { get; set; } = new()
    {
        new SelectOption { Value = "AL", Text = "Alabama" },
        new SelectOption { Value = "AK", Text = "Alaska" },
        new SelectOption { Value = "AZ", Text = "Arizona" },
        new SelectOption { Value = "AR", Text = "Arkansas" },
        new SelectOption { Value = "CA", Text = "California" },
        new SelectOption { Value = "CO", Text = "Colorado" },
        new SelectOption { Value = "CT", Text = "Connecticut" },
        new SelectOption { Value = "DE", Text = "Delaware" },
        new SelectOption { Value = "FL", Text = "Florida" },
        new SelectOption { Value = "GA", Text = "Georgia" },
        new SelectOption { Value = "HI", Text = "Hawaii" },
        new SelectOption { Value = "ID", Text = "Idaho" },
        new SelectOption { Value = "IL", Text = "Illinois" },
        new SelectOption { Value = "IN", Text = "Indiana" },
        new SelectOption { Value = "IA", Text = "Iowa" },
        new SelectOption { Value = "KS", Text = "Kansas" },
        new SelectOption { Value = "KY", Text = "Kentucky" },
        new SelectOption { Value = "LA", Text = "Louisiana" },
        new SelectOption { Value = "ME", Text = "Maine" },
        new SelectOption { Value = "MD", Text = "Maryland" },
        new SelectOption { Value = "MA", Text = "Massachusetts" },
        new SelectOption { Value = "MI", Text = "Michigan" },
        new SelectOption { Value = "MN", Text = "Minnesota" },
        new SelectOption { Value = "MS", Text = "Mississippi" },
        new SelectOption { Value = "MO", Text = "Missouri" },
        new SelectOption { Value = "MT", Text = "Montana" },
        new SelectOption { Value = "NE", Text = "Nebraska" },
        new SelectOption { Value = "NV", Text = "Nevada" },
        new SelectOption { Value = "NH", Text = "New Hampshire" },
        new SelectOption { Value = "NJ", Text = "New Jersey" },
        new SelectOption { Value = "NM", Text = "New Mexico" },
        new SelectOption { Value = "NY", Text = "New York" },
        new SelectOption { Value = "NC", Text = "North Carolina" },
        new SelectOption { Value = "ND", Text = "North Dakota" },
        new SelectOption { Value = "OH", Text = "Ohio" },
        new SelectOption { Value = "OK", Text = "Oklahoma" },
        new SelectOption { Value = "OR", Text = "Oregon" },
        new SelectOption { Value = "PA", Text = "Pennsylvania" },
        new SelectOption { Value = "RI", Text = "Rhode Island" },
        new SelectOption { Value = "SC", Text = "South Carolina" },
        new SelectOption { Value = "SD", Text = "South Dakota" },
        new SelectOption { Value = "TN", Text = "Tennessee" },
        new SelectOption { Value = "TX", Text = "Texas" },
        new SelectOption { Value = "UT", Text = "Utah" },
        new SelectOption { Value = "VT", Text = "Vermont" },
        new SelectOption { Value = "VA", Text = "Virginia" },
        new SelectOption { Value = "WA", Text = "Washington" },
        new SelectOption { Value = "WV", Text = "West Virginia" },
        new SelectOption { Value = "WI", Text = "Wisconsin" },
        new SelectOption { Value = "WY", Text = "Wyoming" }
    };

    protected override void OnInitialized()
    {
        editContext = new EditContext(Model);

        editContext.OnFieldChanged += (_, __) =>
        {
            StateHasChanged();
        };
    }

    private void GoBack()
    {
        Nav.NavigateTo("/ownership");
    }

    private void HandleSaveQuit()
    {
        Nav.NavigateTo("/dashboard");
    }

    private void GoNext()
    {
        Nav.NavigateTo("/address-correction");
    }

    private void OnInvalid()
    {
        formSubmitted = true;
        StateHasChanged();
    }

    private void AddPhysicalLocation()
    {
        // TODO: Add additional physical location support
    }
}
```

### 5.18 `Features\EmployerRegistration\BusinessInformation.razor.css`

```css
.page_wrapper {
    display: flex;
    justify-content: center;
}

.bi-page-content {
    display: flex;
    flex-direction: column;
    width: 684px;
    max-width: 100%;
    padding-top: 32px;
    padding-bottom: 32px;
    box-sizing: border-box;
    font-family: Verdana, sans-serif;
    font-size: 16px;
    gap: 24px;
}

.page-title {
    font-family: Verdana, sans-serif;
    font-size: 32px;
    font-weight: 700;
    line-height: 116%;
    color: #000000;
    margin: 0;
}

.page-subtitle {
    font-family: Verdana, sans-serif;
    font-size: 16px;
    font-weight: 400;
    line-height: 25.6px;
    color: #000000;
    margin: 0;
}

/* Section header */
.bi-section-header {
    width: 100%;
    border-bottom: 1px solid #003663;
    padding-bottom: 6px;
    margin-top: 8px;
}

.bi-section-header span {
    font-family: Verdana, sans-serif;
    font-size: 18px;
    font-weight: 700;
    line-height: 28.8px;
    color: #003663;
}

/* Form - EditForm renders as <form> */
::deep form {
    display: flex;
    flex-direction: column;
    gap: 24px;
}

/* 2-column grid — 330px per column, 24px gap = 684px */
.bi-fields-grid {
    display: grid;
    grid-template-columns: 330px 330px;
    gap: 24px;
    width: 684px;
}

.bi-field {
    display: flex;
    flex-direction: column;
}

/* Zip + Extension row */
.bi-zip-row {
    display: flex;
    align-items: flex-start;
    gap: 8px;
    grid-column: span 1;
}

.bi-zip-field {
    flex: 0 0 140px;
}

.bi-ext-field {
    flex: 1;
}

.bi-zip-dash {
    font-family: Verdana, sans-serif;
    font-size: 16px;
    font-weight: 700;
    color: #424242;
    flex-shrink: 0;
    padding-top: 16px;
}

/* Add another location link */
.bi-add-location {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    background: none;
    border: none;
    padding: 4px 8px;
    cursor: pointer;
    font-family: Verdana, sans-serif;
    font-size: 16px;
    font-weight: 700;
    color: #0D6EFD;
    border-radius: 6px;
}

.bi-add-location:hover {
    text-decoration: underline;
}

/* Button row */
.bi-button-row {
    display: flex;
    justify-content: space-between;
    align-items: center;
    gap: 8px;
    padding-top: 8px;
    width: 684px;
}

.bi-button-row-left {
    display: flex;
    align-items: center;
}

.bi-button-row-right {
    display: flex;
    align-items: center;
    gap: 8px;
}

/* Buttons */
.btn {
    display: inline-flex;
    align-items: center;
    gap: 8px;
    padding: 10px 16px;
    border-radius: 6px;
    font-family: Verdana, sans-serif;
    font-size: 16px;
    font-weight: 700;
    line-height: 25.6px;
    text-transform: uppercase;
    cursor: pointer;
    border: none;
}

.btn--primary {
    background: #003663;
    color: #ffffff;
    outline: 2px solid #003663;
}

.btn--primary:hover {
    background: #002a4d;
}

.btn--secondary {
    background: #ffffff;
    color: #003663;
    outline: 2px solid #003663;
    outline-offset: -2px;
}

.btn--secondary:hover {
    background: #f0f4f8;
}

.btn--tertiary {
    background: transparent;
    color: #003663;
    outline: none;
}

.btn--tertiary:hover {
    text-decoration: underline;
}

@media (max-width: 575px) {
    .bi-page-content {
        padding: 16px;
        align-items: flex-start;
    }

    .bi-fields-grid {
        grid-template-columns: 1fr;
        width: 100%;
    }

    .bi-button-row {
        width: 100%;
    }
}
```

### 5.19 `.gitignore`

```
## .NET / Blazor gitignore

# Build output
bin/
obj/

# User-specific files
*.user
*.suo
.vs/
*.rsuser

# Visual Studio settings
.vscode/
*.launch

# NuGet
*.nupkg
*.snupkg
packages/
project.lock.json
project.fragment.lock.json
artifacts/

# Logs
*.log
*.trace

# OS files
.DS_Store
Thumbs.db
```

---

## 6. Git Status (Uncommitted Changes)

The last git push was BEFORE the major refactoring. These changes need to be committed and pushed:

**Deleted:**
- `Components/Shared/SelectField.razor` + `.cs` + `.css`
- `Components/Shared/SelectOption.cs`
- `Components/Shared/TextField.razor` + `.cs` + `.css`

**Modified:**
- `Components/_Imports.razor` (removed old Shared references)
- `Features/EmployerRegistration/BusinessInformation.razor` (rewritten to use EditForm + OutlinedTextField)
- `Features/EmployerRegistration/BusinessInformation.razor.cs` (rewritten with EditContext pattern)
- `Features/EmployerRegistration/BusinessInformation.razor.css` (rewritten with page_wrapper pattern)
- `Features/EmployerRegistration/Models/BusinessInformationModel.cs` (rewritten with DataAnnotations on nullable string?)
- `Features/_Imports.razor` (added component/model usings)
- `UI.EmployerPortal.Web.csproj` (excludes test file\ from build)

**New (untracked):**
- `Features/EmployerRegistration/Components/` (OutlinedTextField, OutlinedSelectField, FieldError, SelectOption)
- `UI.EmployerPortal.Web.sln`
- `test file/` (reference only — should NOT be committed)

---

## 7. Issues Encountered & Fixes (Reference)

| Issue | Fix |
|-------|-----|
| `dotnet` not recognized | Install .NET 8 SDK separately from Microsoft website (VS Installer only installs runtime) |
| Handlebars comments `{{!-- --}}` | Use Razor comments `@* *@` instead |
| Font mismatch (Helvetica) | Changed `app.css` to `Verdana, Geneva, sans-serif` |
| `InteractiveServer` not recognized in Features folder | Created `Features/_Imports.razor` with `@using static Microsoft.AspNetCore.Components.Web.RenderMode` |
| Validation not working | Added `.AddInteractiveServerComponents()` to `Program.cs` + `@rendermode InteractiveServer` to page |
| File lock during build (MSB3027) | Stop app in Visual Studio before rebuilding |
| Duplicate `Nav` injection | Removed `@inject` from `.razor`, kept `[Inject]` in `.razor.cs` |
| `SelectOption` not found | Moved from `@code {}` block to separate `SelectOption.cs` with proper namespace |
| `test file/` compilation errors | Excluded from build in `.csproj` with `<Compile Remove="test file\**" />` |

---

## 8. User Preferences

- Match design exactly (Figma specifications)
- Use `OutlinedTextField` (not `TextField` or `FormTextField`)
- Section headers are inline HTML (not shared components)
- Inline errors only (Option B — no validation summary box)
- Code-behind for pages, inline `@code {}` for components
- Font: Verdana everywhere
- `.NET 8` (not .NET 10)
- Code should be easy to move into main application

---

## 9. Pending / Future Work

- **Commit & push** refactored code to GitHub (current code on GitHub is outdated)
- **Browser testing** — stop app in VS, rebuild, run, verify UI + validation at `/business-information`
- **"Add Another Physical Location"** — currently a TODO stub in code-behind
- **WCF/Web service integration** — explicitly deferred by user ("later")
- **Orchestrator pattern** — reference shows `EmployerRegOrchestrator` with `ISessionManager`; not yet implemented for Business Information step

---

## 10. Reference Files Location

Team's Step 1 reference files are at `D:\UI.EmployerPortal.Web\test file\`:
- `Components\OutlinedTextField.razor` + `.css` — the team's text field component
- `Model\EmployerRegOrchestrator.cs` — orchestrator pattern with `IEmployerRegOrchestrator` interface
- `Model\RegistrationNumberSelection.cs` — session model using `ISessionModel` record
- `Pages\EmployerRegEntryPoint.razor` + `.css` — Step 1 page using EditForm pattern
- `Pages\EmployerRegOrchestrator.cs` — duplicate orchestrator

These are **read-only reference** — excluded from build, should not be committed.
