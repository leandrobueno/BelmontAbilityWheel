using System;
using System.Collections.Generic;
using Il2CppInterop.Runtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace AbilityWheel;

// The game's own slot art for the wheel: the upright diamonds and gold-leaf materials of the
// Equipment and Arcanas menus. Borrowed, never loaded: it is read off the GameMenu scene (which
// stays loaded, inactive, during gameplay), then by name among loaded objects. Anything not found
// stays null, and the HUD draws its plain tiles for that part instead.
internal static class Art
{
    private const string Menu = "GameMenuView/TabsLayout_GameMenu/TabsContent_Group/TabContent_Equipment/ContentContainer/ContentLayout/";

    // Images whose sprite or material we want, under the GameMenu root.
    private static readonly string[] Sources =
    {
        Menu + "EquipmentView/Charms/BringToFrontWrapper/EquippedCharmSlot",
        Menu + "EquipmentView/Charms/BringToFrontWrapper/EquippedCharmSlot/Icon_Mask",
        Menu + "Overlay_SpellInventory/SpellSelection_ArcanaListView_Icons/SpellList_HorizontalLayout_Anim/20_Base_InventorySpell_ItemSlot/CommonUI_SpellSlot/Equipped",
        Menu + "Overlay_SpellInventory/SelectedSpell_TalentView/SpellAndTalents_Display/Spell_Anim/SpellItem/SpellSlot",
        Menu + "Overlay_CharmsInventory/InventoryGroup/Pages/CharmsItemSlots_GridPage/InventoryCharm_ItemSlot (1)/Background",
    };

    private const string FrameName = "CommonUI_SquareSlot_03";       // dark diamond: the frame, and the icon mask
    private const string GoldFrameName = "CommonUI_SquareSlot_04";   // bright gold diamond
    private const string SpikesName = "TalentGem_Slot_Equipped";     // the game's "equipped" sign
    private const string RimName = "CommonUI_GridSlot";              // white brush square, tinted blue
    private const string IronName = "RedWine_UI_Image_Goldleaf_Iron_Luminance";
    private const string GoldName = "RedWine_UI_Image_Goldleaf_Gold_Luminance";

    internal static Sprite Frame, GoldFrame, Spikes, Rim;
    internal static Material Iron, Gold;

    // Without the frame there is no native slot at all.
    internal static bool Native => Frame != null;

    internal static void Load()
    {
        if (Native)
            return;
        var sprites = new Dictionary<string, Sprite>();
        var materials = new Dictionary<string, Material>();
        string[] spriteNames = { FrameName, GoldFrameName, SpikesName, RimName };
        string[] materialNames = { IronName, GoldName };
        try
        {
            var scene = SceneManager.GetSceneByName("GameMenu");
            if (scene.IsValid() && scene.isLoaded)
                foreach (var root in scene.GetRootGameObjects())
                {
                    if (root == null || root.name != "GameMenu")
                        continue;
                    foreach (var path in Sources)
                    {
                        // Transform.Find also walks inactive children, unlike GameObject.Find.
                        var image = root.transform.Find(path)?.GetComponent<Image>();
                        if (image == null)
                            continue;
                        Keep(sprites, spriteNames, image.sprite);
                        Keep(materials, materialNames, image.material);
                    }
                }

            if (sprites.Count < spriteNames.Length)
                foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<Sprite>()))
                    Keep(sprites, spriteNames, obj?.TryCast<Sprite>());
            if (materials.Count < materialNames.Length)
                foreach (var obj in Resources.FindObjectsOfTypeAll(Il2CppType.Of<Material>()))
                    Keep(materials, materialNames, obj?.TryCast<Material>());
        }
        catch (Exception e)
        {
            Plugin.Logger.LogWarning($"[art] looking for the game's slot art failed: {e.Message}");
        }

        Frame = sprites.GetValueOrDefault(FrameName);
        GoldFrame = sprites.GetValueOrDefault(GoldFrameName);
        Spikes = sprites.GetValueOrDefault(SpikesName);
        Rim = sprites.GetValueOrDefault(RimName);
        Iron = materials.GetValueOrDefault(IronName);
        Gold = materials.GetValueOrDefault(GoldName);

        var missing = new List<string>();
        foreach (var name in spriteNames)
            if (!sprites.ContainsKey(name))
                missing.Add(name);
        foreach (var name in materialNames)
            if (!materials.ContainsKey(name))
                missing.Add(name);
        if (!Native)
            Plugin.Logger.LogWarning($"[art] no {FrameName}, drawing plain tiles");
        else if (missing.Count > 0)
            Plugin.Logger.LogInfo($"[art] native slots, without {string.Join(", ", missing)}");
        else
            Plugin.Logger.LogInfo("[art] native slots, all art found");
    }

    // A weapon icon's own colours, blurred, to fill the diamond around the square Armoury icon. The
    // icon atlas isn't readable and has no mipmaps, so the blur is two bilinear GPU downscales of
    // the sprite's atlas rect (to 32 px, then 8 px), shown stretched. One small texture per icon,
    // kept for the session; null if it can't be made, and the HUD then shows the icon without it.
    private static readonly Dictionary<string, RenderTexture> Backdrops = new();
    private static readonly HashSet<string> BackdropFailed = new();

    internal static Texture Backdrop(Sprite sprite)
    {
        if (sprite == null || BackdropFailed.Contains(sprite.name))
            return null;
        if (Backdrops.TryGetValue(sprite.name, out var cached))
        {
            if (cached != null && cached.IsCreated())
                return cached;
            // Lost with the graphics device (a resolution or fullscreen change): made again.
            if (cached != null)
                cached.Release();
            Backdrops.Remove(sprite.name);
        }

        RenderTexture small = null;
        try
        {
            var texture = sprite.texture;
            var rect = sprite.textureRect;
            var scale = new Vector2(rect.width / texture.width, rect.height / texture.height);
            var offset = new Vector2(rect.x / texture.width, rect.y / texture.height);
            small = RenderTexture.GetTemporary(32, 32, 0);
            small.filterMode = FilterMode.Bilinear;
            Graphics.Blit(texture, small, scale, offset);
            var blurred = new RenderTexture(8, 8, 0)
            {
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontUnloadUnusedAsset,
                name = "AbilityWheel_" + sprite.name,
            };
            blurred.Create();
            Graphics.Blit(small, blurred);
            Backdrops[sprite.name] = blurred;
            return blurred;
        }
        catch (Exception e)
        {
            BackdropFailed.Add(sprite.name);
            Plugin.Logger.LogWarning($"[art] no backdrop for {sprite.name}, showing the plain icon: {e.Message}");
            return null;
        }
        finally
        {
            if (small != null)
                RenderTexture.ReleaseTemporary(small);
        }
    }

    private static void Keep<T>(Dictionary<string, T> found, string[] wanted, T asset) where T : UnityEngine.Object
    {
        if (asset == null)
            return;
        string name = asset.name;
        if (Array.IndexOf(wanted, name) >= 0)
            found.TryAdd(name, asset);
    }
}
