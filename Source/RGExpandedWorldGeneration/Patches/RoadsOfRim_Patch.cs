using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;

namespace RGExpandedWorldGeneration;

[HarmonyPatch]
public static class RoadsOfRim_Patch
{
    private static MethodBase patch_WorldTargeter_StopTargeting;

    public static bool Prepare()
    {
        patch_WorldTargeter_StopTargeting =
            AccessTools.Method("RoadsOfTheRim.Patch_WorldTargeter_StopTargeting:Prefix");
        return patch_WorldTargeter_StopTargeting != null;
    }

    public static IEnumerable<MethodBase> TargetMethods()
    {
        yield return patch_WorldTargeter_StopTargeting;
    }

    [HarmonyPriority(int.MaxValue)]
    public static bool Prefix()
    {
        if (Page_CreateWorldParams_Patch.thread != null)
        {
            return false;
        }

        return true;
    }
}