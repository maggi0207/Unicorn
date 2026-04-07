# Feature: Move 501(c)(3) Business Category to Step 1

## Story
**Label:** Move business category question to step one.

**Narrative:** As an Employer Services Analyst I would like to move the 501(c)(3) business category selection from step six to step one so that employers are more likely to report the business category accurately.

---

## Acceptance Criteria

### Step 1 — Preliminary Questions

- Add standalone Yes/No question: **"Are you a non-profit organization as described in s.501(c)(3) of the IRS code?"**
- If **Yes** → automatically populate Step 6 business category with `NonProfit_501c3`
- If **Yes** → show conditional sub-questions:

  **Q: Do you have a 501(c)(3) ruling from the IRS? (Yes/No)**
  - **Yes:**
    - Display text: "Please upload a copy of your 501(c)(3) ruling from the IRS."
    - Optional document upload for 501(c)(3) ruling
    - Checkbox: "I will supply required documentation at a later date." (required to continue without upload)

  - **No → Q: Have you applied for 501(c)(3) status with the IRS? (Yes/No)**
    - **Yes:**
      - Display text: "Please upload your articles of incorporation and the application acceptance letter from the IRS."
      - Optional document upload for Articles of Incorporation
      - Optional document upload for IRS Acceptance Letter
      - Checkbox: "I will supply required documentation at a later date."

    - **No:**
      - Display text: "You have indicated that you are a 501(c)(3) employer but have not yet applied for that designation with the IRS. We can only treat you as a 501(c)(3) entity if we have a copy of your 501(c)(3) ruling on file. Please upload a copy of your articles of incorporation. Once your 501(c)(3) application has been accepted by the IRS, please submit a copy of your application acceptance letter."
      - Optional document upload for Articles of Incorporation
      - Checkbox: "I will supply required documentation at a later date."

> **Note:** File uploads are OPTIONAL — the checkbox allows the user to continue without uploading.

---

## Files Changed

### New Files
| File | Purpose |
|------|---------|
| `Features/Shared/FileUpload/Components/FileUpload.razor` | Real file upload component (ported from production) |
| `Features/Shared/FileUpload/Components/Component.razor` | Placeholder stub |
| `Features/Shared/FileUpload/Models/FileUploadService.cs` | Upload result model |
| `Features/Shared/FileUpload/Models/FileUploadState.cs` | Enum: Default, Uploading, Successful, ErrorsMustFix |
| `Features/Shared/FileUpload/Services/UploadServices.cs` | WCF upload service interface + implementation |
| `Features/Shared/FileUpload/Pages/FileUploadPage.razor` | Demo page at /FileUploadPage |

### Modified Files

#### `PreliminaryQuestionsModel.cs`
Added 4 new properties after `BusinessCategory`:
```csharp
public bool? IsNonProfit501c3 { get; set; } = null;
public bool? HasRulingFrom501c3IRS { get; set; } = null;
public bool? HasAppliedFor501c3WithIRS { get; set; } = null;
public bool WillSupplyDocumentationLater { get; set; } = false;
```

#### `PreliminaryQuestions.razor`
- Added new Yes/No question before business category radio group
- Added full 501(c)(3) conditional sub-tree (ruling upload, applied upload, not-applied text)
- Each path uses real `FileUpload` component (replaced `DummyFileUpload`)
- Added `@using UI.EmployerPortal.Web.Features.Shared.FileUpload.Components`

#### `PreliminaryQuestions.razor.cs`
- Added `[Inject] RegistrationStateService`
- Visibility properties:
  ```csharp
  private bool Show501c3SubTree => Model.IsNonProfit501c3 == true;
  private bool ShowRulingUpload => Show501c3SubTree && Model.HasRulingFrom501c3IRS == true;
  private bool ShowHasAppliedQuestion => Show501c3SubTree && Model.HasRulingFrom501c3IRS == false;
  private bool ShowAppliedUpload => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == true;
  private bool ShowNotAppliedText => ShowHasAppliedQuestion && Model.HasAppliedFor501c3WithIRS == false;
  ```
- Added `OnIsNonProfit501c3Changed` handler — resets sub-tree when user changes to No
- Added `OnHasRulingFrom501c3IRSChanged`, `OnHasAppliedFor501c3WithIRSChanged`, `OnWillSupplyDocumentationLaterChanged` handlers
- Added validation for `IsNonProfit501c3`, `HasRulingFrom501c3IRS`, `HasAppliedFor501c3WithIRS`
- `Validate()` saves to `RegistrationState`:
  ```csharp
  RegistrationState.PreliminaryBusinessCategory = Model.IsNonProfit501c3 == true
      ? BusinessCategory.NonProfit_501c3
      : Model.BusinessCategory;
  ```

#### `RegistrationStateService.cs`
Added cross-step state property:
```csharp
public BusinessCategory? PreliminaryBusinessCategory { get; set; }
```

#### `UISubjectivity.razor` (Step 6)
- Section 1: conditionally renders locked disabled radio (when pre-populated from Step 1) vs interactive radio group
- Section 2: shown when `NonProfit_Other` selected; if `HasAppliedFor501c3Status == true` shows redirect message

#### `UISubjectivity.razor.cs` (Step 6)
- Added `[Inject] RegistrationStateService`
- Added `_businessCategoryLockedFromStep1` flag
- `OnInitialized` reads from `RegistrationState.PreliminaryBusinessCategory`
- `BusinessCatagories` list: removed `NonProfit_501c3` (it's auto-populated from Step 1, not user-selectable in Step 6)
- `Section2Visible()` returns true when `BusinessCategory == NonProfit_Other`

---

## UI Flow Summary

```
Step 1 — Preliminary Questions
│
├── "Are you a 501(c)(3) non-profit?" 
│     ├── YES ──► saves NonProfit_501c3 to RegistrationState on submit
│     │           ├── "Do you have a 501(c)(3) ruling?" 
│     │           │     ├── YES ──► Upload: 501(c)(3) Ruling  [optional] + checkbox
│     │           │     └── NO  ──► "Have you applied?"
│     │           │                   ├── YES ──► Upload: Articles + IRS Letter [optional] + checkbox
│     │           │                   └── NO  ──► Disclaimer text + Upload: Articles [optional] + checkbox
│     └── NO  ──► saves Model.BusinessCategory to RegistrationState on submit
│
└── Business Category radio (Commercial / Domestic / Agricultural / NonProfit_501c3 / NonProfit_Other)

Step 6 — UI Subjectivity
│
├── [If RegistrationState.PreliminaryBusinessCategory == NonProfit_501c3]
│     └── Show greyed-out disabled pre-selected "Non-Profit with 501(c)(3) Ruling from IRS"
│
└── [Else] Show interactive radio (without NonProfit_501c3 option)
      └── [If NonProfit_Other selected]
            └── "Have you applied for 501(c)(3) status?"
                  └── YES ──► "Please return to step one to correct your answer"
```

---

## Design Notes (Figma Reference)
- Figma: https://www.figma.com/design/TMov8OdJr7SVWHOoLveiKv/DWD-Portal-Concepts?node-id=4406-115660
- File upload component: two-column layout — left (title + accepted types), right (drag-drop zone)
- Locked NonProfit_501c3 in Step 6: greyed-out disabled radio, not interactive
- All document uploads are optional — checkbox is the mechanism to proceed without uploading
