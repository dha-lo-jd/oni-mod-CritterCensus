using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace CritterCensus;

// The skills screen's look: its sprites and text styles are harvested from the live screen by name, its colors are
// the values it uses (read from a style dump). Without the screen (game update) everything falls back to flat colors.
internal static class UiStyle
{
    public static readonly Color Background = new(0.251f, 0.271f, 0.349f, 1f);
    public static readonly Color TitleBar = new(0.529f, 0.272f, 0.401f, 1f);
    public static readonly Color Button = new(0.243f, 0.263f, 0.341f, 1f);
    public static readonly Color SortHeader = new(0.635f, 0.647f, 0.682f, 1f);
    public static readonly Color Content = new(0.192f, 0.205f, 0.258f, 1f);

    private static readonly Dictionary<string, Sprite> sprites = new();
    private static readonly Dictionary<string, TextStyleSetting> styles = new();

    public static void Harvest(Transform skillsScreen)
    {
        if (skillsScreen == null)
        {
            return;
        }

        foreach (var image in skillsScreen.GetComponentsInChildren<Image>(true))
        {
            if (image.sprite != null && !sprites.ContainsKey(image.sprite.name))
            {
                sprites[image.sprite.name] = image.sprite;
            }
        }

        foreach (var text in skillsScreen.GetComponentsInChildren<LocText>(true))
        {
            var style = text.textStyleSetting;
            if (style != null && !styles.ContainsKey(style.name))
            {
                styles[style.name] = style;
            }
        }
    }

    public static Sprite Sprite(string name) => sprites.TryGetValue(name, out var sprite) ? sprite : null;

    public static TextStyleSetting Style(string name) => name != null && styles.TryGetValue(name, out var style) ? style : null;

    // Sliced game sprite with a tint; flat color when the sprite isn't available.
    public static void Apply(Image image, string spriteName, Color color)
    {
        var sprite = Sprite(spriteName);
        image.sprite = sprite;
        image.type = sprite != null ? Image.Type.Sliced : Image.Type.Simple;
        image.color = color;
    }
}
