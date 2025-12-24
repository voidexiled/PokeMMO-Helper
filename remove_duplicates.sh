#!/bin/bash
# Remove duplicate lines 982-989 and fix line endings

cd "$(dirname "$0")"

# Remove lines 982 through 989 (the duplicate block)
sed -i '982,989d' RenderPasaporte.cs

# Convert to DOS line endings (CRLF)
sed -i 's/$/\r/' RenderPasaporte.cs 2>/dev/null || unix2dos RenderPasaporte.cs 2>/dev/null

echo "✅ Fixed duplicates and restored CRLF!"
