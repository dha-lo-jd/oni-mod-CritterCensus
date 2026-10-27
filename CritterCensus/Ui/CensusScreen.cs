using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;

namespace CritterCensus;

// Full-screen modal listing the critters and eggs found in the selected area, laid out like the skills screen: filters
// in the left pane, the list in the right pane. The list is a snapshot taken when the screen opens; filters only narrow
// what is shown. It has its own root canvas set up like the skills screen's.
public sealed class CensusScreen : KModalScreen
{
    private const float Margin = 10f;
    private const float LeftWidth = 320f;
    private const float TitleHeight = 36f;
    private const float ToggleHeight = 28f;
    private const float SectionHeight = 22f;
    private const float RowHeight = 58f;
    private const float IconSize = 46f;
    private const float HeaderHeight = 24f;
    private const float BoxSize = 16f;

    // The management menu buttons sit over the top of the right pane.
    private const float TopReserved = 68f;

    private static readonly Color BoxColor = new Color(0.12f, 0.13f, 0.17f, 1f);
    private static readonly Color CheckColor = new Color(0.45f, 0.85f, 0.45f, 1f);
    private static readonly Color ProgressColor = new Color(0.35f, 0.65f, 0.4f, 0.4f);
    private static readonly Color ProgressTrackColor = new Color(0.12f, 0.25f, 0.15f, 0.6f);
    private static readonly Color GoodColor = new Color(0.55f, 1f, 0.55f, 1f);
    private static readonly Color WarnColor = new Color(1f, 0.65f, 0.2f, 1f);
    private static readonly Color AilmentColor = new Color(1f, 0.6f, 0.6f, 1f);
    private static readonly Color WildTextColor = new Color(0.85f, 0.6f, 0.8f, 1f);
    private static readonly Color WildRowColor = new Color(0.36f, 0.26f, 0.34f, 1f);
    private static readonly Color RowOn = new Color(0.33f, 0.36f, 0.46f, 1f);
    private static readonly float[] ColumnX = { 8f, 80f, 370f, 450f, 710f, 800f, 1050f, 1140f, 1230f, 1330f };
    private static string[] ColumnNames => new string[]
    {
        STRINGS.UI.COLUMNS.STAGE, STRINGS.UI.COLUMNS.CRITTER, STRINGS.UI.COLUMNS.ORIGIN, STRINGS.UI.COLUMNS.AGE,
        STRINGS.UI.COLUMNS.HAPPINESS, STRINGS.UI.COLUMNS.FERTILITY, STRINGS.UI.COLUMNS.FULLNESS, STRINGS.UI.COLUMNS.FUR, STRINGS.UI.COLUMNS.TEMPERATURE, STRINGS.UI.COLUMNS.AILMENTS,
    };

    private static CensusScreen current;

    private sealed class ToggleView
    {
        public MultiToggle Vanilla;
        public Image Background;
        public GameObject Check;

        public void Set(bool on)
        {
            if (Vanilla != null)
            {
                Vanilla.ChangeState(on ? 1 : 0);
                return;
            }

            Background.color = on ? RowOn : UiStyle.Button;
            Check.SetActive(on);
        }
    }

    private RectTransform rect;
    private RectTransform content;
    private LocText countLabel;
    private RectTransform header;
    private readonly List<LocText> headerLabels = new List<LocText>();

    // Column being sorted (-1: snapshot order) and its direction; unknown values always sort last.
    private int sortColumn = -1;
    private bool sortDescending;
    private List<CritterRow> rows;
    private readonly HashSet<CritterStage> stages = new HashSet<CritterStage> { CritterStage.Adult, CritterStage.Baby, CritterStage.Egg };
    private readonly HashSet<string> genera = new HashSet<string>();
    private readonly Dictionary<CritterStage, ToggleView> stageToggles = new Dictionary<CritterStage, ToggleView>();
    private readonly Dictionary<string, ToggleView> genusToggles = new Dictionary<string, ToggleView>();
    private ToggleView allToggle;

    public static void Open(CensusSelection selection)
    {
        if (current != null)
        {
            current.Close();
        }

        // Opening this window while another one is up replaces it.
        CloseOtherScreens();

        var rows = CritterCollector.Collect(selection);
        var skills = ManagementMenu.Instance != null
            ? Traverse.Create(ManagementMenu.Instance).Field("skillsScreen").GetValue<SkillsScreen>()
            : null;
        var skillsCanvas = skills != null ? skills.GetComponent<Canvas>() : null;
        UiStyle.Harvest(skills != null ? skills.transform : null);
        var parent = skillsCanvas != null && skills.transform.parent != null
            ? skills.transform.parent
            : GameScreenManager.Instance.ssOverlayCanvas.transform;

        var go = new GameObject("CritterCensusScreen", typeof(RectTransform));
        go.SetActive(false);
        go.transform.SetParent(parent, false);
        CopyCanvasSetup(go, skills != null ? skills.gameObject : null, skillsCanvas);

        var screen = go.AddComponent<CensusScreen>();
        screen.rect = go.GetComponent<RectTransform>();
        screen.rows = rows.OrderBy(r => r.GenusName).ThenBy(r => r.Stage).ThenBy(r => r.MorphName).ToList();
        current = screen;
        screen.Activate();
        screen.Show(true);
        Canvas.ForceUpdateCanvases();
        screen.Build();
    }

    // Same canvas settings and scaler as the skills screen; without a source, a plain overlay canvas.
    private static void CopyCanvasSetup(GameObject go, GameObject source, Canvas sourceCanvas)
    {
        var canvas = go.AddComponent<Canvas>();
        var scaler = go.AddComponent<CanvasScaler>();
        if (sourceCanvas == null)
        {
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            go.AddComponent<GraphicRaycaster>();
            return;
        }

        canvas.renderMode = sourceCanvas.renderMode;
        canvas.worldCamera = sourceCanvas.worldCamera;
        canvas.planeDistance = sourceCanvas.planeDistance;
        canvas.pixelPerfect = sourceCanvas.pixelPerfect;
        canvas.overrideSorting = sourceCanvas.overrideSorting;
        canvas.sortingLayerID = sourceCanvas.sortingLayerID;
        canvas.sortingOrder = sourceCanvas.sortingOrder;

        var sourceScaler = source.GetComponent<CanvasScaler>();
        if (sourceScaler != null)
        {
            scaler.uiScaleMode = sourceScaler.uiScaleMode;
            scaler.scaleFactor = sourceScaler.scaleFactor;
            scaler.referencePixelsPerUnit = sourceScaler.referencePixelsPerUnit;
            scaler.referenceResolution = sourceScaler.referenceResolution;
            scaler.screenMatchMode = sourceScaler.screenMatchMode;
            scaler.matchWidthOrHeight = sourceScaler.matchWidthOrHeight;
        }

        // Keeps the scale factor following the game's UI scale setting and resolution.
        if (source.GetComponent<KCanvasScaler>() != null)
        {
            go.AddComponent<KCanvasScaler>();
        }

        go.AddComponent<GraphicRaycaster>();
    }

    private static bool ManagementScreenOpen() =>
        ManagementMenu.Instance != null && Traverse.Create(ManagementMenu.Instance).Field("activeScreen").GetValue() != null;

    private static void CloseOtherScreens()
    {
        if (ManagementScreenOpen())
        {
            ManagementMenu.Instance.CloseAll();
        }

        if (PauseScreen.Instance != null && PauseScreen.Instance.gameObject.activeInHierarchy)
        {
            PauseScreen.Instance.Show(false);
        }

        ModalScreens.CloseOthers(null);
    }

    // Hiding this window (e.g. another mod's window opening) closes it for good.
    public override void Show(bool show = true)
    {
        if (!show)
        {
            Close();
            return;
        }

        base.Show(show);
    }

    // Another management screen (skills, research, ...) or the pause menu took over: close this window.
    private void Update()
    {
        if (ManagementScreenOpen() || (PauseScreen.Instance != null && PauseScreen.Instance.gameObject.activeInHierarchy))
        {
            Close();
        }
    }

    public override void OnKeyDown(KButtonEvent e)
    {
        if (!e.Consumed && (e.TryConsume(Action.Escape) || e.TryConsume(Action.MouseRight)))
        {
            Close();
        }

        base.OnKeyDown(e);
    }

    public void Close()
    {
        if (current == this)
        {
            current = null;
        }

        Deactivate();
        Destroy(gameObject);
    }

    private void Build()
    {
        UiStyle.Apply(gameObject.AddComponent<Image>(), "web_box", UiStyle.Background);

        // Right pane: the darker content color the skills screen uses behind its tables.
        var right = UiFactory.CreateRect("Right pane", rect, 0f, 0f, 0f, 0f);
        right.anchorMin = Vector2.zero;
        right.anchorMax = Vector2.one;
        right.pivot = new Vector2(0.5f, 0.5f);
        right.offsetMin = new Vector2(LeftWidth, 0f);
        right.offsetMax = Vector2.zero;
        right.gameObject.AddComponent<Image>().color = UiStyle.Content;
        var rightWidth = (rect.rect.width > 100f ? rect.rect.width : 1200f) - LeftWidth;

        BuildLeftPane();
        BuildHeader(right, rightWidth);
        BuildList(right);
        Refresh();
    }

    // Left pane: black border, purple title, then the scrolling filter list.
    private void BuildLeftPane()
    {
        var border = UiFactory.CreateRect("Pane border", rect, 0f, 0f, LeftWidth, 0f);
        border.anchorMin = new Vector2(0f, 0f);
        border.anchorMax = new Vector2(0f, 1f);
        border.sizeDelta = new Vector2(LeftWidth, 0f);
        border.anchoredPosition = Vector2.zero;
        var borderImage = border.gameObject.AddComponent<Image>();
        UiStyle.Apply(borderImage, "web_border", Color.black);
        borderImage.raycastTarget = false;

        UiFactory.CreateSpritePanel("Pane title", rect, "web_button", UiStyle.TitleBar, 0f, 0f, LeftWidth, TitleHeight);
        var title = UiFactory.CreateLabel(rect, STRINGS.UI.TITLE, 8f, 8f, 190f, 20f, style: "style_titleTextLarge");
        title.fontSize = 18f;
        countLabel = UiFactory.CreateLabel(rect, "", 200f, 8f, 80f, 20f, TMPro.TextAlignmentOptions.Right);
        var close = UiFactory.CreateButton(rect, "X", LeftWidth - 32f, 6f, 24f, 24f, Close);
        close.name = "Close";

        var viewport = UiFactory.CreateRect("Filter viewport", rect, 0f, TitleHeight, 0f, 0f);
        viewport.anchorMin = new Vector2(0f, 0f);
        viewport.anchorMax = new Vector2(0f, 1f);
        viewport.pivot = new Vector2(0f, 1f);
        viewport.offsetMin = new Vector2(4f, 4f);
        viewport.offsetMax = new Vector2(LeftWidth - 4f, -TitleHeight);
        viewport.gameObject.AddComponent<RectMask2D>();
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        var filters = UiFactory.CreateRect("Filters", viewport, 0f, 0f, 0f, 0f);
        filters.anchorMin = new Vector2(0f, 1f);
        filters.anchorMax = new Vector2(1f, 1f);
        filters.pivot = new Vector2(0.5f, 1f);
        filters.offsetMin = Vector2.zero;
        filters.offsetMax = Vector2.zero;
        var layout = filters.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 2f;
        layout.padding = new RectOffset(2, 2, 6, 6);
        filters.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        ConfigureScroll(viewport, filters);

        AddSection(filters, STRINGS.UI.FILTERS.STAGE);
        foreach (var stage in new[] { CritterStage.Adult, CritterStage.Baby, CritterStage.Egg })
        {
            var captured = stage;
            var count = rows.Count(r => r.Stage == stage);
            stageToggles[stage] = AddToggle(filters, StageName(stage) + " (" + count + ")", () =>
            {
                if (!stages.Remove(captured))
                {
                    stages.Add(captured);
                }

                Refresh();
            });
        }

        // One toggle per genus actually present in the area.
        AddSection(filters, STRINGS.UI.FILTERS.GENUS);
        allToggle = AddToggle(filters, STRINGS.UI.FILTERS.ALL + " (" + rows.Count + ")", () =>
        {
            genera.Clear();
            Refresh();
        });
        foreach (var group in rows.GroupBy(r => r.GenusKey).OrderBy(g => g.First().GenusName))
        {
            var key = group.Key;
            genusToggles[key] = AddToggle(filters, group.First().GenusName + " (" + group.Count() + ")", () =>
            {
                if (!genera.Remove(key))
                {
                    genera.Add(key);
                }

                Refresh();
            });
        }
    }

    private static ScrollRect ConfigureScroll(RectTransform viewport, RectTransform content)
    {
        var scroll = viewport.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport;
        scroll.content = content;
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 30f;
        return scroll;
    }

    private static void AddSection(RectTransform parent, string text)
    {
        var holder = UiFactory.CreateRect("Section", parent, 0f, 0f, 0f, SectionHeight);
        holder.gameObject.AddComponent<LayoutElement>().minHeight = SectionHeight;
        UiFactory.CreateLabel(holder, text, 4f, 2f, 240f, 20f);
    }

    // A button row with a checkbox in front of the label. Clicking anywhere on the row toggles; the checkbox mirrors
    // the toggle state. The checkbox is the game's own filter checkbox when available, else a drawn box.
    private static ToggleView AddToggle(RectTransform parent, string text, UnityEngine.Events.UnityAction onClick)
    {
        var button = UiFactory.CreateButton(parent, "", 0f, 0f, 0f, ToggleHeight, onClick);
        button.gameObject.AddComponent<LayoutElement>().minHeight = ToggleHeight;
        var holder = button.GetComponent<RectTransform>();
        UiFactory.CreateLabel(holder, text, 8f + BoxSize + 8f, (ToggleHeight - 20f) / 2f, 260f, 20f);

        var vanilla = GraftVanillaCheckbox(holder);
        if (vanilla != null)
        {
            return new ToggleView { Vanilla = vanilla };
        }

        var box = UiFactory.CreatePanel("Checkbox", holder, BoxColor, 8f, (ToggleHeight - BoxSize) / 2f, BoxSize, BoxSize);
        box.raycastTarget = false;
        var check = UiFactory.CreatePanel("Check", box.transform, CheckColor, 3f, 3f, BoxSize - 6f, BoxSize - 6f);
        check.raycastTarget = false;
        return new ToggleView { Background = button.GetComponent<Image>(), Check = check.gameObject };
    }

    // Takes the checkbox out of the game's filter row and puts it at the left of our own row. Clicks on the checkbox
    // itself are consumed by it, so only row clicks reach the row button (no double toggle).
    private static MultiToggle GraftVanillaCheckbox(RectTransform row)
    {
        var prefab = ToolMenu.Instance != null && ToolMenu.Instance.toolParameterMenu != null
            ? ToolMenu.Instance.toolParameterMenu.widgetPrefab
            : null;
        if (prefab == null)
        {
            return null;
        }

        var source = Util.KInstantiateUI(prefab, row.gameObject, force_active: true);
        var toggle = source.GetComponentInChildren<MultiToggle>();
        if (toggle == null)
        {
            Destroy(source);
            return null;
        }

        var rect = toggle.GetComponent<RectTransform>();
        var size = rect.rect.size;
        rect.SetParent(row, false);
        Destroy(source);
        var layoutElement = toggle.GetComponent<LayoutElement>();
        if (layoutElement != null)
        {
            Destroy(layoutElement);
        }

        rect.anchorMin = new Vector2(0f, 0.5f);
        rect.anchorMax = new Vector2(0f, 0.5f);
        rect.pivot = new Vector2(0f, 0.5f);
        rect.sizeDelta = size.x > 4f && size.y > 4f ? size : new Vector2(BoxSize, BoxSize);
        rect.anchoredPosition = new Vector2(8f, 0f);

        // The checkbox is display only: let clicks on it fall through to the row button behind it.
        foreach (var graphic in toggle.GetComponentsInChildren<Graphic>(true))
        {
            graphic.raycastTarget = false;
        }

        return toggle;
    }

    // The header is one container so it can follow the list when it is pulled down past its top.
    private void BuildHeader(RectTransform right, float width)
    {
        header = UiFactory.CreateRect("Header", right, Margin, TopReserved, width - Margin * 2f, HeaderHeight);
        UiFactory.CreateSpritePanel("Header background", header, "web_box", UiStyle.SortHeader, 0f, 0f, width - Margin * 2f, HeaderHeight);
        for (var i = 0; i < ColumnNames.Length; i++)
        {
            headerLabels.Add(UiFactory.CreateLabel(header, ColumnNames[i], ColumnX[i], 2f, 240f, 20f));

            // Transparent click area over the header cell: ascending, descending, then unsorted.
            var column = i;
            var areaWidth = column + 1 < ColumnX.Length ? ColumnX[column + 1] - ColumnX[column] - 4f : 200f;
            var area = UiFactory.CreatePanel("Sort " + column, header, new Color(0f, 0f, 0f, 0f), ColumnX[column] - 4f, 0f, areaWidth, HeaderHeight);
            var button = area.gameObject.AddComponent<Button>();
            button.targetGraphic = area;
            var colors = button.colors;
            colors.highlightedColor = new Color(1f, 1f, 1f, 0.15f);
            colors.pressedColor = new Color(1f, 1f, 1f, 0.3f);
            button.colors = colors;
            button.onClick.AddListener(() => CycleSort(column));
        }
    }

    private void CycleSort(int column)
    {
        if (sortColumn != column)
        {
            sortColumn = column;
            sortDescending = false;
        }
        else if (!sortDescending)
        {
            sortDescending = true;
        }
        else
        {
            sortColumn = -1;
            sortDescending = false;
        }

        for (var i = 0; i < headerLabels.Count; i++)
        {
            headerLabels[i].text = ColumnNames[i] + (i == sortColumn ? (sortDescending ? " ▼" : " ▲") : "");
        }

        Refresh();
    }

    // Numeric sort key of a column; null when the value is unknown (those rows always go last).
    private static double? SortKey(CritterRow row, int column)
    {
        switch (column)
        {
            case 0: return (int)row.Stage;
            case 2: return row.IsWild ? 1 : 0;
            case 3: return row.Stage == CritterStage.Egg
                ? (row.Hatch < 0f ? null : row.Hatch)
                : (row.Age < 0f ? null : row.Age);
            case 4: return float.IsNaN(row.Happiness) ? null : row.Happiness;
            case 5: return row.Fertility < 0f ? null : row.Fertility;
            case 6: return row.Fullness < 0f ? null : row.Fullness;
            case 7: return row.FurGrowth < 0f ? null : row.FurGrowth;
            case 8: return row.Temperature < 0f ? null : row.Temperature;
            case 9: return string.IsNullOrEmpty(row.Ailments) ? 0 : row.Ailments.Split(new[] { ", " }, System.StringSplitOptions.None).Length;
            default: return null;
        }
    }

    private List<CritterRow> Sorted(List<CritterRow> list)
    {
        if (sortColumn < 0)
        {
            return list;
        }

        if (sortColumn == 1)
        {
            var byName = list.OrderBy(r => r.GenusName, System.StringComparer.CurrentCulture).ThenBy(r => r.MorphName, System.StringComparer.CurrentCulture);
            return (sortDescending ? byName.Reverse() : byName).ToList();
        }

        var known = list.Where(r => SortKey(r, sortColumn) != null);

        // Age column: eggs (ordered by hatch progress) stay above the creatures when ascending, below when descending.
        var eggsFirst = sortColumn == 3 && !sortDescending;
        var eggsLast = sortColumn == 3 && sortDescending;
        var ordered = (sortDescending
            ? known.OrderByDescending(r => SortKey(r, sortColumn).Value)
            : known.OrderBy(r => SortKey(r, sortColumn).Value)).ToList();
        if (eggsFirst || eggsLast)
        {
            var eggs = ordered.Where(r => r.Stage == CritterStage.Egg);
            var creatures = ordered.Where(r => r.Stage != CritterStage.Egg);
            ordered = (eggsFirst ? eggs.Concat(creatures) : creatures.Concat(eggs)).ToList();
        }

        return ordered.Concat(list.Where(r => SortKey(r, sortColumn) == null)).ToList();
    }

    private void BuildList(RectTransform right)
    {
        var top = TopReserved + HeaderHeight + 2f;
        var viewport = UiFactory.CreateRect("Viewport", right, Margin, top, 0f, 0f);
        viewport.anchorMin = Vector2.zero;
        viewport.anchorMax = Vector2.one;
        viewport.pivot = new Vector2(0.5f, 0.5f);
        viewport.offsetMin = new Vector2(Margin, Margin);
        viewport.offsetMax = new Vector2(-Margin, -top);
        viewport.gameObject.AddComponent<RectMask2D>();
        viewport.gameObject.AddComponent<Image>().color = new Color(0f, 0f, 0f, 0f);

        content = UiFactory.CreateRect("Content", viewport, 0f, 0f, 0f, 0f);
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.offsetMin = Vector2.zero;
        content.offsetMax = Vector2.zero;
        var layout = content.gameObject.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;
        layout.spacing = 2f;
        content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scroll = ConfigureScroll(viewport, content);

        // Rubber band like the skills and research screens: overscroll springs back, flicks coast.
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.elasticity = 0.1f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.135f;

        // Pulling down past the top drags the header along; scrolling up leaves it fixed above the list.
        scroll.onValueChanged.AddListener(_ =>
        {
            var overscroll = Mathf.Min(0f, content.anchoredPosition.y);
            UiFactory.Place(header, Margin, TopReserved - overscroll, header.sizeDelta.x, HeaderHeight);
        });
    }

    // Rebuilds the visible rows from the filters (genus selection is OR within itself, AND with the stage toggles).
    private void Refresh()
    {
        allToggle.Set(genera.Count == 0);
        foreach (var pair in stageToggles)
        {
            pair.Value.Set(stages.Contains(pair.Key));
        }

        foreach (var pair in genusToggles)
        {
            pair.Value.Set(genera.Contains(pair.Key));
        }

        for (var i = content.childCount - 1; i >= 0; i--)
        {
            Destroy(content.GetChild(i).gameObject);
        }

        var shown = Sorted(rows.Where(r => stages.Contains(r.Stage) && (genera.Count == 0 || genera.Contains(r.GenusKey))).ToList());
        countLabel.text = shown.Count + " / " + rows.Count;
        if (rows.Count == 0)
        {
            AddMessageRow(STRINGS.UI.EMPTY);
        }

        foreach (var row in shown)
        {
            AddRow(row);
        }
    }

    private void AddMessageRow(string text)
    {
        var holder = UiFactory.CreateRect("Message", content, 0f, 0f, 0f, RowHeight);
        holder.gameObject.AddComponent<LayoutElement>().minHeight = RowHeight;
        UiFactory.CreateLabel(holder, text, 8f, 2f, 600f, 20f);
    }

    private void AddRow(CritterRow row)
    {
        var button = UiFactory.CreateButton(content, "", 0f, 0f, 0f, RowHeight, () => Pick(row));
        button.gameObject.AddComponent<LayoutElement>().minHeight = RowHeight;
        if (row.IsWild)
        {
            button.GetComponent<Image>().color = WildRowColor;
        }

        var holder = button.GetComponent<RectTransform>();
        if (row.Icon != null)
        {
            var icon = UiFactory.CreatePanel("Icon", holder, row.IconColor, ColumnX[1], (RowHeight - IconSize) / 2f, IconSize, IconSize);
            icon.sprite = row.Icon;
            icon.preserveAspect = true;
            icon.raycastTarget = false;
        }

        Cell(holder, 0, StageName(row.Stage));
        Cell(holder, 1, row.Stage == CritterStage.Egg ? row.MorphName + " -> " + row.HatchMorphName : row.MorphName);
        Cell(holder, 2, row.IsWild ? STRINGS.UI.ORIGINS.WILD : STRINGS.UI.ORIGINS.TAME, CritterTooltips.Wildness(row), color: row.IsWild ? WildTextColor : (Color?)null);
        Cell(holder, 3, AgeText(row), ratio: AgeRatio(row));
        Cell(holder, 4, float.IsNaN(row.Happiness) ? "-" : row.Happiness.ToString("0.0"), CritterTooltips.Happiness(row), color: HappinessColor(row.Happiness));
        Cell(holder, 5, ProgressText(row.Fertility, row.FertilityTotalCycles, row.FertilityRemainingCycles), CritterTooltips.Breeding(row), row.Fertility);
        Cell(holder, 6, Percent(row.Fullness), CritterTooltips.Fullness(row));
        Cell(holder, 7, Percent(row.FurGrowth));
        Cell(holder, 8, row.Temperature < 0f ? "-" : GameUtil.GetFormattedTemperature(row.Temperature), color: TemperatureColor(row.TemperatureBand));
        Cell(holder, 9, row.Ailments, color: AilmentColor);
    }

    private static void Cell(RectTransform holder, int column, string text, string tooltip = null, float ratio = -1f, Color? color = null)
    {
        // Progress bar behind the text (age / lifespan, hatch progress, fertility).
        if (ratio >= 0f && column + 1 < ColumnX.Length)
        {
            var barWidth = ColumnX[column + 1] - ColumnX[column] - 8f;
            var track = UiFactory.CreatePanel("Progress track", holder, ProgressTrackColor, ColumnX[column] - 2f, 4f, barWidth, RowHeight - 8f);
            track.raycastTarget = false;
            var bar = UiFactory.CreatePanel("Progress", holder, ProgressColor, ColumnX[column] - 2f, 4f, barWidth * Mathf.Clamp01(ratio), RowHeight - 8f);
            bar.raycastTarget = false;
        }

        var width = column == 9 ? 700f : column == 1 ? 250f - IconSize - 4f : column == 3 || column == 5 ? 250f : 80f;
        var x = column == 1 ? ColumnX[1] + IconSize + 4f : ColumnX[column];
        // Two-line texts (age, fertility) get a taller label; the label centers its lines vertically.
        var height = text != null && text.Contains('\n') ? 40f : 20f;
        var label = UiFactory.CreateLabel(holder, text, x, (RowHeight - height) / 2f, width, height);
        if (color.HasValue)
        {
            label.color = color.Value;
        }

        if (!string.IsNullOrEmpty(tooltip))
        {
            // Invisible hover area over the cell; clicks fall through to the row button behind it.
            var hoverWidth = column + 1 < ColumnX.Length ? ColumnX[column + 1] - ColumnX[column] - 4f : width;
            var hover = UiFactory.CreatePanel("Hover", holder, new Color(0f, 0f, 0f, 0f), ColumnX[column], 0f, hoverWidth, RowHeight);
            hover.gameObject.AddComponent<ToolTip>().SetSimpleTooltip(tooltip);
        }
    }

    private static string StageName(CritterStage stage)
    {
        switch (stage)
        {
            case CritterStage.Baby:
                return STRINGS.UI.STAGES.BABY;
            case CritterStage.Egg:
                return STRINGS.UI.STAGES.EGG;
            default:
                return STRINGS.UI.STAGES.ADULT;
        }
    }

    private static string Percent(float ratio) => ratio < 0f ? "-" : Mathf.RoundToInt(ratio * 100f) + "%";

    private static Color? HappinessColor(float happiness)
    {
        if (float.IsNaN(happiness) || happiness == 0f)
        {
            return null;
        }

        return happiness > 0f ? GoodColor : WarnColor;
    }

    private static Color? TemperatureColor(TemperatureBand band)
    {
        switch (band)
        {
            case TemperatureBand.Comfortable: return GoodColor;
            case TemperatureBand.Hot: return WarnColor;
            case TemperatureBand.HotDeadly: return new Color(1f, 0.25f, 0.25f, 1f);
            case TemperatureBand.Cold: return new Color(0.55f, 0.85f, 1f, 1f);
            case TemperatureBand.ColdDeadly: return new Color(0.3f, 0.45f, 1f, 1f);
            default: return null;
        }
    }

    // Progress shown behind the age cell: hatch progress for eggs, age / lifespan otherwise.
    private static float AgeRatio(CritterRow row)
    {
        if (row.Stage == CritterStage.Egg)
        {
            return row.Hatch;
        }

        return row.Age < 0f || row.Lifespan <= 0f ? -1f : row.Age / row.Lifespan;
    }

    private static string AgeText(CritterRow row)
    {
        if (row.Stage == CritterStage.Egg)
        {
            return ProgressText(row.Hatch, row.HatchTotalCycles, row.HatchRemainingCycles);
        }

        return row.Age < 0f ? "-" : Cycles(row.Age) + "/" + Cycles(row.Lifespan) + "\n" + Left(row.CyclesUntilDeath);
    }

    private static string Cycles(float value) => string.Format(STRINGS.UI.FORMATS.CYCLES, value.ToString("0.0"));

    private static string Left(float value) => string.Format(STRINGS.UI.FORMATS.LEFT, Cycles(value));

    // "[percent]%/[total]cyc([remaining]cyc left)"; the cycle figures are "-" while the amount isn't progressing.
    private static string ProgressText(float ratio, float total, float remaining)
    {
        if (ratio < 0f)
        {
            return "-";
        }

        var percent = Mathf.RoundToInt(ratio * 100f) + "%";
        return total < 0f ? percent + "/-\n(-)" : percent + "/" + Cycles(total) + "\n" + Left(remaining);
    }

    // Closes the modal and shows the clicked critter (or the incubator holding the clicked egg).
    private void Pick(CritterRow row)
    {
        Close();
        if (row.Target == null || SelectTool.Instance == null)
        {
            return;
        }

        SelectTool.Instance.Activate();
        SelectTool.Instance.SelectAndFocus(row.Target.transform.GetPosition(), row.Target);
    }
}
