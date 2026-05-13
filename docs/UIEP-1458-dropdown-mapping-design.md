# UIEP-1458 Dropdown Mapping Design

## Purpose

Document the fix for employer registration dropdown fields that currently submit text values to backend survey responses. This design ensures the frontend sends the numeric integer values expected by the backend.

## Background

The employer registration UI contains several dropdown questions whose selected values are being serialized as strings or enum names. The backend survey API expects numeric integer codes for certain fields.

The key symptoms are:
- `EXPT_PAY_EE_TIME` is currently sending enum names or display strings instead of numeric values.
- Similar fields such as `PRTL_NO_LNGR_EE`, `ICRP_ST_CD`, `LLC_RGST_ST_CD`, and `CX_1500_1IN20_WHN_TXT` may also be sending strings.

## Scope

Fix the frontend mapping for the following survey fields:

- `3023 EXPT_PAY_EE_TIME`
- `3027 PRTL_NO_LNGR_EE`
- `3071 ICRP_ST_CD`
- `3080 LLC_RGST_ST_CD`
- `3276 CX_1500_1IN20_WHN_TXT`

The fix is limited to UI dropdown option values and serialization logic in the employer registration model.

## Existing Implementation

### `EXPT_PAY_EE_TIME`

Affected files:
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/PreliminaryQuestions.razor.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/PreliminaryQuestions.razor`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/PreliminaryQuestionsModel.cs`

Current behavior:
- Dropdown options are defined as `FuturePayPeriod.<Name>.ToString()` values.
- The bound model uses a string property `ExpectedFuturePayrollPeriodAsString`.
- Survey response serializes `ExpectedFuturePayrollPeriod.Value.ToString()`.

### `UISubjectivity` “When?” dropdown

Affected files:
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/UISubjectivity.razor.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/UISubjectivity.razor`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/SubjectivityModel.cs`

Current behavior:
- Option values are plain strings like `"Within 30 Days"`.
- The model stores `WhenExpectToPayWagesInAQuarter` as `string`.
- If this maps to `CX_1500_1IN20_WHN_TXT`, it will submit the label text, not numeric code.

### State dropdowns (`ICRP_ST_CD`, `LLC_RGST_ST_CD`)

Affected files:
- likely ownership or registration pages, not yet fully identified
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/EmployerRegistrationModelStore.cs`

Existing state code helper:
- `EmployerRegistrationModelStore.GetStateProvinceAbbreviationFromCode(string? stateAbbreviation)` converts abbreviations like `WI` to an integer code.

### `PRTL_NO_LNGR_EE`

Affected files:
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/PreliminaryQuestions.razor.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/NoEmployeeReason.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/PreliminaryQuestionsModel.cs`

Current understanding:
- The UI uses `NoEmployeeReason` for reasons why an employer no longer has employees.
- The backend item `PRTL_NO_LNGR_EE` expects a numeric code from `no_lngr_ee_rsn_cd` (confirmed via spreadsheet — 9 values, codes 1–9).

## Code Table Reference (from `EmployerRegistrationCodeTables.xlsx`)

### `future_period` → `EXPT_PAY_EE_TIME` (survey item 3023)

| CD_SK | Label |
|-------|-------|
| 1 | Within 30 days |
| 2 | 30 to 90 days |
| 3 | 6 months |
| 4 | One year |
| 5 | More than a year |

### `no_lngr_ee_rsn_cd` → `PRTL_NO_LNGR_EE` (survey item 3027)

| CD_SK | Label |
|-------|-------|
| 1 | Ended but not sold |
| 2 | No longer operating in Wisconsin |
| 3 | Sold or transferred |
| 4 | Continuing without Employees |
| 5 | Independent Contractors |
| 6 | Death |
| 7 | PEO Leasing |
| 8 | Fiscal Agent |
| 9 | Other |

### `cmn_clnt_st_cd` → `ICRP_ST_CD` / `LLC_RGST_ST_CD` (survey items 3071 / 3080)

75 entries covering US states/territories, Canadian provinces, and Armed Forces APO/FPO codes. Selected entries:

| CD_SK | Abbreviation | Name |
|-------|-------------|------|
| 1 | AL | Alabama |
| 2 | AK | Alaska |
| 58 | WI | Wisconsin |
| 59 | WY | Wyoming |
| 60 | AB | Alberta |
| 72 | YT | Yukon |
| 73 | AA | Armed Forces Americas |
| 74 | AE | Armed Forces Europe |
| 75 | AP | Armed Forces Pacific |

The full list matches `AddressModel.States` already in the codebase. The existing `GetStateProvinceAbbreviationFromCode()` helper in `EmployerRegistrationModelStore` covers these codes.

### `CX_1500_1IN20_WHN_TXT` (survey item 3276)

No dedicated sheet found in the spreadsheet. Based on the `future_period` table and LLD assumption, codes `1`–`5` with the same labels are expected. **Requires backend confirmation before implementation.**

---

## Proposed Solution

### 1. Map `FuturePayPeriod` to numeric values in dropdown options

Change `FuturePayPeriodOptions` so each option value is the numeric code string (confirmed via `future_period` sheet):

- `1` = Within 30 days
- `2` = 30 to 90 days
- `3` = 6 months
- `4` = One year
- `5` = More than a year

Avoid using enum names or display text as the option value.

### 2. Store `ExpectedFuturePayrollPeriod` as an enum-backed value

Replace the string-backed property `ExpectedFuturePayrollPeriodAsString` with a numeric-backed mapping layer or a strongly-typed value property that converts the selected option into `FuturePayPeriod`.

### 3. Serialize the numeric code for `EXPT_PAY_EE_TIME`

Update `PreliminaryQuestionsModel.GetSurveyResponses()` to send:

- `_response = ((int)ExpectedFuturePayrollPeriod.Value).ToString()`

instead of using `ExpectedFuturePayrollPeriod.Value.ToString()`.

### 4. Update UISubjectivity “When?” options to numeric mappings

Refactor `ExpectedWagesCatagories` so values are numeric codes, not text labels. Then ensure `SubjectivityModel.WhenExpectToPayWagesInAQuarter` captures that numeric code.

Possible approach:
- keep display text as user-facing labels
- set `Value` to `"1"`, `"2"`, `"3"`, `"4"`, `"5"`
- if necessary, change `SubjectivityModel.WhenExpectToPayWagesInAQuarter` to `int?` or to a string that contains numeric code only

### 5. Convert state dropdown selections to integer state codes

For `ICRP_ST_CD` and `LLC_RGST_ST_CD`, ensure the selected state abbreviation is converted via `GetStateProvinceAbbreviationFromCode()` before serializing or sending to backend.

### 6. Map `PRTL_NO_LNGR_EE` to `no_lngr_ee_rsn_cd` numeric codes

The spreadsheet confirms 9 valid codes for this field. The `NoEmployeeReason` enum must align with these values (codes 1–9):

| Code | Label |
|------|-------|
| 1 | Ended but not sold |
| 2 | No longer operating in Wisconsin |
| 3 | Sold or transferred |
| 4 | Continuing without Employees |
| 5 | Independent Contractors |
| 6 | Death |
| 7 | PEO Leasing |
| 8 | Fiscal Agent |
| 9 | Other |

Ensure `NoEmployeeReason` enum integer values match the CD_SK column above. Cast the enum to `int` and call `.ToString()` when serializing the survey response.

## Impacted Components

- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/PreliminaryQuestions.razor.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/PreliminaryQuestions.razor`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/PreliminaryQuestionsModel.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/UISubjectivity.razor.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Components/UISubjectivity.razor`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/SubjectivityModel.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/EmployerRegistrationModelStore.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/NoEmployeeReason.cs`
- `src/UI.EmployerPortal.Web/Features/EmployerRegistration/Models/SurveyResponseItem.cs`

## Acceptance Criteria

1. The `When` dropdown for `EXPT_PAY_EE_TIME` submits integer codes `1..5`.
2. The `UISubjectivity` dropdown for the `When?` question submits integer codes, not display strings.
3. `ICRP_ST_CD` and `LLC_RGST_ST_CD` values are converted from state abbreviation to numeric state code.
4. `PRTL_NO_LNGR_EE` submits the correct numeric mapping for the selected value.
5. No existing UI labels change; only the underlying submitted values change.
6. Existing validation and navigation logic should continue to work.

## Risks and Unknowns

- ~~The exact spreadsheet mapping for `PRTL_NO_LNGR_EE` is not available in the repo.~~ **Resolved** — confirmed via `no_lngr_ee_rsn_cd` sheet (9 codes, 1–9).
- `CX_1500_1IN20_WHN_TXT` has no dedicated sheet in the spreadsheet. Its numeric codes (`1`–`5`) are assumed to mirror `future_period` — **requires backend confirmation**.
- There may be additional dropdowns outside the identified files that also require numeric mapping.

---

## Recommended next step

Implement the numeric-value mapping in the identified components, then add focused tests or manual validation for the affected survey submission payload.
