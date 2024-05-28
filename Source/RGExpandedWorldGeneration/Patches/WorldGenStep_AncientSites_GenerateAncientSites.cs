using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_AncientSites), nameof(WorldGenStep_AncientSites.GenerateAncientSites))]
public static class WorldGenStep_AncientSites_GenerateAncientSites
{
    private static void Prefix(WorldGenStep_AncientSites __instance, out FloatRange __state)
    {
        __state = __instance.ancientSitesPer100kTiles;
        __instance.ancientSitesPer100kTiles *=
            Page_CreateWorldParams_DoWindowContents.tmpWorldGenerationPreset.ancientRoadDensity;
    }

    private static void Postfix(WorldGenStep_AncientSites __instance, FloatRange __state)
    {
        __instance.ancientSitesPer100kTiles = __state;
    }
}