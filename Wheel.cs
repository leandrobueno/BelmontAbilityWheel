using System;
using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime;
using Il2CppInterop.Runtime.Attributes;
using MotherBase.ActionFramework;
using MotherBase.EntityFramework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using GamePlayerInput = MotherBase.ActionFramework.PlayerInput;

namespace AbilityWheel;

// A wheel cast equips the slot's card and starts the Spell button's own trigger, so only one
// card is ever equipped, as in the base game: the game charges that card's mana and saves stay
// unchanged.
//
// KeepHomeCard: the Spells-menu card is the "home" card Y casts. A wheel card stays equipped
// ("on loan") while its spell plays, so its passives apply to that cast. The home card goes back
// on before the game reads a Y press and before every save.
public partial class Wheel : MonoBehaviour
{
    // Needed by IL2CPP for a class injected with AddComponent.
    public Wheel(IntPtr pointer) : base(pointer) { }

    internal static Wheel Instance;

    internal static readonly string[] SlotLabels = { "Y", "X", "B", "A" };
    private static readonly string[] FaceButtons = { "buttonNorth", "buttonWest", "buttonEast", "buttonSouth" };

    // A cast refused because Rose is busy is retried this long, as the game buffers a press.
    private const float RetrySeconds = 0.25f;
    private const float RetryInterval = 1f / 30f;   // the game's simulation step
    private const float SpellEndedSeconds = 0.1f;
    // Only a safety net for a spell whose actions never end; long enough for effects that run on
    // Rose for a while (Wheel of Chaos' wheels).
    private const float HomeTimeoutSeconds = 20f;

    private Entity _entity;
    private Actor _actor;
    private GamePlayerInput _input;
    private InventoryComponent _inventoryComponent;
    private StatsStateComponent _stats;
    private ActionTrigger _spellTrigger;
    private string _spellActionName;
    private float _nextLookup;
    private bool _warnedIncomplete;

    private bool _modifierDown;
    private float _modifierDownAt;
    private int _modifierDownFrame;
    private int _lastWheelPressFrame = -1;
    private int _pendingSlot = -1;
    private readonly List<string> _heldBackActions = new();
    private readonly HashSet<string> _swallowed = new();   // press kept from the game, so its release is too
    private readonly Dictionary<string, int> _releaseSwallowedFrame = new();

    private SpellItemData _castSpell;
    private SpellItemData _castPrevious;
    private int _castSlot;
    private bool _castWasEquipped;
    private float _castUntil;
    private float _nextTry;

    private SpellItemData _home;
    private SpellItemData _onLoan;
    private string _lostHomeId;
    private string _lostLoanId;
    private bool _loanSticky;
    private readonly List<string> _loanPrefixes = new();
    private float _loanedAt;
    private float _loanLastSeen;

    private readonly SpellItemData[] _slotSpells = new SpellItemData[4];
    private readonly int[] _slotCosts = new int[4];
    private Hud _hud;
    private bool _hudBroken;

    private const float TraceSeconds = 4f;
    private ActionInputState _spellState;
    private bool _spellWasDown;
    private float _traceUntil;
    private string[] _tracedActions = Array.Empty<string>();
    private readonly HashSet<string> _describedCards = new();

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        try
        {
            Keys.TrackDevice();
            if (!FindPlayer())
            {
                HideHud();
                return;
            }

            TrackHome();
            UpdateAssign();
            TrackModifier();
            TrackPage();
            if (_pendingWeaponSlot < 0)
                PollWeaponKeys();
            if (_pendingWeaponSlot >= 0)
            {
                int weaponSlot = _pendingWeaponSlot;
                _pendingWeaponSlot = -1;
                RequestWeapon(weaponSlot);
            }
            else
            {
                UpdateWeaponSwitch();
            }
            if (_pendingSlot < 0)
                PollSlotKeys();
            if (_pendingSlot >= 0)
            {
                int slot = _pendingSlot;
                _pendingSlot = -1;
                StartCast(slot);
            }
            else if (_castSpell != null && Time.unscaledTime >= _nextTry)
            {
                TryCast();
            }
            UpdateHold();
            WatchLoan();
            if (Plugin.TraceActions.Value)
                Trace();
            UpdateHud();
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[wheel] update failed: {e}");
            Forget();
        }
    }

    // Patches.cs input filter: false keeps the press or release from the game.
    [HideFromIl2Cpp]
    internal bool LetThrough(GamePlayerInput input, InputAction.CallbackContext ctx)
    {
        if (_input == null || input == null || input.Pointer != _input.Pointer)
            return true;
        var action = ctx.action;
        if (action == null)
            return true;
        string name = action.name;

        // A release goes where its press went.
        if (ctx.phase == InputActionPhase.Canceled)
        {
            if (_swallowed.Remove(name))
            {
                if (Plugin.TraceActions.Value)
                    Plugin.Logger.LogInfo($"[trace] t={Time.unscaledTime:0.00} {name} released (held back from the game)");
                _releaseSwallowedFrame[name] = Time.frameCount;
                return false;
            }
            return !(_releaseSwallowedFrame.TryGetValue(name, out int frame) && frame == Time.frameCount);
        }

        var control = ctx.control;
        var modifier = ModifierButton();
        if (control != null && modifier != null)
        {
            // The wheel button's own action only happens on a tap release (OnModifierReleased).
            if (control.Pointer == modifier.Pointer)
            {
                if (!_heldBackActions.Contains(name))
                    _heldBackActions.Add(name);
                _swallowed.Add(name);
                return false;
            }

            bool sameDevice = control.device != null && control.device.Pointer == modifier.device.Pointer;

            // LB pressed while the wheel button is down opens the weapon page instead of drinking.
            // The wheel counts as used, so the wheel button's release won't replay Interact.
            var page = PageButton();
            if (page != null && control.Pointer == page.Pointer && modifier.isPressed && sameDevice)
            {
                _swallowed.Add(name);
                if (!_pageOpen)
                {
                    _pageOpen = true;
                    _lastWheelPressFrame = Time.frameCount;
                    Plugin.Logger.LogDebug($"[weapon] page open ({name} held back)");
                }
                return false;
            }

            int slot = Array.IndexOf(FaceButtons, control.name);
            if (slot >= 0 && modifier.isPressed && sameDevice)
            {
                _swallowed.Add(name);
                if (ctx.phase == InputActionPhase.Performed)
                {
                    _lastWheelPressFrame = Time.frameCount;
                    if (_pageOpen)
                    {
                        _pendingWeaponSlot = slot;
                    }
                    else
                    {
                        _pendingSlot = slot;
                        _pendingControl = control.TryCast<ButtonControl>();
                    }
                }
                return false;
            }
        }

        // A press let through gets its release through too. Y casts the home card, so it goes
        // back on before the game reads the press.
        _swallowed.Remove(name);
        if (name == _spellActionName)
            ReturnHome("Y pressed");
        return true;
    }

    // Rose is replaced when a save loads or she respawns, so she's looked up again.
    [HideFromIl2Cpp]
    private bool FindPlayer()
    {
        if (_entity != null && _inventoryComponent != null && _inventoryComponent.inventory != null)
            return true;
        if (_entity != null || _actor != null)
            Forget();
        if (Time.unscaledTime < _nextLookup)
            return false;
        _nextLookup = Time.unscaledTime + 0.5f;

        Entity entity = null;
        try
        {
            if (!MainCharacterSingleton.TryGetEntity(out entity) || entity == null)
                return false;
        }
        catch
        {
            return false;
        }

        // Right after a save loads, Rose exists before her inventory does.
        var inventoryComponent = entity.GetComponent<InventoryComponent>();
        if (inventoryComponent == null || inventoryComponent.inventory == null)
            return false;

        var input = entity.GetComponent<GamePlayerInput>();
        var actor = entity.GetComponent<Actor>();
        var spellBinding = SpellBinding(input);
        var trigger = spellBinding?.action;
        if (input == null || actor == null || trigger == null)
        {
            if (!_warnedIncomplete)
                Plugin.Logger.LogWarning(
                    $"[wheel] Rose found but incomplete: input {(input != null)}, actor {(actor != null)}, spell trigger {(trigger != null)}");
            _warnedIncomplete = true;
            return false;
        }

        _entity = entity;
        _actor = actor;
        _input = input;
        _inventoryComponent = inventoryComponent;
        _stats = entity.GetComponent<StatsStateComponent>();
        _spellTrigger = trigger;
        _spellActionName = spellBinding.input?.action?.name;
        _spellState = null;
        foreach (var state in input.data.inputStates)
        {
            if (state?.input?.action?.name == _spellActionName)
                _spellState = state.state;
        }
        FindSpellStateIndex();
        _home = inventoryComponent.inventory.GetEquippedSpell();
        RestoreLostHome(inventoryComponent.inventory);
        _warnedIncomplete = false;
        Plugin.Logger.LogInfo(
            $"[wheel] ready on '{entity.gameObject.name}', casting through {trigger.name}, " +
            $"home card {_home?.itemID ?? "none"}, weapon {inventoryComponent.inventory.GetEquippedWeapon()?.itemID ?? "none"}");
        RefreshSlots();
        RefreshWeapons();
        return true;
    }

    // Rose was lost while a wheel card was on loan, so the card she comes back with isn't the
    // home card.
    [HideFromIl2Cpp]
    private void RestoreLostHome(Inventory inventory)
    {
        if (_lostHomeId != null && _home?.itemID == _lostLoanId)
        {
            var home = Cards.Owned(inventory).FirstOrDefault(s => s.itemID == _lostHomeId);
            if (home != null)
            {
                inventory.EquipSpellFromInventory(home);
                _home = home;
                Plugin.Logger.LogInfo($"[home] {_lostLoanId} -> {_lostHomeId}: on loan when Rose was reloaded");
            }
        }
        _lostHomeId = _lostLoanId = null;
    }

    [HideFromIl2Cpp]
    private void Forget()
    {
        // The held Spell state lives in the game, so it must not outlive the hold.
        try
        {
            DropHold();
        }
        catch
        {
            _holdControl = null;
        }
        if (_onLoan != null && _home != null)
        {
            _lostHomeId = _home.itemID;
            _lostLoanId = _onLoan.itemID;
        }
        _entity = null;
        _actor = null;
        _input = null;
        _inventoryComponent = null;
        _stats = null;
        _spellTrigger = null;
        _modifierDown = false;
        _pendingSlot = -1;
        _castSpell = null;
        _holdControl = null;
        _pendingControl = null;
        _home = null;
        _onLoan = null;
        _loanSticky = false;
        _heldBackActions.Clear();
        _swallowed.Clear();
        ForgetWeapons();
        CancelAssign();
        HideHud();
    }

    [HideFromIl2Cpp]
    private Inventory Inventory => _inventoryComponent?.inventory;

    [HideFromIl2Cpp]
    private static PlayerInputData.ActionBinding SpellBinding(GamePlayerInput input)
    {
        var bindings = input?.data?.inputBindings;
        if (bindings == null)
            return null;
        foreach (var binding in bindings)
        {
            var useItem = binding?.action?.TryCast<UseItem>();
            if (useItem != null && useItem.itemType == UseItem.UseItemType.Spell)
                return binding;
        }
        return null;
    }

    // A card equipped some other way during a loan (the Spells menu) becomes the home card.
    [HideFromIl2Cpp]
    private void TrackHome()
    {
        var equipped = Inventory.GetEquippedSpell();
        if (_onLoan == null && _castSpell == null)
        {
            _home = equipped;
            return;
        }
        if (Cards.Same(equipped, _onLoan) || Cards.Same(equipped, _castSpell) || Cards.Same(equipped, _home))
            return;

        Plugin.Logger.LogInfo($"[home] {equipped?.itemID ?? "none"} was equipped elsewhere, it's the home card now");
        _home = equipped;
        _onLoan = null;
        _loanSticky = false;
    }

    // The loan ends once none of Rose's actions carry the spell's name. That also covers effects
    // that keep an action running on her, like Wheel of Chaos' SpawnWheelIdle while its wheels
    // are out, so that card needs no StayEquipped. StayEquipped is for effects that leave nothing
    // running on her but vanish when the card is unequipped (Mirror Walk's mirror).
    [HideFromIl2Cpp]
    private void WatchLoan()
    {
        if (_onLoan == null || _castSpell != null || _loanSticky)
            return;

        float now = Time.unscaledTime;
        if (SpellRunning(_onLoan))
            _loanLastSeen = now;

        if (now - _loanLastSeen >= SpellEndedSeconds)
            ReturnHome("spell ended");
        else if (now - _loanedAt >= HomeTimeoutSeconds)
            ReturnHome("timed out");
    }

    [HideFromIl2Cpp]
    private bool SpellRunning(SpellItemData spell)
    {
        var active = _actor.activeActions;
        for (int i = 0; i < active.Count; i++)
        {
            string name = active[i]?.data?.name;
            if (name == null)
                continue;
            if (name.StartsWith(spell.itemID, StringComparison.Ordinal))
                return true;
            foreach (var prefix in _loanPrefixes)
            {
                if (name.StartsWith(prefix, StringComparison.Ordinal))
                    return true;
            }
        }
        return false;
    }

    [HideFromIl2Cpp]
    private HashSet<string> ActiveActionNames()
    {
        var names = new HashSet<string>();
        var active = _actor.activeActions;
        for (int i = 0; i < active.Count; i++)
        {
            string name = active[i]?.data?.name;
            if (name != null)
                names.Add(name);
        }
        return names;
    }

    // Action names don't always start with the card ID (TargetingChargingWheels' are
    // "TargetingCharchingWheels_..."), so the prefixes of the actions the cast started count too.
    // MC_ actions are Rose's own.
    [HideFromIl2Cpp]
    private void RememberSpellActions(HashSet<string> before)
    {
        _loanPrefixes.Clear();
        foreach (var name in ActiveActionNames())
        {
            if (before.Contains(name))
                continue;
            int cut = name.IndexOf('_');
            string prefix = cut > 0 ? name.Substring(0, cut) : name;
            if (prefix != "MC" && !_loanPrefixes.Contains(prefix))
                _loanPrefixes.Add(prefix);
        }
    }

    [HideFromIl2Cpp]
    internal void ReturnHome(string reason)
    {
        if (_onLoan == null || _home == null)
            return;

        if (_castSpell != null)
        {
            Plugin.Logger.LogInfo($"[cast] {SlotLabels[_castSlot]} {_castSpell.itemID} dropped: {reason}");
            _castSpell = null;
            DropHold();
        }

        var inventory = Inventory;
        if (inventory != null && !Cards.Same(inventory.GetEquippedSpell(), _home))
            inventory.EquipSpellFromInventory(Cards.Live<SpellItemData>(_home) ?? _home);
        Plugin.Logger.LogInfo(
            $"[home] {_onLoan.itemID} -> {_home.itemID}: {reason}, {Time.unscaledTime - _loanedAt:0.00}s after the cast");
        _onLoan = null;
        _loanSticky = false;
    }

    [HideFromIl2Cpp]
    private static ButtonControl ModifierButton()
    {
        var pad = Gamepad.current;
        if (pad == null)
            return null;
        return Plugin.Modifier.Value switch
        {
            "leftShoulder" => pad.leftShoulder,
            "rightTrigger" => pad.rightTrigger,
            "leftTrigger" => pad.leftTrigger,
            "leftStickPress" => pad.leftStickButton,
            "rightStickPress" => pad.rightStickButton,
            "select" => pad.selectButton,
            _ => pad.rightShoulder,
        };
    }

    [HideFromIl2Cpp]
    private void TrackModifier()
    {
        var button = ModifierButton();
        bool down = button != null && button.isPressed;
        if (down == _modifierDown)
            return;

        _modifierDown = down;
        if (!down)
        {
            OnModifierReleased();
            return;
        }

        _modifierDownAt = Time.unscaledTime;
        _modifierDownFrame = Time.frameCount;
        // No Settings.Reload() here: it could revert unsaved MODS-menu edits.
        RefreshSlots();
        RefreshWeapons();
        Plugin.Logger.LogDebug(
            $"[wheel] {Plugin.Modifier.Value} down: gameplay input {GameplayInputActive()} " +
            $"(listening {_input._isListeningUnityInput}, Player map {PlayerMapEnabled()}, time scale {Time.timeScale:0.##})");
    }

    // Gameplay only: in menus the number keys switch sub-tabs.
    [HideFromIl2Cpp]
    private void PollSlotKeys()
    {
        if (MenuOpen || !GameplayInputActive())
            return;
        for (int slot = 0; slot < SlotLabels.Length; slot++)
        {
            if (Keys.Pressed(Plugin.SlotKeys[slot].Value))
            {
                _pendingSlot = slot;
                _pendingControl = Keys.Control(Plugin.SlotKeys[slot].Value);
                return;
            }
        }
    }

    [HideFromIl2Cpp]
    private void OnModifierReleased()
    {
        if (_heldBackActions.Count == 0)
            return;

        float held = Time.unscaledTime - _modifierDownAt;
        bool usedWheel = _lastWheelPressFrame >= _modifierDownFrame;
        if (!usedWheel && held <= Plugin.TapSeconds.Value)
        {
            foreach (var name in _heldBackActions)
                Replay(name);
        }
        else
        {
            Plugin.Logger.LogDebug(
                $"[tap] {string.Join(", ", _heldBackActions)} not done:{(usedWheel ? "the wheel was used" : $"held {held:0.00}s")}");
        }
        _heldBackActions.Clear();
    }

    // What the game does for a press of this input action.
    [HideFromIl2Cpp]
    private void Replay(string actionName)
    {
        foreach (var binding in _input.data.inputBindings)
        {
            if (binding?.input?.action?.name != actionName || binding.inputPhase == InputActionPhase.Canceled)
                continue;

            if (binding.bindingType == PlayerInputData.BindingType.SendSignal)
            {
                bool handled = binding.signal != null && _actor.TryProcessSignal(_entity, binding.signal);
                Plugin.Logger.LogDebug($"[tap] {actionName}: signal {binding.signal?.name ?? "-"} handled {handled}");
            }
            else
            {
                bool started = binding.action != null && _actor.TryStartAction(_entity, binding.action);
                Plugin.Logger.LogDebug($"[tap] {actionName}: {binding.action?.name ?? "-"} started {started}");
            }
        }
    }

    [HideFromIl2Cpp]
    private void StartCast(int slot)
    {
        var inventory = Inventory;
        RefreshSlots();
        var spell = _slotSpells[slot];
        if (spell == null)
        {
            Plugin.Logger.LogInfo($"[cast] {SlotLabels[slot]}: no card in this slot");
            return;
        }

        // Checked before swapping, because unequipping a card removes its effect that is still
        // out (Mirror Walk's mirror). Mana drained over time is left to the game.
        if (spell.manaConsumptionType == SpellItemData.ManaConsumptionType.Instant && Cost(spell) > Mana())
        {
            Plugin.Logger.LogInfo($"[cast] {SlotLabels[slot]} {spell.itemID} not cast (not enough mana: {Mana():0}, cost {Cost(spell)})");
            return;
        }

        // What a refusal puts back. A press that replaces a waiting cast keeps that cast's
        // previous card, since the equipped one is the waiting card. A card already on isn't
        // swapped at all: its own spell may still be running.
        var equipped = inventory.GetEquippedSpell();
        bool keepHome = Plugin.KeepHomeCard.Value && _home != null;
        var previous = keepHome ? _home : _castSpell != null ? _castPrevious : equipped;
        _castWasEquipped = Cards.Same(spell, equipped);
        if (!_castWasEquipped)
            inventory.EquipSpellFromInventory(spell);

        _onLoan = keepHome && !Cards.Same(spell, _home) ? spell : null;
        _loanedAt = Time.unscaledTime;
        _castSpell = spell;
        _castPrevious = previous;
        _castSlot = slot;
        _castUntil = Time.unscaledTime + RetrySeconds;
        StartHold();
        if (Plugin.TraceActions.Value)
        {
            _traceUntil = Time.unscaledTime + TraceSeconds;
            DescribeOnce(spell);
        }
        TryCast();
    }

    [HideFromIl2Cpp]
    private void TryCast()
    {
        var inventory = Inventory;
        string what = $"{SlotLabels[_castSlot]} {_castSpell.itemID}";
        if (!Cards.Same(inventory.GetEquippedSpell(), _castSpell))
        {
            Plugin.Logger.LogInfo($"[cast] {what} dropped: the equipped card changed");
            _castSpell = null;
            DropHold();
            return;
        }

        var before = ActiveActionNames();
        if (_actor.TryStartAction(_entity, _spellTrigger))
        {
            _loanedAt = _loanLastSeen = Time.unscaledTime;
            // A StayEquipped card's next cast while it's still on is its follow-up (Mirror
            // Walk's teleport). After that the loan ends the usual way, or the card would stay
            // on until another card is cast.
            bool followUp = _castWasEquipped && _loanSticky;
            _loanSticky = _onLoan != null && Cards.StaysEquipped(_castSpell) && !followUp;
            if (_onLoan != null)
                RememberSpellActions(before);
            Plugin.Logger.LogInfo(
                $"[cast] {what} cast, mana now {Mana():0}" +
                (_loanSticky ? ", stays equipped until it's used again, Y, another card or a save"
                    : followUp ? ", its follow-up: the Y card comes back when it ends" : ""));
            _castSpell = null;
            return;
        }

        var actionData = _spellTrigger.GetActionData(_entity);
        ActionStartResult? result = actionData == null ? null : _actor.CheckStartAction(_entity, actionData);
        bool busy = result is ActionStartResult.LayerIssue or ActionStartResult.ChainableIssue;
        if (busy && Time.unscaledTime < _castUntil)
        {
            _nextTry = Time.unscaledTime + RetryInterval;
            return;
        }

        string back = "";
        if (!_castWasEquipped && _castPrevious != null && !Cards.Same(_castPrevious, inventory.GetEquippedSpell()))
        {
            inventory.EquipSpellFromInventory(Cards.Live<SpellItemData>(_castPrevious) ?? _castPrevious);
            back = $", back to {_castPrevious.itemID}";
        }
        Plugin.Logger.LogInfo(
            $"[cast] {what} not cast ({result?.ToString() ?? "no action"}, mana {Mana():0}, cost {Cost(_castSpell)}){back}");
        _castSpell = null;
        DropHold();
        if (!_castWasEquipped)
        {
            _onLoan = null;
            _loanSticky = false;
        }
    }

    [HideFromIl2Cpp]
    private void RefreshSlots()
    {
        var inventory = Inventory;
        if (inventory == null)
            return;
        Cards.Resolve(inventory, _slotSpells);
    }

    [HideFromIl2Cpp]
    private float Mana()
    {
        var current = _stats?.GetStatObject(Il2CppType.Of<SpellManaCurrent>());
        return current?.GetFloat(_entity) ?? 0f;
    }

    [HideFromIl2Cpp]
    private int Cost(SpellItemData spell)
    {
        try
        {
            return spell.GetFinalCost(_entity);
        }
        catch
        {
            return 0;
        }
    }

    // False in menus, where the game stops listening to Rose's buttons.
    [HideFromIl2Cpp]
    private bool GameplayInputActive() => _input != null && _input._isListeningUnityInput && PlayerMapEnabled();

    [HideFromIl2Cpp]
    private bool PlayerMapEnabled() => _input?.data?.moveInput?.action?.actionMap?.enabled ?? false;

    [HideFromIl2Cpp]
    private void Trace()
    {
        float now = Time.unscaledTime;
        if (_spellState != null)
        {
            bool down = _input.GetInputState(_spellState).isDown;
            if (down != _spellWasDown)
            {
                _spellWasDown = down;
                Plugin.Logger.LogInfo($"[trace] t={now:0.00} Spell {(down ? "down" : "up")}");
                _traceUntil = now + TraceSeconds;
            }
        }

        if (now > _traceUntil)
        {
            _tracedActions = Array.Empty<string>();
            return;
        }

        var active = _actor.activeActions;
        var names = new List<string>();
        for (int i = 0; i < active.Count; i++)
        {
            string name = active[i]?.data?.name ?? "?";
            if (!name.StartsWith("MC_BaseMovement", StringComparison.Ordinal) && !name.StartsWith("MC_Idle", StringComparison.Ordinal))
                names.Add(name);
        }

        var started = names.Except(_tracedActions).ToArray();
        var stopped = _tracedActions.Except(names).ToArray();
        if (started.Length > 0 || stopped.Length > 0)
            Plugin.Logger.LogInfo(
                $"[trace] t={now:0.00} +[{string.Join(", ", started)}] -[{string.Join(", ", stopped)}] " +
                $"equipped {Inventory.GetEquippedSpell()?.itemID ?? "none"}, mana {Mana():0}, " +
                $"weapon {Inventory.GetEquippedWeapon()?.itemID ?? "none"}");
        _tracedActions = names.ToArray();
    }

    [HideFromIl2Cpp]
    private void DescribeOnce(SpellItemData spell)
    {
        if (!_describedCards.Add(spell.itemID))
            return;
        var triggers = new List<string>();
        if (spell.attackBindings != null)
        {
            foreach (var binding in spell.attackBindings)
                triggers.Add($"{binding?.bindingType?.name ?? "-"} -> {binding?.actionTrigger?.name ?? "-"}");
        }
        int attachments = spell.attachmentBindings?.Length ?? 0;
        Plugin.Logger.LogInfo(
            $"[trace] card {spell.itemID}: {spell.manaCostType} cost {Cost(spell)}, {spell.manaConsumptionType}, " +
            $"{attachments} attachments, triggers [{string.Join("; ", triggers)}]");
    }

    [HideFromIl2Cpp]
    private void UpdateHud()
    {
        string mode = Plugin.HudVisibility.Value;
        bool inMenu = MenuOpen;
        bool weapons = ShowingWeapons;
        bool visible = !_hudBroken &&
            (inMenu || (mode != "Never" && GameplayInputActive() &&
                (mode != "WhileHeld" || _modifierDown || weapons)));
        try
        {
            if (_hud == null || !_hud.Alive)
            {
                if (!visible)
                    return;
                _hud = new Hud();
            }

            _hud.SetVisible(visible);
            if (!visible)
                return;

            _hud.SetHint(inMenu ? MenuHint() : "");
            var keys = weapons ? Plugin.WeaponKeys : Plugin.SlotKeys;
            _hud.SetLetters(Keys.KeyboardMode ? Array.ConvertAll(keys, k => Keys.Label(k.Value)) : null);
            bool held = _assigning || (!inMenu && _modifierDown);   // RB switches tabs in menus
            // While assigning, the letters say which slot a press picks.
            string labels = Plugin.ButtonLabels.Value;
            _hud.ShowLetters(_assigning || labels == "Always" || (labels == "WhileHeld" && held));
            // Using the wheel always shows it, like the Tarot menu does.
            float fade = Plugin.FollowGameHud.Value && !inMenu && !held && !weapons ? GameHud.Alpha() : 1f;
            if (weapons)
            {
                // Gold: the equipped weapon. Blue: a switch waiting for the current attack to end.
                _hud.Refresh(_slotWeapons, null, 0f, Inventory?.GetEquippedWeapon(), _waitingWeapon, held, fade);
                return;
            }

            float mana = Mana();
            var gold = Plugin.KeepHomeCard.Value && _home != null ? _home : Inventory?.GetEquippedSpell();
            for (int slot = 0; slot < _slotSpells.Length; slot++)
                _slotCosts[slot] = _slotSpells[slot] == null ? 0 : Cost(_slotSpells[slot]);
            _hud.Refresh(_slotSpells, _slotCosts, mana, gold, _loanSticky ? _onLoan : null, held, fade);
        }
        catch (Exception e)
        {
            _hudBroken = true;
            Plugin.Logger.LogError($"[hud] failed, HUD turned off: {e}");
            try { _hud?.SetVisible(false); } catch { }
        }
    }

    [HideFromIl2Cpp]
    private void HideHud()
    {
        if (_hud != null && _hud.Alive)
            _hud.SetVisible(false);
    }
}
