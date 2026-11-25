# Contributing to Hirondelle

## Working language

English is the working language of the project: code, tests, comments, commit messages, issues and documentation.

## Workflow

- Every change starts from a GitHub issue.
- Work on a branch named `issue/N`, where `N` is the issue number (e.g. `issue/2` for issue #2).
  This convention is also used by the AI tooling to find the ticket being implemented.
- Build and run the tests with:

  ```sh
  dotnet test
  ```

## Documentation

Functional documentation lives in [`/doc`](doc):

| File | Content |
|------|---------|
| [`doc/analyse.md`](doc/analyse.md) | Analysis of the API and of the supported migration scripts |

Rules:

- A feature must be described in `/doc` before or while it is implemented. If an issue changes the expected behaviour, update the documentation in the same branch.
- `/doc` is reserved for **functional** documentation (what the library does). Tooling and process documentation belong in this file.
- The documentation and the issue are the reference used to decide which scenarios must be tested.

## Tests

- Tests live in `src/Hirondelle.Tests` and use [xUnit v3](https://xunit.net/) and [Shouldly](https://docs.shouldly.org/).
- Every scenario described in the issue or in `/doc` (nominal cases, edge cases, error cases) should be covered by a test.

## AI tooling (Claude Code)

The repository ships [Claude Code](https://code.claude.com/) skills in `.claude/skills/`.
They are available both in the terminal (`claude`) and in Rider through the Claude Code plugin.

### Available skills

| Skill | Usage | What it does |
|-------|-------|--------------|
| `check-tests` | `/check-tests [write]` | Checks that the tests cover every scenario of the GitHub issue of the current `issue/N` branch and of the documentation in `/doc`. Reports the coverage, then offers to write the missing tests (or writes them directly with `write`). |

### Using a skill

- Type `/` in Claude Code to list the skills with their description, then pick one (e.g. `/check-tests`).
- You can also ask in plain language ("check that the tests cover the ticket", "write the missing tests"): Claude picks the matching skill from its description.
- Skills reply in the language you use to talk to them; the code and tests they produce stay in English.
- A newly added skill is only picked up by a new Claude Code session.

### Requirements

- [GitHub CLI](https://cli.github.com/) installed and authenticated (`gh auth login`), so that the issue can be read.
- The current branch follows the `issue/N` convention; otherwise the skill asks for the issue number.
- The .NET SDK, to run `dotnet test`.

### Adding or changing a skill

- Create `.claude/skills/<name>/SKILL.md`. The front matter `description` is shown in the `/` completion and is what Claude uses to trigger the skill automatically: keep it short and accurate.
- Write skills in English.
- Add a row to the [Available skills](#available-skills) table above.
