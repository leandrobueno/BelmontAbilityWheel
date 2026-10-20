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

    private const float FadeOutSeconds = 0.3f;   // from solid to hidden, when the game HUD fades

    private static readonly Color TileColor = new(0.07f, 0.05f, 0.09f, 1f);
    private static readonly Color FrameColor = new(0.02f, 0.01f, 0.03f, 1f);
    private static readonly Color GoldColor = new(0.78f, 0.6f, 0.3f, 1f);      // the Y card
    private static readonly Color ActiveColor = new(0.4f, 0.65f, 0.95f, 1f);   // a StayEquipped card that is out
    private static readonly Color GrayedIcon = new(0.25f, 0.25f, 0.25f, 1f);
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
        internal ItemData Card;           // a spell card or a weapon
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
    private (float, float, float) _layout = (-1f, 0f, 0f);
    private float _fade = 1f;
    private float _alpha = -1f;
    private string _letters = "";

    // False once Unity has destroyed the canvas, so Wheel builds a new one.
    internal bool Alive => _canvas != null;

    internal Hud()
    {
        _canvas = Ui.NewCanvas("AbilityWheelHud", 20000);   // above the game's menus, for the Tarot menu

        _panel = Ui.NewRect("Panel", _canvas.transform, Vector2.zero, Vector2.zero);
        _panel.anchorMin = _panel.anchorMax = _panel.pivot = new Vector2(1f, 0f);
        _group = _panel.gameObject.AddComponent<CanvasGroup>();
        _group.blocksRaycasts = false;
        _group.interactable = false;

        // Centers of the 4 diamonds: touching edge to edge, apart by Gap.
        float d = (Tile + Gap) / Mathf.Sqrt(2f);
        float half = d + Tile / Mathf.Sqrt(2f);   // from the panel center to a diamond's outer tip
        _panel.sizeDelta = new Vector2(2f * half, 2f * half);
        Vector2[] centers = { new(0f, d), new(-d, 0f), new(d, 0f), new(0f, -d) };

        var font = Ui.FindFont();
        for (int slot = 0; slot < _slots.Length; slot++)
            _slots[slot] = NewSlot(slot, centers[slot], font);

        // In the Spells menu's bottom bar, left of the game's own "Back" prompt.
        _hint = Ui.NewText("Hint", _panel, new Vector2(-430f, -half - 24f), new Vector2(760f, 34f), font, 26f, Ui.TextColor);
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
            _slots[slot].Letter.color = keys == null ? LetterColors[slot] : Ui.TextColor;
        }
    }

    internal void SetVisible(bool visible)
    {
        if (visible == _visible)
            return;
        _visible = visible;
        _canvas.SetActive(visible);
    }

    // fade: how visible the game's HUD is (1 = fully). The tiles follow it down over at least
    // FadeOutSeconds, so letting go of the modifier doesn't make them vanish at once, and come
    // back at once. costs is null for weapons, which cost nothing.
    internal void Refresh(ItemData[] cards, int[] costs, float mana, ItemData gold, ItemData active, bool held,
        float fade)
    {
        // Set only on changes: touching the layout every frame makes Unity rebuild the UI.
        var layout = (Plugin.HudScale.Value * (held ? 1.08f : 1f), Plugin.HudOffsetX.Value, Plugin.HudOffsetY.Value);
        if (layout != _layout)
        {
            _layout = layout;
            _panel.localScale = new Vector3(layout.Item1, layout.Item1, 1f);
            _panel.anchoredPosition = new Vector2(-layout.Item2, layout.Item3);
        }

        fade = Mathf.Clamp01(fade);
        _fade = fade >= _fade ? fade : Mathf.Max(fade, _fade - Time.unscaledDeltaTime / FadeOutSeconds);
        float alpha = Plugin.HudOpacity.Value * _fade;
        if (alpha != _alpha)
        {
            _alpha = alpha;
            _group.alpha = alpha;
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

            bool hasCost = costs != null;
            bool affordable = card != null && (!hasCost || costs[i] <= mana);
            bool isGold = Cards.Same(card, gold);
            bool isActive = Cards.Same(card, active);
            bool showCost = held && hasCost && card != null;
            int state = (affordable ? 1 : 0) | (isGold ? 2 : 0) | (showCost ? 4 : 0) | (isActive ? 8 : 0);
            if (state == slot.State && !showCost)
                continue;

            if (state != slot.State)
            {
                slot.State = state;
                slot.Icon.color = affordable ? Color.white : GrayedIcon;
                slot.Name.color = affordable ? Color.white : GrayedIcon;
                var frame = isActive ? ActiveColor : isGold ? GoldColor : FrameColor;
                foreach (var bar in slot.Frame)
                    bar.color = frame;
                slot.Cost.gameObject.SetActive(showCost);
            }
            if (showCost)
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
    private Sprite IconFor(ItemData card)
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
        var root = Ui.NewRect(Wheel.SlotLabels[index], _panel, center, new Vector2(Tile, Tile));
        var slot = new Slot();

        // The frame is 4 bars outside the tile, never under it: with HudOpacity below 1 a frame
        // under the see-through tile shows through (badly with HDR mods like RenoDX). The top
        // and bottom bars also cover the corners.
        var diamond = Ui.NewRect("Diamond", root, Vector2.zero, new Vector2(Tile, Tile));
        diamond.localRotation = Quaternion.Euler(0f, 0f, 45f);
        Ui.NewImage("Tile", diamond, Vector2.zero, new Vector2(Tile, Tile), TileColor);
        float edge = (Tile + Border) / 2f;
        slot.Frame = new[]
        {
            Ui.NewImage("FrameTop", diamond, new Vector2(0f, edge), new Vector2(Tile + 2f * Border, Border), FrameColor),
            Ui.NewImage("FrameBottom", diamond, new Vector2(0f, -edge), new Vector2(Tile + 2f * Border, Border), FrameColor),
            Ui.NewImage("FrameLeft", diamond, new Vector2(-edge, 0f), new Vector2(Border, Tile), FrameColor),
            Ui.NewImage("FrameRight", diamond, new Vector2(edge, 0f), new Vector2(Border, Tile), FrameColor),
        };

        slot.Icon = Ui.NewImage("Icon", root, Vector2.zero, new Vector2(Tile * 0.7f, Tile * 0.7f), Color.white);
        slot.Icon.preserveAspect = true;
        slot.Icon.enabled = false;

        slot.Name = Ui.NewText("Name", root, Vector2.zero, new Vector2(Tile * 0.7f, Tile * 0.7f), font, 15f, Color.white);
        slot.Name.enableWordWrapping = true;

        float tip = Tile / Mathf.Sqrt(2f) - 12f;
        slot.Letter = Ui.NewText("Letter", root, new Vector2(0f, -tip), new Vector2(40f, 30f), font, 24f, LetterColors[index]);
        slot.Letter.text = Wheel.SlotLabels[index];
        slot.Letter.fontStyle = FontStyles.Bold;
        slot.Cost = Ui.NewText("Cost", root, new Vector2(0f, tip), new Vector2(60f, 26f), font, 20f, Color.white);
        slot.Cost.gameObject.SetActive(false);
        return slot;
    }
}
