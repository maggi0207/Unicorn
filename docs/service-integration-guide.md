# WCF Service Integration Guide

Reference for integrating any WCF/SOAP service into the Employer Portal following the same pattern used for `AddressValidationService`.

---

## Folder Structure

```
src/UI.EmployerPortal.Web/
├── Features/EmployerRegistration/
│   └── Services/
│       ├── IAddressValidationWrapper.cs   ← interface + result record
│       ├── AddressValidationService.cs    ← WCF wrapper implementation
│       └── RegistrationStateService.cs    ← scoped navigation state
├── Program.cs                             ← DI registration
└── _Imports.razor                         ← global @using

generated/UI.EmployerPortal.Generated.ServiceClients/
└── AddressValidationService/
    └── Reference.cs                       ← auto-generated WCF proxy
```

---

## Step-by-Step: Add a New WCF Service

### 1. Locate the Generated Client

Generated proxies live in:
```
generated/UI.EmployerPortal.Generated.ServiceClients/<ServiceName>/Reference.cs
```
Key things to check in `Reference.cs`:
- Interface name: `IXxxService`
- Method signatures and parameter types
- Response type name (e.g. `ValidateAddressResponse` — not always obvious from the method name)
- Enum values and their exact string names

### 2. Create the Wrapper Interface

File: `Features/EmployerRegistration/Services/IXxxWrapper.cs`

```csharp
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Abstraction over the SOAP XxxService.
/// Allows the real WCF implementation to be swapped for a stub in tests.
/// </summary>
public interface IXxxWrapper
{
    /// <summary>Calls the service and returns a strongly-typed result.</summary>
    Task<XxxResult> DoSomethingAsync(SomeInputModel input);
}

/// <summary>Result returned by <see cref="IXxxWrapper.DoSomethingAsync"/>.</summary>
public record XxxResult(bool IsSuccess, string? ErrorMessage, SomeOutputModel? Data);
```

### 3. Implement the Wrapper

File: `Features/EmployerRegistration/Services/XxxService.cs`

```csharp
using System.ServiceModel;
using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.XxxService;

namespace UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

/// <summary>
/// Wraps the generated WCF <see cref="GeneratedClient.IXxxService"/> client.
/// </summary>
public class XxxService : IXxxWrapper
{
    private readonly GeneratedClient.IXxxService _client;

    public XxxService(GeneratedClient.IXxxService client)
    {
        _client = client;
    }

    /// <inheritdoc />
    public async Task<XxxResult> DoSomethingAsync(SomeInputModel input)
    {
        var request = new GeneratedClient.XxxRequest
        {
            // map input fields
        };

        GeneratedClient.XxxResponse response;
        try
        {
            response = await _client.DoSomethingAsync(request);
        }
        catch (CommunicationException)
        {
            // WCF communication failure (network error, SOAP fault, serialization failure)
            return new XxxResult(false, "Service is temporarily unavailable. Please try again.", null);
        }
        catch (Exception)
        {
            // Unexpected failure — fail safe
            return new XxxResult(false, "Service is temporarily unavailable. Please try again.", null);
        }

        // map response fields to output model
        return new XxxResult(true, null, /* mapped output */);
    }
}
```

### 4. Register in DI

File: `Program.cs`

```csharp
using UI.EmployerPortal.Web.Features.EmployerRegistration.Services;

builder.Services.AddWcfServiceClients(builder.Configuration); // already present
builder.Services.AddScoped<IXxxWrapper, XxxService>();
```

> Use `AddScoped` for stateful or per-request services.
> Use `AddTransient` for purely stateless API callers with no shared state.

### 5. Add Global Using

File: `_Imports.razor`

```razor
@using UI.EmployerPortal.Web.Features.EmployerRegistration.Services
```
(already present — new services in the same namespace are automatically available in all Razor files)

### 6. Inject and Use in a Page

```razor
@inject IXxxWrapper XxxService

@code {
    private async Task HandleContinue()
    {
        var result = await XxxService.DoSomethingAsync(Model.SomeInput);
        if (!result.IsSuccess)
        {
            // show error
        }
    }
}
```

---

## Key Gotchas Learned from AddressValidationService

| Issue | Fix |
|---|---|
| `CountryCode` must be ISO code (`"US"`), not display name (`"United States"`) | Add a `ToCountryCode()` mapping method |
| `AddressRequestType` must be `Employer`, not `Claimant` | Confirmed via WCF Test Client; `Claimant` returns `OutputAddress = null` |
| `IsValid` — `ReturnCode` is always null | Use `string.IsNullOrEmpty(response.ErrorMessageOne)` instead |
| `OutputAddress.LineOneAddress` can be empty | Service sometimes puts street in `LineTwoAddress`; swap logic required |
| `OutputAddress.CountryCode` is null | Fall back to input address country |
| Text in `ZipCodeExtension` causes SOAP deserialization fault | Strip non-digits in UI with `InputMode="numeric"` before sending |
| Response type name differs from method name | Check `Reference.cs` — e.g. method is `ValidateAddressAsync` but response type is `ValidateAddressResponse` |

---

## Error Handling Pattern (from Original App)

Always use two catch blocks — matching the pattern used in other portal services:

```csharp
catch (CommunicationException)
{
    // WCF communication failure (SOAP fault, network error, serialization failure)
    return /* safe fallback result */;
}
catch (Exception)
{
    // Unexpected failure — prevent page crash
    return /* safe fallback result */;
}
```

---

## Navigation State Between Pages

Use `RegistrationStateService` (scoped) to carry data between wizard pages:

```csharp
public class RegistrationStateService
{
    public BusinessInformationModel? BusinessInfo { get; set; }
    public List<AddressCorrectionItem> AddressCorrections { get; set; } = new();
    public int CorrectionIndex { get; set; }
}
```

- Inject in pages via `[Inject] private RegistrationStateService RegistrationState { get; set; }`
- Set state before navigating: `RegistrationState.BusinessInfo = Model;`
- Restore state on `OnInitialized`: `if (RegistrationState.BusinessInfo is not null) Model = RegistrationState.BusinessInfo;`

---

## WCF Test Client — Verifying Service Behavior

Use the WCF Test Client (`WcfTestClient.exe`) to test service calls before coding:

1. Open WCF Test Client
2. Add the service WSDL endpoint
3. Locate `IAddressValidationService` → `ValidateAddress()`
4. Set `AddressRequestType = Employer` (not `Claimant`)
5. Fill in address fields with a real US address
6. Invoke and inspect `OutputAddress`, `ErrorMessageOne`, `ReturnCode`

This confirms field names, enum values, and response structure before writing any code.
