using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// Key names in the config are Input System Key names, plus Mouse4 / Mouse5 for the mouse side
// buttons. Any key can be a slot key: the settings page binds one by pressing it (Belmont UI 0.4).
internal static class Keys
{
    // The lists Belmont UI 0.3 shows, which has no press-to-bind rows: keys the game doesn't use in
    // gameplay (it uses WASD, Space, E, R, Left Shift, X, C, Esc, Tab, the arrows and the main mouse
    // buttons).
    internal static readonly string[] SlotKeyChoices =
    {
        "Digit1", "Digit2", "Digit3", "Digit4", "Digit5", "Digit6", "Digit7", "Digit8", "Digit9", "Digit0",
        "Q", "F", "G", "T", "V", "Z", "B", "Mouse4", "Mouse5",
    };

    internal static readonly string[] AssignKeyChoices = { "F", "G", "T", "V", "Z", "B", "N", "M" };

    // What the Arcanas and Equipment menus use themselves, so the assign key can't be one of them:
    // its press would also switch tabs, equip or close the menu.
    internal static readonly string[] MenuKeys =
    {
        "Q", "E", "Digit1", "Digit3", "R", "Enter", "NumpadEnter", "Space", "Backspace", "Delete", "LeftAlt", "RightAlt",
        "Escape", "Tab", "W", "A", "S", "D", "UpArrow", "DownArrow", "LeftArrow", "RightArrow",
    };

    internal static bool KeyboardMode { get; private set; }

    internal static bool Known(string name) =>
        name == "Mouse4" || name == "Mouse5" || (Enum.TryParse(name, out Key key) && key != Key.None);

    internal static ButtonControl Control(string name)
    {
        if (name == "Mouse4")
            return Mouse.current?.backButton;
        if (name == "Mouse5")
            return Mouse.current?.forwardButton;
        var keyboard = Keyboard.current;
        if (keyboard == null || !Enum.TryParse(name, out Key key) || key == Key.None)
            return null;
        return keyboard[key];
    }

    internal static bool Pressed(string name) => Control(name)?.wasPressedThisFrame ?? false;

    internal static bool Held(string name) => Control(name)?.isPressed ?? false;

    // Short enough for a wheel tile: "1", "F5", "LShift", "Num1", "M4".
    internal static string Label(string name) => name switch
    {
        "Mouse4" => "M4",
        "Mouse5" => "M5",
        "LeftShift" => "LShift",
        "RightShift" => "RShift",
        "LeftCtrl" => "LCtrl",
        "RightCtrl" => "RCtrl",
        "LeftAlt" => "LAlt",
        "RightAlt" => "RAlt",
        "UpArrow" => "Up",
        "DownArrow" => "Down",
        "LeftArrow" => "Left",
        "RightArrow" => "Right",
        "Backquote" => "`",
        "Minus" => "-",
        "Equals" => "=",
        "LeftBracket" => "[",
        "RightBracket" => "]",
        "Semicolon" => ";",
        "Quote" => "'",
        "Comma" => ",",
        "Period" => ".",
        "Slash" => "/",
        "Backslash" => "\\",
        "CapsLock" => "Caps",
        "PageUp" => "PgUp",
        "PageDown" => "PgDn",
        "Insert" => "Ins",
        "Delete" => "Del",
        "NumpadEnter" => "NumEnt",
        "NumpadPlus" => "Num+",
        "NumpadMinus" => "Num-",
        "NumpadMultiply" => "Num*",
        "NumpadDivide" => "Num/",
        "NumpadPeriod" => "Num.",
        "NumpadEquals" => "Num=",
        _ when name.StartsWith("Digit", StringComparison.Ordinal) => name.Substring(5),
        _ when name.StartsWith("Numpad", StringComparison.Ordinal) => "Num" + name.Substring(6),
        _ => name,
    };

    internal static void TrackDevice()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if ((keyboard != null && keyboard.anyKey.wasPressedThisFrame) ||
            (mouse != null && (mouse.leftButton.wasPressedThisFrame || mouse.rightButton.wasPressedThisFrame)))
        {
            KeyboardMode = true;
            return;
        }

        var pad = Gamepad.current;
        if (pad != null && GamepadUsed(pad))
            KeyboardMode = false;
    }

    private static bool GamepadUsed(Gamepad pad) =>
        pad.buttonSouth.wasPressedThisFrame || pad.buttonEast.wasPressedThisFrame ||
        pad.buttonWest.wasPressedThisFrame || pad.buttonNorth.wasPressedThisFrame ||
        pad.leftShoulder.wasPressedThisFrame || pad.rightShoulder.wasPressedThisFrame ||
        pad.leftTrigger.wasPressedThisFrame || pad.rightTrigger.wasPressedThisFrame ||
        pad.startButton.wasPressedThisFrame || pad.selectButton.wasPressedThisFrame ||
        pad.dpad.up.wasPressedThisFrame || pad.dpad.down.wasPressedThisFrame ||
        pad.dpad.left.wasPressedThisFrame || pad.dpad.right.wasPressedThisFrame ||
        pad.leftStick.up.wasPressedThisFrame || pad.leftStick.down.wasPressedThisFrame ||
        pad.leftStick.left.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame ||
        pad.rightStick.up.wasPressedThisFrame || pad.rightStick.down.wasPressedThisFrame ||
        pad.rightStick.left.wasPressedThisFrame || pad.rightStick.right.wasPressedThisFrame;
}
