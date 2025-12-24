#!/bin/bash
# Script to fix Add button in RenderPasaporte.cs

FILE="RenderPasaporte.cs"

# Create the replacement code
cat > /tmp/add_button_fix.txt << 'EOF'
                            if (ImGui.Button("Add"))
                            {
                                _showPokemonEditorWindow = true;
                                _editingPosition = i;
                                _editedBasePokemon = null;
                                _editedPokemonName = "";
                                _editedNickname = "";
                                _editedLevel = 50;
                            }
EOF

# Use awk to replace line 973
awk 'NR==973{system("cat /tmp/add_button_fix.txt");next}1' "$FILE" > "$FILE.tmp" && mv "$FILE.tmp" "$FILE"

# Convert back to DOS line endings
unix2dos "$FILE" 2>/dev/null || sed -i 's/$/\r/' "$FILE"

echo "✅ Add button fixed!"
