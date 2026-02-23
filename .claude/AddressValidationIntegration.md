# Address Validation Service — Integration Guide
> How to merge the Unicorn repo work into the main original project.

## What Already Exists in the Original Project
- ✅ `WcfServiceClients/DependencyInjection.cs` (with `AddWcfServiceClients`)
- ✅ Generated `IAddressValidationService` (SOAP client) registered as `Transient`
- ✅ `EmployerRegistrationInfo` scoped service (state carrier)
- ✅ `BusinessInformation.razor` and `BusinessContact.razor` (existing pages)

---

## Files to ADD

### 1. `Services/IAddressValidationWrapper.cs` — NEW
```csharp
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public interface IAddressValidationWrapper
{
    Task<AddressValidationResult> ValidateAsync(AddressModel address);
}

public record AddressValidationResult(bool IsValid, string? ErrorMessage, AddressModel? CorrectedAddress);
```

### 2. `Services/AddressValidationWrapper.cs` — NEW
```csharp
using GeneratedClient = UI.EmployerPortal.Generated.ServiceClients.AddressValidationService;
using UI.EmployerPortal.Razor.SharedComponents.Model;

namespace UI.EmployerPortal.Web.Services;

public class AddressValidationWrapper : IAddressValidationWrapper
{
    private readonly GeneratedClient.IAddressValidationService _client;

    public AddressValidationWrapper(GeneratedClient.IAddressValidationService client)
    {
        _client = client;
    }

    public async Task<AddressValidationResult> ValidateAsync(AddressModel address)
    {
        var request = new GeneratedClient.AddressProxy
        {
            AddressRequestType = GeneratedClient.AddressRequestTypeEnum.Employer,
            LineOneAddress     = address.AddressLine1,
            LineTwoAddress     = address.AddressLine2,
            CityName           = address.City,
            StateCode          = address.State,
            ZipCode            = address.Zip,
            ZipCodeExtension   = address.Extension,
            CountryCode        = address.Country
        };

        var response = await _client.ValidateAddressAsync(request);

        var isValid = string.IsNullOrEmpty(response.ReturnCode) || response.ReturnCode == "0";

        var correctedAddress = response.OutputAddress is null ? null : new AddressModel
        {
            AddressLine1 = response.OutputAddress.LineOneAddress,
            AddressLine2 = response.OutputAddress.LineTwoAddress,
            City         = response.OutputAddress.CityName,
            State        = response.OutputAddress.StateCode,
            Zip          = response.OutputAddress.ZipCode,
            Extension    = response.OutputAddress.ZipCodeExtension,
            Country      = response.OutputAddress.CountryCode
        };

        return new AddressValidationResult(
            isValid,
            isValid ? null : response.ErrorMessageOne ?? response.ErrorMessageTwo,
            correctedAddress);
    }
}
```

### 3. `Features/EmployerRegistration/AddressCorrection.razor` — NEW (placeholder)
```razor
@page "/employer-registration/address-correction"
@rendermode InteractiveServer

<PageTitle>Address Correction</PageTitle>

<div class="page_wrapper">
    <div class="bi-page-content">
        <h1 class="page-title">Address Correction</h1>
        <p class="page-subtitle">Coming soon</p>
    </div>
</div>
```

---

## Files to EDIT

### 4. `WcfServiceClients/DependencyInjection.cs` — add 1 line inside `AddWcfServiceClients()`
```csharp
services.AddScoped<IAddressValidationWrapper, AddressValidationWrapper>();
```

### 5. `EmployerRegistrationInfo.cs` — add property + record
```csharp
// Add property to existing EmployerRegistrationInfo class:
public List<AddressCorrectionItem> AddressCorrections { get; set; } = new();

// Add this record (same file or new file):
public record AddressCorrectionItem(
    string Label,
    AddressModel Original,
    AddressModel? Suggested
);
```

### 6. `BusinessInformation.razor` — add inject + replace HandleContinue

Add at top:
```razor
@inject IAddressValidationWrapper AddressValidator
```

Replace `HandleContinue()` with:
```csharp
private async Task HandleContinue()
{
    formSubmitted = true;
    customValidator?.ClearErrors();

    // Step 1: Local validation — required fields + format
    var isTopLevelValid = editContext.Validate();
    var addressErrors = ValidateAddressModels();
    if (addressErrors.Any()) { customValidator?.DisplayErrors(addressErrors); isTopLevelValid = false; }

    hasValidationErrors = !isTopLevelValid;
    if (!isTopLevelValid) { StateHasChanged(); return; }

    // Step 2: Service validation — only runs when local validation passed
    var corrections = new List<AddressCorrectionItem>();

    var mailingResult = await AddressValidator.ValidateAsync(Model.MailingAddress);
    if (!mailingResult.IsValid)
        corrections.Add(new AddressCorrectionItem("Business Mailing Address", Model.MailingAddress, mailingResult.CorrectedAddress));

    for (var i = 0; i < Model.PhysicalLocations.Count; i++)
    {
        var result = await AddressValidator.ValidateAsync(Model.PhysicalLocations[i]);
        if (!result.IsValid)
            corrections.Add(new AddressCorrectionItem($"Physical Location {i + 1}", Model.PhysicalLocations[i], result.CorrectedAddress));
    }

    if (corrections.Any())
    {
        RegistrationState.AddressCorrections = corrections;   // RegistrationState = EmployerRegistrationInfo
        Nav.NavigateTo("/employer-registration/address-correction");
    }
    else
    {
        Nav.NavigateTo("/employer-registration/business-contact");
    }
}
```

### 7. `BusinessContact.razor` — add inject + update HandleContinue

Add at top:
```razor
@inject IAddressValidationWrapper AddressValidator
```

In `HandleContinue()`, add service call when `IsDifferentAddress == true` and local validation passes:
```csharp
if (Model.IsDifferentAddress == true)
{
    var addressErrors = ValidateContactAddress();

    if (!addressErrors.Any())
    {
        var result = await AddressValidator.ValidateAsync(Model.ContactAddress);
        if (!result.IsValid)
        {
            var fi = new FieldIdentifier(Model.ContactAddress, nameof(Model.ContactAddress.State));
            addressErrors[fi] = new List<string>
            {
                result.ErrorMessage ?? "Address could not be validated. Please check the state and country."
            };
        }
    }

    if (addressErrors.Any()) { customValidator?.DisplayErrors(addressErrors); isValid = false; }
}
```

---

## Summary Table

| Action | File |
|--------|------|
| **New** | `Services/IAddressValidationWrapper.cs` |
| **New** | `Services/AddressValidationWrapper.cs` |
| **New** | `Features/EmployerRegistration/AddressCorrection.razor` |
| **Edit** | `WcfServiceClients/DependencyInjection.cs` — add 1 line |
| **Edit** | `EmployerRegistrationInfo.cs` — add `AddressCorrections` property + `AddressCorrectionItem` record |
| **Edit** | `BusinessInformation.razor` — async `HandleContinue` with service call |
| **Edit** | `BusinessContact.razor` — async `HandleContinue` with service call |

## Validation Flow
```
Click CONTINUE
    ↓
Local DataAnnotations validation (required fields, format)
    ↓ fail → show errors on page, stop
    ↓ pass
Call AddressValidationService (SOAP/WCF)
    ↓ any address invalid → navigate to /address-correction
    ↓ all valid → navigate to /business-contact
```
