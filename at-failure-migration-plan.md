# Migration Plan: .github/skills/*.md → VS Code Copilot Chat Participant

## What You Have vs What You Need

```
BEFORE (current)                        AFTER (target)
────────────────────────────────────    ────────────────────────────────────
.github/                                your-extension/
  skills/                               ├── src/
    igxl-at-domain.md          ──┐      │   └── extension.ts   ← NEW
    igxl-development.md          │      ├── skills/
    rudder-at-domain.md        ──┘      │   ├── igxl-at-domain.md     (moved)
    jarvis-get-testresults.md  ──┐      │   ├── igxl-development.md   (moved)
    codesearch.md                │      │   └── rudder-at-domain.md   (moved)
    tsrx-error-scan.md           │→     ├── patterns/
    pattern-library-match.md     │      │   ├── confirmed/            (existing)
    myinfo-query.md              │      │   └── pending/              (existing)
    create-pattern.md            │      ├── package.json              ← NEW
    escalate.md                ──┘      └── tsconfig.json             ← NEW

  ↑ These become TypeScript tools in extension.ts
  ↑ These stay as .md but move to skills/ folder
```

---

## Phase 1 — Classify Your Existing .md Skills

Every `.github/skills/*.md` file falls into one of two buckets:

### Bucket A — Domain Knowledge (keep as .md, inject as context)
These are files that contain background knowledge, terminology, rules, or
domain-specific facts that help the model reason. They do NOT describe
a sequence of actions with inputs/outputs.

**Signals that a skill is Bucket A:**
- Contains terminology definitions or glossaries
- Describes product behavior, known bugs, version history
- Has "know that..." or "understand that..." language
- No clear input/output structure

**Your Bucket A skills (likely):**
- `igxl-at-domain.md`
- `igxl-development.md`
- `rudder-at-domain.md`

### Bucket B — Tool Wrappers (convert to TypeScript tools)
These are files that describe a specific action: call an API, run a command,
search something, write a file. They have implicit inputs and outputs.

**Signals that a skill is Bucket B:**
- Describes calling an MCP, API, or shell command
- Has "given X, return Y" structure
- Mentions specific parameters, file paths, or query formats
- Describes retry logic, limits, or budgets

**Your Bucket B skills (likely):**
- `jarvis-get-testresults.md` → `jarvis-query` tool
- `codesearch.md` → `codesearch` tool
- `tsrx-error-scan.md` → `tsrx-scan` tool
- `pattern-library-match.md` → `pattern-match` tool
- `myinfo-query.md` → `myinfo-query` tool
- `create-pattern.md` → `write-pattern` tool
- `escalate.md` → `escalate` tool

---

## Phase 2 — Migration Steps

### Step 1: Create the extension project skeleton
```bash
mkdir at-failure-extension && cd at-failure-extension
npm init
npm install --save-dev @types/vscode typescript
mkdir src skills patterns/confirmed patterns/pending
```

### Step 2: Move Bucket A skills
Copy your domain knowledge .md files directly — no changes needed.
```bash
cp .github/skills/igxl-at-domain.md    ./skills/
cp .github/skills/igxl-development.md  ./skills/
cp .github/skills/rudder-at-domain.md  ./skills/
```

### Step 3: Convert Bucket B skills → TypeScript tools
For each Bucket B .md file, you extract three things:
1. **Tool name** — short kebab-case identifier
2. **Tool description** — what it does in one sentence (for the LLM)
3. **Input schema** — what parameters it needs (inferred from the .md)

Then you wire the implementation to your existing MCP connections.

### Step 4: Build the system prompt
The system prompt = your flowchart logic expressed as natural language.
Use the system prompt from `at-failure-copilot-architecture.md` (Section 2).
No changes needed — it was designed for this exact stack.

### Step 5: Wire the ChatRequestHandler
Use the extension.ts scaffold from `at-failure-copilot-architecture.md`
(Section 4). Replace the stub functions with your real MCP calls.

### Step 6: Register in package.json
Add the `chatParticipants` contribution point (Section 5 of architecture doc).

---

## Phase 3 — MCP Connection Mapping

You already have all 4 integrations working. Here's how they map:

| Existing MCP handle | Maps to tool | Stub function to replace |
|---|---|---|
| `mcp_teradyne-jarv_jarvis_data` | `jarvis-query` | `callJarvisMcp()` |
| `mcp_myinfo-copilo_teradyne_myinfo_copilot_query` | `myinfo-query` | `callMyInfoMcp()` |
| Your codesearch integration | `codesearch` | `callCodesearch()` |
| `patterns/confirmed/` + `patterns/pending/` | `pattern-match` + `write-pattern` | `matchPatterns()` + `writePattern()` |

The key difference from before: instead of a skill .md file instructing the
model to call these MCPs, the TypeScript tool implementation calls them
directly and returns structured JSON. The model just decides WHEN to call
the tool — not HOW.

---

## Phase 4 — Validation Checklist

Before going live, verify:
- [ ] `@at-failure` participant appears in VS Code Copilot Chat sidebar
- [ ] Typing `@at-failure <suite_id> <version> <env>` triggers the handler
- [ ] Tool calls appear in the VS Code Output panel (add `console.log` in each tool)
- [ ] Domain knowledge .md files are being loaded (log their char count at activation)
- [ ] Rudder context is only injected when the prompt contains rudder-related terms
- [ ] Report format matches the template in the system prompt
- [ ] `write-pattern` creates files in `patterns/pending/` correctly
- [ ] `escalate` notifies the right team channel

---

---

# The Copilot + Claude Conversion Prompt

## How to use this prompt

1. Open a new Copilot Chat in VS Code
2. Make sure Claude (claude-sonnet or claude-opus) is selected as the model
3. Attach all your `.github/skills/*.md` files using `#file:` references
4. Paste the prompt below

---

## PASTE THIS INTO COPILOT CHAT ↓

```
You are a senior VS Code extension architect. I have an existing set of
skill files in `.github/skills/` that define an AT failure investigation
workflow for IG-XL automated tests at Teradyne.

I need you to convert this project into a VS Code Copilot Chat Participant
using the Claude model via the VS Code Language Model API.

## My existing skill files (attached above with #file:)
[list them here, e.g.]
#file:.github/skills/igxl-at-domain.md
#file:.github/skills/igxl-development.md
#file:.github/skills/rudder-at-domain.md
#file:.github/skills/jarvis-get-testresults.md
#file:.github/skills/codesearch.md
#file:.github/skills/tsrx-error-scan.md
#file:.github/skills/pattern-library-match.md
#file:.github/skills/myinfo-query.md
#file:.github/skills/create-pattern.md
#file:.github/skills/escalate.md

## Target architecture (follow this exactly)

### Rules
1. Skills that contain domain knowledge, terminology, or background facts
   → Keep as .md files in a `skills/` folder. Load them in the
   ChatRequestHandler and inject them into the messages[] array BEFORE
   calling the model. These are NOT tools.

2. Skills that describe a specific action (call MCP, run command, search,
   write file) → Convert to VS Code LM tools registered via the tools[]
   array passed to model.sendRequest(). Each tool must have a tight JSON
   input schema.

3. The orchestration logic (step sequence, gates, confidence rules, budgets)
   → Express entirely as a system prompt in natural language. Do NOT encode
   this logic in TypeScript. The LLM follows the system prompt.

4. Use an agentic tool-calling loop:
   - Call model.sendRequest() with the tools[] array
   - Iterate response.stream chunks
   - If LanguageModelToolCallPart: execute the tool, append result to
     messages[], loop again
   - If LanguageModelTextPart only: final answer reached, exit loop

5. Model selection: prefer Claude via:
   vscode.lm.selectChatModels({ vendor: 'copilot', family: 'claude-sonnet-4-5' })

### My existing MCP connections (already working — just wire them up):
- Jarvis: mcp_teradyne-jarv_jarvis_data
- MyInfo: mcp_myinfo-copilo_teradyne_myinfo_copilot_query
- Codesearch: [your codesearch MCP handle]
- Pattern library: local files at patterns/confirmed/ and patterns/pending/

## What I need you to produce

### Output 1: Skill classification table
Read each attached .md file and classify it as:
- DOMAIN KNOWLEDGE → stays as .md, will be injected as context
- TOOL WRAPPER → will become a TypeScript tool

For each TOOL WRAPPER, extract:
- Proposed tool name (kebab-case)
- One-sentence tool description
- Input parameters (name, type, required/optional, description)
- What MCP or function it maps to

### Output 2: Complete extension.ts
Generate the full TypeScript file including:
- All tool definitions with their inputSchema (JSON Schema format)
- The ChatRequestHandler with domain context injection
- The agentic tool-calling loop
- Tool handler switch statement with stub implementations
  (I will replace stubs with my real MCP calls)
- The system prompt as a const string, with:
    - Investigation steps 0-4 from my flowchart
    - Tool budget limits per tool
    - Confidence rules (HIGH/MEDIUM/LOW)
    - Report output format

### Output 3: Updated folder structure
Show me exactly where each file goes in the new project, e.g.:
  skills/igxl-at-domain.md
  skills/igxl-development.md
  ...
  src/extension.ts
  patterns/confirmed/
  patterns/pending/
  package.json

### Output 4: package.json snippet
The chatParticipants contribution point registration for @at-failure.

## Constraints
- TypeScript only (no Python, no shell scripts in the extension itself)
- Each tool must be thin — no branching logic inside tool implementations
- Tool descriptions must be specific enough for Claude to know exactly
  when to call each one (Claude selects tools based on description quality)
- The system prompt must encode ALL flowchart logic — gates, retries,
  confidence rules, escalation conditions
- Do not generate tests or documentation files unless I ask
- Produce Output 1 first, wait for my confirmation, then produce Outputs 2-4

Begin with Output 1.
```

---

## After Copilot produces Output 1 — Review Checklist

Before saying "looks good, continue to Output 2", verify:

**Domain Knowledge classification:**
- [ ] All three domain .md files (igxl-at-domain, igxl-development, rudder-at-domain)
  are classified as DOMAIN KNOWLEDGE, not tools
- [ ] No tool is classified as domain knowledge by mistake

**Tool Wrapper classification:**
- [ ] Every skill that calls an MCP or runs a command is listed as a TOOL WRAPPER
- [ ] Each tool has a clear, tight input schema (not just `object: {}`)
- [ ] Tool descriptions are specific — "Queries Jarvis for AT test results given
  suite_id, version, and environment" is good; "Gets data" is not good

**If anything looks wrong:**
Tell Copilot: "Reclassify [skill name] as [DOMAIN KNOWLEDGE / TOOL WRAPPER]
because [reason]. Update the table and continue."

Once Output 1 is confirmed, tell Copilot: **"Output 1 looks correct. Generate Output 2."**

---

## Pro Tips for the Conversion

**Tip 1 — If a skill .md has both domain knowledge AND action steps:**
Split it. Extract the background facts into a domain .md file, and the
action instructions become the tool description + system prompt language.

**Tip 2 — Tool descriptions are the most important thing Claude reads:**
Claude picks which tool to call entirely based on the description. Make it
specific. Include: what it does, when to call it, what it returns.

Good: "Runs a combined regex Select-String scan on AT log files at the given
folder path. Returns the first 40 matching lines. Use this as the primary
error signal source after obtaining the AT folder path from Jarvis."

Bad: "Scans logs for errors."

**Tip 3 — Keep your existing .md content intact:**
Don't rewrite your domain knowledge files. They already contain hard-earned
knowledge. Just move them from `.github/skills/` to `skills/`. The model
will read them exactly as written.

**Tip 4 — Test tool descriptions with a quick sanity check:**
After generating extension.ts, open Copilot Chat, type:
`@at-failure suite ABC version 99.99.92 env Win11`
Watch the VS Code Output panel. If the model calls tools in the wrong order
or skips steps, the system prompt needs tightening — not the TypeScript.

**Tip 5 — Migrate one tool at a time:**
Start with `jarvis-query` since it's the first real tool call (Step 1a).
Get it returning real data before wiring up the rest. This lets you validate
the tool-calling loop is working before the full flow is live.
