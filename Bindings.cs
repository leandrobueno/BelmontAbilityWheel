using System;
using System.Collections.Generic;
using BepInEx.Configuration;

namespace AbilityWheel;

// The buttons and keys the wheel uses, checked together: a slot on the wheel or weapon button,
// two slots of one wheel on the same button, or two slot keys on one key would make a press
// ambiguous. Belmont UI 0.4's binding rows ask Check before taking a press, and refuse it with the
// reason. Any other change that breaks the rules (the plain lists of Belmont UI 0.3) is undone at
// once, so the menu shows the old value again, with a warning in the log. A config file edited by
// hand is only reported; the first matching slot wins there.
internal static class Bindings
{
    internal enum Trouble { None, InUse, WheelFirst, NotAllowed }

    private static readonly Dictionary<ConfigEntryBase, object> Last = new();
    private static bool _busy;

    private static IEnumerable<ConfigEntryBase> Watched()
    {
        yield return Plugin.Modifier;
        yield return Plugin.WeaponPage;
        yield return Plugin.WeaponButton;
        yield return Plugin.MergedWheels;
        foreach (var entry in Plugin.SlotButtons)
            yield return entry;
        foreach (var entry in Plugin.WeaponSlotButtons)
            yield return entry;
        foreach (var entry in Plugin.SlotKeys)
            yield return entry;
        foreach (var entry in Plugin.WeaponKeys)
            yield return entry;
    }

    internal static void Watch()
    {
        Remember();
        var (trouble, problem) = Problem(null, null);
        if (trouble != Trouble.None)
            Plugin.Logger.LogWarning($"[buttons] in the config file: {problem}");
        Plugin.Settings.SettingChanged += (_, args) => OnChanged(args.ChangedSetting);
    }

    // For a binding row: what's wrong with giving entry this value, everything else as it is.
    internal static Trouble Check(ConfigEntryBase entry, string value) => Problem(entry, value).Trouble;

    private static void Remember()
    {
        foreach (var entry in Watched())
            Last[entry] = entry.BoxedValue;
    }

    private static void OnChanged(ConfigEntryBase changed)
    {
        if (_busy || !Last.ContainsKey(changed))
            return;
        _busy = true;
        try
        {
            if (changed == Plugin.MergedWheels)
            {
                MoveWeaponSlots(Plugin.MergedWheels.Value);
                // The weapon wheel toggle is hidden while merged, so it can't be left off.
                if (Plugin.MergedWheels.Value)
                    Plugin.WeaponPage.Value = true;
            }
            // A combination follows the wheel button it starts with.
            if (changed == Plugin.Modifier && WeaponCombo() is { Held: { } held } combo &&
                held == (string)Last[Plugin.Modifier] && Plugin.WeaponButton.Value != Plugin.WheelThenLB)
                Plugin.WeaponButton.Value = Plugin.Modifier.Value + "+" + combo.Pressed;
            var (trouble, problem) = Problem(null, null);
            if (trouble == Trouble.None)
            {
                Remember();
                return;
            }
            foreach (var entry in Watched())
                entry.BoxedValue = Last[entry];
            Plugin.Logger.LogWarning($"[buttons] {changed.Definition.Key} not changed: {problem}");
        }
        finally
        {
            _busy = false;
        }
    }

    // Merged, the weapon slots can't share the face buttons with the spell slots: the ones still on
    // their default buttons move to the D-pad, and back when the wheels are split again.
    private static void MoveWeaponSlots(bool merged)
    {
        var from = merged ? Plugin.FaceButtons : Plugin.DpadButtons;
        var to = merged ? Plugin.DpadButtons : Plugin.FaceButtons;
        for (int slot = 0; slot < Plugin.WeaponSlotButtons.Length; slot++)
        {
            if (Plugin.WeaponSlotButtons[slot].Value == from[slot])
                Plugin.WeaponSlotButtons[slot].Value = to[slot];
        }
    }

    // The weapon button: one button of its own (Held null), or a combination whose held part must
    // be the wheel button, like the default, the wheel button then LB. "wheelButtonThenLB", from
    // 1.5 and older and from the plain list on Belmont UI 0.3, means that default.
    internal static (string Held, string Pressed) WeaponCombo() => Combo(Plugin.WeaponButton.Value, Plugin.Modifier.Value);

    private static (string Held, string Pressed) Combo(string value, string wheel)
    {
        value = (value ?? "").Trim();
        if (value == Plugin.WheelThenLB)
            return (wheel, "leftShoulder");
        int plus = value.IndexOf('+');
        return plus < 0 ? (null, value) : (value.Substring(0, plus), value.Substring(plus + 1));
    }

    // Buttons that can hold a wheel open: not the face buttons or the D-pad (slots and Move), not
    // Start (the menu), not a stick.
    internal static bool CanHoldWheel(string control) => Array.IndexOf(Plugin.ModifierChoices, control) >= 0;

    // The button whose press opens the weapon wheel, or null when there's none (merged, off, or a
    // combination that can't work).
    internal static string WeaponWheelButton() => WeaponWheelButton(V(null, null));

    private static string WeaponWheelButton(Func<ConfigEntryBase, object> value)
    {
        if ((bool)value(Plugin.MergedWheels) || !(bool)value(Plugin.WeaponPage))
            return null;
        string wheel = (string)value(Plugin.Modifier);
        var (held, pressed) = Combo((string)value(Plugin.WeaponButton), wheel);
        if ((held != null && held != wheel) || pressed == wheel)
            return null;
        return pressed;
    }

    // From 1.5's "wheelButtonThenLB" to the combination itself, for the binding row to show.
    internal static void Migrate()
    {
        if (Plugin.WeaponButton.Value != Plugin.WheelThenLB)
            return;
        Plugin.WeaponButton.Value = Plugin.Modifier.Value + "+leftShoulder";
        Plugin.Logger.LogInfo($"[buttons] weapon button: {Plugin.WheelThenLB} is now {Plugin.WeaponButton.Value}");
    }

    // Each entry's value, or the candidate for the one being checked. A new wheel button carries
    // the weapon combination along, as OnChanged does.
    private static Func<ConfigEntryBase, object> V(ConfigEntryBase candidate, string value) => entry =>
    {
        if (entry == candidate)
            return value;
        if (candidate == Plugin.Modifier && entry == Plugin.WeaponButton && WeaponCombo() is { Held: { } held } combo &&
            held == Plugin.Modifier.Value && Plugin.WeaponButton.Value != Plugin.WheelThenLB)
            return value + "+" + combo.Pressed;
        return entry.BoxedValue;
    };

    private static (Trouble Trouble, string Message) Problem(ConfigEntryBase candidate, string candidateValue)
    {
        var value = V(candidate, candidateValue);
        string wheel = (string)value(Plugin.Modifier);
        if (!CanHoldWheel(wheel))
            return (Trouble.NotAllowed, $"{Name(wheel)} can't be the wheel button");
        bool weapons = (bool)value(Plugin.WeaponPage);
        bool merged = (bool)value(Plugin.MergedWheels) && weapons;
        if (weapons && !merged)
        {
            var (held, pressed) = Combo((string)value(Plugin.WeaponButton), wheel);
            if (held != null && held != wheel)
                return (Trouble.WheelFirst, $"the weapon button's combination must start with the wheel button ({Name(wheel)})");
            if (!CanHoldWheel(pressed))
                return (Trouble.NotAllowed, $"{Name(pressed)} can't be the weapon button");
            if (pressed == wheel)
                return (Trouble.InUse, $"the weapon button can't be the wheel button ({Name(wheel)})");
        }
        string weaponButton = WeaponWheelButton(value);

        var seen = new Dictionary<string, string>();
        string Overlap(ConfigEntry<string>[] slots, string kind, bool keepSeen, bool buttons)
        {
            if (!keepSeen)
                seen.Clear();
            for (int slot = 0; slot < slots.Length; slot++)
            {
                string control = (string)value(slots[slot]);
                string what = $"{kind} slot {slot + 1}";
                if (buttons && control == wheel)
                    return $"{what} is on the wheel button ({Name(control)})";
                if (buttons && control == weaponButton)
                    return $"{what} is on the weapon button ({Name(control)})";
                if (seen.TryGetValue(control, out var other))
                    return $"{what} and {other} are both on {Name(control)}";
                seen[control] = what;
            }
            return null;
        }

        // Keys need no hold, so all 8 of them are live at once.
        string overlap = Overlap(Plugin.SlotButtons, "spell", false, true) ??
            (weapons ? Overlap(Plugin.WeaponSlotButtons, "weapon", merged, true) : null) ??
            Overlap(Plugin.SlotKeys, "spell key", false, false) ??
            (weapons ? Overlap(Plugin.WeaponKeys, "weapon key", true, false) : null);
        return overlap == null ? (Trouble.None, null) : (Trouble.InUse, overlap);
    }

    // For the log.
    internal static string Name(string control) => control switch
    {
        "buttonNorth" => "Y",
        "buttonWest" => "X",
        "buttonEast" => "B",
        "buttonSouth" => "A",
        "dpad/up" => "D-pad up",
        "dpad/left" => "D-pad left",
        "dpad/right" => "D-pad right",
        "dpad/down" => "D-pad down",
        "leftShoulder" => "LB",
        "rightShoulder" => "RB",
        "leftTrigger" => "LT",
        "rightTrigger" => "RT",
        "leftStickPress" => "L3",
        "rightStickPress" => "R3",
        "select" => "View",
        _ => control,
    };
}
