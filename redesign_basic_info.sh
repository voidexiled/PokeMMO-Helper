#!/bin/bash
# Replace Nature/Ability/Item sections with new searchable layout

cd "$(dirname "$0")"

# This will replace lines 1254-1296 with the new layout
# Backup first
cp RenderPasaporte.cs RenderPasaporte.cs.backup

# Create the new Nature/Ability/Item section
cat > /tmp/new_basic_info.txt << 'NEWCODE'
                            // Nature with individual search
                            ImGui.Text("Nature:");
                            ImGui.SetNextItemWidth(200);
                            ImGui.InputText("##NatureSearch", ref _natureSearchFilter, 64);
                            ImGui.SameLine();
                            var natures = Nature.GetAllNatures();
                            var filteredNatures = string.IsNullOrWhiteSpace(_natureSearchFilter)
                                ? natures
                                : natures.Where(n => n.Name.Contains(_natureSearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                            
                            string currentNature = _editedNatureIndex >= 0 && _editedNatureIndex < natures.Count 
                                ? natures[_editedNatureIndex].Name 
                                : "Select Nature";
                            
                            if (ImGui.BeginCombo("##NatureCombo", currentNature))
                            {
                                foreach (var nature in filteredNatures)
                                {
                                    int idx = natures.IndexOf(nature);
                                    bool isSelected = _editedNatureIndex == idx;
                                    if (ImGui.Selectable(nature.Name, isSelected))
                                    {
                                        _editedNatureIndex = idx;
                                        CalculateStatsRealTime();
                                    }
                                    
                                    // Tooltip inside combo
                                    if (ImGui.IsItemHovered())
                                    {
                                        string tooltip = nature.IsNeutral() 
                                            ? $"{nature.Name}\nNeutral (no stat changes)" 
                                            : $"{nature.Name}\n+10% {nature.IncreasedStat}\n-10% {nature.DecreasedStat}";
                                        ImGui.SetTooltip(tooltip);
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            
                            ImGui.Spacing();
                            
                            // Ability with individual search
                            ImGui.Text("Ability:");
                            ImGui.SetNextItemWidth(200);
                            ImGui.InputText("##AbilitySearch", ref _abilitySearchFilter, 64);
                            ImGui.SameLine();
                            var abilities = Ability.GetCommonAbilities();
                            var filteredAbilities = string.IsNullOrWhiteSpace(_abilitySearchFilter)
                                ? abilities
                                : abilities.Where(a => a.Name.Contains(_abilitySearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                            
                            string currentAbility = _editedAbilityIndex >= 0 && _editedAbilityIndex < abilities.Count 
                                ? abilities[_editedAbilityIndex].Name 
                                : "Select Ability";
                            
                            if (ImGui.BeginCombo("##AbilityCombo", currentAbility))
                            {
                                foreach (var ability in filteredAbilities)
                                {
                                    int idx = abilities.IndexOf(ability);
                                    bool isSelected = _editedAbilityIndex == idx;
                                    if (ImGui.Selectable(ability.Name, isSelected))
                                    {
                                        _editedAbilityIndex = idx;
                                    }
                                    
                                    // Tooltip inside combo
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip($"{ability.Name}\n{ability.Effect}");
                                    }
                                }
                                ImGui.EndCombo();
                            }
                            
                            ImGui.Spacing();
                            
                            // Held Item with individual search
                            ImGui.Text("Held Item:");
                            ImGui.SetNextItemWidth(200);
                            ImGui.InputText("##ItemSearch", ref _itemSearchFilter, 64);
                            ImGui.SameLine();
                            var items = Item.GetCommonItems();
                            var filteredItems = string.IsNullOrWhiteSpace(_itemSearchFilter)
                                ? items
                                : items.Where(i => i.Name.Contains(_itemSearchFilter, StringComparison.OrdinalIgnoreCase)).ToList();
                            
                            string currentItem = _editedItemIndex >= 0 && _editedItemIndex < items.Count 
                                ? items[_editedItemIndex].Name 
                                : "Select Item";
                            
                            if (ImGui.BeginCombo("##ItemCombo", currentItem))
                            {
                                foreach (var item in filteredItems)
                                {
                                    int idx = items.IndexOf(item);
                                    bool isSelected = _editedItemIndex == idx;
                                    if (ImGui.Selectable(item.Name, isSelected))
                                    {
                                        _editedItemIndex = idx;
                                    }
                                    
                                    // Tooltip inside combo
                                    if (ImGui.IsItemHovered())
                                    {
                                        ImGui.SetTooltip($"{item.Name}\n{item.Effect}");
                                    }
                                }
                                ImGui.EndCombo();
                            }
NEWCODE

# Delete old lines 1254-1296 and insert new code
sed -i '1254,1296d' RenderPasaporte.cs
sed -i '1253r /tmp/new_basic_info.txt' RenderPasaporte.cs

echo "✅ Nature/Ability/Item redesigned with individual searches!"
