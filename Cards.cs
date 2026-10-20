using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime;

namespace AbilityWheel;

internal static class Cards
{
    private static string _lastLogged = "";
    private static string _lastLoggedWeapons = "";

    private static ItemsManager _itemsManager;
    private static readonly HashSet<string> _reloaded = new();

    // The inventory's item objects are loaded per room (AbstractLibrary.LoadOrGetElement uses
    // ResourceContext.Room), so one can be unloaded and destroyed while the inventory still lists
    // it. The game itself goes by item ID (saves, HasItemInInventory(string)), and its menus load
    // the item again (InventoryWeaponChecker.TrySetupSlot). So does this: a destroyed item, which
    // Unity reports as null, is looked up again by ID.
    internal static T Live<T>(ItemData item) where T : ItemData
    {
        if (item is null)
            return null;
        var typed = item.TryCast<T>();
        if (typed is null)
            return null;
        if (typed != null)   // Unity's check: false once destroyed
            return typed;

        string id = item.itemID;
        if (_itemsManager == null)
            _itemsManager = UnityEngine.Object.FindObjectOfType<ItemsManager>();
        var found = _itemsManager?.FindItem(id)?.TryCast<T>();
        if (_reloaded.Add(id ?? "?"))
            Plugin.Logger.LogInfo($"[cards] {id} was unloaded by the game, {(found != null ? "loaded it again" : "can't load it again")}");
        return found;
    }

    internal static List<SpellItemData> Owned(Inventory inventory) => OwnedOf<SpellItemData>(inventory);

    private static List<T> OwnedOf<T>(Inventory inventory) where T : ItemData
    {
        var result = new List<T>();
        var items = inventory.GetAllItemsOfTypeInInventory(Il2CppType.Of<T>());
        if (items == null)
            return result;
        foreach (var item in items)
        {
            var live = Live<T>(item);
            if (live != null)
                result.Add(live);
        }
        return result;
    }

    internal static void Resolve(Inventory inventory, SpellItemData[] slots)
    {
        var owned = Owned(inventory);
        if (Plugin.AutoFillSlots.Value)
            AutoFill(owned);

        for (int slot = 0; slot < slots.Length; slot++)
        {
            string id = Plugin.Slots[slot].Value.Trim();
            slots[slot] = id.Length == 0
                ? null
                : owned.FirstOrDefault(s => string.Equals(s.itemID, id, StringComparison.OrdinalIgnoreCase));
        }

        // The config descriptions send players here for the card IDs.
        var line = new StringBuilder("[cards] wheel");
        for (int slot = 0; slot < slots.Length; slot++)
        {
            string id = Plugin.Slots[slot].Value.Trim();
            line.Append($" {Wheel.SlotLabels[slot]}={(id.Length == 0 ? "empty" : id)}");
            if (id.Length > 0 && slots[slot] == null)
                line.Append(" (not owned)");
        }
        line.Append($"; owned: {string.Join(", ", owned.Select(s => s.itemID))}");
        if (line.ToString() != _lastLogged)
        {
            _lastLogged = line.ToString();
            Plugin.Logger.LogInfo(_lastLogged);
        }
    }

    private static void AutoFill(List<SpellItemData> owned)
    {
        var used = new HashSet<string>(
            Plugin.Slots.Select(s => s.Value.Trim()).Where(id => id.Length > 0),
            StringComparer.OrdinalIgnoreCase);

        foreach (var entry in Plugin.Slots)
        {
            if (entry.Value.Trim().Length > 0)
                continue;
            var card = owned.FirstOrDefault(s => !used.Contains(s.itemID));
            if (card == null)
                return;
            entry.Value = card.itemID;   // saves the config file
            used.Add(card.itemID);
            Plugin.Logger.LogInfo($"[cards] {entry.Definition.Key} was empty, filled with {card.itemID}");
        }
    }

    internal static List<WeaponItemData> OwnedWeapons(Inventory inventory) => OwnedOf<WeaponItemData>(inventory);

    // No auto-fill: the weapon page stays out of the way until the player fills a slot.
    internal static void ResolveWeapons(Inventory inventory, WeaponItemData[] slots)
    {
        var owned = OwnedWeapons(inventory);
        for (int slot = 0; slot < slots.Length; slot++)
        {
            string id = Plugin.WeaponSlots[slot].Value.Trim();
            slots[slot] = id.Length == 0
                ? null
                : owned.FirstOrDefault(w => string.Equals(w.itemID, id, StringComparison.OrdinalIgnoreCase));
        }

        var line = new StringBuilder("[weapons] wheel");
        for (int slot = 0; slot < slots.Length; slot++)
        {
            string id = Plugin.WeaponSlots[slot].Value.Trim();
            line.Append($" {Wheel.SlotLabels[slot]}={(id.Length == 0 ? "empty" : id)}");
            if (id.Length > 0 && slots[slot] == null)
                line.Append(" (not owned)");
        }
        line.Append($"; owned: {string.Join(", ", owned.Select(w => w.itemID))}");
        if (line.ToString() != _lastLoggedWeapons)
        {
            _lastLoggedWeapons = line.ToString();
            Plugin.Logger.LogInfo(_lastLoggedWeapons);
        }
    }

    internal static string DisplayName(ItemData item)
    {
        string id = item.itemID ?? item.name;
        var name = new StringBuilder();
        for (int i = 0; i < id.Length; i++)
        {
            if (i > 0 && char.IsUpper(id[i]) && !char.IsUpper(id[i - 1]))
                name.Append(' ');
            name.Append(id[i]);
        }
        return name.ToString();
    }

    internal static bool StaysEquipped(SpellItemData spell) =>
        spell != null && Plugin.StayEquipped.Value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Any(id => string.Equals(id, spell.itemID, StringComparison.OrdinalIgnoreCase));

    // By ID: the same item can be two objects, one of them destroyed (see Live).
    internal static bool Same(ItemData a, ItemData b) =>
        a is not null && b is not null && (a.Pointer == b.Pointer || (a.itemID != null && a.itemID == b.itemID));
}
