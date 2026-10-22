using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// Assigning cards in the Arcanas (Spells) menu and weapons in the Equipment menu, where L3 is
// free. While a slot is being chosen, the menu's own actions on those buttons and keys are
// switched off so the press only assigns; they come back once it's released.
public partial class Wheel
{
    // From the game's Player_InputBindings: face buttons, Esc, tabs (Q/E, LB/RB) and sub-tabs
    // (1/3, LT/RT), which double as keyboard slot keys, and navigation, for slots on the D-pad.
    internal static readonly (string Map, string Action)[] BlockedWhileAssigning =
    {
        ("UI", "Validate"), ("UI", "Close"), ("UI", "GameMenuClose"), ("UI", "MenuContextualValidate"),
        ("UI", "MenuContextualCancel"), ("UI", "MenuContextualReset"), ("UI", "Map_PlaceMarker"),
        ("UI", "Map_Interact"), ("UI", "Map_UpperMap"), ("UI", "PreviousTab"), ("UI", "NextTab"),
        ("UI", "PreviousSubTab"), ("UI", "NextSubTab"), ("UI", "OpenInventory"), ("Menu", "OpenInventory"),
        ("UI", "Navigate"), ("UI", "NavigateUp"), ("UI", "NavigateDown"), ("UI", "NavigateLeft"), ("UI", "NavigateRight"),
    };

    private InventorySpellMenuManager _spellsMenu;
    private InventoryWeaponMenuManager _weaponsMenu;
    private bool _assigning;
    private ItemData _assignCard;
    private ConfigEntry<string>[] _assignSlots;   // Plugin.Slots or Plugin.WeaponSlots
    private bool _waitFaceRelease;
    private readonly List<InputAction> _blockedUiActions = new();

    [HideFromIl2Cpp]
    internal void OnSpellsMenuOpened(InventorySpellMenuManager menu) => _spellsMenu = menu;

    [HideFromIl2Cpp]
    internal void OnWeaponsMenuOpened(InventoryWeaponMenuManager menu) => _weaponsMenu = menu;

    [HideFromIl2Cpp]
    internal bool BlocksMenuClicks => _assigning || _waitFaceRelease;

    [HideFromIl2Cpp]
    private bool SpellsMenuOpen =>
        _spellsMenu != null && _spellsMenu.opened && _spellsMenu.gameObject.activeInHierarchy;

    [HideFromIl2Cpp]
    private bool WeaponsMenuOpen =>
        _weaponsMenu != null && _weaponsMenu.opened && _weaponsMenu.gameObject.activeInHierarchy;

    [HideFromIl2Cpp]
    private bool MenuOpen => SpellsMenuOpen || WeaponsMenuOpen;

    // The highlighted card or weapon, if the player owns it.
    [HideFromIl2Cpp]
    private ItemData Highlighted =>
        SpellsMenuOpen ? _spellsMenu.selectedInventoryItemSlot?._item
        : WeaponsMenuOpen ? _weaponsMenu.selectedInventoryItemSlot?._item
        : null;

    [HideFromIl2Cpp]
    private void UpdateAssign()
    {
        var pad = Gamepad.current;
        if (_waitFaceRelease)
        {
            if (!AnyAssignInputHeld(pad))
            {
                _waitFaceRelease = false;
                UnblockUi();
            }
            return;
        }

        if (!MenuOpen)
        {
            if (_assigning)
                EndAssign("the menu closed");
            return;
        }

        bool assignPressed = (pad != null && pad.leftStickButton.wasPressedThisFrame) || Keys.Pressed(Plugin.AssignKey.Value);
        if (!_assigning)
        {
            if (assignPressed)
                StartAssign(Highlighted, SpellsMenuOpen ? Plugin.Slots : Plugin.WeaponSlots);
            return;
        }

        // Slot keys first: the assign key may also be a slot key.
        bool weapons = _assignSlots == Plugin.WeaponSlots;
        var slotKeys = weapons ? Plugin.WeaponKeys : Plugin.SlotKeys;
        var slotButtons = weapons ? Plugin.WeaponSlotButtons : Plugin.SlotButtons;
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if ((PadButton(slotButtons[slot].Value)?.wasPressedThisFrame ?? false) || Keys.Pressed(slotKeys[slot].Value))
            {
                Assign(slot);
                return;
            }
        }
        if (assignPressed || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            EndAssign("cancelled");
    }

    [HideFromIl2Cpp]
    private void StartAssign(ItemData card, ConfigEntry<string>[] slots)
    {
        if (card == null)
        {
            Plugin.Logger.LogInfo("[assign] nothing highlighted");
            return;
        }
        // The Equipment menu also lists weapons not found yet.
        if (slots == Plugin.WeaponSlots && !Inventory.HasItemInInventoryOrEquipped(card))
        {
            Plugin.Logger.LogInfo($"[assign] {card.itemID}: not owned");
            return;
        }
        BlockUi(BlockedWhileAssigning);
        _assigning = true;
        _assignCard = card;
        _assignSlots = slots;
        Plugin.Logger.LogInfo($"[assign] {card.itemID}: waiting for a slot button");
    }

    [HideFromIl2Cpp]
    private void EndAssign(string reason)
    {
        Plugin.Logger.LogInfo($"[assign] {_assignCard?.itemID ?? "-"} not assigned: {reason}");
        _assigning = false;
        _assignCard = null;
        _assignSlots = null;
        if (AnyAssignInputHeld(Gamepad.current))
            _waitFaceRelease = true;
        else
            UnblockUi();
    }

    [HideFromIl2Cpp]
    private void Assign(int slot)
    {
        var slots = _assignSlots;
        string kind = slots == Plugin.WeaponSlots ? "weapon " : "";
        string id = _assignCard.itemID;
        string old = slots[slot].Value.Trim();
        int had = Array.FindIndex(slots, s => string.Equals(s.Value.Trim(), id, StringComparison.OrdinalIgnoreCase));

        string note = "";
        if (had == slot)
        {
            note = " (already there)";
        }
        else
        {
            if (had >= 0)
            {
                slots[had].Value = old;   // saves the config file
                note = $", {SlotLabels[had]} now {(old.Length == 0 ? "empty" : old)}";
            }
            slots[slot].Value = id;
        }

        Plugin.Logger.LogInfo($"[assign] {kind}{SlotLabels[slot]} = {id}{note}");
        _assigning = false;
        _assignCard = null;
        _assignSlots = null;
        _waitFaceRelease = true;   // unblock the menu once the slot button is up
        RefreshSlots();
        RefreshWeapons();
    }

    // Only actions that are on, so unblocking restores exactly what was there.
    [HideFromIl2Cpp]
    private void BlockUi((string Map, string Action)[] blocked)
    {
        _blockedUiActions.Clear();
        var asset = _input?.data?.moveInput?.asset;
        if (asset == null)
        {
            Plugin.Logger.LogWarning("[assign] menu controls not found, menu buttons stay active");
            return;
        }
        foreach (var (mapName, actionName) in blocked)
        {
            var action = asset.FindActionMap(mapName, false)?.FindAction(actionName, false);
            if (action != null && action.enabled)
            {
                action.Disable();
                _blockedUiActions.Add(action);
            }
        }
    }

    // Not while their map is off (menu closed): the game turns them on with the map.
    [HideFromIl2Cpp]
    private void UnblockUi()
    {
        foreach (var action in _blockedUiActions)
        {
            if (action.actionMap != null && action.actionMap.enabled)
                action.Enable();
        }
        _blockedUiActions.Clear();
    }

    [HideFromIl2Cpp]
    private void CancelAssign()
    {
        _assigning = false;
        _assignCard = null;
        _assignSlots = null;
        _waitFaceRelease = false;
        UnblockUi();
    }

    // Laid out like the game's own prompts ("Back" + button).
    [HideFromIl2Cpp]
    private string MenuHint()
    {
        bool keyboard = Keys.KeyboardMode;
        if (_assigning)
        {
            bool weapons = _assignSlots == Plugin.WeaponSlots;
            var keys = weapons ? Plugin.WeaponKeys : Plugin.SlotKeys;
            var buttons = weapons ? Plugin.WeaponSlotButtons : Plugin.SlotButtons;
            string slots = keyboard
                ? string.Join(" ", Array.ConvertAll(keys, k => Keys.Label(k.Value)))
                : string.Join(" ", Array.ConvertAll(buttons, b => Glyphs.Tag(b.Value)));
            return $"{Texts.Get(Texts.ChooseSlot)}  {slots}      {Texts.Get(Texts.Cancel)}  {(keyboard ? "Esc" : Glyphs.Tag("leftStickPress"))}";
        }
        return Highlighted == null ? "" :
            $"{Texts.Get(Texts.AddToWheel)}  {(keyboard ? Keys.Label(Plugin.AssignKey.Value) : Glyphs.Tag("leftStickPress"))}";
    }

    [HideFromIl2Cpp]
    private static bool AnyAssignInputHeld(Gamepad pad)
    {
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if ((PadButton(Plugin.SlotButtons[slot].Value)?.isPressed ?? false) ||
                (PadButton(Plugin.WeaponSlotButtons[slot].Value)?.isPressed ?? false) ||
                Keys.Held(Plugin.SlotKeys[slot].Value) ||
                Keys.Held(Plugin.WeaponKeys[slot].Value))
                return true;
        }
        return Keys.Held(Plugin.AssignKey.Value) || (Keyboard.current?.escapeKey.isPressed ?? false);
    }
}
