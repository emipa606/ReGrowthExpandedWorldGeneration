using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldLayer), "RegenerateNow")]
public static class RegenerateNow_Patch
{
    public static bool Prefix()
    {
        if (Page_CreateWorldParams_Patch.dirty && Find.WindowStack.WindowOfType<Page_CreateWorldParams>() != null)
        {
            return false;
        }

        return true;
    }
}