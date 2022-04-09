﻿using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_AncientSites), "GenerateAncientSites")]
public static class GenerateAncientSites_Patch
{
    private static void Prefix(WorldGenStep_AncientSites __instance, out FloatRange __state)
    {
        __state = __instance.ancientSitesPer100kTiles;
        __instance.ancientSitesPer100kTiles *= Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.ancientRoadDensity;
    }

    private static void Postfix(WorldGenStep_AncientSites __instance, FloatRange __state)
    {
        __instance.ancientSitesPer100kTiles = __state;
    }
}