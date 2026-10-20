using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// Assigning cards in the Arcanas (Spells) menu, where L3 is free. While a slot is being chosen,
// the menu's own actions on those buttons and keys are switched off so the press only assigns;
// they come back once it's released.
public partial class Wheel
{
    // From the game's Player_InputBindings: face buttons, Esc, tabs (Q/E, LB/RB) and sub-tabs
    // (1/3, LT/RT), which double as keyboard slot keys.
    internal static readonly (string Map, string Action)[] BlockedWhileAssigning =
    {
        ("UI", "Validate"), ("UI", "Close"), ("UI", "GameMenuClose"), ("UI", "MenuContextualValidate"),
        ("UI", "MenuContextualCancel"), ("UI", "MenuContextualReset"), ("UI", "Map_PlaceMarker"),
        ("UI", "Map_Interact"), ("UI", "Map_UpperMap"), ("UI", "PreviousTab"), ("UI", "NextTab"),
        ("UI", "PreviousSubTab"), ("UI", "NextSubTab"), ("UI", "OpenInventory"), ("Menu", "OpenInventory"),
    };

    private InventorySpellMenuManager _spellsMenu;
    private bool _assigning;
    private SpellItemData _assignCard;
    private bool _waitFaceRelease;
    private readonly List<InputAction> _blockedUiActions = new();

    [HideFromIl2Cpp]
    internal void OnSpellsMenuOpened(InventorySpellMenuManager menu) => _spellsMenu = menu;

    [HideFromIl2Cpp]
    internal bool BlocksMenuClicks => _assigning || _waitFaceRelease;

    [HideFromIl2Cpp]
    private bool SpellsMenuOpen =>
        _spellsMenu != null && _spellsMenu.opened && _spellsMenu.gameObject.activeInHierarchy;

    [HideFromIl2Cpp]
    private SpellItemData HighlightedCard => SpellsMenuOpen ? _spellsMenu.selectedInventoryItemSlot?._item : null;

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

        if (!SpellsMenuOpen)
        {
            if (_assigning)
                EndAssign("the Arcanas menu closed");
            return;
        }

        bool assignPressed = (pad != null && pad.leftStickButton.wasPressedThisFrame) || Keys.Pressed(Plugin.AssignKey.Value);
        if (!_assigning)
        {
            if (assignPressed)
                StartAssign(HighlightedCard);
            return;
        }

        // Slot keys first: the assign key may also be a slot key.
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if ((pad != null && FaceButton(pad, slot).wasPressedThisFrame) || Keys.Pressed(Plugin.SlotKeys[slot].Value))
            {
                Assign(slot);
                return;
            }
        }
        if (assignPressed || (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame))
            EndAssign("cancelled");
    }

    [HideFromIl2Cpp]
    private void StartAssign(SpellItemData card)
    {
        if (card == null)
        {
            Plugin.Logger.LogInfo("[assign] no card highlighted");
            return;
        }
        BlockUi(BlockedWhileAssigning);
        _assigning = true;
        _assignCard = card;
        Plugin.Logger.LogInfo($"[assign] {card.itemID}: waiting for Y/X/B/A");
    }

    [HideFromIl2Cpp]
    private void EndAssign(string reason)
    {
        Plugin.Logger.LogInfo($"[assign] {_assignCard?.itemID ?? "-"} not assigned: {reason}");
        _assigning = false;
        _assignCard = null;
        if (AnyAssignInputHeld(Gamepad.current))
            _waitFaceRelease = true;
        else
            UnblockUi();
    }

    [HideFromIl2Cpp]
    private void Assign(int slot)
    {
        string id = _assignCard.itemID;
        string old = Plugin.Slots[slot].Value.Trim();
        int had = Array.FindIndex(Plugin.Slots, s => string.Equals(s.Value.Trim(), id, StringComparison.OrdinalIgnoreCase));

        string note = "";
        if (had == slot)
        {
            note = " (already there)";
        }
        else
        {
            if (had >= 0)
            {
                Plugin.Slots[had].Value = old;   // saves the config file
                note = $", {SlotLabels[had]} now {(old.Length == 0 ? "empty" : old)}";
            }
            Plugin.Slots[slot].Value = id;
        }

        Plugin.Logger.LogInfo($"[assign] {SlotLabels[slot]} = {id}{note}");
        _assigning = false;
        _assignCard = null;
        _waitFaceRelease = true;   // unblock the menu once the slot button is up
        RefreshSlots();
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
            string slots = keyboard
                ? string.Join(" ", Array.ConvertAll(Plugin.SlotKeys, k => Keys.Label(k.Value)))
                : "Y X B A";
            return $"Choose a slot  {slots}      Cancel  {(keyboard ? "Esc" : "L3")}";
        }
        return HighlightedCard == null ? "" : $"Add to wheel  {(keyboard ? Keys.Label(Plugin.AssignKey.Value) : "L3")}";
    }

    [HideFromIl2Cpp]
    private static ButtonControl FaceButton(Gamepad pad, int slot) => slot switch
    {
        0 => pad.buttonNorth,
        1 => pad.buttonWest,
        2 => pad.buttonEast,
        _ => pad.buttonSouth,
    };

    [HideFromIl2Cpp]
    private static bool AnyAssignInputHeld(Gamepad pad)
    {
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if ((pad != null && FaceButton(pad, slot).isPressed) || Keys.Held(Plugin.SlotKeys[slot].Value))
                return true;
        }
        return Keys.Held(Plugin.AssignKey.Value) || (Keyboard.current?.escapeKey.isPressed ?? false);
    }
}
