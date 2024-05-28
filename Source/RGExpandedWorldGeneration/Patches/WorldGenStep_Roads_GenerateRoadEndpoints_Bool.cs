﻿using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using HarmonyLib;
using RimWorld.Planet;

namespace RGExpandedWorldGeneration;

[HarmonyPatch]
internal static class WorldGenStep_Roads_GenerateRoadEndpoints_Bool
{
    private static MethodBase TargetMethod()
    {
        foreach (var nestType in typeof(WorldGenStep_Roads).GetNestedTypes(AccessTools.all))
        {
            foreach (var meth in AccessTools.GetDeclaredMethods(nestType))
            {
                if (meth.Name.Contains("GenerateRoadEndpoints") && meth.ReturnType == typeof(bool))
                {
                    return meth;
                }
            }
        }

        return null;
    }

    private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
    {
        var codes = instructions.ToList();
        var found = false;
        foreach (var code in codes)
        {
            yield return code;
            if (found || !code.OperandIs(0.05f))
            {
                continue;
            }

            yield return new CodeInstruction(OpCodes.Ldsfld,
                AccessTools.Field(typeof(Page_CreateWorldParams_DoWindowContents),
                    nameof(Page_CreateWorldParams_DoWindowContents.tmpWorldGenerationPreset)));
            yield return new CodeInstruction(OpCodes.Ldfld,
                AccessTools.Field(typeof(WorldGenerationPreset), nameof(WorldGenerationPreset.factionRoadDensity)));
            yield return new CodeInstruction(OpCodes.Div);
            found = true;
        }
    }
}