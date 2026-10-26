using System.Collections.Generic;
using HarmonyLib;

namespace CritterCensus;

// Full-screen windows of other mods are not management menu screens, so the menu cannot close them. They are
// still on the screen manager's stack, which lets a window that opens close them like the menu closes its own.
public static class ModalScreens
{
    // Hides every other mod's modal screen that is currently shown.
    public static void CloseOthers(KScreen self)
    {
        if (KScreenManager.Instance == null)
        {
            return;
        }

        var stack = Traverse.Create(KScreenManager.Instance).Field("screenStack").GetValue<List<KScreen>>();
        if (stack == null)
        {
            return;
        }

        var vanilla = typeof(KModalScreen).Assembly;
        foreach (var screen in stack.ToArray())
        {
            if (screen != null && screen != self && screen is KModalScreen && screen.GetType().Assembly != vanilla
                && screen.gameObject.activeInHierarchy)
            {
                screen.Show(false);
            }
        }
    }
}
