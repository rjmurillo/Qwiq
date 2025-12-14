#!/usr/bin/env bash
# YAML Syntax Validation (Quick Check)
#
# Fast syntax-only validation using Python's yaml module.
# For formatting/style, use: dotnet pprettier --write file.yml
#
# Usage: ./validate-yaml.sh [file]
# Example: ./validate-yaml.sh .github/workflows/main.yml

set -e

if [[ -z "$1" ]]; then
    echo "Usage: $0 <yaml-file>"
    echo "Example: $0 .github/workflows/main.yml"
    exit 1
fi

FILE="$1"

if [[ ! -f "$FILE" ]]; then
    echo "Error: File not found: $FILE"
    exit 1
fi

# Validate syntax
if python3 -c "import yaml; yaml.safe_load(open('$FILE'))" 2>&1; then
    echo "✓ Valid YAML: $FILE"
    exit 0
else
    echo "✗ Invalid YAML: $FILE"
    exit 1
fi
