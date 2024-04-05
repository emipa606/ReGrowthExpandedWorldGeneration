using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.SetupElevationNoise))]
public static class SetupElevationNoise_Patch
{
    public static void Prefix(ref FloatRange ___ElevationRange)
    {
        if (Page_CreateWorldParams_Patch.tmpWorldGenerationPreset is null)
        {
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset = new WorldGenerationPreset();
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.Init();
        }

        ___ElevationRange =
            new FloatRange(-500f * Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.seaLevel, 5000f);
    }
}