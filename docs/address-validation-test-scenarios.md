# Address Validation — Test Scenarios

Manual test cases for the Address Correction flow. Run these end-to-end in the browser after starting the app.

---

## Setup

1. Start the app: `dotnet run` in `src/UI.EmployerPortal.Web/`
2. Navigate to: `https://localhost:7275/employer-registration/business-information`
3. Fill in required fields (FEIN, Legal Name, Phone, Email) — use any valid values
4. Use the address fields below to test each scenario

---

## Test Cases

### 1. Valid Address — Service Returns Suggestion (Happy Path)

**Input (Mailing Address)**
```
Street:    201 E Washington Ave Apartment 3
City:      Madison
State:     WI
Zip:       53703
+4:        (leave blank)
Country:   United States
```

**Expected**
- Navigates to Address Correction page
- Warning banner shows
- "You entered:" box shows `201 E WASHINGTON AVE APARTMENT 3 / MADISON, WI 53703`
- "Corrected address:" box shows `201 E WASHINGTON AVE APT 3 / MADISON, WI 53703-2866`
- "USE SUGGESTED ADDRESS" button visible

**Click USE SUGGESTED ADDRESS**
- Navigates to next address correction (if physical address also needs review) OR to BusinessContact

---

### 2. Invalid / Fake Address — No Suggestion Available

**Input (Mailing Address)**
```
Street:    117
City:      New Heaven
State:     AR
Zip:       06450
+4:        (leave blank)
```

**Expected**
- Navigates to Address Correction page
- Warning banner shows
- "You entered:" box shows `117 / NEW HEAVEN, AR 06450`
- No "Corrected address:" section (service returned no suggestion)
- Only EDIT ADDRESS and USE ADDRESS AS ENTERED buttons visible

**Click USE ADDRESS AS ENTERED**
- Undeliverable warning appears at TOP of page (above "Address Correction" title)
- CONTINUE button appears at bottom right

**Click CONTINUE**
- Advances to next address or BusinessContact

---

### 3. Use Address As Entered Flow

**Precondition:** Any address that triggers the correction page

**Steps**
1. Click "USE ADDRESS AS ENTERED"
2. Verify: undeliverable warning appears above page title in red bold text
3. Verify: CONTINUE button appears (right-aligned)
4. Click CONTINUE
5. Verify: advances to next step

---

### 4. Edit Address Flow

**Precondition:** Any address that triggers the correction page

**Steps**
1. Click "EDIT ADDRESS"
2. Verify: navigates back to BusinessInformation
3. Verify: previously entered values are restored (model persisted in `RegistrationStateService`)
4. Change address to a real address (e.g. Scenario 1)
5. Click CONTINUE
6. Verify: goes through correction flow again with new address

---

### 5. Multiple Addresses — Mailing + Physical

**Setup:** Enter an invalid/fake mailing address AND an invalid physical address

**Expected**
- Address Correction shows for mailing address first ("Business Mailing Address" section header)
- After resolving mailing, correction shows for physical address ("Physical Location 1" section header)
- After all resolved, navigates to BusinessContact

---

### 6. Text in Zip Code Extension (Input Guard)

**Steps**
1. Click in the +4 (Extension) field
2. Type: `abcd`
3. Verify: no characters appear in the field (non-digits are stripped on input)
4. Type: `12345`
5. Verify: only `1234` appears (max 4 digits enforced)

**Steps for Zip Code**
1. Click in Zip Code field
2. Type: `abc12`
3. Verify: only `12` appears (non-digits stripped)
4. Type: `123456`
5. Verify: only `12345` appears (max 5 digits)

---

### 7. WCF Service Unavailable (Simulated)

**Setup:** Stop the WCF service or disconnect from the network

**Expected**
- After clicking CONTINUE on BusinessInformation, navigates to Address Correction
- Warning banner shows
- Error message: "Address validation is temporarily unavailable. Please try again."
- No corrected address shown
- User can click "USE ADDRESS AS ENTERED" → CONTINUE to proceed

---

### 8. Country Code Mapping

**Input (Mailing Address)**
```
Country:   Canada
State:     (select a Canadian province, e.g. ON)
Zip:       M5H 2N2
```

**Expected**
- No SOAP fault (CountryCode sent as `"CA"` not `"Canada"`)
- Service returns a result (valid or invalid depending on address)

---

### 9. Valid Address — Identical to Suggestion

**Input:** Enter a perfectly standardized US address that the service would return unchanged

**Expected**
- The service may still return `OutputAddress` (even if identical)
- Address Correction page shows with both "You entered" and "Corrected address" looking the same
- User can choose either option

> Note: This is a known edge case. The service always populates `OutputAddress` for found addresses, even if no standardization was needed.

---

## Regression Checklist

After any change to `AddressValidationService.cs`, `BusinessInformation.razor`, or `AddressCorrection.razor`, verify:

- [ ] Scenario 1 (valid address with suggestion) works end-to-end
- [ ] Scenario 2 (fake address, no suggestion) works end-to-end
- [ ] Scenario 3 (Use As Entered + Continue) works
- [ ] Scenario 4 (Edit Address restores form values) works
- [ ] Scenario 6 (digit-only zip fields) enforced in UI
- [ ] No SOAP faults in browser console or debug output
