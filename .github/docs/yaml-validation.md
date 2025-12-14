# YAML Validation Guide

This repository uses **automatic YAML validation and formatting** via pre-commit hooks. No manual intervention required.

## Token-Efficient Workflow

### Pre-Commit Auto-Fix (Recommended - Zero OODA Loop)

YAML files are **automatically** formatted and validated on commit:

```bash
git add .github/workflows/my-workflow.yml
git commit -m "feat: add workflow"
# ✓ Auto-formats YAML with pprettier
# ✓ Validates syntax
# ✓ Re-stages if fixed
```

**Tools used:**
- `dotnet pprettier` - Auto-fixes formatting (spacing, indentation, line length)
- Python `yaml.safe_load()` - Validates syntax (done automatically by pprettier)

**Result:** YAML is validated and fixed without agent interaction = **zero tokens spent on OODA loop**.

### Quick Syntax Check (Optional)

For quick syntax validation without formatting:

```bash
python3 -c "import yaml; yaml.safe_load(open('.github/workflows/copilot-setup-steps.yml'))" && echo "✓ Valid"
```

Or use the helper script:

```bash
./.github/scripts/validate-yaml.sh .github/workflows/copilot-setup-steps.yml
```

### Manual Format (If Needed)

```bash
# Format single file
dotnet pprettier --write .github/workflows/main.yml

# Format all YAML files
dotnet pprettier --write "**/*.{yml,yaml}"
```

## How It Works

### 1. Pre-Commit Hook (.githooks/pre-commit)

Enabled automatically via `copilot-setup-steps.yml` workflow or manual setup:

```bash
git config core.hooksPath .githooks
```

Hook automatically:
1. Detects staged YAML files
2. Runs `dotnet pprettier --write` to fix formatting
3. Validates syntax
4. Re-stages fixed files
5. Commits if valid

### 2. Environment Variable

```bash
# Auto-fix mode (default)
git commit

# Check-only mode (CI)
SKIP_AUTOFIX=1 git commit
```

## Common YAML Issues (Auto-Fixed)

| Issue | Auto-Fixed? | Example |
|-------|-------------|---------|
| Inconsistent indentation | ✓ Yes | 2 vs 4 spaces |
| Trailing whitespace | ✓ Yes | `value:   ` → `value:` |
| Missing newline at EOF | ✓ Yes | Adds `\n` |
| Line length | ✓ Yes | Wraps long lines |
| Quote style | ✓ Yes | Normalizes quotes |
| **Syntax errors** | ✗ No | Must fix manually |
| **Duplicate keys** | ✗ No | Must fix manually |

## Syntax Errors (Manual Fix Required)

### Invalid Indentation

```yaml
# ❌ WRONG
jobs:
  build:
  runs-on: ubuntu-latest  # Not indented properly
```

```yaml
# ✓ CORRECT
jobs:
  build:
    runs-on: ubuntu-latest
```

### Duplicate Keys

```yaml
# ❌ WRONG
jobs:
  build:
    name: Build
    name: Build Again  # Duplicate!
```

### Tab Characters

```yaml
# ❌ WRONG (uses tabs - shown as →)
jobs:
→ build:

# ✓ CORRECT (uses spaces)
jobs:
  build:
```

**Error:** Python will show `found character '\t' that cannot start any token`

## GitHub Actions-Specific

### Required Job Name

For `copilot-setup-steps.yml`:

```yaml
jobs:
  # MUST be named exactly "copilot-setup-steps"
  copilot-setup-steps:
    runs-on: ubuntu-latest
```

Reference: [Customize Copilot Agent Environment](https://docs.github.com/en/copilot/how-tos/use-copilot-agents/coding-agent/customize-the-agent-environment)

## Troubleshooting

### Pre-commit hook not running

```bash
# Re-enable git hooks
git config core.hooksPath .githooks

# Or run setup script
bash .github/copilot-setup.sh
```

### pprettier not found

```bash
# Restore dotnet tools
dotnet tool restore
```

### Python not found

```bash
# Check installation
which python3

# Install if needed (macOS)
brew install python3
```

## Token Efficiency Comparison

| Method | Token Cost | When to Use |
|--------|-----------|-------------|
| **Pre-commit auto-fix** | 0 | ✓ Always (default) |
| Quick syntax check | ~10 | Only when debugging |
| Manual formatting | ~50 | Only when hook disabled |
| Full CI workflow | ~200 | Only in CI/CD |

**Best Practice:** Let pre-commit hooks handle validation automatically = **zero token overhead**.

## References

- [Pre-commit hook](./../.githooks/pre-commit) - Auto-fix implementation
- [pprettier](https://github.com/belav/csharpier/tree/main/Src/PackedPrettier) - Dotnet tool for JSON/YAML formatting
- [GitHub Actions Workflow Syntax](https://docs.github.com/en/actions/using-workflows/workflow-syntax-for-github-actions)
