using System;
using System.Collections.Generic;
using System.Linq;
using AbilityWheel;
using BepInEx.Configuration;

namespace BelmontModSettings;

// The page in the pause menu's MODS tab, which comes from the Belmont Minimap mod. It finds
// this class by reflection, so its full name and the object[] layout Describe() returns must
// stay exactly as they are; no reference to that mod is needed. Every Chinese character here
// must exist in the game's Chinese font, or Belmont Minimap drops the row.
public static class ProviderV2
{
    public static object[] Describe()
    {
        var items = new List<object>
        {
            Choice(Plugin.Modifier, "轮盘键(手柄)", "Wheel button (gamepad)",
                "按住此键并按 Y/X/B/A 施放对应法术。轻按仍会执行该键原本的动作(RB 为互动)。LT 是圣光格挡,不建议使用。",
                "Hold it and press Y/X/B/A to cast that slot's spell. A quick tap still does the button's normal action (RB: Interact). LT is Light Parry, so it's a poor choice.",
                Plugin.ModifierChoices,
                new[] { "RB", "LB", "RT", "LT", "L3", "R3", "View" }),
        };

        string[] keyNames = Keys.SlotKeyChoices.Select(KeyName).ToArray();
        for (int slot = 0; slot < Wheel.SlotLabels.Length; slot++)
        {
            string label = Wheel.SlotLabels[slot];
            items.Add(Choice(Plugin.SlotKeys[slot], $"{label} 的键盘按键", $"Keyboard key for slot {label}",
                "按下即施放该位置的法术,键盘无需按住轮盘键。",
                "Casts that slot's spell at once; no hold needed on keyboard.",
                Keys.SlotKeyChoices, keyNames));
        }

        items.Add(Choice(Plugin.AssignKey, "加入轮盘按键(键盘)", "Add to wheel key (keyboard)",
            "在塔罗菜单中选中一张卡牌,按此键后再按对应的键盘按键即可放入轮盘。手柄使用 L3。",
            "In the Tarot (Arcanas) menu, highlight a card, press this key, then a slot key to put it on the wheel. Gamepad: L3.",
            Keys.AssignKeyChoices, Keys.AssignKeyChoices));

        items.Add(Toggle(Plugin.KeepHomeCard, "固定 Y 键法术", "Pin Y spell",
            "开启:在塔罗菜单中装备的法术始终留在 Y 键;轮盘法术只在施放期间装备。镜行回溯放出镜子后会保持装备,直到再次施放(传送回镜子)、按 Y、施放其他轮盘法术或存档;混沌之轮在轮存在期间保持装备。使用其他法术会让镜子或轮消失。关闭:最后施放的轮盘法术保持装备。",
            "On: the spell equipped in the Tarot menu always stays on Y; a wheel spell is only equipped while it plays. " +
            "Mirror Walk stays equipped after placing its mirror, until you cast it again (teleport), press Y, cast " +
            "another wheel spell or save; Wheel of Chaos stays equipped while its wheels are out. " +
            "Casting another spell makes the mirror or wheels vanish. " +
            "Off: the last wheel spell stays equipped."));

        items.Add(Choice(Plugin.HudVisibility, "轮盘显示", "Show wheel",
            "游戏中何时在右下角显示 4 张卡牌。塔罗菜单中始终显示。",
            "When the 4 cards show in the bottom-right corner during gameplay. Always shown in the Tarot menu.",
            new[] { "Always", "WhileHeld", "Never" },
            new[] { "Always", "While holding the wheel button", "Never" },
            new[] { "始终", "按住轮盘键时", "从不" }));

        items.Add(Slider(Plugin.HudOpacity, "轮盘不透明度", "Wheel opacity", "", "", 0.2f, 1f, 0.05f, "{0:0%}"));
        items.Add(Slider(Plugin.HudScale, "轮盘大小", "Wheel size", "", "", 0.5f, 2f, 0.05f, "{0:0.00}x"));
        items.Add(Slider(Plugin.HudOffsetX, "距右边缘", "Distance from right edge",
            "以 1080p 像素计。", "In pixels at 1080p.", 0f, 1600f, 8f, "{0:0}"));
        items.Add(Slider(Plugin.HudOffsetY, "距下边缘", "Distance from bottom edge",
            "以 1080p 像素计。", "In pixels at 1080p.", 0f, 800f, 8f, "{0:0}"));

        return new object[]
        {
            Plugin.Guid,
            "技能轮盘", "Ability Wheel",
            "按住轮盘键并按 Y/X/B/A,快速施放 4 张塔罗牌的法术。",
            "Cast up to 4 Tarot spells: hold the wheel button and press Y/X/B/A.",
            items.ToArray(),
            (Action)Save,
            (Action)Reset,
        };
    }

    private static void Save() => Plugin.Settings.Save();

    private static void Reset()
    {
        foreach (var entry in MenuEntries())
            entry.BoxedValue = entry.DefaultValue;
        Plugin.Settings.Save();
    }

    // Must match the rows above.
    private static IEnumerable<ConfigEntryBase> MenuEntries()
    {
        yield return Plugin.Modifier;
        foreach (var key in Plugin.SlotKeys)
            yield return key;
        yield return Plugin.AssignKey;
        yield return Plugin.KeepHomeCard;
        yield return Plugin.HudVisibility;
        yield return Plugin.HudOpacity;
        yield return Plugin.HudScale;
        yield return Plugin.HudOffsetX;
        yield return Plugin.HudOffsetY;
    }

    private static Dictionary<string, object> Row(ConfigEntryBase entry, string type,
        string labelZh, string labelEn, string hintZh, string hintEn) => new()
    {
        ["entry"] = entry,
        ["type"] = type,
        ["labelZh"] = labelZh,
        ["labelEn"] = labelEn,
        ["hintZh"] = hintZh,
        ["hintEn"] = hintEn,
    };

    private static Dictionary<string, object> Toggle(ConfigEntryBase entry,
        string labelZh, string labelEn, string hintZh, string hintEn) =>
        Row(entry, "bool", labelZh, labelEn, hintZh, hintEn);

    private static Dictionary<string, object> Slider(ConfigEntryBase entry, string labelZh, string labelEn,
        string hintZh, string hintEn, float min, float max, float step, string format)
    {
        var row = Row(entry, "float", labelZh, labelEn, hintZh, hintEn);
        row["min"] = min;
        row["max"] = max;
        row["step"] = step;
        row["format"] = format;
        return row;
    }

    private static Dictionary<string, object> Choice(ConfigEntryBase entry, string labelZh, string labelEn,
        string hintZh, string hintEn, string[] values, string[] choicesEn, string[] choicesZh = null)
    {
        var row = Row(entry, "enum", labelZh, labelEn, hintZh, hintEn);
        row["values"] = values;
        row["choicesEn"] = choicesEn;
        row["choicesZh"] = choicesZh ?? choicesEn;
        return row;
    }

    private static string KeyName(string key) =>
        key.StartsWith("Digit", StringComparison.Ordinal) ? key.Substring(5)
        : key.StartsWith("Mouse", StringComparison.Ordinal) ? "Mouse " + key.Substring(5)
        : key;
}
