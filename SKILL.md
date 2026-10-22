---
description: Mandatory coding and review rules for preserving existing
  behavior, following repository patterns, minimizing refactoring, and
  verifying AI-generated changes. Use for every coding, debugging,
  refactoring, testing, or code-review task.
name: coding-pattern-enforcement
---

# Coding Pattern Enforcement Skill

## Purpose

Prevent regressions and unnecessary changes by requiring evidence-based
repository analysis, behavior preservation, minimal scope, meaningful
tests, and careful review of AI-generated code.

## Required workflow

### Phase 1 --- Inspect before editing

1.  Read repository-level instructions, this skill, relevant ticket
    requirements, and nearby code.
2.  Trace the existing execution path and identify the actual owner of
    each responsibility (UI, API/controller, service, repository,
    database, generated client).
3.  Inspect similar working features and tests. Follow local conventions
    rather than introducing a new architecture without a requirement.
4.  Establish current behavior, expected behavior, dependencies, and the
    smallest sufficient change.
5.  Verify paths, class names, method names, schema, API contracts, and
    generated-client workflows from repository evidence. Never invent
    them.
6.  If a required schema change, business decision, API contract, or
    other dependency is unavailable, report the exact blocker and do not
    implement a guessed substitute.

### Phase 2 --- Implement narrowly

1.  Make only changes required by the ticket or explicitly requested
    task.
2.  Preserve existing behavior unless a requirement explicitly changes
    it.
3.  Do not remove or omit existing conditions, assignments, queries,
    validation, persistence calls, error handling, logging,
    authorization, or side effects without verifying that the change is
    intentional and safe.
4.  Avoid unrelated refactoring, formatting churn, renaming, and
    cleanup.
5.  Reuse existing methods when their responsibilities and behavior
    genuinely match. Do not merge methods just because they look similar
    if their behavior or contracts differ.
6.  Do not create helper methods for simple checks solely to make them
    testable. Extract a method when it isolates meaningful logic that
    benefits from independent testing or improves an established
    pattern.
7.  Do not use `dynamic`, `any`, casts, disabled checks, or weakened
    types merely to silence compiler errors. Fix the underlying type or
    follow a verified local convention.
8.  Use braces for conditional blocks, including single-line `if`
    statements, consistent with reviewer feedback.
9.  Use precise domain terminology from the ticket and codebase. Do not
    expand acronyms or invent business meanings.
10. Keep generated code generated. Update generated clients using the
    repository's documented workflow and authoritative API
    specifications; do not manually duplicate generated definitions.

### Phase 3 --- Data access and API behavior

1.  Follow the existing database, ORM, transaction, authorization,
    error-response, and API conventions.
2.  Ensure database queries are translatable by the actual provider.
    Avoid unnecessary transformations such as `ToLower()` when database
    collation already provides the intended comparison or when
    transformations could prevent index use.
3.  Consider nullable values and missing records. Do not assume data is
    always present.
4.  Respect foreign-key constraints and relationships. Do not add
    cascade behavior, delete records, or change persistence semantics
    without confirmed requirements.
5.  Keep multi-step data changes atomic when the operation requires it,
    following existing transaction patterns.
6.  Preserve existing endpoint permissions when changing authorization
    for a specific operation.
7.  Validate behavior at the appropriate boundary and return errors
    consistent with the existing application.

### Phase 4 --- Tests

1.  Inspect existing test patterns before adding tests.
2.  Test behavior through public controller/service methods where
    practical; do not expose implementation details solely for testing.
3.  Cover the relevant success path, validation/error paths, missing
    data, authorization, and persistence/dependency behavior.
4.  Add regression tests for bugs fixed by the change.
5.  Do not write tests that merely mirror implementation details or
    assert behavior not required by the ticket.
6.  Run the relevant tests and builds where possible. If unable to run
    them, say so explicitly and explain why.

### Phase 5 --- Mandatory review of AI-generated changes

Before finishing, review the complete diff, including all modified and
newly created files---not only the latest edits.

Check explicitly for:

-   Existing source logic accidentally omitted during a refactor or
    rewrite.
-   Calls to the wrong overload or a method with different semantics.
-   Changes in control flow, persistence timing, exception behavior,
    permissions, return values, or side effects.
-   Duplicate or redundant queries and methods that can safely share
    implementation.
-   Unnecessary helper methods and unrelated refactoring.
-   SQL translation problems, avoidable transformations, nullable-value
    failures, and missed indexes.
-   Missing braces, inconsistent formatting, malformed strings, HTML
    special characters, and incorrect domain terminology.
-   Generated-client drift, API contract mismatch, missing tests, and
    unverified assumptions.
-   Tests that were not actually executed or results reported more
    confidently than the evidence supports.

Compare old and new logic when code is being moved or refactored. Verify
that every original behavior is preserved unless the ticket explicitly
requires a change. Passing tests or a successful build does not replace
this review.

## Stop conditions

Pause the dependent implementation and report a specific blocker when:

-   The required schema migration or foreign-key relationship is not
    present.
-   The source of truth for an API contract or generated client is
    unavailable.
-   A business rule needed to determine correct behavior is unresolved.
-   Repository evidence does not establish the expected behavior safely.

Do not create placeholders that appear complete, guess table/column
names, or claim the work is finished when a required dependency is
missing.

## Final response format

At the end of the task, report:

1.  **Implemented:** concise summary of behavior changed.
2.  **Files changed:** list the relevant files.
3.  **Verification:** exact build and test commands/results,
    distinguishing executed checks from checks not run.
4.  **Diff review:** confirm whether the full diff was inspected and any
    issues found/fixed.
5.  **Remaining blockers:** list unresolved dependencies, or state none
    if verified.

Never fabricate successful builds, passing tests, repository findings,
or completed work.
