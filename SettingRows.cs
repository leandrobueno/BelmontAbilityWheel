using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// One setting as Belmont UI's settings page shows it, with its texts in the game's languages
// (Texts*.cs).
internal sealed class SettingRow
{
    internal ConfigEntryBase Entry;
    internal string Kind;   // "bool", "float" or "enum"
    internal T Label, Hint;
    internal float Min, Max, Step;
    internal string Format;
    internal string[] Values;
    internal T[] Choices;   // per value; null: the value names below are shown in every language

    private string[] _names;

    internal string LabelEn => Texts.Resolve(Label, "en");
    internal string HintEn => Texts.Resolve(Hint, "en");
    internal string[] ChoicesEn => ChoiceNames("en");

    internal string[] ChoiceNames(string locale) =>
        Choices != null ? Array.ConvertAll(Choices, c => Texts.Resolve(c, locale)) : _names;

    internal SettingRow WithNames(string[] names)
    {
        _names = names;
        return this;
    }
}

internal static class SettingRows
{
    private static readonly string[] ShowValues = { "Always", "WhileHeld", "Never" };

    internal static List<SettingRow> All()
    {
        var shown = new[] { Texts.Always, Texts.WhileHeld, Texts.Never };
        var rows = new List<SettingRow>
        {
            Choice(Plugin.Modifier, Texts.WheelButton, Texts.WheelButtonHint, Plugin.ModifierChoices)
                .WithNames(new[] { "RB", "LB", "RT", "LT", "L3", "R3", "View" }),
            Toggle(Plugin.KeepHomeCard, Texts.PinY, Texts.PinYHint),
            Choice(Plugin.HudVisibility, Texts.ShowWheel, Texts.ShowWheelHint, ShowValues, shown),
            Choice(Plugin.ButtonLabels, Texts.ButtonLabels, Texts.ButtonLabelsHint, ShowValues, shown),
            Toggle(Plugin.FollowGameHud, Texts.FadeWithHud, Texts.FadeWithHudHint),
            Slider(Plugin.HudOpacity, Texts.Opacity, Texts.OpacityHint, 0.2f, 1f, 0.05f, "{0:0%}"),
            Slider(Plugin.HudScale, Texts.Size, Texts.SizeHint, 0.5f, 2f, 0.05f, "{0:0.00}x"),
            Slider(Plugin.HudOffsetX, Texts.RightEdge, Texts.PixelsHint, 0f, 1600f, 8f, "{0:0}"),
            Slider(Plugin.HudOffsetY, Texts.BottomEdge, Texts.PixelsHint, 0f, 800f, 8f, "{0:0}"),
        };

        string[] keyNames = Keys.SlotKeyChoices.Select(KeyName).ToArray();
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            rows.Add(Choice(Plugin.SlotKeys[slot], Texts.Format(Texts.SlotKey, Wheel.SlotLabels[slot]), Texts.SlotKeyHint,
                Keys.SlotKeyChoices).WithNames(keyNames));
        }

        rows.Add(Choice(Plugin.AssignKey, Texts.AssignKey, Texts.AssignKeyHint, Keys.AssignKeyChoices)
            .WithNames(Keys.AssignKeyChoices));

        rows.Add(Toggle(Plugin.WeaponPage, Texts.WeaponWheel, Texts.WeaponWheelHint));
        var buttons = new[] { "RB", "LB", "RT", "LT", "L3", "R3", "View" };
        var weaponButtons = new T[Plugin.WeaponButtonChoices.Length];
        weaponButtons[0] = Texts.WheelThenLB;
        for (int i = 1; i < weaponButtons.Length; i++)
            weaponButtons[i] = new T { ["en"] = buttons[i - 1] };
        rows.Add(Choice(Plugin.WeaponButton, Texts.WeaponButton, Texts.WeaponButtonHint, Plugin.WeaponButtonChoices,
            weaponButtons));
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            rows.Add(Choice(Plugin.WeaponKeys[slot], Texts.Format(Texts.WeaponKey, Wheel.SlotLabels[slot]),
                Texts.WeaponKeyHint, Keys.SlotKeyChoices).WithNames(keyNames));
        }

        return rows;
    }

    private static SettingRow Toggle(ConfigEntryBase entry, T label, T hint) =>
        new() { Entry = entry, Kind = "bool", Label = label, Hint = hint };

    private static SettingRow Slider(ConfigEntryBase entry, T label, T hint, float min, float max, float step,
        string format) =>
        new() { Entry = entry, Kind = "float", Label = label, Hint = hint, Min = min, Max = max, Step = step, Format = format };

    private static SettingRow Choice(ConfigEntryBase entry, T label, T hint, string[] values, T[] choices = null) =>
        new() { Entry = entry, Kind = "enum", Label = label, Hint = hint, Values = values, Choices = choices };

    private static string KeyName(string key) =>
        key.StartsWith("Digit", StringComparison.Ordinal) ? key.Substring(5)
        : key.StartsWith("Mouse", StringComparison.Ordinal) ? "Mouse " + key.Substring(5)
        : key;
}
