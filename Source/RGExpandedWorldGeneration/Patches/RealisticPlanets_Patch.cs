using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace RGExpandedWorldGeneration;

[HarmonyPatch]
public static class RealisticPlanets_Patch
{
    private static MethodBase pageUtility_StitchedPages;
    private static MethodBase genTemperature_SeasonalShiftAmplitudeAt;

    public static bool Prepare()
    {
        pageUtility_StitchedPages = AccessTools.Method("Planets_Code.PageUtility_StitchedPages:Postfix");
        genTemperature_SeasonalShiftAmplitudeAt =
            AccessTools.Method("Planets_Code.GenTemperature_SeasonalShiftAmplitudeAt:Postfix");
        return pageUtility_StitchedPages != null && genTemperature_SeasonalShiftAmplitudeAt != null;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return pageUtility_StitchedPages;
        yield return genTemperature_SeasonalShiftAmplitudeAt;
    }

    [HarmonyPriority(Priority.First)]
    public static bool Prefix()
    {
        return false;
    }
}