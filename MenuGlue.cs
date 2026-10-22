using System;
using System.Runtime.CompilerServices;
using BepInEx.Unity.IL2CPP;

namespace AbilityWheel;

// The mod's page in the pause menu's MODS tab (title screen: Settings) comes from the Belmont UI
// library, an optional plugin. Everything that touches it lives in the nested classes below: a
// method is only compiled when it's first called, so with the library missing those methods are
// never compiled and the mod loads and works without the page.
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
                $"Belmont UI ({LibGuid}) isn't installed: no settings page in the pause menu, edit the config file instead");
            return;
        }
        try
        {
            // Press-to-bind rows need Belmont UI 0.4, translated texts 0.3; with 0.2 the page is in
            // English, with lists for the buttons.
            if (Lib.Has("BelmontUi.BindingOptions"))
                Bound.Register();
            else if (Lib.Has("BelmontUi.LocalizedText"))
                Translated.Register();
            else
                Lib.RegisterEnglish();
        }
        catch (Exception e)
        {
            Available = false;
            Plugin.Logger.LogError($"[settings] Belmont UI registration failed, the settings page is off: {e}");
        }
    }

    // Belmont UI 0.4 and later: section headers, and buttons and keys set by pressing them. Each
    // version's code is a class of its own, lambdas included: an older library lacks the newer
    // types, and a class that mentions one (even in a lambda's hidden closure class) can't be
    // loaded there.
    private static class Bound
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Register()
        {
            Bindings.Migrate();
            bool canHide = typeof(BelmontUi.SettingsMenu).GetMethod("VisibleWhen") != null;
            BelmontUi.Settings.Register(Plugin.Guid, new BelmontUi.LocalizedText(Texts.Title), page =>
            {
                page.LocalizedSummary = new BelmontUi.LocalizedText(Texts.Summary);
                foreach (var row in SettingRows.All())
                {
                    var label = new BelmontUi.LocalizedText(row.Label);
                    switch (row.Kind)
                    {
                        case "header":
                            page.Header(label);
                            break;
                        case "bool":
                            page.Toggle(row.Entry, label, new BelmontUi.LocalizedText(row.Hint));
                            break;
                        case "float":
                            page.Slider(row.Entry, label, row.Min, row.Max, row.Step, row.Format,
                                new BelmontUi.LocalizedText(row.Hint));
                            break;
                        case "pad":
                        case "key":
                            page.Binding(row.Entry, label, Options(row), new BelmontUi.LocalizedText(row.BindHint ?? row.Hint));
                            break;
                        default:
                            page.Choice(row.Entry, label, row.Values, Choices(row), new BelmontUi.LocalizedText(row.Hint));
                            break;
                    }
                    // Hidden with the wheels merged (VisibleWhen came late in 0.4: without it they stay shown).
                    if (row.HiddenWhenMerged && canHide)
                        HideWhenMerged(page);
                }
                page.ResetButton();   // translated by the library
            });
            Texts.Language = BelmontUi.Settings.Language;
            BelmontUi.Settings.LanguageChanged += code => Texts.Language = code;
        }

        // On its own, so a 0.4 build without VisibleWhen only fails here, when called, not the whole page.
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static void HideWhenMerged(BelmontUi.SettingsMenu page) =>
            page.VisibleWhen(() => !Plugin.MergedWheels.Value);

        private static BelmontUi.BindingOptions Options(SettingRow row)
        {
            var entry = row.Entry;
            return new BelmontUi.BindingOptions
            {
                Devices = row.Kind == "key" ? BelmontUi.BindingDevices.Keyboard : BelmontUi.BindingDevices.Gamepad,
                AllowCombinations = row.Combos,
                // A combination's parts are both buttons that can hold a wheel; Bindings checks the order.
                Allowed = row.Combos ? Plugin.ModifierChoices : row.Values,
                Validate = binding => Bindings.Check(entry, binding) switch
                {
                    Bindings.Trouble.InUse => new BelmontUi.LocalizedText(Texts.AlreadyAssigned),
                    Bindings.Trouble.WheelFirst => new BelmontUi.LocalizedText(Texts.WheelFirst),
                    _ => null,
                },
            };
        }

        private static BelmontUi.LocalizedText[] Choices(SettingRow row) => row.Choices != null
            ? Array.ConvertAll(row.Choices, c => new BelmontUi.LocalizedText(c))
            : Array.ConvertAll(row.ChoicesEn, c => new BelmontUi.LocalizedText(c));
    }

    // Belmont UI 0.3: translated texts, lists for buttons and keys.
    private static class Translated
    {
        [MethodImpl(MethodImplOptions.NoInlining)]
        internal static void Register()
        {
            BelmontUi.Settings.Register(Plugin.Guid, new BelmontUi.LocalizedText(Texts.Title), page =>
            {
                page.LocalizedSummary = new BelmontUi.LocalizedText(Texts.Summary);
                foreach (var row in SettingRows.All())
                {
                    var label = new BelmontUi.LocalizedText(row.Label);
                    var hint = new BelmontUi.LocalizedText(row.Hint ?? new());
                    switch (row.Kind)
                    {
                        case "header":
                            break;
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

    // Belmont UI 0.2 (English only), and finding out which version is there.
    private static class Lib
    {
        internal static bool Has(string type) => typeof(BelmontUi.Settings).Assembly.GetType(type) != null;

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
                        case "header":
                            break;
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
