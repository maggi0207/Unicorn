# Meeting Review Summary – AI Scrap Chatbot

## Key Findings

### 1. Location Parameter Issue
- Scrap Details MCP Tool was updated to make location optional.
- Chatbot still behaves as if location is mandatory in some cases.
- Possible causes:
  - Mapping file not updated.
  - Agent routing logic still expecting location.
  - Tool selection logic not using latest configuration.

### 2. MCP Tool Invocation Problems
- MCP tool works independently and locally.
- Chatbot is not consistently invoking the correct MCP tool.
- Agent sometimes stops before reaching MCP execution.
- Intent classification/routing appears to be selecting incorrect tools.

### 3. Inconsistent Intent Classification
- Same query sometimes works and sometimes fails.
- User phrasing significantly affects tool selection.
- More training examples ("golden set") are needed to improve routing accuracy.

### 4. Inventory Response Format Improvement
Current behavior:
- Returns inventory details for each depot separately.

Expected behavior:
- Show consolidated summary first:
  - Total On-Hand
  - Total Forecast
  - LDOS
  - Total Scrap Quantity
  - Number of Depots
- Allow users to drill down into individual depot details.

### 5. Tool Discovery Issue
- Agent is not identifying all available Scrap MCP tools.
- Requests to list tools sometimes trigger Scrap Workflow instead.
- Tool discovery and selection logic require improvement.

### 6. Operational Issues Observed
- MCP tool service occasionally reported as stopped.
- Some requests returned HTTP 400 errors and rate-limit errors.
- Need investigation into timeout handling, deployment consistency, and throttling.

## Action Items

### High Priority
1. Fix tool routing and ensure correct MCP tool invocation.
2. Verify mapping files reflect latest MCP tool definitions.
3. Improve intent classifier with additional examples.
4. Add detailed logging for tool selection and execution.

### Medium Priority
1. Implement consolidated inventory response view.
2. Improve tool discovery mechanism.
3. Investigate HTTP 400 and rate-limit issues.
4. Validate all reviewed use cases end-to-end.

## Overall Assessment
The MCP tools appear largely functional. The primary issues are intent classification, tool routing, and consistency of tool selection. Immediate focus should be on improving intent recognition, logging, and reliable MCP tool execution.
