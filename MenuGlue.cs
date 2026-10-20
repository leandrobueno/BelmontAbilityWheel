using System;
using BepInEx.Unity.IL2CPP;

namespace AbilityWheel;

// The mod's section of the Mods Settings tab (Pause menu or title screen → Settings) comes from
// the Belmont UI library, an optional plugin. Everything that touches it lives in the nested Lib
// class: a method is only compiled when it's first called, so with the library missing those
// methods are never compiled and the mod loads and works without the tab.
internal static class MenuGlue
{
    internal const string LibGuid = "leandrobueno.belmontui";

    internal static bool Available { get; private set; }

    internal static void Register()
    {
        Available = IL2CPPChainloader.Instance.Plugins.ContainsKey(LibGuid);
        if (!Available)
        {
            Plugin.Logger.LogWarning(
                $"Belmont UI ({LibGuid}) isn't installed: no Mods Settings tab in the Settings menu, edit the config file instead");
            return;
        }
        try
        {
            Lib.Register();
        }
        catch (Exception e)
        {
            Available = false;
            Plugin.Logger.LogError($"[settings] Belmont UI registration failed, the Mods Settings section is off: {e}");
        }
    }

    private static class Lib
    {
        // The same rows as the Belmont Minimap page (SettingRows.cs), English texts.
        internal static void Register()
        {
            BelmontUi.Settings.Register(Plugin.Guid, "Ability Wheel", page =>
            {
                page.Summary = "Cast up to 4 Tarot spells: hold the wheel button and press Y/X/B/A.";
                foreach (var row in SettingRows.All())
                {
                    switch (row.Kind)
                    {
                        case "bool":
                            page.Toggle(row.Entry, row.LabelEn, row.HintEn);
                            break;
                        case "float":
                            page.Slider(row.Entry, row.LabelEn, row.Min, row.Max, row.Step, row.Format, row.HintEn);
                            break;
                        default:
                            page.Choice(row.Entry, row.LabelEn, row.Values, row.ChoicesEn, row.HintEn);
                            break;
                    }
                }
                page.ResetButton(hint: "Every Ability Wheel setting back to its default.");
            });
        }
    }
}
