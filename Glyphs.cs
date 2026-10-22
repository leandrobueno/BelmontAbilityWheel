using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.Switch;
using UnityEngine.InputSystem.XInput;
using Object = UnityEngine.Object;

namespace AbilityWheel;

// The game's own controller icons, chosen the way its button prompts choose them
// (InputTagTextPreprocessor.GetPrefix): the gamepad it last saw used, DualSense as "ps5_",
// DualShock as "ps4_", Switch Pro as "switch_", anything else as "xbox_" (a PlayStation pad
// through Steam Input shows up as XInput). The art is in TMP's default sprite asset, which falls
// back to every glyph set, so a <sprite> tag of prefix + control path shows it in any text. Belmont
// UI uses the same rule for its rows; the HUD keeps its own copy so it needs no library.
internal static class Glyphs
{
    private static InputDeviceWatcherService _watcher;
    private static float _nextLookup;

    internal static string Tag(string control) => $"<sprite name=\"{Prefix()}{control}\">";

    internal static string Prefix()
    {
        try
        {
            var watched = Watcher()?.LastUsedDevice?.TryCast<Gamepad>();
            var pad = watched ?? Gamepad.current;
            if (pad == null || pad.TryCast<XInputController>() != null)
                return "xbox_";
            if (pad.TryCast<DualSenseGamepadHID>() != null)   // before DualShock, which it derives from
                return "ps5_";
            if (pad.TryCast<DualShockGamepad>() != null)
                return "ps4_";
            if (pad.TryCast<SwitchProControllerHID>() != null)
                return "switch_";
        }
        catch
        {
        }
        return "xbox_";
    }

    // The game keeps one InputDeviceWatcherService (in the Settings scene); looked up again at most
    // once a second while missing.
    private static InputDeviceWatcherService Watcher()
    {
        if (_watcher != null)
            return _watcher;
        if (Time.unscaledTime < _nextLookup)
            return null;
        _nextLookup = Time.unscaledTime + 1f;
        _watcher = Object.FindObjectOfType<InputDeviceWatcherService>();
        return _watcher;
    }
}
