using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Il2CppInterop.Runtime;

namespace AbilityWheel;

internal static class Cards
{
    private static string _lastLogged = "";

    internal static List<SpellItemData> Owned(Inventory inventory)
    {
        var result = new List<SpellItemData>();
        var items = inventory.GetAllItemsOfTypeInInventory(Il2CppType.Of<SpellItemData>());
        if (items == null)
            return result;
        foreach (var item in items)
        {
            var spell = item?.TryCast<SpellItemData>();
            if (spell != null)
                result.Add(spell);
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

    internal static string DisplayName(SpellItemData spell)
    {
        string id = spell.itemID ?? spell.name;
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

    internal static bool Same(SpellItemData a, SpellItemData b) =>
        a != null && b != null && a.Pointer == b.Pointer;
}
