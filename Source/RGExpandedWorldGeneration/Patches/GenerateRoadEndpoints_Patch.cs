using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Roads), "GenerateRoadEndpoints")]
public static class GenerateRoadEndpoints_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var methodToHook = AccessTools.Method(typeof(FloatRange), "get_RandomInRange");
        var codes = instructions.ToList();
        var found = false;
        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            yield return code;
            if (found || !code.Calls(methodToHook))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(Page_CreateWorldParams_Patch),
                    nameof(Page_CreateWorldParams_Patch.tmpWorldGenerationPreset)));
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(WorldGenerationPreset), nameof(WorldGenerationPreset.factionRoadDensity)));
            yield return new CodeInstruction(OpCodes.Mul);
            found = true;
        }
    }
}