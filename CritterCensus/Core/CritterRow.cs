using UnityEngine;

namespace CritterCensus;

public enum CritterStage
{
    Adult,
    Baby,
    Egg
}

// Snapshot of one critter or egg taken when the census opens.
public enum TemperatureBand
{
    Unknown,
    Comfortable,
    Hot,
    HotDeadly,
    Cold,
    ColdDeadly
}

public sealed class CritterRow
{
    public CritterStage Stage;
    public string GenusKey;
    public string GenusName;
    public string MorphName;

    // Egg only: morph that will hatch.
    public string HatchMorphName;

    public bool IsWild;

    // Cycles; negative when unknown.
    public float Age = -1f;
    public float Lifespan = -1f;
    public float CyclesUntilDeath = -1f;

    // Egg: incubation progress 0..1 and the cycles it takes / has left at the current incubation speed.
    public float Hatch = -1f;
    public float HatchTotalCycles = -1f;
    public float HatchRemainingCycles = -1f;

    // Same for fertility (breeding) at the current fertility speed.
    public float Fertility = -1f;
    public float FertilityTotalCycles = -1f;
    public float FertilityRemainingCycles = -1f;

    public float Happiness = float.NaN;

    // 0..1; negative when the critter has no such amount.
    public float Fullness = -1f;

    // Fur / scale growth 0..1; negative when the critter has none.
    public float FurGrowth = -1f;

    // Body temperature in kelvin (negative when unknown) and where it sits against the critter's comfort limits.
    public float Temperature = -1f;
    public TemperatureBand TemperatureBand;

    public string Ailments;

    // Default "ui" pose of this critter's prefab, as the game shows it in its own lists.
    public Sprite Icon;
    public Color IconColor = Color.white;

    // The creature or egg itself, for live tooltip data.
    public GameObject Source;

    // What a click selects, and where the camera goes.
    public KSelectable Target;
    public Vector3 Position;
}
