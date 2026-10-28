# Team ask — UIEP-3745 COE (copy/paste ready)

## Summary
Front-end for Corporate Officer Exclusion election end messaging is implemented in the Unicorn working copy. We need UAT accounts and two clarifications to finish verification.

## Please provide

### 1. Test accounts
| Need | Why |
|---|---|
| **Eligible** newly subject Corporation / LLC-as-Corp (payroll under $500k, still inside election window) | Full success path: Intro → Worksheet → Submit → confirmation |
| Account that **already has COE in effect** | Revisit / already-elected confirmation screen |

Helen’s tip still applies: register a corp that activates on submit, then take COE before initial report due date or within 30 days of Initial Determination.

### 2. Clarifications
1. **Confirmation ID** — Is `CorporateOfficerExclusionSK` from `GetCorporateOfficerExclusions` acceptable for the Figma “Confirmation ID” field, or will Mendota add `ConfirmationId` / `DateSubmitted` on `COEResponse`?
2. **Initial Determination links** — How should “View Your Initial Determination letter” and “Mail a copy…” be wired (UI Documents deep link, correspondence API, etc.)? FE can add links once we have the target.

### 3. Related backend
- Contradictory “already in effect” + “suspended” errors → **UIEP-3750** (not FE).

## FE status (ready for office paste)
- Intro screen + NEXT  
- Worksheet: CALCULATE, must-calculate banner, submit-failed banner, inline missing-value validation  
- Success / already-elected confirmation + ACCOUNT SUMMARY → account-summary  
- Letter view/mail links: **deferred** until clarification #2  
