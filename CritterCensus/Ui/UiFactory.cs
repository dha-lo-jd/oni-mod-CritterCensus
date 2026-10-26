using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace CritterCensus;

// Builds the few widgets the window needs in code. Text reuses the game's own tooltip text style
// (a LocText without a style renders invisible), and every layout rect is top-left anchored with y growing downward.
internal static class UiFactory
{
    public static readonly Color ButtonColor = UiStyle.Button;

    public static RectTransform CreateRect(string name, Transform parent, float x, float y, float width, float height)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent, false);
        return Place(go.GetComponent<RectTransform>(), x, y, width, height);
    }

    public static RectTransform Place(RectTransform rect, float x, float y, float width, float height)
    {
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(x, -y);
        rect.sizeDelta = new Vector2(width, height);
        return rect;
    }

    // Re-anchors a rect to the parent's top-right corner, keeping its size.
    public static void AnchorTopRight(RectTransform rect, float marginRight, float y)
    {
        rect.anchorMin = new Vector2(1f, 1f);
        rect.anchorMax = new Vector2(1f, 1f);
        rect.pivot = new Vector2(1f, 1f);
        rect.anchoredPosition = new Vector2(-marginRight, -y);
    }

    public static Image CreatePanel(string name, Transform parent, Color color, float x, float y, float width, float height)
    {
        var rect = CreateRect(name, parent, x, y, width, height);
        var image = rect.gameObject.AddComponent<Image>();
        image.color = color;
        return image;
    }

    // A panel using one of the skills screen's sprites (web_box, web_button, ...).
    public static Image CreateSpritePanel(string name, Transform parent, string sprite, Color color, float x, float y, float width, float height)
    {
        var image = CreateRect(name, parent, x, y, width, height).gameObject.AddComponent<Image>();
        UiStyle.Apply(image, sprite, color);
        return image;
    }

    public static LocText CreateLabel(Transform parent, string text, float x, float y, float width, float height, TextAlignmentOptions alignment = TextAlignmentOptions.Left, string style = "style_bodyTextWhite")
    {
        var rect = CreateRect("Label", parent, x, y, width, height);

        // Awake runs on activation; configure first so LocText doesn't look up a null string key.
        rect.gameObject.SetActive(false);
        var label = rect.gameObject.AddComponent<LocText>();
        label.key = "";
        label.allowOverride = true;
        label.textStyleSetting = UiStyle.Style(style) ?? ToolTipScreen.Instance.defaultTooltipBodyStyle;
        rect.gameObject.SetActive(true);

        label.ApplySettings();
        label.alignment = alignment;
        label.raycastTarget = false;
        label.textWrappingMode = TextWrappingModes.NoWrap;
        label.text = text;
        return label;
    }

    public static Button CreateButton(Transform parent, string text, float x, float y, float width, float height, UnityAction onClick)
    {
        var rect = CreateRect("Button", parent, x, y, width, height);
        var image = rect.gameObject.AddComponent<Image>();
        UiStyle.Apply(image, "web_button", ButtonColor);
        var button = rect.gameObject.AddComponent<Button>();
        button.targetGraphic = image;
        var colors = button.colors;
        colors.highlightedColor = new Color(1.25f, 1.25f, 1.25f, 1f);
        colors.pressedColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        button.colors = colors;
        button.onClick.AddListener(onClick);
        CreateLabel(rect, text, 0f, 0f, width, height, TextAlignmentOptions.Center);
        return button;
    }
}
