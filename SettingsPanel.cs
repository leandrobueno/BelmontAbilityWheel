using System.Collections.Generic;
using System.Linq;
using Il2CppInterop.Runtime.Attributes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace AbilityWheel;

// The settings popup in the Tarot menu, so the settings don't need Belmont Minimap's MODS tab.
internal sealed class SettingsPanel
{
    private const float Width = 640f;
    private const float Pad = 24f;
    private const float RowTop = 76f;
    private const float RowHeight = 36f;
    private const float HintHeight = 96f;

    private static readonly Color PanelColor = new(0.05f, 0.04f, 0.07f, 0.96f);
    private static readonly Color BorderColor = new(0.62f, 0.48f, 0.24f, 1f);
    private static readonly Color SelectedColor = new(0.75f, 0.58f, 0.28f, 0.22f);
    private static readonly Color SelectedText = new(1f, 0.85f, 0.5f, 1f);
    private static readonly Color HintText = new(0.78f, 0.74f, 0.66f, 1f);

    private sealed class Line
    {
        internal Image Highlight;
        internal TextMeshProUGUI Label;
        internal TextMeshProUGUI Value;
        internal string Shown;
    }

    private readonly GameObject _canvas;
    private readonly List<SettingRow> _rows;
    private readonly List<Line> _lines = new();
    private readonly TextMeshProUGUI _key;
    private readonly TextMeshProUGUI _hint;
    private readonly TextMeshProUGUI _footer;
    private int _selected;
    private int _shownSelected = -1;
    private bool? _shownKeyboard;
    private string _status = "";

    internal bool Alive => _canvas != null;

    // The last line, after the settings, restores the defaults.
    private int LineCount => _rows.Count + 1;

    internal SettingsPanel(List<SettingRow> rows)
    {
        _rows = rows;
        _canvas = Ui.NewCanvas("AbilityWheelSettings", 20001);   // above the HUD
        var font = Ui.FindFont();

        float height = RowTop + LineCount * RowHeight + 12f + HintHeight + 12f + 28f + Pad;
        var panel = Ui.NewRect("Panel", _canvas.transform, Vector2.zero, Vector2.zero);
        panel.anchorMin = panel.anchorMax = panel.pivot = new Vector2(0f, 0.5f);
        panel.anchoredPosition = new Vector2(60f, 0f);
        panel.sizeDelta = new Vector2(Width, height);

        Stretch(Ui.NewImage("Background", panel, Vector2.zero, Vector2.zero, PanelColor).rectTransform);
        Place(Ui.NewImage("BorderTop", panel, Vector2.zero, Vector2.zero, BorderColor), 0f, 0f, Width, 2f);
        Place(Ui.NewImage("BorderBottom", panel, Vector2.zero, Vector2.zero, BorderColor), 0f, height - 2f, Width, 2f);
        Place(Ui.NewImage("BorderLeft", panel, Vector2.zero, Vector2.zero, BorderColor), 0f, 0f, 2f, height);
        Place(Ui.NewImage("BorderRight", panel, Vector2.zero, Vector2.zero, BorderColor), Width - 2f, 0f, 2f, height);

        var title = Text("Title", panel, font, 30f, Ui.TextColor, TextAlignmentOptions.MidlineLeft, Pad, Pad - 4f, 400f, 40f);
        title.text = "Ability Wheel";
        _key = Text("Key", panel, font, 20f, HintText, TextAlignmentOptions.MidlineRight, Width - Pad - 120f, Pad - 4f, 120f, 40f);

        for (int i = 0; i < LineCount; i++)
        {
            float y = RowTop + i * RowHeight;
            var line = new Line
            {
                Highlight = Ui.NewImage("Selected", panel, Vector2.zero, Vector2.zero, SelectedColor),
                Label = Text("Label", panel, font, 22f, Ui.TextColor, TextAlignmentOptions.MidlineLeft, Pad, y, 380f, RowHeight),
                Value = Text("Value", panel, font, 22f, Ui.TextColor, TextAlignmentOptions.MidlineRight, Width - Pad - 230f, y, 230f, RowHeight),
            };
            Place(line.Highlight, 10f, y + 2f, Width - 20f, RowHeight - 4f);
            line.Label.text = i < _rows.Count ? _rows[i].LabelEn : "Restore defaults";
            _lines.Add(line);
        }

        float hintTop = RowTop + LineCount * RowHeight + 12f;
        _hint = Text("Hint", panel, font, 19f, HintText, TextAlignmentOptions.TopLeft, Pad, hintTop, Width - 2f * Pad, HintHeight);
        _hint.enableWordWrapping = true;
        _footer = Text("Footer", panel, font, 18f, HintText, TextAlignmentOptions.MidlineLeft, Pad, height - Pad - 28f, Width - 2f * Pad, 28f);

        _canvas.SetActive(false);
    }

    internal void Show(bool visible)
    {
        _status = "";
        _shownSelected = -1;
        _canvas.SetActive(visible);
    }

    internal void Move(int direction)
    {
        _selected = (_selected + direction + LineCount) % LineCount;
        _status = "";
    }

    internal void Change(int direction)
    {
        if (_selected < _rows.Count)
            _rows[_selected].Change(direction);
    }

    internal void Activate()
    {
        if (_selected < _rows.Count)
        {
            _rows[_selected].Change(1);
            return;
        }
        SettingRows.ResetAll();
        _status = "All settings are back to their defaults.";
        _shownSelected = -1;
        Plugin.Logger.LogInfo("[settings] defaults restored");
    }

    // Values can change from elsewhere too (Belmont Minimap's page), so they're re-read every time.
    internal void Refresh(bool keyboard)
    {
        for (int i = 0; i < LineCount; i++)
        {
            var line = _lines[i];
            bool selected = i == _selected;
            string value = i < _rows.Count ? _rows[i].ValueText() : "";
            string shown = selected && i < _rows.Count ? $"<  {value}  >" : value;
            if (shown != line.Shown)
            {
                line.Shown = shown;
                line.Value.text = shown;
            }
        }

        if (_selected != _shownSelected)
        {
            for (int i = 0; i < LineCount; i++)
            {
                bool selected = i == _selected;
                _lines[i].Highlight.enabled = selected;
                _lines[i].Label.color = selected ? SelectedText : Ui.TextColor;
                _lines[i].Value.color = selected ? SelectedText : Ui.TextColor;
            }
            _shownSelected = _selected;
            _shownKeyboard = null;
        }

        if (keyboard != _shownKeyboard || _status.Length > 0)
        {
            _shownKeyboard = keyboard;
            _key.text = keyboard ? Keys.Label(Plugin.SettingsKey.Value) : "R3";
            _footer.text = keyboard
                ? "Choose  Up / Down      Change  Left / Right      Close  Esc"
                : "Choose  Up / Down      Change  Left / Right      Close  B";
            _hint.text = _status.Length > 0 ? _status
                : _selected < _rows.Count ? _rows[_selected].HintEn
                : keyboard ? "Press Enter to put every setting back to its default."
                : "Press A to put every setting back to its default.";
        }
    }

    private static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font, float size, Color color,
        TextAlignmentOptions alignment, float x, float y, float width, float height)
    {
        var text = Ui.NewText(name, parent, Vector2.zero, Vector2.zero, font, size, color);
        text.alignment = alignment;
        text.overflowMode = TextOverflowModes.Ellipsis;
        Place(text, x, y, width, height);
        return text;
    }

    // Position from the panel's top-left corner, like a page layout.
    private static void Place(Graphic graphic, float x, float y, float width, float height)
    {
        var rect = graphic.rectTransform;
        rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.sizeDelta = Vector2.zero;
    }
}

// Opening, input and closing of the settings popup (F3 / R3 in the Tarot menu). It reads the
// controller and keyboard itself, so the menu's own controls are switched off while it's open,
// and stay off until the button that closed it is released (B would also close the menu).
public partial class Wheel
{
    private const float RepeatDelay = 0.4f;
    private const float RepeatInterval = 0.08f;

    // A property: static fields in different partial files have no fixed initialization order.
    private static (string Map, string Action)[] BlockedWhileSettingsOpen =>
        BlockedWhileAssigning.Concat(new[]
        {
            ("UI", "Navigate"), ("UI", "NavigateUp"), ("UI", "NavigateDown"), ("UI", "NavigateLeft"),
            ("UI", "NavigateRight"), ("UI", "OpenMap"), ("Menu", "OpenMap"),
        }).ToArray();

    private SettingsPanel _settings;
    private bool _settingsOpen;
    private bool _settingsWaitRelease;
    private int _verticalDir;
    private float _verticalNext;
    private int _horizontalDir;
    private float _horizontalNext;

    // True while the popup has the input.
    [HideFromIl2Cpp]
    private bool UpdateSettings()
    {
        var pad = Gamepad.current;
        var keyboard = Keyboard.current;
        if (_settingsWaitRelease)
        {
            if (!AnyMenuInputHeld(pad, keyboard))
            {
                _settingsWaitRelease = false;
                UnblockUi();
            }
            return false;
        }

        if (!_settingsOpen)
        {
            if (SpellsMenuOpen && !_assigning && !_waitFaceRelease &&
                ((pad != null && pad.rightStickButton.wasPressedThisFrame) || Keys.Pressed(Plugin.SettingsKey.Value)))
                OpenSettings();
            return _settingsOpen;
        }

        if (!SpellsMenuOpen)
        {
            CloseSettings(waitRelease: false);
            return false;
        }

        bool close = (pad != null && (pad.buttonEast.wasPressedThisFrame || pad.rightStickButton.wasPressedThisFrame)) ||
            (keyboard != null && (keyboard.escapeKey.wasPressedThisFrame || keyboard.backspaceKey.wasPressedThisFrame)) ||
            Keys.Pressed(Plugin.SettingsKey.Value);
        if (close)
        {
            CloseSettings(waitRelease: true);
            return true;
        }

        int move = Repeat(ref _verticalDir, ref _verticalNext,
            Held(pad?.dpad.up, pad?.leftStick.up, keyboard?.upArrowKey, keyboard?.wKey),
            Held(pad?.dpad.down, pad?.leftStick.down, keyboard?.downArrowKey, keyboard?.sKey));
        int change = Repeat(ref _horizontalDir, ref _horizontalNext,
            Held(pad?.dpad.left, pad?.leftStick.left, keyboard?.leftArrowKey, keyboard?.aKey),
            Held(pad?.dpad.right, pad?.leftStick.right, keyboard?.rightArrowKey, keyboard?.dKey));
        bool activate = (pad != null && pad.buttonSouth.wasPressedThisFrame) ||
            (keyboard != null && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame ||
                                  keyboard.spaceKey.wasPressedThisFrame));

        if (move != 0)
            _settings.Move(move);
        if (change != 0)
            _settings.Change(change);
        if (activate)
            _settings.Activate();
        _settings.Refresh(Keys.KeyboardMode);
        return true;
    }

    [HideFromIl2Cpp]
    private void OpenSettings()
    {
        BlockUi(BlockedWhileSettingsOpen);
        if (_settings == null || !_settings.Alive)
            _settings = new SettingsPanel(SettingRows.All());
        _settings.Show(true);
        _settings.Refresh(Keys.KeyboardMode);
        _settingsOpen = true;
        _verticalDir = _horizontalDir = 0;
        Plugin.Logger.LogInfo("[settings] opened");
    }

    [HideFromIl2Cpp]
    private void CloseSettings(bool waitRelease)
    {
        if (!_settingsOpen)
            return;
        if (_settings != null && _settings.Alive)
            _settings.Show(false);
        _settingsOpen = false;
        if (waitRelease)
            _settingsWaitRelease = true;
        else
            UnblockUi();
        Plugin.Logger.LogInfo("[settings] closed");
    }

    // A press moves once; holding repeats after a moment, for stepping through values quickly.
    [HideFromIl2Cpp]
    private static int Repeat(ref int heldDir, ref float next, bool negative, bool positive)
    {
        int dir = negative == positive ? 0 : negative ? -1 : 1;
        float now = Time.unscaledTime;
        if (dir == 0)
        {
            heldDir = 0;
            return 0;
        }
        if (dir != heldDir)
        {
            heldDir = dir;
            next = now + RepeatDelay;
            return dir;
        }
        if (now < next)
            return 0;
        next = now + RepeatInterval;
        return dir;
    }

    [HideFromIl2Cpp]
    private static bool Held(params UnityEngine.InputSystem.Controls.ButtonControl[] controls) =>
        controls.Any(c => c != null && c.isPressed);

    [HideFromIl2Cpp]
    private static bool AnyMenuInputHeld(Gamepad pad, Keyboard keyboard) =>
        (keyboard != null && keyboard.anyKey.isPressed) ||
        (pad != null && Held(pad.buttonSouth, pad.buttonEast, pad.buttonWest, pad.buttonNorth, pad.leftStickButton,
            pad.rightStickButton, pad.dpad.up, pad.dpad.down, pad.dpad.left, pad.dpad.right));
}
