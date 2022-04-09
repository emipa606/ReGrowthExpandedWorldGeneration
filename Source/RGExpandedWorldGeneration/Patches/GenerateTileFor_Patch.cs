using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using UnityEngine;
using Verse.Noise;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Terrain), "GenerateTileFor")]
public static class GenerateTileFor_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var methodToHook = AccessTools.Method(typeof(ModuleBase), "GetValue", new[] { typeof(Vector3) });
        var noiseMountainLinesField = AccessTools.Field(typeof(WorldGenStep_Terrain), "noiseMountainLines");
        var codes = instructions.ToList();
        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            yield return code;
            if (i <= 2 || !code.Calls(methodToHook) || !codes[i - 2].LoadsField(noiseMountainLinesField))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(Page_CreateWorldParams_Patch),
                    nameof(Page_CreateWorldParams_Patch.tmpWorldGenerationPreset)));
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(WorldGenerationPreset), nameof(WorldGenerationPreset.mountainDensity)));
            yield return new CodeInstruction(OpCodes.Div);
        }
    }
}