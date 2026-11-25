# Contributing to Hirondelle

## Working language

English is the working language of the project: code, tests, comments, commit messages, issues and documentation.

## Workflow

- Every change starts from a GitHub issue.
- Work on a branch named `issue/N`, where `N` is the issue number (e.g. `issue/2` for issue #2).
  This convention is also used by the AI tooling to find the ticket being implemented.
- Build and run the tests with:

  ```sh
  dotnet tool restore
  dotnet cake
  ```

## Versioning

The version is computed by [GitVersion](https://gitversion.net/) (`GitVersion.yml`, GitHub Flow) from the git tags and the commit messages, which follow [Conventional Commits](https://www.conventionalcommits.org/).

- A release is a tag `X.Y.Z` (or `vX.Y.Z`) on `master`. Only a tagged commit gets a plain version (e.g. `0.2.0`).
- Between two tags, GitVersion applies **once** the strongest increment found in the commits since the last tag:

  | Commit message | Increment |
  |---|---|
  | `feat:` / `feat(scope):` | minor |
  | `fix:` / `perf:` | patch |
  | `<type>!:` (breaking change) | major |
  | anything else (`chore:`, `docs:`…) | patch (default on `master`) |

- Untagged commits on `master` get a beta version whose number counts the commits since the last tag (`0.2.0-beta.1`, `0.2.0-beta.2`…). On an `issue/N` branch the label is the branch name (`0.2.0-issue-N.1`).

Example, starting from the tag `v0.1.0`:

| History | Version |
|---|---|
| `fix: …` | `0.1.1-beta.1` |
| then `feat: …` | `0.2.0-beta.2` |
| then `fix: …` | `0.2.0-beta.3` |
| tag `v0.2.0` | `0.2.0` |

With squash merges, the PR title becomes the commit message on `master`: it must follow the same convention. Avoid `!` before `1.0.0`, as it would bump the version to `1.0.0`.

## Continuous integration and releases

The [CI workflow](.github/workflows/ci.yml) runs `dotnet cake` on every pull request targeting `master`: a PR can only be merged when the build and the tests pass.

After each merge on `master`, once the build and the tests pass, the CI creates or updates a **draft pre-release** with [GitReleaseManager](https://github.com/GitTools/GitReleaseManager) (`dotnet cake --target=Release-Draft`):

- the release is named after the next version computed by GitVersion (`X.Y.Z`);
- its notes are generated from the issues closed in the **milestone with the same name**: each milestone must be named after the version it delivers (e.g. `0.2.0`). If no milestone has this name (e.g. a tooling change), no draft is created;
- every closed issue of the milestone must carry exactly one of these labels, otherwise the job fails (see [`GitReleaseManager.yml`](GitReleaseManager.yml)):

  | Label | Section of the release notes |
  |---|---|
  | `enhancement` | Features |
  | `bug` | Bug fixes |
  | `documentation` | Documentation |
  | `build`, `duplicate`, `invalid`, `wontfix`, `question` | excluded from the notes |

To publish a final release, open the draft on GitHub, check the tag (`X.Y.Z`), untick *pre-release* and publish it. GitHub creates the tag on `master`, and the next commits are versioned from it.

Versions `0.x` are initial development: they follow the milestones and are not meant to be usable before the first NuGet package.

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
