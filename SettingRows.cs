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
    // "bool", "float" or "enum"; "pad" and "key" are buttons and keys, a press-to-bind row on Belmont
    // UI 0.4 and a list (Values) before it; "header" starts a section.
    internal string Kind;
    internal T Label, Hint;
    internal T BindHint;    // the hint of the press-to-bind row, when it says more than Hint
    internal bool Combos;   // the press-to-bind row also takes "held+pressed"
    internal bool HiddenWhenMerged;   // only means something with the wheels apart
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

    // Wheel visuals first, then the gamepad, then the keyboard.
    internal static List<SettingRow> All()
    {
        var shown = new[] { Texts.Always, Texts.WhileHeld, Texts.Never };
        var rows = new List<SettingRow>
        {
            Header(Texts.HeaderWheel),
            Choice(Plugin.HudVisibility, Texts.ShowWheel, Texts.ShowWheelHint, ShowValues, shown),
            Toggle(Plugin.FollowGameHud, Texts.FadeWithHud, Texts.FadeWithHudHint),
            Choice(Plugin.ButtonLabels, Texts.ButtonLabels, Texts.ButtonLabelsHint, ShowValues, shown),
            Slider(Plugin.HudOpacity, Texts.Opacity, Texts.OpacityHint, 0.2f, 1f, 0.05f, "{0:0%}"),
            Slider(Plugin.HudScale, Texts.Size, Texts.SizeHint, 0.5f, 2f, 0.05f, "{0:0.00}x"),
            Slider(Plugin.HudOffsetX, Texts.RightEdge, Texts.PixelsHint, 0f, 1600f, 8f, "{0:0}"),
            Slider(Plugin.HudOffsetY, Texts.BottomEdge, Texts.PixelsHint, 0f, 800f, 8f, "{0:0}"),
            Toggle(Plugin.KeepHomeCard, Texts.PinY, Texts.PinYHint),

            Header(Texts.HeaderGamepad),
            Pad(Plugin.Modifier, Texts.WheelButton, Texts.WheelButtonHint, Plugin.ModifierChoices, Texts.WheelButtonBindHint)
                .WithNames(Array.ConvertAll(Plugin.ModifierChoices, Bindings.Name)),
            Toggle(Plugin.MergedWheels, Texts.Merged, Texts.MergedHint),
        };
        var weaponPage = Toggle(Plugin.WeaponPage, Texts.WeaponWheel, Texts.WeaponWheelHint);
        weaponPage.HiddenWhenMerged = true;
        rows.Add(weaponPage);

        var weaponButtons = new T[Plugin.WeaponButtonChoices.Length];
        weaponButtons[0] = Texts.WheelThenLB;
        for (int i = 1; i < weaponButtons.Length; i++)
            weaponButtons[i] = new T { ["en"] = Bindings.Name(Plugin.WeaponButtonChoices[i]) };
        var weaponButton = Pad(Plugin.WeaponButton, Texts.WeaponButton, Texts.WeaponButtonHint, Plugin.WeaponButtonChoices,
            Texts.WeaponButtonBindHint);
        weaponButton.Choices = weaponButtons;
        weaponButton.Combos = true;
        weaponButton.HiddenWhenMerged = true;
        rows.Add(weaponButton);

        string[] buttonNames = Array.ConvertAll(Plugin.SlotButtonChoices, Bindings.Name);
        for (int slot = 0; slot < Plugin.SlotButtons.Length; slot++)
        {
            rows.Add(Pad(Plugin.SlotButtons[slot], Texts.Format(Texts.SpellSlotButton, Number(slot)),
                Texts.SpellSlotButtonHint, Plugin.SlotButtonChoices).WithNames(buttonNames));
        }
        for (int slot = 0; slot < Plugin.WeaponSlotButtons.Length; slot++)
        {
            rows.Add(Pad(Plugin.WeaponSlotButtons[slot], Texts.Format(Texts.WeaponSlotButton, Number(slot)),
                Texts.WeaponSlotButtonHint, Plugin.SlotButtonChoices).WithNames(buttonNames));
        }

        rows.Add(Header(Texts.HeaderKeyboard));
        string[] keyNames = Keys.SlotKeyChoices.Select(KeyName).ToArray();
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            rows.Add(Key(Plugin.SlotKeys[slot], Texts.Format(Texts.SlotKey, Number(slot)), Texts.SlotKeyHint,
                Keys.SlotKeyChoices).WithNames(keyNames));
        }
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            rows.Add(Key(Plugin.WeaponKeys[slot], Texts.Format(Texts.WeaponKey, Number(slot)),
                Texts.WeaponKeyHint, Keys.SlotKeyChoices).WithNames(keyNames));
        }
        rows.Add(Key(Plugin.AssignKey, Texts.AssignKey, Texts.AssignKeyHint, Keys.AssignKeyChoices)
            .WithNames(Keys.AssignKeyChoices));

        return rows;
    }

    // Slots are numbered 1-4 on the page; the config keeps its Y/X/B/A names.
    private static string Number(int slot) => (slot + 1).ToString();

    private static SettingRow Toggle(ConfigEntryBase entry, T label, T hint) =>
        new() { Entry = entry, Kind = "bool", Label = label, Hint = hint };

    private static SettingRow Slider(ConfigEntryBase entry, T label, T hint, float min, float max, float step,
        string format) =>
        new() { Entry = entry, Kind = "float", Label = label, Hint = hint, Min = min, Max = max, Step = step, Format = format };

    private static SettingRow Choice(ConfigEntryBase entry, T label, T hint, string[] values, T[] choices = null) =>
        new() { Entry = entry, Kind = "enum", Label = label, Hint = hint, Values = values, Choices = choices };

    private static SettingRow Pad(ConfigEntryBase entry, T label, T hint, string[] values, T bindHint = null) =>
        new() { Entry = entry, Kind = "pad", Label = label, Hint = hint, BindHint = bindHint, Values = values };

    private static SettingRow Key(ConfigEntryBase entry, T label, T hint, string[] values) =>
        new() { Entry = entry, Kind = "key", Label = label, Hint = hint, Values = values };

    private static SettingRow Header(T title) => new() { Kind = "header", Label = title };

    private static string KeyName(string key) =>
        key.StartsWith("Digit", StringComparison.Ordinal) ? key.Substring(5)
        : key.StartsWith("Mouse", StringComparison.Ordinal) ? "Mouse " + key.Substring(5)
        : key;
}
