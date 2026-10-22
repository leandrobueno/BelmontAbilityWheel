using System;
using HarmonyLib;
using UnityEngine.InputSystem;
using GamePlayerInput = MotherBase.ActionFramework.PlayerInput;

namespace AbilityWheel;

// Every patch catches its own exceptions: one thrown here would surface inside the game's code.

// Where the game turns a button press into Rose's actions (Jump, Dash, Attack, Spell and
// Interact all start on the press). Menu input never comes through here.
[HarmonyPatch(typeof(GamePlayerInput), nameof(GamePlayerInput.ProcessInputEventActionBinding))]
internal static class FilterActionInput
{
    private static bool Prefix(GamePlayerInput __instance, InputAction.CallbackContext ctx)
    {
        try
        {
            return Wheel.Instance?.LetThrough(__instance, ctx) ?? true;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[input] filter failed: {e}");
            return true;
        }
    }
}

// Rose's Move (stick and D-pad) arrives here, not through the binding above.
[HarmonyPatch(typeof(GamePlayerInput), nameof(GamePlayerInput.ProcessMoveInputActionBinding))]
internal static class FilterMoveInput
{
    private static bool Prefix(GamePlayerInput __instance, InputAction.CallbackContext ctx)
    {
        try
        {
            return Wheel.Instance?.LetMoveThrough(__instance, ctx) ?? true;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[input] move filter failed: {e}");
            return true;
        }
    }
}

// Hold spells ask here whether Spell is still held (Hold.cs).
[HarmonyPatch(typeof(GamePlayerInput), nameof(GamePlayerInput.GetInputState))]
internal static class SpellHeldByWheel
{
    private static void Postfix(MotherBase.ActionFramework.ActionInputState actionInputState, ref GamePlayerInput.InputState __result)
    {
        try
        {
            if (Wheel.Instance != null && Wheel.Instance.HoldsSpell(actionInputState))
                __result.isDown = true;
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[hold] input state patch failed: {e}");
        }
    }
}

[HarmonyPatch(typeof(InventorySpellMenuManager), nameof(InventorySpellMenuManager.OnOpened))]
internal static class SpellsMenuOpened
{
    private static void Postfix(InventorySpellMenuManager __instance)
    {
        try
        {
            Wheel.Instance?.OnSpellsMenuOpened(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[assign] Spells menu hook failed: {e}");
        }
    }
}

// While a card waits for its slot button, a click on a card (A) must not equip it.
[HarmonyPatch(typeof(InventorySpellMenuManager), nameof(InventorySpellMenuManager.OnClickedInventoryItemSlot))]
internal static class NoEquipWhileAssigning
{
    private static bool Prefix()
    {
        try
        {
            return !(Wheel.Instance?.BlocksMenuClicks ?? false);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[assign] click guard failed: {e}");
            return true;
        }
    }
}

[HarmonyPatch(typeof(InventoryWeaponMenuManager), nameof(InventoryWeaponMenuManager.OnOpened))]
internal static class WeaponsMenuOpened
{
    private static void Postfix(InventoryWeaponMenuManager __instance)
    {
        try
        {
            Wheel.Instance?.OnWeaponsMenuOpened(__instance);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[assign] Equipment menu hook failed: {e}");
        }
    }
}

// While a weapon waits for its slot button, a click (A) must not equip it.
[HarmonyPatch(typeof(InventoryWeaponMenuManager), nameof(InventoryWeaponMenuManager.OnClickedInventoryItemSlot))]
internal static class NoWeaponEquipWhileAssigning
{
    private static bool Prefix()
    {
        try
        {
            return !(Wheel.Instance?.BlocksMenuClicks ?? false);
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[assign] weapon click guard failed: {e}");
            return true;
        }
    }
}

// So the save holds the home card, not a wheel card on loan.
[HarmonyPatch(typeof(InventoryComponent), nameof(InventoryComponent.WritePlayerSave))]
internal static class HomeCardInSaves
{
    private static void Prefix()
    {
        try
        {
            Wheel.Instance?.ReturnHome("saving");
        }
        catch (Exception e)
        {
            Plugin.Logger.LogError($"[home] before saving failed: {e}");
        }
    }
}
