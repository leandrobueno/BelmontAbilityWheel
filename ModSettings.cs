using System;
using System.Collections.Generic;
using System.Linq;
using AbilityWheel;

namespace BelmontModSettings;

// The page in the pause menu's MODS tab, which comes from the Belmont Minimap mod. It finds
// this class by reflection, so its full name and the object[] layout Describe() returns must
// stay exactly as they are; no reference to that mod is needed. Without it, the same settings
// are in the Tarot menu's settings panel.
public static class ProviderV2
{
    public static object[] Describe() => new object[]
    {
        Plugin.Guid,
        "技能轮盘", "Ability Wheel",
        "按住轮盘键并按 Y/X/B/A,快速施放 4 张塔罗牌的法术。",
        "Cast up to 4 Tarot spells: hold the wheel button and press Y/X/B/A.",
        SettingRows.All().Select(Item).ToArray<object>(),
        (Action)Plugin.Settings.Save,
        (Action)SettingRows.ResetAll,
    };

    private static Dictionary<string, object> Item(SettingRow row)
    {
        var item = new Dictionary<string, object>
        {
            ["entry"] = row.Entry,
            ["type"] = row.Kind,
            ["labelZh"] = row.LabelZh,
            ["labelEn"] = row.LabelEn,
            ["hintZh"] = row.HintZh,
            ["hintEn"] = row.HintEn,
        };
        if (row.Kind == "float")
        {
            item["min"] = row.Min;
            item["max"] = row.Max;
            item["step"] = row.Step;
            item["format"] = row.Format;
        }
        else if (row.Kind == "enum")
        {
            item["values"] = row.Values;
            item["choicesEn"] = row.ChoicesEn;
            item["choicesZh"] = row.ChoicesZh;
        }
        return item;
    }
}
