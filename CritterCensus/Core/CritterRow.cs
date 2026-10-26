using UnityEngine;

namespace CritterCensus;

public enum CritterStage
{
    Adult,
    Baby,
    Egg
}

// Snapshot of one critter or egg taken when the census opens.
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
    public float CyclesUntilDeath = -1f;
    public float HatchCycles = -1f;

    // 0..1; negative when the critter has no such amount.
    public float Fertility = -1f;
    public float Fullness = -1f;

    public string Ailments;

    // Default "ui" pose of this critter's prefab, as the game shows it in its own lists.
    public Sprite Icon;
    public Color IconColor = Color.white;

    // What a click selects, and where the camera goes.
    public KSelectable Target;
    public Vector3 Position;
}
