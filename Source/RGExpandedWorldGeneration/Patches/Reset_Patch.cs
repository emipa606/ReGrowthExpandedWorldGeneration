using HarmonyLib;
using RimWorld;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(Page_CreateWorldParams), "Reset")]
public static class Reset_Patch
{
    public static void Postfix()
    {
        if (Page_CreateWorldParams_Patch.tmpWorldGenerationPreset != null)
        {
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.Reset();
        }
    }
}