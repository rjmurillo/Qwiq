# AGENTS.md

Compact index for coding agents. Detail lives in the linked files. Per-project guides live in `src/*/AGENTS.md` and `test/*/AGENTS.md`.

## Index

| Topic             | Read                                                                         |
| ----------------- | ---------------------------------------------------------------------------- |
| Build, test, lint | `CLAUDE.md`, `claude/skills/qwiq-build/SKILL.md`                             |
| Full guide        | `.github/copilot-instructions.md`                                            |
| TFM table         | `claude/skills/qwiq-build/REFERENCE.md`                                      |
| Package baselines | `.github/copilot-instructions.md` (Package Tests), `test/Qwiq.Package.Tests` |
| CI                | `.github/workflows/main.yml`                                                 |

## Must not miss

| Rule                                  | Fact                                                                                                                                                                                                                                                                    |
| ------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Canonical TFMs                        | Libraries, unit tests, Mocks, Tests.Common: `net472;net48;net481;net10.0`. SOAP and Integration tests: `net472`. Benchmarks: `net472;net10.0`. Package and WireMock tests: `net10.0`. net8.0, net9.0 and netstandard2.0 are gone. Do not add them back.                 |
| TFM change touches docs               | When a TFM set changes, update the csproj, the TFM table in `claude/skills/qwiq-build/REFERENCE.md`, the Multi-Targeting section of `.github/copilot-instructions.md`, and the affected `*/AGENTS.md`. Then refresh package baselines.                                  |
| Tests.Common TFMs cover all consumers | `test/Qwiq.Tests.Common` must target every TFM that any project referencing it targets. Adding a TFM to a test project means adding it to Tests.Common in the same change.                                                                                              |
| SDK bumps refresh package baselines   | `global.json` pins the SDK (10.0.404 today). A new SDK band or servicing release can change `dotnet pack` output. Regenerate `test/Qwiq.Package.Tests/*.verified.*` in the same PR. Those tests run only on `Build (windows-latest)`, so a Linux run will not catch it. |
| SDK version appears in docs           | Grep for the old version on every bump. `.github/README.md` and `.github/copilot-instructions.md` hardcode it.                                                                                                                                                          |
