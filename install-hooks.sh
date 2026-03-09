#!/bin/bash

# Git hooks installation script for Gemeinschaftsgipfel
# This script installs the Git hooks from the hooks/ directory into .git/hooks/

set -e

echo "Installing Git hooks..."

# Get the root directory of the git repository
ROOT_DIR="$(git rev-parse --show-toplevel)"
HOOKS_DIR="$ROOT_DIR/hooks"
GIT_HOOKS_DIR="$ROOT_DIR/.git/hooks"

# Check if hooks directory exists
if [ ! -d "$HOOKS_DIR" ]; then
    echo "❌ Error: hooks/ directory not found"
    exit 1
fi

# Install each hook
for hook in "$HOOKS_DIR"/*; do
    if [ -f "$hook" ]; then
        hook_name=$(basename "$hook")
        echo "  Installing $hook_name..."
        cp "$hook" "$GIT_HOOKS_DIR/$hook_name"
        chmod +x "$GIT_HOOKS_DIR/$hook_name"
    fi
done

echo ""
echo "✅ Git hooks installed successfully!"
echo ""
echo "The following hooks are now active:"
ls -1 "$GIT_HOOKS_DIR" | grep -v ".sample" | sed 's/^/  - /'
echo ""
