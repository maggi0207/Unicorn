# Office build — current fix (duplicate class)

## Error you have now
`CS0101: namespace already contains a definition for 'COEElectionSummary'`

**Cause:** class exists in **two** files:
1. `Models/COEElectionSummary.cs`
2. Inside `Models/COESaveResponse.cs` (from earlier merge attempt)

## Fix in office (30 seconds)
1. Keep `COEElectionSummary.cs` (your open file is fine)
2. Overwrite `COESaveResponse.cs` from Unicorn again — it now has **only** `COESaveResponse` (no nested `COEElectionSummary`)
3. Overwrite `CorporateOfficerExclusionService.cs` (nullable warnings fixed)
4. Rebuild

## Expected result
- CS0101 gone  
- CS8600 / CS8604 gone  
- Build succeeds (IDE0046 warnings are optional style only)
