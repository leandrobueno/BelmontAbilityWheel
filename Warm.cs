using System;
using System.Collections.Generic;
using System.Diagnostics;
using Il2CppInterop.Runtime.Attributes;
using RedWine.ResourceManagement;
using Pools = Il2CppSystem.Collections.Generic.Dictionary<RedWine.ResourceManagement.ResourceContext,
    Il2CppSystem.Collections.Generic.Dictionary<int, PooledPreWarmAssetData>>;

namespace AbilityWheel;

// Why a wheel press doesn't hold the game up. The game's equip (Inventory.EquipWeaponFromInventory,
// EquipSpellFromInventory) unloads everything the item taken off needs (its assets, and the pools
// of its effects), then loads the same for the new one, all inside the call: measured at 20 to
// 45 ms for a weapon and 10 to 45 ms for a card, a visible hitch while the screen scrolls. The game
// only equips from its paused menus, where that doesn't show; the wheel equips in the middle of
// play, a weapon per switch and a card or two per cast.
//
// So the wheel's weapons and cards are loaded ahead, at moments where a hitch doesn't show (a save
// loading, the Equipment or Tarot menu closing), and an equip that takes off a wheel item (or the
// home card, which comes back after the cast) skips the unload: the KeepWeaponLoaded patch, on for
// that one call. The game's own load of the new item still runs and finds everything there; if
// something isn't (the game unloaded it meanwhile), that load brings it back, at the price of one
// hitch.
//
// Loaded assets belong to the game's World context, the one the game uses for equipped items, by
// key: loading twice or unloading twice does nothing more than once. An item that leaves the wheel
// is unloaded here, unless Rose holds it (the game unloads it when she takes it off).
public partial class Wheel
{
    // Read by the KeepWeaponLoaded patch.
    internal static bool KeepLoaded;
    // Set once that patch is in. Without it nothing is loaded ahead: it would be unloaded anyway.
    internal static bool CanKeepLoaded;

    private readonly Dictionary<string, ItemData> _loadedAhead = new(StringComparer.OrdinalIgnoreCase);
    private string _loadedLine = "";
    private bool _loadAheadBroken;
    private bool _menuWasOpen;

    [HideFromIl2Cpp]
    private bool KeepsItemsLoaded => CanKeepLoaded && !_loadAheadBroken;

    [HideFromIl2Cpp]
    private bool InWheel(ItemData item)
    {
        if (item is null)
            return false;
        foreach (var slot in _slotWeapons)
        {
            if (Cards.Same(slot, item))
                return true;
        }
        foreach (var slot in _slotSpells)
        {
            if (Cards.Same(slot, item))
                return true;
        }
        return false;
    }

    // Items are put on the wheel, and equipped by the game, in the Equipment and Tarot menus.
    [HideFromIl2Cpp]
    private void TrackMenus()
    {
        bool open = MenuOpen;
        if (_menuWasOpen && !open)
        {
            RefreshSlots();
            RefreshWeapons();
            LoadWheelItems("a menu closed");
        }
        _menuWasOpen = open;
    }

    [HideFromIl2Cpp]
    private void LoadWheelItems(string why)
    {
        if (!KeepsItemsLoaded)
            return;
        var inventory = Inventory;
        if (inventory == null)
            return;
        long start = Stopwatch.GetTimestamp();
        try
        {
            ItemData weaponHeld = Cards.Live<WeaponItemData>(inventory.GetEquippedWeapon());
            ItemData cardHeld = Cards.Live<SpellItemData>(inventory.GetEquippedSpell());
            var wanted = new Dictionary<string, ItemData>(StringComparer.OrdinalIgnoreCase);
            if (Plugin.WeaponPage.Value)
            {
                foreach (var slot in _slotWeapons)
                {
                    var weapon = Cards.Live<WeaponItemData>(slot);
                    if (weapon != null && weapon.itemID != null)
                        wanted[weapon.itemID] = weapon;
                }
            }
            foreach (var slot in _slotSpells)
            {
                var spell = Cards.Live<SpellItemData>(slot);
                if (spell != null && spell.itemID != null)
                    wanted[spell.itemID] = spell;
            }

            var dropped = new List<string>();
            foreach (var id in new List<string>(_loadedAhead.Keys))
            {
                if (wanted.ContainsKey(id))
                    continue;
                var item = Cards.Live<ItemData>(_loadedAhead[id]);
                _loadedAhead.Remove(id);
                if (item == null || Cards.Same(item, weaponHeld) || Cards.Same(item, cardHeld) || Cards.Same(item, _home))
                    continue;
                item.UnloadRefs(ResourceContext.World);
                dropped.Add(id);
            }

            // What Rose holds too: an unload, above or the game's, takes what items share.
            var pools = new Pools();
            if (wanted.Count > 0)
            {
                weaponHeld?.PreLoadRefs(ResourceContext.World, pools);
                cardHeld?.PreLoadRefs(ResourceContext.World, pools);
            }
            foreach (var pair in wanted)
            {
                pair.Value.PreLoadRefs(ResourceContext.World, pools);
                _loadedAhead[pair.Key] = pair.Value;
            }
            PreWarmerUtils.CreatePools(pools);

            double ms = Ms(start, Stopwatch.GetTimestamp());
            string line = $"[wheel] loaded ahead: {(wanted.Count == 0 ? "nothing" : string.Join(", ", wanted.Keys))}" +
                (dropped.Count > 0 ? $"; unloaded {string.Join(", ", dropped)}" : "");
            if (line != _loadedLine)
                Plugin.Logger.LogInfo(line);
            _loadedLine = line;
            Plugin.Logger.LogDebug($"[perf] wheel items loaded in {ms:0.0} ms ({why})");
        }
        catch (Exception e)
        {
            _loadAheadBroken = true;
            Plugin.Logger.LogWarning($"[wheel] loading the wheel's items ahead failed, equips go back to the game's own loading: {e.Message}");
        }
    }

    // Every card the wheel equips goes through here: the loan for a cast, the way back home, a
    // selected card. The card taken off stays loaded if it is a wheel card, or the home card when
    // it comes back after the cast (homeReturns).
    [HideFromIl2Cpp]
    private void EquipSpell(Inventory inventory, SpellItemData spell, bool homeReturns)
    {
        var previous = inventory.GetEquippedSpell();
        bool keep = KeepsItemsLoaded && (InWheel(previous) || (homeReturns && Cards.Same(previous, _home)));
        long before = Stopwatch.GetTimestamp();
        KeepLoaded = keep;
        try
        {
            inventory.EquipSpellFromInventory(spell);
        }
        finally
        {
            KeepLoaded = false;
        }
        double ms = Ms(before, Stopwatch.GetTimestamp());
        if (ms >= SlowUpdateMs)
            Plugin.Logger.LogDebug(
                $"[perf] equipping {spell?.itemID} took {ms:0.0} ms{(keep ? "" : $", {previous?.itemID ?? "nothing"} was unloaded")}");
        NoteEquip(previous, spell, keep);
    }

    // After an equip. An item taken off that isn't on the wheel was unloaded by the game as usual,
    // with whatever it shared with the wheel's items: those are loaded again right away, in the
    // frame that already paid for a load.
    [HideFromIl2Cpp]
    private void NoteEquip(ItemData previous, ItemData equipped, bool kept)
    {
        if (!KeepsItemsLoaded)
            return;
        if (kept && previous?.itemID != null && InWheel(previous))
            _loadedAhead[previous.itemID] = previous;
        if (equipped?.itemID != null && InWheel(equipped))
            _loadedAhead[equipped.itemID] = equipped;
        if (!kept && previous is not null)
            LoadWheelItems($"the game unloaded {previous.itemID}");
    }

    // The game unloads item objects with the room they were loaded for (Cards.Live), the wheel's
    // cards and weapons among them. One is loaded again per frame (about 3 ms), ahead of the next
    // press, which would otherwise load them all at once.
    [HideFromIl2Cpp]
    private void ReviveSlotItem()
    {
        for (int slot = 0; slot < _slotSpells.Length; slot++)
        {
            var spell = _slotSpells[slot];
            if (spell is not null && spell == null)
            {
                _slotSpells[slot] = Cards.Live<SpellItemData>(spell);
                return;
            }
        }
        for (int slot = 0; slot < _slotWeapons.Length; slot++)
        {
            var weapon = _slotWeapons[slot];
            if (weapon is not null && weapon == null)
            {
                _slotWeapons[slot] = Cards.Live<WeaponItemData>(weapon);
                return;
            }
        }
    }

    [HideFromIl2Cpp]
    private void ForgetLoaded()
    {
        KeepLoaded = false;
        _loadedAhead.Clear();
        _loadedLine = "";
        _menuWasOpen = false;
    }
}
