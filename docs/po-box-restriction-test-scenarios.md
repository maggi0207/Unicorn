# PO Box Restriction — Test Scenarios
## Feature: Business Information Step 3

---

### TC-01: Checkbox disabled when mailing address is a PO Box

| | |
|---|---|
| **Precondition** | Business Information page is open |
| **Steps** | Enter `PO Box 1234` in the mailing address Street Address field |
| **Expected Result** | "Physical Location is the same as the Business address." checkbox is **disabled** (greyed out, not clickable) |

---

### TC-02: Checkbox enabled when mailing address is a street address

| | |
|---|---|
| **Precondition** | Business Information page is open |
| **Steps** | Enter `123 Main St` in the mailing address Street Address field |
| **Expected Result** | Checkbox is **enabled** and clickable |

---

### TC-03: Checking the checkbox copies mailing address to physical location

| | |
|---|---|
| **Precondition** | Mailing address is filled with a valid street address (not PO Box) |
| **Steps** | Check the "Physical Location is the same as the Business address." checkbox |
| **Expected Result** | All physical location fields (Street, City, State, Zip, etc.) are **auto-filled** with the mailing address values and **disabled** |

---

### TC-04: Auto-uncheck when mailing changes to PO Box while checkbox is checked

| | |
|---|---|
| **Precondition** | Mailing address is `123 Main St`, checkbox is **checked** |
| **Steps** | Change mailing Street Address to `PO Box 999` |
| **Expected Result** | Checkbox is **automatically unchecked**, physical location fields are **cleared** |

---

### TC-05: Validation error when physical location is typed as PO Box (no checkbox)

| | |
|---|---|
| **Precondition** | Checkbox is unchecked, all other fields are valid |
| **Steps** | Type `PO Box 500` directly into the Physical Location Street Address field, then click Continue |
| **Expected Result** | Form fails validation, error message shown: *"Physical location cannot be a PO Box. Please enter a street address."* |

---

### TC-06: No error when physical location is a valid street address

| | |
|---|---|
| **Precondition** | All fields filled with valid data, physical location Street Address = `456 Oak Ave` |
| **Steps** | Click Continue |
| **Expected Result** | No PO Box error, form proceeds to next step |

---

### TC-07: Checkbox disabled even if other mailing fields are filled

| | |
|---|---|
| **Precondition** | Mailing City, State, Zip are filled |
| **Steps** | Enter `PO Box 777` in mailing Street Address |
| **Expected Result** | Checkbox is **disabled** regardless of other fields being valid |

---

### TC-08: Checkbox disabled when mailing Apt/Suite field contains PO Box

| | |
|---|---|
| **Precondition** | Business Information page is open |
| **Steps** | Enter `PO Box 1234` in the mailing address **Apt/Suite** field (AddressLine2) |
| **Expected Result** | "Physical Location is the same as the Business address." checkbox is **disabled** |

---

### TC-09: Validation error when physical location Apt/Suite field is a PO Box

| | |
|---|---|
| **Precondition** | Checkbox is unchecked, all other fields are valid |
| **Steps** | Type `PO Box 500` into the Physical Location **Apt/Suite** field (AddressLine2), then click Continue |
| **Expected Result** | Form fails validation, error shown under the Apt/Suite field: *"Physical location cannot be a PO Box. Please enter a street address."* |

---

### TC-10: Both AddressLine1 and AddressLine2 show errors when both contain PO Box

| | |
|---|---|
| **Precondition** | Checkbox is unchecked, all other fields are valid |
| **Steps** | Enter `PO Box 100` in Street Address AND `PO Box 200` in Apt/Suite for a physical location, then click Continue |
| **Expected Result** | Two separate error messages shown — one under Street Address, one under Apt/Suite |
