# UIEP-1458 Dropdown Mapping Low-Level Design

## Objective

Provide concrete implementation details for converting front-end dropdown selections into the numeric survey response values expected by the backend.

## Data Flow

1. User selects a dropdown option in the UI.
2. The selected option writes to a Razor component property.
3. The model stores the selected value as either an enum or a numeric code.
4. `GetSurveyResponses()` converts the selected value to the exact string used by the backend survey payload.

## Detailed Implementation Changes

### A. `EXPT_PAY_EE_TIME` fix

#### Current behavior

In `PreliminaryQuestions.razor.cs`:
- `FuturePayPeriodOptions` uses `Value = FuturePayPeriod.X.ToString()`.
- The select field binds to `ExpectedFuturePayrollPeriodAsString`.

In `PreliminaryQuestionsModel.cs`:
- `GetSurveyResponses()` serializes `ExpectedFuturePayrollPeriod.Value.ToString()`.

#### LLD change

1. Change `FuturePayPeriodOptions` so values are numeric strings:
   - `new SelectOption { Value = "1", Text = "Within 30 days" }`
   - `new SelectOption { Value = "2", Text = "30 to 90 days" }`
   - `new SelectOption { Value = "3", Text = "6 months" }`
   - `new SelectOption { Value = "4", Text = "One year" }`
   - `new SelectOption { Value = "5", Text = "More than a year" }`

2. Replace `ExpectedFuturePayrollPeriodAsString` with a new binding path that maps numeric option strings into `FuturePayPeriod`.
   - Option A: keep the string facade, but deserialize numeric strings into `FuturePayPeriod` using `Enum.IsDefined` or `Enum.TryParse`.
   - Option B: use a strongly-typed `int?` or `FuturePayPeriod?` property for binding.

3. In `PreliminaryQuestionsModel.GetSurveyResponses()` update serialization:
   - `_response = ((int)ExpectedFuturePayrollPeriod.Value).ToString()`

#### Example conversion logic

```csharp
private string? ExpectedFuturePayrollPeriodValue
{
    get => Model.ExpectedFuturePayrollPeriod.HasValue ? ((int)Model.ExpectedFuturePayrollPeriod.Value).ToString() : null;
    set
    {
        if (int.TryParse(value, out var intValue) && Enum.IsDefined(typeof(FuturePayPeriod), intValue))
        {
            Model.ExpectedFuturePayrollPeriod = (FuturePayPeriod)intValue;
        }
        else
        {
            Model.ExpectedFuturePayrollPeriod = null;
        }
    }
}
```

### B. `UISubjectivity` dropdown fix (`CX_1500_1IN20_WHN_TXT`)

> **Note:** No dedicated sheet for `CX_1500_1IN20_WHN_TXT` exists in `EmployerRegistrationCodeTables.xlsx`. Codes `1`–`5` below are assumed to mirror the `future_period` table. **Requires backend confirmation before implementation.**

#### Current behavior

In `UISubjectivity.razor.cs`:
- `ExpectedWagesCatagories` uses text values for both `Text` and `Value`.
- `SubjectivityModel.WhenExpectToPayWagesInAQuarter` is a `string`.

#### LLD change

1. Change `ExpectedWagesCatagories` to numeric values or enum-backed values.
2. Set values to `"1"`..`"5"` (assumed codes) and keep display labels unchanged.
3. Change `SubjectivityModel.WhenExpectToPayWagesInAQuarter` to either:
   - `int?` and cast to string when serializing, or
   - `string` containing only the numeric code.
4. Ensure any response serialization that maps to item `3276` uses that numeric code.

#### Suggested model update

```csharp
public int? WhenExpectToPayWagesInAQuarter { get; set; }
```

#### Suggested option list

```csharp
public static readonly List<SelectOption> ExpectedWagesCatagories = new()
{
    new SelectOption { Value = "1", Text = "Within 30 Days" },
    new SelectOption { Value = "2", Text = "30 to 90 days" },
    new SelectOption { Value = "3", Text = "6 months" },
    new SelectOption { Value = "4", Text = "One year" },
    new SelectOption { Value = "5", Text = "More than a year" }
};
```

### C. `ICRP_ST_CD` and `LLC_RGST_ST_CD` state code serialization

#### Confirmed code table (`cmn_clnt_st_cd` from `EmployerRegistrationCodeTables.xlsx`)

75 entries. Selected samples:

| CD_SK | Abbreviation | Name |
|-------|-------------|------|
| 1 | AL | Alabama |
| 2 | AK | Alaska |
| 58 | WI | Wisconsin |
| 60 | AB | Alberta |
| 73 | AA | Armed Forces Americas |
| 74 | AE | Armed Forces Europe |
| 75 | AP | Armed Forces Pacific |

Full list aligns with `AddressModel.States` in the codebase.

#### Current behavior

The repo already contains a state/province abbreviation mapping helper in `EmployerRegistrationModelStore`.

#### LLD change

1. Identify the page/model where `ICRP_ST_CD` and `LLC_RGST_ST_CD` are stored.
2. When serializing the survey response, convert the selected abbreviation using:
   - `GetStateProvinceAbbreviationFromCode(address.State)`
   - `GetStateProvinceAbbreviationFromCode(address.Province)`
3. Submit the resulting integer code as a string in `_response`.

#### Example

```csharp
responses.Add(new SurveyResponse
{
    _surveyResponseItemSk = (int)SurveyResponseItem.ICRP_ST_CD,
    _response = GetStateProvinceAbbreviationFromCode(Model.IncorporationState).ToString()
});
```

### D. `PRTL_NO_LNGR_EE` numeric mapping

#### Confirmed code table (`no_lngr_ee_rsn_cd` from `EmployerRegistrationCodeTables.xlsx`)

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

#### Current behavior

The relevant UI state is likely captured by `NoEmployeeReason` and/or `HaveEmployeesCurrentlyWorkingInWisconsin` in `PreliminaryQuestions`.

#### LLD change

1. Verify `NoEmployeeReason` enum integer values align with CD_SK above (1–9 in the same order).
2. Cast the selected enum value to `int` and call `.ToString()` — no lookup dictionary needed if enum values match.
3. Serialize the numeric code as `_response` when the item SK is `3027`.

#### Suggested helper

```csharp
private static string MapNoEmployeeReasonToBackendCode(NoEmployeeReason? reason)
{
    return reason.HasValue ? ((int)reason.Value).ToString() : string.Empty;
}
```

### E. Survey response serialization patterns

#### Use integer output consistently

For all affected responses, the final payload should use a string representation of the integer code.

Examples:
- `EXPT_PAY_EE_TIME` => `"1"`, `"2"`, `"3"`, `"4"`, `"5"`
- `ICRP_ST_CD` and `LLC_RGST_ST_CD` => map abbreviation to numeric state SK, then call `ToString()`
- `PRTL_NO_LNGR_EE` => numeric code from the selected reason
- `CX_1500_1IN20_WHN_TXT` => numeric code from the dropdown

## Validation and Verification

### Unit tests

If tests already exist for dropdown bindings or `GetSurveyResponses()`, add cases for:
- numeric option values in `FuturePayPeriodOptions`
- correct serialization of `ExpectedFuturePayrollPeriod`
- numeric mapping for `WhenExpectToPayWagesInAQuarter`
- state abbreviation to numeric code mapping
- `NoEmployeeReason` or equivalent mapping to `PRTL_NO_LNGR_EE`

### Manual verification

1. Select each value in the affected dropdowns.
2. Submit the registration flow.
3. Inspect the generated survey payload and verify that relevant `_response` values are numeric codes.

## Open Questions

- ~~The exact backend mapping for `PRTL_NO_LNGR_EE` must be validated against the spreadsheet.~~ **Resolved** — `no_lngr_ee_rsn_cd` sheet confirms 9 codes (1–9).
- Confirm whether `CX_1500_1IN20_WHN_TXT` is implemented in the current UI and whether it already maps to item `3276`. Its numeric codes are **assumed** to be 1–5 (no sheet in spreadsheet — needs backend confirmation).
- Confirm whether any other dropdowns in the same page also require numeric mapping.

## Notes

- Do not change user-facing dropdown labels.
- Only the underlying option `Value` and serialization should change.
- Preserve the existing component structure and model validation flow.
