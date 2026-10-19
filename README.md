# Ability Wheel

An ability wheel for **Castlevania: Belmont's Curse**: put up to 4 Tarot spells on the wheel and cast any of them at once, without going through the Tarot menu.

- **Gamepad:** hold **RB** and press **Y / X / B / A**.
- **Keyboard:** press **1 / 2 / 3 / 4**. No hold needed.

Your normal spell button still casts your equipped spell, as before.

## Features

- **Instant casts from 4 slots.** The wheel swaps the card in for its spell and, by default, puts your equipped spell back right after (see *Pin Y spell*).
- **Every kind of spell works:** hold and charge spells (Life Drain, Laser Scythe Slash), Mirror Walk's teleport, Wheel of Chaos' recasts.
- **Tap RB as usual.** A quick tap still interacts (on release). Only a hold opens the wheel.
- **Assign cards in the Tarot menu.** Highlight a card, click **L3** (keyboard **F**), then press the slot's button (keyboard **1–4**). If the card was already on the wheel, the two slots swap. L3 again (keyboard **Esc**) cancels.
- **HUD in the bottom-right corner.**
  - Each tile shows the card's icon.
  - A card you can't afford right now is grayed out.
  - Your Y spell has a gold frame, and Mirror Walk shows a blue frame while its mirror is out.
  - Holding RB enlarges the tiles and shows their mana costs.
  - Labels follow the device you last used (Y X B A or 1 2 3 4).
- **Settings in the pause menu** (MODS tab, with the Belmont Minimap mod installed), or in the config file.
- **Saves stay vanilla.** Only one card is ever equipped, as in the base game, so the mod can be removed at any time.

### Pin Y spell (on by default)

- **On:** the spell you equip in the Tarot menu stays on Y. A wheel spell is only equipped while it plays, then your Y spell goes back on.
  - **Mirror Walk** stays equipped after placing its mirror until you cast it again (the teleport), press Y, cast another wheel spell or the game saves. The game removes the mirror when the card is unequipped, so casting another spell makes it vanish.
  - **Wheel of Chaos** stays equipped while its wheels are out, so its recasts work.
- **Off:** the last spell cast from the wheel stays equipped, and Y casts it again.

Some cards have Blessings or Works of Mercy tasks that only count while the card is equipped. With Pin Y spell on, a wheel card only counts during its own cast.

## Settings

Pause menu → **MODS** → **Ability Wheel**, or `BepInEx/config/leandrobueno.abilitywheel.cfg`. Changes made in the menu apply at once; edits to the file apply the next time the game starts.

| Setting | Default | |
|---|---|---|
| Wheel button (gamepad) | RB | RB, LB, RT, LT, L3, R3 or View. A tap still does the button's own action. LT is Light Parry, so it's a poor choice. |
| Keyboard key for slot Y / X / B / A | 1 / 2 / 3 / 4 | Number keys, Q, F, G, T, V, Z, B or the mouse side buttons |
| Add to wheel key (keyboard) | F | The gamepad always uses L3 |
| Pin Y spell | On | See above |
| Show wheel | Always | Always, while holding the wheel button, or never. Always shown in the Tarot menu. |
| Wheel opacity | 100% | 20–100% |
| Wheel size | 1.00x | 0.5–2x |
| Distance from right / bottom edge | 48 / 48 | Pixels at 1080p |

The config file has a few more:
- `TapSeconds`: how long a press of the wheel button still counts as a tap.
- `AutoFillSlots`: fill empty slots with cards you own.
- `StayEquipped`: cards that behave like Mirror Walk.
- `TraceActions`: detailed log for checking a card's behavior.
- `SlotY` to `SlotA`: the cards on the wheel, by card ID.

## Requirements

- Castlevania: Belmont's Curse, Steam version on Windows x64. Tested on game version 1.1.3 (BuildID 25975867).
- [BepInEx 6 Unity IL2CPP x64, bleeding edge #788](https://builds.bepinex.dev/projects/bepinex_be/788/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788%2B5b766a3.zip), or the game's [BepInEx for Castlevania](https://www.nexusmods.com/castlevaniabelmontscurse/mods/3) package (same build).
- Optional: **Belmont Minimap** 1.1.0 or newer for the in-game settings page (it provides the MODS tab). Without it, everything works and the settings are in the config file.

## Install

1. Install BepInEx if you haven't: extract it into the game folder, start the game once, then exit.
2. Extract this zip into the game folder, so the mod ends up in `BepInEx/plugins/AbilityWheel/AbilityWheel.dll`.
3. Start the game. The wheel fills itself with the first cards you own. Rearrange them in the Tarot menu.

To upgrade, overwrite the files; your settings are kept.

## Uninstall

Exit the game and delete `BepInEx/plugins/AbilityWheel/`. Your save is unaffected. You can keep or delete `BepInEx/config/leandrobueno.abilitywheel.cfg`.

## Compatibility and known limitations

- Works alongside other BepInEx mods, including HDR mods such as RenoDX.
- A tap of the wheel button does its normal action (RB: Interact) when you release it, not when you press it.
- The game itself removes Mirror Walk's mirror and Wheel of Chaos' wheels when another spell is equipped, so casting another spell ends them.
- Like the game's own spell button, a cast pressed in the middle of an attack combo may need a second press.
- In-game hints are in English. The settings page is in English and Simplified Chinese.

## Troubleshooting

`BepInEx/LogOutput.log` records what the mod does, tagged `[wheel]`, `[cast]`, `[home]`, `[assign]`, `[hud]` and `[cards]`. For a card that misbehaves on the wheel, set `TraceActions = true` under `[Debug]` in the config file, reproduce it, and include the log in your report together with the game version, mod version and what you pressed.

## Changelog

- **1.0.0**: first release.

## Credits

- [BepInEx](https://github.com/BepInEx/BepInEx), with Il2CppInterop and HarmonyX.
- [BepInEx for Castlevania](https://www.nexusmods.com/castlevaniabelmontscurse/mods/3) by KZekai.
- Belmont Minimap, for the shared MODS tab that other mods can add their settings to.
