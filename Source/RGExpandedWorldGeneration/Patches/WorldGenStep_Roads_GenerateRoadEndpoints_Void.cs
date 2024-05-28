using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Roads), nameof(WorldGenStep_Roads.GenerateRoadEndpoints))]
public static class WorldGenStep_Roads_GenerateRoadEndpoints_Void
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var methodToHook = AccessTools.Method(typeof(FloatRange), $"get_{nameof(FloatRange.RandomInRange)}");
        var codes = instructions.ToList();
        var found = false;
        foreach (var code in codes)
        {
            yield return code;
            if (found || !code.Calls(methodToHook))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(Page_CreateWorldParams_DoWindowContents),
                    nameof(Page_CreateWorldParams_DoWindowContents.tmpWorldGenerationPreset)));
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(WorldGenerationPreset), nameof(WorldGenerationPreset.factionRoadDensity)));
            yield return new CodeInstruction(OpCodes.Mul);
            found = true;
        }
    }
}