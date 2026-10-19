using Il2CppInterop.Runtime.Attributes;
using MotherBase.ActionFramework;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace AbilityWheel;

// Hold spells (Life Drain, charged spells) last while the Spell input state is down and end on
// Spell's release signal. A wheel cast comes from another button or key, so while that one is
// down Spell is reported held (in PlayerInput's state array and through GetInputState), and
// letting go sends Spell's release signals.
public partial class Wheel
{
    private ButtonControl _pendingControl;
    private ButtonControl _holdControl;
    private int _spellStateIndex = -1;

    [HideFromIl2Cpp]
    internal bool HoldsSpell(ActionInputState state) =>
        _holdControl != null && _holdControl.isPressed &&
        _spellState != null && state != null && state.Pointer == _spellState.Pointer;

    // PlayerInput's state array is in the same order as its data's inputStates.
    [HideFromIl2Cpp]
    private void FindSpellStateIndex()
    {
        _spellStateIndex = -1;
        var states = _input.data.inputStates;
        for (int i = 0; i < states.Length; i++)
        {
            if (states[i]?.state != null && _spellState != null && states[i].state.Pointer == _spellState.Pointer)
                _spellStateIndex = i;
        }
        int arrayLength = _input.inputActionStates?.Length ?? -1;
        if (arrayLength != states.Length)
        {
            Plugin.Logger.LogWarning(
                $"[hold] state array has {arrayLength} entries for {states.Length} input states, using GetInputState only");
            _spellStateIndex = -1;
        }
    }

    [HideFromIl2Cpp]
    private void StartHold()
    {
        DropHold();
        if (_pendingControl != null && _pendingControl.isPressed)
        {
            _holdControl = _pendingControl;
            SetSpellDown(true);
        }
        _pendingControl = null;
    }

    [HideFromIl2Cpp]
    private void UpdateHold()
    {
        if (_holdControl == null)
            return;
        if (_holdControl.isPressed)
        {
            SetSpellDown(true);
            return;
        }

        _holdControl = null;
        SetSpellDown(false);
        SendSpellRelease();
    }

    // No release signals: the cast didn't happen.
    [HideFromIl2Cpp]
    private void DropHold()
    {
        if (_holdControl == null)
            return;
        _holdControl = null;
        SetSpellDown(false);
    }

    [HideFromIl2Cpp]
    private void SetSpellDown(bool down)
    {
        var states = _input?.inputActionStates;
        if (states == null || _spellStateIndex < 0 || _spellStateIndex >= states.Length)
            return;
        var state = states[_spellStateIndex];
        if (state.isDown == down)
            return;
        state.isDown = down;
        states[_spellStateIndex] = state;
    }

    // What releasing the Spell button sends (Player_Signal_Spell_Release).
    [HideFromIl2Cpp]
    private void SendSpellRelease()
    {
        foreach (var binding in _input.data.inputBindings)
        {
            if (binding?.input?.action?.name != _spellActionName || binding.inputPhase != InputActionPhase.Canceled ||
                binding.bindingType != PlayerInputData.BindingType.SendSignal || binding.signal == null)
                continue;
            bool handled = _actor.TryProcessSignal(_entity, binding.signal);
            Plugin.Logger.LogDebug($"[hold] released: {binding.signal.name} handled {handled}");
        }
    }
}
