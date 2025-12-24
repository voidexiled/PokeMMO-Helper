#!/bin/bash
# Add tooltips to Nature/Ability/Item at lines 1254-1271

cd "$(dirname "$0")"

# Insert tooltip code after Nature combo (after line 1261)
sed -i '1261 a\                            if (ImGui.IsItemHovered() && _editedNatureIndex >= 0 && _editedNatureIndex < natures.Count)\
                            {\
                                var nature = natures[_editedNatureIndex];\
                                string tooltip = nature.IsNeutral() ? $"{nature.Name}\\nNeutral (no stat changes)" : $"{nature.Name}\\n+10% {nature.IncreasedStat}\\n-10% {nature.DecreasedStat}";\
                                ImGui.SetTooltip(tooltip);\
                            }' RenderPasaporte.cs

# Insert tooltip code after Ability combo (after line 1266, now shifted)
sed -i '1272 a\                            if (ImGui.IsItemHovered() && _editedAbilityIndex >= 0 && _editedAbilityIndex < abilities.Count)\
                            {\
                                var ability = abilities[_editedAbilityIndex];\
                                ImGui.SetTooltip($"{ability.Name}\\n{ability.Effect}");\
                            }' RenderPasaporte.cs

# Insert tooltip code after Item combo (after line 1271, now shifted more)
sed -i '1282 a\                            if (ImGui.IsItemHovered() && _editedItemIndex >= 0 && _editedItemIndex < items.Count)\
                            {\
                                var item = items[_editedItemIndex];\
                                ImGui.SetTooltip($"{item.Name}\\n{item.Effect}");\
                            }' RenderPasaporte.cs

echo "✅ Tooltips added to Nature, Ability, and Item!"
