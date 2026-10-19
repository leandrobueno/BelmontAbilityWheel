using Il2CppInterop.Runtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace AbilityWheel;

// Building blocks for the mod's own UI (the HUD and the settings panel).
internal static class Ui
{
    internal static readonly Color TextColor = new(0.93f, 0.87f, 0.74f, 1f);   // the game's menu text

    // Sized for 1080p and scaled with the screen, like the game's own UI.
    internal static GameObject NewCanvas(string name, int sortingOrder)
    {
        var go = new GameObject(name);
        Object.DontDestroyOnLoad(go);
        var canvas = go.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = sortingOrder;
        var scaler = go.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;
        return go;
    }

    internal static RectTransform NewRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    internal static Image NewImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var image = NewRect(name, parent, position, size).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    internal static TextMeshProUGUI NewText(string name, Transform parent, Vector2 position, Vector2 size,
        TMP_FontAsset font, float fontSize, Color color)
    {
        var text = NewRect(name, parent, position, size).gameObject.AddComponent<TextMeshProUGUI>();
        if (font != null)
            text.font = font;
        text.fontSize = fontSize;
        text.color = color;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = false;
        text.raycastTarget = false;
        // No outline: it erodes the thin strokes of the game's BaseFont.
        return text;
    }

    // BaseFont is the game's main UI font.
    internal static TMP_FontAsset FindFont()
    {
        TMP_FontAsset any = null;
        foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<TMP_FontAsset>()))
        {
            var font = obj?.TryCast<TMP_FontAsset>();
            if (font == null)
                continue;
            if (font.name == "BaseFont")
                return font;
            any ??= font;
        }
        return any;
    }
}
