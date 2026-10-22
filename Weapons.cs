using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime.Attributes;
using MotherBase.ActionFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// The weapon page: hold the wheel button, then LB, and press Y/X/B/A (keyboard: the weapon keys)
// to switch weapons. The weapon stays equipped, as if picked in the Equipment menu, and attacks
// stay on X. LB is the potion, which is hold-to-drink, so it can't be a tap-or-hold button of its
// own: by default it only opens the page when pressed while the wheel button is already down.
// WeaponButton can instead give the page a button of its own, which works like the wheel button
// (a tap replays its action), except LB, whose tap can't drink: in that mode LB never drinks.
//
// Equipping unloads the old weapon's assets and destroys its attachments (Inventory.
// EquipWeaponFromInventory -> UnequipWeaponInternal -> ItemData.UnloadRefs, ActiveItemData.
// OnUnequipItem), which the game itself only ever does from the paused menu. So a switch waits
// until every weapon action of Rose's is past its swing, and ends what's left of them first.
public partial class Wheel
{
    // A switch waits at most this long on one attack's swing (not counting while the attack
    // button is held: a charge lasts as long as the player wants), and this long overall.
    // Past either it's dropped, not forced.
    private const float WeaponWaitSeconds = 2f;
    private const float WeaponWaitMaxSeconds = 10f;
    // How long the HUD shows the weapon page after a switch, so the new weapon can be seen.
    private const float WeaponShowSeconds = 1.2f;

    private readonly WeaponItemData[] _slotWeapons = new WeaponItemData[4];
    private bool _pageOpen;
    private int _pendingWeaponSlot = -1;
    private WeaponItemData _waitingWeapon;
    private int _waitingSlot;
    private float _waitingSince;
    private float _waitClock;          // when the current blocking attack started counting
    private string _waitingAction;     // that attack's name
    private string _waitingFor;        // the same, with its frames, for the log
    private float _showWeaponsUntil;
    private ActionInputState _attackState;

    // A weapon button of its own (WeaponButton other than the wheel button then LB).
    private bool _weaponDown;
    private float _weaponDownAt;
    private int _weaponDownFrame;
    private readonly List<string> _weaponHeldBack = new();
    private bool _warnedSameButton;

    [HideFromIl2Cpp]
    private bool HasWeapons => Plugin.WeaponPage.Value && !Array.TrueForAll(_slotWeapons, w => w == null);

    // A combination (default: the wheel button, then LB): its second button, while the weapon page
    // is on, has a weapon to offer and isn't the wheel button itself.
    [HideFromIl2Cpp]
    private ButtonControl PageButton()
    {
        var (held, pressed) = Bindings.WeaponCombo();
        if (Merged || held == null || held != Plugin.Modifier.Value || pressed == Plugin.Modifier.Value || !HasWeapons)
            return null;
        return PadButton(pressed);
    }

    // The weapon page's own button, if it has one and a weapon to offer. Until a weapon slot is
    // filled the button keeps its normal action, LB included.
    [HideFromIl2Cpp]
    private ButtonControl WeaponButton()
    {
        var (held, name) = Bindings.WeaponCombo();
        if (Merged || held != null || !HasWeapons)
            return null;
        if (name == Plugin.Modifier.Value)
        {
            if (!_warnedSameButton)
                Plugin.Logger.LogWarning($"[weapon] the weapon button is the wheel button ({name}): weapon wheel off");
            _warnedSameButton = true;
            return null;
        }
        _warnedSameButton = false;
        return PadButton(name);
    }

    // Default mode: the page closes when either button is let go. LB stays held back from the
    // game until it's released (see LetThrough), so letting go of the wheel button first never
    // drinks a potion. Own button: the page is open while it's held, unless the wheel button was
    // pressed after it.
    [HideFromIl2Cpp]
    private void TrackPage()
    {
        var button = WeaponButton();
        bool down = button != null && button.isPressed;
        if (down != _weaponDown)
        {
            _weaponDown = down;
            if (down)
            {
                _weaponDownAt = Time.unscaledTime;
                _weaponDownFrame = Time.frameCount;
                RefreshWeapons();
            }
            else
            {
                // LB's own action is the hold-to-drink potion, which a tap can't do.
                EndHold(_weaponHeldBack, _weaponDownAt, _weaponDownFrame, Bindings.WeaponCombo().Pressed != "leftShoulder");
            }
        }
        if (Merged)
        {
            _pageOpen = false;
            return;
        }
        if (Bindings.WeaponCombo().Held == null)
        {
            bool open = _weaponDown && (!_modifierDown || _weaponDownFrame > _modifierDownFrame);
            if (open != _pageOpen)
                Plugin.Logger.LogDebug($"[weapon] page {(open ? "open" : "closed")}");
            _pageOpen = open;
            return;
        }

        if (!_pageOpen)
            return;
        var page = PageButton();
        if (page == null || !page.isPressed || !_modifierDown)
        {
            _pageOpen = false;
            Plugin.Logger.LogDebug("[weapon] page closed");
        }
    }

    [HideFromIl2Cpp]
    private void RefreshWeapons()
    {
        var inventory = Inventory;
        if (inventory != null)
            Cards.ResolveWeapons(inventory, _slotWeapons);
    }

    [HideFromIl2Cpp]
    private void PollWeaponKeys()
    {
        if (MenuOpen || !GameplayInputActive())
            return;
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if (Keys.Pressed(Plugin.WeaponKeys[slot].Value))
            {
                _pendingWeaponSlot = slot;
                return;
            }
        }
    }

    [HideFromIl2Cpp]
    private void RequestWeapon(int slot)
    {
        RefreshWeapons();
        var weapon = _slotWeapons[slot];
        if (weapon == null)
        {
            Plugin.Logger.LogInfo($"[weapon] {SlotLabels[slot]}: no weapon in this slot");
            return;
        }
        if (Cards.Same(weapon, Inventory.GetEquippedWeapon()))
        {
            if (_waitingWeapon != null)
                Plugin.Logger.LogInfo($"[weapon] {_waitingWeapon.itemID} dropped: {weapon.itemID} is already equipped");
            _waitingWeapon = null;
            Plugin.Logger.LogDebug($"[weapon] {SlotLabels[slot]} {weapon.itemID}: already equipped");
            return;
        }
        _waitingWeapon = weapon;
        _waitingSlot = slot;
        _waitingSince = _waitClock = Time.unscaledTime;
        _waitingAction = null;
        _waitingFor = null;
        _showWeaponsUntil = Time.unscaledTime + WeaponShowSeconds;
        UpdateWeaponSwitch();
    }

    [HideFromIl2Cpp]
    private void UpdateWeaponSwitch()
    {
        if (_waitingWeapon == null)
            return;
        var inventory = Inventory;
        string what = $"{SlotLabels[_waitingSlot]} {_waitingWeapon.itemID}";
        float waited = Time.unscaledTime - _waitingSince;
        if (Cards.Same(inventory.GetEquippedWeapon(), _waitingWeapon))
        {
            Plugin.Logger.LogInfo($"[weapon] {what}: equipped some other way meanwhile");
            _waitingWeapon = null;
            return;
        }

        var recovering = new List<ActionInstance>();
        string busy = SwingingWeaponAction(recovering, out string busyName);
        if (busy != null)
        {
            float now = Time.unscaledTime;
            if (busyName != _waitingAction || AttackHeld())
                _waitClock = now;
            if (busyName != _waitingAction)
                Plugin.Logger.LogDebug($"[weapon] {what} waits for {busy}");
            _waitingAction = busyName;
            _waitingFor = busy;
            if (now - _waitClock < WeaponWaitSeconds && waited < WeaponWaitMaxSeconds)
                return;
            Plugin.Logger.LogInfo($"[weapon] {what} dropped: {busy} after {waited:0.0}s");
            _waitingWeapon = null;
            return;
        }

        // An attack past its chainable frame is only its recovery pose (Whip Attack3 runs to frame
        // 99 but is chainable from 37). The game's CancelWithMovementFeature ends it the same way
        // when the player moves.
        foreach (var action in recovering)
        {
            Plugin.Logger.LogInfo(
                $"[weapon] {what}: ending {action.data.name} at frame {action.frameInfo.integerFrame} " +
                $"(chainable from {action.data.chainableFrame})");
            _actor.StopAction(_entity, action, null);
        }

        var previous = inventory.GetEquippedWeapon();
        var weapon = Cards.Live<WeaponItemData>(_waitingWeapon);
        if (weapon == null)
        {
            Plugin.Logger.LogInfo($"[weapon] {what} dropped: the game unloaded it and it can't be loaded again");
            _waitingWeapon = null;
            return;
        }
        inventory.EquipWeaponFromInventory(weapon);
        bool ok = Cards.Same(inventory.GetEquippedWeapon(), _waitingWeapon);
        Plugin.Logger.LogInfo(
            $"[weapon] {what} {(ok ? "equipped" : "NOT equipped")}, was {previous?.itemID ?? "none"}" +
            (_waitingAction != null ? $" (waited {waited:0.00}s, last for {_waitingAction})" : ""));
        _waitingWeapon = null;
        _showWeaponsUntil = Time.unscaledTime + WeaponShowSeconds;
        if (Plugin.TraceActions.Value)
            _traceUntil = Time.unscaledTime + TraceSeconds;
    }

    // Weapon attack actions are named after the weapon family ("WeaponTwoHandedBase_Attack1_Prepare",
    // "WeaponWhipTier1_Attack3"), not after the item ID. Returns the first one still before its
    // chainable frame (the swing itself); the ones past it go into recovering.
    [HideFromIl2Cpp]
    private string SwingingWeaponAction(List<ActionInstance> recovering, out string blockingName)
    {
        blockingName = null;
        var active = _actor.activeActions;
        for (int i = 0; i < active.Count; i++)
        {
            var action = active[i];
            var data = action?.data;
            string name = data?.name;
            if (name == null || !name.StartsWith("Weapon", StringComparison.Ordinal))
                continue;
            int frame = action.frameInfo.integerFrame;
            if (data.chainableFrame > 0 && frame >= data.chainableFrame)
            {
                recovering.Add(action);
                continue;
            }
            blockingName = name;
            return $"{name} (frame {frame}, chainable from {data.chainableFrame})";
        }
        return null;
    }

    // Attack is held (X / left mouse button), as the game's charge features read it.
    [HideFromIl2Cpp]
    private bool AttackHeld()
    {
        if (_attackState == null)
        {
            foreach (var state in _input.data.inputStates)
            {
                if (state?.input?.action?.name == "AttackPrimary")
                    _attackState = state.state;
            }
            if (_attackState == null)
                return false;
        }
        return _input.GetInputState(_attackState).isDown;
    }

    [HideFromIl2Cpp]
    private void ForgetWeapons()
    {
        _pageOpen = false;
        _weaponDown = false;
        _weaponHeldBack.Clear();
        _attackState = null;
        _pendingWeaponSlot = -1;
        _waitingWeapon = null;
        _showWeaponsUntil = 0f;
    }

    [HideFromIl2Cpp]
    private bool ShowingWeapons =>
        _pageOpen || WeaponsMenuOpen ||
        (!SpellsMenuOpen && (_waitingWeapon != null || Time.unscaledTime < _showWeaponsUntil));
}
