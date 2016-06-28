#!/usr/bin/env sh

# go to root directory
cd "$(git rev-parse --show-toplevel)"

DIRS=$(find runtime -type d -depth 1 | grep -v common)
for d in $DIRS; do
    echo "Processing '$d' for npm ignore purposes"
    ls $d | grep -v templates | grep -v "^js$" | grep -v README > "$d/.npmignore" || echo "  Nothing to ignore"
done
