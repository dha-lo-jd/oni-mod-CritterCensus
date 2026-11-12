using System;
using System.IO;
using UnityEngine;

namespace CritterCensus;

// The tool button's own icon: ModAssets/tool_icon.png, registered with the game's sprites under a name of this mod so
// the tool menu finds it like a vanilla icon. Without the file the button borrows a vanilla icon.
internal static class ToolIcon
{
    public const string FileName = "tool_icon.png";
    public const string SpriteName = "crittercensus_tool_icon";
    public const string Fallback = "icon_action_capture";

    private static Sprite sprite;

    // The sprite name to give the tool menu. Called when the menu builds its buttons; the game's sprite table is
    // rebuilt with each loaded game, so the icon is registered again whenever it is missing there.
    public static string Resolve()
    {
        if (Assets.Sprites == null)
        {
            return Fallback;
        }

        if (sprite == null)
        {
            sprite = Load();
        }

        if (sprite == null)
        {
            return Fallback;
        }

        Assets.Sprites[SpriteName] = sprite;
        return SpriteName;
    }

    private static Sprite Load()
    {
        if (CritterCensusMod.modPath == null)
        {
            return null;
        }

        var file = Path.Combine(CritterCensusMod.modPath, "ModAssets", FileName);
        if (!File.Exists(file))
        {
            return null;
        }

        try
        {
            // The button draws the icon far smaller than the file: mipmaps keep it smooth.
            var texture = new Texture2D(2, 2, TextureFormat.RGBA32, true)
            {
                filterMode = FilterMode.Trilinear,
                wrapMode = TextureWrapMode.Clamp,
                name = SpriteName,
            };
            if (texture.LoadImage(File.ReadAllBytes(file)))
            {
                var loaded = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f), 100f);
                loaded.name = SpriteName;
                return loaded;
            }

            Debug.LogWarning("[CritterCensus] " + FileName + " is not a readable image");
        }
        catch (Exception e)
        {
            Debug.LogWarning("[CritterCensus] Failed to load " + FileName + ": " + e.Message);
        }

        return null;
    }
}
