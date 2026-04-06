# AT Failure Investigator — VS Code Copilot + Claude Architecture

## Overview

One `@at-failure` Chat Participant. Eight thin tools. Domain knowledge injected as context — not called as tools. The LLM reasons through the entire flowchart; your TypeScript code just wires up the plumbing.

---

## 1. Redesigned Architecture at a Glance

```
VS Code Copilot Chat
└── @at-failure  (Chat Participant — ChatRequestHandler)
    │
    ├── BEFORE model call: inject domain context into messages[]
    │   ├── igxl-at-domain.md       (always)
    │   ├── igxl-development.md     (always)
    │   └── rudder-at-domain.md     (only if AT type = Rudder, detected at runtime)
    │
    ├── Model: Claude (claude-sonnet / claude-opus via VS Code LM API)
    │   └── Guided by system prompt (your flowchart as natural language)
    │
    └── Tools (LLM decides when to call these):
        ├── normalize-input
        ├── jarvis-query
        ├── codesearch
        ├── tsrx-scan
        ├── pattern-match
        ├── myinfo-query
        ├── write-pattern
        └── escalate
```

---

## 2. The System Prompt  ← paste this into your ChatRequestHandler

```
You are **AT Failure Investigator**, an expert diagnostics agent for IG-XL
automated test failures at Teradyne. You operate inside VS Code Copilot.

Your job: given a Suite ID, Version, and Environment, determine the root
cause of an AT failure and recommend a precise fix path.

You have access to 8 tools. You MUST follow the investigation protocol below
exactly. Do not skip steps. Do not call tools outside their defined budget.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
TOOL BUDGET (hard limits — never exceed)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
• normalize-input  : 1 call (always first)
• jarvis-query     : max 2 calls  (Step 1a + Step 4b)
• codesearch       : max 3 calls  (Step 1b + Step 4d, shared budget)
• tsrx-scan        : max 2 calls  (Step 1c + Step 4c)
• pattern-match    : max 1 call
• myinfo-query     : max 1 call
• write-pattern    : max 1 call   (only if new pattern discovered)
• escalate         : max 1 call   (only if root cause NOT found after Step 4)

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
INVESTIGATION PROTOCOL
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━

STEP 0 — INPUT NORMALIZATION
Call normalize-input before any other tool.
Rules:
  - Version: append platform suffix if missing
      99.99.92       → 99.99.92_uflx
      99.99.92_uflx  → unchanged
  - Environment: expand abbreviations
      Win11          → Windows11
      win10          → Windows10
      uflx / UFLX   → uFlexPlus
If normalization is ambiguous, use the closest known canonical form and note
the assumption in your report.

STEP 1a — JARVIS QUERY
Call jarvis-query with normalized suite_id, version, environment.
Evaluate the result:
  - If version AND environment MATCH exactly: confidence ceiling = HIGH
  - If EITHER mismatches: use 90-day recovery window parameter = true,
    confidence ceiling = MEDIUM (cannot be raised regardless of later findings)
Extract from result: at_folder_path, metadata, log_text_snippet.
If jarvis-query returns no results: note this and proceed to Step 4 directly.

STEP 1b — RESOLVE AT SOURCE
Call codesearch to fetch the _testdesc file and entry script (.bat / .pl / macro).
  - Inspect entry script to determine AT type (Rudder or standard IG-XL).
  - If AT type = Rudder: the Rudder domain knowledge has been injected into
    your context above. Apply it from this point forward.
  - Max 2 codesearch calls in this step (counts toward your 3-call budget).

STEP 1c — TSRX ERROR SCAN
Call tsrx-scan with the at_folder_path from Step 1a.
Use ONE combined regex pattern — do not make multiple scan calls in this step.
Limit results to first 40 lines.
This is your PRIMARY error signal. Extract: error tokens, error codes, module names.

STEP 2 — PATTERN LIBRARY MATCH
Call pattern-match with the error tokens from Step 1c.
The tool searches patterns/confirmed/ and patterns/pending/ automatically.
Result will contain: matched_pattern (or null), confidence_tier, recommended_action.
  - If match found: go to Step 3.
  - If no match: go to Step 4.

STEP 3 — CONFIRM VIA MYINFO
Call myinfo-query to confirm the matched pattern against known IG-XL product
behavior and current product state.
  - Use the pattern's recommended_action as your query context.
  - Myinfo result either confirms or weakens the confidence tier.
  - After myinfo-query: proceed to REPORT.

STEP 4 — DEEP INVESTIGATION (no pattern match)
You have a remaining budget of up to 3 tool calls. Use them in this order,
stopping as soon as root cause is identified:
  4b: Call jarvis-query again — request extended log details (set detail=full).
  4c: Call tsrx-scan with an alternate log path (check sibling directories).
  4d: Call codesearch targeting IG-XL product source for the failing module.
After each call, reason about whether root cause is now clear.
  - If root cause found: go to Step 3 (myinfo-query to confirm), then REPORT.
  - If root cause NOT found after all 4 calls: call escalate, then REPORT
    with confidence = LOW and escalation noted.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
CONFIDENCE RULES
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
HIGH   — version+env match AND confirmed pattern AND myinfo validation passed
MEDIUM — any one of: version/env mismatch, unconfirmed pattern, no myinfo
LOW    — root cause inferred without pattern match, or from Step 4 only

A MEDIUM ceiling (set in Step 1a) cannot be upgraded. All downstream
confirmations keep confidence at MEDIUM.

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
REPORT FORMAT (always end with this)
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
## AT Failure Report

**Suite ID:** {suite_id}
**Version:** {normalized_version}
**Environment:** {normalized_env}
**Confidence:** HIGH | MEDIUM | LOW

### Root Cause
{One clear sentence describing the root cause}

### Evidence
- {Finding from Step 1a}
- {Finding from Step 1c}
- {Pattern match result or deep investigation finding}
- {MyInfo confirmation or lack thereof}

### Recommended Action
{Specific, actionable fix path — file, function, or config to change}

### New Pattern Detected?
{Yes / No}
{If Yes: I will call write-pattern to document this for future investigations.}

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
IMPORTANT BEHAVIORS
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
- Never hallucinate file paths, versions, or error codes. Only report what
  tool results contain.
- If a tool returns an error or empty result, note it explicitly and adjust
  your investigation path accordingly.
- Do not ask the user clarifying questions mid-investigation. Proceed with
  reasonable assumptions and state them in the report.
- Always call write-pattern if your report identifies a genuinely new failure
  pattern not present in patterns/confirmed/ or patterns/pending/.
```

---

## 3. Tool Schemas (JSON — register with vscode.lm.registerTool)

### normalize-input
```json
{
  "name": "normalize-input",
  "description": "Normalizes version and environment strings to canonical IG-XL forms before investigation begins.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "version": { "type": "string", "description": "Raw version string, e.g. '99.99.92'" },
      "environment": { "type": "string", "description": "Raw environment string, e.g. 'Win11'" }
    },
    "required": ["version", "environment"]
  }
}
```

### jarvis-query
```json
{
  "name": "jarvis-query",
  "description": "Queries Jarvis for AT test results metadata and log text. Supports exact and 90-day recovery window queries.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "suite_id":         { "type": "string" },
      "version":          { "type": "string", "description": "Normalized version string" },
      "environment":      { "type": "string", "description": "Normalized environment string" },
      "recovery_window":  { "type": "boolean", "default": false, "description": "Set true if version/env mismatch — expands search to 90 days" },
      "detail":           { "type": "string", "enum": ["summary", "full"], "default": "summary" }
    },
    "required": ["suite_id", "version", "environment"]
  }
}
```

### codesearch
```json
{
  "name": "codesearch",
  "description": "Searches the IG-XL codebase. Use to fetch _testdesc files, entry scripts (.bat/.pl/macro), or product source for a failing module. Budget: 3 calls total across the entire investigation.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query":    { "type": "string", "description": "Search query or file path pattern" },
      "target":   { "type": "string", "enum": ["testdesc", "entry_script", "product_source"], "description": "What to look for" },
      "module":   { "type": "string", "description": "Optional: specific module name to scope the search" }
    },
    "required": ["query", "target"]
  }
}
```

### tsrx-scan
```json
{
  "name": "tsrx-scan",
  "description": "Runs a combined regex Select-String scan on AT log files. Returns first 40 matching lines. Primary error signal source.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "at_folder_path": { "type": "string", "description": "Path to the AT results folder from Jarvis" },
      "error_patterns": {
        "type": "array",
        "items": { "type": "string" },
        "description": "Regex patterns to scan for. These are combined into one Select-String call."
      },
      "alternate_path": { "type": "string", "description": "Optional: sibling/alternate log path for Step 4c fallback" }
    },
    "required": ["at_folder_path", "error_patterns"]
  }
}
```

### pattern-match
```json
{
  "name": "pattern-match",
  "description": "Matches error tokens against the known pattern library (patterns/confirmed/ and patterns/pending/). Returns best match with confidence tier and recommended action.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "error_tokens": {
        "type": "array",
        "items": { "type": "string" },
        "description": "Error tokens, codes, and module names extracted from TSRX scan"
      },
      "at_type": { "type": "string", "enum": ["igxl", "rudder"], "description": "AT type determined from entry script" }
    },
    "required": ["error_tokens"]
  }
}
```

### myinfo-query
```json
{
  "name": "myinfo-query",
  "description": "Queries the MyInfo Copilot API to confirm IG-XL product behavior against a matched pattern or investigation finding.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "query":           { "type": "string", "description": "Natural language question about IG-XL product behavior" },
      "pattern_id":      { "type": "string", "description": "Optional: pattern ID from pattern-match result for targeted lookup" },
      "product_context": { "type": "string", "description": "Optional: module or feature area to scope the query" }
    },
    "required": ["query"]
  }
}
```

### write-pattern
```json
{
  "name": "write-pattern",
  "description": "Documents a newly discovered failure pattern to patterns/pending/ for future investigations.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "pattern_name":     { "type": "string" },
      "error_tokens":     { "type": "array", "items": { "type": "string" } },
      "root_cause":       { "type": "string" },
      "recommended_action": { "type": "string" },
      "at_type":          { "type": "string", "enum": ["igxl", "rudder", "both"] },
      "evidence_summary": { "type": "string" }
    },
    "required": ["pattern_name", "error_tokens", "root_cause", "recommended_action"]
  }
}
```

### escalate
```json
{
  "name": "escalate",
  "description": "Escalates an unresolved AT failure to the at-failure-test-automation team. Called only when root cause cannot be determined after full investigation.",
  "inputSchema": {
    "type": "object",
    "properties": {
      "suite_id":          { "type": "string" },
      "version":           { "type": "string" },
      "environment":       { "type": "string" },
      "investigation_log": { "type": "string", "description": "Summary of all steps attempted and what each returned" },
      "escalation_team":   { "type": "string", "enum": ["at-failure-test-automation", "at-failure-debug"], "default": "at-failure-test-automation" }
    },
    "required": ["suite_id", "version", "environment", "investigation_log"]
  }
}
```

---

## 4. TypeScript Extension Scaffold (extension.ts)

```typescript
import * as vscode from 'vscode';
import * as fs from 'fs';
import * as path from 'path';

// ─── Domain Knowledge Loader ────────────────────────────────────────────────
// Load these ONCE at activation. Inject into messages[] before every model call.
// These are NOT tools — they are static context files.

const SKILLS_DIR = path.join(__dirname, '..', 'skills');

function loadDomainContext(filename: string): string {
  try {
    return fs.readFileSync(path.join(SKILLS_DIR, filename), 'utf-8');
  } catch {
    return ''; // graceful degradation if file missing
  }
}

const igxlAtDomain     = loadDomainContext('igxl-at-domain.md');
const igxlDevelopment  = loadDomainContext('igxl-development.md');
const rudderAtDomain   = loadDomainContext('rudder-at-domain.md');

// ─── System Prompt ───────────────────────────────────────────────────────────
// Paste the full system prompt from Section 2 of this document here.
const SYSTEM_PROMPT = `[PASTE SYSTEM PROMPT FROM SECTION 2 HERE]`;

// ─── Tool Implementations ────────────────────────────────────────────────────
// Each tool maps 1:1 to a JSON schema above. Keep these thin — no branching.

const tools: vscode.LanguageModelChatTool[] = [
  {
    name: 'normalize-input',
    description: 'Normalizes version and environment strings to canonical IG-XL forms.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'jarvis-query',
    description: 'Queries Jarvis for AT test results metadata and log text.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'codesearch',
    description: 'Searches the IG-XL codebase for testdesc, entry scripts, or product source.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'tsrx-scan',
    description: 'Runs a combined regex scan on AT log files.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'pattern-match',
    description: 'Matches error tokens against the known pattern library.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'myinfo-query',
    description: 'Queries the MyInfo Copilot API to confirm IG-XL product behavior.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'write-pattern',
    description: 'Documents a newly discovered failure pattern.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
  {
    name: 'escalate',
    description: 'Escalates an unresolved AT failure to the test-automation team.',
    inputSchema: { /* paste schema from Section 3 */ } as any,
  },
];

// ─── Tool Handler ────────────────────────────────────────────────────────────
// Route tool calls to your actual MCP/service implementations.

async function handleToolCall(
  toolName: string,
  toolInput: Record<string, unknown>
): Promise<string> {
  switch (toolName) {
    case 'normalize-input':
      return JSON.stringify(normalizeInput(toolInput));

    case 'jarvis-query':
      // Call your existing mcp_teradyne-jarv_jarvis_data MCP here
      return JSON.stringify(await callJarvisMcp(toolInput));

    case 'codesearch':
      return JSON.stringify(await callCodesearch(toolInput));

    case 'tsrx-scan':
      return JSON.stringify(await runTsrxScan(toolInput));

    case 'pattern-match':
      return JSON.stringify(await matchPatterns(toolInput));

    case 'myinfo-query':
      // Call your existing mcp_myinfo-copilo_teradyne_myinfo_copilot_query MCP here
      return JSON.stringify(await callMyInfoMcp(toolInput));

    case 'write-pattern':
      return JSON.stringify(await writePattern(toolInput));

    case 'escalate':
      return JSON.stringify(await escalate(toolInput));

    default:
      return JSON.stringify({ error: `Unknown tool: ${toolName}` });
  }
}

// ─── Chat Participant ─────────────────────────────────────────────────────────

export function activate(context: vscode.ExtensionContext) {

  const participant = vscode.chat.createChatParticipant(
    'at-failure',
    async (
      request: vscode.ChatRequest,
      chatContext: vscode.ChatContext,
      stream: vscode.ChatResponseStream,
      token: vscode.CancellationToken
    ) => {
      // 1. Determine if Rudder AT (quick pre-scan of the user message)
      const isRudderAT = request.prompt.toLowerCase().includes('rudder');

      // 2. Build messages — inject domain knowledge BEFORE model call
      const messages: vscode.LanguageModelChatMessage[] = [
        // System prompt with investigation protocol
        vscode.LanguageModelChatMessage.User(SYSTEM_PROMPT),

        // Always-on domain context
        vscode.LanguageModelChatMessage.User(
          `## IG-XL AT Domain Knowledge\n${igxlAtDomain}`
        ),
        vscode.LanguageModelChatMessage.User(
          `## IG-XL Development Knowledge\n${igxlDevelopment}`
        ),

        // Conditionally inject Rudder context
        ...(isRudderAT
          ? [vscode.LanguageModelChatMessage.User(
              `## Rudder AT Domain Knowledge\n${rudderAtDomain}`
            )]
          : []),

        // The actual user request
        vscode.LanguageModelChatMessage.User(request.prompt),
      ];

      // 3. Select model (prefer Claude via VS Code LM API)
      const [model] = await vscode.lm.selectChatModels({
        vendor: 'copilot',
        family: 'claude-sonnet-4-5', // or claude-opus-4-5
      });

      if (!model) {
        stream.markdown('❌ No Claude model available. Check your Copilot plan.');
        return;
      }

      // 4. Agentic tool-calling loop
      let continueLoop = true;

      while (continueLoop && !token.isCancellationRequested) {
        const response = await model.sendRequest(messages, { tools }, token);

        let hasToolCall = false;
        let assistantContent = '';

        for await (const chunk of response.stream) {
          if (chunk instanceof vscode.LanguageModelTextPart) {
            assistantContent += chunk.value;
            stream.markdown(chunk.value); // stream text to user in real time
          } else if (chunk instanceof vscode.LanguageModelToolCallPart) {
            hasToolCall = true;

            // Show the user which tool is being called
            stream.progress(`🔧 Calling ${chunk.name}...`);

            // Execute the tool
            const toolResult = await handleToolCall(
              chunk.name,
              chunk.input as Record<string, unknown>
            );

            // Append assistant message + tool result to messages for next loop
            messages.push(
              vscode.LanguageModelChatMessage.Assistant([
                new vscode.LanguageModelTextPart(assistantContent),
                new vscode.LanguageModelToolCallPart(chunk.callId, chunk.name, chunk.input),
              ])
            );
            messages.push(
              vscode.LanguageModelChatMessage.User([
                new vscode.LanguageModelToolResultPart(chunk.callId, toolResult),
              ])
            );

            assistantContent = ''; // reset for next iteration
          }
        }

        // Stop looping when model produces only text (final report)
        continueLoop = hasToolCall;
      }
    }
  );

  participant.iconPath = new vscode.ThemeIcon('bug');
  context.subscriptions.push(participant);
}

// ─── Stub implementations (replace with your real MCP calls) ─────────────────

function normalizeInput(input: Record<string, unknown>) {
  // Your normalization logic here
  return { normalized_version: input.version, normalized_environment: input.environment };
}

async function callJarvisMcp(input: Record<string, unknown>) {
  // Wire to: mcp_teradyne-jarv_jarvis_data
  return { at_folder_path: '', metadata: {}, log_text_snippet: '' };
}

async function callCodesearch(input: Record<string, unknown>) {
  // Wire to your codesearch MCP
  return { results: [] };
}

async function runTsrxScan(input: Record<string, unknown>) {
  // Execute Select-String via shell or MCP
  return { lines: [] };
}

async function matchPatterns(input: Record<string, unknown>) {
  // Scan patterns/confirmed/ and patterns/pending/
  return { matched_pattern: null, confidence_tier: 'LOW', recommended_action: '' };
}

async function callMyInfoMcp(input: Record<string, unknown>) {
  // Wire to: mcp_myinfo-copilo_teradyne_myinfo_copilot_query
  return { confirmation: '', confidence_adjustment: 'none' };
}

async function writePattern(input: Record<string, unknown>) {
  // Write to patterns/pending/<pattern_name>.json
  return { written: true, path: `patterns/pending/${input.pattern_name}.json` };
}

async function escalate(input: Record<string, unknown>) {
  // Create issue / notify team channel
  return { escalated: true, team: input.escalation_team };
}
```

---

## 5. File & Folder Structure

```
your-extension/
├── src/
│   └── extension.ts          ← scaffold above
├── skills/
│   ├── igxl-at-domain.md     ← injected as context (not a tool)
│   ├── igxl-development.md   ← injected as context (not a tool)
│   └── rudder-at-domain.md   ← injected conditionally
├── patterns/
│   ├── confirmed/             ← curated, validated patterns
│   └── pending/               ← newly discovered patterns (from write-pattern)
├── package.json
└── tsconfig.json
```

### package.json — Chat Participant Registration
```json
{
  "contributes": {
    "chatParticipants": [
      {
        "id": "at-failure",
        "name": "at-failure",
        "description": "Diagnoses IG-XL AT failures and recommends fix paths",
        "isSticky": true
      }
    ]
  }
}
```

---

## 6. Key Design Decisions Summary

| Decision | Rationale |
|---|---|
| Domain knowledge injected, not called as tools | Avoids wasting tool-call budget on static reads; model sees it before reasoning begins |
| Single chat participant, no sub-agents | VS Code Copilot has no sub-agent primitive; the LLM's tool-calling loop IS the agent |
| Tool budget enforced in system prompt, not code | LLM follows budget instructions reliably; TypeScript code stays simple |
| Rudder detection before model call | Avoids a round-trip; inject rudder context proactively if suite hints at Rudder AT |
| Agentic loop stops when no tool calls | Model produces the final report as pure text — that's your exit condition |
| Patterns stored as files, not DB | write-pattern is a simple file write; codesearch can scan them natively |
