using System;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// Key names in the config are Input System Key names, plus Mouse4 / Mouse5 for the mouse side
// buttons.
internal static class Keys
{
    // Only keys the game doesn't use in gameplay (it uses WASD, Space, E, R, Left Shift, X, C,
    // Esc, Tab, the arrows and the main mouse buttons).
    internal static readonly string[] SlotKeyChoices =
    {
        "Digit1", "Digit2", "Digit3", "Digit4", "Digit5", "Digit6", "Digit7", "Digit8", "Digit9", "Digit0",
        "Q", "F", "G", "T", "V", "Z", "B", "Mouse4", "Mouse5",
    };

    // Keys the Arcanas menu doesn't use (it uses Q/E, 1/3, Enter, Space, Backspace, Delete, R, Alt).
    internal static readonly string[] AssignKeyChoices = { "F", "G", "T", "V", "Z", "B", "N", "M" };

    internal static bool KeyboardMode { get; private set; }

    internal static ButtonControl Control(string name)
    {
        if (name == "Mouse4")
            return Mouse.current?.backButton;
        if (name == "Mouse5")
            return Mouse.current?.forwardButton;
        var keyboard = Keyboard.current;
        if (keyboard == null || !Enum.TryParse(name, out Key key))
            return null;
        return keyboard[key];
    }

    internal static bool Pressed(string name) => Control(name)?.wasPressedThisFrame ?? false;

    internal static bool Held(string name) => Control(name)?.isPressed ?? false;

    internal static string Label(string name) =>
        name.StartsWith("Digit", StringComparison.Ordinal) ? name.Substring(5)
        : name == "Mouse4" ? "M4"
        : name == "Mouse5" ? "M5"
        : name;

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
        pad.leftStick.left.wasPressedThisFrame || pad.leftStick.right.wasPressedThisFrame;
}
