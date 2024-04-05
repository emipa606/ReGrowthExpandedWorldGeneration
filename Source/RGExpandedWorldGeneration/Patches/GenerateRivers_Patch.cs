using System.Collections.Generic;
using HarmonyLib;
using RimWorld;
using RimWorld.Planet;
using Verse;

namespace RGExpandedWorldGeneration;

[HarmonyPatch(typeof(WorldGenStep_Rivers), nameof(WorldGenStep_Rivers.GenerateRivers))]
public static class GenerateRivers_Patch
{
    [HarmonyPriority(Priority.First)]
    private static void Prefix(out Dictionary<RiverDef, RiverData> __state)
    {
        __state = new Dictionary<RiverDef, RiverData>();
        foreach (var def in DefDatabase<RiverDef>.AllDefs)
        {
            var riverData = new RiverData();
            __state[def] = riverData;
            riverData.spawnChance = def.spawnChance;
            def.spawnChance *= Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.riverDensity;
            if (def.branches == null)
            {
                continue;
            }

            riverData.branchChance = new float[def.branches.Count];
            for (var i = 0; i < def.branches.Count; i++)
            {
                riverData.branchChance[i] = def.branches[i].chance;
                def.branches[i].chance *= Page_CreateWorldParams_Patch.tmpWorldGenerationPreset.riverDensity;
            }
        }
    }

    //private static void Postfix(Dictionary<RiverDef, RiverData> __state)
    //{
    //    __state = new Dictionary<RiverDef, RiverData>();

    //    foreach (var data in __state)
    //    {
    //        data.Key.spawnChance = data.Value.spawnChance;
    //        if (data.Key.branches == null)
    //        {
    //            continue;
    //        }

    //        for (var i = 0; i < data.Key.branches.Count; i++)
    //        {
    //            data.Key.branches[i].chance = data.Value.branchChance[i];
    //        }
    //    }
    //}

    public struct RiverData
    {
        public float spawnChance;
        public float[] branchChance;
    }
}