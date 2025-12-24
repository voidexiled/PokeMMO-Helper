#!/bin/bash
# Fix moveset editor lines 1320-1365 in RenderPasaporte.cs

cd "$(dirname "$0")"

# Create the new moveset editor code
cat > /tmp/new_moveset.txt << 'EOF'
                        // Moveset Editor with Search
                        if (ImGui.TreeNode("Moveset (Max 4)"))
                        {
                            // Move search filter
                            ImGui.SetNextItemWidth(300);
                            ImGui.InputText("Search Moves", ref _moveSearchFilter, 64);
                            ImGui.SameLine();
                            if (ImGui.Button("Clear##MoveSearch"))
                            {
                                _moveSearchFilter = "";
                            }
                            
                            // Get available moves from Pokemon
                            string[] availableMoveNames = _editedBasePokemon.Moves?.Select(m => m.Name).ToArray() ?? Array.Empty<string>();
                            
                            // Filter moves based on search
                            var filteredMoves = string.IsNullOrWhiteSpace(_moveSearchFilter)
                                ? availableMoveNames
                                : availableMoveNames.Where(m => m.Contains(_moveSearchFilter, StringComparison.OrdinalIgnoreCase)).ToArray();
                            
                            ImGui.Text($"Showing {filteredMoves.Length} of {availableMoveNames.Length} moves");
                            ImGui.Separator();
                            
                            for (int i = 0; i < 4; i++)
                            {
                                string currentMove = i < _selectedMoves.Count 
                                    ? _selectedMoves[i].MoveData.Name 
                                    : "Empty";
                                
                                if (ImGui.BeginCombo($"Move {i + 1}", currentMove))
                                {
                                    // Option to clear the move
                                    if (ImGui.Selectable("Empty"))
                                    {
                                        if (i < _selectedMoves.Count)
                                        {
                                            _selectedMoves.RemoveAt(i);
                                        }
                                    }
                                    
                                    // List filtered moves
                                    foreach (var moveName in filteredMoves)
                                    {
                                        bool isSelected = currentMove == moveName;
                                        if (ImGui.Selectable(moveName, isSelected))
                                        {
                                            var moveData = _editedBasePokemon.Moves?.FirstOrDefault(m => m.Name == moveName);
                                            
                                            if (moveData != null)
                                            {
                                                var learnedMove = new LearnedMove(moveData, moveData.PP);
                                                
                                                if (i < _selectedMoves.Count)
                                                    _selectedMoves[i] = learnedMove;
                                                else
                                                    _selectedMoves.Add(learnedMove);
                                            }
                                        }
                                        
                                        // Tooltip with move info
                                        if (ImGui.IsItemHovered())
                                        {
                                            var moveData = _editedBasePokemon.Moves?.FirstOrDefault(m => m.Name == moveName);
                                            if (moveData != null)
                                            {
                                                ImGui.SetTooltip($"{moveName}\nPower: {moveData.Power}\nPP: {moveData.PP}\nAccuracy: {moveData.Accuracy}%");
                                            }
                                        }
                                    }
                                    ImGui.EndCombo();
                                }
                            }
                            ImGui.TreePop();
                        }
EOF

# Delete old moveset editor (lines 1320-1365) and insert new one
sed -i '1320,1365d' RenderPasaporte.cs
sed -i '1319r /tmp/new_moveset.txt' RenderPasaporte.cs

echo "✅ Moveset editor replaced!"
