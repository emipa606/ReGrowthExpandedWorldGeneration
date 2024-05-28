using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldLayer), nameof(WorldLayer.RegenerateNow))]
public static class RegenerateNow_Patch
{
    public static bool Prefix()
    {
        return !Page_CreateWorldParams_DoWindowContents.dirty ||
               Find.WindowStack.WindowOfType<Page_CreateWorldParams>() == null;
    }
}