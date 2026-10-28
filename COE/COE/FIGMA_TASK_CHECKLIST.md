# UIEP-3745 — Figma + Ticket coverage checklist

## Figma screens

| Figma file | Required UI | FE status |
|---|---|---|
| `Initial.png` | Intro: eligibility copy, account/legal/effective date, **NEXT** | Done |
| `Officer Exclusion Calculate Worksheet.png` | Worksheet table, CANCEL, CALCULATE | Done |
| `Officer Exclusion Caluclator Worksheet.png` | Same + **SUBMIT ELECTION** | Done |
| `…inline errors.png` | Top Alert + field **Missing value** | Done |
| `…submit failed.png` | Banner: *Worksheet failed to submit. Please try again.* | Done |
| `…Success.png` | *Your election successfully submitted.* + letter links | Done |
| `…Success with banner.png` | Mail-copy info banner + success + links | Done (mail link shows banner) |
| `…Already visited.png` | Confirmation banner + card + ACCOUNT SUMMARY only | Done |

## Ticket defects / notes

| Item | Status |
|---|---|
| Nothing after successful submit | Done — success screen |
| Can Submit again on same page | Done — success / already-elected leave worksheet |
| No effective date | Done — shown on intro / success / already-elected |
| No Account Summary navigation | Done — button → `manage-account/account-summary` |
| Intro / effective-year screen | Done |
| Already-elected on revisit | Done — `GetCorporateOfficerExclusions` |
| Account Summary (not Complete) | Done |
| View Initial Determination | Done UI — navigates to `ui-documents` (letter may appear when BE generates it) |
| Mail a copy | Done UI — shows Figma mail banner; actual mail send needs BE if required |
| Confirmation ID / Date Submitted | Shown when exclusions API returns SK / dates |
| Dual already-in-effect + suspended errors | Out of scope — UIEP-3750 backend |
| Line 1 With Exclusion `$0.00` | Done (confirm with product if needed) |

## Still needs team (not FE blockers)

1. Eligible UAT account for success path screenshots  
2. Already-elected account for revisit screenshot  
3. Confirm Confirmation ID mapping / mailed-copy backend if mail must actually send  

## Files to paste into office repo

- `Features/ManageAccount/Pages/CorporateOfficerExclusion.razor`
- `Features/ManageAccount/Pages/CorporateOfficerExclusion.razor.cs`
- `Features/ManageAccount/Pages/CorporateOfficerExclusion.razor.css`
- `Features/ManageAccount/Services/CorporateOfficerExclusionService.cs`
- `Features/ManageAccount/Models/COEElectionSummary.cs`
- `Features/ManageAccount/Models/COESaveResponse.cs`
- `Program.cs` (DI line if missing)
