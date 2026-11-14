using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace AbilityWheel;

// The wheel's prompts in the Arcanas and Armoury menus, drawn as the game draws its own footer
// prompts ("Back (B)"): each is a copy of that prompt, so the font, its gold leaf, the button
// icons and the spacing are the game's. They go at the end of the footer's left group, where the
// Armoury has Navigate and Equip. A copy keeps what draws and lays out; what makes the original a
// button, translates it or fills in its input is taken off.
internal sealed class MenuPrompts
{
    private const string LabelPath = "CommonUI_InputButton/InputText layout/InputText";
    private const string TagPath = "CommonUI_InputButton/CommonUI_Input/InputTag";
    private const string KeyFramePath = "CommonUI_InputButton/CommonUI_Input/KeyboardFrame";

    // In this order: the triggers hang on the button.
    private static readonly string[] Unwanted =
    {
        "PointerExitTrigger", "PointerClickTrigger", "UISubmitTrigger", "UISelectedTrigger", "SelectableSfxPlayer",
        "UILocalizedString", "InputTagTextPreprocessor", "LocaleChangeReactor", "UIDelayEvent", "UIButton",
    };

    private sealed class Prompt
    {
        internal GameObject Root;
        internal TextMeshProUGUI Label, Tag;
        internal GameObject KeyFrame;
        internal string Shown;
    }

    private readonly RectTransform _group;
    private readonly Prompt[] _prompts;

    private MenuPrompts(RectTransform group, Prompt[] prompts)
    {
        _group = group;
        _prompts = prompts;
    }

    // False once the menu's scene was unloaded and took the copies with it.
    internal bool Alive => _group != null && Array.TrueForAll(_prompts, p => p.Root != null);

    // menu: the Arcanas or Armoury overlay. Null when its footer isn't what's expected (a game
    // update, say); the caller then shows its own plain line.
    internal static MenuPrompts Create(Transform menu, int count)
    {
        Transform footer = null;
        for (int i = 0; i < menu.childCount && footer == null; i++)
        {
            var child = menu.GetChild(i);
            if (child.name.StartsWith("Footnote_", StringComparison.Ordinal))
                footer = child;
        }
        var group = footer?.Find("FootnoteAnchor_Left")?.TryCast<RectTransform>();
        var template = footer?.Find("FootnoteAnchor_Right/FootnoteElement_Close");
        if (group == null || template == null || template.Find(LabelPath)?.GetComponent<TextMeshProUGUI>() == null ||
            template.Find(TagPath)?.GetComponent<TextMeshProUGUI>() == null)
        {
            Plugin.Logger.LogWarning(
                $"[prompt] {menu.name}: the game's footer prompt wasn't found (footer {footer != null}, left group " +
                $"{group != null}, Back prompt {template != null}); showing the plain hint line instead");
            return null;
        }

        // Copied into an inactive holder, so nothing on a copy wakes up before it's stripped.
        var holder = new GameObject("AbilityWheel_PromptHolder");
        holder.SetActive(false);
        holder.transform.SetParent(group, false);
        var prompts = new Prompt[count];
        try
        {
            for (int i = 0; i < count; i++)
            {
                var copy = Object.Instantiate(template.gameObject, holder.transform);
                copy.name = "FootnoteElement_AbilityWheel" + (i + 1);
                Strip(copy);
                prompts[i] = new Prompt
                {
                    Root = copy,
                    Label = copy.transform.Find(LabelPath).GetComponent<TextMeshProUGUI>(),
                    Tag = copy.transform.Find(TagPath).GetComponent<TextMeshProUGUI>(),
                    KeyFrame = copy.transform.Find(KeyFramePath)?.gameObject,
                };
                prompts[i].Tag.textPreprocessor = null;
                prompts[i].Tag.richText = true;
                // The Back prompt has its text first; the footer's left side has the button first.
                var row = copy.transform.Find("CommonUI_InputButton");
                var layout = row.GetComponent<HorizontalLayoutGroup>();
                if (layout != null)
                    layout.reverseArrangement = true;
                var canvasGroup = row.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.alpha = 1f;
                    canvasGroup.blocksRaycasts = false;
                    canvasGroup.interactable = false;
                }
                copy.SetActive(false);
            }
            foreach (var prompt in prompts)
            {
                prompt.Root.transform.SetParent(group, false);
                prompt.Root.transform.SetAsLastSibling();
            }
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"[prompt] {menu.name}: copying the game's footer prompt failed, showing the plain hint line instead: {e}");
            foreach (var prompt in prompts)
            {
                if (prompt?.Root != null)
                    Object.Destroy(prompt.Root);
            }
            return null;
        }
        finally
        {
            Object.Destroy(holder);
        }

        if (!group.gameObject.activeSelf)
            group.gameObject.SetActive(true);
        Plugin.Logger.LogInfo($"[prompt] {menu.name}: prompts added to the game's footer ({footer.name})");
        return new MenuPrompts(group, prompts);
    }

    private static void Strip(GameObject copy)
    {
        foreach (string unwanted in Unwanted)
        {
            foreach (var component in copy.GetComponentsInChildren<Component>(true))
            {
                if (component != null && component.GetIl2CppType().Name == unwanted)
                    Object.DestroyImmediate(component);
            }
        }
        // One that something else still needs can't be taken off; switched off, it does nothing either.
        foreach (var component in copy.GetComponentsInChildren<Component>(true))
        {
            if (component == null || Array.IndexOf(Unwanted, component.GetIl2CppType().Name) < 0)
                continue;
            var behaviour = component.TryCast<Behaviour>();
            if (behaviour != null)
                behaviour.enabled = false;
            Plugin.Logger.LogDebug($"[prompt] {component.GetIl2CppType().Name} couldn't be removed from the copy, switched off");
        }
    }

    // label null hides the prompt. input: sprite tags for gamepad buttons, or key names; keyFrame
    // puts the game's key cap behind it, as its own keyboard prompts have.
    internal void Set(int index, string label, string input, bool keyFrame)
    {
        var prompt = _prompts[index];
        if (label == null)
        {
            if (prompt.Root.activeSelf)
            {
                prompt.Root.SetActive(false);
                Rebuild();
            }
            return;
        }

        bool changed = false;
        string shown = label + "\n" + input + "\n" + keyFrame;
        if (shown != prompt.Shown)
        {
            prompt.Shown = shown;
            prompt.Label.text = label;
            prompt.Tag.text = $"<style=\"InputTag\">{input}</style>";
            if (prompt.KeyFrame != null)
                prompt.KeyFrame.SetActive(keyFrame);
            changed = true;
        }
        if (!prompt.Root.activeSelf)
        {
            prompt.Root.SetActive(true);
            changed = true;
        }
        if (changed)
            Rebuild();
    }

    private void Rebuild() => LayoutRebuilder.ForceRebuildLayoutImmediate(_group);
}
