using HarmonyLib;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(GenTemperature), nameof(GenTemperature.SeasonalShiftAmplitudeAt), null)]
public static class GenTemperature_SeasonalShiftAmplitudeAt
{
    public static bool Prefix()
    {
        return false;
    }

    [HarmonyPriority(int.MaxValue)]
    public static void Postfix(int tile, ref float __result)
    {
        if (Find.WorldGrid.LongLatOf(tile).y >= 0f)
        {
            __result = WorldComponent_WorldGenerator.mappedValues[WorldComponent_WorldGenerator.Instance.axialTilt]
                .Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
            return;
        }

        __result = -WorldComponent_WorldGenerator.mappedValues[WorldComponent_WorldGenerator.Instance.axialTilt]
            .Evaluate(Find.WorldGrid.DistanceFromEquatorNormalized(tile));
    }
}