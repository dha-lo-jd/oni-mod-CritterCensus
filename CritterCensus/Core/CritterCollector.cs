using System.Collections.Generic;
using System.Text;
using Klei.AI;
using UnityEngine;

namespace CritterCensus;

// Finds every creature and egg standing in the selected cells and snapshots their stats.
public static class CritterCollector
{
    private const float SecondsPerCycle = 600f;

    public static List<CritterRow> Collect(CensusSelection selection)
    {
        var rows = new List<CritterRow>();

        foreach (var brain in Components.Brains.Items)
        {
            if (brain == null || !(brain is CreatureBrain creatureBrain))
            {
                continue;
            }

            var go = brain.gameObject;
            var prefabId = go.GetComponent<KPrefabID>();
            if (prefabId == null || prefabId.HasTag(GameTags.Dead) || prefabId.HasTag(GameTags.Stored))
            {
                continue;
            }

            if (!selection.Cells.Contains(Grid.PosToCell(go.transform.GetPosition())))
            {
                continue;
            }

            rows.Add(BuildCreatureRow(creatureBrain, prefabId));
        }

        foreach (var egg in Components.IncubationMonitors.Items)
        {
            if (egg == null || egg.gameObject == null)
            {
                continue;
            }

            var holder = egg.incubator != null ? egg.incubator.gameObject : egg.gameObject;
            if (!selection.Cells.Contains(Grid.PosToCell(holder.transform.GetPosition())))
            {
                continue;
            }

            rows.Add(BuildEggRow(egg, holder));
        }

        return rows;
    }

    private static CritterRow BuildCreatureRow(CreatureBrain brain, KPrefabID prefabId)
    {
        var go = brain.gameObject;
        var baby = go.GetDef<BabyMonitor.Def>();
        var row = new CritterRow
        {
            Stage = baby != null ? CritterStage.Baby : CritterStage.Adult,
            GenusKey = brain.species.ToString(),
            GenusName = brain.species.ProperName(),
            MorphName = MorphName(baby != null ? baby.adultPrefab : prefabId.PrefabTag),
            IsWild = prefabId.HasTag(GameTags.Creatures.Wild),
            Source = go,
            Target = go.GetComponent<KSelectable>(),
            Position = go.transform.GetPosition(),
        };

        var age = go.GetSMI<AgeMonitor.Instance>();
        if (age != null)
        {
            row.Age = age.age.value;
            row.Lifespan = age.age.GetMax();
            row.CyclesUntilDeath = age.CyclesUntilDeath;
        }

        var happiness = go.GetSMI<HappinessMonitor.Instance>();
        if (happiness != null)
        {
            row.Happiness = happiness.happiness.GetTotalValue();
        }

        SetIcon(row, go);
        var fertility = Db.Get().Amounts.Fertility.Lookup(go);
        row.Fertility = Ratio(fertility);
        Progress(fertility, out row.FertilityTotalCycles, out row.FertilityRemainingCycles);
        row.Fullness = Ratio(Db.Get().Amounts.Calories.Lookup(go));
        row.Ailments = Ailments(row.Target);
        return row;
    }

    private static CritterRow BuildEggRow(IncubationMonitor.Instance egg, GameObject holder)
    {
        var go = egg.gameObject;
        var babyTag = egg.def.spawnedCreature;
        var babyPrefab = Assets.GetPrefab(babyTag);
        var adultTag = babyTag;
        var genusKey = babyTag.ToString();
        var genusName = babyTag.ProperName();
        if (babyPrefab != null)
        {
            var baby = babyPrefab.GetDef<BabyMonitor.Def>();
            if (baby != null)
            {
                adultTag = baby.adultPrefab;
            }

            var brain = babyPrefab.GetComponent<CreatureBrain>();
            if (brain != null)
            {
                genusKey = brain.species.ToString();
                genusName = brain.species.ProperName();
            }
        }

        var row = new CritterRow
        {
            Stage = CritterStage.Egg,
            GenusKey = genusKey,
            GenusName = genusName,
            MorphName = go.GetProperName(),
            HatchMorphName = MorphName(adultTag),
            IsWild = go.HasTag(GameTags.Creatures.Wild),
            // An egg inside an incubator is picked through the incubator.
            Source = go,
            Target = holder.GetComponent<KSelectable>(),
            Position = holder.transform.GetPosition(),
        };

        var incubation = egg.incubation;
        if (incubation != null)
        {
            row.Hatch = Ratio(incubation);
            Progress(incubation, out row.HatchTotalCycles, out row.HatchRemainingCycles);
        }

        SetIcon(row, go);
        row.Ailments = Ailments(go.GetComponent<KSelectable>());
        return row;
    }

    private static void SetIcon(CritterRow row, GameObject go)
    {
        var icon = Def.GetUISprite(go);
        if (icon != null)
        {
            row.Icon = icon.first;
            row.IconColor = icon.second;
        }
    }

    private static string MorphName(Tag prefab)
    {
        var go = Assets.GetPrefab(prefab);
        return go != null ? go.GetProperName() : prefab.ProperName();
    }

    // Cycles for the amount to fill from empty / from now, at its current speed; -1 when it isn't filling.
    private static void Progress(AmountInstance amount, out float total, out float remaining)
    {
        total = -1f;
        remaining = -1f;
        var rate = amount != null ? amount.GetDelta() : 0f;
        if (rate > 0f)
        {
            total = amount.GetMax() / rate / SecondsPerCycle;
            remaining = Mathf.Max(0f, amount.GetMax() - amount.value) / rate / SecondsPerCycle;
        }
    }

    private static float Ratio(AmountInstance amount)
    {
        if (amount == null)
        {
            return -1f;
        }

        var max = amount.GetMax();
        return max > 0f ? Mathf.Clamp01(amount.value / max) : -1f;
    }

    // Names of the status items the game flags as bad (starving, stuck, overcrowded, ...).
    private static string Ailments(KSelectable selectable)
    {
        var group = selectable != null ? selectable.GetStatusItemGroup() : null;
        if (group == null)
        {
            return "";
        }

        var text = new StringBuilder();
        foreach (var entry in group)
        {
            if (entry.item == null || entry.item.notificationType != NotificationType.Bad)
            {
                continue;
            }

            if (text.Length > 0)
            {
                text.Append(", ");
            }

            text.Append(entry.GetName());
        }

        return text.ToString();
    }
}
