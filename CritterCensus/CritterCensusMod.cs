using System.IO;
using HarmonyLib;
using KMod;
using UnityEngine;

namespace CritterCensus;

public sealed class CritterCensusMod : UserMod2
{
    private static string modPath;

    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        modPath = path;
        harmony.PatchAll(typeof(CritterCensusMod).Assembly);
    }

    // Registers the strings and overloads them with translations/<language code>.po of the selected language.
    [HarmonyPatch(typeof(Localization), nameof(Localization.Initialize))]
    public static class LocalizationInitializePatch
    {
        public static void Postfix()
        {
            Localization.RegisterForTranslation(typeof(STRINGS));

            var code = Localization.GetLocale()?.Code;
            if (!string.IsNullOrEmpty(code) && modPath != null)
            {
                var file = Path.Combine(modPath, "translations", code + ".po");
                if (File.Exists(file))
                {
                    Localization.OverloadStrings(Localization.LoadStringsFile(file, false));
                    Debug.Log("[CritterCensus] Loaded translation " + code);
                }
            }

            LocString.CreateLocStringKeys(typeof(STRINGS), null);
        }
    }
}
