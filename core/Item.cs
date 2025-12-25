namespace PasaporteFiller.core;

/// <summary>
/// Represents a held item that can be equipped to a Pokemon
/// </summary>
public class Item
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string Effect { get; set; }  // Battle effect description
    public ItemCategory Category { get; set; }
    public int Cost { get; set; }  // Item cost in Poké Dollars
    public int? FlingPower { get; set; }  // Power when used with Fling move
    public string SpriteUrl { get; set; }  // URL to item sprite image

    public Item(string name, ItemCategory category = ItemCategory.None, string description = "", string effect = "", int cost = 0, int? flingPower = null, string spriteUrl = "")
    {
        Name = name;
        Category = category;
        Description = description;
        Effect = effect;
        Cost = cost;
        FlingPower = flingPower;
        SpriteUrl = spriteUrl;
    }


    /// <summary>
    /// Common battle items - can be extended later
    /// </summary>
    public static List<Item> GetCommonItems()
    {
        return new List<Item>
        {
            // Choice items
            new Item("Choice Band", ItemCategory.StatBoost, "Boosts Attack 1.5x but locks you into one move", "1.5x Attack, locked into first move"),
            new Item("Choice Specs", ItemCategory.StatBoost, "Boosts Special Attack 1.5x but locks you into one move", "1.5x Special Attack, locked into first move"),
            new Item("Choice Scarf", ItemCategory.StatBoost, "Boosts Speed 1.5x but locks you into one move", "1.5x Speed, locked into first move"),
            
            // Life Orb
            new Item("Life Orb", ItemCategory.DamageBoost, "Boosts move power by 30% but damages the user", "1.3x power, lose 10% HP per hit"),
            
            // Leftovers
            new Item("Leftovers", ItemCategory.Recovery, "Restores a little HP each turn", "Restores 1/16 HP per turn"),
            
            // Berries
            new Item("Sitrus Berry", ItemCategory.Berry, "Restores 25% HP when HP falls below 50%", "Heal 25% HP when below 50%"),
            new Item("Lum Berry", ItemCategory.Berry, "Cures any status condition", "Cures all status conditions"),
            new Item("Oran Berry", ItemCategory.Berry, "Restores 10 HP", "Heal 10 HP when HP is low"),
            
            // Type-boosting items
            new Item("Charcoal", ItemCategory.TypeBoost, "Powers up Fire-type moves", "1.2x Fire-type move power"),
            new Item("Mystic Water", ItemCategory.TypeBoost, "Powers up Water-type moves", "1.2x Water-type move power"),
            new Item("Miracle Seed", ItemCategory.TypeBoost, "Powers up Grass-type moves", "1.2x Grass-type move power"),
            
            // Focus Sash
            new Item("Focus Sash", ItemCategory.Protection, "Survives a one-hit KO from full HP", "Survive OHKO with 1 HP (once per battle)"),
            
            // Assault Vest
            new Item("Assault Vest", ItemCategory.StatBoost, "Boosts Special Defense 1.5x but prevents status moves", "1.5x Special Defense, cannot use status moves"),
            
            // Eviolite
            new Item("Eviolite", ItemCategory.StatBoost, "Boosts Defense and Special Defense of unevolved Pokemon", "1.5x Def and SpDef (not fully evolved only)"),
            
            // None
            new Item("None", ItemCategory.None, "No item equipped", "")
        };
    }

    /// <summary>
    /// Gets an item by name
    /// </summary>
    public static Item? GetItem(string name)
    {
        return GetCommonItems().FirstOrDefault(i => i.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public override string ToString()
    {
        return string.IsNullOrEmpty(Effect) ? Name : $"{Name} - {Effect}";
    }
}

/// <summary>
/// Categories of items
/// </summary>
public enum ItemCategory
{
    None,
    StatBoost,
    DamageBoost,
    TypeBoost,
    Recovery,
    Berry,
    Protection,
    Other
}
