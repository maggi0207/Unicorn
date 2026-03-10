# Address Correction — UI Progress & Figma Comparison

Tracks what the Figma design specifies, what has been implemented, and open items for discussion.

---

## Figma Design — What It Shows

The Address Correction page (from Figma) has this layout top-to-bottom:

```
[Warning banner — red border, light pink bg]
  "Verify your address: We think this address may be incorrect or incomplete."
  [red circle icon] "To proceed, please choose one of the options below..."

[Section header — blue underline]
  e.g. "Business Mailing Address"

You entered:
[Gray box]
  201 E WASHINGTON AVE APARTMENT 3
  MADISON, WI 53703

EDIT ADDRESS                    [USE ADDRESS AS ENTERED]

Corrected address:              ← only when service has a suggestion
[Gray box]
  201 E WASHINGTON AVE APT 3
  MADISON, WI 53703-2866

                        [USE SUGGESTED ADDRESS]
```

When user clicks "USE ADDRESS AS ENTERED":
- Undeliverable warning appears at top of page (above title)
- CONTINUE button appears at the bottom right

---

## What Has Been Implemented

### Layout & Structure
| Feature | Status | Notes |
|---|---|---|
| Warning banner at top (before section header) | Done | Moved above `ac-section-header` |
| Section header ("Business Mailing Address") | Done | Blue underline, `#003663` color |
| "You entered:" label | Done | 16px, `#000000` |
| Gray address box (original address) | Done | `#efefef` bg, 16px bold |
| EDIT ADDRESS link (left) | Done | `#003663`, underlined, uppercase |
| USE ADDRESS AS ENTERED button (right) | Done | Outline style, matches ButtonBar |
| "Corrected address:" label | Done | Only shows when `Suggested is not null` |
| Gray address box (corrected address) | Done | Same style as original box |
| USE SUGGESTED ADDRESS button (right-aligned) | Done | Primary dark blue, matches ButtonBar |
| Undeliverable warning above page title | Done | Shows when user clicks "Use As Entered" |
| CONTINUE button | Done | Only shows after undeliverable warning |

### Styling
| Property | Value | Status |
|---|---|---|
| Page width | 684px | Done |
| Page gap | 24px (matches BusinessInformation) | Done |
| Page title | 32px bold, margin-bottom 8px | Done |
| Section label font-size | 16px, `#000000` | Done |
| Address box text | 16px bold, `#1a1a1a` | Done |
| Button font-size | 16px (matches ButtonBar shared component) | Done |
| Button line-height | 25.6px (matches ButtonBar) | Done |
| Hover guard | `:hover:not(:disabled)` | Done |

### Service Integration
| Feature | Status | Notes |
|---|---|---|
| Show address correction for invalid addresses | Done | `!isValid` |
| Show address correction for valid + standardized | Removed | Was `CorrectedAddress is not null` — caused correction page to show even for valid addresses; reverted to `!isValid` only |
| Copy suggested values into Original on accept | Done | In-place mutation of `AddressModel` |
| Multi-address correction queue | Done | `CorrectionIndex` iterates through list |
| Navigate to BusinessContact after all corrections | Done | When `CorrectionIndex >= count` |
| Navigate back to BusinessInformation on edit | Done | Restores model from `RegistrationState.BusinessInfo` |

---

## Improvements Made Beyond Basic Figma

These were added based on real service behavior and UX requirements:

1. **Undeliverable warning at TOP of page** — Figma showed it inline below buttons; moved above `<h1>` for higher visibility.

2. **Multi-address correction index** — Figma shows a single address. Implementation handles multiple addresses (Mailing + up to 2 Physical Locations) in sequence using `CorrectionIndex`.

3. **Digit-only enforcement on Zip/Extension** — Not in Figma spec but required to prevent SOAP deserialization faults. Strips non-digits on input.

4. **WCF error graceful fallback** — Service unavailability returns `IsValid=false` with a user-friendly message; user can still proceed via "Use As Entered".

5. **Country code ISO mapping** — Service requires `"US"` not `"United States"`. Handled in `ToCountryCode()` method.

6. **LineOneAddress/LineTwoAddress swap** — Service occasionally returns street in `LineTwoAddress` when `LineOneAddress` is empty. Handled in response mapping.

---

## Open / Pending Discussion

Items to discuss and finalize:

### 1. Section Header Label — Needs Design Confirmation

**Current behavior:** The section header shows a generic label such as "Business Mailing Address" or "Physical Location 1".

**Requirement:** The label must clearly communicate to the user which address they are correcting, especially when multiple addresses are queued. The exact wording and format (e.g. "Mailing Address — Step 1 of 2") needs to be confirmed against the Figma design before implementation.

**Action needed:** Share design mock-up and get sign-off on the label format.

---

### 2. "Use Address As Entered" — Warning + Continue Flow

**Current behavior:** Clicking "Use Address As Entered" shows the undeliverable warning above the page title.

**Requirement (from UAT):** After the warning appears, a **Continue** button must be shown so the user can explicitly acknowledge the warning and proceed. The page should not auto-advance.

**Open question:** Does the Continue button appear inline below the warning, or does it replace the button bar at the bottom? Confirm placement against UAT spec.

---

### 3. Duplicate Address — Skip Physical Correction if Mailing Already Accepted

**Scenario:** The user's mailing address and physical address are identical. The service returns the same correction suggestion for both. The user already accepted the suggestion (or chose "Use As Entered") for mailing.

**Requirement:** Should the physical address correction step be skipped automatically since the user already resolved the same address?

**Options to decide:**
- **Skip automatically** — If the corrected address for physical matches the already-accepted mailing correction, apply the same choice silently and advance.
- **Always show** — Show the correction screen for each address independently, even if they are identical.

**Action needed:** Product/UX decision required before implementation.

---

### 4. EditAddress Behavior for Physical Locations

Currently always navigates back to BusinessInformation. Should it scroll to or highlight the specific address field?

### 5. Service Unavailable UX

Currently shows a warning with "Use As Entered". Should there be a retry button?

### 6. Address Correction for BusinessContact Page

BusinessContact also has a mailing address. Does it go through the same correction flow?

---

## File Locations

| File | Purpose |
|---|---|
| `Features/EmployerRegistration/AddressCorrection.razor` | Page markup and logic |
| `Features/EmployerRegistration/AddressCorrection.razor.css` | Page styles |
| `Features/EmployerRegistration/Services/AddressValidationService.cs` | WCF wrapper |
| `Features/EmployerRegistration/Services/IAddressValidationWrapper.cs` | Interface + result record |
| `Features/EmployerRegistration/Services/RegistrationStateService.cs` | Navigation state |
| `Features/EmployerRegistration/BusinessInformation.razor` | Calls validation, builds correction list |
