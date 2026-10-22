using System;
using System.Runtime.CompilerServices;
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
            // Translated texts need Belmont UI 0.3; with 0.2 the page is in English.
            if (Lib.CanTranslate())
                Translated.Register();
            else
                Lib.RegisterEnglish();
        }
        catch (Exception e)
        {
            Available = false;
            Plugin.Logger.LogError($"[settings] Belmont UI registration failed, the Mods Settings section is off: {e}");
        }
    }

    // Belmont UI 0.3 and later. A class of its own, lambdas included: Belmont UI 0.2 has no
    // LocalizedText, and a class that mentions it (even in a lambda's hidden closure class) can't
    // be loaded there.
    private static class Translated
    {
        // The same rows as the Belmont Minimap page (SettingRows.cs), in the game's language.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Register()
        {
            BelmontUi.Settings.Register(Plugin.Guid, new BelmontUi.LocalizedText(Texts.Title), page =>
            {
                page.LocalizedSummary = new BelmontUi.LocalizedText(Texts.Summary);
                foreach (var row in SettingRows.All())
                {
                    var label = new BelmontUi.LocalizedText(row.Label);
                    var hint = new BelmontUi.LocalizedText(row.Hint);
                    switch (row.Kind)
                    {
                        case "bool":
                            page.Toggle(row.Entry, label, hint);
                            break;
                        case "float":
                            page.Slider(row.Entry, label, row.Min, row.Max, row.Step, row.Format, hint);
                            break;
                        default:
                            var choices = row.Choices != null
                                ? Array.ConvertAll(row.Choices, c => new BelmontUi.LocalizedText(c))
                                : Array.ConvertAll(row.ChoicesEn, c => new BelmontUi.LocalizedText(c));
                            page.Choice(row.Entry, label, row.Values, choices, hint);
                            break;
                    }
                }
                page.ResetButton();   // translated by the library
            });

            // The HUD's menu hints follow the game's language too.
            Texts.Language = BelmontUi.Settings.Language;
            BelmontUi.Settings.LanguageChanged += code => Texts.Language = code;
        }
    }

    // Each method is compiled only when called, so without the library none of this runs.
    private static class Lib
    {
        internal static bool CanTranslate() =>
            typeof(BelmontUi.Settings).Assembly.GetType("BelmontUi.LocalizedText") != null;

        // Belmont UI 0.2: English texts.
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void RegisterEnglish()
        {
            BelmontUi.Settings.Register(Plugin.Guid, Texts.Resolve(Texts.Title, "en"), page =>
            {
                page.Summary = Texts.Resolve(Texts.Summary, "en");
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
