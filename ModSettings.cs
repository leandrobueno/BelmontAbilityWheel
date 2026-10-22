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
        Texts.Resolve(Texts.Title, "zh-CN"), Texts.Resolve(Texts.Title, "en"),
        Texts.Resolve(Texts.Summary, "zh-CN"),
        Texts.Resolve(Texts.Summary, "en"),
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
