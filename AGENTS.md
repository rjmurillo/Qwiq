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

| Rule                                  | Fact                                                                                                                                                                                                                                                                                        |
| ------------------------------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| Canonical TFMs                        | Libraries, unit tests, Mocks, Tests.Common: `net472;net48;net481;net10.0`. SOAP and Integration tests: `net472`. Benchmarks: `net472;net10.0`. Package and WireMock tests: `net10.0`. net8.0, net9.0 and netstandard2.0 are gone. Do not add them back.                                     |
| TFM change touches docs               | When a TFM set changes, update the csproj, the TFM tables in `claude/skills/qwiq-build/REFERENCE.md` and `SKILL.md`, the Multi-Targeting section of `.github/copilot-instructions.md`, the affected `*/AGENTS.md`, and the Canonical TFMs row in this file. Then refresh package baselines. |
| Tests.Common TFMs cover all consumers | `test/Qwiq.Tests.Common` must target every TFM of each project that references it. When a consumer gains a TFM that Tests.Common lacks, add it to Tests.Common in the same change.                                                                                                          |
| SDK bumps refresh package baselines   | `global.json` pins the SDK. A new SDK band or servicing release can change `dotnet pack` output. Regenerate `test/Qwiq.Package.Tests/*.verified.*` in the same PR. Those tests run only on `Build (windows-latest)`, so a Linux run will not catch it.                                      |
| SDK version appears in docs           | On every bump, run `git grep -nE "10\.0\.[0-9]{3}"` and update stale hits outside `.agents/`. `.github/README.md`, `CONTRIBUTING.md`, `.github/copilot-instructions.md`, `.github/instructions/json.instructions.md` and `qwiq-build/REFERENCE.md` hardcode it.                             |
