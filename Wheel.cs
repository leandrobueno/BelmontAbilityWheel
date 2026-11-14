using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
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

    // How long the wheel shows after a slot is used without holding a wheel button.
    private const float FlashSeconds = 1.2f;
    private float _flashUntil;
    // With the wheels swapped the HUD shows the weapon wheel: a spell slot just used shows the
    // spell wheel for a moment, as a weapon switch shows the weapon wheel otherwise.
    private float _showSpellsUntil;

    private const float TraceSeconds = 4f;
    private ActionInputState _spellState;
    private bool _spellWasDown;
    private float _traceUntil;
    private string[] _tracedActions = Array.Empty<string>();
    private readonly HashSet<string> _describedCards = new();

    private const int HudRehearsalFrames = 3;
    private int _hudRehearsal;

    private void Awake()
    {
        Instance = this;
    }

    private void Update()
    {
        try
        {
            _markStart = System.Diagnostics.Stopwatch.GetTimestamp();
            TrackFrames();
            Keys.TrackDevice();
            if (!FindPlayer())
            {
                HideHud();
                return;
            }

            TrackHome();
            UpdateAssign();
            TrackMenus();
            ReviveSlotItem();
            TrackModifier();
            TrackPage();
            PollPadSlots();
            _markInput = System.Diagnostics.Stopwatch.GetTimestamp();
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
            _markWeapons = System.Diagnostics.Stopwatch.GetTimestamp();
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
            _markSpells = System.Diagnostics.Stopwatch.GetTimestamp();
            UpdateHud();
            ReportUpdate();
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

            // The weapon page's own button, like the wheel button.
            var weaponButton = WeaponButton();
            if (weaponButton != null && control.Pointer == weaponButton.Pointer)
            {
                if (!_weaponHeldBack.Contains(name))
                    _weaponHeldBack.Add(name);
                _swallowed.Add(name);
                return false;
            }

            // LB pressed while the wheel button is down opens the weapon page instead of drinking
            // (or, with TapToSwitch, turns the wheel: TrackPage does that). The wheel counts as
            // used, so the wheel button's release won't replay Interact.
            var page = PageButton();
            if (page != null && control.Pointer == page.Pointer && modifier.isPressed && sameDevice)
            {
                _swallowed.Add(name);
                _lastWheelPressFrame = Time.frameCount;
                if (!_pageOpen && !Plugin.TapToSwitch.Value)
                {
                    _pageOpen = true;
                    Plugin.Logger.LogDebug($"[weapon] page open ({name} held back)");
                }
                return false;
            }

            // A slot button while its wheel is held belongs to the wheel; PollPadSlots acts on it.
            bool wheelHeld = modifier.isPressed;
            bool otherHeld = Merged ? wheelHeld : _pageOpen || (weaponButton != null && weaponButton.isPressed);
            bool swapped = Swapped;
            bool spellHeld = swapped ? otherHeld : wheelHeld;
            bool weaponsHeld = swapped ? wheelHeld : otherHeld;
            if (sameDevice && ((spellHeld && IsSlotButton(control, Plugin.SlotButtons)) ||
                    (weaponsHeld && IsSlotButton(control, Plugin.WeaponSlotButtons))))
            {
                _swallowed.Add(name);
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
        LoadWheelItems("a save was loaded");
        return true;
    }

    // Rose was lost while a wheel card was on loan, so the card she comes back with isn't the
    // home card.
    [HideFromIl2Cpp]
    private void RestoreLostHome(Inventory inventory)
    {
        if (_lostHomeId != null && _home?.itemID == _lostLoanId)
        {
            var home = Cards.Find(Cards.Owned(inventory), _lostHomeId);
            if (home != null)
            {
                EquipSpell(inventory, home, true);
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
        _flashUntil = 0f;
        _showSpellsUntil = 0f;
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
            EquipSpell(inventory, Cards.Live<SpellItemData>(_home) ?? _home, true);
        Plugin.Logger.LogInfo(
            $"[home] {_onLoan.itemID} -> {_home.itemID}: {reason}, {Time.unscaledTime - _loanedAt:0.00}s after the cast");
        _onLoan = null;
        _loanSticky = false;
    }

    [HideFromIl2Cpp]
    private static ButtonControl ModifierButton() =>
        PadButton(Plugin.Modifier.Value) ?? Gamepad.current?.rightShoulder;

    // A config value (an Input System control path on a gamepad) to the current pad's button.
    [HideFromIl2Cpp]
    internal static ButtonControl PadButton(string name)
    {
        var pad = Gamepad.current;
        if (pad == null)
            return null;
        return name switch
        {
            "buttonNorth" => pad.buttonNorth,
            "buttonWest" => pad.buttonWest,
            "buttonEast" => pad.buttonEast,
            "buttonSouth" => pad.buttonSouth,
            "dpad/up" => pad.dpad.up,
            "dpad/left" => pad.dpad.left,
            "dpad/right" => pad.dpad.right,
            "dpad/down" => pad.dpad.down,
            "rightStick/up" => pad.rightStick.up,
            "rightStick/left" => pad.rightStick.left,
            "rightStick/right" => pad.rightStick.right,
            "rightStick/down" => pad.rightStick.down,
            "leftShoulder" => pad.leftShoulder,
            "rightShoulder" => pad.rightShoulder,
            "leftTrigger" => pad.leftTrigger,
            "rightTrigger" => pad.rightTrigger,
            "leftStickPress" => pad.leftStickButton,
            "rightStickPress" => pad.rightStickButton,
            "select" => pad.selectButton,
            "start" => pad.startButton,
            _ => null,
        };
    }

    [HideFromIl2Cpp]
    private static bool IsSlotButton(InputControl control, ConfigEntry<string>[] slots)
    {
        foreach (var entry in slots)
        {
            var button = PadButton(entry.Value);
            if (button != null && button.Pointer == control.Pointer)
                return true;
        }
        return false;
    }

    // Both wheels at once on the wheel button, each slot on its own button (Bindings checks that).
    [HideFromIl2Cpp]
    private static bool Merged => Plugin.MergedWheels.Value && Plugin.WeaponPage.Value;

    // The wheel button opens the weapon wheel and the weapon button the spell wheel. Like the weapon
    // wheel itself, only once a weapon slot is filled: until then the wheel button casts spells.
    [HideFromIl2Cpp]
    private bool Swapped => Plugin.SwapWheels.Value && !Merged && HasWeapons;

    // Slots that SlotsWithoutHolding lets work on their own: the game only uses the D-pad and the
    // right stick for things the mod can keep from it (Move and the camera).
    [HideFromIl2Cpp]
    private static bool IsDirect(string control) =>
        control.StartsWith("dpad/", StringComparison.Ordinal) || control.StartsWith("rightStick/", StringComparison.Ordinal);

    // Which wheels take slot presses now. spells / weapons: that wheel is held open, so all of its
    // slot buttons count. direct: no wheel button is down and SlotsWithoutHolding is on, so the
    // D-pad and right-stick slots of both wheels count.
    [HideFromIl2Cpp]
    private void LiveWheels(out bool spells, out bool weapons, out bool direct)
    {
        bool merged = Merged;
        bool other = !merged && _pageOpen;
        bool wheel = _modifierDown && !other;
        bool swapped = Swapped;
        spells = merged ? _modifierDown : swapped ? other : wheel;
        weapons = merged ? _modifierDown : swapped ? wheel : other;
        direct = Plugin.NoHold.Value && !_modifierDown && !_weaponDown && !other;
    }

    // A slot button pressed while its wheel is held: Y/X/B/A by default, the D-pad for the merged
    // weapon wheel. Polled rather than taken from the game's input, which never sees the D-pad or
    // the right stick as buttons (they're Move and the camera). The wheel button's own wheel goes
    // first, so it wins a button both wheels have without a hold.
    [HideFromIl2Cpp]
    private void PollPadSlots()
    {
        if (MenuOpen || !GameplayInputActive())
            return;
        LiveWheels(out bool spells, out bool weapons, out bool direct);
        bool weaponsFirst = Swapped;
        for (int pass = 0; pass < 2; pass++)
        {
            bool weaponPass = (pass == 0) == weaponsFirst;
            if (weaponPass && !HasWeapons)
                continue;
            bool held = weaponPass ? weapons : spells;
            if (!held && !direct)
                continue;
            var buttons = weaponPass ? Plugin.WeaponSlotButtons : Plugin.SlotButtons;
            for (int slot = 0; slot < SlotLabels.Length; slot++)
            {
                string name = buttons[slot].Value;
                if ((!held && !IsDirect(name)) || !SlotPressed(name, out var button))
                    continue;
                if (weaponPass)
                {
                    _pendingWeaponSlot = slot;
                }
                else
                {
                    _pendingSlot = slot;
                    _pendingControl = button;
                }
                if (held)
                    _lastWheelPressFrame = Time.frameCount;   // the wheel button won't replay its tap action
                else
                    _flashUntil = Time.unscaledTime + FlashSeconds;
                return;
            }
        }
    }

    // A stick pushed diagonally presses two directions; only the stronger one counts.
    [HideFromIl2Cpp]
    private static bool SlotPressed(string name, out ButtonControl button)
    {
        button = PadButton(name);
        if (button == null || !button.wasPressedThisFrame)
            return false;
        if (!name.StartsWith("rightStick/", StringComparison.Ordinal))
            return true;
        var stick = Gamepad.current.rightStick.ReadValue();
        bool vertical = name.EndsWith("up", StringComparison.Ordinal) || name.EndsWith("down", StringComparison.Ordinal);
        return vertical ? Mathf.Abs(stick.y) >= Mathf.Abs(stick.x) : Mathf.Abs(stick.x) > Mathf.Abs(stick.y);
    }

    // Patches.cs Move filter: false drops a D-pad press while a wheel has a live slot on it, so Rose
    // doesn't walk while a slot is picked. Releases always go through, so she never keeps walking.
    [HideFromIl2Cpp]
    internal bool LetMoveThrough(GamePlayerInput input, InputAction.CallbackContext ctx)
    {
        if (_input == null || input == null || input.Pointer != _input.Pointer || ctx.phase == InputActionPhase.Canceled)
            return true;
        // The game binds the whole D-pad as one Vector2 control; a single direction's parent is "dpad".
        var control = ctx.control;
        if (control == null || (control.name != "dpad" && control.parent?.name != "dpad"))
            return true;
        return !SlotsLiveOn("dpad/");
    }

    // Patches.cs right-stick filter: the same for the camera while a wheel has a live slot on the
    // right stick. The game keeps the stick's last value, so it's cleared too.
    [HideFromIl2Cpp]
    internal bool LetAimThrough(GamePlayerInput input, InputAction.CallbackContext ctx)
    {
        if (_input == null || input == null || input.Pointer != _input.Pointer || ctx.phase == InputActionPhase.Canceled)
            return true;
        if (ctx.control?.device?.TryCast<Gamepad>() == null || !SlotsLiveOn("rightStick/"))
            return true;
        input.aimInputValue = Vector2.zero;
        return false;
    }

    [HideFromIl2Cpp]
    private bool SlotsLiveOn(string prefix)
    {
        LiveWheels(out bool spells, out bool weapons, out bool direct);
        return ((spells || direct) && Uses(Plugin.SlotButtons, prefix)) ||
            ((weapons || direct) && HasWeapons && Uses(Plugin.WeaponSlotButtons, prefix));
    }

    [HideFromIl2Cpp]
    private static bool Uses(ConfigEntry<string>[] slots, string prefix) =>
        Array.Exists(slots, s => s.Value.StartsWith(prefix, StringComparison.Ordinal));

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
        EndHold(_heldBackActions, _modifierDownAt, _modifierDownFrame, true);
        SaveWheelTurn();
    }

    // A held-back button let go: a tap that didn't use the wheel does the button's own action.
    [HideFromIl2Cpp]
    private void EndHold(List<string> heldBack, float downAt, int downFrame, bool canReplay)
    {
        if (heldBack.Count == 0)
            return;

        float held = Time.unscaledTime - downAt;
        bool usedWheel = _lastWheelPressFrame >= downFrame;
        if (canReplay && !usedWheel && held <= Plugin.TapSeconds.Value)
        {
            foreach (var name in heldBack)
                Replay(name);
        }
        else
        {
            string why = usedWheel ? "the wheel was used" : !canReplay ? "LB is the weapon button" : $"held {held:0.00}s";
            Plugin.Logger.LogDebug($"[tap] {string.Join(", ", heldBack)} not done:{why}");
        }
        heldBack.Clear();
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

        if (Swapped)
            _showSpellsUntil = Time.unscaledTime + FlashSeconds;
        if (Plugin.SelectOnly.Value)
        {
            Select(slot, spell);
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
            EquipSpell(inventory, spell, true);

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

    // SelectOnly: the slot's card is equipped as the Tarot menu would, and the Spell button casts it.
    // It's the home card from here on, so nothing brings another one back.
    [HideFromIl2Cpp]
    private void Select(int slot, SpellItemData spell)
    {
        var inventory = Inventory;
        _pendingControl = null;
        if (_castSpell != null)
        {
            _castSpell = null;
            DropHold();
        }
        var previous = inventory.GetEquippedSpell();
        // The card taken off doesn't come back by itself: it only stays loaded if it's on the wheel.
        if (!Cards.Same(spell, previous))
            EquipSpell(inventory, spell, false);
        _home = spell;
        _onLoan = null;
        _loanSticky = false;
        Plugin.Logger.LogInfo($"[select] {SlotLabels[slot]} {spell.itemID} equipped, was {previous?.itemID ?? "none"}");
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
            EquipSpell(inventory, Cards.Live<SpellItemData>(_castPrevious) ?? _castPrevious, true);
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
        // A wheel in use always shows, whatever hides it otherwise.
        bool inUse = _modifierDown || _weaponDown || _pageOpen || _waitingWeapon != null ||
            Time.unscaledTime < _showWeaponsUntil || Time.unscaledTime < _flashUntil ||
            Time.unscaledTime < _showSpellsUntil;
        bool visible = !_hudBroken &&
            (inMenu || (mode != "Never" && GameplayInputActive() && (mode != "WhileHeld" || inUse)));
        // Built ahead: a new HUD is drawn for a few frames, fully transparent, so its icons,
        // pictures and text are made before the first press in a fight (about 30 ms otherwise).
        bool rehearsal = false;
        if (!visible && !_hudBroken)
        {
            if ((_hud == null || !_hud.Alive) && mode != "Never" && GameplayInputActive())
                _hudRehearsal = HudRehearsalFrames;
            rehearsal = _hudRehearsal > 0;
        }
        if (visible)
            _hudRehearsal = 0;
        else if (rehearsal)
            _hudRehearsal--;
        UpdateMenuPrompts(inMenu);
        try
        {
            if (_hud == null || !_hud.Alive)
            {
                if (!visible && !rehearsal)
                    return;
                _hud = new Hud();
            }

            _hud.Silent = rehearsal;
            if (rehearsal)
            {
                // Both wheels' tiles, as a press would show them; gone again with the last frame.
                _hud.SetVisible(_hudRehearsal > 0, true);
                if (_hudRehearsal > 0)
                {
                    _hud.SetMerged(true);
                    SetLetters(0, false);
                    SetLetters(1, true);
                    _hud.ShowLetters(true);
                    _hud.Frame(false, 1f);
                    ShowSpells(0, false);
                    ShowWeapons(1, false);
                }
                return;
            }

            _hud.SetVisible(visible);
            if (!visible)
            {
                _hud.Animate();   // fades out showing what it last showed
                return;
            }

            // Only when the game's own footer can't carry the prompts.
            _hud.SetHint(inMenu && !_nativePrompts ? MenuHint() : "");
            // Merged, gameplay shows both wheels; the menus show the one being assigned.
            bool merged = Merged && !inMenu;
            _hud.SetMerged(merged);
            _hud.SetPage(weapons && !merged);
            SetLetters(0, weapons && !merged);
            if (merged)
                SetLetters(1, true);
            bool held = _assigning || (!inMenu && (_modifierDown || _weaponDown));   // RB switches tabs in menus
            // While assigning, the letters say which slot a press picks.
            string labels = Plugin.ButtonLabels.Value;
            _hud.ShowLetters(_assigning || labels == "Always" || (labels == "WhileHeld" && held));
            float fade = Plugin.FollowGameHud.Value && !inMenu && !held && !inUse ? GameHud.Alpha() : 1f;
            _hud.Frame(held, fade);
            if (merged)
            {
                ShowSpells(0, held);
                ShowWeapons(1, held);
            }
            else if (weapons)
            {
                ShowWeapons(0, held);
            }
            else
            {
                ShowSpells(0, held);
            }
        }
        catch (Exception e)
        {
            _hudBroken = true;
            Plugin.Logger.LogError($"[hud] failed, HUD turned off: {e}");
            try { _hud?.SetVisible(false, true); } catch { }
        }
    }

    [HideFromIl2Cpp]
    private void ShowSpells(int cluster, bool held)
    {
        float mana = Mana();
        var gold = Plugin.KeepHomeCard.Value && _home != null ? _home : Inventory?.GetEquippedSpell();
        for (int slot = 0; slot < _slotSpells.Length; slot++)
            _slotCosts[slot] = _slotSpells[slot] == null ? 0 : Cost(_slotSpells[slot]);
        _hud.Refresh(cluster, _slotSpells, _slotCosts, mana, gold, _loanSticky ? _onLoan : null, held);
    }

    // Gold: the equipped weapon. Blue: a switch waiting for the current attack to end.
    [HideFromIl2Cpp]
    private void ShowWeapons(int cluster, bool held) =>
        _hud.Refresh(cluster, _slotWeapons, null, 0f, Inventory?.GetEquippedWeapon(), _waitingWeapon, held);

    // The slots' keys after the keyboard was used, the game's icons of their gamepad buttons otherwise.
    [HideFromIl2Cpp]
    private void SetLetters(int cluster, bool weapons)
    {
        var labels = new string[SlotLabels.Length];
        var colors = new Color[SlotLabels.Length];
        var keys = weapons ? Plugin.WeaponKeys : Plugin.SlotKeys;
        var buttons = weapons ? Plugin.WeaponSlotButtons : Plugin.SlotButtons;
        for (int slot = 0; slot < labels.Length; slot++)
        {
            labels[slot] = Keys.KeyboardMode ? Keys.Label(keys[slot].Value) : Glyphs.Tag(buttons[slot].Value);
            colors[slot] = Keys.KeyboardMode ? Ui.TextColor : Color.white;
        }
        _hud.SetLetters(cluster, labels, colors);
    }

    [HideFromIl2Cpp]
    private void HideHud()
    {
        if (_hud != null && _hud.Alive)
            _hud.SetVisible(false, true);
    }
}
