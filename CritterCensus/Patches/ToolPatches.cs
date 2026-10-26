using System;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace CritterCensus;

// Registers the census tool like a vanilla one: a tool object on the PlayerController (look and cursor copied from the
// wrangle tool) and a button in the basic tool menu.
[HarmonyPatch(typeof(PlayerController), "OnPrefabInit")]
public static class PlayerControllerOnPrefabInitPatch
{
    public static void Postfix(PlayerController __instance)
    {
        var template = __instance.tools.OfType<CaptureTool>().FirstOrDefault();
        if (template == null || __instance.tools.OfType<CensusTool>().Any())
        {
            return;
        }

        var go = new GameObject(CensusTool.ToolName);
        go.SetActive(false);
        go.transform.SetParent(__instance.gameObject.transform, false);
        var tool = go.AddComponent<CensusTool>();
        CopyFields(typeof(InterfaceTool), template, tool);
        CopyFields(typeof(DragTool), template, tool);
        go.SetActive(true);
        go.SetActive(false);
        __instance.tools = __instance.tools.Concat(new InterfaceTool[] { tool }).ToArray();
    }

    // Copies the prefab-configured (public / serialized) fields so the new tool looks and behaves like the template.
    private static void CopyFields(Type type, object from, object to)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly;
        foreach (var field in type.GetFields(flags))
        {
            if (field.IsPublic || field.GetCustomAttribute<SerializeField>() != null)
            {
                field.SetValue(to, field.GetValue(from));
            }
        }
    }
}

[HarmonyPatch(typeof(ToolMenu), "CreateBasicTools")]
public static class ToolMenuCreateBasicToolsPatch
{
    public static void Postfix(ToolMenu __instance)
    {
        // CreateBasicTools can run more than once on the same menu; add the button only once.
        if (__instance.basicTools.Any(c => c.tools.Count > 0 && c.tools[0].toolName == CensusTool.ToolName))
        {
            return;
        }

        var collection = ToolMenu.CreateToolCollection(STRINGS.UI.TOOL_NAME, "icon_action_capture", Action.NumActions,
            CensusTool.ToolName, STRINGS.UI.TOOL_TOOLTIP, largeIcon: false);
        var index = __instance.basicTools.FindIndex(c => c.tools.Count > 0 && c.tools[0].toolName == "CaptureTool");
        __instance.basicTools.Insert(index >= 0 ? index + 1 : __instance.basicTools.Count, collection);
    }
}

// The census button has no hotkey; vanilla asserts when comparing an unbound action.
[HarmonyPatch(typeof(GameInputMapping), "CompareActionKeyCodes")]
public static class CompareActionKeyCodesPatch
{
    public static bool Prefix(Action a, Action b, ref bool __result)
    {
        if (a == Action.NumActions || b == Action.NumActions)
        {
            __result = false;
            return false;
        }

        return true;
    }
}

// Rooms overlay: clicking a stable (creature pen) takes the census of that room instead of selecting.
[HarmonyPatch(typeof(SelectTool), nameof(SelectTool.OnLeftClickDown))]
public static class SelectToolRoomClickPatch
{
    public static bool Prefix(Vector3 cursor_pos)
    {
        if (OverlayScreen.Instance == null || OverlayScreen.Instance.GetMode() != OverlayModes.Rooms.ID)
        {
            return true;
        }

        var cavity = Game.Instance.roomProber.GetCavityForCell(Grid.PosToCell(cursor_pos));
        if (cavity == null || cavity.room == null || cavity.room.roomType != Db.Get().RoomTypes.CreaturePen)
        {
            return true;
        }

        CensusScreen.Open(CensusSelection.FromCells(cavity.cells));
        return false;
    }
}
