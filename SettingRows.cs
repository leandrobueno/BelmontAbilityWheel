using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;
using UnityEngine;

namespace AbilityWheel;

// One setting as the settings panel and Belmont Minimap's MODS page show it. Chinese texts are
// only shown on the MODS page, and every character in them must exist in the game's Chinese
// font, or that mod drops the row.
internal sealed class SettingRow
{
    internal ConfigEntryBase Entry;
    internal string Kind;   // "bool", "float" or "enum", as the MODS page names them
    internal string LabelEn, LabelZh, HintEn, HintZh;
    internal float Min, Max, Step;
    internal string Format;
    internal string[] Values, ChoicesEn, ChoicesZh;

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
    internal static List<SettingRow> All()
    {
        var rows = new List<SettingRow>
        {
            Choice(Plugin.Modifier, "轮盘键(手柄)", "Wheel button (gamepad)",
                "按住此键并按 Y/X/B/A 施放对应法术。轻按仍会执行该键原本的动作(RB 为互动)。LT 是圣光格挡,不建议使用。",
                "Hold it and press Y/X/B/A to cast that slot's spell. A quick tap still does the button's normal action (RB: Interact). LT is Light Parry, so it's a poor choice.",
                Plugin.ModifierChoices, new[] { "RB", "LB", "RT", "LT", "L3", "R3", "View" }),

            Toggle(Plugin.KeepHomeCard, "固定 Y 键法术", "Pin Y spell",
                "开启:在塔罗菜单中装备的法术始终留在 Y 键;轮盘法术只在施放期间装备。镜行回溯放出镜子后会保持装备,直到再次施放(传送回镜子)、按 Y、施放其他轮盘法术或存档;混沌之轮在轮存在期间保持装备。使用其他法术会让镜子或轮消失。关闭:最后施放的轮盘法术保持装备。",
                "On: the spell equipped in the Tarot menu always stays on Y; a wheel spell is only equipped while it plays. " +
                "Mirror Walk stays equipped after placing its mirror, until you cast it again (teleport), press Y, cast " +
                "another wheel spell or save; Wheel of Chaos stays equipped while its wheels are out. " +
                "Casting another spell makes the mirror or wheels vanish. Off: the last wheel spell stays equipped."),

            Choice(Plugin.HudVisibility, "轮盘显示", "Show wheel",
                "游戏中何时在右下角显示 4 张卡牌。塔罗菜单中始终显示。",
                "When the 4 cards show in the bottom-right corner during gameplay. Always shown in the Tarot menu.",
                new[] { "Always", "WhileHeld", "Never" },
                new[] { "Always", "While holding the wheel button", "Never" },
                new[] { "始终", "按住轮盘键时", "从不" }),

            Choice(Plugin.ButtonLabels, "按键标签", "Button labels",
                "何时在卡牌上显示对应按键(Y/X/B/A,使用键盘时为键盘按键)。放入轮盘时始终显示。",
                "When each card shows its button (Y/X/B/A, or its key after using the keyboard). " +
                "Always shown while putting a card or weapon on the wheel.",
                new[] { "Always", "WhileHeld", "Never" },
                new[] { "Always", "While holding the wheel button", "Never" },
                new[] { "始终", "按住轮盘键时", "从不" }),

            Toggle(Plugin.FollowGameHud, "随游戏 HUD 隐藏", "Fade with game HUD",
                "游戏 HUD 被隐藏时(例如使用 HUD Auto Hide),轮盘一同淡出。按住轮盘键或在塔罗菜单中时始终显示。",
                "Fade the cards out while the game's HUD is hidden, for example by HUD Auto Hide. " +
                "Holding the wheel button or the Tarot menu always shows them."),

            Slider(Plugin.HudOpacity, "轮盘不透明度", "Wheel opacity",
                "轮盘卡牌的不透明程度。", "How solid the wheel's tiles are.", 0.2f, 1f, 0.05f, "{0:0%}"),
            Slider(Plugin.HudScale, "轮盘大小", "Wheel size",
                "轮盘的显示大小。", "How big the wheel is.", 0.5f, 2f, 0.05f, "{0:0.00}x"),
            Slider(Plugin.HudOffsetX, "距右边缘", "Distance from right edge",
                "以 1080p 像素计。", "In pixels at 1080p.", 0f, 1600f, 8f, "{0:0}"),
            Slider(Plugin.HudOffsetY, "距下边缘", "Distance from bottom edge",
                "以 1080p 像素计。", "In pixels at 1080p.", 0f, 800f, 8f, "{0:0}"),
        };

        string[] keyNames = Keys.SlotKeyChoices.Select(KeyName).ToArray();
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            string label = Wheel.SlotLabels[slot];
            rows.Add(Choice(Plugin.SlotKeys[slot], $"{label} 的键盘按键", $"Keyboard key for slot {label}",
                "按下即施放该位置的法术,键盘无需按住轮盘键。",
                "Casts that slot's spell at once; no hold needed on keyboard.",
                Keys.SlotKeyChoices, keyNames));
        }

        rows.Add(Choice(Plugin.AssignKey, "加入轮盘按键(键盘)", "Add to wheel key (keyboard)",
            "在塔罗菜单或装备菜单中选中一张卡牌或一把武器,按此键后再按对应的键盘按键即可放入轮盘。手柄使用 L3。",
            "In the Tarot or Equipment menu, highlight a card or weapon, press this key, then a slot key to put it " +
            "on the wheel. Gamepad: L3.",
            Keys.AssignKeyChoices, Keys.AssignKeyChoices));

        rows.Add(Toggle(Plugin.WeaponPage, "武器轮盘", "Weapon wheel",
            "按住轮盘键后再按住 LB,然后按 Y/X/B/A 切换为该位置的武器,武器会保持装备。在装备菜单中选中武器并按 L3 放入轮盘。单独按 LB 仍是使用药水。",
            "Hold the wheel button, then also hold LB, and press Y/X/B/A to switch to that slot's weapon; it stays " +
            "equipped. In the Equipment menu, highlight a weapon and click L3 to put it on the wheel. LB on its own " +
            "still drinks a potion."));
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            string label = Wheel.SlotLabels[slot];
            rows.Add(Choice(Plugin.WeaponKeys[slot], $"{label} 的武器按键", $"Weapon key for slot {label}",
                "按下即装备该位置的武器。",
                "Switches to that slot's weapon at once.",
                Keys.SlotKeyChoices, keyNames));
        }

        return rows;
    }

    internal static void ResetAll()
    {
        foreach (var row in All())
            row.Entry.BoxedValue = row.Entry.DefaultValue;
        Plugin.Settings.Save();
    }

    private static SettingRow Toggle(ConfigEntryBase entry, string labelZh, string labelEn, string hintZh, string hintEn) =>
        new() { Entry = entry, Kind = "bool", LabelZh = labelZh, LabelEn = labelEn, HintZh = hintZh, HintEn = hintEn };

    private static SettingRow Slider(ConfigEntryBase entry, string labelZh, string labelEn, string hintZh, string hintEn,
        float min, float max, float step, string format) =>
        new()
        {
            Entry = entry, Kind = "float", LabelZh = labelZh, LabelEn = labelEn, HintZh = hintZh, HintEn = hintEn,
            Min = min, Max = max, Step = step, Format = format,
        };

    private static SettingRow Choice(ConfigEntryBase entry, string labelZh, string labelEn, string hintZh, string hintEn,
        string[] values, string[] choicesEn, string[] choicesZh = null) =>
        new()
        {
            Entry = entry, Kind = "enum", LabelZh = labelZh, LabelEn = labelEn, HintZh = hintZh, HintEn = hintEn,
            Values = values, ChoicesEn = choicesEn, ChoicesZh = choicesZh ?? choicesEn,
        };

    private static string KeyName(string key) =>
        key.StartsWith("Digit", StringComparison.Ordinal) ? key.Substring(5)
        : key.StartsWith("Mouse", StringComparison.Ordinal) ? "Mouse " + key.Substring(5)
        : key;
}
