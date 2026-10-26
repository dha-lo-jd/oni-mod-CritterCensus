using System.Linq;
using System.Text;
using Klei.AI;
using UnityEngine;

namespace CritterCensus;

// Tooltip texts for the list cells. Each returns null when there is nothing to show.
public static class CritterTooltips
{
    private const float SecondsPerCycle = 600f;

    // Chance of each egg when the critter lays one.
    public static string Breeding(CritterRow row)
    {
        var fertility = row.Source != null ? row.Source.GetSMI<FertilityMonitor.Instance>() : null;
        if (fertility == null || fertility.breedingChances == null)
        {
            return null;
        }

        var total = fertility.breedingChances.Sum(c => c.weight);
        if (total <= 0f)
        {
            return null;
        }

        var text = new StringBuilder(STRINGS.UI.TOOLTIPS.EGG_CHANCES);
        foreach (var chance in fertility.breedingChances.OrderByDescending(c => c.weight))
        {
            var prefab = Assets.GetPrefab(chance.egg);
            var name = prefab != null ? prefab.GetProperName() : chance.egg.ProperName();
            text.Append('\n').Append(name).Append(": ").Append((chance.weight / total * 100f).ToString("0.#")).Append('%');
        }

        return text.ToString();
    }

    // Total happiness and each modifier contributing to it.
    public static string Happiness(CritterRow row)
    {
        var monitor = row.Source != null ? row.Source.GetSMI<HappinessMonitor.Instance>() : null;
        if (monitor == null)
        {
            return null;
        }

        var attribute = monitor.happiness;
        var text = new StringBuilder(string.Format(STRINGS.UI.TOOLTIPS.HAPPINESS, attribute.GetTotalValue().ToString("0.0")));
        AppendModifiers(text, attribute, 1f);
        return text.ToString();
    }

    public static string Fullness(CritterRow row)
    {
        var calories = row.Source != null ? Db.Get().Amounts.Calories.Lookup(row.Source) : null;
        if (calories == null)
        {
            return null;
        }

        return string.Format(STRINGS.UI.TOOLTIPS.CALORIES, GameUtil.GetFormattedCalories(calories.value), GameUtil.GetFormattedCalories(calories.GetMax()));
    }

    // Wildness level, its change per cycle and the modifiers behind that change. Only for wild critters.
    public static string Wildness(CritterRow row)
    {
        var wildness = row.IsWild && row.Source != null ? Db.Get().Amounts.Wildness.Lookup(row.Source) : null;
        if (wildness == null)
        {
            return null;
        }

        var max = wildness.GetMax();
        var percent = Mathf.RoundToInt(max > 0f ? wildness.value / max * 100f : 0f);
        var text = new StringBuilder(string.Format(STRINGS.UI.TOOLTIPS.WILDNESS, wildness.value.ToString("0.0"), max.ToString("0.#"), percent));
        text.Append('\n').Append(string.Format(STRINGS.UI.TOOLTIPS.WILDNESS_CHANGE, (wildness.GetDelta() * SecondsPerCycle).ToString("+0.##;-0.##;0")));
        AppendModifiers(text, wildness.deltaAttribute, SecondsPerCycle);
        return text.ToString();
    }

    private static void AppendModifiers(StringBuilder text, AttributeInstance attribute, float scale)
    {
        for (var i = 0; i < attribute.Modifiers.Count; i++)
        {
            var modifier = attribute.Modifiers[i];
            var value = modifier.IsMultiplier
                ? (modifier.Value * 100f).ToString("+0.##;-0.##;0") + "%"
                : (modifier.Value * scale).ToString("+0.##;-0.##;0");
            text.Append("\n- ").Append(modifier.GetDescription()).Append(": ").Append(value);
        }
    }
}
