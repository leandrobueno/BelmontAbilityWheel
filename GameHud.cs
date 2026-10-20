using UnityEngine;

namespace AbilityWheel;

// How visible the game's own player HUD (top left: health, mana, potions) is right now, so the
// wheel can fade with it when another mod, such as HUD Auto Hide, fades that HUD through a
// CanvasGroup on HUD_Canvas/PlayerHUD. Only reads: it never adds or changes a component, and it
// needs no reference to any other mod.
internal static class GameHud
{
    private const float SearchSeconds = 1f;

    private static GameObject _root;
    private static float _nextSearch;

    // 1 while the HUD can't be found, so a game update never hides the wheel for good.
    internal static float Alpha()
    {
        float now = Time.unscaledTime;
        // An inactive HUD may be a leftover while the game builds a new one, so look again now and then.
        if ((_root == null || !_root.activeInHierarchy) && now >= _nextSearch)
        {
            _nextSearch = now + SearchSeconds;
            var found = Find();
            if (found != null && found != _root)
                Plugin.Logger.LogInfo($"[hud] following the game HUD: {Path(found.transform)}");
            _root = found;
        }
        if (_root == null)
            return 1f;
        if (!_root.activeInHierarchy)
            return 0f;

        float alpha = 1f;
        for (var t = _root.transform; t != null; t = t.parent)
        {
            var canvas = t.GetComponent<Canvas>();
            if (canvas != null && !canvas.enabled)
                return 0f;
            var group = t.GetComponent<CanvasGroup>();
            if (group == null || !group.enabled)
                continue;
            alpha *= group.alpha;
            if (group.ignoreParentGroups)
                break;
        }
        return alpha;
    }

    // The parent of the player's HUDControllerHealth (not a boss bar's), preferring an active one.
    private static GameObject Find()
    {
        GameObject inactive = null;
        foreach (var health in Object.FindObjectsByType<HUDControllerHealth>(FindObjectsInactive.Include,
                     FindObjectsSortMode.None))
        {
            if (health == null || health.hudControllerHealthContext != HUDControllerHealthContext.Player)
                continue;
            var parent = health.transform.parent;
            if (parent == null)
                continue;
            if (parent.gameObject.activeInHierarchy)
                return parent.gameObject;
            inactive ??= parent.gameObject;
        }
        return inactive;
    }

    private static string Path(Transform t)
    {
        string path = t.name;
        for (t = t.parent; t != null; t = t.parent)
            path = t.name + "/" + path;
        return path;
    }
}
