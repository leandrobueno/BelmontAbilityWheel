using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;
using T = System.Collections.Generic.Dictionary<string, string>;

namespace AbilityWheel;

// One setting as the Mods Settings page (Belmont UI) and Belmont Minimap's MODS page show it, with
// its texts in the game's languages (Texts*.cs). Belmont Minimap shows only English and
// Simplified Chinese, and drops a row with a character missing from the game's Chinese font.
internal sealed class SettingRow
{
    internal ConfigEntryBase Entry;
    internal string Kind;   // "bool", "float" or "enum", as the MODS page names them
    internal T Label, Hint;
    internal float Min, Max, Step;
    internal string Format;
    internal string[] Values;
    internal T[] Choices;   // per value; null: the value names below are shown in every language

    private string[] _names;

    internal string LabelEn => Texts.Resolve(Label, "en");
    internal string LabelZh => Texts.Resolve(Label, "zh-CN");
    internal string HintEn => Texts.Resolve(Hint, "en");
    internal string HintZh => Texts.Resolve(Hint, "zh-CN");
    internal string[] ChoicesEn => ChoiceNames("en");
    internal string[] ChoicesZh => ChoiceNames("zh-CN");

    internal string[] ChoiceNames(string locale) =>
        Choices != null ? Array.ConvertAll(Choices, c => Texts.Resolve(c, locale)) : _names;

    internal SettingRow WithNames(string[] names)
    {
        _names = names;
        return this;
    }

    internal string ValueText()
    {
        switch (Kind)
        {
            case "bool":
                return (bool)Entry.BoxedValue ? "On" : "Off";
            case "float":
                return string.Format(Format, (float)Entry.BoxedValue);
            default:
                int index = Array.IndexOf(Values, (string)Entry.BoxedValue);
                return index >= 0 ? ChoicesEn[index] : (string)Entry.BoxedValue;
        }
    }

    internal void Change(int direction)
    {
        switch (Kind)
        {
            case "bool":
                Entry.BoxedValue = !(bool)Entry.BoxedValue;
                break;
            case "float":
                float value = Mathf.Clamp((float)Entry.BoxedValue + direction * Step, Min, Max);
                // On the step grid and rounded, so the config file doesn't get 0.70000005.
                value = Min + Mathf.Round((value - Min) / Step) * Step;
                Entry.BoxedValue = (float)Math.Round(value, 4);
                break;
            default:
                int index = Array.IndexOf(Values, (string)Entry.BoxedValue);
                Entry.BoxedValue = Values[((index < 0 ? 0 : index) + direction + Values.Length) % Values.Length];
                break;
        }
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

    internal static void ResetAll()
    {
        foreach (var row in All())
            row.Entry.BoxedValue = row.Entry.DefaultValue;
        Plugin.Settings.Save();
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
