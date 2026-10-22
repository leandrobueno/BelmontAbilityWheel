# Ability Wheel

An ability wheel for **Castlevania: Belmont's Curse**: put up to 4 Tarot spells on the wheel and cast any of them at once, without going through the Tarot menu. A second page switches between up to 4 weapons.

- **Spells, gamepad:** hold **RB** and press **Y / X / B / A**.
- **Weapons, gamepad:** hold **RB**, then also hold **LB**, and press **Y / X / B / A**. Or pick a weapon button of its own (see *Weapon button*), for example LB alone.
- **Keyboard:** press **1 / 2 / 3 / 4** for spells and **5 / 6 / 7 / 8** for weapons. No hold needed.

Your normal spell button still casts your equipped spell, as before, and LB on its own still drinks a potion.

## Features

- **Instant casts from 4 slots.** The wheel swaps the card in for its spell and, by default, puts your equipped spell back right after (see *Pin Y spell*).
- **Every kind of spell works:** hold and charge spells (Life Drain, Laser Scythe Slash), Mirror Walk's teleport, Wheel of Chaos' recasts.
- **Tap RB as usual.** A quick tap still interacts (on release). Only a hold opens the wheel.
- **Assign cards in the Tarot menu.** Highlight a card, click **L3** (keyboard **F**), then press the slot's button (keyboard **1–4**). If the card was already on the wheel, the two slots swap. L3 again (keyboard **Esc**) cancels.
- **HUD in the bottom-right corner.**
  - Each card sits in a gold diamond frame, the same art as the game's Equipment and Tarot menus, with the card's icon inside.
  - A card you can't afford right now is grayed out.
  - Your Y spell has a bright gold frame with the game's four gold "equipped" spikes, and Mirror Walk gets a blue rim while its mirror is out.
  - Holding RB enlarges the wheel and shows the mana costs.
  - Labels follow the device you last used (Y X B A or 1 2 3 4). *Button labels* can hide them, or show them only while you hold the wheel button.
  - Fades out together with the game's own HUD, for example when the HUD Auto Hide mod hides it. Holding RB always shows it.
- **Weapon wheel.** Hold RB, then LB, and press a face button to switch to that slot's weapon. The weapon stays equipped, as if you had picked it in the Equipment menu, and you keep attacking with X.
  - Nothing changes until you put a weapon on the wheel: in the **Equipment** menu, highlight a weapon, click **L3** (keyboard **F**), then press the slot's button (keyboard **5–8**).
  - The order matters: RB first, then LB. LB pressed first drinks a potion, as always.
  - **Weapon button** gives the weapon wheel a button of its own instead: hold it and press a face button, the same way RB works for spells. A tap still does the button's normal action, except LB: the potion is hold-to-drink, so with LB as the weapon button, **LB never drinks a potion**. That suits a build with the **Blackened Heart** relic, which blocks healing potions anyway. With RB and the weapon button both held, the face buttons go to the one you pressed last.
  - A switch pressed in the middle of an attack happens as soon as the swing is over (its slot has a blue rim while it waits), the same moment you could walk out of the attack. Keeping on attacking for 2 seconds cancels it.
  - While the weapon button is held, and for a moment after a switch, the HUD shows the weapons, upright as in the Armoury; the equipped one has the gold frame and spikes.
- **Settings in the pause menu:** the **MODS** tab, after SYSTEM (**MODS SETTINGS** with Belmont UI 0.2; title screen: Settings → the same tab), with the Belmont UI plugin (see Requirements).
- **Saves stay vanilla.** Only one card is ever equipped, as in the base game, so the mod can be removed at any time.

### Pin Y spell (on by default)

- **On:** the spell you equip in the Tarot menu stays on Y. A wheel spell is only equipped while it plays, then your Y spell goes back on.
  - **Mirror Walk** stays equipped after placing its mirror until you cast it again (the teleport), press Y, cast another wheel spell or the game saves. The game removes the mirror when the card is unequipped, so casting another spell makes it vanish.
  - **Wheel of Chaos** stays equipped while its wheels are out, so its recasts work.
- **Off:** the last spell cast from the wheel stays equipped, and Y casts it again.

Some cards have Blessings or Works of Mercy tasks that only count while the card is equipped. With Pin Y spell on, a wheel card only counts during its own cast.

## Settings

With the Belmont UI plugin installed, the settings are in the pause menu: the **MODS** tab after SYSTEM (**MODS SETTINGS** with Belmont UI 0.2), then **Settings** on the **Ability Wheel** row (on the title screen: **Settings** → **Mods Settings**). They look and work like the game's other settings and apply at once; **Restore defaults** at the end of the section (a Reset button) puts them all back. Without that plugin the wheel works the same, and the config file is the only place to change settings.

You can also edit `BepInEx/config/leandrobueno.abilitywheel.cfg`; edits to the file apply the next time the game starts.

| Setting | Default | |
|---|---|---|
| Wheel button (gamepad) | RB | RB, LB, RT, LT, L3, R3 or View. A tap still does the button's own action. LT is Holy Light, so it's a poor choice. |
| Keyboard key for slot Y / X / B / A | 1 / 2 / 3 / 4 | Number keys, Q, F, G, T, V, Z, B or the mouse side buttons |
| Add to wheel key (keyboard) | F | Tarot and Equipment menus. The gamepad always uses L3 |
| Weapon wheel | On | Hold the weapon button and press Y/X/B/A to switch weapons |
| Weapon button (gamepad) | Wheel button + LB | Wheel button + LB (LB alone still drinks; off when the wheel button is LB), or a button of its own: RB, LB, RT, LT, L3, R3 or View, not the wheel button. A tap still does the button's own action, except LB, which then never drinks a potion |
| Weapon key for slot Y / X / B / A | 5 / 6 / 7 / 8 | Same choices as the spell keys |
| Pin Y spell | On | See above |
| Show wheel | Always | Always, while holding the wheel button, or never. Always shown in the Tarot menu. |
| Button labels | Always | When each card shows its button (Y/X/B/A, or its key after using the keyboard): always, while holding the wheel button, or never. Always shown while putting a card or weapon on the wheel. |
| Fade with game HUD | On | Fades the cards out while the game's HUD (health and mana) is hidden, for example by HUD Auto Hide. Holding the wheel button or the Tarot menu always shows them. |
| Wheel opacity | 100% | 20–100% |
| Wheel size | 1.00x | 0.5–2x |
| Distance from right / bottom edge | 48 / 48 | Pixels at 1080p |

The config file has a few more:
- `TapSeconds`: how long a press of the wheel button still counts as a tap.
- `AutoFillSlots`: fill empty slots with cards you own.
- `StayEquipped`: cards that behave like Mirror Walk.
- `TraceActions`: detailed log for checking a card's behavior.
- `SlotY` to `SlotA` under `[Cards]`: the cards on the wheel, by card ID.
- `SlotY` to `SlotA` under `[Weapons]`: the weapons on the wheel, by weapon ID (for example `WeaponSpearTier4`).

## Requirements

- Castlevania: Belmont's Curse, Steam version on Windows x64. Tested on game version 1.1.3 (BuildID 25975867).
- [BepInEx 6 Unity IL2CPP x64, bleeding edge #788](https://builds.bepinex.dev/projects/bepinex_be/788/BepInEx-Unity.IL2CPP-win-x64-6.0.0-be.788%2B5b766a3.zip), or the game's [BepInEx for Castlevania](https://www.nexusmods.com/castlevaniabelmontscurse/mods/3) package (same build).
- [Belmont UI](../BelmontUI) 0.2.0 or newer, for the settings page in the pause menu (0.3.0 or newer for translated settings). Optional: without it the mod still works, with the config file as the only way to change settings.

## Install

1. Install BepInEx if you haven't: extract it into the game folder, start the game once, then exit.
2. Extract this zip into the game folder, so the mod ends up in `BepInEx/plugins/AbilityWheel/AbilityWheel.dll` and the library in `BepInEx/plugins/BelmontUI/BelmontUI.dll`.
3. Start the game. The wheel fills itself with the first cards you own. Rearrange them in the Tarot menu.

To upgrade, overwrite the files; your settings are kept.

## Uninstall

Exit the game and delete `BepInEx/plugins/AbilityWheel/`. Your save is unaffected. You can keep or delete `BepInEx/config/leandrobueno.abilitywheel.cfg`.

## Compatibility and known limitations

- Works alongside other BepInEx mods, including HDR mods such as RenoDX and Belmont Minimap. Since 1.5.0 the settings are only in Belmont UI's MODS tab (or the config file), not in Belmont Minimap's MODS tab.
- With HUD Auto Hide, the wheel fades in and out with the game's HUD (*Fade with game HUD*). No setup is needed in either mod.
- A tap of the wheel button does its normal action (RB: Interact) when you release it, not when you press it.
- The game itself removes Mirror Walk's mirror and Wheel of Chaos' wheels when another spell is equipped, so casting another spell ends them.
- Like the game's own spell button, a cast pressed in the middle of an attack combo may need a second press.
- The settings page and the wheel's menu hints are in the game's 11 languages with Belmont UI 0.3.0 or newer (English with 0.2.x).

## Troubleshooting

`BepInEx/LogOutput.log` records what the mod does, tagged `[wheel]`, `[cast]`, `[home]`, `[weapon]`, `[assign]`, `[hud]`, `[art]`, `[cards]` and `[weapons]`. For a card that misbehaves on the wheel, set `TraceActions = true` under `[Debug]` in the config file, reproduce it, and include the log in your report together with the game version, mod version and what you pressed.

## Building from source

Needs the .NET SDK and the game with BepInEx installed and started once (BepInEx generates the interop assemblies the project references). `dotnet build -c Release` builds the mod and copies `AbilityWheel.dll` into the game's `BepInEx/plugins/AbilityWheel`, so close the game first. If the game isn't in the default Steam folder, add `-p:GameDir="<game folder>"`. Build [Belmont UI](../BelmontUI) first: the project references its dll from `BepInEx/plugins/BelmontUI`.

## License

MIT, see [LICENSE](LICENSE).

## Changelog

- **1.5.0**: *Weapon button* setting: the weapon wheel can have a button of its own (for example LB alone) instead of the wheel button then LB. With LB, LB no longer drinks potions, which suits the Blackened Heart relic. Weapon icons are bigger and fill their diamond with a blur of their own colours. With Belmont UI 0.3.0 or newer, the settings page and the wheel's menu hints follow the game's language (all 11). Settings are no longer added to Belmont Minimap's MODS tab; use Belmont UI's MODS tab or the config file.
- **1.4.1**: *Button labels* setting: show the Y/X/B/A (or key) labels on the cards always, only while holding the wheel button, or never. They always show while putting a card or weapon on the wheel.
- **1.4.0**: native look: the wheel uses the game's own slot art (the gold diamond frames of the Equipment and Tarot menus), the four gold spikes on the equipped card, a blue rim on a card that stays out or a weapon switch that waits, grayed-out frames for cards you can't afford, and weapon icons upright as in the Armoury. Falls back to the plain tiles if the art isn't found.
- **1.3.0**: weapon wheel: hold RB, then LB, and press Y/X/B/A (keyboard 5–8) to switch between 4 weapons; assign them in the Equipment menu with L3.
- **1.2.0**: *Fade with game HUD* (on by default): the wheel fades out while the game's HUD is hidden, for example by HUD Auto Hide, and holding the wheel button always shows it.
- **1.1.0**: settings moved from a popup in the Tarot menu (F3 / R3) to the pause menu's Mods Settings tab, which the Belmont UI plugin adds (optional dependency). The SettingsKey entry is gone.
- **1.0.0**: first release.

## Credits

- [BepInEx](https://github.com/BepInEx/BepInEx), with Il2CppInterop and HarmonyX.
- [BepInEx for Castlevania](https://www.nexusmods.com/castlevaniabelmontscurse/mods/3) by KZekai.
