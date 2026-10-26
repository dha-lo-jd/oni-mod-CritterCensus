using HarmonyLib;
using KMod;

namespace CritterCensus;

public sealed class CritterCensusMod : UserMod2
{
    public override void OnLoad(Harmony harmony)
    {
        base.OnLoad(harmony);
        harmony.PatchAll(typeof(CritterCensusMod).Assembly);
    }
}
