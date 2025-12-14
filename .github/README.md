# GitHub Copilot Workspace Setup

This directory contains setup automation for GitHub Copilot Workspace environments.

## Quick Setup

Run the setup script once after opening the workspace:

```bash
bash .github/copilot-setup.sh
```

This will:

- ✓ Enable git hooks at `.githooks/`
- ✓ Set `SKIP_AUTOFIX=0` (auto-fix mode enabled)
- ✓ Restore dotnet tools (nbgv)
- ✓ Install markdownlint-cli2

## Files

- `copilot-setup.sh` - Setup script for Copilot Workspace (run manually)
- `workflows/copilot-setup-steps.yml` - GitHub Actions workflow for automated setup

## GitHub Actions Workflow

The `copilot-setup-steps.yml` workflow can be triggered manually via:

```bash
gh workflow run copilot-setup-steps.yml
```

Or from the GitHub Actions UI:

1. Go to Actions tab
2. Select "Copilot Workspace Setup"
3. Click "Run workflow"

## Comparison with DevContainer

| Feature                | DevContainer              | Copilot Workspace        |
| ---------------------- | ------------------------- | ------------------------ |
| Git hooks setup        | Automatic (postCreate)    | Manual (run script)      |
| SKIP_AUTOFIX           | Set via containerEnv      | Set via script           |
| Dotnet tools           | Automatic restore         | Automatic restore        |
| markdownlint-cli2      | Automatic install         | Automatic install        |
| Trigger                | Container creation        | Manual script execution  |

## Environment Variables

| Variable       | Default | Description                                      |
| -------------- | ------- | ------------------------------------------------ |
| `SKIP_AUTOFIX` | `0`     | Controls auto-fix mode (0=enabled, 1=check only) |

To persist `SKIP_AUTOFIX` across sessions, add to your shell profile:

```bash
echo 'export SKIP_AUTOFIX=0' >> ~/.bashrc
source ~/.bashrc
```

## Verification

After running the setup script, verify the configuration:

```bash
# Check git hooks path
git config --get core.hooksPath
# Should output: .githooks

# Check SKIP_AUTOFIX
echo $SKIP_AUTOFIX
# Should output: 0

# Verify dotnet tools
dotnet tool list
# Should include: nbgv

# Verify markdownlint
npx markdownlint-cli2 --help
# Should display help text
```

## Troubleshooting

### Git hooks not running

Re-run the setup script:

```bash
bash .github/copilot-setup.sh
```

Or manually configure:

```bash
git config core.hooksPath .githooks
```

### SDK version mismatch

The script will warn but continue if the .NET SDK version doesn't match `global.json`. You can:

1. Install the required SDK version (check `global.json`)
2. Or update `global.json` to use an installed SDK version

### npm packages not found

Ensure Node.js and npm are installed:

```bash
node --version
npm --version
```

If missing, install Node.js LTS from <https://nodejs.org/>

## Related Documentation

- [CONTRIBUTING.md](../CONTRIBUTING.md) - Git hooks documentation
- [.githooks/pre-commit](../.githooks/pre-commit) - Pre-commit hook implementation
- [.devcontainer/](../.devcontainer/) - DevContainer setup for VS Code/Codespaces
- [.agents/retrospective/2025-12-14-automated-git-hooks-setup.md](../.agents/retrospective/2025-12-14-automated-git-hooks-setup.md) - Background on automated setup
