using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;

namespace AbilityWheel;

[BepInPlugin(Guid, "Ability Wheel", "0.5.2")]
public class Plugin : BasePlugin
{
    public const string Guid = "yourname.abilitywheel";   // any unique ID

    // Input System control names.
    internal static readonly string[] ModifierChoices =
        { "rightShoulder", "leftShoulder", "rightTrigger", "leftTrigger", "leftStickPress", "rightStickPress", "select" };

    internal static ManualLogSource Logger;
    internal static ConfigFile Settings;
    internal static ConfigEntry<string> Modifier;
    internal static ConfigEntry<float> TapSeconds;
    internal static ConfigEntry<string>[] Slots;      // Wheel.SlotLabels order
    internal static ConfigEntry<string>[] SlotKeys;   // Wheel.SlotLabels order
    internal static ConfigEntry<string> AssignKey;
    internal static ConfigEntry<bool> AutoFillSlots;
    internal static ConfigEntry<bool> KeepHomeCard;
    internal static ConfigEntry<string> StayEquipped;
    internal static ConfigEntry<string> HudVisibility;
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

        Slots = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        SlotKeys = new ConfigEntry<string>[Wheel.SlotLabels.Length];
        for (int slot = 0; slot < Slots.Length; slot++)
        {
            Slots[slot] = Config.Bind("Cards", "Slot" + Wheel.SlotLabels[slot], "",
                $"Card cast by holding the modifier and pressing {Wheel.SlotLabels[slot]}: its ID, for example " +
                "CursedFireBall, BoomerangCross, FireSeismicWave or PetrifyingRay. The IDs of the cards you own " +
                "are listed in BepInEx\\LogOutput.log ([cards] owned).\n" +
                "Easier: in the Arcanas menu, highlight a card, click L3 (keyboard: AssignKey) and press the slot's button.");
            SlotKeys[slot] = Config.Bind("Keyboard", "SlotKey" + Wheel.SlotLabels[slot], "Digit" + (slot + 1),
                new ConfigDescription(
                    $"Keyboard key (or mouse side button) that casts the card in slot {Wheel.SlotLabels[slot]}. " +
                    "No hold needed on keyboard.",
                    new AcceptableValueList<string>(Keys.SlotKeyChoices)));
        }
        AssignKey = Config.Bind("Keyboard", "AssignKey", "F",
            new ConfigDescription(
                "In the Arcanas menu: puts the highlighted card on the wheel (then press a slot key). " +
                "The gamepad uses L3.",
                new AcceptableValueList<string>(Keys.AssignKeyChoices)));

        AutoFillSlots = Config.Bind("Cards", "AutoFillSlots", true,
            "Fill empty slots with owned cards that aren't on the wheel yet.");
        KeepHomeCard = Config.Bind("Cards", "KeepHomeCard", true,
            "On: the card equipped in the Spells menu stays your card for Y. A wheel card is only equipped " +
            "while its own spell plays, then the Y card goes back on.\n" +
            "Off: the last card cast from the wheel stays equipped, so Y casts it again.\n" +
            "Cards with passives or arcana tasks that need the card equipped only count it while it's on.");

        StayEquipped = Config.Bind("Cards", "StayEquipped", "PocketMirror",
            "With KeepHomeCard: cards whose effect stays out in the world, vanishes when the card is " +
            "unequipped, and leaves nothing running on Rose (Mirror Walk's mirror, card ID PocketMirror). " +
            "After a wheel cast they stay equipped until their next cast (the follow-up: teleporting to the " +
            "mirror) has played, you press Y, cast another card from the wheel, or the game saves. " +
            "Card IDs, separated by commas. (Wheel of Chaos doesn't need this: its wheels keep an action " +
            "running on Rose, which already keeps the card on.)");
        // The 0.3.2-0.5.1 default also had Wheel of Chaos, which kept it on after its wheels were gone.
        if (StayEquipped.Value.Replace(" ", "") == "PocketMirror,TargetingChargingWheels")
            StayEquipped.Value = "PocketMirror";

        HudVisibility = Config.Bind("Display", "HudVisibility", "Always",
            new ConfigDescription(
                "When to show the 4 cards in the bottom-right corner during gameplay: Always, " +
                "WhileHeld (only while the modifier is held) or Never.",
                new AcceptableValueList<string>("Always", "WhileHeld", "Never")));
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
            "Log the actions Rose starts and stops for a few seconds after each cast or Y press " +
            "([trace] in BepInEx\\LogOutput.log). For checking how a card behaves on the wheel.");

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
        Log.LogInfo($"Ability Wheel loaded (hold {Modifier.Value} + Y/X/B/A to cast)");
    }
}
