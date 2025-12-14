#!/usr/bin/env bash
#
# GitHub Copilot Workspace Setup Script
# Automatically configures git hooks and linting tools for Copilot Workspace
#
# This script can be run manually in Copilot Workspace environments:
#   bash .github/copilot-setup.sh
#
# Or triggered via the copilot-setup-steps.yml workflow
#

set -e

# Colors for output
GREEN='\033[0;32m'
CYAN='\033[0;36m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

echo_success() {
    echo -e "${GREEN}✓${NC} $1"
}

echo_info() {
    echo -e "${CYAN}ℹ${NC} $1"
}

echo_warning() {
    echo -e "${YELLOW}⚠${NC} $1"
}

# Get repository root
REPO_ROOT=$(git rev-parse --show-toplevel 2>/dev/null || pwd)
cd "$REPO_ROOT"

echo_info "Setting up Qwiq development environment for GitHub Copilot Workspace..."
echo ""

# 1. Enable git hooks
echo_info "Enabling git hooks..."
git config core.hooksPath .githooks
echo_success "Git hooks enabled at .githooks/"

# 2. Set SKIP_AUTOFIX environment variable
echo_info "Setting environment variables..."
export SKIP_AUTOFIX=0
echo_success "SKIP_AUTOFIX=0 (auto-fix enabled)"

# 3. Restore dotnet tools
echo_info "Restoring dotnet tools..."
if command -v dotnet &> /dev/null; then
    # Try to restore tools, but don't fail if SDK version mismatch
    if dotnet tool restore 2>/dev/null; then
        echo_success "Dotnet tools restored"
    else
        echo_warning "dotnet tool restore failed (possible SDK version mismatch)"
        echo "  You may need to install the SDK version specified in global.json"
    fi
else
    echo_warning "dotnet CLI not found, skipping tool restore"
fi

# 4. Install npm packages (for markdownlint-cli2)
echo_info "Installing npm packages..."
if command -v npm &> /dev/null; then
    npm install --global markdownlint-cli2
    echo_success "markdownlint-cli2 installed"
else
    echo_warning "npm not found, skipping npm package installation"
fi

# 5. Verify environment
echo ""
echo_info "Environment setup complete!"
echo ""
echo "Git hooks: $(git config --get core.hooksPath)"
echo "SKIP_AUTOFIX: ${SKIP_AUTOFIX:-0} (0=enabled, 1=disabled)"
echo ""

# Test that tools are available
if command -v npx &> /dev/null && npx markdownlint-cli2 --help &> /dev/null; then
    echo_success "markdownlint-cli2 is ready"
fi

if command -v dotnet &> /dev/null && dotnet tool list 2>/dev/null | grep -q "nbgv"; then
    echo_success "nbgv tool is ready"
fi

echo ""
echo_success "Ready to develop! Pre-commit hooks will auto-fix linting issues."
echo ""
echo "To add SKIP_AUTOFIX to your shell profile, run:"
echo "  echo 'export SKIP_AUTOFIX=0' >> ~/.bashrc"
