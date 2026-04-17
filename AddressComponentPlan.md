# Implementing Country-Specific Phone Layouts in Address Component

This document outlines the proposed changes to match the three screenshots provided for the `AddressField` component.

## Background Context
Currently, the `AddressField.razor` component shows the **Phone Type** dropdown unconditionally for all countries when `ShowPhone` is enabled. Further, the Razor view references `Address.PhoneType` and `Address.PhoneExtension` which were missing from `AddressModel.cs` (which caused the 19 compilation errors in your project).

## Proposed Changes

### Address Model (`AddressModel.cs`)

> [!NOTE] 
> I have already applied the `PhoneType` and `PhoneExtension` fix to resolve the compilation errors so we can build successfully, but I want to explicitly document it along with the newly requested field separation.

- **[MODIFIED]** [AddressModel.cs](file:///d:/Unicorn-repo/src/UI.EmployerPortal.Razor.SharedComponents/Model/AddressModel.cs)
  - Added `public string? PhoneType { get; set; }` and `public string? PhoneExtension { get; set; }`.
  - Added `public string? Province { get; set; }` and `public string? PostalCode { get; set; }` as distinct properties for Canada.
  - Added `public string? AddressLine3 { get; set; }` and `public string? AddressLine4 { get; set; }` as distinct properties for Other International.
  - Added a custom validation attribute `RequiredIfCountryAttribute` to conditionally mandate fields based on the selected `Country`.
  - Swapped the unconditional `[Required]` attributes on `State` and `Zip` for `[RequiredIfCountry("United States")]`.
  - Placed `[RequiredIfCountry("Canada")]` on the new `Province` and `PostalCode` fields to ensure validation executes smoothly.
  - Placed `[RequiredIfCountry("Other International")]` on the new `AddressLine3` and `AddressLine4` fields.

### UI Component (`AddressField.razor`)

- **[MODIFIED]** [AddressField.razor](file:///d:/Unicorn-repo/src/UI.EmployerPortal.Razor.SharedComponents/Address/AddressField.razor)
  - Under the Canada block, dynamically point `@bind-Value` and `For` delegates to `Address.Province` and `Address.PostalCode` instead of reusing `Address.State` and `Address.Zip`.
  - In `OnCountryChanged`, add logic to clear `Province` and `PostalCode` similarly to how `Zip` and `State` are cleared.
  - Wrap the "Phone Type" dropdown with `@if (Address.Country != "United States")`.
  - Add `Id="PhoneType"` to the `OutlinedSelectField` for `PhoneType` so the `NotificationBanner` focus anchor links work when errors are thrown on submission.
  - Leave the `PhoneNumberField` (which contains Phone Number and Ext.) unconditionally visible inside the `@if (ShowPhone)` block.

### Business Details Validations (`BusinessInformation.razor`)

- **[MODIFIED]** [BusinessInformation.razor](file:///d:/Unicorn-repo/src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/BusinessInformation.razor)
  - Update `CopyMailingToPhysical`, `NotifyPhysicalLocationChanged`, and `ClearPhysicalLocation` to correctly include the new `Province` and `PostalCode` fields during physical address syncing.
  - **Phone Validations**: In the manual validation logic (`ValidateAddressModels`), enforce that `PhoneType` and `PhoneCountryCode` (International Area Code) are required if left empty. Ensure we append the `nameof(AddressModel.PhoneType)` and `nameof(AddressModel.PhoneCountryCode)` field identifiers for tab-focus targeting via the NotificationBanner.
  - **Phone Number Format**: Restrict the strict `999-999-9999` regex validation to "United States" and "Canada", allowing international numbers to be evaluated more generously since their length might differ.
  - **Skip WCF Validation**: Filter elements in `addressesToValidate` so and only United States addresses continue to the `AddressCoordinator.ValidateAndRedirectAsync()` call. Canada and Other International skip the WCF step.

### Business Contact (`BusinessContact.razor`)

- **[MODIFIED]** [BusinessContact.razor](file:///d:/Unicorn-repo/src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/BusinessContact.razor)
  - **Skip WCF Validation**: When `Validate()` evaluates `Model.ContactAddress`, skip the `AddressCoordinator` service check if `Model.ContactAddress.Country != "United States"`. 

### Backend Model State Store (`EmployerRegistrationModelStore.cs`)

- **[MODIFIED]** [EmployerRegistrationModelStore.cs](file:///d:/Unicorn-repo/src/UI.EmployerPortal.Web/Features/EmployerRegistration/EmployerRegistrationModelStore.cs)
  - Update `SaveRegistrationAddress` mapping for Canadian requests (`PortalRegistrationAddressRequestCanada`) to map `ProvinceCodeSK` from `address.Province` (resolving string abbreviation) and `CanadianPostalCode` from `address.PostalCode`.


## Open Questions

Are there any other layout changes in those images that I missed, or is just updating the phone row conditioning sufficient to match your expectations?

## Verification Plan

### Automated Tests
- Run `dotnet build d:\Unicorn-repo\src\UI.EmployerPortal.Razor.SharedComponents\UI.EmployerPortal.Razor.SharedComponents.csproj` to confirm we have 0 compilation errors.

### Manual Verification
- Render the `AddressField` locally.
- Select "United States" - verify `Phone Type` dropdown is hidden.
- Select "Canada" - verify `Phone Type` dropdown is shown, and "Phone Number / Ext." uses the 2-column layout.
- Select "Other International" - verify `Phone Type` dropdown is shown, and "Area Code / Phone Number / Ext." uses the 3-column layout.
