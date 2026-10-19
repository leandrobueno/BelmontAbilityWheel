using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using RedWine.ResourceManagement;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;
using GameResources = RedWine.ResourceManagement.ResourceManager;

namespace AbilityWheel;

internal sealed class Hud
{
    private const float Tile = 96f;
    private const float Gap = 8f;

    private const float Border = 3f;

    private static readonly Color TileColor = new(0.07f, 0.05f, 0.09f, 1f);
    private static readonly Color FrameColor = new(0.02f, 0.01f, 0.03f, 1f);
    private static readonly Color GoldColor = new(0.78f, 0.6f, 0.3f, 1f);      // the Y card
    private static readonly Color ActiveColor = new(0.4f, 0.65f, 0.95f, 1f);   // a StayEquipped card that is out
    private static readonly Color GrayedIcon = new(0.25f, 0.25f, 0.25f, 1f);
    private static readonly Color HintColor = new(0.93f, 0.87f, 0.74f, 1f);
    private static readonly Color[] LetterColors =   // Y, X, B, A as on an Xbox pad
    {
        new(1f, 0.8f, 0.2f), new(0.35f, 0.6f, 1f), new(1f, 0.35f, 0.35f), new(0.45f, 0.85f, 0.35f),
    };

    private sealed class Slot
    {
        internal Image[] Frame;
        internal Image Icon;
        internal TextMeshProUGUI Letter;
        internal TextMeshProUGUI Name;
        internal TextMeshProUGUI Cost;
        internal SpellItemData Card;
        internal int State = -1;           // to touch the UI only on changes
    }

    private readonly GameObject _canvas;
    private readonly RectTransform _panel;
    private readonly CanvasGroup _group;
    private readonly Slot[] _slots = new Slot[4];
    private readonly TextMeshProUGUI _hint;
    private string _hintText = "";
    private readonly Dictionary<string, Sprite> _icons = new();
    private readonly HashSet<string> _iconFailed = new();
    private bool _visible = true;
    private (float, float, float, float) _layout = (-1f, 0f, 0f, 0f);
    private string _letters = "";

    // False once Unity has destroyed the canvas, so Wheel builds a new one.
    internal bool Alive => _canvas != null;

    internal Hud()
    {
        _canvas = new GameObject("AbilityWheelHud");
        UnityEngine.Object.DontDestroyOnLoad(_canvas);
        var canvas = _canvas.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 20000;   // above the game's menus, for the Spells menu
        var scaler = _canvas.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 1f;

        _panel = NewRect("Panel", _canvas.transform, Vector2.zero, Vector2.zero);
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(1f, 0f);
        _group = _panel.gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        _group.interactable = false;

        // Centers of the 4 diamonds: touching edge to edge, apart by Gap.
        float d = (Tile + Gap) / Mathf.Sqrt(2f);
        float half = d + Tile / Mathf.Sqrt(2f);   // from the panel center to a diamond's outer tip
        _panel.sizeDelta = new Vector2(2f * half, 2f * half);
        Vector2[] centers = { new(0f, d), new(-d, 0f), new(d, 0f), new(0f, -d) };

        var font = FindFont();
        for (int slot = 0; slot < _slots.Length; slot++)
            _slots[slot] = NewSlot(slot, centers[slot], font);

        // In the Spells menu's bottom bar, left of the game's own "Back" prompt.
        _hint = NewText("Hint", _panel, new Vector2(-330f, -half - 24f), new Vector2(560f, 34f), font, 26f, HintColor);
        _hint.alignment = TextAlignmentOptions.Right;
        _hint.gameObject.SetActive(false);

        Plugin.Logger.LogInfo($"[hud] created, font {(font != null ? font.name : "none (TextMeshPro default)")}");
    }

    internal void SetHint(string text)
    {
        if (text == _hintText)
            return;
        _hintText = text;
        _hint.text = text;
        _hint.gameObject.SetActive(text.Length > 0);
    }

    // null: the gamepad letters.
    internal void SetLetters(string[] keys)
    {
        string joined = keys == null ? "" : string.Join("|", keys);
        if (joined == _letters)
            return;
        _letters = joined;
        for (int slot = 0; slot < _slots.Length; slot++)
        {
            _slots[slot].Letter.text = keys == null ? Wheel.SlotLabels[slot] : keys[slot];
            _slots[slot].Letter.color = keys == null ? LetterColors[slot] : HintColor;
        }
    }

    internal void SetVisible(bool visible)
    {
        if (visible == _visible)
            return;
        _visible = visible;
        _canvas.SetActive(visible);
    }

    internal void Refresh(SpellItemData[] cards, int[] costs, float mana, SpellItemData gold, SpellItemData active, bool held)
    {
        // Set only on changes: touching the layout every frame makes Unity rebuild the UI.
        var layout = (Plugin.HudScale.Value * (held ? 1.08f : 1f), Plugin.HudOffsetX.Value, Plugin.HudOffsetY.Value,
            Plugin.HudOpacity.Value);
        if (layout != _layout)
        {
            _layout = layout;
            _panel.localScale = new Vector3(layout.Item1, layout.Item1, 1f);
            _panel.anchoredPosition = new Vector2(-layout.Item2, layout.Item3);
            _group.alpha = layout.Item4;
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            var slot = _slots[i];
            var card = cards[i];
            if (!Cards.Same(card, slot.Card) && !(card == null && slot.Card == null))
            {
                slot.Card = card;
                slot.State = -1;
                var icon = card == null ? null : IconFor(card);
                slot.Icon.sprite = icon;
                slot.Icon.enabled = icon != null;
                slot.Name.text = card == null ? "-" : icon == null ? Cards.DisplayName(card) : "";
            }

            bool affordable = card != null && costs[i] <= mana;
            bool isGold = Cards.Same(card, gold);
            bool isActive = Cards.Same(card, active);
            int state = (affordable ? 1 : 0) | (isGold ? 2 : 0) | (held ? 4 : 0) | (isActive ? 8 : 0);
            if (state == slot.State && (!held || card == null))
                continue;

            if (state != slot.State)
            {
                slot.State = state;
                slot.Icon.color = affordable ? Color.white : GrayedIcon;
                slot.Name.color = affordable ? Color.white : GrayedIcon;
                var frame = isActive ? ActiveColor : isGold ? GoldColor : FrameColor;
                foreach (var bar in slot.Frame)
                    bar.color = frame;
                slot.Cost.gameObject.SetActive(held && card != null);
            }
            if (held && card != null)
            {
                string cost = costs[i].ToString();
                if (slot.Cost.text != cost)
                    slot.Cost.text = cost;
                slot.Cost.color = affordable ? Color.white : new Color(1f, 0.45f, 0.45f);
            }
        }
    }

    // Loaded through the game's own loader into its "Always" context, which keeps it for the
    // whole session.
    private Sprite IconFor(SpellItemData card)
    {
        string id = card.itemID ?? card.name;
        if (_icons.TryGetValue(id, out var cached) && cached != null)
            return cached;
        if (_iconFailed.Contains(id))
            return null;

        foreach (var (what, reference) in new (string, AssetReferenceT<Sprite>)[]
                 { ("atlased", card.itemDisplayIconAtlased), ("plain", card.itemDisplayIconRef) })
        {
            try
            {
                if (reference == null || !reference.RuntimeKeyIsValid())
                    continue;
                var sprite = GameResources.LoadAssetByRef<Sprite>(ResourceContext.Always, reference);
                if (sprite != null)
                {
                    _icons[id] = sprite;
                    Plugin.Logger.LogInfo($"[hud] icon for {id}: {sprite.name} ({what})");
                    return sprite;
                }
            }
            catch (Exception e)
            {
                Plugin.Logger.LogWarning($"[hud] loading the {what} icon of {id} failed: {e.Message}");
            }
        }

        _iconFailed.Add(id);
        Plugin.Logger.LogWarning($"[hud] no icon for {id}, showing its name");
        return null;
    }

    private Slot NewSlot(int index, Vector2 center, TMP_FontAsset font)
    {
        var root = NewRect(Wheel.SlotLabels[index], _panel, center, new Vector2(Tile, Tile));
        var slot = new Slot();

        // The frame is 4 bars outside the tile, never under it: with HudOpacity below 1 a frame
        // under the see-through tile shows through (badly with HDR mods like RenoDX). The top
        // and bottom bars also cover the corners.
        var diamond = NewRect("Diamond", root, Vector2.zero, new Vector2(Tile, Tile));
        diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
        NewImage("Tile", diamond, Vector2.zero, new Vector2(Tile, Tile), TileColor);
        float edge = (Tile + Border) / 2f;
        slot.Frame = new[]
        {
            NewImage("FrameTop", diamond, new Vector2(0f, edge), new Vector2(Tile + 2f * Border, Border), FrameColor),
            NewImage("FrameBottom", diamond, new Vector2(0f, -edge), new Vector2(Tile + 2f * Border, Border), FrameColor),
            NewImage("FrameLeft", diamond, new Vector2(-edge, 0f), new Vector2(Border, Tile), FrameColor),
            NewImage("FrameRight", diamond, new Vector2(edge, 0f), new Vector2(Border, Tile), FrameColor),
        };

        slot.Icon = NewImage("Icon", root, Vector2.zero, new Vector2(Tile * 0.7f, Tile * 0.7f), Color.white);
        slot.Icon.preserveAspect = true;
        slot.Icon.enabled = false;

        slot.Name = NewText("Name", root, Vector2.zero, new Vector2(Tile * 0.7f, Tile * 0.7f), font, 15f, Color.white);
        slot.Name.enableWordWrapping = true;

        float tip = Tile / Mathf.Sqrt(2f) - 12f;
        slot.Letter = NewText("Letter", root, new Vector2(0f, -tip), new Vector2(40f, 30f), font, 24f, LetterColors[index]);
        slot.Letter.text = Wheel.SlotLabels[index];
        slot.Letter.fontStyle = FontStyles.Bold;
        slot.Cost = NewText("Cost", root, new Vector2(0f, tip), new Vector2(60f, 26f), font, 20f, Color.white);
        slot.Cost.gameObject.SetActive(false);
        return slot;
    }

    private static RectTransform NewRect(string name, Transform parent, Vector2 position, Vector2 size)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        var rect = go.AddComponent<RectTransform>();
        rect.anchoredPosition = position;
        rect.sizeDelta = size;
        return rect;
    }

    private static Image NewImage(string name, Transform parent, Vector2 position, Vector2 size, Color color)
    {
        var image = NewRect(name, parent, position, size).gameObject.AddComponent<Image>();
        image.color = color;
        image.raycastTarget = false;
        return image;
    }

    private static TextMeshProUGUI NewText(string name, Transform parent, Vector2 position, Vector2 size,
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
    private static TMP_FontAsset FindFont()
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
