#if DEBUG && HARNESS
// In-game tests run by Gravitas.TestHarness:
//   python Gravitas.TestProbe/probe.py run --project <this repo> --tests '{"tags":["crittercensus"]}'
// Compiled only in Debug builds that set HarnessDllPath in Local.Build.props (Check / Arrange come from the harness assembly).
using System.Collections;
using System.Linq;
using Gravitas.TestHarness;
using HarmonyLib;
using UnityEngine;

namespace CritterCensus;

internal static class HarnessTests
{
    [HarnessTest(Name = "crittercensus.mod_loaded", Tags = "crittercensus", RequiresGame = false)]
    public static void ModLoaded()
    {
        Check.True(CritterCensusMod.modPath != null, "CritterCensusMod.OnLoad has not run");
    }

    // UserMod2.OnLoad applies the patches; a second PatchAll would run every patch twice.
    [HarnessTest(Name = "crittercensus.patches_applied_once", Tags = "crittercensus", RequiresGame = false)]
    public static void PatchesAppliedOnce()
    {
        CheckPatchedOnce(AccessTools.Method(typeof(PlayerController), "OnPrefabInit"), typeof(PlayerControllerOnPrefabInitPatch), prefix: false);
        CheckPatchedOnce(AccessTools.Method(typeof(ToolMenu), "CreateBasicTools"), typeof(ToolMenuCreateBasicToolsPatch), prefix: false);
        CheckPatchedOnce(AccessTools.Method(typeof(GameInputMapping), "CompareActionKeyCodes"), typeof(CompareActionKeyCodesPatch), prefix: true);
        CheckPatchedOnce(AccessTools.Method(typeof(GameUtil), nameof(GameUtil.GetHotkeyString)), typeof(GetHotkeyStringPatch), prefix: true);
        CheckPatchedOnce(AccessTools.Method(typeof(SelectTool), nameof(SelectTool.OnLeftClickDown)), typeof(SelectToolRoomClickPatch), prefix: true);
    }

    private static void CheckPatchedOnce(System.Reflection.MethodBase target, System.Type patch, bool prefix)
    {
        Check.NotNull(target, "patch target for " + patch.Name + " not found");
        var info = Harmony.GetPatchInfo(target);
        Check.NotNull(info, target.Name + " is not patched");
        var patches = prefix ? info.Prefixes : info.Postfixes;
        Check.Equal(1, patches.Count(p => p.PatchMethod.DeclaringType == patch), patch.Name + " on " + target.Name);
    }

    // The tool exists on the player controller and its button sits right after the capture tool, once.
    [HarnessTest(Name = "crittercensus.tool_registered", Tags = "crittercensus")]
    public static void ToolRegistered()
    {
        Check.Equal(1, PlayerController.Instance.tools.OfType<CensusTool>().Count(), "census tools on the player controller");

        var names = ToolMenu.Instance.basicTools.Where(c => c.tools.Count > 0).Select(c => c.tools[0].toolName).ToList();
        Check.Equal(1, names.Count(n => n == CensusTool.ToolName), "census buttons in the tool menu");
        Check.Equal(names.IndexOf("CaptureTool") + 1, names.IndexOf(CensusTool.ToolName), "position of the census button");
    }

    // The button shows the mod's own icon when ModAssets/tool_icon.png is there, else the borrowed vanilla one.
    [HarnessTest(Name = "crittercensus.tool_icon", Tags = "crittercensus")]
    public static void ToolIconUsed()
    {
        var file = System.IO.Path.Combine(CritterCensusMod.modPath, "ModAssets", ToolIcon.FileName);
        var expected = System.IO.File.Exists(file) ? ToolIcon.SpriteName : ToolIcon.Fallback;
        Check.Equal(expected, ToolIcon.Resolve(), "icon name for the tool button");
        Check.NotNull(Assets.GetSprite(expected), "the game has no sprite named " + expected);

        var collection = ToolMenu.Instance.basicTools.First(c => c.tools.Count > 0 && c.tools[0].toolName == CensusTool.ToolName);
        Check.Equal(expected, collection.icon, "icon of the census button");
        var image = collection.tools[0].toggle != null ? collection.tools[0].toggle.transform.Find("FG") : null;
        Check.NotNull(image, "the census button has no icon image");
        Check.True(image.GetComponent<UnityEngine.UI.Image>().sprite == Assets.GetSprite(expected), "the census button shows another sprite");
        Check.NoLogErrors();
    }

    // A critter standing in the area is listed with its species and stats; one outside is not.
    [HarnessTest(Name = "crittercensus.critter_listed", Tags = "crittercensus")]
    public static System.Collections.IEnumerator CritterListed()
    {
        var cell = Arrange.HomeCell();
        var hatch = Arrange.Spawn("Hatch", cell);
        try
        {
            yield return null;
            yield return null;
            var here = Grid.PosToCell(hatch);

            var rows = CritterCollector.Collect(CensusSelection.FromRect(here, here));
            Check.Equal(1, rows.Count, "rows for the Hatch's cell");
            var row = rows[0];
            Check.True(!string.IsNullOrEmpty(row.GenusName) && !string.IsNullOrEmpty(row.MorphName), "names are '" + row.GenusName + "' / '" + row.MorphName + "'");
            Check.True(row.Target == hatch.GetComponent<KSelectable>(), "the row selects the Hatch");
            Check.NotNull(row.Icon, "icon");

            var far = Grid.OffsetCell(here, 20, 0);
            Check.Equal(0, CritterCollector.Collect(CensusSelection.FromRect(far, far)).Count, "rows for a cell away from the Hatch");
            Check.NoLogErrors();
        }
        finally
        {
            Arrange.Destroy(hatch);
        }
    }

    // The census button has no hotkey (Action.NumActions). Vanilla's ToolMenu.OnKeyDown used to throw an IndexOutOfRangeException
    // for it on every key press (fixed by KButtonEventIsActionPatch, 2026-11-13). The press goes through the game's own input path.
    [HarnessTest(Name = "crittercensus.key_press_does_not_throw", Tags = "crittercensus")]
    public static IEnumerator KeyPressDoesNotThrow()
    {
        yield return Wait.Ticks(2);
        Play.ClearTool();
        Play.Press(KKeyCode.Escape);
        Play.Press(Action.Dig); // a tool hotkey: ToolMenu compares every tool's hotkey, ours included
        Play.ClearTool();
        Play.Press(KKeyCode.Escape);
    }
}
#endif
