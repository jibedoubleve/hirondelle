---
name: check-tests
description: Check that the tests cover every scenario of the GitHub issue implemented on the current branch (issue/N) and of the documentation in /doc. Reports coverage, then offers to write the missing tests, or writes them directly when asked. Use when the user asks to review, check, verify or write tests for the current ticket.
argument-hint: "[write]"
---

# Check tests against the ticket and the documentation

## Language

The working language of the project is English: code, test names, comments and commit messages stay in English.
Reply to the user in the language they used when invoking this skill (e.g. if they write in French, the report and questions are in French).

## Mode

- No argument: analyse, report, then **offer** to write the missing tests and wait for confirmation.
- `write` argument, or the user explicitly asks to write the tests: analyse, report, then write the missing tests without asking.

## Steps

### 1. Find the ticket

- Run `git branch --show-current`. A branch named `issue/N` means GitHub issue #N.
- Run `gh issue view N --comments` to get the description, acceptance criteria and discussion.
- If the branch does not follow `issue/N`, or `gh` fails, ask the user for the issue number.

### 2. Check the documentation

- Search `/doc` for everything related to the ticket (feature names, keywords from the issue).
- Note for each requirement whether the documentation covers it, is silent, or contradicts the ticket. Report contradictions; do not resolve them silently.

### 3. List the expected scenarios

From the ticket and the documentation, build the list of scenarios to cover:

- nominal cases;
- edge cases (empty, null, boundaries, whitespace, casing, duplicates…);
- error cases (invalid input, expected exceptions or error results).

Each scenario must cite its source (ticket or doc file).

### 4. Verify the implementation

- Read the changes of the branch: `git diff master...HEAD` plus uncommitted changes (`git diff`, `git status`), focusing on `src/Hirondelle`.
- For each scenario, check that the production code actually handles it. A test can only be judged if the feature exists: flag any scenario that is not implemented, rather than writing a test that cannot pass.

### 5. Compare with the tests

- Read the tests in `src/Hirondelle.Tests` related to the feature.
- Run `dotnet test` and note failures.
- Match each scenario to the test(s) covering it. Also flag weak tests: missing or too loose assertions, tests that cannot fail, tests whose name does not match what they check.

### 6. Report

Give a table:

| Scenario | Source | Implemented | Test | Status |
|----------|--------|-------------|------|--------|

Status: ✅ covered, ⚠️ partial or weak, ❌ missing. Then list doc/ticket gaps or contradictions and failing tests.

### 7. Write the tests

Depending on the mode (see above), offer or write the missing tests:

- reuse the framework, naming conventions, structure and style of the existing tests (follow `.editorconfig`);
- one scenario per test, or a parameterised test when only the data changes;
- do not modify production code; if a scenario is not implemented, report it instead;
- run `dotnet test` afterwards and report the result faithfully.
