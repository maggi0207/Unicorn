# UIEP-3745 — Demo / UAT Document  
**Corporate Officer Exclusion (COE) — End Messaging & Flow**

| Field | Value |
|---|---|
| Ticket | [UIEP-3745](https://dwd-wi.atlassian.net/browse/UIEP-3745) |
| Feature | Corporate Officer Exclusion election confirmation flow |
| Route | `/manage-account/corporate-office-exclusion` |
| Release | Oct 28 UAT / NNMilestone2 |
| Demo date | _fill in_ |
| Tester | _fill in_ |
| Environment | _UAT / Local_ |

---

## 1. Purpose

Show that after submitting a Corporate Officer Exclusion election, the new portal:

1. No longer stays silent on Submit  
2. Shows clear success / error messaging  
3. Prevents a second election attempt on the same page / revisit  
4. Matches Figma screens for Intro → Worksheet → Success → Already elected  

---

## 2. Screen map (Figma + capture folder)

| # | Screen | Figma reference | Captured demo shot |
|---|---|---|---|
| A | **Before fix** (baseline) | — | `Demo-Screenshots/00-before-initial-worksheet.png` |
| B | Intro | `figma screenshot/Initial.png` | `Demo-Screenshots/06-intro-screen.png` ✅ |
| C | Worksheet default | `…Calculate Worksheet.png` | _(current UI)_ |
| D | Must-calculate banner | — | `Demo-Screenshots/01-submit-without-calculate-must-calculate-banner.png` |
| E | Inline missing value | `…inline errors.png` | _capture as `05-inline-missing-value.png`_ |
| F | After Calculate | `…Caluclator Worksheet.png` | _capture as `06-after-calculate.png`_ |
| G | No-savings warning | — | `Demo-Screenshots/02-no-savings-warning-on-submit.png` |
| H | Eligibility reject | Ticket attachment | `Demo-Screenshots/03-eligibility-rejected-deadline-and-payroll.png` |
| I | Submit failed | `…submit failed.png` | _capture if reproducible_ |
| J | Success + letter links | `…Success.png` | _needs eligible account_ |
| K | Success + mail banner | `…Success with banner.png` | _needs eligible account_ |
| L | Already elected | `…Already visited.png` | _needs already-elected account_ |

Folder paths (this repo):

- Figma: `COE/COE/figma screenshot/`  
- Captures: `COE/COE/Demo-Screenshots/`  

---

## 3. Accounts required

| Scenario | Account type | Notes |
|---|---|---|
| Happy path (Intro → Success) | **Eligible** newly subject Corp / LLC-as-Corp | Payroll under $500k; inside election window (initial report due date **or** within 30 days of Initial Determination) |
| Already elected | Account with COE **already in effect** | Reopen COE menu item |
| Negative / blocked paths | Any openable COE account (e.g. VANDERLOOP-style) | Deadline / $500k / no-savings |

**How to get an eligible account (from ticket):**  
Register a Corporation that activates on submit → newly subject → take COE before deadline.

| Account used in demo | Purpose |
|---|---|
| _fill — eligible_ | Steps 1–6 success |
| _fill — already elected_ | Step 7 |
| _fill — ineligible (optional)_ | Step 8 |

---

## 4. Pre-demo checklist

1. Office build succeeds (0 errors) with latest COE files pasted  
2. Logged in as employer user with access to test account  
3. Navigate: **View & Manage Account → Corporate Officer Exclusion**  
4. Have Figma folder + Demo-Screenshots open for side-by-side compare  

---

## 5. Step-by-step reproduce scripts

### STEP 0 — Baseline (what was broken)

| | |
|---|---|
| **Goal** | Show original defect: Submit did nothing |
| **Screen ref** | `Demo-Screenshots/00-before-initial-worksheet.png` |
| **Steps** | 1. Open old build (if available) or show captured screenshot<br>2. On worksheet click **SUBMIT ELECTION** without messaging |
| **Expected (before)** | No change — no success, no error |
| **Ticket link** | “Nothing happens after successful COE election submission” |

---

### STEP 1 — Intro screen (new)

| | |
|---|---|
| **Goal** | Eligible account lands on Intro, not worksheet |
| **Figma** | `figma screenshot/Initial.png` |
| **Capture** | Save as `Demo-Screenshots/06-intro-screen.png` |
| **Account** | Eligible (or any not-yet-elected) |
| **Steps** | 1. Select test employer account<br>2. Open **Corporate Officer Exclusion**<br>3. Do **not** click NEXT yet |
| **Expected** | Title: **Corporate Officer Exclusion**<br>Eligibility copy ($500k / Corp / LLC-as-Corp)<br>Grey card: Account Number, Legal Name, effective-as-of date<br>Button: **NEXT** only |
| **Pass?** | ☐ |

---

### STEP 2 — Worksheet after NEXT

| | |
|---|---|
| **Goal** | NEXT opens reasonableness worksheet |
| **Figma** | `…Calculate Worksheet.png` / `…Caluclator Worksheet.png` |
| **Steps** | 1. From Intro click **NEXT** |
| **Expected** | Title: **Corporate Officer Exclusion Reasonableness Worksheet**<br>Table lines 1–10 + Net Savings<br>Buttons: **CANCEL** \| **CALCULATE** \| **SUBMIT ELECTION**<br>Line 1 With Exclusion shows **$0.00** (if product confirmed) |
| **Pass?** | ☐ |

---

### STEP 3 — Submit without Calculate (must-calculate)

| | |
|---|---|
| **Goal** | Submit is never silent |
| **Screen ref** | `Demo-Screenshots/01-submit-without-calculate-must-calculate-banner.png` |
| **Steps** | 1. On worksheet (values may be $0.00)<br>2. Click **SUBMIT ELECTION** without clicking CALCULATE |
| **Expected** | Red Alert banner: *You must calculate your net savings before electing this exclusion.*<br>Stay on worksheet |
| **Pass?** | ☐ |

---

### STEP 4 — Inline missing value (Figma validation)

| | |
|---|---|
| **Goal** | Empty / $0 required fields show Missing value |
| **Figma** | `…inline errors.png` |
| **Capture** | `05-inline-missing-value.png` |
| **Steps** | 1. Leave Line 1 FUTA Without at **$0.00** (and/or WI Without at $0)<br>2. Click **CALCULATE** |
| **Expected** | Top Alert: *Please fix the following errors:* + *Payroll Subject to FUTA missing.* (and WI if applicable)<br>Under field: red border + **Missing value** |
| **Pass?** | ☐ |

---

### STEP 5 — Calculate updates worksheet

| | |
|---|---|
| **Goal** | CALCULATE refreshes tax rows / Net Savings |
| **Figma** | Worksheet with values |
| **Steps** | 1. Enter FUTA Without &gt; 0 (e.g. `400`)<br>2. Enter WI Without &gt; 0 (e.g. `600`)<br>3. Enter WI With (e.g. `0` or lower)<br>4. Click **CALCULATE** |
| **Expected** | Button shows CALCULATING… briefly<br>Lines 2–10 and Net Savings update<br>Must-calculate / missing-value banners clear |
| **Pass?** | ☐ |

---

### STEP 6A — Submit success (happy path) ★ needs eligible account

| | |
|---|---|
| **Goal** | End messaging after successful election |
| **Figma** | `…Success.png` + card fields from `…Already visited.png` |
| **Capture** | `07-success-confirmation.png` |
| **Account** | Eligible |
| **Steps** | 1. Complete Step 5<br>2. Click **SUBMIT ELECTION**<br>3. If no-savings warning appears (Step 6B), click **SUBMIT ELECTION** again to confirm<br>4. Observe success screen |
| **Expected** | Centered green: **Your election successfully submitted.**<br>Links:<br>• View Your Initial Determination letter → UI Documents<br>• Mail a copy of the ID… → shows mail banner (Step 6C)<br>Summary card: Account Number, Legal Name, Confirmation ID (if API), Date Submitted (if API), effective date<br>Button: **ACCOUNT SUMMARY** only (no BACK) |
| **Pass?** | ☐ |

---

### STEP 6B — No-savings / pay MORE warning (optional)

| | |
|---|---|
| **Goal** | Backend warning when estimates show higher tax |
| **Screen ref** | `Demo-Screenshots/02-no-savings-warning-on-submit.png` |
| **Steps** | 1. Calculate with figures that produce negative Net Savings<br>2. Click **SUBMIT ELECTION** once |
| **Expected** | Alert: *Warning - Your estimates indicate you will pay MORE unemployment tax…*<br>Message says click Submit again to proceed, or Cancel |
| **Pass?** | ☐ |

---

### STEP 6C — Mail copy banner (Figma Success with banner)

| | |
|---|---|
| **Goal** | Mail-copy link shows Figma banner |
| **Figma** | `…Success with banner.png` |
| **Steps** | 1. On success screen click **Mail a copy of the ID to the main business mailing address** |
| **Expected** | Green/confirmation banner: *Initial Determination letter will be mailed to your main business mailing address.* |
| **Pass?** | ☐ |

---

### STEP 6D — Account Summary exit

| | |
|---|---|
| **Goal** | Shaun-approved end button |
| **Steps** | 1. On success click **ACCOUNT SUMMARY** |
| **Expected** | Navigates to `/manage-account/account-summary`<br>Not account-details; not legacy Complete |
| **Pass?** | ☐ |

---

### STEP 7 — Already elected (revisit) ★ needs elected account

| | |
|---|---|
| **Goal** | Cannot take election again; confirmation shown |
| **Figma** | `…Already visited.png` |
| **Capture** | `08-already-elected.png` |
| **Account** | Same as successful Step 6, or known elected account |
| **Steps** | 1. From Account Summary, open **Corporate Officer Exclusion** again |
| **Expected** | **No worksheet**, no Submit<br>Banner: *Corporate Officer Exclusion submitted.*<br>Card: Account Number, Legal Name, Confirmation ID, Date Submitted, effective date<br>Button: **ACCOUNT SUMMARY** only |
| **Pass?** | ☐ |

---

### STEP 8 — Eligibility reject (optional / negative)

| | |
|---|---|
| **Goal** | Backend rule violations still surface clearly |
| **Screen ref** | `Demo-Screenshots/03-eligibility-rejected-deadline-and-payroll.png` |
| **Account** | Ineligible (deadline / ≥$500k payroll) |
| **Steps** | 1. Intro → NEXT → Calculate → Submit |
| **Expected** | Alert with messages such as:<br>• Request received after application deadline…<br>• Previous calendar year taxable payroll is $500,000 or more… |
| **Pass?** | ☐ |

---

### STEP 9 — Submit failed banner (if forceable)

| | |
|---|---|
| **Goal** | Unexpected save failure messaging |
| **Figma** | `…submit failed.png` |
| **Steps** | Simulate service down / exception (dev only) **or** skip if not practical in UAT |
| **Expected** | *Worksheet failed to submit. Please try again.* |
| **Pass?** | ☐ / N/A |

---

## 6. Ticket acceptance mapping

| Ticket technical note | Demo step that proves it |
|---|---|
| Nothing after successful submit | Step 6A |
| Can Submit again on same page | Steps 6A + 7 |
| No COE effective date | Steps 1, 6A, 7 |
| No Account Summary navigation | Step 6D |
| No intro screen | Step 1 |
| Already-elected messaging | Step 7 |
| Account Summary (not Complete) | Step 6D |
| View / mail Initial Determination (gap) | Steps 6A + 6C (UI; letter content depends on BE) |

---

## 7. Out of scope / known follow-ups

| Item | Owner |
|---|---|
| Dual “already in effect” + “suspended” errors | UIEP-3750 (backend) |
| Confirmation ID exact contract vs `CorporateOfficerExclusionSK` | Mendota / product |
| Actual mail send of ID letter | Backend / product |
| Line 1 With Exclusion always `$0.00` | Product confirm (FE implemented) |

---

## 8. Demo talking points (2 minutes)

1. **Before:** Submit did nothing → employer could retry and get confusing errors.  
2. **Now:** Intro explains eligibility → Calculate required → clear banners.  
3. **Success:** Green confirmation + letter links + Account Summary.  
4. **Revisit:** Already elected card — no second Submit on worksheet.  

---

## 9. Sign-off

| Role | Name | Date | Result |
|---|---|---|---|
| Developer | | | ☐ Pass ☐ Fail |
| QA / UAT | | | ☐ Pass ☐ Fail |
| Product (optional) | | | ☐ Pass ☐ Fail |

**Notes / defects found during demo:**  
_ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _ _

---

## 10. File paste list (office repo)

```
Features/ManageAccount/Pages/CorporateOfficerExclusion.razor
Features/ManageAccount/Pages/CorporateOfficerExclusion.razor.cs
Features/ManageAccount/Pages/CorporateOfficerExclusion.razor.css
Features/ManageAccount/Services/CorporateOfficerExclusionService.cs
Features/ManageAccount/Models/COESaveResponse.cs
Features/ManageAccount/Models/COEElectionSummary.cs
Program.cs  (DI registration line, if missing)
```

---

*End of UIEP-3745 demo document.*
