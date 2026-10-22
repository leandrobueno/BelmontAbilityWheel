using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AbilityWheel;

[BepInPlugin(Guid, "Ability Wheel", Version)]
[BepInDependency(MenuGlue.LibGuid, BepInDependency.DependencyFlags.SoftDependency)]
public class Plugin : BasePlugin
{
    public const string Guid = "leandrobueno.abilitywheel";
    public const string Version = "1.6.0";

    // Input System control names.
    internal static readonly string[] ModifierChoices =
        { "rightShoulder", "leftShoulder", "rightTrigger", "leftTrigger", "leftStickPress", "rightStickPress", "select" };
    // The weapon page: the wheel button then LB (the default), or a button of its own.
    internal const string WheelThenLB = "wheelButtonThenLB";
    internal static readonly string[] WeaponButtonChoices =
    {
        WheelThenLB, "rightShoulder", "leftShoulder", "rightTrigger", "leftTrigger", "leftStickPress", "rightStickPress",
        "select",
    };
    // Gamepad buttons a wheel slot can use, as Input System control paths on a gamepad.
    internal static readonly string[] FaceButtons = { "buttonNorth", "buttonWest", "buttonEast", "buttonSouth" };
    internal static readonly string[] DpadButtons = { "dpad/up", "dpad/left", "dpad/right", "dpad/down" };
    internal static readonly string[] SlotButtonChoices =
    {
        "buttonNorth", "buttonWest", "buttonEast", "buttonSouth", "dpad/up", "dpad/left", "dpad/right", "dpad/down",
        "leftShoulder", "rightShoulder", "leftTrigger", "rightTrigger", "leftStickPress", "rightStickPress",
    };

    internal static ManualLogSource Logger;
    internal static ConfigFile Settings;
    internal static ConfigEntry<string> Modifier;
    internal static ConfigEntry<float> TapSeconds;
    internal static ConfigEntry<bool> MergedWheels;
    internal static ConfigEntry<string>[] SlotButtons;         // spell slots 1-4
    internal static ConfigEntry<string>[] WeaponSlotButtons;   // weapon slots 1-4
    internal static ConfigEntry<string>[] Slots;      // Wheel.SlotLabels order
    internal static ConfigEntry<string>[] SlotKeys;   // Wheel.SlotLabels order
    internal static ConfigEntry<string> AssignKey;
    internal static ConfigEntry<bool> WeaponPage;
    internal static ConfigEntry<string> WeaponButton;
    internal static ConfigEntry<string>[] WeaponSlots;  // Wheel.SlotLabels order
    internal static ConfigEntry<string>[] WeaponKeys;   // Wheel.SlotLabels order
    internal static ConfigEntry<bool> AutoFillSlots;
    internal static ConfigEntry<bool> KeepHomeCard;
    internal static ConfigEntry<string> StayEquipped;
    internal static ConfigEntry<string> HudVisibility;
    internal static ConfigEntry<string> ButtonLabels;
    internal static ConfigEntry<bool> FollowGameHud;
    internal static ConfigEntry<float> HudOpacity;
    internal static ConfigEntry<float> HudScale;
    internal static ConfigEntry<float> HudOffsetX;
    internal static ConfigEntry<float> HudOffsetY;
    internal static ConfigEntry<bool> TraceActions;

    public override void Load()
    {
        Logger = Log;
        Settings = Config;

        Modifier = Config.Bind("Controls", "Modifier", "rightShoulder",
            new ConfigDescription(
                "The gamepad button to hold for the wheel (rightShoulder is RB / R1).\n" +
                "Whatever this button normally does still happens when you tap it, on release.",
                new AcceptableValueList<string>(ModifierChoices)));
        TapSeconds = Config.Bind("Controls", "TapSeconds", 0.3f,
            new ConfigDescription(
                "Releasing the modifier within this many seconds, without casting, counts as a tap " +
                "and does the button's normal action (Interact for RB).",
                new AcceptableValueRange<float>(0.05f, 2f)));
        MergedWheels = Config.Bind("Controls", "MergedWheels", false,
            "On: holding the wheel button shows the spell and weapon wheels side by side, each with its own 4 " +
            "slot buttons (when this is turned on, weapon slots still on Y/X/B/A move to the D-pad). All 8 slot " +
            "buttons must differ, and WeaponButton doesn't apply.");
        SlotButtons = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        WeaponSlotButtons = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        for (int slot = 0; slot < SlotButtons.Length; slot++)
        {
            SlotButtons[slot] = Config.Bind("Controls", $"SlotButton{slot + 1}", FaceButtons[slot],
                new ConfigDescription(
                    $"Gamepad button that casts the card in spell slot {slot + 1} while the wheel button is held.",
                    new AcceptableValueList<string>(SlotButtonChoices)));
            WeaponSlotButtons[slot] = Config.Bind("Controls", $"WeaponSlotButton{slot + 1}", FaceButtons[slot],
                new ConfigDescription(
                    $"Gamepad button that switches to the weapon in weapon slot {slot + 1} while the weapon wheel is " +
                    "open.",
                    new AcceptableValueList<string>(SlotButtonChoices)));
        }

        Slots = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        SlotKeys = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        for (int slot = 0; slot < Slots.Length; slot++)
        {
            Slots[slot] = Config.Bind("Cards", "Slot" + Wheel.SlotLabels[slot], "",
                $"Card in spell slot {slot + 1} (its gamepad button is SlotButton{slot + 1}): its ID, for example " +
                "CursedFireBall, BoomerangCross, FireSeismicWave or PetrifyingRay. The IDs of the cards you own " +
                "are listed in BepInEx\\LogOutput.log ([cards] owned).\n" +
                "Easier: in the Arcanas menu, highlight a card, click L3 (keyboard: AssignKey) and press the slot's button.");
            SlotKeys[slot] = Config.Bind("Keyboard", "SlotKey" + Wheel.SlotLabels[slot], "Digit" + (slot + 1),
                new ConfigDescription(
                    $"Keyboard key (or mouse side button) that casts the card in spell slot {slot + 1}. " +
                    "No hold needed on keyboard.",
                    new AcceptableValueList<string>(Keys.SlotKeyChoices)));
        }
        AssignKey = Config.Bind("Keyboard", "AssignKey", "F",
            new ConfigDescription(
                "In the Arcanas or Equipment menu: puts the highlighted card or weapon on the wheel (then press a " +
                "slot key). " +
                "The gamepad uses L3.",
                new AcceptableValueList<string>(Keys.AssignKeyChoices)));

        WeaponPage = Config.Bind("Weapons", "WeaponPage", true,
            "Hold the weapon button (see WeaponButton) and press Y/X/B/A to switch to that slot's weapon. " +
            "The weapon stays equipped. Nothing changes until a weapon slot is filled.");
        WeaponButton = Config.Bind("Weapons", "WeaponButton", WheelThenLB,
            "The gamepad button for the weapon wheel, as an Input System control name, or two joined by + " +
            "(hold the first, then press the second).\n" +
            "wheelButtonThenLB (default) or <wheel button>+leftShoulder, e.g. rightShoulder+leftShoulder: hold the " +
            "wheel button, then also hold LB. LB pressed on its own (or before the wheel button) is still the " +
            "potion. A combination must start with the wheel button.\n" +
            "One button (rightShoulder, leftShoulder, rightTrigger, leftTrigger, leftStickPress, rightStickPress or " +
            "select): hold it on its own and press a weapon slot's button, like the wheel button for spells; a tap " +
            "still does the button's normal action. leftShoulder (LB) is the exception: the potion is " +
            "hold-to-drink, so with LB here, LB never drinks a potion (handy with a relic that blocks potions). " +
            "With both buttons held, the slot buttons go to the one pressed last. It can't be the wheel button.");
        WeaponSlots = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        WeaponKeys = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        for (int slot = 0; slot < WeaponSlots.Length; slot++)
        {
            WeaponSlots[slot] = Config.Bind("Weapons", "Slot" + Wheel.SlotLabels[slot], "",
                $"Weapon in weapon slot {slot + 1} (its gamepad button is WeaponSlotButton{slot + 1}): its ID, " +
                "for example WeaponOneHandedTier2 or WeaponSpearTier4. The IDs of the weapons you own are listed in " +
                "BepInEx\\LogOutput.log ([weapons] owned).\n" +
                "Easier: in the Equipment menu, highlight a weapon, click L3 (keyboard: AssignKey) and press the slot's button.");
            WeaponKeys[slot] = Config.Bind("Keyboard", "WeaponKey" + Wheel.SlotLabels[slot], "Digit" + (slot + 5),
                new ConfigDescription(
                    $"Keyboard key (or mouse side button) that equips the weapon in weapon slot {slot + 1}.",
                    new AcceptableValueList<string>(Keys.SlotKeyChoices)));
        }

        AutoFillSlots = Config.Bind("Cards", "AutoFillSlots", true,
            "Fill empty slots with owned cards that aren't on the wheel yet.");
        KeepHomeCard = Config.Bind("Cards", "KeepHomeCard", true,
            "On: the card equipped in the Tarot menu stays equipped. A wheel card is only equipped " +
            "while its own spell plays, then your card goes back on.\n" +
            "Off: the last card cast from the wheel stays equipped, so your spell button casts it again.\n" +
            "Cards with passives or arcana tasks that need the card equipped only count it while it's on.");

        StayEquipped = Config.Bind("Cards", "StayEquipped", "PocketMirror",
            "With KeepHomeCard: cards whose effect stays out in the world, vanishes when the card is " +
            "unequipped, and leaves nothing running on Rose (Mirror Walk's mirror, card ID PocketMirror). " +
            "After a wheel cast they stay equipped until their next cast (the follow-up: teleporting to the " +
            "mirror) has played, you press Y, cast another card from the wheel, or the game saves. " +
            "Card IDs, separated by commas. (Wheel of Chaos doesn't need this: its wheels keep an action " +
            "running on Rose, which already keeps the card on.)");

        HudVisibility = Config.Bind("Display", "HudVisibility", "Always",
            new ConfigDescription(
                "When to show the 4 cards in the bottom-right corner during gameplay: Always, " +
                "WhileHeld (only while the modifier is held) or Never.",
                new AcceptableValueList<string>("Always", "WhileHeld", "Never")));
        ButtonLabels = Config.Bind("Display", "ButtonLabels", "Always",
            new ConfigDescription(
                "When to show each card's button (Y/X/B/A, or its key after using the keyboard): Always, " +
                "WhileHeld (only while the modifier is held) or Never. Always shown while putting a card or " +
                "weapon on the wheel, since they say which slot a press picks.",
                new AcceptableValueList<string>("Always", "WhileHeld", "Never")));
        FollowGameHud = Config.Bind("Display", "FollowGameHud", true,
            "Fade the 4 cards out while the game's own HUD (health and mana, top left) is hidden, for example " +
            "by the HUD Auto Hide mod. Holding the modifier, assigning a card and the Tarot menu always show them.");
        HudOpacity = Config.Bind("Display", "HudOpacity", 1f,
            new ConfigDescription("Opacity of the card tiles (1 = solid).", new AcceptableValueRange<float>(0.2f, 1f)));
        HudScale = Config.Bind("Display", "HudScale", 1f,
            new ConfigDescription("Size of the card tiles.", new AcceptableValueRange<float>(0.5f, 2f)));
        HudOffsetX = Config.Bind("Display", "HudOffsetX", 48f,
            new ConfigDescription("Distance from the right edge of the screen, in pixels at 1080p.",
                new AcceptableValueRange<float>(0f, 1600f)));
        HudOffsetY = Config.Bind("Display", "HudOffsetY", 48f,
            new ConfigDescription("Distance from the bottom edge of the screen, in pixels at 1080p.",
                new AcceptableValueRange<float>(0f, 800f)));

        TraceActions = Config.Bind("Debug", "TraceActions", false,
            "Log the actions Rose starts and stops for a few seconds after each cast, weapon switch or Y press " +
            "([trace] in BepInEx\\LogOutput.log). For checking how a card or weapon behaves on the wheel.");

        Bindings.Watch();
        MenuGlue.Register();
        AddComponent<Wheel>();

        // One patch class at a time, so one that fails (after a game update, say) only turns off
        // its own feature instead of every patch after it.
        var harmony = new Harmony(Guid);
        foreach (var type in typeof(Plugin).Assembly.GetTypes())
        {
            if (type.GetCustomAttributes(typeof(HarmonyPatch), false).Length == 0)
                continue;
            try
            {
                harmony.CreateClassProcessor(type).Patch();
            }
            catch (System.Exception e)
            {
                Log.LogError($"[patch] {type.Name} failed, that part of the mod is off: {e}");
            }
        }
        Log.LogInfo($"Ability Wheel loaded (hold {Modifier.Value} + Y/X/B/A to cast" +
            (!WeaponPage.Value ? ")"
                : MergedWheels.Value ? ", the same button for weapons)"
                : $", {WeaponButton.Value} for weapons)"));
    }
}
