using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Terrain), nameof(WorldGenStep_Terrain.BiomeFrom))]
public static class BiomeFrom_Patch
{
    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var methodToHook = AccessTools.Method(typeof(BiomeWorker), "GetScore");
        var getScoreAdjustedMethod = AccessTools.Method(typeof(BiomeFrom_Patch), "GetScoreAdjusted");
        var codes = instructions.ToList();
        var found = false;
        for (var i = 0; i < codes.Count; i++)
        {
            var code = codes[i];
            yield return code;
            if (found || codes[i].opcode != OpCodes.Stloc_S || codes[i].operand is not LocalBuilder { LocalIndex: 5 } ||
                !codes[i - 1].Calls(methodToHook))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldloc_S, 4);
            yield return new CodeInstruction(OpCodes.Ldloc_S, 5);
            yield return new CodeInstruction(OpCodes.Call, getScoreAdjustedMethod);
            yield return new CodeInstruction(OpCodes.Stloc_S, 5);
            found = true;
        }
    }

    private static float GetScoreAdjusted(BiomeDef biomeDef, float score)
    {
        if (!Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeScoreOffsets?.ContainsKey(biomeDef.defName) ==
            true)
        {
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeScoreOffsets[biomeDef.defName] = 0;
        }

        if (!Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeCommonalities?.ContainsKey(biomeDef.defName) ==
            true)
        {
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeCommonalities[biomeDef.defName] = 10;
        }

        if (Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeScoreOffsets == null)
        {
            return score;
        }

        var scoreOffset = Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeScoreOffsets[biomeDef.defName];
        score += scoreOffset;
        if (Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeCommonalities == null)
        {
            return score;
        }

        var biomeCommonalityOverride =
            Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.biomeCommonalities[biomeDef.defName] / 10f;
        if (biomeCommonalityOverride == 0)
        {
            if (scoreOffset != 0)
            {
                return scoreOffset;
            }

            return -999;
        }

        var adjustedScore = score < 0 ? score / biomeCommonalityOverride : score * biomeCommonalityOverride;
        return adjustedScore;
    }
}