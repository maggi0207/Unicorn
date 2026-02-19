# Shared Components — Simple Usage Guide

---

## What are these components?

These are reusable input fields used across the Employer Registration form.
Instead of building a new input every time, you drop in one of these components.

---

## 1. FEIN Number Field

**What it does**
A text box where the user types their Federal Employer Identification Number.
Shows a format hint below the box. Turns red if left empty on submit.

**File**
`Features/EmployerRegistration/Components/OutlinedTextField.razor`

**How to use it**
```razor
<OutlinedTextField Label="FEIN"
                   @bind-Value="Model.FEIN"
                   For="() => Model.FEIN"
                   FormatText="Format: XX-XXXXXXX"
                   Visible="formSubmitted" />
<FieldError For="@(() => Model.FEIN)" Visible="formSubmitted" />
```

**Where the value is saved**
`Features/Shared/Registrations/Models/BusinessInformationModel.cs`
```csharp
[Required(ErrorMessage = "FEIN is required.")]
public string? FEIN { get; set; }
```

---

## 2. Phone Number Field

**What it does**
A text box for a phone number. Opens the numeric keyboard on mobile devices.
Turns red if left empty on submit.

**File**
`Features/EmployerRegistration/Components/OutlinedTextField.razor`
*(same component as FEIN — just add `Type="tel"`)*

**How to use it**
```razor
<OutlinedTextField Label="Phone Number"
                   Type="tel"
                   @bind-Value="Model.PhoneNumber"
                   For="() => Model.PhoneNumber"
                   FormatText="Format: (XXX) XXX-XXXX"
                   Visible="formSubmitted" />
<FieldError For="@(() => Model.PhoneNumber)" Visible="formSubmitted" />
```

**Where the value is saved**
`Features/Shared/Registrations/Models/BusinessInformationModel.cs`
```csharp
[Required(ErrorMessage = "Phone Number is required.")]
public string? PhoneNumber { get; set; }
```

---

## 3. Select / Dropdown Field

**What it does**
A dropdown where the user picks one option from a list (e.g. State, Country).
Turns red if nothing is selected on submit.

**Files**
- Component: `Features/EmployerRegistration/Components/OutlinedSelectField.razor`
- Option model: `Features/EmployerRegistration/Components/SelectOption.cs`

**How to use it**
```razor
<OutlinedSelectField Label="State"
                     @bind-Value="Model.MailingState"
                     Options="States"
                     Placeholder="Select a state"
                     For="() => Model.MailingState"
                     Visible="formSubmitted" />
<FieldError For="@(() => Model.MailingState)" Visible="formSubmitted" />
```

The list of options is defined in the code-behind file:
`Features/EmployerRegistration/BusinessInformation.razor.cs`
```csharp
protected List<SelectOption> States { get; set; } = new()
{
    new SelectOption { Value = "AL", Text = "Alabama" },
    new SelectOption { Value = "AK", Text = "Alaska" },
    // add more as needed
};
```

**Where the value is saved**
`Features/Shared/Registrations/Models/BusinessInformationModel.cs`
```csharp
[Required(ErrorMessage = "State is required.")]
public string? State { get; set; }
```

---

## 4. Address Fields (Using AddressModel)

**What it does**
A reusable address form component that collects Country, Address Line 1/2, City, State, and Zip.
Instead of binding individual properties, use the `AddressField` component with `AddressModel`.
Required fields show errors if left empty; optional fields (Address Line 2, Extension) do not.

**Files**
- Component: `Razor.SharedComponents/Address/AddressField.razor`
- Model: `Razor.SharedComponents/Model/AddressModel.cs`

**How to use it**
```razor
<AddressField @bind-Value="Model.MailingAddress"
             Visible="formSubmitted" />
```

The form automatically handles all 7 address fields (Country, Address Line 1/2, City, State, Zip, Extension).

**Where the value is saved**
`Features/Shared/Registrations/Models/BusinessInformationModel.cs`
```csharp
public AddressModel MailingAddress { get; set; } = new();
public List<AddressModel> PhysicalLocations { get; set; } = new()
{
    new AddressModel()
};
```

For Business Contact:
`Features/Shared/Registrations/Models/BusinessContactModel.cs`
```csharp
public AddressModel ContactAddress { get; set; } = new();
```

**AddressModel properties**
```csharp
public string? Country { get; set; } = "United States";  // required
public string? AddressLine1 { get; set; }                // required
public string? AddressLine2 { get; set; }                // optional
public string? City { get; set; }                        // required
public string? State { get; set; }                       // required
public string? Zip { get; set; }                         // required
public string? Extension { get; set; }                   // optional (ZIP+4)
```

---

## 5. Validation (Error Messages)

**What it does**
Error messages stay hidden when the page first loads.
They only appear after the user clicks Submit and leaves a required field empty.
Once the user starts typing in that field, the error clears automatically.

**File**
`Features/EmployerRegistration/Components/FieldError.razor`

**The rule — always pair every required field with FieldError**

```
OutlinedTextField  ← the input box
FieldError         ← the red error message below it
```

Both must point to the same model property using `For`:

```razor
<OutlinedTextField Label="Zip Code"
                   @bind-Value="Model.MailingZip"
                   For="() => Model.MailingZip"
                   Visible="formSubmitted" />

<FieldError For="@(() => Model.MailingZip)" Visible="formSubmitted" />
```

**Where errors are defined**
`Features/Shared/Registrations/Models/BusinessInformationModel.cs`

Add `[Required]` above a property to make it mandatory:
```csharp
[Required(ErrorMessage = "Zip Code is required.")]
public string? Zip { get; set; }
```

Change the text inside `ErrorMessage = "..."` to control what the user sees.

---

## Quick Checklist — Adding a New Field

- [ ] Add the property to `BusinessInformationModel.cs` or `BusinessContactModel.cs`
- [ ] Add `[Required]` if the field is mandatory
- [ ] Drop in `OutlinedTextField` or `OutlinedSelectField` in the `.razor` page
- [ ] Add `<FieldError>` directly below it (required fields only)
- [ ] Make sure `For=` on both lines points to the same property
- [ ] Make sure `Visible="formSubmitted"` is on both lines

---

## Address Model Structure

For address data (mailing, physical locations, contact), use the reusable `AddressModel` in `Razor.SharedComponents/Model/AddressModel.cs`:

**In BusinessInformationModel:**
```csharp
public AddressModel MailingAddress { get; set; } = new();
public List<AddressModel> PhysicalLocations { get; set; } = new();
```

**In BusinessContactModel:**
```csharp
public AddressModel ContactAddress { get; set; } = new();
```

**In your .razor page, bind the entire address at once:**
```razor
<AddressField @bind-Value="Model.MailingAddress" />
```
